using AsyncDemo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AsyncApi.Controllers
{
    /// <summary>
    /// Polly Controller
    /// </summary>
    public class PollyController : BaseController
    {
        private readonly ILogger<PollyController> _logger;

        /// <summary>
        ///
        /// </summary>
        /// <param name="logger"></param>
        public PollyController(ILogger<PollyController> logger)
        {
            _logger = logger;
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
            MockResults mockResults = new() { LoopCount = loopCount, MaxTimeMS = maxTimeMs };
            HttpResponseMessage response = new(System.Net.HttpStatusCode.InternalServerError);

            try
            {
                response = await _httpIndexPolicy.ExecuteAsync(ctx => _httpClient.PostAsJsonAsync($"remote/Results", mockResults), context);
                if (response.IsSuccessStatusCode)
                {
                    mockResults = await response.Content.ReadFromJsonAsync<MockResults>();
                }
                else
                {
                    mockResults.ResultValue = $"{response.StatusCode}";
                    mockResults.Message = $"{response.Content}";
                }
            }
            catch (Exception ex)
            {
                mockResults.Message = $"Error:{ex.Message}";
            }

            // Stop timing.
            stopWatch.Stop();
            mockResults.RunTimeMS = stopWatch.ElapsedMilliseconds;
            object retries;
            var finalRetryCount = context.TryGetValue(retryCountKey, out retries);

            if ((int)retries > 0)
                mockResults.Message = $"{mockResults.Message} - retries:{retries}";

            return View("Index", mockResults);
        }
    }
}