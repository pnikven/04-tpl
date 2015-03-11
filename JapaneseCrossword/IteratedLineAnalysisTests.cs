using System;
using System.IO;
using System.Linq;
using MoreLinq;
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

		[Test]
		public void SolveCrossword_SimpleInput()
		{
			var crossword = createCrossword(File.ReadAllText(@"TestFiles\SampleInput.txt"));
			var sourcePicture = new CellState[crossword.RowCount, crossword.ColumnCount];
			var lines = lineProvider.GetLines(crossword).ToArray();
			var expected = ConvertPictureToCellStateArray(@"TestFiles\SampleInput.solved.txt");

			var result = iteratedLineAnalysis.SolveCrossword(sourcePicture, lines);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void SolveCrossword_Car()
		{
			var crossword = createCrossword(File.ReadAllText(@"TestFiles\Car.txt"));
			var sourcePicture = new CellState[crossword.RowCount, crossword.ColumnCount];
			var lines = lineProvider.GetLines(crossword).ToArray();
			var expected = ConvertPictureToCellStateArray(@"TestFiles\Car.solved.txt");

			var result = iteratedLineAnalysis.SolveCrossword(sourcePicture, lines);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void SolveCrossword_Flower()
		{
			var crossword = createCrossword(File.ReadAllText(@"TestFiles\Flower.txt"));
			var sourcePicture = new CellState[crossword.RowCount, crossword.ColumnCount];
			var lines = lineProvider.GetLines(crossword).ToArray();
			var expected = ConvertPictureToCellStateArray(@"TestFiles\Flower.solved.txt");

			var result = iteratedLineAnalysis.SolveCrossword(sourcePicture, lines);

			Assert.AreEqual(expected, result);
		}

		private CellState[,] ConvertPictureToCellStateArray(string picturePath)
		{
			var rows = File.ReadAllLines(picturePath);
			var rowCount = rows.Length;
			var colCount = rows.First().Length;
			var i = 0;
			var j = 0;
			var result = new CellState[rowCount, colCount];
			rows.ForEach(row =>
			{
				row.ToCharArray().ForEach(c => result[i, j++] = CellStateStringConverter.ConvertCharToCellState(c));
				j = 0;
				i++;
			});
			return result;
		}

	}
}