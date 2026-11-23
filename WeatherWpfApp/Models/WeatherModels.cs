using System.Collections.Generic;

namespace WeatherWpfApp.Models
{
    public class DayWeather
    {
        public int DayNumber { get; set; }
        public string MonthName { get; set; }
        public string DayLabel { get; set; }
        public List<PeriodWeather> Periods { get; set; }
    }

    public class PeriodWeather
    {
        public string PeriodName { get; set; }   
        public string TempRange { get; set; }    
        public string IconUrl { get; set; }     
        public string Condition { get; set; }   
        public string Pressure { get; set; }   
        public string Humidity { get; set; }   
        public string WindSpeed { get; set; }  
        public string WindDir { get; set; }      
        public string FeelsLike { get; set; }   
    }
}