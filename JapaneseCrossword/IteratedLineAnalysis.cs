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
							.Where(cellIndex => UpdateCell(cells, cellIndex, analysisResult))
							.ForEach(cellIndex =>
							{
								InvalidateCrossLine(lines, line.Type, cellIndex);
								UpdatePicture(line, picture, cellIndex, cells);
							});
					});
			}
			return picture;
		}

		private void UpdatePicture(ILine line, CellState[,] picture, int cellIndex, CellState[] cells)
		{
			if (line.Type.IsRow())
				picture[line.Index, cellIndex] = cells[cellIndex];
			else
				picture[cellIndex, line.Index] = cells[cellIndex];
		}

		private void InvalidateCrossLine(ILine[] lines, LineType currentLineType, int cellIndex)
		{
			lines
				.First(l => l.Type == currentLineType.Reverse() && l.Index == cellIndex)
				.Invalidate();
		}

		private bool UpdateCell(CellState[] cells, int cellIndex, ILineAnalysisResult analysisResult)
		{
			if (cells[cellIndex] == analysisResult.Cells[cellIndex])
				return false;
			cells[cellIndex] = analysisResult.Cells[cellIndex];
			return true;
		}

		private CellState[] CreateCells(ILine line, CellState[,] picture)
		{
			return Enumerable.Range(0, line.Type.IsRow() ? picture.GetLength(1) : picture.GetLength(0))
				.Select(i => line.Type.IsRow()
					? picture[line.Index, i]
					: picture[i, line.Index])
				.ToArray();
		}
	}
}