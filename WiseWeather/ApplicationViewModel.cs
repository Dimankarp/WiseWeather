using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HtmlAgilityPack;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Globalization;
using System.Configuration;

namespace WiseWeather
{
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        private DayInfo currentDay;
        private Animation weatherImageAnimation;
        private string userCity;
        private string userCountry;
        private string OPEN_WEATHER_KEY = ConfigurationManager.AppSettings.Get("OpenWeatherKey");
        private string IP_DATA_KEY = ConfigurationManager.AppSettings.Get("IPDataKey");

        private List<Thread> LaunchedThreads = new List<Thread>();

        public DayInfo CurrentDay
        {
            get { return currentDay; }
            set
            {
                currentDay = value;
                OnPropertyChanged("CurrentDay");
            }
        }

        public Animation WeatherImageAnimation
        {
            get { return weatherImageAnimation; }
            set
            {
                weatherImageAnimation = value;
                OnPropertyChanged("WeatherImageAnimation");
            }
        }
        public string UserCity
        {
            get { return userCity; }
            set
            {
                userCity = value;
                OnPropertyChanged("UserCity");
            }

        }

        public string UserCountry
        {
            get { return userCountry; }
            set
            {
                userCountry = value;
                OnPropertyChanged("UserCountry");
            }

        }

        private RelayCommand startAnimationCommand;
        public RelayCommand StartAnimationCommand
        {
            get
            {
                return startAnimationCommand ?? (startAnimationCommand = new RelayCommand(obj =>
                {
                    WeatherImageAnimation.Start();
                }));
            }
        }


        private RelayCommand stopAnimationCommand;
        public RelayCommand StopAnimationCommand
        {
            get
            {
                return stopAnimationCommand ?? (stopAnimationCommand = new RelayCommand(obj =>
                {
                    WeatherImageAnimation.Stop();
                }));
            }
        }


        public ApplicationViewModel()
        {
            WebHandler.client = new System.Net.WebClient();
            WebHandler.SetSecurityPoints();

            DayInitialize();
        }

        private void DayInitialize()
        {
            while(LaunchedThreads.Count != 0)
            { 
                if (LaunchedThreads.First().IsAlive) LaunchedThreads.First().Abort();
                LaunchedThreads.Remove(LaunchedThreads.First());
            }

            CurrentDay = new DayInfo()
            { Date = DateTime.Today.ToString().Split(' ')[0] };

            CurrentDay.TimeThread = new Thread(CurrentDay.UpdateTime);
            CurrentDay.TimeThread.IsBackground = true;
            LaunchedThreads.Add(CurrentDay.TimeThread);
            CurrentDay.TimeThread.Start();

            Thread LocationThread = new Thread(SetUserLocation);
            LocationThread.IsBackground = true;
            LaunchedThreads.Add(LocationThread);
            LocationThread.Start();

            Thread QuoteThread = new Thread(SetQuote);
            QuoteThread.IsBackground = true;
            LaunchedThreads.Add(QuoteThread);
            QuoteThread.Start();

            Thread DayCheckThread = new Thread(CheckDayChange);
            DayCheckThread.IsBackground = true;
            DayCheckThread.Start();

        }

        private void SetUserLocation()
        {
            userCity = "...";
            userCountry = "...";
            while (true)
            {
                try { GetUserLocation(); }
                catch { continue; }
                Thread WeatherThread = new Thread(SetWeatherData);
                WeatherThread.IsBackground = true;
                WeatherThread.Start();
                break;
            }

        }

        private void SetWeatherData()
        {
            while (true)
            {
                try { CurrentDay.CurrentWeather = GetWeatherData(); }
                catch { continue; }
                break;
            }
            WeatherImageAnimation = GetWeatherImageAnimation(int.Parse(CurrentDay.CurrentWeather.Parameters["id"]));
        }

        private void SetQuote()
        {
            CurrentDay.Quote = "...";
            CurrentDay.QuoteAuthor = "...";
            string quoteBlock;
            while (true)
            {
                try { quoteBlock = GetQuoteBlock(WebHandler.GetString("https://www.quotegarden.com/")); }
                catch { continue; }
                string[] quoteParts = quoteBlock.Split('~');
                while (quoteParts[0].Contains("&#"))
                {
                    int startIndex = quoteParts[0].IndexOf("&#"); //Start of &#33; - like string
                    int endIndex = APIParser.FindNextIndex(quoteParts[0], startIndex, ';');//End of &#33; - like string

                    quoteParts[0] = quoteParts[0].Replace(quoteParts[0].Substring(startIndex, endIndex - startIndex + 1),
                       Convert.ToChar(int.Parse(quoteParts[0].Substring(startIndex + 2, endIndex - startIndex - 2))).ToString());
                }
                CurrentDay.Quote = $"\"{quoteParts[0].Trim()}\"";
                CurrentDay.QuoteAuthor = $"- {quoteParts[1].Replace("&#160;", " ")}";
                break;
            }

        }


