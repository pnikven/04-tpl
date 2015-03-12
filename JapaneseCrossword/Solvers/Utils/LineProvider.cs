using System.Collections.Generic;
using System.Linq;

namespace JapaneseCrossword
{
	public class LineProvider : ILineProvider
	{
		public IEnumerable<ILine> GetLines(LineType lineType, IEnumerable<int[]> blocks)
		{
			return blocks.Select((lineBlocks, i) => new Line(lineType, i, lineBlocks));
		}

		public IEnumerable<ILine> GetLines(ICrossword crossword)
		{
			return
				GetLines(LineType.Row, crossword.RowBlocks)
				.Concat(
				GetLines(LineType.Column, crossword.ColumnBlocks));
		}
	}
}
