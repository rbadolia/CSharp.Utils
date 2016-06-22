using System.Runtime.InteropServices;

namespace CSharp.Utils.Drawing
{
    [StructLayout(LayoutKind.Sequential)]
        public struct BitmapInfo
        {
        public uint Size;

        public int Width;

        public int Height;

        public short Planes;

        public short BitCount;

        public uint Compression;

        public uint SizeImage;

        public int XPelsPerMeter;

        public int YPelsPerMeter;

        public uint ClrUsed;

        public uint ClrImportant;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public uint[] Cols;
        }

}
