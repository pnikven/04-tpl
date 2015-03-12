using System.Collections.Generic;
using JapaneseCrossword.Solvers.Utils.Enums;

namespace JapaneseCrossword.Solvers.Utils.Interfaces
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