using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace JapaneseCrossword
{
	class LineAnalyzer : ILineAnalyzer
	{
		public ILineAnalysisResult Analyze(ILine line, CellState[] cells)
		{
			var canBeFilled = new bool[cells.Length];
			var canBeEmpty = new bool[cells.Length];

			var block = line.Blocks.First();
			Enumerable.Range(0, cells.Length - block.Length + 1)
				.Where(startPosition => TryBlock(block, startPosition, cells))
				.Select(startPosition => Enumerable.Range(startPosition, block.Length).ToArray())
				.ForEach(occupiedCells => UpdateFilledEmpty(canBeFilled, canBeEmpty, occupiedCells));
			return new LineAnalysisResult(canBeFilled, canBeEmpty);
		}

		private static void UpdateFilledEmpty(bool[] canBeFilled, bool[] canBeEmpty, int[] occupiedCells)
		{
			occupiedCells.ForEach(i => canBeFilled[i] = true);

			Enumerable.Range(0, canBeEmpty.Length)
				.Except(occupiedCells)
				.ForEach(i => canBeEmpty[i] = true);
		}

		private bool TryBlock(IBlock block, int startPosition, CellState[] cells)
		{
			if (cells
				.Skip(startPosition)
				.Take(block.Length)
				.Any(cellState => cellState == CellState.Filled))
				return false;
			return true;
		}
	}
}