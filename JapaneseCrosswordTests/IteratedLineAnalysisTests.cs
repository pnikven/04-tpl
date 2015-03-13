using System;
using System.IO;
using System.Linq;
using JapaneseCrossword;
using JapaneseCrossword.Enums;
using JapaneseCrossword.Solvers.Algoritms;
using JapaneseCrossword.Solvers.Algoritms.Interfaces;
using JapaneseCrossword.Solvers.Algoritms.Utils;
using JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces;
using MoreLinq;
using NUnit.Framework;

namespace JapaneseCrosswordTests
{
	[TestFixture]
	public class IteratedLineAnalysisTests
	{
		private ICrosswordSolverAlgorithm iteratedLineAnalysis;
		private ILineAnalyzer lineAnalyzer;

		[TestFixtureSetUp]
		public void SetUp()
		{
			lineAnalyzer = new LineAnalyzer();
			iteratedLineAnalysis = new IteratedLineAnalysis(lineAnalyzer);
		}

		private void Check(string inputPath, string solvedPath)
		{
			var crossword = CrosswordDescription.Create(File.ReadAllText(inputPath));
			var solvedCrossword = File.ReadAllText(solvedPath);
			var expected = ConvertToPicture(solvedCrossword);

			var result = iteratedLineAnalysis.SolveCrossword(crossword);

			Assert.AreEqual(expected, result);
		}

		public Cell[,] ConvertToPicture(string crosswordAsPlainText)
		{
			var rows = crosswordAsPlainText
				.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			var rowCount = rows.Length;
			var colCount = rows.First().Length;
			var i = 0;
			var j = 0;
			var result = new Cell[rowCount, colCount];
			rows.ForEach(row =>
			{
				row.ToCharArray().ForEach(c => result[i, j++] = Cell.Create(c));
				j = 0;
				i++;
			});
			return result;
		}

		private CellState ConvertToCellState(char c)
		{
			switch (c)
			{
				case '.':
					return CellState.Empty;
				case '*':
					return CellState.Filled;
				case '?':
					return CellState.Unknown;
				default:
					throw new ArgumentOutOfRangeException(string.Format("Unknown char {0}", c));
			}
		}

		[TestCase(@"TestFiles\SampleInput.txt", @"TestFiles\SampleInput.solved.txt")]
		[TestCase(@"TestFiles\Car.txt", @"TestFiles\Car.solved.txt")]
		[TestCase(@"TestFiles\Flower.txt", @"TestFiles\Flower.solved.txt")]
		[TestCase(@"TestFiles\Winter.txt", @"TestFiles\Winter.solved.txt")]
		[Ignore(
			"These tests check subset of operations being tested by CrosswordSolverTests, but can be useful for individual testing of IteratedLineAnalysis.SolveCrossword"
			)]
		public void SolveCrossword_SimpleInput(string inputPath, string solvedPath)
		{
			Check(inputPath, solvedPath);
		}
	}
}