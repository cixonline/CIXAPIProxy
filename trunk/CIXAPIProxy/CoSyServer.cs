using CIXAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Telnet;

namespace CIXAPIProxy
{
    internal sealed class CoSyServer
    {
        private TcpListener _tcpListener;
        private Thread _listenThread;
        private readonly int _portNumber;
        private volatile bool _endThread;

        private Forums _forums;
        private Topic _currentTopic;

        private readonly StringBuilder _scratch = new StringBuilder();
        private string _script;

        // Command state variables
        private enum CoSyState
        {
            MAIN, READ, OPT
        }
        private readonly Stack<CoSyState> _stateStack = new Stack<CoSyState>();

        // Option settings
        private bool _isTerse;

        /// <summary>
        /// Create a CoSyServer running on the specified port.
        /// </summary>
        /// <param name="port"></param>
        public CoSyServer(int port)
        {
            _portNumber = port;
            _isTerse = false;
        }

        /// <summary>
        /// Start the CoSy server running on the previously specified port.
        /// </summary>
        public void Start()
        {
            _endThread = false;

            _tcpListener = new TcpListener(IPAddress.Any, _portNumber);
            _listenThread = new Thread(ListenForClients);
            _listenThread.Start();
        }

        /// <summary>
        /// Stop the CoSy server.
        /// </summary>
        public void Stop()
        {
            _endThread = true;
        }

        /// <summary>
        /// This is the listener thread for the server. We basically sit in a tight
        /// loop forever waiting for incoming connections and then spin off a new client
        /// thread for each one.
        /// </summary>
        private void ListenForClients()
        {
            _tcpListener.Start();
            while (!_endThread)
            {
                if (_tcpListener.Pending())
                {
                    TcpClient client = _tcpListener.AcceptTcpClient();
                    Thread clientThread = new Thread(HandleClientComm);
                    clientThread.Start(client);
                }
            }
        }

        /// <summary>
        /// This is the main CoSy client communication thread.
        /// </summary>
        /// <param name="client">The handle of the TCP client connection</param>
        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            TelnetStream stream = new TelnetStream(tcpClient.GetStream());
            LineBuffer buffer = new LineBuffer(stream);

            try
            {
                DateTime loginTime = DateTime.Now;

                buffer.WriteLine("CIX Conferencing System");

                while (true)
                {
                    buffer.WriteString("login: ");
                    string loginString = buffer.ReadLine();
                    if (loginString == "qix" || loginString == "cix")
                    {
                        break;
                    }
                    buffer.WriteLine("Unknown login");
                }

                Version ver = Assembly.GetEntryAssembly().GetName().Version;
                string versionString = string.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
                buffer.WriteLine(string.Format("{0} {1}", Properties.Resources.CIXAPIProxyTitle, versionString));

                buffer.WriteString("Nickname? (Enter 'new' for new user) ");
                string userName = buffer.ReadLine();

                buffer.WriteString("Password: ");
                string password = buffer.ReadLineWithoutEcho();
                buffer.WriteLine("");

                Console.WriteLine("Logged in as '{0}' with password '{1}'", userName, password);

                APIRequest.Username = userName;
                APIRequest.Password = password;

                _forums = new Forums();

                buffer.WriteLine(string.Format("You are a member of {0} conference(s).", _forums.Count));
                buffer.WriteLine("Max scratchsize during file has been set to 20000000 bytes");

                TopLevelCommand(buffer);

                buffer.Output = null;

                // Round up login time to 1 minute.
                TimeSpan onlineTime = (DateTime.Now - loginTime).Add(new TimeSpan(0, 1, 0));

                buffer.WriteLine(string.Format("{0}, you have been online {1}:{2} on line 5", userName, onlineTime.Hours,
                                               onlineTime.Minutes));
                buffer.WriteLine("Goodbye from CIX     !!!HANGUP NOW!!!");
            }
            catch (IOException)
            {
                Console.WriteLine("Client terminated connection");
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Client terminated connection");
            }
            catch (AuthenticationException)
            {
                Properties.Settings.Default.Save();
            }

