using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ReedSolomonImageEncoding
{
    public static class ImageProcessing
    {
        public static int[] GetRawBytesFromRGBImage(Bitmap image)
        {
            var data = new int[image.Width * image.Height * 3];

            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var pixelColor = image.GetPixel(x, y);
                    data[x * image.Height * 3 + 3 * y] = pixelColor.R;
                    data[x * image.Height * 3 + 3 * y + 1] = pixelColor.G;
                    data[x * image.Height * 3 + 3 * y + 2] = pixelColor.B;
                }
            }

            return data;
        }

        public static Bitmap GetRGBImageFromRawBytes(Bitmap image, int[] data)
        {
            var bitmap = new Bitmap(image);

            for (var x = 0; x < bitmap.Width; x++)
            {
                for (var y = 0; y < bitmap.Height; y++)
                {
                    var pixelColor =
                        Color.FromArgb(
                        data[x * bitmap.Height * 3 + 3 * y],
                        data[x * bitmap.Height * 3 + 3 * y + 1],
                        data[x * bitmap.Height * 3 + 3 * y + 2]
                        );
                    bitmap.SetPixel(x, y, pixelColor);
                }
            }

            return bitmap;
        }

        public static void SaveImageAs(Bitmap image, string fileName)
        {
            image.Save(fileName, ImageFormat.Bmp);
        }

        public static Bitmap OpenImage(string path)
        {
            return new Bitmap(path);
        }

        public static int Compare(Bitmap processedImage, Bitmap originalImage, out Bitmap diffImage)
        {
            var width = processedImage.Width;
            var height = processedImage.Height;
            diffImage = new Bitmap(width, height);

            var diffCount = 0;
            var eqColor = Color.White;
            var neColor = Color.Red;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (processedImage.GetPixel(x, y).Equals(originalImage.GetPixel(x, y)))
                        diffImage.SetPixel(x, y, eqColor);
                    else
                    {
                        diffImage.SetPixel(x, y, neColor);
                        diffCount++;
                    }
                }
            }

            return diffCount;
        }
    }
}
