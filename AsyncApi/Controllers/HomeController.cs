using AsyncApi.Models;
using AsyncDemo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AsyncApi.Controllers
{
    /// <summary>
    /// Home MVC Controller
    /// </summary>
    public class HomeController : Controller
    {
        private readonly AsyncRetryPolicy<HttpResponseMessage> _httpIndexPolicy;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _httpWeatherPolicy;
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient client;
        private readonly Stopwatch stopWatch;
        private readonly Random jitter;


        /// <summary>
        ///
        /// </summary>
        /// <param name="logger"></param>
        public HomeController(ILogger<HomeController> logger)
        {
            jitter = new Random();
            _logger = logger;
            _httpIndexPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .WaitAndRetryAsync(0, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2)
                    + TimeSpan.FromSeconds(jitter.Next(0, 3)));
            _httpWeatherPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2)
                    + TimeSpan.FromSeconds(jitter.Next(0, 3)));

            client = new HttpClient();

            // How to get the base URL for the current web site
            //            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            stopWatch = new Stopwatch();
        }

        /// <summary>
        /// Home error page
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }

        /// <summary>
        /// Home Page
        /// </summary>
        /// <param name="loopCount"></param>
        /// <param name="maxTimeMs"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int loopCount = 1000, int maxTimeMs = 1000)
        {
            stopWatch.Reset();
            stopWatch.Start();
            string myResult = "<h1>Results</h1>";
            client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/"); ;




            MockResults mockResults;
            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);

            try
            {
                response = await _httpIndexPolicy.ExecuteAsync(() => client.GetAsync($"remote/Results?loopCount={loopCount}&maxTimeMs={maxTimeMs}"));
                if (response.IsSuccessStatusCode)
                {
                    mockResults = await response.Content.ReadFromJsonAsync<MockResults>();
                }
                else
                {
                    mockResults = new MockResults
                    {
                        Message = $"<br/>Remote Call Failed:{response.StatusCode}"
                    };
                }
                myResult = $"{myResult} <br/><br/> Mock Result: {mockResults.Message } <br/>loops:{mockResults.LoopCount}<br/>max time:{mockResults.MaxTimeMS}<br/>run time:{mockResults.RunTimeMS}<br/><hr/>";
            }
            catch (Exception ex)
            {
                myResult = $"{myResult} <br/><br/> {response.Content} <br/><br/> {ex.Message}";
            }

            // Stop timing.
            stopWatch.Stop();

            myResult = $"{myResult} <br/><strong>Total Elapsed Time: {stopWatch.Elapsed.TotalMilliseconds}</strong><br/>";

            return View("Index", myResult);
        }


        /// <summary>
        /// Weather
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Weather()
        {
            stopWatch.Reset();
            stopWatch.Start();
            string myResult = "<h1>Results</h1>";
            client.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/"); ;


            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);

            List<WeatherForecast> resp;
            try
            {
                response = await _httpWeatherPolicy.ExecuteAsync(() => client.GetAsync($"remote/weather"));

                if (response.IsSuccessStatusCode)
                {
                    resp = await response.Content.ReadFromJsonAsync<List<WeatherForecast>>();
                    var forecast = resp.FirstOrDefault();
                    myResult = $"{myResult} <br/><br/> Forecast for {forecast.Date.ToShortDateString() } is {forecast.Summary} ({forecast.TemperatureF} degrees)<br/><br/>";
                }
            }
            catch (Exception ex)
            {
                myResult = $"{myResult} <br/><br/> {response.Content} <br/><br/> {ex.Message}";
            }

            // Stop timing.
            stopWatch.Stop();

            myResult = $"{myResult} <br/><strong>Total Elapsed Time: {stopWatch.Elapsed.TotalMilliseconds}</strong><br/>";

            return View("Weather", myResult);
        }

        /// <summary>
        /// Privacy Page
        /// </summary>
        /// <returns></returns>
        public IActionResult Privacy() { return View(); }
    }
}