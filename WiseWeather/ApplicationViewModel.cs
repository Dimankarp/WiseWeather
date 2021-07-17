using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HtmlAgilityPack;
using System.Windows.Media.Imaging;

namespace WiseWeather
{
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        public DayInfo CurrentDay { get; }
        public Animation WeatherImageAnimation { get; }
        private string userCity;
        private string OPEN_WEATHER_KEY = "0d2795f4c1fb3b9c8b85e3bff1bc6c46";
        private string IP_DATA_KEY = "07828e29589b8d1376e50764483ad12aa371911cb2e6bf11692c0f7e";

        private string UserCity
        {
            get { return userCity; }
            set
            {
                userCity = value;
                OnPropertyChanged("UserCity");
            }

        }

        public ApplicationViewModel()
        {
            WebHandler.client = new System.Net.WebClient();
            UserCity = GetUserCity();

              string quoteBlock = GetQuoteBlock(WebHandler.GetString("https://www.brainyquote.com/quote_of_the_day#"));
              string[] quoteParts = quoteBlock.Split('\n');

            CurrentDay = new DayInfo()
            {
                Date = DateTime.Today,
                Quote = $"\"{quoteParts[2]}\"",
                QuoteAuthor = $"- {quoteParts[3]}",//The index is 2, because there is an extra \n character before "Quote of The Day"
                CurrentWeather = GetWeatherData(),
            };
            CurrentDay.TimeThread = new System.Threading.Thread(CurrentDay.UpdateTime);
            CurrentDay.TimeThread.IsBackground = true;
            CurrentDay.TimeThread.Start();

            WeatherImageAnimation = GetWeatherImageAnimation(int.Parse(CurrentDay.CurrentWeather.Parameters["id"]));
            WeatherImageAnimation.Start();
        }

        private string GetQuoteBlock(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            HtmlNode quoteBlock = document.DocumentNode.SelectNodes("//div[contains(@class, 'grid-item qb clearfix bqQt')]").ToArray()[1];
            return quoteBlock.InnerText;

        }

        private string GetUserCity()
        {
            string userIP = WebHandler.GetUserIP();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(WebHandler.GetString($"https://api.ipdata.co/{userIP}?api-key={IP_DATA_KEY}"));

            string city = document.DocumentNode.InnerHtml.Trim('{', '}').Split(',')[2].Split(':')[1];
            city = city.Trim(' ', '"');
            return city;
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
            weatherData.Parameters.Add("description", parsedData["weather"]["description"]);
            weatherData.Parameters.Add("temp", ((Math.Round(float.Parse(parsedData["main"]["temp"]) - 273)).ToString() + "°C"));
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
                    switch(weatherId.ToString())
                    {
                        case "800":
                            image.UriSource = new Uri("pack://application:,,,/Images/ClearSkySprite.png");
                            break;
                        case "801":
                            image.UriSource = new Uri("pack://application:,,,/Images/SkyCloudsSprite.png");
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


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

    }
}
