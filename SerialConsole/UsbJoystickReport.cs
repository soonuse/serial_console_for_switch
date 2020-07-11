using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialConsole
{
    public enum Hat
    {
        TOP = 0x00,
        TOP_RIGHT = 0x01,
        RIGHT = 0x02,
        BOTTOM_RIGHT = 0x03,
        BOTTOM = 0x04,
        BOTTOM_LEFT = 0x05,
        LEFT = 0x06,
        TOP_LEFT = 0x07,
        CENTER = 0x08,
    }
    public enum Button
    {
        Y = 0x01,
        B = 0x02,
        A = 0x04,
        X = 0x08,
        L = 0x10,
        R = 0x20,
        ZL = 0x40,
        ZR = 0x80,
        MINUS = 0x100,
        PLUS = 0x200,
        LCLICK = 0x400,
        RCLICK = 0x800,
        HOME = 0x1000,
        CAPTURE = 0x2000,
    }
    public enum Stick
    {
        MIN = 0,
        CENTER = 128,
        MAX = 255,
    }
    public class UsbJoystickReport
    {
        public ushort buttons;
        public byte hat;
        public byte lx;
        public byte ly;
        public byte rx;
        public byte ry;
        public byte[] GetBytes()
        {
            byte[] bArr = new byte[7];
            byte[] bButtons = BitConverter.GetBytes(buttons);
            bArr[0] = bButtons[0];
            bArr[1] = bButtons[1];
            bArr[2] = hat;
            bArr[3] = lx;
            bArr[4] = ly;
            bArr[5] = rx;
            bArr[6] = ry;
            return bArr;
        }
        public byte[] GetSerialBytes()
        {
            byte[] bArr = GetBytes();
            byte[] retArr = new byte[8];
            retArr[0] = (byte)(0x80 | (bArr[0] >> 1));
            for (int i = 1; i < 7; i++)
            {
               byte temp = (byte)(bArr[i - 1] << (7 - i));
                retArr[i] = (byte)((temp | (bArr[i] >> (i + 1))) & 0x7F);
            }
            retArr[retArr.Length - 1] = (byte)(bArr[bArr.Length - 1] & 0x7F);
            return retArr;
        }
        public void SetReport(
            ushort buttons = 0x00,
            Hat hat = Hat.CENTER,
            Stick lx = Stick.CENTER,
            Stick ly = Stick.CENTER,
            Stick rx = Stick.CENTER,
            Stick ry = Stick.CENTER
        )
        {
            this.buttons = buttons;
            this.hat = (byte)hat;
            this.lx = (byte)lx;
            this.ly = (byte)ly;
            this.rx = (byte)rx;
            this.ry = (byte)ry;
        }
        public void ResetReport()
        {
            this.buttons = 0x0000;
            this.hat = (byte)Hat.CENTER;
            this.lx = (byte)Stick.CENTER;
            this.ly = (byte)Stick.CENTER;
            this.rx = (byte)Stick.CENTER;
            this.ry = (byte)Stick.CENTER;
        }
    }
}
