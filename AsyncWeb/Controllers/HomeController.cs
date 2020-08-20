using AsyncDemo;
using AsyncWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AsyncMock asyncMock = new AsyncMock();

        public HomeController(ILogger<HomeController> logger) { _logger = logger; }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }

        public IActionResult Index()
        {
            CancelANonCancellableTaskAsync().Wait();

            return View();
        }

        public IActionResult Privacy() { return View(); }

        private async Task CancelANonCancellableTaskAsync()
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                // Listening to key press to cancel
                var keyBoardTask = Task.Run(() =>
                {
                    // Sending the cancellation message
                    cancellationTokenSource.Cancel();
                });

                try
                {
                    // Running the long running task
                    var longRunningTask = asyncMock.LongRunningOperationWithCancellationTokenAsync(100,
                                                                                                   cancellationTokenSource.Token)
                        .ConfigureAwait(false);
                    var result = await longRunningTask;
                }
                catch (TaskCanceledException)
                {
                }
                await keyBoardTask;
            }
        }
    }
}
