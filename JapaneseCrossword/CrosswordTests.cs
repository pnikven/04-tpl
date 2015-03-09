using NUnit.Framework;

namespace JapaneseCrossword
{
	[TestFixture]
	class CrosswordTests
	{
		[Test]
		public void Crossword_SampleInput_CreatedCorrectly()
		{
			var input = "rows: 2  \n 2 \n 1 \n columns: 2 \n 1 \n 2";

			var crossword = new Crossword(input);

			Assert.AreEqual(2, crossword.RowCount);
			Assert.AreEqual(2, crossword.ColumnCount);
			Assert.AreEqual(new[] { new[] { 2 }, new[] { 1 } }, crossword.RowBlocks);
			Assert.AreEqual(new[] { new[] { 1 }, new[] { 2 } }, crossword.ColumnBlocks);
		}

		[TestCase("rows: 2  \n 2 \n 1 \n columns: 2 \n 1 \n 2", true)]
		[TestCase("rows: 2 \n 2 \n 2 \n columns: 2 \n 1 \n 1 ", false)]
		public void BlockLengthSumsByRowsAndColumnsAreEqual(string input, bool isCorrect)
		{
			var crossword = new Crossword(input);

			Assert.AreEqual(isCorrect, crossword.IsCorrect);
		}

		//[TestCase("rows: 2  \n 3 \n 1 \n columns: 3 \n 1 \n 2 \n 1", true)]
		//[TestCase("rows: 2  \n 3 \n 1 \n columns: 2 \n 2 \n 2", false)]
		//[TestCase("rows: 2  \n 1 2 \n 1 \n columns: 3 \n 1 \n 2 \n 1", false)]
		//TODO test that rows and columns can accomodate all blocks

	}
}
