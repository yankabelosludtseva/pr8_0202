using System.Collections.Generic;

namespace WeatherWpfApp
{
    public class DayForecast
    {
        public string DayNumber { get; set; }
        public string MonthName { get; set; }
        public string DayLabel { get; set; }
        public List<PeriodForecast> Periods { get; set; }

        public DayForecast()
        {
            Periods = new List<PeriodForecast>();
        }
    }
}
