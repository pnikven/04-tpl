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

		public virtual CellState[,] SolveCrossword(CellState[,] sourcePicture, ILine[] lines)
		{
			var picture = new CellState[sourcePicture.GetLength(0), sourcePicture.GetLength(1)];
			while (true)
			{
				var invalidLines = GetInvalidLines(lines);
				if (!invalidLines.Any())
					break;
				invalidLines.ForEach(line => AnalyzeLine(lines, line, picture));
			}

			return picture;
		}

		protected ILine[] GetInvalidLines(ILine[] lines)
		{
			return lines
				.Where(line => line.NeedRefresh)
				.ToArray();
		}

		protected void AnalyzeLine(ILine[] lines, ILine line, CellState[,] picture)
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