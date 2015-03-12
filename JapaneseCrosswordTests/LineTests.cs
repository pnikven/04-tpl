using JapaneseCrossword.Solvers.Utils;
using JapaneseCrossword.Solvers.Utils.Enums;
using JapaneseCrossword.Solvers.Utils.Interfaces;
using NUnit.Framework;

namespace JapaneseCrosswordTests
{
	[TestFixture]
	public class LineTests
	{
		[Test]
		public void Constructor_ProjectsArrayOfIntsToSequenceOfBlocksCorrectly()
		{
			ILine line = new Line(LineType.Row, 0, new[] { 1, 2 });

			Assert.AreEqual(new[] { new Block(1, 0), new Block(2, 1) }, line.Blocks);
		}

	}
}