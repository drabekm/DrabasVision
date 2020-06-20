using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrabasVision
{
    class Analyzer
    {
        public Analyzer()
        {

        }

        public Bitmap Analyse(Bitmap blackAndWhitewinformsBitmap, Bitmap grayscaleWinformsBitmap)
        {
            int width = blackAndWhitewinformsBitmap.Width;
            int height = blackAndWhitewinformsBitmap.Height;

            int[,] objectMask = FindObjects(blackAndWhitewinformsBitmap, width, height);
            int[,] separatedObjectsMask = SeparateObjects(objectMask, width, height);

            //Bitmap newWinformsBitmap = new Bitmap(width, height);
            grayscaleWinformsBitmap = ColorObjects(grayscaleWinformsBitmap, separatedObjectsMask, width, height);

            return grayscaleWinformsBitmap;

        }

        private int[,] FindObjects(Bitmap image, int width, int height)
        {
            int[,] objectMask = new int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pixel = image.GetPixel(x, y);
                    if (pixel.R + pixel.G + pixel.B == 0)
                    {
                        objectMask[x, y] = 1;
                    }
                }
            }
            return objectMask;
        }

        private Bitmap ColorObjects(Bitmap image, int[,] separatedObjectsMask, int width, int height)
        {
            Color[] colors = { Color.Yellow, Color.Red, Color.Blue, Color.Green};
            
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (separatedObjectsMask[x, y] > 0)
                    {
                        image.SetPixel(x, y, colors[separatedObjectsMask[x, y] % colors.Length]);
                    }
                }
            }

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
    }
}
