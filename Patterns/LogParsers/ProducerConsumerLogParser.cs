using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Patterns.LogParsers
{
    public class ProducerConsumerLogParser : ILogParser
    {
        public KeyValuePair<string, int>[] GetTop10Users(string logPath)
        {
            var queue = new BlockingCollection<string>(10000);
            var processorCount = Environment.ProcessorCount;
            var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

            taskFactory.StartNew(() =>
                                 {
                                     foreach (var line in File.ReadLines(logPath))
                                         queue.Add(line);

                                     queue.CompleteAdding();
                                 });

            var consumers = new Task<Dictionary<string, int>>[processorCount];
            for (int i = 0; i < consumers.Length; i++)
            {
                consumers[i] = taskFactory.StartNew(() =>
                                                    {
                                                        var localDictionary = new Dictionary<string, int>();

                                                        while (!queue.IsCompleted)
                                                        {
                                                            try
                                                            {
                                                                var line = queue.Take();
                                                                var ipInfo = IpInfo.Parse(line);

                                                                if (!localDictionary.ContainsKey(ipInfo.Ip))
                                                                    localDictionary.Add(ipInfo.Ip, ipInfo.CallDuration);
                                                                else
                                                                    localDictionary[ipInfo.Ip] += ipInfo.CallDuration;
                                                            }
                                                            catch (ObjectDisposedException)
                                                            {
                                                            }
                                                            catch (InvalidOperationException)
                                                            {
                                                            }
                                                        }

                                                        return localDictionary;
                                                    });
            }

            var summarizedDictionary = new Dictionary<string, int>();
            foreach (var dictionary in Task.WhenAll(consumers).Result)
                foreach (var key in dictionary.Keys)
                {
                    if (!summarizedDictionary.ContainsKey(key))
                        summarizedDictionary.Add(key, dictionary[key]);
                    else
                        summarizedDictionary[key] += dictionary[key];
                }

            return summarizedDictionary.OrderByDescending(keyValuePair => keyValuePair.Value).Take(10).ToArray();
        }
    }
}