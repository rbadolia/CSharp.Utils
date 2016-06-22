using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharp.Utils.Net
{
    public abstract class TelnetProtocolHandler
    {
        #region Constants

        private const byte DO = 253;

        private const byte DONT = 254;

        private const byte EOR = 239;

        private const byte IAC = 255;

        private const byte SB = 250;

        private const byte SE = 240;

        private const byte STATE_DATA = 0;

        private const byte STATE_IAC = 1;

        private const byte STATE_IACDO = 4;

        private const byte STATE_IACDONT = 6;

        private const byte STATE_IACSB = 2;

        private const byte STATE_IACSBDATA = 8;

        private const byte STATE_IACSBDATAIAC = 9;

        private const byte STATE_IACSBIAC = 7;

        private const byte STATE_IACWILL = 3;

        private const byte STATE_IACWONT = 5;

        private const byte TELOPT_BINARY = 0; /* binary mode */

        private const byte TELOPT_ECHO = 1; /* echo on/off */

        private const byte TELOPT_EOR = 25; /* end of record */

        private const byte TELOPT_NAWS = 31; /* NA-WindowSize*/

        private const byte TELOPT_SGA = 3; /* supress go ahead */

        private const byte TELOPT_TTYPE = 24; /* terminal type */

        private const byte TELQUAL_IS = 0;

        private const byte TELQUAL_SEND = 1;

        private const byte WILL = 251;

        private const byte WONT = 252;

        #endregion Constants

        #region Static Fields

        private static readonly byte[] IACSB = { IAC, SB };

        private static readonly byte[] IACSE = { IAC, SE };

        private static byte[] IACDO = { IAC, DO };

        private static byte[] IACDONT = { IAC, DONT };

        private static byte[] IACWILL = { IAC, WILL };

        private static byte[] IACWONT = { IAC, WONT };

        #endregion Static Fields

        #region Fields

        protected string terminalType = "dumb";

        protected Size windowSize = Size.Empty;

        private byte[] cr = new byte[2];

        private byte[] crlf = new byte[2];

        private byte current_sb;

        private byte neg_state;

        private byte[] receivedDX;

        private byte[] receivedWX;

        private byte[] sentDX;

        private byte[] sentWX;

        private byte[] tempbuf = new byte[0];

        #endregion Fields

        #region Constructors and Finalizers

        protected TelnetProtocolHandler()
        {
            this.Reset();

            this.crlf[0] = 13;
            this.crlf[1] = 10;
            this.cr[0] = 13;
            this.cr[1] = 0;
        }

        #endregion Constructors and Finalizers

        #region Public Properties

        public string CR
        {
            get
            {
                return Encoding.ASCII.GetString(this.cr);
            }

            set
            {
                this.cr = Encoding.ASCII.GetBytes(value);
            }
        }

        public string CRLF
        {
            get
            {
                return Encoding.ASCII.GetString(this.crlf);
            }

            set
            {
                this.crlf = Encoding.ASCII.GetBytes(value);
            }
        }

        #endregion Public Properties

        #region Methods

        protected void InputFeed(byte[] b, int len)
        {
            var bytesTmp = new byte[this.tempbuf.Length + len];

            Array.Copy(this.tempbuf, 0, bytesTmp, 0, this.tempbuf.Length);
            Array.Copy(b, 0, bytesTmp, this.tempbuf.Length, len);

            this.tempbuf = bytesTmp;
        }

        protected int Negotiate(byte[] nbuf)
        {
            int count = this.tempbuf.Length;
            if (count == 0)
            {
                return -1;
            }

            var sendbuf = new byte[3];
            var sbbuf = new byte[this.tempbuf.Length];
            byte[] buf = this.tempbuf;

            int sbcount = 0;
            int boffset = 0;
            int noffset = 0;

            bool done = false;
            bool foundSE = false;
            while (!done && (boffset < count && noffset < nbuf.Length))
            {
                byte b = buf[boffset++];
                if (b >= 128)
                {
                    b = (byte)(b - 256);
                }

                byte reply;
                switch (this.neg_state)
                {
                    case STATE_DATA:
                        if (b == IAC)
                        {
                            this.neg_state = STATE_IAC;
                        }
                        else
                        {
                            nbuf[noffset++] = b;
                        }

                        break;

                    case STATE_IAC:
                        switch (b)
                        {
                            case IAC:
                                this.neg_state = STATE_DATA;
                                nbuf[noffset++] = IAC; // got IAC, IAC: set option to IAC
                                break;

                            case WILL:
                                this.neg_state = STATE_IACWILL;
                                break;

                            case WONT:
                                this.neg_state = STATE_IACWONT;
                                break;

                            case DONT:
                                this.neg_state = STATE_IACDONT;
                                break;

                            case DO:
                                this.neg_state = STATE_IACDO;
                                break;

                            case EOR:
                                this.NotifyEndOfRecord();
                                this.neg_state = STATE_DATA;
                                break;

                            case SB:
                                this.neg_state = STATE_IACSB;
                                sbcount = 0;
                                break;

                            default:
                                this.neg_state = STATE_DATA;
                                break;
                        }

                        break;

                    case STATE_IACWILL:
                        switch (b)
                        {
                            case TELOPT_ECHO:
                                reply = DO;
                                this.SetLocalEcho(false);
                                break;

                            case TELOPT_SGA:
                                reply = DO;
                                break;

                            case TELOPT_EOR:
                                reply = DO;
                                break;

                            case TELOPT_BINARY:
                                reply = DO;
                                break;

                            default:
                                reply = DONT;
                                break;
                        }

                        if (reply != this.sentDX[b + 128] || WILL != this.receivedWX[b + 128])
                        {
                            sendbuf[0] = IAC;
                            sendbuf[1] = reply;
                            sendbuf[2] = b;
                            Write(sendbuf);

                            this.sentDX[b + 128] = reply;
                            this.receivedWX[b + 128] = WILL;
                        }

                        this.neg_state = STATE_DATA;
                        break;

                    case STATE_IACWONT:

                        switch (b)
                        {
                            case TELOPT_ECHO:
                                this.SetLocalEcho(true);
                                reply = DONT;
                                break;

                            case TELOPT_SGA:
                                reply = DONT;
                                break;

                            case TELOPT_EOR:
                                reply = DONT;
                                break;

                            case TELOPT_BINARY:
                                reply = DONT;
                                break;

                            default:
                                reply = DONT;
                                break;
                        }

                        if (reply != this.sentDX[b + 128] || WONT != this.receivedWX[b + 128])
                        {
                            sendbuf[0] = IAC;
                            sendbuf[1] = reply;
                            sendbuf[2] = b;
                            Write(sendbuf);

                            this.sentDX[b + 128] = reply;
                            this.receivedWX[b + 128] = WILL;
                        }

                        this.neg_state = STATE_DATA;
                        break;

                    case STATE_IACDO:
                        switch (b)
                        {
                            case TELOPT_ECHO:
                                reply = WILL;
                                this.SetLocalEcho(true);
                                break;

                            case TELOPT_SGA:
                                reply = WILL;
                                break;

                            case TELOPT_TTYPE:
                                reply = WILL;
                                break;

                            case TELOPT_BINARY:
                                reply = WILL;
                                break;

                            case TELOPT_NAWS:
                                Size size = this.windowSize;
                                this.receivedDX[b] = DO;

                                if (size.GetType() != typeof(Size))
                                {
                                    this.Write(IAC);
                                    this.Write(WONT);
                                    this.Write(TELOPT_NAWS);
                                    reply = WONT;
                                    this.sentWX[b] = WONT;
                                    break;
                                }

                                reply = WILL;
                                this.sentWX[b] = WILL;
                                sendbuf[0] = IAC;
                                sendbuf[1] = WILL;
                                sendbuf[2] = TELOPT_NAWS;

                                Write(sendbuf);
                                this.Write(IAC);
                                this.Write(SB);
                                this.Write(TELOPT_NAWS);
                                this.Write((byte)(size.Width >> 8));
                                this.Write((byte)(size.Width & 0xff));
                                this.Write((byte)(size.Height >> 8));
                                this.Write((byte)(size.Height & 0xff));
                                this.Write(IAC);
                                this.Write(SE);
                                break;

                            default:
                                reply = WONT;
                                break;
                        }

                        if (reply != this.sentWX[128 + b] || DO != this.receivedDX[128 + b])
                        {
                            sendbuf[0] = IAC;
                            sendbuf[1] = reply;
                            sendbuf[2] = b;
                            Write(sendbuf);

                            this.sentWX[b + 128] = reply;
                            this.receivedDX[b + 128] = DO;
                        }

                        this.neg_state = STATE_DATA;
                        break;

                    case STATE_IACDONT:
                        switch (b)
                        {
                            case TELOPT_ECHO:
                                reply = WONT;
                                this.SetLocalEcho(false);
                                break;

                            case TELOPT_SGA:
                                reply = WONT;
                                break;

                            case TELOPT_NAWS:
                                reply = WONT;
                                break;

                            case TELOPT_BINARY:
                                reply = WONT;
                                break;

                            default:
                                reply = WONT;
                                break;
                        }

                        if (reply != this.sentWX[b + 128] || DONT != this.receivedDX[b + 128])
                        {
                            sendbuf[0] = IAC;
                            sendbuf[1] = reply;
                            sendbuf[2] = b;
                            Write(sendbuf);

                            this.sentWX[b + 128] = reply;
                            this.receivedDX[b + 128] = DONT;
                        }

                        this.neg_state = STATE_DATA;
                        break;

                    case STATE_IACSBIAC:
                        for (int i = boffset; i < this.tempbuf.Length; i++)
                        {
                            if (this.tempbuf[i] == SE)
                            {
                                foundSE = true;
                            }
                        }

                        if (!foundSE)
                        {
                            boffset--;
                            done = true;
                            break;
                        }

                        foundSE = false;

                        if (b == IAC)
                        {
                            sbcount = 0;
                            this.current_sb = b;
                            this.neg_state = STATE_IACSBDATA;
                        }
                        else
                        {
                            this.neg_state = STATE_DATA;
                        }

                        break;

                    case STATE_IACSB:
                        for (int i = boffset; i < this.tempbuf.Length; i++)
                        {
                            if (this.tempbuf[i] == SE)
                            {
                                foundSE = true;
                            }
                        }

                        if (!foundSE)
                        {
                            boffset--;
                            done = true;
                            break;
                        }

                        foundSE = false;

                        switch (b)
                        {
                            case IAC:
                                this.neg_state = STATE_IACSBIAC;
                                break;

                            default:
                                this.current_sb = b;
                                sbcount = 0;
                                this.neg_state = STATE_IACSBDATA;
                                break;
                        }

                        break;

                    case STATE_IACSBDATA:
                        for (int i = boffset; i < this.tempbuf.Length; i++)
                        {
                            if (this.tempbuf[i] == SE)
                            {
                                foundSE = true;
                            }
                        }

                        if (!foundSE)
                        {
                            boffset--;
                            done = true;
                            break;
                        }

                        foundSE = false;

                        switch (b)
                        {
                            case IAC:
                                this.neg_state = STATE_IACSBDATAIAC;
                                break;

                            default:
                                sbbuf[sbcount++] = b;
                                break;
                        }

                        break;

                    case STATE_IACSBDATAIAC:
                        switch (b)
                        {
                            case IAC:
                                this.neg_state = STATE_IACSBDATA;
                                sbbuf[sbcount++] = IAC;
                                break;

                            case SE:
                                this.HandleSB(this.current_sb, sbbuf, sbcount);
                                this.current_sb = 0;
                                this.neg_state = STATE_DATA;
                                break;

                            case SB:
                                this.HandleSB(this.current_sb, sbbuf, sbcount);
                                this.neg_state = STATE_IACSB;
                                break;

                            default:
                                this.neg_state = STATE_DATA;
                                break;
                        }

                        break;

                    default:
                        this.neg_state = STATE_DATA;
                        break;
                }
            }

            var xb = new byte[count - boffset];
            Array.Copy(this.tempbuf, boffset, xb, 0, count - boffset);
            this.tempbuf = xb;

            return noffset;
        }

        protected abstract void NotifyEndOfRecord();

        protected void Reset()
        {
            this.neg_state = 0;
            this.receivedDX = new byte[256];
            this.sentDX = new byte[256];
            this.receivedWX = new byte[256];
            this.sentWX = new byte[256];
        }

        protected void SendTelnetControl(byte code)
        {
            var b = new byte[2];

            b[0] = IAC;
            b[1] = code;
            Write(b);
        }

        protected abstract void SetLocalEcho(bool echo);

        protected void Transpose(byte[] buf)
        {
            int i;

            byte[] xbuf;
            int nbufptr = 0;
            var nbuf = new byte[buf.Length * 2];

            for (i = 0; i < buf.Length; i++)
            {
                switch (buf[i])
                {
                    case IAC:
                        nbuf[nbufptr++] = IAC;
                        nbuf[nbufptr++] = IAC;
                        break;
                    case 10: // \n
                        if (this.receivedDX[TELOPT_BINARY + 128] != DO)
                        {
                            while (nbuf.Length - nbufptr < this.crlf.Length)
                            {
                                xbuf = new byte[nbuf.Length * 2];
                                Array.Copy(nbuf, 0, xbuf, 0, nbufptr);
                                nbuf = xbuf;
                            }

                            foreach (byte b in this.crlf)
                            {
                                nbuf[nbufptr++] = b;
                            }

                            break;
                        }

                        nbuf[nbufptr++] = buf[i];
                        break;
                    case 13: // \r
                        if (this.receivedDX[TELOPT_BINARY + 128] != DO)
                        {
                            while (nbuf.Length - nbufptr < this.cr.Length)
                            {
                                xbuf = new byte[nbuf.Length * 2];
                                Array.Copy(nbuf, 0, xbuf, 0, nbufptr);
                                nbuf = xbuf;
                            }

                            foreach (byte b in this.cr)
                            {
                                nbuf[nbufptr++] = b;
                            }
                        }
                        else
                        {
                            nbuf[nbufptr++] = buf[i];
                        }

                        break;
                    default:
                        nbuf[nbufptr++] = buf[i];
                        break;
                }
            }

            xbuf = new byte[nbufptr];
            Array.Copy(nbuf, 0, xbuf, 0, nbufptr);
            Write(xbuf);
        }

        protected abstract void Write(byte[] b);

        private void HandleSB(byte type, IList<byte> sbdata, int sbcount)
        {
            switch (type)
            {
                case TELOPT_TTYPE:
                    if (sbcount > 0 && sbdata[0] == TELQUAL_SEND)
                    {
                        this.Write(IACSB);
                        this.Write(TELOPT_TTYPE);
                        this.Write(TELQUAL_IS);

                        /* FIXME: need more logic here if we use
                        * more than one terminal type
                        */
                        this.Write(Encoding.ASCII.GetBytes(this.terminalType));
                        this.Write(IACSE);
                    }

                    break;
            }
        }

        private void Write(byte b)
        {
            Write(new[] { b });
        }

        #endregion Methods
    }
}
