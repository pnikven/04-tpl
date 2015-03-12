using JapaneseCrossword.Enums;

namespace JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces
{
	public interface ILineAnalysisResult
	{
		CellState[] Cells { get; }
	}
}