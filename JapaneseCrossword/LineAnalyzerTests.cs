using NUnit.Framework;

namespace JapaneseCrossword
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
		public void Analyze_OneBlock_ReturnsCorrectResult()
		{
			var line = new Line(LineType.Row, 1, new[] { 2 });
			var cells = new[] { CellState.Unknown, CellState.Unknown, CellState.Unknown };
			var canBeFilled = new[] { true, true, true };
			var canBeEmpty = new[] { true, false, true };
			var expected = new LineAnalysisResult(canBeFilled, canBeEmpty);

			var result = lineAnalyzer.Analyze(line, cells);

			Assert.AreEqual(expected, result);
		}

		[Test]
		public void Analyze_TwoBlocks_ReturnsCorrectResult()
		{
			var line = new Line(LineType.Row, 1, new[] { 1, 2 });
			var cells = new CellState[5];
			var canBeFilled = new[] { true, true, true, true, true };
			var canBeEmpty = new[] { true, true, true, false, true };
			var expected = new LineAnalysisResult(canBeFilled, canBeEmpty);

			var result = lineAnalyzer.Analyze(line, cells);

			Assert.AreEqual(expected, result);
		}
	}
}