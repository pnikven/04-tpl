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

		[TestFixtureSetUp]
		public void SetUp()
		{
			lineProvider = new LineProvider();
			lineAnalyzer=new LineAnalyzer();
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
	}
}