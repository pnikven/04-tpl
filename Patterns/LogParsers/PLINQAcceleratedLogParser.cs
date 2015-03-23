using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Patterns.LogParsers
{
    public class PLINQAcceleratedLogParser : ILogParser
    {
        public KeyValuePair<string, int>[] GetTop10Users(string logPath)
        {
            var usersStats = new ConcurrentDictionary<string, int>();

            File.ReadLines(logPath).AsParallel()
                .Select(IpInfo.Parse)
                .ForAll(ipInfo =>
                        {
                            usersStats.AddOrUpdate(ipInfo.Ip, key => ipInfo.CallDuration,
                                (key, oldDuration) => oldDuration + ipInfo.CallDuration);
                        });

            return usersStats.AsParallel()
                .OrderByDescending(keyValuePair => keyValuePair.Value).Take(10).ToArray();
        }
    }
}