using System;
using System.IO;
using System.Linq;
using JapaneseCrossword;
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
			var expected = CellStateStringConverter.ConvertPictureToCellStateArray(solvedPath);

			var result = iteratedLineAnalysis.SolveCrossword(sourcePicture, lines);

			Assert.AreEqual(expected, result);
		}

		[Ignore(
			"These tests check subset of operations being tested by CrosswordSolverTests, but can be useful for individual testing of IteratedLineAnalysis.SolveCrossword"
			)]
		[TestCase(@"TestFiles\SampleInput.txt", @"TestFiles\SampleInput.solved.txt")]
		[TestCase(@"TestFiles\Car.txt", @"TestFiles\Car.solved.txt")]
		[TestCase(@"TestFiles\Flower.txt", @"TestFiles\Flower.solved.txt")]
		[TestCase(@"TestFiles\Winter.txt", @"TestFiles\Winter.solved.txt")]
		public void SolveCrossword_SimpleInput(string inputPath, string solvedPath)
		{
			Check(inputPath, solvedPath);
		}
	}
}