            tcpClient.Close();
        }

        /// <summary>
        /// Handle the top-level Main commands.
        /// </summary>
        /// <param name="buffer">The telnet input stream</param>
        private void TopLevelCommand(LineBuffer buffer)
        {
            ChangeState(CoSyState.MAIN);
            do
            {
                if (!buffer.IsRunningScript)
                {
                    buffer.Output = null;
                    buffer.WriteString(PromptForState());
                }
            }
            while (!_endThread && !ActionCommand(buffer, buffer.ReadLine()));
        }

        /// <summary>
        /// Switch to the new command state.
        /// </summary>
        /// <param name="newState">The state to switch to</param>
        private void ChangeState(CoSyState newState)
        {
            if (newState == CoSyState.MAIN)
            {
                _currentTopic = null;
            }
            _stateStack.Push(newState);
        }

        /// <summary>
        /// Exit from the current state and return to the previous one.
        /// Does nothing if we're at the first state.
        /// </summary>
        private void QuitState()
        {
            if (_stateStack.Count > 0)
            {
                _stateStack.Pop();
            }
        }

        /// <summary>
        /// Return the state at the top of the stack.
        /// </summary>
        private CoSyState CurrentState
        {
            get { return _stateStack.Peek(); }
        }

        /// <summary>
        /// Return the command prompt for the current state we're in.
        /// </summary>
        /// <returns>Command prompt string</returns>
        private string PromptForState()
        {
            switch (CurrentState)
            {
                case CoSyState.MAIN:
                    return _isTerse ? "M:" : "Main:";

                case CoSyState.OPT:
                    return "Opt:";

                case CoSyState.READ:
                    return _isTerse ? "R:" : "Read:";
            }
            return "";
        }

        /// <summary>
        /// Direct the specified input to the appropriate command handler depending
        /// on the state which we're in.
        /// </summary>
        /// <param name="buffer">The I/O buffer</param>
        /// <param name="command">The command string</param>
        /// <returns>Return true if the command was a BYE</returns>
        private bool ActionCommand(LineBuffer buffer, string command)
        {
            Parser parser = new Parser(command);
            bool isBye = false;

            switch (CurrentState)
            {
                case CoSyState.MAIN:
                    isBye = MainCommand(buffer, parser);
                    break;

                case CoSyState.OPT:
                    isBye = OptionCommand(parser);
                    break;

                case CoSyState.READ:
                    isBye = ReadCommand(buffer, parser);
                    break;
            }
            return isBye;
        }

        /// <summary>
        /// Handle commands at the main level.
        /// </summary>
        /// <param name="buffer">Line buffer</param>
        /// <param name="parser">The command parser</param>
        private bool MainCommand(LineBuffer buffer, Parser parser)
        {
            Parser.ParseToken nextCommand = parser.NextCommand();

            if (nextCommand == Parser.ParseToken.FILE)
            {
                buffer.Output = _scratch;
                nextCommand = parser.NextCommand();
            }

            switch (nextCommand)
            {
                case Parser.ParseToken.DOWNLOAD:
                    DownloadCommand(buffer);
                    break;

                case Parser.ParseToken.UPLOAD:
                    UploadCommand(buffer);
                    break;

                case Parser.ParseToken.OPTION:
                    ChangeState(CoSyState.OPT);
                    OptionCommand(parser);
                    break;

                case Parser.ParseToken.SCPUT:
                    ScputCommand(parser);
                    break;

                case Parser.ParseToken.SCRIPT:
                    buffer.Script = _script;
                    break;

                case Parser.ParseToken.JOIN:
                    JoinCommand(buffer, parser);
                    break;

                case Parser.ParseToken.RESIGN:
                    ResignCommand(buffer, parser);
                    break;

                case Parser.ParseToken.READ:
                    if (parser.NextArgument() == "all")
                    {
                        buffer.WriteLine("......");
                        ReadScratchpad(buffer);
                        buffer.WriteLine("");
                        ShowScratchpadSize(buffer);
                    }
                    break;

                case Parser.ParseToken.KILLSCRATCH:
                    _scratch.Clear();
                    buffer.WriteLine("Scratchpad Deleted");
                    break;

                case Parser.ParseToken.BYE:
                    return ByeCommand(buffer, parser);
            }
            return false;
        }

