using AsyncApi.Models;
using AsyncDemo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncApi.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        private readonly AsyncMock asyncMock = new AsyncMock();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            //_httpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            //        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2));
        }

        private async Task<string> DemoStringAsync(int loopCount)
        {
            string sReturn = "init";
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                try
                {
                    // Running the long running task
                    var longRunningTask = asyncMock.LongRunningOperationWithCancellationTokenAsync(loopCount, cancellationTokenSource.Token)
                        .ConfigureAwait(false);
                    var result = await longRunningTask;

                    sReturn = $"Task Complete <br/>Result:{result}";
                }
                catch (TaskCanceledException)
                {
                    sReturn = "TaskCanceledException";
                }
            }
            return sReturn;
        }
        private HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(@"https://localhost:44377/api/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private async Task<string> GetResultsAsync(int loopCount, int maxTimeMs)
        {
            string myResult = "init";
            var listOfTasks = new List<Task>();
            var task1 = DemoStringAsync(loopCount);
            listOfTasks.Add(task1);
            var taskResults = await Task.WhenAll(listOfTasks.Select(x => Task.WhenAny(x, Task.Delay(TimeSpan.FromMilliseconds(maxTimeMs)))));
            var succeedResults = taskResults.OfType<Task<string>>().Select(s => s.Result).ToList();
            if (succeedResults.Count() != listOfTasks.Count())
            {
                myResult = "Time Out Occured";
            }
            else
            {
                myResult = succeedResults.FirstOrDefault();
            }

            return myResult;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }

        public async Task<IActionResult> Index(int loopCount = 1000, int maxTimeMs = 1000)
        {
            // Create new stopwatch.
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            IEnumerable<WeatherForecast> resp = new List<WeatherForecast>();
            string myResult = await GetResultsAsync(loopCount, maxTimeMs).ConfigureAwait(true);
            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            try
            {
                var client = GetHttpClient();
                string requestEndPoint = $"remote/";
                response = await client.GetAsync(requestEndPoint);
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

        public IActionResult Privacy() { return View(); }
    }
}
