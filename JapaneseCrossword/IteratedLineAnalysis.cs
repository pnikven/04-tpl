using System.Linq;
using MoreLinq;

namespace JapaneseCrossword
{
	public class IteratedLineAnalysis : ICrosswordSolverAlgorithm
	{
		private readonly ILineAnalyzer lineAnalyzer;

		public IteratedLineAnalysis(ILineAnalyzer lineAnalyzer)
		{
			this.lineAnalyzer = lineAnalyzer;
		}

		public CellState[,] SolveCrossword(CellState[,] sourcePicture, ILine[] lines)
		{
			var rowCount = sourcePicture.GetLength(0);
			var columnCount = sourcePicture.GetLength(1);
			var picture = new CellState[rowCount, columnCount];

			while (lines.Any(line => line.NeedRefresh))
			{
				lines
					.Where(line => line.NeedRefresh)
					.ForEach(line =>
					{
						line.Refresh();

						var cells = Enumerable.Range(0, line.Type == LineType.Row ? columnCount : rowCount)
							.Select(i => line.Type == LineType.Row ?
								picture[line.Index, i] :
								picture[i, line.Index])
							.ToArray();
						ILineAnalysisResult analysisResult = lineAnalyzer.Analyze(line, cells);
						Enumerable.Range(0, cells.Length)
							.Where(i => cells[i] == CellState.Unknown &&
								analysisResult.CanBeFilled[i] ^ analysisResult.CanBeEmpty[i]
							)
							.ForEach(i =>
							{
								lines.First(l => l.Type == line.Type.Reverse() && l.Index == i).Invalidate();
								cells[i] = analysisResult.CanBeFilled[i] ? CellState.Filled : CellState.Empty;
								if (line.Type == LineType.Row)
									picture[line.Index, i] = cells[i];
								else
									picture[i, line.Index] = cells[i];
							});
					});
			}

			return picture;
		}
	}
}