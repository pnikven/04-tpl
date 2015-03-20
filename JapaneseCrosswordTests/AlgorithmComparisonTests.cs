using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JapaneseCrossword;
using JapaneseCrossword.Solvers.Algoritms;
using JapaneseCrossword.Solvers.Algoritms.Interfaces;
using JapaneseCrossword.Solvers.Algoritms.Utils;
using MoreLinq;
using NUnit.Framework;

namespace JapaneseCrosswordTests
{
	enum CrosswordSolverAlgorithmType
	{
		SingleThreaded,
		MultiThreaded
	}

	[TestFixture]
	public class AlgorithmComparisonTests
	{
		private ICrosswordSolverAlgorithm singleThreadedSolverAlgorithm;
		private ICrosswordSolverAlgorithm multiThreadedSolverAlgorithm;

		[TestFixtureSetUp]
		public void SetUp()
		{
			var lineAnalyzer = new LineAnalyzer();

			singleThreadedSolverAlgorithm = new IteratedLineAnalysis(lineAnalyzer);
			multiThreadedSolverAlgorithm = new MultiThreadedIteratedLineAnalysis(lineAnalyzer);
		}

		private ICrosswordSolverAlgorithm GetAlgorithm(CrosswordSolverAlgorithmType algorithmType)
		{
			switch (algorithmType)
			{
				case CrosswordSolverAlgorithmType.SingleThreaded:
					return singleThreadedSolverAlgorithm;
				case CrosswordSolverAlgorithmType.MultiThreaded:
					return multiThreadedSolverAlgorithm;
				default:
					throw new ArgumentOutOfRangeException("algorithmType");
			}
		}

		[Ignore("This test only demonstrates speed of each algorithm and not intended for testing program correctness")]
		[Test]
		public void SpeedTest()
		{
			var testsDir = "TestFiles";
			var testFiles = new[] { "SampleInput.txt", "Car.txt", "Flower.txt", "Winter.txt", "SuperBig.txt", "Japanese.txt" };
			var algorythmTypes = new[] { CrosswordSolverAlgorithmType.SingleThreaded, CrosswordSolverAlgorithmType.MultiThreaded };
			var stringFormat = "{0,15}{1,15}{2,15}{3,15}";
			Console.WriteLine(stringFormat, "TestFile", "SingleThreaded", "MultiThreaded", "SpeedUp");
			testFiles
				.Cartesian(algorythmTypes, Tuple.Create)
				.Batch(algorythmTypes.Length)
				.ForEach(solvers =>
				{
					var s = solvers.ToArray();
					var elapsedTimes = s
						.Select(pair =>
						{
							var crossword = File.ReadAllText(Path.Combine(testsDir, pair.Item1), Encoding.UTF8);
							var crosswordDescription = CrosswordDescription.Create(crossword);
							var stopwatch = new Stopwatch();
							stopwatch.Start();
							GetAlgorithm(pair.Item2)
								.SolveCrossword(crosswordDescription);
							stopwatch.Stop();
							return stopwatch.ElapsedMilliseconds;
						})
						.ToArray();
					stringFormat = "{0,15}{1,15}{2,15}{3,15:0.0}";
					Console.WriteLine(stringFormat, s.First().Item1,
						elapsedTimes[0], elapsedTimes[1], (double)elapsedTimes[0] / elapsedTimes[1]);
				});
		}
	}
}