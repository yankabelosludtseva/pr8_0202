using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WeatherWpfApp.Models;

namespace WeatherWpfApp.Services
{
    public class WeatherService
    {
        private static readonly HttpClient _http = new HttpClient();

        private static string GetCondition(int code)
        {
            if (code == 0) return "Ясно";
            if (code == 1 || code == 2) return "Малооблачно";
            if (code == 3) return "Облачно";
            if (code == 45 || code == 48) return "Туман";
            if (code == 51 || code == 53 || code == 55) return "Морось";
            if (code == 61 || code == 63 || code == 65) return "Дождь";
            if (code == 71 || code == 73 || code == 75) return "Снег";
            if (code == 80 || code == 81 || code == 82) return "Ливень";
            if (code == 95) return "Гроза";
            if (code == 96 || code == 99) return "Гроза с градом";
            return "—";
        }

        private static string GetIconUrl(int code)
        {
            if (code == 0) return "https://openweathermap.org/img/wn/01d@2x.png";
            if (code == 1 || code == 2) return "https://openweathermap.org/img/wn/02d@2x.png";
            if (code == 3) return "https://openweathermap.org/img/wn/03d@2x.png";
            if (code == 45 || code == 48) return "https://openweathermap.org/img/wn/50d@2x.png";
            if (code == 51 || code == 53 || code == 55) return "https://openweathermap.org/img/wn/09d@2x.png";
            if (code == 61 || code == 63 || code == 65 || code == 80 || code == 81 || code == 82)
                return "https://openweathermap.org/img/wn/10d@2x.png";
            if (code == 71 || code == 73 || code == 75) return "https://openweathermap.org/img/wn/13d@2x.png";
            if (code == 95 || code == 96 || code == 99) return "https://openweathermap.org/img/wn/11d@2x.png";
            return "https://openweathermap.org/img/wn/01d@2x.png";
        }

        private static string WindDirection(double deg)
        {
            string[] dirs = { "С", "СВ", "В", "ЮВ", "Ю", "ЮЗ", "З", "СЗ" };
            int idx = (int)Math.Round(((deg % 360) / 45)) % 8;
            return dirs[idx];
        }

        public async Task<List<DayWeather>> GetForecastAsync(string city)
        {
            string geoUrl = "https://geocoding-api.open-meteo.com/v1/search?name=" +
                            Uri.EscapeDataString(city) + "&count=1&language=ru&format=json";
            string geoJson = await _http.GetStringAsync(geoUrl);
            JObject geoObj = JObject.Parse(geoJson);

            JToken results = geoObj["results"];
            if (results == null || !results.HasValues)
                throw new Exception("Город «" + city + "» не найден.");

            double lat = results[0]["latitude"].Value<double>();
            double lon = results[0]["longitude"].Value<double>();

            string url = "https://api.open-meteo.com/v1/forecast?latitude=" +
                         lat.ToString(CultureInfo.InvariantCulture) +
                         "&longitude=" + lon.ToString(CultureInfo.InvariantCulture) +
                         "&hourly=temperature_2m,apparent_temperature,relative_humidity_2m,surface_pressure," +
                         "wind_speed_10m,wind_direction_10m,weather_code" +
                         "&forecast_days=3&timezone=auto";

            string json = await _http.GetStringAsync(url);
            JObject obj = JObject.Parse(json);
            JObject hourly = (JObject)obj["hourly"];

            JArray times = (JArray)hourly["time"];
            JArray temps = (JArray)hourly["temperature_2m"];
            JArray feels = (JArray)hourly["apparent_temperature"];
            JArray hums = (JArray)hourly["relative_humidity_2m"];
            JArray press = (JArray)hourly["surface_pressure"];
            JArray winds = (JArray)hourly["wind_speed_10m"];
            JArray windDirs = (JArray)hourly["wind_direction_10m"];
            JArray codes = (JArray)hourly["weather_code"];

            List<DayWeather> days = new List<DayWeather>();
            DateTime today = DateTime.Today;
            string[] monthNames = { "", "янв.", "фев.", "мар.", "апр.", "мая", "июн.",
                                    "июл.", "авг.", "сен.", "окт.", "ноя", "дек." };

            for (int d = 0; d < 3; d++)
            {
                DateTime dayDate = today.AddDays(d);
                DayWeather day = new DayWeather();
                day.DayNumber = dayDate.Day;
                day.MonthName = monthNames[dayDate.Month];
                day.DayLabel = d == 0 ? "Сегодня" : d == 1 ? "Завтра" : "Послезавтра";
                day.Periods = new List<PeriodWeather>();

                string[] periodNames = { "Утром", "Днём", "Вечером", "Ночью" };
                int[] startHours = { 6, 12, 18, 0 };
                int[] endHours = { 11, 17, 22, 5 };

                for (int p = 0; p < 4; p++)
                {
                    List<int> idxs = new List<int>();
                    for (int i = 0; i < times.Count; i++)
                    {
                        DateTime t = DateTime.Parse(times[i].ToString());
                        if (t.Date != dayDate) continue;

                        if (periodNames[p] == "Ночью")
                        {
                            if (t.Hour <= 5) idxs.Add(i);
                        }
                        else
                        {
                            if (t.Hour >= startHours[p] && t.Hour <= endHours[p]) idxs.Add(i);
                        }
                    }
                    if (idxs.Count == 0) continue;

                    double minT = idxs.Min(i => temps[i].Value<double>());
                    double maxT = idxs.Max(i => temps[i].Value<double>());
                    double avgFeels = idxs.Average(i => feels[i].Value<double>());
                    double avgHum = idxs.Average(i => hums[i].Value<double>());
                    double avgPressHpa = idxs.Average(i => press[i].Value<double>());
                    double avgWind = idxs.Average(i => winds[i].Value<double>());
                    double avgDir = idxs.Average(i => windDirs[i].Value<double>());
                    int code = codes[idxs[idxs.Count / 2]].Value<int>();

                    int pressMmHg = (int)Math.Round(avgPressHpa * 0.75006);

                    PeriodWeather pw = new PeriodWeather();
                    pw.PeriodName = periodNames[p];
                    pw.TempRange = string.Format("{0:+0;-0}…{1:+0;-0}°", minT, maxT);
                    pw.IconUrl = GetIconUrl(code);
                    pw.Condition = GetCondition(code);
                    pw.Pressure = pressMmHg.ToString();
                    pw.Humidity = ((int)avgHum).ToString() + "%";
                    pw.WindSpeed = avgWind.ToString("0.0", CultureInfo.InvariantCulture);
                    pw.WindDir = WindDirection(avgDir);
                    pw.FeelsLike = string.Format("{0:+0;-0}°", avgFeels);
                    day.Periods.Add(pw);
                }

                days.Add(day);
            }

            return days;
        }
    }
}