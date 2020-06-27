using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DrabasVision
{
    class Analyzer
    {
        public WriteableBitmap Analyse(WriteableBitmap blackAndWhitewinformsBitmap, WriteableBitmap overlayBitmap)
        {
            int width = (int)blackAndWhitewinformsBitmap.Width;
            int height = (int)blackAndWhitewinformsBitmap.Height;

            blackAndWhitewinformsBitmap = BitmapHelper.ConvertToBgra32Format(blackAndWhitewinformsBitmap);
            overlayBitmap = BitmapHelper.ConvertToBgra32Format(overlayBitmap);

            int[,] objectMask = FindObjects(blackAndWhitewinformsBitmap, width, height);
            int[,] separatedObjectsMask = SeparateObjects(objectMask, width, height);

            overlayBitmap = ColorObjects(overlayBitmap, separatedObjectsMask, width, height);
            
            return overlayBitmap;

        }

        private WriteableBitmap ColorObjects(WriteableBitmap image, int[,] separatedObjectsMask, int width, int height)
        {           
            Color[] colors = { Colors.Yellow, Colors.Red, Colors.Blue, Colors.Green };

            int stride = image.PixelWidth * (image.Format.BitsPerPixel / 8);
            byte[] data = new byte[stride * image.PixelHeight];

            image.CopyPixels(data, stride, 0);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (separatedObjectsMask[x, y] > 0)
                    {
                        Color currentObjectColor = colors[separatedObjectsMask[x, y] % colors.Length];
                        int index = (y * stride) + (x * 4);
                        data[index + 3] = 255;
                        data[index + 2] = currentObjectColor.R;
                        data[index + 1] = currentObjectColor.G;
                        data[index] = currentObjectColor.B;
                    }
                }
            }

            image.WritePixels(new Int32Rect(0, 0, width, height), data, stride, 0);
            return image;
        }

        private int[,] SeparateObjects(int[,] objectMask, int width, int height)
        {
            int[,] separatedObjectsMask = new int[width, height];
            for (int y = 0; y < height; y++)
            {
                int objectIndex = 0;
                int leftPixelValue = 0; //object index of a pixel that's on left side of current pixel
                int topPixelValue = 0; //object index of a pixel that's on top side of current pixel

                for (int x = 0; x < width; x++)
                {
                    if (y != 0)
                    {
                        topPixelValue = separatedObjectsMask[x, y - 1];
                    }
                    if (x != 0)
                    {
                        leftPixelValue = separatedObjectsMask[x - 1, y];
                    }

                    if (objectMask[x, y] == 1)
                    {

                        if (leftPixelValue == 0 && topPixelValue == 0)
                        {
                            objectIndex++;
                            separatedObjectsMask[x, y] = objectIndex;
                        }
                        if (leftPixelValue > 0)
                        {
                            separatedObjectsMask[x, y] = leftPixelValue;
                        }
                        if (topPixelValue > 0)
                        {
                            separatedObjectsMask[x, y] = topPixelValue;
                        }
                    }
                }
            }

            return separatedObjectsMask;
        }

        private int[,] FindObjects(WriteableBitmap image, int width, int height)
        {
            int stride = image.PixelWidth * (image.Format.BitsPerPixel / 8);
            byte[] data = new byte[stride * image.PixelHeight];
            image.CopyPixels(data, stride, 0);
            int[,] objectMask = new int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int index = (y * stride) + (x * 4);
                    if(data[index] == 0)
                    {
                        objectMask[x, y] = 1;
                    }

                }
            }
            return objectMask;
        }
    }
}
