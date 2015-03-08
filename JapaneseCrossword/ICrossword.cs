using System.Collections.Generic;

namespace JapaneseCrossword
{
	public interface ICrossword
	{
		int RowCount { get; }
		int ColumnCount { get; }
		IEnumerable<int[]> Rows { get; }
		IEnumerable<int[]> Columns { get; }
		bool IsCorrect();
	}
}