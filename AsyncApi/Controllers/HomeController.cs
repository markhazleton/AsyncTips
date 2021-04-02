using AsyncApi.Models;
using AsyncDemo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
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
        private const string retryCountKey = "retrycount";
        private readonly AsyncRetryPolicy<HttpResponseMessage> _httpIndexPolicy;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _httpWeatherPolicy;
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
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

            _httpWeatherPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2)
                    + TimeSpan.FromSeconds(jitter.Next(0, 3)));


            _httpIndexPolicy = HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryCount => TimeSpan.FromMilliseconds(retryCount),
                onRetry: (response, delay, retryCount, context) =>
                {
                    context[retryCountKey] = retryCount;
                });

            _httpClient = new HttpClient();

            // How to get the base URL for the current web site
            // client.BaseAddress = new Uri(apiUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
        /// 
        /// </summary>
        /// <param name="mockResult"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Create(MockResults mockResult)
            {
            if (mockResult != null)
                {
                // Start timing.
                stopWatch.Reset();
                stopWatch.Start();
                string myResult = "<h1>Results</h1>";
                _httpClient.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/"); ;
                var context = new Context { { retryCountKey, 0 } };
                HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);

                try
                    {
                    response = await _httpIndexPolicy.ExecuteAsync(ctx => _httpClient.PostAsJsonAsync($"remote/Results", mockResult), context);
                    if (response.IsSuccessStatusCode)
                        {
                        mockResult = await response.Content.ReadFromJsonAsync<MockResults>();
                        }
                    else
                        {
                        mockResult = new MockResults
                            {
                            Message = $"<br/>Remote Call Failed:{response.StatusCode}"
                            };
                        }
                    myResult = $"{myResult} <br/><br/> Mock Result: {mockResult.Message } <br/>loops:{mockResult.LoopCount}<br/>max time:{mockResult.MaxTimeMS}<br/>run time:{mockResult.RunTimeMS}<br/><hr/>";
                    }
                catch (Exception ex)
                    {
                    myResult = $"{myResult} <br/><br/> {response.Content} <br/><br/> {ex.Message}";
                    }

                // Stop timing.
                stopWatch.Stop();


                object retries;
                var finalRetryCount = context.TryGetValue(retryCountKey, out retries);


                myResult = $"{myResult}<br/><strong>Retries:</strong>{retries ??= 0} <br/><strong>Total Elapsed Time: {stopWatch.Elapsed.TotalMilliseconds}</strong><br/>";

                return View("Create", myResult);
                }
            return RedirectToAction("Create");
            }


        /// <summary>
        /// Home Page
        /// </summary>
        /// <param name="loopCount"></param>
        /// <param name="maxTimeMs"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index(int loopCount = 30, int maxTimeMs = 1500)
            {
            // Start timing.
            stopWatch.Reset();
            stopWatch.Start();
            string myResult = "<h1>Results</h1>";
            _httpClient.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/"); ;
            var context = new Context { { retryCountKey, 0 } };
            MockResults mockResults = new MockResults() { LoopCount = loopCount, MaxTimeMS = maxTimeMs };
            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);

            try
                {
                response = await _httpIndexPolicy.ExecuteAsync(ctx => _httpClient.PostAsJsonAsync($"remote/Results", mockResults), context);
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
                myResult = $"{myResult} <br/> Mock Result: {mockResults.Message } <br/>loops:{mockResults.LoopCount}<br/>max time:{mockResults.MaxTimeMS}<br/>run time:{mockResults.RunTimeMS}<br/>";
                }
            catch (Exception ex)
                {
                myResult = $"{myResult} <br/> {response.Content} <br/> {ex.Message}";
                }

            // Stop timing.
            stopWatch.Stop();


            object retries;
            var finalRetryCount = context.TryGetValue(retryCountKey, out retries);


            myResult = $"{myResult}<br/><strong>Retries:</strong>{retries ??= 0} <br/><strong>Total Elapsed Time: {stopWatch.Elapsed.TotalMilliseconds}</strong><br/>";

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
            _httpClient.BaseAddress = new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/"); ;


            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);

            List<WeatherForecast> resp;
            try
                {
                response = await _httpWeatherPolicy.ExecuteAsync(() => _httpClient.GetAsync($"remote/weather"));

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
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> WeatherTimeout(int timeOutMs=100000)
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