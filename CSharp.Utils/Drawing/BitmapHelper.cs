using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace CSharp.Utils.Drawing
{
    public static class BitmapHelper
    {
        #region Constants

        private const uint BI_RGB = 0;

        private const uint DIB_RGB_COLORS = 0;

        private const int SRCCOPY = 0x00CC0020;

        #endregion Constants

        #region Static Fields

        private static ImageCodecInfo _tiffEncoder;

        #endregion Static Fields

        #region Public Methods and Operators

        public static Bitmap ChangePixelFormat(Bitmap inputBitmap, PixelFormat format)
        {
            Bitmap newBitmap = inputBitmap.Clone(new Rectangle(0, 0, inputBitmap.Width, inputBitmap.Height), format);
            return newBitmap;
        }

        public static void CopyBitmap(Bitmap sourceBitmap, Rectangle sourceRectangle, Bitmap targetBitmap, int targetX, int targetY)
        {
            BitmapData targetData = targetBitmap.LockBits(new Rectangle(targetX, targetY, sourceRectangle.Width, sourceRectangle.Height), ImageLockMode.WriteOnly, targetBitmap.PixelFormat);
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(sourceRectangle.X, sourceRectangle.Y, sourceRectangle.Width, sourceRectangle.Height), ImageLockMode.ReadOnly, sourceBitmap.PixelFormat);
            unsafe
            {
                var src = (byte*)sourceData.Scan0.ToPointer();
                var dst = (byte*)targetData.Scan0.ToPointer();

                int height = sourceRectangle.Height;
                int width = sourceRectangle.Width;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, src++, dst++)
                    {
                        *dst = *src;
                    }

                    src = src + sourceData.Stride - sourceData.Width;
                    dst = dst + targetData.Stride - targetData.Width;
                }
            }

            sourceBitmap.UnlockBits(sourceData);
            targetBitmap.UnlockBits(targetData);
        }

        public static Bitmap CopyToBpp(Bitmap b, int bpp)
        {
            if (bpp != 1 && bpp != 8)
            {
                throw new ArgumentException(@"1 or 8", "bpp");
            }

            int w = b.Width, h = b.Height;
            IntPtr hbm = b.GetHbitmap(); // this is step (1)
            var bmi = new BitmapInfo { Size = 40, Width = w, Height = h, Planes = 1, BitCount = (short)bpp, Compression = BI_RGB, SizeImage = (uint)(((w + 7) & 0xFFFFFFF8) * h / 8), XPelsPerMeter = 1000000, YPelsPerMeter = 1000000 };
            uint ncols = (uint)1 << bpp; // 2 colours for 1bpp; 256 colours for 8bpp
            bmi.ClrUsed = ncols;
            bmi.ClrImportant = ncols;
            bmi.Cols = new uint[256];
            if (bpp == 1)
            {
                bmi.Cols[0] = MAKERGB(0, 0, 0);
                bmi.Cols[1] = MAKERGB(255, 255, 255);
            }
            else
            {
                for (int i = 0; i < ncols; i++)
                {
                    bmi.Cols[i] = MAKERGB(i, i, i);
                }
            }

            IntPtr bits0;
            IntPtr hbm0 = NativeMethods.CreateDIBSection(IntPtr.Zero, ref bmi, DIB_RGB_COLORS, out bits0, IntPtr.Zero, 0);
            IntPtr sdc = NativeMethods.GetDC(IntPtr.Zero); // First we obtain the DC for the screen
            IntPtr hdc = NativeMethods.CreateCompatibleDC(sdc);
            NativeMethods.SelectObject(hdc, hbm);
            IntPtr hdc0 = NativeMethods.CreateCompatibleDC(sdc);
            NativeMethods.SelectObject(hdc0, hbm0);
            NativeMethods.BitBlt(hdc0, 0, 0, w, h, hdc, 0, 0, SRCCOPY);
            Bitmap b0 = Image.FromHbitmap(hbm0);
            NativeMethods.DeleteDC(hdc);
            NativeMethods.DeleteDC(hdc0);
            NativeMethods.ReleaseDC(IntPtr.Zero, sdc);
            NativeMethods.DeleteObject(hbm);
            NativeMethods.DeleteObject(hbm0);

            return b0;
        }

        public static Image CreateThumbnailImage(Stream sourceImageStream, Size size)
        {
            return CreateThumbnailImage(sourceImageStream, size.Width, size.Height);
        }

        public static Image CreateThumbnailImage(Stream sourceImageStream, int width, int height)
        {
            using (Image image = Image.FromStream(sourceImageStream))
            {
                return CreateThumbnailImage(image, width, height);
            }
        }

        public static Image CreateThumbnailImage(string sourceImageFileName, Size size)
        {
            return CreateThumbnailImage(sourceImageFileName, size.Width, size.Height);
        }

        public static Image CreateThumbnailImage(string sourceImageFileName, int width, int height)
        {
            using (Image image = Image.FromFile(sourceImageFileName))
            {
                return CreateThumbnailImage(image, width, height);
            }
        }

        public static Image CreateThumbnailImage(string sourceImageFileName, int width)
        {
            using (Image image = Image.FromFile(sourceImageFileName))
            {
                return CreateThumbnailImage(image, width, (image.Height * width) / image.Width);
            }
        }

        public static Image CreateThumbnailImage(Image sourceImage, Size size)
        {
            return CreateThumbnailImage(sourceImage, size.Width, size.Height);
        }

        public static Image CreateThumbnailImage(Image sourceImage, int width, int height)
        {
            Image thumbnailImage = sourceImage.GetThumbnailImage(width, height, thumbnailCallback, IntPtr.Zero);
            return thumbnailImage;
        }

        public static Bitmap FaqCopyTo1bpp(Bitmap b)
        {
            int w = b.Width, h = b.Height;
            var r = new Rectangle(0, 0, w, h);
            if (b.PixelFormat != PixelFormat.Format32bppPArgb)
            {
                var temp = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
                Graphics g = Graphics.FromImage(temp);
                g.DrawImage(b, r, 0, 0, w, h, GraphicsUnit.Pixel);
                g.Dispose();
                b = temp;
            }

            BitmapData bdat = b.LockBits(r, ImageLockMode.ReadOnly, b.PixelFormat);
            var b0 = new Bitmap(w, h, PixelFormat.Format1bppIndexed);
            BitmapData b0dat = b0.LockBits(r, ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int index = y * bdat.Stride + (x * 4);
                    if (Color.FromArgb(Marshal.ReadByte(bdat.Scan0, index + 2), Marshal.ReadByte(bdat.Scan0, index + 1), Marshal.ReadByte(bdat.Scan0, index)).GetBrightness() > 0.5f)
                    {
                        int index0 = y * b0dat.Stride + (x >> 3);
                        byte p = Marshal.ReadByte(b0dat.Scan0, index0);
                        var mask = (byte)(0x80 >> (x & 0x7));
                        Marshal.WriteByte(b0dat.Scan0, index0, (byte)(p | mask));
                    }
                }
            }

            b0.UnlockBits(b0dat);
            b.UnlockBits(bdat);
            return b0;
        }

        public static void FillRectangle(Bitmap bitmap, Color pixelColor, Rectangle rectangle)
        {
            Graphics g = Graphics.FromImage(bitmap);
            var sb = new SolidBrush(pixelColor);
            g.FillRectangle(sb, rectangle);
            g.Dispose();
        }

        public static Bitmap GetSubBitmap(Bitmap sourceBitmap, Rectangle region)
        {
            var b = new Bitmap(region.Width, region.Height, sourceBitmap.PixelFormat);
            CopyBitmap(sourceBitmap, region, b, 0, 0);
            return b;
        }

        public static Image ResizeImage(Image srcImg, Size newSize)
        {
            if (srcImg.Height < newSize.Height && srcImg.Width < newSize.Width)
            {
                return srcImg;
            }

            double heightChange = srcImg.Height / (double)newSize.Height;
            double widthChange = srcImg.Width / (double)newSize.Width;

            double Change = Math.Max(heightChange, widthChange);

            var newImgHeight = (int)(srcImg.Height / Change);
            var newImgWidth = (int)(srcImg.Width / Change);

            var b = new Bitmap(newImgWidth, newImgHeight);
            Graphics g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(srcImg, 0, 0, newImgWidth, newImgHeight);
            g.Dispose();

            return b;
        }

        public static Bitmap RotateImageByAngle(Image oldBitmap, float angle, Color backgroundColor)
        {
            var newBitmap = new Bitmap(oldBitmap.Width, oldBitmap.Height, oldBitmap.PixelFormat);
            Graphics graphics = Graphics.FromImage(newBitmap);
            var brush = new SolidBrush(backgroundColor);
            graphics.FillRectangle(brush, 0, 0, newBitmap.Width, newBitmap.Height);
            graphics.TranslateTransform((float)oldBitmap.Width / 2, (float)oldBitmap.Height / 2);
            graphics.RotateTransform(angle);
            graphics.TranslateTransform(-(float)oldBitmap.Width / 2, -(float)oldBitmap.Height / 2);
            graphics.DrawImage(oldBitmap, 0, 0, newBitmap.Width, newBitmap.Height);
            return newBitmap;
        }

        public static void SaveTiff(Image sourceImage, Bitmap modifiedBitmap, int modifiedBitmapIndex, string saveFileName)
        {
            int pageCount = sourceImage.GetFrameCount(FrameDimension.Page);
            if (_tiffEncoder == null)
            {
                _tiffEncoder = getTiffEncoder();
            }

            ImageCodecInfo encoderInfo = _tiffEncoder;
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
            Image saveImage = getPage(sourceImage, modifiedBitmap, 0, modifiedBitmapIndex);
            saveImage.Save(saveFileName, encoderInfo, encoderParameters);

            encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
            for (int i = 1; i < pageCount; i++)
            {
                Image img = getPage(sourceImage, modifiedBitmap, i, modifiedBitmapIndex);
                saveImage.SaveAdd(img, encoderParameters);
            }

            encoderParameters.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
            saveImage.SaveAdd(encoderParameters);
        }

        public static void SplashImage(Bitmap b, int x, int y)
        {
            IntPtr hbm = b.GetHbitmap();
            IntPtr sdc = NativeMethods.GetDC(IntPtr.Zero);
            IntPtr hdc = NativeMethods.CreateCompatibleDC(sdc);
            NativeMethods.SelectObject(hdc, hbm);
            NativeMethods.BitBlt(sdc, x, y, b.Width, b.Height, hdc, 0, 0, SRCCOPY);
            NativeMethods.DeleteDC(hdc);
            NativeMethods.ReleaseDC(IntPtr.Zero, sdc);
            NativeMethods.DeleteObject(hbm);
        }

        public static MemoryStream WriteImageToMemoryStream(Image image)
        {
            return WriteImageToMemoryStream(image, image.RawFormat);
        }

        public static MemoryStream WriteImageToMemoryStream(Image image, ImageFormat format)
        {
            var imageStream = new MemoryStream();
            image.Save(imageStream, format);
            return imageStream;
        }

        #endregion Public Methods and Operators

        #region Methods

        private static uint MAKERGB(int r, int g, int b)
        {
            return ((uint)(b & 255)) | ((uint)((r & 255) << 8)) | ((uint)((g & 255) << 16));
        }

        private static Image getPage(Image originalImage, Image modifiedImage, int pageIndex, int modifiedBitmapIndex)
        {
            Image img;
            if (pageIndex == modifiedBitmapIndex)
            {
                img = modifiedImage;
            }
            else
            {
                originalImage.SelectActiveFrame(FrameDimension.Page, pageIndex);
                img = originalImage;
            }

            return img;
        }

        private static ImageCodecInfo getTiffEncoder()
        {
            foreach (ImageCodecInfo v in ImageCodecInfo.GetImageEncoders())
            {
                if (v.MimeType == "image/tiff")
                {
                    return v;
                }
            }

            return null;
        }

        private static bool thumbnailCallback()
        {
            return true;
        }

        #endregion Methods

        private static class NativeMethods
        {
            #region Methods

            [DllImport("gdi32.dll")]
            public static extern int BitBlt(IntPtr hdcDst, int xDst, int yDst, int w, int h, IntPtr hdcSrc, int xSrc, int ySrc, int rop);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BitmapInfo bmi, uint Usage, out IntPtr bits, IntPtr hSection, uint dwOffset);

            [DllImport("gdi32.dll")]
            public static extern int DeleteDC(IntPtr hdc);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);

            [DllImport("user32.dll")]
            public static extern IntPtr GetDC(IntPtr hwnd);

            [DllImport("user32.dll")]
            public static extern int InvalidateRect(IntPtr hwnd, IntPtr rect, int bErase);

            [DllImport("user32.dll")]
            public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

            #endregion Methods
        }
    }
}
