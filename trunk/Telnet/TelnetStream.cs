using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Telnet
{
    public sealed class TelnetStream
    {
        private const int BufferMax = 4096;

        private const char SE = (char)240;
        private const char SB = (char)250;
        private const char WILLTEL = (char)251;
        private const char DOTEL = (char)253;
        private const char IAC = (char)255;

        private const char BINARY = (char)0;
        private const char ECHO = (char)1;
        private const char TERMTYPE = (char)24;

        private const char SEND = (char)1;

        private readonly byte[] _buffer = new byte[BufferMax];
        private readonly NetworkStream _stream;
        private int _bufferIndex;
        private int _bufferSize;
        private bool _eatNewline;

        public TelnetStream(NetworkStream stream)
        {
            _stream = stream;
            _bufferIndex = 0;
            _bufferSize = 0;
        }

        /// <summary>
        /// Specifies whether data received from the client should be
        /// echoed back. This is generally controlled by an IAC code.
        /// </summary>
        public bool Echo { get; set; }

        public bool IsBinary { get; private set; }

        /// <summary>
        /// Write a line of text to the client followed by a newline (ASCII 10)
        /// character.
        /// </summary>
        /// <param name="str">The string to write</param>
        public void WriteLine(string str)
        {
            WriteString(str);
            WriteChar('\r');
        }

        /// <summary>
        /// Write a string to the client. Strings are sent in raw ASCII format.
        /// </summary>
        /// <param name="str">The string to write</param>
        public void WriteString(string str)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] rawString = encoder.GetBytes(str);
            _stream.Write(rawString, 0, rawString.Length);
        }

        /// <summary>
        /// Write a string to the client. Strings are sent in raw ASCII format.
        /// </summary>
        /// <param name="byteStream">The byte array to write</param>
        /// <param name="offset">The offset to start from</param>
        /// <param name="length">The length to write</param>
        public void WriteBytes(byte[] byteStream, int offset, int length)
        {
            _stream.Write(byteStream, offset, length);
        }

        /// <summary>
        /// Read a line from the client. The input line is assumed to be terminated by
        /// either a carriage return or a newline, or both in that order. If a carriage
        /// return is detected then we flag to discard the newline that MAY come later
        /// as we've no way of telling at this point.
        /// </summary>
        /// <returns>The line of text. Note that line terminators are stripped and
        /// are not part of the returned string.</returns>
        public string ReadString()
        {
            StringBuilder str = new StringBuilder();

            char ch = ReadChar();
            while (ch != '\n' && ch != '\r')
            {
                if (ch == '\b') // Backspace
                {
                    int currentIndex = str.Length - 1;
                    if (currentIndex >= 0)
                    {
                        str.Remove(currentIndex, 1);
                    }
                }
                else
                {
                    str.Append(ch);
                }
                ch = ReadChar();
            }
            _eatNewline = (ch == '\r');
            return str.ToString();
        }

        /// <summary>
        /// Write a single character to the client.
        /// </summary>
        /// <param name="ch">The character to write</param>
        public void WriteByte(char ch)
        {
            byte[] charByte = new[] { (byte)ch };
            _stream.Write(charByte, 0, 1);
        }

        /// <summary>
        /// Read a single character from the client. Data is buffered in blocks of
        /// BufferMax for efficiency.
        /// 
        /// If Read() returns zero then the client has effectively terminated the
        /// connection and we throw an exception.
        /// </summary>
        /// <returns>The raw ASCII character</returns>
        public char ReadByte()
        {
            if (_bufferIndex == _bufferSize)
            {
                _bufferSize = _stream.Read(_buffer, 0, BufferMax);
                _bufferIndex = 0;
            }
            if (_bufferIndex < _bufferSize)
            {
                char ch = (char)_buffer[_bufferIndex++];
                return ch;
            }
            throw new IndexOutOfRangeException();
        }

        public bool HasBytesToRead
        {
            get { return _stream.DataAvailable || _bufferIndex < _bufferSize; }
        }

        public void Clear()
        {
            while (_stream.DataAvailable)
            {
                _stream.Read(_buffer, 0, BufferMax);
                _bufferSize = 0;
                _bufferIndex = 0;
            }
        }

        /// <summary>
        /// Read a single character from the client. We also handle telnet IAC negotiation
        /// as part of the process. If Echo is enabled, the character is echoed back to the
        /// client.
        /// </summary>
        /// <returns>The character read (in ASCII)</returns>
        private char ReadChar()
        {
            while (true)
            {
                char ch = ReadByte();
                switch (ch)
                {
                    case IAC:
                        if (HandleIAC())
                        {
                            return (char)255;
                        }
                        break;

                    case '\n':
                        if (!_eatNewline)
                        {
                            if (Echo)
                            {
                                WriteChar(ch);
                            }
                            return ch;
                        }
                        _eatNewline = false;
                        break;

                    default:
                        if (Echo)
                        {
                            WriteChar(ch);
                        }
                        _eatNewline = false;
                        return ch;
                }
            }
        }

        /// <summary>
        /// Write a single character to the client, expanding any carriage return
        /// character to a carriage return then newline.
        /// </summary>
        /// <param name="ch">The character to write</param>
        private void WriteChar(char ch)
        {
            if (ch == '\r')
            {
                WriteByte('\n');
            }
            WriteByte(ch);
        }

        /// <summary>
        /// Handle the telnet IAC negotiation. This is typically a three character sequence
        /// of which the first is the IAC code.
        /// </summary>
        private bool HandleIAC()
        {
            char chRequest = ReadByte();
            if (chRequest == IAC)
            {
                return true;
            }
            if (chRequest == SB)
            {
                List<char> sbData = new List<char>();
                char chData = ReadByte();
                while (chData != SE)
                {
                    sbData.Add(chData);
                    chData = ReadByte();
                }
                return false;
            }
            char chParam = ReadByte();
            switch (chRequest)
            {
                case WILLTEL:
                    switch (chParam)
                    {
                        case TERMTYPE:
                            WriteChars(new[] {IAC, SB, TERMTYPE, SEND, SB});
                            break;
                    }
                    break;

                case DOTEL:
                    switch (chParam)
                    {
                        case BINARY:
                            IsBinary = true;
                            break;

                        case ECHO:
                            Echo = true;
                            break;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Write a character array to the client.
        /// </summary>
        /// <param name="charsToWrite">The character array to write</param>
        private void WriteChars(char[] charsToWrite)
        {
            foreach (char ch in charsToWrite)
            {
                WriteByte(ch);
            }
        }
    }
}
