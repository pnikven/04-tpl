using System.Collections.Generic;
using NUnit.Framework;
using Patterns.LogParsers;

namespace Patterns
{
    [TestFixture]
    public class LogsParsing
    {
        [Test]
        public void SingleThread()
        {
            Test(new SingleThreadLogParser());
        }

        [Test]
        public void SingleThreadConcurrentDictionary()
        {
            Test(new SingleThreadConcurrentDictionaryLogParser());
        }

        [Test]
        public void ParallelFor()
        {
            Test(new ParallelForLogParser());
        }

        [Test]
        public void ProducerConsumer()
        {
            Test(new ProducerConsumerLogParser());
        }

        [Test]
        public void MapReduce()
        {
            Test(new MapReduceLogParser());
        }

        [Test]
        public void MapReduceUsingGlobalDictionary()
        {
            Test(new MapReduceUsingGlobalDictionaryLogParser());
        }

        [Test]
        public void LINQ()
        {
            Test(new LINQLogParser());
        }

        [Test]
        public void PLINQ()
        {
            Test(new PLINQLogParser());
        }

        [Test]
        public void PLINQAccelerated()
        {
            Test(new PLINQAcceleratedLogParser());
        }

        private static void Test(ILogParser parser)
        {
            var expectedResult = new Dictionary<string, int>
                                 {
                                     {"213.24.62.119", 40475776},
                                     {"91.208.121.254", 38198276},
                                     {"46.17.201.50", 27744726},
                                     {"46.17.203.253", 27313932},
                                     {"46.17.201.58", 24282986},
                                     {"195.234.190.100", 22713210},
                                     {"195.43.90.253", 18936533},
                                     {"195.130.216.202", 18237678},
                                     {"92.50.171.106", 15146850},
                                     {"194.190.140.73", 14623515}
                                 };
            const string logPath = "Files/ips.txt";
            var result = parser.GetTop10Users(logPath);
            
            CollectionAssert.AreEqual(expectedResult, result);
        }
    }
}