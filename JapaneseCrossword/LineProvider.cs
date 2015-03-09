using System;
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
	}
}
