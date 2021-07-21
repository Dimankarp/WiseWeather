using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WiseWeather
{
     static class ImageHandler
    {

    
        public static CroppedBitmap[] CropSpriteSheet(BitmapSource source, int spriteWidth)
        {
            CroppedBitmap[] croppedBitmaps = new CroppedBitmap[source.PixelWidth / spriteWidth];
            for(int i = 0; i < source.PixelWidth / spriteWidth; i++)
            {
                croppedBitmaps[i] = new CroppedBitmap(source, new System.Windows.Int32Rect(i * 150, 0, 150, 150));
                croppedBitmaps[i].Freeze(); //Making sprites cross-threaded.
            }
            return croppedBitmaps;
        }
    }
}
