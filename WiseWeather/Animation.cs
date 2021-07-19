using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WiseWeather
{
    public class Animation : INotifyPropertyChanged
    {
        private Thread animationThread;
        private ImageSource currentImageSource;
        private CroppedBitmap[] croppedBitmaps;

        public int CurrentSpriteIndex { get; set; }

        public int SpriteChangeDelay { get; set; }
        public ImageSource CurrentImageSource
        {
            get { return currentImageSource; }
            set
            {
                currentImageSource = value;
                OnPropertyChange("CurrentImageSource");
            }

        }


        public Animation(CroppedBitmap[] Bitmaps, int currSpriteIndex, int initialChangeDelay)
        {
            croppedBitmaps = Bitmaps;
            CurrentSpriteIndex = currSpriteIndex;
            SpriteChangeDelay = initialChangeDelay;

            CurrentImageSource = croppedBitmaps[currSpriteIndex];

        }

        private  void PlayAnimation()
        {
            while (true)
            {
                Thread.Sleep(SpriteChangeDelay);
                if (CurrentSpriteIndex >= croppedBitmaps.Length) CurrentSpriteIndex = 0;
                CurrentImageSource = croppedBitmaps[CurrentSpriteIndex];
                CurrentSpriteIndex++;
            }
        }

        public void Start()
        {
            if (animationThread == null)
            {
                animationThread = new Thread(PlayAnimation);
                animationThread.IsBackground = true;
                animationThread.Start();
            }
        }

        public void Stop()
        {
            if (animationThread.IsAlive)
            {
                animationThread.Abort();
                animationThread = null;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChange([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

    }
}
