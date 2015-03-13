using JapaneseCrossword.Enums;
using JapaneseCrossword.Solvers.Utils;

namespace JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces
{
	public interface ILineAnalyzer
	{
		ILineAnalysisResult Analyze(Line line, Cell[] cells);
	}
}