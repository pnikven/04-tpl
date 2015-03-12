using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace JapaneseCrossword
{
	[TestFixture]
	public class IteratedLineAnalysisTests
	{
		private ILineProvider lineProvider;
		private ILineAnalyzer lineAnalyzer;
		private ICrosswordSolverAlgorithm iteratedLineAnalysis;
		private Func<string, ICrossword> createCrossword;

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

		[Ignore]
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