using JapaneseCrossword.Enums;
using JapaneseCrossword.Solvers.Utils;

namespace JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces
{
	public interface ILineAnalysisResult
	{
		Cell[] Cells { get; }
	}
}