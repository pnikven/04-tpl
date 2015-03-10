using System.Collections.Generic;

namespace JapaneseCrossword
{
	public interface ILineAnalyzer
	{
		ILineAnalysisResult Analyze(ILine line, CellState[] cells);
	}
}