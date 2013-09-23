using System;
using System.Text;
using Telnet;

namespace CIXAPIProxy
{
    class LineBuffer
    {
        private readonly TelnetStream _stream;
        private string[] _input;
        private StringBuilder _output;
        private int _lineIndex;

        public LineBuffer(TelnetStream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// Assigns a script to the input. All subsequent calls to ReadLine will
        /// read from the script until the lines are exhausted, then revert to
        /// reading from the input stream.
        /// </summary>
        public string Script
        {
            set {
                _input = value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                _lineIndex = 0;
            }
        }

        /// <summary>
        /// Returns whether or not we're running a script.
        /// </summary>
        public bool IsRunningScript
        {
            get
            {
                return (_input != null && _lineIndex < _input.Length);   
            }
        }

        /// <summary>
        /// Assigns a stringbuilder output object to capture any redirectable
        /// writes. To remove the redirection, assign a value of null to the
        /// Output property.
        /// </summary>
        public StringBuilder Output
        {
            set
            {
                _output = value;
            }
        }

        /// <summary>
        /// Returns the I/O stream associated with this line buffer.
        /// </summary>
        public TelnetStream Stream
        {
            get { return _stream; }
        }

        /// <summary>
        /// Read a line from the input source. If were running a script, we
        /// return the next line from the script buffer. Otherwise we get the
        /// line from the input.
        /// </summary>
        /// <returns>The line read from the input</returns>
        public string ReadLine()
        {
            return IsRunningScript ? _input[_lineIndex++] : _stream.ReadString();
        }

        /// <summary>
        /// Read a line from the input without echoing it. Note that this
        /// cannot be used from a script.
        /// </summary>
        /// <returns>The line read from the input</returns>
        public string ReadLineWithoutEcho()
        {
            bool oldEcho = _stream.Echo;
            _stream.Echo = false;
            string inputString = _stream.ReadString();
            _stream.Echo = oldEcho;
            return inputString;
        }

        /// <summary>
        /// Write a string to the output followed by a line terminator.
        /// </summary>
        /// <param name="lineToWrite">The string to write</param>
        public void WriteLine(string lineToWrite)
        {
            _stream.WriteLine(lineToWrite);
        }

        /// <summary>
        /// Write a string to the output.
        /// </summary>
        /// <param name="stringToWrite">The string to write</param>
        public void WriteString(string stringToWrite)
        {
            _stream.WriteString(stringToWrite);
        }

        /// <summary>
        /// Write a string to the output, redirecting it to the output
        /// buffer if one has been assigned.
        /// </summary>
        /// <param name="lineToWrite">The line to write</param>
        public void WriteRedirectableLine(string lineToWrite)
        {
            if (_output != null)
            {
                _output.Append(lineToWrite);
                _output.Append("\n");
                return;
            }
            _stream.WriteString(lineToWrite);
            _stream.WriteString("\r\n");
        }
    }
}
