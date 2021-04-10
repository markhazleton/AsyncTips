using AsyncApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AsyncApi.Controllers
{
    /// <summary>
    /// Weather Controller
    /// </summary>
    public class WeatherController : BaseController
    {
        private readonly ILogger<WeatherController> _logger;

        /// <summary>
        ///
        /// </summary>
        /// <param name="logger"></param>
        public WeatherController(ILogger<WeatherController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Weather
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index(int timeOutMs = 100000)
        {
            {
                stopWatch.Reset();
                stopWatch.Start();
                string myResult = "<h1>Results</h1>";
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
                List<WeatherForecast> resp;
                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Get, $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/remote/weather");
                    using var cts = new System.Threading.CancellationTokenSource();
                    cts.CancelAfter(TimeSpan.FromMilliseconds(timeOutMs));
                    response = await _httpClient.SendAsync(req, cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        resp = await response.Content.ReadFromJsonAsync<List<WeatherForecast>>();
                        var forecast = resp.FirstOrDefault();
                        myResult = $"{myResult} <br/><br/> Forecast for {forecast.Date.ToShortDateString() } is {forecast.Summary} ({forecast.TemperatureF} degrees)<br/><br/>";
                    }
                }
                catch (Exception ex)
                {
                    myResult = $"{myResult} <br/><br/>{ex.Message}";
                }

                // Stop timing.
                stopWatch.Stop();

                myResult = $"{myResult}<br/><strong>Total Elapsed Time: {stopWatch.Elapsed.TotalMilliseconds}</strong><br/><strong>timeOutMs:{timeOutMs}</strong><br/>";

                return View("Weather", myResult);
            }
        }
    }
}