using System;

namespace WeatherWpfApp
{
    internal class HourlyPoint
    {
        public DateTime Time { get; set; }
        public double Temp { get; set; }
        public double ApparentTemp { get; set; }
        public int Humidity { get; set; }
        public double PressureMmHg { get; set; }
        public double WindSpeedMs { get; set; }
        public int WindDirDeg { get; set; }
        public int WeatherCode { get; set; }
    }
}
