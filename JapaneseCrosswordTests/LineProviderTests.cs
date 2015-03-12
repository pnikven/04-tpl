using System.Collections.Generic;
using JapaneseCrossword;
using NUnit.Framework;

namespace JapaneseCrosswordTests
{
	[TestFixture]
	public class LineProviderTests
	{
		[Test]
		public void GetLines()
		{
			var lineType = LineType.Row;
			ILineProvider lineProvider = new LineProvider();

			var lines = lineProvider.GetLines(lineType, new[] { new[] { 1, 2 }, new[] { 2 } });
			IEnumerable<ILine> expected = new[]
			{
				new Line(lineType, 0, new[] {1, 2}),
				new Line(lineType, 1, new[] {2})
			};

			Assert.AreEqual(expected, lines);
		}

	}
}