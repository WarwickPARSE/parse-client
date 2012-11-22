using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace PARSE.Library
{
    class ImageResizer
    {
        public static System.Drawing.Bitmap ResizeImage(System.Drawing.Bitmap image, int width, int height) 
        {
            Bitmap result = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(result)) 
            {
                //try to reduce the loss of quality 
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode =System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                //draw image into bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            return result; 
        }
    }
}
