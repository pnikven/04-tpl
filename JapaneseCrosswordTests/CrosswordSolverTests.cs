using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JapaneseCrossword.Enums;
using JapaneseCrossword.Solvers;
using JapaneseCrossword.Solvers.Algoritms;
using JapaneseCrossword.Solvers.Algoritms.Interfaces;
using JapaneseCrossword.Solvers.Algoritms.Utils;
using JapaneseCrossword.Solvers.Interfaces;
using MoreLinq;
using NUnit.Framework;

namespace JapaneseCrosswordTests
{
	public enum CrosswordSolverType
	{
		SingleThreaded,
		MultiThreaded
	}

	[TestFixture]
	public class CrosswordSolverTests
	{
		private ICrosswordSolver singleThreadedSolver;
		private ICrosswordSolver multiThreadedSolver;

		private ICrosswordSolver GetSolver(CrosswordSolverType solverType)
		{
			switch (solverType)
			{
				case CrosswordSolverType.SingleThreaded:
					return singleThreadedSolver;
				case CrosswordSolverType.MultiThreaded:
					return multiThreadedSolver;
				default:
					throw new ArgumentOutOfRangeException("solverType");
			}
		}

		[TestFixtureSetUp]
		public void SetUp()
		{
			var lineAnalyzer = new LineAnalyzer();

			ICrosswordSolverAlgorithm singleThreadedSolverAlgorithm = new IteratedLineAnalysis(lineAnalyzer);
			singleThreadedSolver = new CrosswordSolver(singleThreadedSolverAlgorithm);

			ICrosswordSolverAlgorithm multiThreadedSolverAlgorithm = new MultiThreadedIteratedLineAnalysis(lineAnalyzer);
			multiThreadedSolver = new CrosswordSolver(multiThreadedSolverAlgorithm);
		}

		[TestCase(CrosswordSolverType.SingleThreaded)]
		[TestCase(CrosswordSolverType.MultiThreaded)]
		public void InputFileNotFound(CrosswordSolverType solverType)
		{
			var solver = GetSolver(solverType);
			var solutionStatus = solver.Solve(Path.GetRandomFileName(), Path.GetRandomFileName());
			Assert.AreEqual(SolutionStatus.BadInputFilePath, solutionStatus);
		}

		[TestCase(CrosswordSolverType.SingleThreaded)]
		[TestCase(CrosswordSolverType.MultiThreaded)]
		public void IncorrectOutputFile(CrosswordSolverType solverType)
		{
			var solver = GetSolver(solverType);
			var inputFilePath = @"TestFiles\SampleInput.txt";
			var outputFilePath = "///.&*#";
			var solutionStatus = solver.Solve(inputFilePath, outputFilePath);
			Assert.AreEqual(SolutionStatus.BadOutputFilePath, solutionStatus);
		}

		[TestCase(CrosswordSolverType.SingleThreaded)]
		[TestCase(CrosswordSolverType.MultiThreaded)]
		public void IncorrectCrossword(CrosswordSolverType solverType)
		{
			var solver = GetSolver(solverType);
			var inputFilePath = @"TestFiles\IncorrectCrossword.txt";
			var outputFilePath = Path.GetRandomFileName();
			var solutionStatus = solver.Solve(inputFilePath, outputFilePath);
			Assert.AreEqual(SolutionStatus.IncorrectCrossword, solutionStatus);
		}

		[TestCase(CrosswordSolverType.SingleThreaded)]
		[TestCase(CrosswordSolverType.MultiThreaded)]
		public void Simplest(CrosswordSolverType solverType)
		{
			Check(solverType, @"TestFiles\SampleInput.txt", @"TestFiles\SampleInput.solved.txt",
				SolutionStatus.Solved);
		}

		[TestCase(CrosswordSolverType.SingleThreaded)]
		[TestCase(CrosswordSolverType.MultiThreaded)]
		public void Car(CrosswordSolverType solverType)
		{
			Check(solverType, @"TestFiles\Car.txt", @"TestFiles\Car.solved.txt",
				SolutionStatus.Solved);
		}

		[TestCase(CrosswordSolverType.SingleThreaded)]
		[TestCase(CrosswordSolverType.MultiThreaded)]
		public void Flower(CrosswordSolverType solverType)
		{
			Check(solverType, @"TestFiles\Flower.txt", @"TestFiles\Flower.solved.txt",
				SolutionStatus.Solved);
		}

		[TestCase(CrosswordSolverType.SingleThreaded)]
		[TestCase(CrosswordSolverType.MultiThreaded)]
		public void Winter(CrosswordSolverType solverType)
		{
			Check(solverType, @"TestFiles\Winter.txt", @"TestFiles\Winter.solved.txt",
				SolutionStatus.PartiallySolved);
		}

		private void Check(CrosswordSolverType solverType, string inputPath, string correctOutputPath,
			SolutionStatus expectedSolutionStatus)
		{
			var solver = GetSolver(solverType);
			var outputFilePath = Path.GetRandomFileName();
			var solutionStatus = solver.Solve(inputPath, outputFilePath);
			Assert.AreEqual(expectedSolutionStatus, solutionStatus);
			CollectionAssert.AreEqual(File.ReadAllText(correctOutputPath), File.ReadAllText(outputFilePath));
		}

		[Ignore("This test only demonstrates speed of each algorithm and not intended for testing program correctness")]
		[Test]
		public void SpeedTest()
		{
			var testsDir = "TestFiles";
			var testFiles = new[] { "SampleInput.txt", "Car.txt", "Flower.txt", "Winter.txt" };
			var solverTypes = new[] { CrosswordSolverType.SingleThreaded, CrosswordSolverType.MultiThreaded };
			var stringFormat = "{0,15}{1,15}{2,15}{3,15}";
			Console.WriteLine(stringFormat, "TestFile", "SingleThreaded", "MultiThreaded", "SpeedUp");
			testFiles
				.Cartesian<string, CrosswordSolverType, Tuple<string, CrosswordSolverType>>(solverTypes, Tuple.Create)
				.Batch(solverTypes.Length)
				.ForEach(solvers =>
				{
					var s = solvers.ToArray();
					var elapsedTimes = s
						.Select(pair =>
						{
							var stopwatch = new Stopwatch();
							stopwatch.Start();
							GetSolver(pair.Item2)
								.Solve(Path.Combine(testsDir, pair.Item1), Path.GetRandomFileName());
							stopwatch.Stop();
							return stopwatch.ElapsedMilliseconds;
						})
						.ToArray();
					Console.WriteLine(stringFormat, s.First().Item1,
						elapsedTimes[0], elapsedTimes[1], elapsedTimes[0] / elapsedTimes[1]);
				});
		}
	}
}