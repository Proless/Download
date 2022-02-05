#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Download.Core
{
    public static class DownloadTimeout
    {
        public static async Task WaitAsync(Func<CancellationToken, Task> awaitableOperation, Func<Task> timeoutCallback, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (timeout == TimeSpan.Zero)
            {
                await timeoutCallback();
                return;
            }

            if (timeout == Timeout.InfiniteTimeSpan)
            {
                await awaitableOperation(cancellationToken);
                return;
            }

            using var timeoutCts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var awaitableTask = awaitableOperation(linkedCts.Token);
            var timeoutTask = Task.Delay(timeout, timeoutCts.Token);

            var completedTask = await Task.WhenAny(awaitableTask, timeoutTask).ConfigureAwait(false);
            timeoutCts.Cancel();
            if (completedTask == awaitableTask)
            {
                await awaitableTask;
            }
            else
            {
                await timeoutCallback();
            }
        }
        public static async Task<TResult> WaitAsync<TResult>(Func<CancellationToken, Task<TResult>> awaitableOperation, Func<Task<TResult>> timeoutCallback, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (timeout == TimeSpan.Zero)
            {
                return await timeoutCallback();
            }

            if (timeout == Timeout.InfiniteTimeSpan)
            {
                return await awaitableOperation(cancellationToken);
            }

            using var timeoutCts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var awaitableTask = awaitableOperation(linkedCts.Token);
            var timeoutTask = Task.Delay(timeout, timeoutCts.Token);

            var completedTask = await Task.WhenAny(awaitableTask, timeoutTask).ConfigureAwait(false);
            timeoutCts.Cancel();
            if (completedTask == awaitableTask)
            {
                return await awaitableTask;
            }
            else
            {
                return await timeoutCallback();
            }
        }
    }
}
