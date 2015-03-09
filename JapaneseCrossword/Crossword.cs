using System;
using System.Collections.Generic;
using System.Linq;

namespace JapaneseCrossword
{
	class Crossword : ICrossword
	{
		public int RowCount { get; private set; }
		public int ColumnCount { get; private set; }
		public IEnumerable<int[]> RowBlocks { get; private set; }
		public IEnumerable<int[]> ColumnBlocks { get; private set; }

		public Crossword(string crosswordAsPlainText)
		{
			var strings = crosswordAsPlainText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			RowCount = GetValueAfterColon(strings.First());
			RowBlocks = strings
				.Skip(1)
				.Take(RowCount)
				.Select(GetIntArrayFromString);

			ColumnCount = GetValueAfterColon(strings.Skip(RowCount + 1).First());
			ColumnBlocks = strings
				.Skip(RowCount + 2)
				.Take(ColumnCount)
				.Select(GetIntArrayFromString);
		}

		public bool IsCorrect
		{
			get { return BlockLengthSumsByRowsAndColumnsAreEqual(); }
		}

		private bool BlockLengthSumsByRowsAndColumnsAreEqual()
		{
			return GetSumOfBlockLengths(RowBlocks)==GetSumOfBlockLengths(ColumnBlocks);
		}

		private int GetSumOfBlockLengths(IEnumerable<int[]> blockLengths)
		{
			return blockLengths.Sum(x => x.Sum());
		}

		private int GetValueAfterColon(string s)
		{
			return int.Parse(s.Split(':')[1]);
		}

		private int[] GetIntArrayFromString(string s)
		{
			return s
				.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(int.Parse)
				.ToArray();
		}
	}
}