        /// <summary>
        /// Handle commands at the Option level
        /// </summary>
        /// <param name="parser">Parser object</param>
        private bool OptionCommand(Parser parser)
        {
            string optionArgument = parser.NextArgument();

            while (optionArgument != "")
            {
                switch (optionArgument)
                {
                    case "q":
                        QuitState();
                        return false;

                    case "file":
                        break;

                    case "terse":
                        _isTerse = true;
                        break;
                }
                optionArgument = parser.NextArgument();
            }
            return false;
        }

        /// <summary>
        /// Handle commands at the Read level
        /// </summary>
        /// <param name="buffer">Line buffer</param>
        /// <param name="parser">Parser</param>
        private bool ReadCommand(LineBuffer buffer, Parser parser)
        {
            switch (parser.NextCommand())
            {
                case Parser.ParseToken.COMMENT:
                    CommentCommand(buffer, parser);
                    break;

                case Parser.ParseToken.SAY:
                    SayCommand(buffer);
                    break;

                case Parser.ParseToken.OPTION:
                    ChangeState(CoSyState.OPT);
                    OptionCommand(parser);
                    break;

                case Parser.ParseToken.JOIN:
                    JoinCommand(buffer, parser);
                    break;

                case Parser.ParseToken.RESIGN:
                    ResignCommand(buffer, parser);
                    break;

                case Parser.ParseToken.KILLSCRATCH:
                    _scratch.Clear();
                    buffer.WriteLine("Scratchpad Deleted");
                    break;

                case Parser.ParseToken.BYE:
                    return ByeCommand(buffer, parser);

                case Parser.ParseToken.QUIT:
                    QuitState();
                    break;
            }
            return false;
        }

