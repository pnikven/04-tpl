using JapaneseCrossword.Enums;
using JapaneseCrossword.Solvers.Utils.Interfaces;

namespace JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces
{
	public interface ILineAnalyzer
	{
		ILineAnalysisResult Analyze(ILine line, Cell[] cells);
	}
}