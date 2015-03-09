using System.Collections.Generic;

namespace JapaneseCrossword
{
	public interface ILineProvider
	{
		IEnumerable<ILine> GetLines(LineType lineType, IEnumerable<int[]> blocks);
	}
}