using AsyncApi.Models;
using AsyncDemo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AsyncApi.Controllers
{
    /// <summary>
    /// Home MVC Controller
    /// </summary>
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        ///
        /// </summary>
        /// <param name="logger"></param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
                    mockResults.Message = $"<br/>Remote Call Failed:{response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                mockResults.Message = ex.Message;
            }

            // Stop timing.
            stopWatch.Stop();


            object retries;
            var finalRetryCount = context.TryGetValue(retryCountKey, out retries);

            return View("Index", mockResults);
        }



        /// <summary>
        /// Privacy Page
        /// </summary>
        /// <returns></returns>
        public IActionResult Privacy() { return View(); }
    }
}