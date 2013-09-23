namespace CIXAPIProxy
{
    public class Parser
    {
        struct ParseCommand
        {
            public string name { get; set; }
            public ParseToken token { get; set; }
        }

        public enum ParseToken
        {
            NONE,
            FILE,
            READ,
            JOIN,
            RESIGN,
            DOWNLOAD,
            UPLOAD,
            OPTION,
            SCPUT,
            SCRIPT,
            KILLSCRATCH,
            BYE,
            SAY,
            QUIT,
            COMMENT
        }

        // This is all the supported commands. However each state (Main, Opt, Read) have their own
        // set of commands. For now since we're being OLR driven rather than interactive, we don't
        // care. Later might want to have separate tables for each state
        private readonly ParseCommand[] _commands = new[]
            {
                new ParseCommand { name = "file", token = ParseToken.FILE },
                new ParseCommand { name = "read", token = ParseToken.READ },
                new ParseCommand { name = "join", token = ParseToken.JOIN },
                new ParseCommand { name = "resign", token = ParseToken.RESIGN },
                new ParseCommand { name = "download", token = ParseToken.DOWNLOAD },
                new ParseCommand { name = "upload", token = ParseToken.UPLOAD },
                new ParseCommand { name = "option", token = ParseToken.OPTION },
                new ParseCommand { name = "scput", token = ParseToken.SCPUT },
                new ParseCommand { name = "script", token = ParseToken.SCRIPT },
                new ParseCommand { name = "killscratch", token = ParseToken.KILLSCRATCH },
                new ParseCommand { name = "say", token = ParseToken.SAY },
                new ParseCommand { name = "comment", token = ParseToken.COMMENT },
                new ParseCommand { name = "quit", token = ParseToken.QUIT },
                new ParseCommand { name = "bye", token = ParseToken.BYE }
            };

        private readonly string[] _tokens;
        private int _tokenIndex;

        public Parser(string line)
        {
            _tokens = line.Trim().Split(new[] { ' ' });
            _tokenIndex = 0;
        }

        /// <summary>
        /// Retrieve the token of the next command in the input. If we can't
        /// determine the command or it is ambiguous, ParseToken.NONE is returned.
        /// </summary>
        /// <returns>The token ID of the command</returns>
        public ParseToken NextCommand()
        {
            ParseToken bestToken = ParseToken.NONE;

            if (_tokenIndex < _tokens.Length)
            {
                string name = _tokens[_tokenIndex++].ToLower();
                int bestMatchWeight = 999;

                foreach (ParseCommand command in _commands)
                {
                    if (command.name.StartsWith(name))
                    {
                        int matchWeight = command.name.Length - name.Length;
                        if (matchWeight < bestMatchWeight)
                        {
                            bestMatchWeight = matchWeight;
                            bestToken = command.token;
                        }
                    }
                }
            }
            return bestToken;
        }

        /// <summary>
        /// Return the next word from the command list.
        /// </summary>
        /// <returns>The next word, or an empty string if no more arguments</returns>
        public string NextArgument()
        {
            return (_tokenIndex < _tokens.Length) ? _tokens[_tokenIndex++] : "";
        }
    }
}
