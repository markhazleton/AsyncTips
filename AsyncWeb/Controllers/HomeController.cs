using AsyncDemo;
using AsyncWeb.Models;
//using Polly.Retry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
//using Polly;
using System.Threading.Tasks;

namespace AsyncWeb.Controllers
{
    public class HomeController : Controller
    {
        //private readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        private readonly AsyncMock asyncMock = new AsyncMock();
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            //_httpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            //        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }

        public async Task<IActionResult> Index(int loopCount = 1000, int maxTimeMs = 1000)
        {
            // Create new stopwatch.
            Stopwatch stopwatch = new Stopwatch();

            // Begin timing.
            stopwatch.Start();


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

            // Stop timing.
            stopwatch.Stop();

            myResult = $"{myResult} <br/> Elapsed Time: {stopwatch.Elapsed.TotalMilliseconds}<br/>";

            return View("Index", myResult);
        }

        public IActionResult Privacy() { return View(); }

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
    }
}