        /// <summary>
        /// Display a prompt and request a Yes or No response.
        /// </summary>
        /// <param name="buffer">Line buffer</param>
        /// <param name="prompt">The prompt string</param>
        /// <returns>True if the user responded Yes, false if anything else</returns>
        private static bool Ask(LineBuffer buffer, string prompt)
        {
            while (true)
            {
                buffer.WriteString(prompt);
                buffer.WriteString("Y\b");

                string yesNo = buffer.ReadLine();

                if (string.IsNullOrEmpty(yesNo))
                {
                    continue;
                }
                if (yesNo.ToUpper().StartsWith("Y"))
                {
                    return true;
                }
                if (yesNo.ToUpper().StartsWith("N"))
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Handle the SCPUT command.
        /// </summary>
        /// <param name="parser">Parser</param>
        private void ScputCommand(Parser parser)
        {
            switch (parser.NextArgument())
            {
                case "script":
                    _script = _scratch.ToString();
                    _scratch.Clear();
                    break;
            }
        }

        /// <summary>
        /// Handle the BYE command.
        /// </summary>
        /// <param name="buffer">Line buffer</param>
        /// <param name="parser">Parser</param>
        private bool ByeCommand(LineBuffer buffer, Parser parser)
        {
            if (parser.NextArgument() != "y" && _scratch.Length > 0)
            {
                buffer.WriteString("OK to delete your scratchpad? (y/n/q)? ");
                string yesNoQuit = buffer.ReadLine();
                if (yesNoQuit == "q")
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Handle the JOIN command.
        /// </summary>
        /// <param name="buffer">Line buffer</param>
        /// <param name="parser">Parser</param>
        private void JoinCommand(LineBuffer buffer, Parser parser)
        {
            string forumName = parser.NextArgument();
            if (!string.IsNullOrEmpty(forumName))
            {
                string topicName = null;

                string[] joinParams = forumName.Split(new[] {'/'});
                if (joinParams.Length == 2)
                {
                    forumName = joinParams[0];
                    topicName = joinParams[1];
                }

                if (!_forums.IsJoined(forumName))
                {
                    buffer.WriteString(string.Format("You are not registered in '{0}'. ", forumName));
                    if (Ask(buffer, "Would you like to register  (y/n)? "))
                    {
                        if (!_forums.Join(forumName))
                        {
                            buffer.WriteRedirectableLine(string.Format("No conference '{0}'.", forumName));
                            return;
                        }
                    }
                }

                Forum thisForum = _forums[forumName];
                Topic thisTopic;

                do
                {
                    if (topicName != null && !thisForum.Topics.Contains(topicName))
                    {
                        buffer.WriteLine("Couldn't find topic: No such file or directory");
                        buffer.WriteLine(string.Format("Topic '{0}' not found.", topicName));

                        topicName = thisForum.Topics[0].Name;
                    }

                    if (topicName == null)
                    {
                        buffer.WriteString("Topics are:");
                        for (int c = 0; c < thisForum.Topics.Count; ++c)
                        {
                            string topicString = thisForum.Topics[c].Name;
                            if (c > 0)
                            {
                                buffer.WriteString(",");
                            }
                            buffer.WriteString(string.Format(" '{0}'", topicString));
                        }
                        buffer.WriteLine("");
                        buffer.WriteString("Topic? ");

                        topicName = buffer.ReadLine();
                        if (topicName == "quit")
                        {
                            return;
                        }
                    }

                    thisTopic = thisForum.Topics[topicName];
                } while (thisTopic == null);

                buffer.WriteRedirectableLine(string.Format("Joining conference '{0}', topic '{1}'. {2} new message(s).", forumName, topicName, thisTopic.Unread));
                _currentTopic = thisTopic;

                ChangeState(CoSyState.READ);
            }
        }

        /// <summary>
        /// Handle the RESIGN command.
        /// </summary>
        /// <param name="buffer">Input buffer</param>
        /// <param name="parser">Parser</param>
        private void ResignCommand(LineBuffer buffer, Parser parser)
        {
            string forumName = parser.NextArgument();
            string topicName = string.Empty;
            if (string.IsNullOrEmpty(forumName))
            {
                if (_currentTopic == null)
                {
                    buffer.WriteString("Conference name? ");
                    forumName = buffer.ReadLine();
                }
                else
                {
                    forumName = _currentTopic.Forum.Name;
                }
            }
            else
            {
                string[] joinParams = forumName.Split(new[] {'/'});
                if (joinParams.Length == 2)
                {
                    forumName = joinParams[0];
                    topicName = joinParams[1];
                }
            }

            if (!_forums.IsJoined(forumName))
            {
                buffer.WriteLine(string.Format("You are not a member of conference '{0}'.", forumName));
                return;
            }

            buffer.WriteLine(string.Format("Resigning from conference '{0}'.", forumName));
            _forums.Resign(forumName, topicName);

            ChangeState(CoSyState.MAIN);
        }

        /// <summary>
        /// Start a new message in the specified topic. The message body follows
        /// in the input stream.
        /// </summary>
        /// <param name="buffer">Input stream</param>
        private void SayCommand(LineBuffer buffer)
        {
            SayOrCommentCommand(buffer, 0);
        }

        /// <summary>
        /// Comment to an existing message in the specified topic. The message body
        /// follows in the input stream. The command array contains the argument to the
        /// comment command.
        /// </summary>
        /// <param name="buffer">Input stream</param>
        /// <param name="parser">Buffer</param>
        private void CommentCommand(LineBuffer buffer, Parser parser)
        {
            if (_currentTopic != null)
            {
                int replyNumber = _currentTopic.MessageID;
                string commentArgument = parser.NextArgument();

                if (commentArgument != "")
                {
                    if (!Int32.TryParse(commentArgument, out replyNumber))
                    {
                        buffer.WriteLine("Invalid comment number");
                        return;
                    }
                }
                SayOrCommentCommand(buffer, replyNumber);
            }
        }

        /// <summary>
        /// Handle a SAY or COMMENT in a forum topic. The current topic must be set before this
        /// function is called or it does nothing. The replyNumber specifies whether this is a new
        /// thread (a SAY) or a reply to an existing thread (a COMMENT). A value of 0 starts a new
        /// thread otherwise it is the number of the message to which this is a reply.
        /// </summary>
        /// <param name="buffer">The I/O buffer</param>
        /// <param name="replyNumber">The message to which this is a reply</param>
        private void SayOrCommentCommand(LineBuffer buffer, int replyNumber)
        {
            if (_currentTopic != null)
            {
                PostMessage message = new PostMessage
                    {
                        MsgID = replyNumber,
                        Forum = _currentTopic.Forum.Name,
                        Topic = _currentTopic.Name
                    };

                buffer.WriteLine("Enter message. End with '.<CR>'");

                // The body of the message comprises multiple lines terminated by a line
                // that contains a single period character and nothing else.
                StringBuilder bodyMessage = new StringBuilder();

                string bodyLine = buffer.ReadLine();
                while (bodyLine != ".")
                {
                    bodyMessage.AppendLine(bodyLine.Trim());
                    bodyLine = buffer.ReadLine();
                }
                message.Body = bodyMessage.ToString();

                WebRequest wrPosturl = APIRequest.Post("forums/post", APIRequest.APIFormat.XML, message);

                buffer.WriteString("Adding..");

                try
                {
                    Stream objStream = wrPosturl.GetResponse().GetResponseStream();
                    if (objStream != null)
                    {
                        using (TextReader reader = new StreamReader(objStream))
                        {
                            XmlDocument doc = new XmlDocument {InnerXml = reader.ReadLine()};

                            if (doc.DocumentElement != null)
                            {
                                int newMessageNumber = Int32.Parse(doc.DocumentElement.InnerText);
                                buffer.WriteLine(string.Format("Message {0} added.", newMessageNumber));
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    if (e.Message.Contains("401"))
                    {
                        throw new AuthenticationException("Authentication Failed", e);
                    }
                }
            }
        }

        /// <summary>
        /// Do a ZModem download of the scratchpad and optionally delete the scratchpad
        /// after a successful completion.
        /// </summary>
        /// <param name="buffer">Output buffer</param>
        private void DownloadCommand(LineBuffer buffer)
        {
            buffer.WriteLine("Zmodem download started... (to abort ^X^X^X^X^X)");
            buffer.WriteLine(string.Format("Filesize {0} bytes, estimated time at 2880 cps : 1 sec", _scratch.Length));

            string scratchFileName = Path.GetTempFileName();
            using (FileStream fileStream = new FileStream(scratchFileName, FileMode.Create))
            {
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] rawScratchpad = encoder.GetBytes(_scratch.ToString());
                fileStream.Write(rawScratchpad, 0, rawScratchpad.Length);
            }

            ZModem.ZModem zModem = new ZModem.ZModem(buffer.Stream) {Filename = scratchFileName};
            bool success = zModem.Send();

            buffer.WriteLine(success ? "Download succeeded" : "Download failed");
            if (success)
            {
                buffer.WriteString("OK to delete the downloaded scratchpad-file? (y/n)? N\b");

                string yesNoResponse = buffer.ReadLine();
                if (yesNoResponse.Trim() == "Y")
                {
                    _scratch.Clear();
                }
            }

            File.Delete(scratchFileName);
        }

        /// <summary>
        /// Do a ZModem upload into the scratchpad then show the scratchpad size afterwards.
        /// </summary>
        /// <param name="buffer">The I/O buffer</param>
        private void UploadCommand(LineBuffer buffer)
        {
            buffer.WriteLine("Zmodem upload (to abort ^X^X^X^X^X)");

            string scratchFileName = Path.GetTempFileName();

            ZModem.ZModem zModem = new ZModem.ZModem(buffer.Stream) { Filename = scratchFileName };
            bool success = zModem.Receive();

            buffer.WriteLine(success ? "Upload succeeded" : "Upload failed");

            _scratch.Clear();
            if (success)
            {
                using (FileStream fileStream = new FileStream(scratchFileName, FileMode.Open))
                {
                    byte[] rawScratchpad = new byte[fileStream.Length];
                    fileStream.Read(rawScratchpad, 0, (int)fileStream.Length);

                    _scratch.Append(new StringBuilder(Encoding.ASCII.GetString(rawScratchpad)));
                }

                File.Delete(scratchFileName);
            }

            ShowScratchpadSize(buffer);
        }

        /// <summary>
        /// Show the current scratchpad size in bytes. Messages from the CIX API are reformatted into
        /// CoSy scratchpad format messages.
        /// </summary>
        /// <param name="buffer">The output buffer</param>
        private void ShowScratchpadSize(LineBuffer buffer)
        {
            buffer.WriteLine(string.Format("Scratchpad is {0} bytes.", _scratch.Length));
        }

        /// <summary>
        /// Read all unread messages into the scratchpad.
        /// </summary>
        /// <param name="buffer">The I/O buffer</param>
        private static void ReadScratchpad(LineBuffer buffer)
        {
            WebRequest wrGeturl = APIRequest.Get("user/cixtelnetd/65535/0/scratchpad", APIRequest.APIFormat.XML);
            try
            {
                Stream objStream = wrGeturl.GetResponse().GetResponseStream();
                if (objStream != null)
                {
                    using (XmlReader reader = XmlReader.Create(objStream))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Scratchpad));
                        Scratchpad scratchpad = (Scratchpad)serializer.Deserialize(reader);

                        buffer.WriteRedirectableLine("Checking for conference activity.");

                        Dictionary<string, List<ScratchpadMessagesMsg> > allMessages = new Dictionary<string, List<ScratchpadMessagesMsg> >();

                        foreach (ScratchpadMessagesMsg message in scratchpad.Messages)
                        {
                            string thisTopic = string.Format("{0}/{1}", message.Forum, message.Topic);
                            List<ScratchpadMessagesMsg> topicList;

                            if (!allMessages.TryGetValue(thisTopic, out topicList))
                            {
                                topicList = new List<ScratchpadMessagesMsg>();
                                allMessages[thisTopic] = topicList;
                            }
                            topicList.Add(message);
                        }

                        foreach (string thisTopic in allMessages.Keys)
                        {
                            List<ScratchpadMessagesMsg> topicList = allMessages[thisTopic];
                            buffer.WriteRedirectableLine(string.Format("Joining {0} {1} new message(s).", thisTopic, topicList.Count));

                            foreach (ScratchpadMessagesMsg message in topicList)
                            {
                                string [] monthString = { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec"};

                                StringBuilder messageHeader = new StringBuilder(); 
                                messageHeader.AppendFormat(">>>{0} {1} {2}({3})", thisTopic, message.ID, message.Author, message.Body.Length);

                                DateTime messageDate = DateTime.Parse(message.DateTime);

                                messageHeader.AppendFormat("{0}{1}{2} ", messageDate.Day, monthString[messageDate.Month - 1], messageDate.Year - 2000);
                                messageHeader.AppendFormat("{0}:{1}", messageDate.Hour, messageDate.Minute);
                                if (message.ReplyTo != "0")
                                {
                                    messageHeader.AppendFormat(" c{0}", message.ReplyTo);
                                }
                                buffer.WriteRedirectableLine(messageHeader.ToString());
                                buffer.WriteRedirectableLine(message.Body);
                            }
                        }

                        buffer.WriteRedirectableLine("No unread messages");
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Message.Contains("401"))
                {
                    throw new AuthenticationException("Authentication Failed", e);
                }
            }
        }
    }
}
