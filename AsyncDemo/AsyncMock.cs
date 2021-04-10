using System.Threading;
using System.Threading.Tasks;

namespace AsyncDemo
{
    /// <summary>
    /// Samples for Async Methods
    /// Supporting Video: https://channel9.msdn.com/Series/Three-Essential-Tips-for-Async/Three-Essential-Tips-For-Async-Introduction
    /// Supporting Blog: https://johnthiriet.com/configure-await/
    /// </summary>
    public class AsyncMock
    {
        /// <summary>
        /// Compute a value for a long time.
        /// </summary>
        /// <returns>The value computed.</returns>
        /// <param name="loop">Number of iterations to do.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task<decimal> LongRunningCancellableOperation(int loop, CancellationToken cancellationToken)
        {
            Task<decimal> task = null;
            // Start a task a return it
            task = Task.Run(() =>
            {
                decimal result = 0;
                // Loop for a defined number of iterations
                for (int i = 0; i < loop; i++)
                {
                    // Check if a cancellation is requested, if yes,
                    // throw a TaskCanceledException.
                    if (cancellationToken.IsCancellationRequested)
                        throw new TaskCanceledException(task);

                    cancellationToken.ThrowIfCancellationRequested();

                    // Do something that takes times.
                    Thread.Sleep(i);
                    result += i;
                }
                return result;
            });
            return task;
        }

        /// <summary>
        /// Compute a value for a long time.
        /// </summary>
        /// <returns>The value computed.</returns>
        /// <param name="loop">Number of iterations to do.</param>
        public Task<decimal> LongRunningOperation(int loop)
        {
            // Start a task a return it
            return Task.Run(() =>
            {
                decimal result = 0;

                // Loop for a defined number of iterations
                for (int i = 0; i < loop; i++)
                {
                    // Do something that takes a long time (i.e. sleep) 
                    Thread.Sleep(10);
                    result += i;
                }
                return result;
            });
        }

        public async Task<decimal> LongRunningOperationWithCancellationTokenAsync(int loop,
                                                                                  CancellationToken cancellationToken)
        {
            // We create a TaskCompletionSource of decimal
            var taskCompletionSource = new TaskCompletionSource<decimal>();

            // Registering a lambda into the cancellationToken
            cancellationToken.Register(() =>
            {
                // We received a cancellation message, cancel the TaskCompletionSource.Task
                taskCompletionSource.TrySetCanceled();
            });

            var task = LongRunningOperation(loop);

            // Wait for the first task to finish among the two
            var completedTask = await Task.WhenAny(task, taskCompletionSource.Task).ConfigureAwait(false);

            // If the completed task is our long running operation we set its result.
            if (completedTask == task)
            {
                // Extract the result, the task is finished and the await will return immediately
                var result = await task;

                // Set the taskCompletionSource result
                taskCompletionSource.TrySetResult(result);
            }
            // Return the result of the TaskCompletionSource.Task
            return await taskCompletionSource.Task;
        }
    }
}