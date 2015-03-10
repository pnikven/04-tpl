using System.Linq;
using MoreLinq;

namespace JapaneseCrossword
{
	class LineAnalyzer : ILineAnalyzer
	{
		private const int MinSpaceBetweenBlocks = 1;

		public ILineAnalysisResult Analyze(ILine line, CellState[] cells)
		{
			var canBeFilled = new bool[cells.Length];
			var canBeEmpty = new bool[cells.Length];

			var block = line.Blocks.First();
			Enumerable.Range(0, GetMaxStartPositionOfBlock(block, line, cells.Length) + 1)
				.Where(startPosition => TryBlock(block, startPosition, line, cells, canBeEmpty, canBeFilled))
				.ForEach(startPosition => UpdateEmptyBeforeBlock(startPosition, canBeEmpty));
			return new LineAnalysisResult(canBeFilled, canBeEmpty);
		}

		private int GetMaxStartPositionOfBlock(IBlock block, ILine line, int cellsCount)
		{
			return cellsCount - line.Blocks.Skip(block.Index).Sum(b => b.Length + MinSpaceBetweenBlocks) + 1;
		}

		private void UpdateEmptyBeforeBlock(int blockStartPosition, bool[] canBeEmpty)
		{
			Enumerable.Range(0, blockStartPosition).ForEach(i => canBeEmpty[i] = true);
		}

		private bool TryBlock(IBlock block, int start, ILine line, CellState[] cells, bool[] canBeEmpty, bool[] canBeFilled)
		{
			if (cells
				.Skip(start)
				.Take(block.Length)
				.Any(cellState => cellState == CellState.Filled))
				return false;

			if (IsLastBlockInLine(block, line))
			{
				if (cells
					.Skip(start + block.Length)
					.Any(state => state == CellState.Filled))
					return false;
				UpdateFilledOnBlock(block, start, canBeFilled);
				UpdateEmptyAfterBlock(block, start, canBeEmpty);
				return true;
			}

			var result = false;
			var nextBlock = line.Blocks.ElementAt(block.Index + 1);
			var startPositionOfNextBlock = start + block.Length + MinSpaceBetweenBlocks;
			Enumerable.Range(startPositionOfNextBlock,
				GetMaxStartPositionOfBlock(nextBlock, line, cells.Length) - startPositionOfNextBlock + 1)
				.Where(nextStart =>
					cells[nextStart - 1] != CellState.Filled &&
					TryBlock(nextBlock, nextStart, line, cells, canBeEmpty, canBeFilled))
				.ForEach(nextStart =>
				{
					result = true;
					UpdateFilledOnBlock(block, start, canBeFilled);
					UpdateEmptyAfterBlockBeforeNextBlock(block, start, nextStart, canBeEmpty);
				});

			return result;
		}

		private void UpdateEmptyAfterBlockBeforeNextBlock(IBlock block, int start, int startNext, bool[] canBeEmpty)
		{
			for (var i = start + block.Length; i < startNext; i++) canBeEmpty[i] = true;
		}

		private void UpdateFilledOnBlock(IBlock block, int startPosition, bool[] canBeFilled)
		{
			Enumerable.Range(startPosition, block.Length).ForEach(i => canBeFilled[i] = true);
		}

		private void UpdateEmptyAfterBlock(IBlock block, int startPosition, bool[] canBeEmpty)
		{
			for (var i = startPosition + block.Length; i < canBeEmpty.Length; i++) canBeEmpty[i] = true;
		}

		private bool IsLastBlockInLine(IBlock block, ILine line)
		{
			return block.Index == line.BlockCount - 1;
		}
	}
}