﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace ZSS
{
    public static class WatermarkMaker
    {
        /// <summary>
        /// Get Image with Watermark
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Image GetImage(Image img)
        {
            switch (Program.conf.WatermarkMode)
            {
                case WatermarkType.NONE:
                    return img;
                case WatermarkType.TEXT:
                    return DrawWatermark(img, NameParser.Convert(NameParser.NameType.Watermark, true),
                        XMLSettings.DeserializeFont(Program.conf.WatermarkFont),
                        XMLSettings.DeserializeColor(Program.conf.WatermarkFontColor),
                        (int)Program.conf.WatermarkFontTrans, (int)Program.conf.WatermarkOffset,
                        (int)Program.conf.WatermarkBackTrans, XMLSettings.DeserializeColor(Program.conf.WatermarkGradient1),
                        XMLSettings.DeserializeColor(Program.conf.WatermarkGradient2),
                        XMLSettings.DeserializeColor(Program.conf.WatermarkBorderColor),
                        Program.conf.WatermarkPositionMode, (int)Program.conf.WatermarkCornerRadius,
                        Program.conf.WatermarkGradientType);
                case WatermarkType.IMAGE:
                    return DrawImageWatermark(img, Program.conf.WatermarkImageLocation, Program.conf.WatermarkPositionMode,
                        (int)Program.conf.WatermarkOffset);
                default:
                    return img;
            }
        }

        private static Image DrawWatermark(Image img, string drawText, Font font, Color fontColor, int fontTrans,
            int offset, int backTrans, Color backColor1, Color backColor2, Color borderColor,
            WatermarkPositionType position, int cornerRadius, LinearGradientMode gradientType)
        {
            try
            {
                Size textSize = TextRenderer.MeasureText(drawText, font);
                Size labelSize = new Size(textSize.Width + 10, textSize.Height + 10);
                Point labelPosition = FindPosition(position, offset, img.Size,
                    new Size(textSize.Width + 10, textSize.Height + 10), 1);
                if (Program.conf.WatermarkAutoHide && ((img.Width < labelSize.Width + offset) ||
                    (img.Height < labelSize.Height + offset)))
                {
                    throw new Exception("Image size smaller than watermark size.");
                }
                Rectangle labelRectangle = new Rectangle(Point.Empty, labelSize);
                GraphicsPath gPath;
                if (cornerRadius > 0)
                {
                    gPath = MyGraphics.RoundedRectangle(labelRectangle, cornerRadius);
                }
                else
                {
                    gPath = new GraphicsPath();
                    gPath.AddRectangle(labelRectangle);
                }
                Bitmap bmp = new Bitmap(labelRectangle.Width + 1, labelRectangle.Height + 1);
                Graphics g = Graphics.FromImage(bmp);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.FillPath(new LinearGradientBrush(labelRectangle, Color.FromArgb(backTrans, backColor1),
                    Color.FromArgb(backTrans, backColor2), gradientType), gPath);
                g.DrawPath(new Pen(Color.FromArgb(backTrans, borderColor)), gPath);
                g.DrawString(drawText, font, new SolidBrush(Color.FromArgb(fontTrans, fontColor)), 5, 5);
                Graphics gImg = Graphics.FromImage(img);
                gImg.SmoothingMode = SmoothingMode.HighQuality;
                gImg.DrawImage(bmp, labelPosition);
                if (Program.conf.WatermarkAddReflection)
                {
                    Bitmap bmp2 = AddReflection(bmp);
                    gImg.DrawImage(bmp2, new Rectangle(labelPosition.X, labelPosition.Y + bmp.Height - 1, bmp2.Width, bmp2.Height));
                }
            }
            catch (Exception ex)
            {
                FileSystem.AppendDebug(ex.ToString());
            }
            return img;
        }

        private static Image DrawImageWatermark(Image img, string imgPath, WatermarkPositionType position, int offset)
        {
            try
            {
                if (!string.IsNullOrEmpty(imgPath) && File.Exists(imgPath))
                {
                    Image img2 = Image.FromFile(imgPath);
                    img2 = ImageChangeSize((Bitmap)img2);
                    Point imgPos = FindPosition(position, offset, img.Size, img2.Size, 0);
                    if (Program.conf.WatermarkAutoHide && ((img.Width < img2.Width + offset) ||
                        (img.Height < img2.Height + offset)))
                    {
                        throw new Exception("Image size smaller than watermark size.");
                    }
                    Graphics g = Graphics.FromImage(img);
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.DrawImage(img2, imgPos);
                    if (Program.conf.WatermarkAddReflection)
                    {
                        Bitmap bmp = AddReflection((Bitmap)img2);
                        g.DrawImage(bmp, new Rectangle(imgPos.X, imgPos.Y + img2.Height - 1, bmp.Width, bmp.Height));
                    }
                    if (Program.conf.WatermarkUseBorder)
                    {
                        g.DrawRectangle(new Pen(Color.Black), new Rectangle(imgPos.X - 1, imgPos.Y - 1, img2.Width, img2.Height));
                    }
                }
            }
            catch (Exception ex)
            {
                FileSystem.AppendDebug(ex.ToString());
            }
            return img;
        }

        private static Bitmap AddReflection(Bitmap b)
        {
            b.RotateFlip(RotateFlipType.RotateNoneFlipY);
            b = b.Clone(new Rectangle(0, 0, b.Width, (int)(b.Height / 1.5)), PixelFormat.Format32bppArgb);
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            byte alpha;
            int nOffset = bmData.Stride - b.Width * 4;

            unsafe
            {
                byte* p = (byte*)(void*)bmData.Scan0;

                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        alpha = (byte)(200 - 200 * (y + 1) / b.Height);
                        if (p[3] > alpha) p[3] = alpha;
                        p += 4;
                    }
                    p += nOffset;
                }
            }

            b.UnlockBits(bmData);

            return b;
        }

        private static Bitmap ImageChangeSize(Bitmap img)
        {
            Bitmap bmp = new Bitmap((int)((float)img.Width / 100 * (float)Program.conf.WatermarkImageScale),
                (int)((float)img.Height / 100 * (float)Program.conf.WatermarkImageScale));
            Graphics g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height));
            return bmp;
        }

        private static Point FindPosition(WatermarkPositionType positionType, int offset, Size img, Size img2, int add)
        {
            Point position;
            switch (positionType)
            {
                case WatermarkPositionType.TOP_LEFT:
                    position = new Point(offset, offset);
                    break;
                case WatermarkPositionType.TOP_RIGHT:
                    position = new Point(img.Width - img2.Width - offset - add, offset);
                    break;
                case WatermarkPositionType.BOTTOM_LEFT:
                    position = new Point(offset, img.Height - img2.Height - offset - add);
                    break;
                case WatermarkPositionType.BOTTOM_RIGHT:
                    position = new Point(img.Width - img2.Width - offset - add, img.Height - img2.Height - offset - add);
                    break;
                case WatermarkPositionType.CENTER:
                    position = new Point(img.Width / 2 - img2.Width / 2 - add, img.Height / 2 - img2.Height / 2 - add);
                    break;
                case WatermarkPositionType.LEFT:
                    position = new Point(offset, img.Height / 2 - img2.Height / 2 - add);
                    break;
                case WatermarkPositionType.TOP:
                    position = new Point(img.Width / 2 - img2.Width / 2 - add, offset);
                    break;
                case WatermarkPositionType.RIGHT:
                    position = new Point(img.Width - img2.Width - offset - add, img.Height / 2 - img2.Height / 2 - add);
                    break;
                case WatermarkPositionType.BOTTOM:
                    position = new Point(img.Width / 2 - img2.Width / 2 - add, img.Height - img2.Height - offset - add);
                    break;
                default:
                    position = Point.Empty;
                    break;
            }
            return position;
        }
    }
}