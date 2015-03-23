using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Patterns
{
    [TestFixture]
    public class LongRunningTasks
    {
        private int N = Environment.ProcessorCount;
        private const int MillisecondsToSleep = 10000;

        [Test]
        public void ExcessiveThreads()
        {
            var tasks = new Task[N];
            for (int i = 0; i < N; i++)
                tasks[i] = CreateLongRunningCPUIntensiveTask(MillisecondsToSleep);

            Task.WaitAll(tasks);
        }

        [Test]
        public void NoExcessiveThreads()
        {
            var processorCount = Environment.ProcessorCount;
            var tasks = new List<Task>(processorCount);
            var finishedTasksCount = 0;

            for (int i = 0; i < processorCount; i++)
                tasks.Add(CreateLongRunningCPUIntensiveTask(MillisecondsToSleep));

            do
            {
                var finishedTaskId = Task.WaitAny(tasks.ToArray());
                finishedTasksCount++;
                tasks.RemoveAt(finishedTaskId);
                tasks.Add(CreateLongRunningCPUIntensiveTask(MillisecondsToSleep));
            } while (finishedTasksCount < N);
        }
        
        [Test]
        public void LoggingTask()
        {
            var sw = Stopwatch.StartNew();
            var loggingTasks = new Task[N];
            for (int i = 0; i < N; i++)
                loggingTasks[i] = CreateLongRunningLoggingTask(MillisecondsToSleep);
            var usefulTasks = new Task[N];
            for (int i = 0; i < N; i++)
                usefulTasks[i] = Task.Run(() =>
                                          {
                                              var sum = 0;
                                              for (int j = 0; j < int.MaxValue; j++)
                                                  sum += j;
                                              Console.WriteLine(sum);
                                          });

            Task.WaitAll(usefulTasks);
            Console.WriteLine("Finished in {0} ms", sw.ElapsedMilliseconds);
        }

        private Task CreateLongRunningCPUIntensiveTask(int msToSleep)
        {
            return Task.Factory.StartNew(() =>
                                         {
                                             var sw = Stopwatch.StartNew();
                                             while (sw.ElapsedMilliseconds < msToSleep)
                                                 Thread.SpinWait(1000000);
                                         });
        }

        private Task CreateLongRunningLoggingTask(int msToSleep)
        {
            return Task.Factory.StartNew(() =>
                                         {
                                             while (true)
                                             {
                                                 Thread.Sleep(msToSleep);
                                                 Console.WriteLine("Threads count: {0}", Process.GetCurrentProcess().Threads.Count);
                                             }
                                         });
        }
    }
}
