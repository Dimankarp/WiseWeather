using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HtmlAgilityPack;
using System.Threading;

namespace WiseWeather
{
    public class DayInfo : INotifyPropertyChanged
    {
        private string date;
        private string time;
        private string quote;
        private string quoteAuthor;
        private WeatherData currentWeather;
        public Thread TimeThread;
        public struct WeatherData
        {
            public Dictionary<string, string> Parameters { get; set; }
            /* Current Weather Params(Key - Value):
             id - Id of weather:
                2XX - Thunderstorms
                3XX - Drizzles
                5XX - Rains
                6XX - Snows
                7XX - Atmosphere
                800 - Clear Sky
                    Clouds:
                801 - Few Clouds
                802 - Scattered Clouds
                803 - Broken Clouds
                804 - Overcast Clouds

             main - Name of weather
             description - A brief description of weather
             temp - An average temperature of air
             feels_like - What this temperature feels like
             humidity - Humidity of air
             speed - Speed of wind
            */
        }
        public string Date
        {
            get { return date; }
            set
            {
                date = value;
                OnPropertyChange("Date");
            }
        }

        public string Time
        {
            get { return time; }
            set
            {
                time = value;
                OnPropertyChange("Time");
            }

        }

        public string Quote
        {
            get { return quote; }
            set
            {
                quote = value;
                OnPropertyChange("Quote");
            }
        }

        public string QuoteAuthor
        {
            get { return quoteAuthor; }
            set
            {
                quoteAuthor = value;
                OnPropertyChange("QuoteAuthor");
            }
        }

        public WeatherData CurrentWeather
        {
            get { return currentWeather; }
            set
            {
                currentWeather = value;
                OnPropertyChange("CurrentWeather");
            }
        }

        public async void UpdateTime()
        {
            Time = DateTime.Now.ToString().Split()[1];
            await Task.Delay(1000);
            UpdateTime();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChange([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

    }
}
