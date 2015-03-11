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
			var picture = new CellState[sourcePicture.GetLength(0), sourcePicture.GetLength(1)];
			while (lines.Any(line => line.NeedRefresh))
			{
				lines
					.Where(line => line.NeedRefresh)
					.ForEach(line =>
					{
						line.Refresh();
						var cells = CreateCells(line, picture);
						ILineAnalysisResult analysisResult = lineAnalyzer.Analyze(line, cells);
						Enumerable.Range(0, cells.Length)
							.Where(cellIndex => IsCellUpdated(cells, cellIndex, analysisResult))
							.ForEach(cellIndex =>
							{
								InvalidateCrossLine(lines, line.Type, cellIndex);
								cells[cellIndex] = analysisResult.CanBeFilled[cellIndex] ? CellState.Filled : CellState.Empty;
								if (line.Type == LineType.Row)
									picture[line.Index, cellIndex] = cells[cellIndex];
								else
									picture[cellIndex, line.Index] = cells[cellIndex];
							});
					});
			}

			return picture;
		}

		private void InvalidateCrossLine(ILine[] lines, LineType currentLineType, int cellIndex)
		{
			lines
				.First(l => l.Type == currentLineType.Reverse() && l.Index == cellIndex)
				.Invalidate();
		}

		private bool IsCellUpdated(CellState[] cells, int cellIndex, ILineAnalysisResult analysisResult)
		{
			return
				cells[cellIndex] == CellState.Unknown &&
				analysisResult.CanBeFilled[cellIndex] ^ analysisResult.CanBeEmpty[cellIndex];
		}

		private CellState[] CreateCells(ILine line, CellState[,] picture)
		{
			return Enumerable.Range(0, line.Type == LineType.Row ? picture.GetLength(1) : picture.GetLength(0))
				.Select(i => line.Type == LineType.Row
					? picture[line.Index, i]
					: picture[i, line.Index])
				.ToArray();
		}
	}
}