using AsyncApi.Models;
using AsyncDemo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncApi.Controllers.api
{
    /// <summary>
    /// Remote Server MOCK
    /// </summary>
    [ApiController]
    [Route("api/remote")]
    public class RemoteController : ControllerBase
    {
        private readonly ILogger<RemoteController> _logger;
        private readonly AsyncMock asyncMock = new AsyncMock();

        /// <summary>
        ///
        /// </summary>
        /// <param name="logger"></param>
        public RemoteController(ILogger<RemoteController> logger)
        {
            _logger = logger;
        }

        private async Task<MockResults> MockResultsAsync(int loopCount)
        {
            MockResults returnMock = new MockResults() { LoopCount = loopCount, Message = "init" };
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                try
                {
                    // Running the long running task
                    var result = await asyncMock.LongRunningOperationWithCancellationTokenAsync(loopCount, cancellationTokenSource.Token)
                        .ConfigureAwait(false);
                    returnMock.Message = $"Task Complete";
                    returnMock.ResultValue = result.ToString();
                }
                catch (TaskCanceledException)
                {
                    returnMock.Message = "TaskCanceledException";
                    returnMock.ResultValue = "-1";
                }
            }
            return returnMock;
        }

        /// <summary>
        /// Get the current forecast
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("weather")]
        public IEnumerable<WeatherForecast> GetWeather(int days = 10)
        {
            return Enumerable.Range(1, days).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
            })
            .ToArray();
        }


        /// <summary>
        /// Get Results
        /// </summary>
        /// <param name="loopCount"></param>
        /// 
        /// <param name="maxTimeMs"></param>
        /// <returns></returns>
        /// <response code="200">Request Processed successfully.</response>
        /// <response code="200">Request Timeout.</response>
        [ProducesResponseType(typeof(MockResults), 200)]
        [ProducesResponseType(typeof(MockResults), 408)]
        [HttpGet]
        [Route("Results")]
        public async Task<IActionResult> GetResults(int loopCount, int maxTimeMs)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            MockResults myResult = new MockResults() { LoopCount = loopCount, MaxTimeMS = maxTimeMs, Message = "init" };
            var listOfTasks = new List<Task>();
            var task1 = MockResultsAsync(loopCount);
            listOfTasks.Add(task1);
            var taskResults = await Task.WhenAll(listOfTasks.Select(x => Task.WhenAny(x, Task.Delay(TimeSpan.FromMilliseconds(maxTimeMs)))));
            var succeedResults = taskResults.OfType<Task<MockResults>>().Select(s => s.Result).ToList();

            watch.Stop();
            myResult.RunTimeMS = (int)watch.ElapsedMilliseconds;
            if (succeedResults.Count() != listOfTasks.Count())
            {
                myResult.Message = "Time Out Occurred";
                myResult.ResultValue = "-1";
                return StatusCode((int)HttpStatusCode.RequestTimeout, myResult);
            }

            myResult.Message = succeedResults.FirstOrDefault().Message;
            myResult.ResultValue = succeedResults.FirstOrDefault().ResultValue;
            return Ok(myResult);
        }













    }
}
