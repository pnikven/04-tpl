using JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces;
using JapaneseCrossword.Solvers.Utils;

namespace JapaneseCrossword.Solvers.Algoritms.Utils
{
	public class LineAnalyzer : ILineAnalyzer
	{
		private const int MinSpaceBetweenBlocks = 1;

		public ILineAnalysisResult Analyze(Line line, Cell[] cells)
		{
			var canBeFilled = new bool[cells.Length];
			var canBeEmpty = new bool[cells.Length];

			var block = line.Blocks[0];
			for (var startPosition = 0;
				startPosition <= GetMaxStartPositionOfBlock(block, line, cells.Length);
				startPosition++)
			{
				if (TryBlock(block, startPosition, line, cells, canBeEmpty, canBeFilled))
					UpdateEmptyBeforeBlock(startPosition, canBeEmpty);
			}
			return new LineAnalysisResult(canBeFilled, canBeEmpty);
		}

		private int GetMaxStartPositionOfBlock(Block currentBlock, Line line, int cellsCount)
		{
			var blockLengthsPlusMinSpacesFromCurrentBlock = 0;
			for (var i = currentBlock.Index; i < line.BlockCount; i++)
			{
				blockLengthsPlusMinSpacesFromCurrentBlock +=
					(line.Blocks[i].Length + MinSpaceBetweenBlocks);
			}
			blockLengthsPlusMinSpacesFromCurrentBlock -= MinSpaceBetweenBlocks;
			return cellsCount - blockLengthsPlusMinSpacesFromCurrentBlock;
		}

		private void UpdateEmptyBeforeBlock(int blockStartPosition, bool[] canBeEmpty)
		{
			for (var i = 0; i < blockStartPosition; i++)
				canBeEmpty[i] = true;
		}

		private bool TryBlock(Block block, int start, Line line, Cell[] cells, bool[] canBeEmpty, bool[] canBeFilled)
		{
			for (var i = start; i < start + block.Length; i++)
				if (cells[i].IsEmpty)
					return false;

			if (IsFirstBlockInLine(block))
				for (var i = 0; i < start; i++)
					if (cells[i].IsFilled)
						return false;

			if (IsLastBlockInLine(block, line))
			{
				for (var i = start + block.Length; i < cells.Length; i++)
					if (cells[i].IsFilled)
						return false;
				UpdateFilledOnBlock(block, start, canBeFilled);
				UpdateEmptyAfterBlock(block, start, canBeEmpty);
				return true;
			}

			var result = false;
			var nextBlock = line.Blocks[block.Index + 1];
			var endPositionOfCurrentBlock = start + block.Length - 1;
			var startPositionOfNextBlock = start + block.Length + MinSpaceBetweenBlocks;
			for (var nextStart = startPositionOfNextBlock;
				nextStart <= GetMaxStartPositionOfBlock(nextBlock, line, cells.Length);
				nextStart++)
			{
				if (MinEmptySpaceBeforeNextBlockExists(cells, nextStart) &&
					NoFilledCellsBetweenCurrentAndNextBlocks(endPositionOfCurrentBlock, nextStart, cells) &&
					TryBlock(nextBlock, nextStart, line, cells, canBeEmpty, canBeFilled)
					)
				{
					result = true;
					UpdateFilledOnBlock(block, start, canBeFilled);
					UpdateEmptyAfterBlockBeforeNextBlock(block, start, nextStart, canBeEmpty);
				}
			}

			return result;
		}

		private bool NoFilledCellsBetweenCurrentAndNextBlocks(int endPositionOfCurrentBlock, int nextStart, Cell[] cells)
		{
			for (var i = endPositionOfCurrentBlock + 1; i < nextStart; i++)
				if (cells[i].IsFilled)
					return false;
			return true;
		}

		private static bool MinEmptySpaceBeforeNextBlockExists(Cell[] cells, int nextStart)
		{
			for (var i = nextStart - MinSpaceBetweenBlocks; i < nextStart; i++)
				if (cells[i].IsFilled) return false;
			return true;
		}

		private void UpdateEmptyAfterBlockBeforeNextBlock(Block block, int start, int startNext, bool[] canBeEmpty)
		{
			for (var i = start + block.Length; i < startNext; i++) canBeEmpty[i] = true;
		}

		private void UpdateFilledOnBlock(Block block, int startPosition, bool[] canBeFilled)
		{
			for (var i = startPosition; i < startPosition + block.Length; i++)
				canBeFilled[i] = true;
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