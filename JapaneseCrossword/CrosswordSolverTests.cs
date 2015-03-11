using System;
using System.IO;
using NUnit.Framework;

namespace JapaneseCrossword
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
			var lineProvider = new LineProvider();
			var lineAnalyzer = new LineAnalyzer();

			ICrosswordSolverAlgorithm singleThreadedSolverAlgorithm = new IteratedLineAnalysis(lineAnalyzer);
			singleThreadedSolver = new CrosswordSolver(
				crosswordAsPlainText => new Crossword(crosswordAsPlainText),
				lineProvider.GetLines,
				(picture, lines) => singleThreadedSolverAlgorithm.SolveCrossword(picture, lines),
				CellStateStringConverter.ConvertPictureToString
			);

			ICrosswordSolverAlgorithm multiThreadedSolverAlgorithm = new MultiThreadedIteratedLineAnalysis();
			multiThreadedSolver = new CrosswordSolver(
				crosswordAsPlainText => new Crossword(crosswordAsPlainText),
				lineProvider.GetLines,
				(picture, lines) => multiThreadedSolverAlgorithm.SolveCrossword(picture, lines),
				CellStateStringConverter.ConvertPictureToString
			);
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
			var solver = GetSolver(solverType);
			var inputFilePath = @"TestFiles\SampleInput.txt";
			var outputFilePath = Path.GetRandomFileName();
			var correctOutputFilePath = @"TestFiles\SampleInput.solved.txt";
			var solutionStatus = solver.Solve(inputFilePath, outputFilePath);
			Assert.AreEqual(SolutionStatus.Solved, solutionStatus);
			CollectionAssert.AreEqual(File.ReadAllText(correctOutputFilePath), File.ReadAllText(outputFilePath));
		}

		[TestCase(CrosswordSolverType.SingleThreaded)]
		[TestCase(CrosswordSolverType.MultiThreaded)]
		public void Car(CrosswordSolverType solverType)
		{
			var solver = GetSolver(solverType);
			var inputFilePath = @"TestFiles\Car.txt";
			var outputFilePath = Path.GetRandomFileName();
			var correctOutputFilePath = @"TestFiles\Car.solved.txt";
			var solutionStatus = solver.Solve(inputFilePath, outputFilePath);
			Assert.AreEqual(SolutionStatus.Solved, solutionStatus);
			CollectionAssert.AreEqual(File.ReadAllText(correctOutputFilePath), File.ReadAllText(outputFilePath));
		}

		[TestCase(CrosswordSolverType.SingleThreaded)]
		[TestCase(CrosswordSolverType.MultiThreaded)]
		public void Flower(CrosswordSolverType solverType)
		{
			var solver = GetSolver(solverType);
			var inputFilePath = @"TestFiles\Flower.txt";
			var outputFilePath = Path.GetRandomFileName();
			var correctOutputFilePath = @"TestFiles\Flower.solved.txt";
			var solutionStatus = solver.Solve(inputFilePath, outputFilePath);
			Assert.AreEqual(SolutionStatus.Solved, solutionStatus);
			CollectionAssert.AreEqual(File.ReadAllText(correctOutputFilePath), File.ReadAllText(outputFilePath));
		}

		[TestCase(CrosswordSolverType.SingleThreaded)]
		[TestCase(CrosswordSolverType.MultiThreaded)]
		public void Winter(CrosswordSolverType solverType)
		{
			var solver = GetSolver(solverType);
			var inputFilePath = @"TestFiles\Winter.txt";
			var outputFilePath = Path.GetRandomFileName();
			var correctOutputFilePath = @"TestFiles\Winter.solved.txt";
			var solutionStatus = solver.Solve(inputFilePath, outputFilePath);
			Assert.AreEqual(SolutionStatus.PartiallySolved, solutionStatus);
			CollectionAssert.AreEqual(File.ReadAllText(correctOutputFilePath), File.ReadAllText(outputFilePath));
		}
	}
}