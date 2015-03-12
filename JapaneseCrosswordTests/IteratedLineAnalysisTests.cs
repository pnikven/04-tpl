using System;
using System.IO;
using System.Linq;
using JapaneseCrossword;
using JapaneseCrossword.Enums;
using JapaneseCrossword.Extensions;
using JapaneseCrossword.Interfaces;
using JapaneseCrossword.Solvers.Algoritms;
using JapaneseCrossword.Solvers.Algoritms.Interfaces;
using JapaneseCrossword.Solvers.Algoritms.Utils;
using JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces;
using JapaneseCrossword.Solvers.Utils;
using JapaneseCrossword.Solvers.Utils.Interfaces;
using NUnit.Framework;

namespace JapaneseCrosswordTests
{
	[TestFixture]
	public class IteratedLineAnalysisTests
	{
		private Func<string, ICrossword> createCrossword;
		private ICrosswordSolverAlgorithm iteratedLineAnalysis;
		private ILineAnalyzer lineAnalyzer;
		private ILineProvider lineProvider;

		[TestFixtureSetUp]
		public void SetUp()
		{
			lineProvider = new LineProvider();
			lineAnalyzer = new LineAnalyzer();
			iteratedLineAnalysis = new IteratedLineAnalysis(lineAnalyzer);
			createCrossword = crosswordAsPlainText => new Crossword(crosswordAsPlainText);
		}

		private void Check(string inputPath, string solvedPath)
		{
			var crossword = createCrossword(File.ReadAllText(inputPath));
			var sourcePicture = new CellState[crossword.RowCount, crossword.ColumnCount];
			var lines = lineProvider.GetLines(crossword).ToArray();
			var solvedCrossword = File.ReadAllText(solvedPath);
			var expected = solvedCrossword.ToCellStateMatrix();

			var result = iteratedLineAnalysis.SolveCrossword(sourcePicture, lines);

			Assert.AreEqual(expected, result);
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