        private void GetUserLocation()
        {
            string userIP = WebHandler.GetUserIP();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(WebHandler.GetString($"https://api.ipdata.co/{userIP}?api-key={IP_DATA_KEY}"));

            string city = document.DocumentNode.InnerHtml.Trim('{', '}').Split(',')[2].Split(':')[1];
            city = city.Trim(' ', '"');
            UserCity = city;

            string country = document.DocumentNode.InnerHtml.Trim('{', '}').Split(',')[5].Split(':')[1];
            country = country.Trim(' ', '"');
            UserCountry = country;

        }

        private string GetQuoteBlock(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            HtmlNode quoteBlock = document.DocumentNode.SelectNodes("//script[contains(@language, 'JavaScript')]").ToArray().First();
            return quoteBlock.InnerText.Split('\n')[13 + DateTime.Today.Day].Split('"')[1];
            /*Yes-yes, I know - this is insane! Pure insanity! But I have to change this bloody module because of a damn CloudsFlare and BrainyQuote administration!
            //If I were to keep using BrainyQuote(which worked just fine until 17.07.21) I would have to deal with CloudsFlare checking systems,
            //Which are extremely difficult, just insanely hard to deal with(trust me, I've tried multiple solutions of which I didn't understand a thing of)
            So, I hope this little rant clears some things out and will clear my reputation as an amature programmer.*/
        }

        private DayInfo.WeatherData GetWeatherData()
        {
            HtmlDocument document = new HtmlDocument();
            DayInfo.WeatherData weatherData = new DayInfo.WeatherData();
            weatherData.Parameters = new Dictionary<string, string>();
            document.LoadHtml(WebHandler.GetString($"https://api.openweathermap.org/data/2.5/weather?q={userCity}&appid={OPEN_WEATHER_KEY}"));

            string text = document.DocumentNode.InnerHtml;
            Dictionary<string, Dictionary<string, string>> parsedData = APIParser.Parse(text);
            weatherData.Parameters.Add("id", parsedData["weather"]["id"]);
            weatherData.Parameters.Add("main", parsedData["weather"]["main"]);
            weatherData.Parameters.Add("description", parsedData["weather"]["description"].First().ToString().ToUpper() + parsedData["weather"]["description"].Substring(1));
            weatherData.Parameters.Add("temp", ((Math.Round(float.Parse(parsedData["main"]["temp"], NumberFormatInfo.InvariantInfo) - 273)).ToString() + "°C"));
            weatherData.Parameters.Add("feels_like", parsedData["main"]["feels_like"]);
            weatherData.Parameters.Add("humidity", parsedData["main"]["humidity"] + "%");
            weatherData.Parameters.Add("speed", parsedData["wind"]["speed"] + "m/s");

            return weatherData;
        }

        private Animation GetWeatherImageAnimation(int weatherId)
        {
            BitmapImage image = new BitmapImage();
            int spriteChangeDelay = 200; //200 ms is a default
            image.BeginInit();
            image.UriSource = new Uri("pack://application:,,,/Images/ClearSkySprite.png");
            switch (weatherId.ToString()[0])//Weather codes can be found in DayInfo.cs
            {
                case '2':
                    image.UriSource = new Uri("pack://application:,,,/Images/ThunderSprite.png");
                    break;
                case '3':
                    image.UriSource = new Uri("pack://application:,,,/Images/DarkCloudsSprite.png");
                    break;
                case '5':
                    image.UriSource = new Uri("pack://application:,,,/Images/RainSprite.png");
                    break;
                case '6':
                    image.UriSource = new Uri("pack://application:,,,/Images/SnowSprite.png");
                    break;
                case '7':
                    image.UriSource = new Uri("pack://application:,,,/Images/MistSprite.png");
                    break;
                case '8':
                    switch (weatherId.ToString())
                    {
                        case "800":
                            image.UriSource = new Uri("pack://application:,,,/Images/ClearSkySprite.png");
                            break;
                        case "801":
                            image.UriSource = new Uri("pack://application:,,,/Images/SkyCloudSprite.png");
                            break;
                        case "802":
                            image.UriSource = new Uri("pack://application:,,,/Images/CloudsSprite.png");
                            break;
                        case "803":
                            image.UriSource = new Uri("pack://application:,,,/Images/DarkCloudsSprite.png");
                            break;
                        case "804":
                            image.UriSource = new Uri("pack://application:,,,/Images/DarkCloudsSprite.png");
                            break;
                    }
                    break;
            }
            image.EndInit();
            return new Animation(ImageHandler.CropSpriteSheet(image, 150), 0, spriteChangeDelay);
        }


        private void CheckDayChange()
        {
            while (true)
            {
                Thread.Sleep(300000);//5 minutes
                if (CurrentDay.Date != DateTime.Today.ToString().Split(' ')[0])
                {
                    DayInitialize();
                    break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }



    }
}
