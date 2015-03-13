using System.Linq;
using JapaneseCrossword.Enums;
using JapaneseCrossword.Solvers.Algoritms.Utils;
using JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces;
using JapaneseCrossword.Solvers.Utils;
using JapaneseCrossword.Solvers.Utils.Enums;
using MoreLinq;
using NUnit.Framework;

namespace JapaneseCrosswordTests
{
	[TestFixture]
	class LineAnalyzerTests
	{
		private ILineAnalyzer lineAnalyzer;

		[TestFixtureSetUp]
		public void SetUp()
		{
			lineAnalyzer = new LineAnalyzer();
		}

		[Test]
		public void Analyze_OneBlock()
		{
			var line = Line.Create(LineType.Row, 1, new[] { 2 });
			var cells = Cell.Create("???");
			var canBeFilled = new[] { true, true, true };
			var canBeEmpty = new[] { true, false, true };
			var expected = new LineAnalysisResult(canBeFilled, canBeEmpty);

			var result = lineAnalyzer.Analyze(line, cells);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void Analyze_TwoBlocks()
		{
			var line = Line.Create(LineType.Row, 1, new[] { 1, 2 });
			var cells = Enumerable.Repeat(new Cell(), 5).ToArray();
			var canBeFilled = new[] { true, true, true, true, true };
			var canBeEmpty = new[] { true, true, true, false, true };
			var expected = new LineAnalysisResult(canBeFilled, canBeEmpty);

			var result = lineAnalyzer.Analyze(line, cells);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void Analyze_ThreeBlocks()
		{
			var line = Line.Create(LineType.Row, 1, new[] { 2, 1, 3 });
			var cells = Enumerable.Repeat(new Cell(), 11).ToArray();
			var canBeFilled = Enumerable.Repeat(true, 11).ToArray();
			var canBeEmpty = canBeFilled;
			var expected = new LineAnalysisResult(canBeFilled, canBeEmpty);

			var result = lineAnalyzer.Analyze(line, cells);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void Analyze_ThreeBlocksShorterLine()
		{
			var line = Line.Create(LineType.Row, 1, new[] { 2, 1, 3 });
			var cells = Enumerable.Repeat(new Cell(), 9).ToArray();
			var canBeFilled = Enumerable.Repeat(true, 9).ToArray();
			var canBeEmpty = new[] { true, false, true, true, true, true, false, false, true };
			var expected = new LineAnalysisResult(canBeFilled, canBeEmpty);

			var result = lineAnalyzer.Analyze(line, cells);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void Analyze_OneBlockWithFirstFilledCell()
		{
			var line = Line.Create(LineType.Row, 1, new[] { 2 });
			var cells = Cell.Create("*??");
			var canBeFilled = new[] { true, true, false };
			var canBeEmpty = new[] { false, false, true };
			var expected = new LineAnalysisResult(canBeFilled, canBeEmpty);

			var result = lineAnalyzer.Analyze(line, cells);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void Analyze_OneBlockWithLastFilledCell()
		{
			var line = Line.Create(LineType.Row, 1, new[] { 2 });
			var cells = Cell.Create("???*");
			var canBeFilled = new[] { false, false, true, true };
			var canBeEmpty = new[] { true, true, false, false };
			var expected = new LineAnalysisResult(canBeFilled, canBeEmpty);

			var result = lineAnalyzer.Analyze(line, cells);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void Analyze_OneBlockWithFirstEmptyCell()
		{
			var line = Line.Create(LineType.Row, 1, new[] { 2 });
			var cells = Cell.Create(".??");
			var canBeFilled = new[] { false, true, true };
			var canBeEmpty = new[] { true, false, false };
			var expected = new LineAnalysisResult(canBeFilled, canBeEmpty);

			var result = lineAnalyzer.Analyze(line, cells);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void Analyze_OneBlockWithFirstEmptyCellInLongerLine()
		{
			var line = Line.Create(LineType.Row, 1, new[] { 2 });
			var cells = Cell.Create(".???");
			var canBeFilled = new[] { false, true, true, true };
			var canBeEmpty = new[] { true, true, false, true };
			var expected = new LineAnalysisResult(canBeFilled, canBeEmpty);

			var result = lineAnalyzer.Analyze(line, cells);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void Analyze_Row23FromFlower()
		{
			var line = Line.Create(LineType.Row, 23, new[] { 10, 1, 4, 1, 1, 3 });
			var cells = ConvertStringToCells("???********???*..?***??*.*.***");
			var expected = ".??********??.*..?***?.*.*.***";

			var analysisResult = lineAnalyzer.Analyze(line, cells);
			UpdateCells(cells, analysisResult);
			var result = ConvertCellsToString(cells);

			Assert.AreEqual(expected, result);
		}

		public static string ConvertCellsToString(Cell[] cells)
		{
			return cells.Select(cell => cell.ToChar()).ToDelimitedString("");
		}

		private Cell[] ConvertStringToCells(string s)
		{
			return s
				.Select(Cell.Create)
				.ToArray();
		}

		[Test]
		public void Analyze_Row23TruncatedFromFlower()
		{
			var line = Line.Create(LineType.Row, 23, new[] { 1, 1, 3 });
			var cells = ConvertStringToCells("??*.*.***");
			var expected = "..*.*.***";

			var analysisResult = lineAnalyzer.Analyze(line, cells);
			UpdateCells(cells, analysisResult);
			var result = ConvertCellsToString(cells);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void Analyze_ObviousRow()
		{
			var line = Line.Create(LineType.Row, 23, new[] { 1, 1 });
			var cells = ConvertStringToCells("?*?*");
			var expected = ".*.*";

			var analysisResult = lineAnalyzer.Analyze(line, cells);
			UpdateCells(cells, analysisResult);
			var result = ConvertCellsToString(cells);

			Assert.AreEqual(expected, result);
		}

		private void UpdateCells(Cell[] cells, ILineAnalysisResult analysisResult)
		{
			Enumerable.Range(0, cells.Length)
				.Where(i => cells[i].IsUnknown && analysisResult.Cells[i].IsKnown)
				.ForEach(i => cells[i] = analysisResult.Cells[i]);
		}

	}
}