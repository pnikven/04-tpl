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
			var sourcePicture = new CellState[2, 2];
			var expected = ConvertPictureToCellStateArray(@"TestFiles\SampleInput.solved.txt");
			var crossword = createCrossword(File.ReadAllText(@"TestFiles\SampleInput.txt"));
			var lines = lineProvider.GetLines(crossword).ToArray();

			var result = iteratedLineAnalysis.SolveCrossword(sourcePicture, lines);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void SolveCrossword_Car()
		{
			var sourcePicture = new CellState[8, 10];
			var expected = ConvertPictureToCellStateArray(@"TestFiles\Car.solved.txt");
			var crossword = createCrossword(File.ReadAllText(@"TestFiles\Car.txt"));
			var lines = lineProvider.GetLines(crossword).ToArray();

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
				row.ToCharArray().ForEach(c => result[i, j++] = ConvertCharToCellState(c));
				j = 0;
				i++;
			});
			return result;
		}

		private CellState ConvertCharToCellState(char c)
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
					throw new Exception(string.Format("Unknown char {0} in picture", c));
			}
		}
	}
}