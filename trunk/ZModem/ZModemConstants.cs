namespace ZModem
{
    internal static class ZModemConstants
    {
        public const int ZRQINIT = 0;           /* Request receive init */
        public const int ZRINIT = 1;            /* Receive init */
        public const int ZSINIT = 2;            /* Send init sequence (optional) */
        public const int ZACK = 3;              /* ACK to above */
        public const int ZFILE = 4;             /* File name from sender */
        public const int ZSKIP = 5;             /* To sender: skip this file */
        public const int ZNAK = 6;              /* Last packet was garbled */
        public const int ZFIN = 8;              /* Finish session */
        public const int ZRPOS = 9;             /* Resume data trans at this position */
        public const int ZDATA = 10;            /* Data packet(s) follow */
        public const int ZEOF = 11;             /* End of file */
        public const int ZCOMPL = 15;           /* Request is complete */
        public const int ZCAN = 16;             /* Other end canned session with CAN*5 */
        public const int ZFREECNT = 17;
        public const int ZCOMMAND = 18;
        public const int ZCRCE = 'h';           /* CRC next, frame ends, header packet follows */
        public const int ZCRCG = 'i';           /* CRC next, frame continues nonstop */
        public const int ZCRCQ = 'j';           /* CRC next, frame continues, ZACK expected */
        public const int ZCRCW = 'k';           /* CRC next, ZACK expected, end of frame */
        public const int ZRUB0 = 'l';           /* Translate to rubout 0177 */
        public const int ZRUB1 = 'm';           /* Translate to rubout 0377 */
        public const int ZOK = 0;               /* No error */
        public const int ZERROR = -1;
        public const int ZTIMEOUT = -2;         /* Receiver timeout error */
        public const int RCDO = -3;             /* Loss of carrier */
        public const int GOTOR = 256;
        public const int GOTCRCE = 360;
        public const int GOTCRCG = 361;
        public const int GOTCRCQ = 362;
        public const int GOTCRCW = 363;
        public const int GOTCAN = 272;
        public const int ZF0 = 3;
        public const int ZP0 = 0;
        public const int ZP1 = 1;
        public const int ZP2 = 2;
        public const int ZP3 = 3;
        public const byte DLE = 16;
        public const byte XON = 17;
        public const byte XOFF = 19;
        public const byte CAN = 24;
        public const byte CR = 13;
        public const byte LF = 10;
        public const byte ZPAD = (byte) '*';
        public const byte ZDLE = CAN;
        public const byte ZBIN = (byte) 'A';
        public const byte ZHEX = (byte) 'B';
        public const byte ZBIN32 = (byte) 'C';

        public const int ZMAXSPLEN = 1024;
        public const int ZMAXBUFLEN = ((ZMAXSPLEN*2) + 12);
        public const int ZATTNLEN = 32;                         // Max length of attention string
        public const int ZHEADERLEN = 4;                        // Receive/transmit header

        public const int ASSUCCESS = 0;
        public const int ASGENERALERROR = -1;
        public const int ASBUFREMPTY = -8;

        public const int XFER_RETURN_SUCCESS = 0;               // Successful file transfer
        public const int XFER_RETURN_CANT_GET_BUFFER = -601;    // Failed to allocate comm buffer
        public const int XFER_RETURN_REMOTE_ABORT = -606;       // CAN-CAN abort from remote end
        public const int XFER_RETURN_TOO_MANY_ERRORS = -610;    // Got too many errors to go on
        
        public const int ZMODEM_MAX_ERRORS = 10;                // Maximum number of block retries

        public const int CANFDX = 1;                            // Can handle full-duplex (yes for PC's)
        public const int CANOVIO = 2;                           // Can overlay disk and serial I/O (ditto)
        public const int CANBRK = 4;                            // Can send a break - true but superfluous
        public const int CANFC32 = 32;                          // Can use 32 bit crc frame checks - true
    }
}