using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TPLBasics
{
	[TestFixture]
	class TPLBasicsTests
	{
		[Test]
		public void TestStatuses()
		{
			var task = new Task(() => { Console.WriteLine(" Task1 running..."); Thread.Sleep(1000); });
			Console.WriteLine("Task1 status: {0}", task.Status);
			task.Start();
			Console.WriteLine("Task1 status: {0}", task.Status);
			Thread.Sleep(500);
			Console.WriteLine("Task1 status: {0}", task.Status);
			task.Wait();
			Console.WriteLine("Task1 status: {0}", task.Status);
		}

		[Test]
		public void TestSynchronousRun()
		{
			var task = new Task(() => { Console.WriteLine("Task1 running..."); Thread.Sleep(1000); });
			Console.WriteLine("Task1 status: {0}", task.Status);
			task.RunSynchronously();
			Console.WriteLine("Task1 status: {0}", task.Status);
		}

		[Test]
		public void TestWaitAllTasks()
		{
			var firstTask = new Task(() =>
			{
				Console.WriteLine("Task 0 starting...");
				Thread.SpinWait(10000000);
				Console.WriteLine("Task 0 finishing...");
			});
			var secondTask = new Task(() =>
			{
				Console.WriteLine("Task 1 starting...");
				Thread.SpinWait(1000000000);
				Console.WriteLine("Task 1 finishing...");
			});

			firstTask.Start();
			secondTask.Start();
			var finishedTaskId = Task.WaitAny(firstTask, secondTask);
			Console.WriteLine("Task {0} finished", finishedTaskId);

			Task.WaitAll(firstTask, secondTask);
			Console.WriteLine("All tasks finished");
		}

		[Test]
		public void TestParent()
		{
			var parent = Task.Factory.StartNew(() =>
			{
				Console.WriteLine("Outer task executing.");
				Task.Factory.StartNew(() =>
				{
					Console.WriteLine("Nested task executing.");
					Thread.SpinWait(500000);
					Console.WriteLine("Nested task completing.");
				}, TaskCreationOptions.AttachedToParent);
			}/*, TaskCreationOptions.DenyChildAttach*/);

			parent.Wait();
			Console.WriteLine("Outer has completed.");
		}

		[Test]
		public void TestFuture()
		{
			Task<int> deepThoughtTask = Task.Run(() => 42);
			Console.WriteLine("Answer to the Ultimate Question of Life, the Universe, and Everything is {0}",
							  deepThoughtTask.Result);
		}

		[Test]
		public void TestContinueWith()
		{
			var tasksChain = Task.Run(() => Console.WriteLine("Asking Deep Thought..."))
								 .ContinueWith(previousTask => Thread.SpinWait(500000000))
								 .ContinueWith(previousTask => Console.WriteLine("Processing is done!"))
								 .ContinueWith(previousTask => 42)
								 .ContinueWith(
									 previousTask =>
									 Console.WriteLine(
										 "Answer to the Ultimate Question of Life, the Universe, and Everything is {0}",
										 previousTask.Result));

			tasksChain.Wait();
		}

		[Test]
		public void TestContinueWithException()
		{
			var tasksChain = Task.Run<int>(() =>
			{
				if(true) throw new Exception("haha!");
				else return 1;
			})
				.ContinueWith(previousTask =>
				{
					try
					{
						Console.WriteLine(previousTask.Result);
					}
					catch(Exception e)
					{
						Console.WriteLine(e);
					}
					finally
					{
						Console.WriteLine("finished second");
					}
				});
			tasksChain.Wait();
		}
	}
}
