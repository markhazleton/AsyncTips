using AsyncApi.Models;
using AsyncDemo;
using Microsoft.AspNetCore.Http;
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
        private readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient client;

        /// <summary>
        ///
        /// </summary>
        /// <param name="logger"></param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _httpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2));

            client = new HttpClient();
            client.BaseAddress = new Uri(@"https://localhost:44377/api/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
            // Create new stopwatch.
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string myResult = "<h1>Results</h1>";

            List<WeatherForecast> resp;
            MockResults mockResults;
            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);

            try
            {
                response = await client.GetAsync($"remote/Results?loopCount={loopCount}&maxTimeMs={maxTimeMs}");
                if (response.IsSuccessStatusCode)
                {
                    mockResults = await response.Content.ReadFromJsonAsync<MockResults>();
                    myResult = $"{myResult} <br/><br/> Mock Result: {mockResults.Message } <br/>loops:{mockResults.LoopCount}<br/>max time:{mockResults.MaxTimeMS}<br/><hr/>";
                }
                else 
                {
                    mockResults = await response.Content.ReadFromJsonAsync<MockResults>();
                    myResult = $"{myResult} <br/><br/> Mock Result: {mockResults.Message } <br/>loops:{mockResults.LoopCount}<br/>max time:{mockResults.MaxTimeMS}<br/><hr/>";
                }
            }
            catch (Exception ex)
            {
                myResult = $"{myResult} <br/><br/> {response.Content} <br/><br/> {ex.Message}";
            }

            try
            {
                response = await client.GetAsync($"remote/weather");
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
            stopwatch.Stop();

            myResult = $"{myResult} <br/> Elapsed Time: {stopwatch.Elapsed.TotalMilliseconds}<br/>";

            return View("Index", myResult);
        }

        /// <summary>
        /// Privacy Page
        /// </summary>
        /// <returns></returns>
        public IActionResult Privacy() { return View(); }
    }
}