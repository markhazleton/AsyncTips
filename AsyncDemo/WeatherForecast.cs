using System;

namespace AsyncApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class WeatherForecast
    {
        private Random rng = new Random();

        /// <summary>
        /// 
        /// </summary>
        private readonly string[] Summaries = { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        /// <summary>
        /// 
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Summary
        {
            get
            {
                return Summaries[rng.Next(Summaries.Length)];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int TemperatureC
        {
            get
            {
                return rng.Next(-20, 55);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);




    }
}
