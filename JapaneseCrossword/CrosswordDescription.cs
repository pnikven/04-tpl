using System;
using System.Collections.Generic;
using System.Linq;
using JapaneseCrossword.Interfaces;

namespace JapaneseCrossword
{
	public class CrosswordDescription : ICrosswordDescription
	{
		public int RowCount { get; private set; }
		public int ColumnCount { get; private set; }
		public IEnumerable<int[]> RowBlocks { get; private set; }
		public IEnumerable<int[]> ColumnBlocks { get; private set; }

		public CrosswordDescription(IEnumerable<int[]> rowBlocks, IEnumerable<int[]> columnBlocks,
			int rowCount, int columnCount)
		{
			RowBlocks = rowBlocks;
			ColumnBlocks = columnBlocks;
			RowCount = rowCount;
			ColumnCount = columnCount;
		}

		public bool IsCorrect
		{
			get
			{
				return
					BlockLengthSumsByRowsAndColumnsAreEqual() &&
					CanLinesAccomodateBlocks();
			}
		}

		public static CrosswordDescription Create(string crossword)
		{
			try
			{
				var strings = crossword.Split(new[] { '\n', '\r' },
					StringSplitOptions.RemoveEmptyEntries);

				var rowCount = GetValueAfterColon(strings.First());
				var rowBlocks = strings
					.Skip(1)
					.Take(rowCount)
					.Select(GetIntArrayFromString);

				var columnCount = GetValueAfterColon(strings.Skip(rowCount + 1).First());
				var columnBlocks = strings
					.Skip(rowCount + 2)
					.Take(columnCount)
					.Select(GetIntArrayFromString);

				return new CrosswordDescription(rowBlocks, columnBlocks, rowCount, columnCount);
			}
			catch
			{
				return null;
			}
		}

		private static int GetValueAfterColon(string s)
		{
			return int.Parse(s.Split(':')[1]);
		}

		private static int[] GetIntArrayFromString(string s)
		{
			return s
				.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(int.Parse)
				.ToArray();
		}

		private bool BlockLengthSumsByRowsAndColumnsAreEqual()
		{
			return GetSumOfBlockLengths(RowBlocks) == GetSumOfBlockLengths(ColumnBlocks);
		}

		private bool CanLinesAccomodateBlocks()
		{
			return CanLineAccommodateBlocks(ColumnCount, RowBlocks) &&
				CanLineAccommodateBlocks(RowCount, ColumnBlocks);
		}

		private bool CanLineAccommodateBlocks(int lineLength, IEnumerable<int[]> blocks)
		{
			return blocks
				.All(lineBlocks => GetEmptyCellsMinCount(lineBlocks) + lineBlocks.Sum() <= lineLength);
		}

		private int GetEmptyCellsMinCount(int[] lineBlocks)
		{
			return lineBlocks.Length - 1;
		}

		private int GetSumOfBlockLengths(IEnumerable<int[]> blockLengths)
		{
			return blockLengths.Sum(x => x.Sum());
		}
	}
}
