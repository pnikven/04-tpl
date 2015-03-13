using System.Linq;
using JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces;
using JapaneseCrossword.Solvers.Utils;
using MoreLinq;

namespace JapaneseCrossword.Solvers.Algoritms.Utils
{
	public class LineAnalyzer : ILineAnalyzer
	{
		private const int MinSpaceBetweenBlocks = 1;

		public ILineAnalysisResult Analyze(Line line, Cell[] cells)
		{
			var canBeFilled = new bool[cells.Length];
			var canBeEmpty = new bool[cells.Length];

			var block = line.Blocks.First();
			Enumerable.Range(0, GetMaxStartPositionOfBlock(block, line, cells.Length) + 1)
				.Where(startPosition => TryBlock(block, startPosition, line, cells, canBeEmpty, canBeFilled))
				.ForEach(startPosition => UpdateEmptyBeforeBlock(startPosition, canBeEmpty));
			return new LineAnalysisResult(canBeFilled, canBeEmpty);
		}

		private int GetMaxStartPositionOfBlock(Block block, Line line, int cellsCount)
		{
			return cellsCount - line.Blocks.Skip(block.Index).Sum(b => b.Length + MinSpaceBetweenBlocks) + 1;
		}

		private void UpdateEmptyBeforeBlock(int blockStartPosition, bool[] canBeEmpty)
		{
			Enumerable.Range(0, blockStartPosition).ForEach(i => canBeEmpty[i] = true);
		}

		private bool TryBlock(Block block, int start, Line line, Cell[] cells, bool[] canBeEmpty, bool[] canBeFilled)
		{
			if (cells
				.Skip(start)
				.Take(block.Length)
				.Any(cell => cell.IsEmpty))
				return false;

			if (IsFirstBlockInLine(block) &&
				cells
					.Take(start)
					.Any(cell => cell.IsFilled))
				return false;

			if (IsLastBlockInLine(block, line))
			{
				if (cells
					.Skip(start + block.Length)
					.Any(cell => cell.IsFilled))
					return false;
				UpdateFilledOnBlock(block, start, canBeFilled);
				UpdateEmptyAfterBlock(block, start, canBeEmpty);
				return true;
			}

			var result = false;
			var nextBlock = line.Blocks.ElementAt(block.Index + 1);
			var endPositionOfCurrentBlock = start + block.Length - 1;
			var startPositionOfNextBlock = start + block.Length + MinSpaceBetweenBlocks;
			Enumerable.Range(startPositionOfNextBlock,
				GetMaxStartPositionOfBlock(nextBlock, line, cells.Length) - startPositionOfNextBlock + 1)
				.Where(nextStart =>
					MinEmptySpaceBeforeNextBlockExists(cells, nextStart) &&
					NoFilledCellsBetweenCurrentAndNextBlocks(endPositionOfCurrentBlock, nextStart, cells) &&
					TryBlock(nextBlock, nextStart, line, cells, canBeEmpty, canBeFilled))
				.ForEach(nextStart =>
				{
					result = true;
					UpdateFilledOnBlock(block, start, canBeFilled);
					UpdateEmptyAfterBlockBeforeNextBlock(block, start, nextStart, canBeEmpty);
				});

			return result;
		}

		private bool NoFilledCellsBetweenCurrentAndNextBlocks(int endPositionOfCurrentBlock, int nextStart, Cell[] cells)
		{
			return cells
				.Skip(endPositionOfCurrentBlock + 1)
				.Take(nextStart - endPositionOfCurrentBlock - 1)
				.All(state => state.IsNotFilled);
		}

		private static bool MinEmptySpaceBeforeNextBlockExists(Cell[] cells, int nextStart)
		{
			return Enumerable.Range(nextStart - MinSpaceBetweenBlocks, MinSpaceBetweenBlocks)
				.All(pos => cells[pos].IsNotFilled);
		}

		private void UpdateEmptyAfterBlockBeforeNextBlock(Block block, int start, int startNext, bool[] canBeEmpty)
		{
			for (var i = start + block.Length; i < startNext; i++) canBeEmpty[i] = true;
		}

		private void UpdateFilledOnBlock(Block block, int startPosition, bool[] canBeFilled)
		{
			Enumerable.Range(startPosition, block.Length).ForEach(i => canBeFilled[i] = true);
		}

		private void UpdateEmptyAfterBlock(Block block, int startPosition, bool[] canBeEmpty)
		{
			for (var i = startPosition + block.Length; i < canBeEmpty.Length; i++) canBeEmpty[i] = true;
		}

		private bool IsLastBlockInLine(Block block, Line line)
		{
			return block.Index == line.BlockCount - 1;
		}
		private bool IsFirstBlockInLine(Block block)
		{
			return block.Index == 0;
		}
	}
}