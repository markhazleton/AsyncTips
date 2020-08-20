using AsyncDemo;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTips
{
    internal class Program
    {
        private static readonly AsyncMock asyncMock = new AsyncMock();

        public static void Main(string[] args)
        {
            ExecuteTaskAsync().Wait();
            ExecuteTaskWithTimeoutAsync(TimeSpan.FromSeconds(2)).Wait();
            ExecuteTaskWithTimeoutAsync(TimeSpan.FromSeconds(10)).Wait();
            ExecuteManuallyCancellableTaskAsync().Wait();
            ExecuteManuallyCancellableTaskAsync().Wait();
            CancelANonCancellableTaskAsync().Wait();
        }

        private static async Task CancelANonCancellableTaskAsync()
        {
            Console.WriteLine(nameof(CancelANonCancellableTaskAsync));
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                // Listening to key press to cancel
                var keyBoardTask = Task.Run(() =>
                {
                    Console.WriteLine("Press enter to cancel");
                    Console.ReadKey();
                    // Sending the cancellation message
                    cancellationTokenSource.Cancel();
                });

                try
                {
                    // Running the long running task
                    var longRunningTask = asyncMock.LongRunningOperationWithCancellationTokenAsync(100,
                                                                                                   cancellationTokenSource.Token).ConfigureAwait(false);
                    var result = await longRunningTask;
                    Console.WriteLine("Result {0}", result);
                    Console.WriteLine("Press enter to continue");
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task was cancelled");
                }
                await keyBoardTask;
            }
        }

        private static async Task ExecuteManuallyCancellableTaskAsync()
        {
            Console.WriteLine(nameof(ExecuteManuallyCancellableTaskAsync));
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                // Creating a task to listen to keyboard key press
                var keyBoardTask = Task.Run(() =>
                {
                    Console.WriteLine("Press enter to cancel");
                    Console.ReadKey();
                    // Cancel the task
                    cancellationTokenSource.Cancel();
                });

                try
                {
                    var longRunningTask = asyncMock.LongRunningCancellableOperation(500, cancellationTokenSource.Token).ConfigureAwait(false);
                    var result = await longRunningTask;
                    Console.WriteLine("Result {0}", result);
                    Console.WriteLine("Press enter to continue");
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task was cancelled");
                }
                await keyBoardTask;
            }
        }

        private static async Task ExecuteTaskAsync()
        {
            Console.WriteLine(nameof(ExecuteTaskAsync));
            Console.WriteLine("Result {0}", await asyncMock.LongRunningOperation(100).ConfigureAwait(false));
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();
        }


        private static async Task ExecuteTaskWithTimeoutAsync(TimeSpan timeSpan)
        {
            Console.WriteLine(nameof(ExecuteTaskWithTimeoutAsync));
            using (var cancellationTokenSource = new CancellationTokenSource(timeSpan))
            {
                try
                {
                    var result = await asyncMock.LongRunningCancellableOperation(500, cancellationTokenSource.Token).ConfigureAwait(false);
                    Console.WriteLine("Result {0}", result);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task was cancelled");
                }
            }
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();
        }
    }
}
