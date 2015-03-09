using System.Collections.Generic;
using System.Linq;

namespace JapaneseCrossword
{
	public class Line : ILine
	{
		public LineType Type { get; private set; }
		public int Index { get; private set; }
		public bool NeedRefresh { get; private set; }
		public IEnumerable<IBlock> Blocks { get; private set; }
		public int BlockCount { get; private set; }

		public Line(LineType type, int index, int[] blocks)
		{
			Type = type;
			Index = index;
			NeedRefresh = true;
			Blocks = blocks.Select((blockLength, i) => new Block(blockLength, i));
			BlockCount = blocks.Length;
		}
	}
}