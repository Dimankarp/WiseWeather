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
        private DateTime date;
        private string time;
        private string quote;
        private string quoteAuthor;
        private WeatherData currentWeather;
        public Thread TimeThread;
        public struct WeatherData
        {
            public Dictionary<string, string> Parameters { get; set; }
            /* Current Weather Params:
             Id
             Main
             Description;
             Temperature
             TempFeel
             Pressure
             Humidity
             WindSpeed
            */
        }
        public DateTime Date
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
