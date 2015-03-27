using System;
using System.Threading;
using System.Threading.Tasks;

namespace Balancer
{
	public static class TaskExtensions
	{
		public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int timeout)
		{
			var timeoutCancellationTokenSource = new CancellationTokenSource();
			var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
			if (completedTask != task) throw new TimeoutException("The operation has timed out.");
			timeoutCancellationTokenSource.Cancel();
			return await task;
		}
	}
}