using System.Collections.Generic;

namespace JapaneseCrossword
{
	public interface ILine
	{
		LineType Type { get; }
		int Index { get; }
		bool NeedRefresh { get; }
		void Refresh();
		void Invalidate();
		IEnumerable<IBlock> Blocks { get; }
		int BlockCount { get; }

	}
}