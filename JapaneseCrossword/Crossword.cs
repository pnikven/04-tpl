using System;
using System.Collections.Generic;
using System.Linq;

namespace JapaneseCrossword
{
	class Crossword : ICrossword
	{
		public int RowCount { get; private set; }
		public int ColumnCount { get; private set; }
		public IEnumerable<int[]> Rows { get; private set; }
		public IEnumerable<int[]> Columns { get; private set; }

		public Crossword(string crosswordAsPlainText)
		{
			var lines = crosswordAsPlainText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

			RowCount = GetValueAfterColon(lines.First());
			Rows = lines
				.Skip(1)
				.Take(RowCount)
				.Select(GetIntArrayFromLine);

			ColumnCount = GetValueAfterColon(lines.Skip(RowCount + 1).First());
			Columns = lines
				.Skip(RowCount + 2)
				.Take(ColumnCount)
				.Select(GetIntArrayFromLine);
		}

		public bool IsCorrect()
		{
			return Rows.Sum(a => a.Sum()) == Columns.Sum(a => a.Sum());
		}

		private int GetValueAfterColon(string line)
		{
			return int.Parse(line.Split(':')[1]);
		}

		private int[] GetIntArrayFromLine(string line)
		{
			return line
				.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(int.Parse)
				.ToArray();
		}
	}
}
