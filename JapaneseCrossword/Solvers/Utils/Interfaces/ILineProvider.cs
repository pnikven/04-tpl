using System.Collections.Generic;
using JapaneseCrossword.Interfaces;
using JapaneseCrossword.Solvers.Utils.Enums;

namespace JapaneseCrossword.Solvers.Utils.Interfaces
{
	public interface ILineProvider
	{
		IEnumerable<ILine> GetLines(LineType lineType, IEnumerable<int[]> blocks);
		IEnumerable<ILine> GetLines(ICrossword crossword);
	}
}