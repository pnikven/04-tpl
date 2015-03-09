using System.Collections.Generic;

namespace JapaneseCrossword
{
	public interface ICrossword
	{
		int RowCount { get; }
		int ColumnCount { get; }
		IEnumerable<int[]> RowBlocks { get; }
		IEnumerable<int[]> ColumnBlocks { get; }
		bool IsCorrect { get; }
		CellState[,] Picture { get; }

	}
}