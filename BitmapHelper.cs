using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DrabasVision
{

    class BitmapHelper
    {
        /// <summary>
        /// Converts winforms bitmap to a wpf one.
        /// </summary>
        /// <param name="winformsImage">winforms bitmap</param>
        /// <returns>Wpf style bitmap</returns>
        public static BitmapImage ConvertWinformBitmapToWPFBitmap(Bitmap winformsImage)
        {
            BitmapImage wpfImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                winformsImage.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                wpfImage.BeginInit();
                wpfImage.StreamSource = memory;
                wpfImage.CacheOption = BitmapCacheOption.OnLoad;
                wpfImage.EndInit();
            }

            return wpfImage;
        }
    }
}
