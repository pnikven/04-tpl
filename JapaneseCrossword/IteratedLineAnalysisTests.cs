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

		[TestFixtureSetUp]
		public void SetUp()
		{
			lineProvider = new LineProvider();
			lineAnalyzer = new LineAnalyzer();
			iteratedLineAnalysis = new IteratedLineAnalysis(lineAnalyzer);
		}

		[Test]
		public void SolveCrossword_SimpleInput()
		{
			var sourcePicture = new[,]
			{
				{CellState.Unknown, CellState.Unknown},
				{CellState.Unknown, CellState.Unknown}
			};
			var expected = new[,]
			{
				{CellState.Filled, CellState.Filled},
				{CellState.Empty, CellState.Filled}
			};
			var lines = lineProvider.GetLines(LineType.Row, new[] { new[] { 2 }, new[] { 1 } })
				.Concat(lineProvider.GetLines(LineType.Column, new[] { new[] { 1 }, new[] { 2 } }))
				.ToArray();

			var result = iteratedLineAnalysis.SolveCrossword(sourcePicture, lines);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void SolveCrossword_Car()
		{
			var sourcePicture = new CellState[8, 10];
			var expected = ConvertPictureToCellStateArray(@"TestFiles\Car.solved.txt");
			var lines = lineProvider.GetLines(LineType.Row,
				new[] { new[] { 6 }, new[] { 2, 1, 1 }, new[] { 1, 1, 1 }, new[] { 9 }, new[] { 9 }, new[] { 10 }, new[] { 2, 2 }, new[] { 2, 2 } })
				.Concat(lineProvider.GetLines(LineType.Column,
				new[] { new[] { 1 }, new[] { 3 }, new[] { 5 }, new[] { 7 }, new[] { 2, 3 }, new[] { 1, 3 }, new[] { 6 }, new[] { 1, 5 }, new[] { 1, 5 }, new[] { 6 } }))
				.ToArray();

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