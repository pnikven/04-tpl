using System.Collections.Generic;

namespace JapaneseCrossword.Interfaces
{
	public interface ICrosswordDescription
	{
		int RowCount { get; }
		int ColumnCount { get; }
		IEnumerable<int[]> RowBlocks { get; }
		IEnumerable<int[]> ColumnBlocks { get; }
		bool IsCorrect { get; }
	}
}