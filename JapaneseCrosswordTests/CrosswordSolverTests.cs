using System;
using System.IO;
using JapaneseCrossword.Enums;
using JapaneseCrossword.Solvers;
using JapaneseCrossword.Solvers.Algoritms;
using JapaneseCrossword.Solvers.Algoritms.Interfaces;
using JapaneseCrossword.Solvers.Algoritms.Utils;
using JapaneseCrossword.Solvers.Interfaces;
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

		[TestCase(CrosswordSolverType.SingleThreaded)]
		[TestCase(CrosswordSolverType.MultiThreaded)]
		public void Japanese(CrosswordSolverType solverType)
		{
			Check(solverType, @"TestFiles\Japanese.txt", @"TestFiles\Japanese.solved.txt",
				SolutionStatus.Solved);
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
	}
}