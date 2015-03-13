using System.Collections.Generic;
using System.Linq;
using JapaneseCrossword.Enums;
using JapaneseCrossword.Interfaces;
using JapaneseCrossword.Solvers.Algoritms.Interfaces;
using JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces;
using JapaneseCrossword.Solvers.Utils;
using JapaneseCrossword.Solvers.Utils.Enums;
using MoreLinq;

namespace JapaneseCrossword.Solvers.Algoritms
{
	public class IteratedLineAnalysis : ICrosswordSolverAlgorithm
	{
		private readonly ILineAnalyzer lineAnalyzer;

		public IteratedLineAnalysis(ILineAnalyzer lineAnalyzer)
		{
			this.lineAnalyzer = lineAnalyzer;
		}

		public virtual Cell[,] SolveCrossword(ICrosswordDescription crosswordDescription)
		{
			var picture = CreatePicture(crosswordDescription);
			Enumerable.Range(0, crosswordDescription.RowCount)
				.Cartesian(Enumerable.Range(0, crosswordDescription.ColumnCount), (i, j) =>
					picture[i, j] = new Cell());
			var lines = GetLines(crosswordDescription).ToArray();
			while (true)
			{
				var invalidLines = GetInvalidLines(lines);
				if (!invalidLines.Any())
					break;
				invalidLines.ForEach(line => AnalyzeLine(lines, line, picture));
			}

			return picture;
		}

		protected Cell[,] CreatePicture(ICrosswordDescription crosswordDescription)
		{
			var picture = new Cell[crosswordDescription.RowCount, crosswordDescription.ColumnCount];
			for (var i = 0; i < crosswordDescription.RowCount; i++)
				for (var j = 0; j < crosswordDescription.ColumnCount; j++)
					picture[i, j] = new Cell();
			return picture;
		}

		protected Line[] GetInvalidLines(Line[] lines)
		{
			return lines
				.Where(line => line.NeedRefresh)
				.ToArray();
		}

		protected void AnalyzeLine(Line[] lines, Line line, Cell[,] picture)
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

		private void UpdatePicture(Line line, Cell[,] picture, int cellIndex, Cell[] cells)
		{
			if (line.Type == LineType.Row)
				picture[line.Index, cellIndex] = cells[cellIndex];
			else
				picture[cellIndex, line.Index] = cells[cellIndex];
		}

		private void InvalidateCrossLine(Line[] lines, LineType lineType, int cellIndex)
		{
			lines
				.First(l => l.Type == ReverseLineType(lineType) && l.Index == cellIndex)
				.Invalidate();
		}

		protected LineType ReverseLineType(LineType lineType)
		{
			return lineType == LineType.Row ? LineType.Column : LineType.Row;
		}

		private bool UpdateCell(Cell[] cells, int cellIndex, ILineAnalysisResult analysisResult)
		{
			if (cells[cellIndex].Equals(analysisResult.Cells[cellIndex]))
				return false;
			cells[cellIndex] = analysisResult.Cells[cellIndex];
			return true;
		}

		private Cell[] CreateCells(Line line, Cell[,] picture)
		{
			return Enumerable.Range(0, line.IsRow ? picture.GetLength(1) : picture.GetLength(0))
				.Select(i => line.IsRow
					? picture[line.Index, i]
					: picture[i, line.Index])
				.ToArray();
		}

		private IEnumerable<Line> GetLines(LineType lineType, IEnumerable<int[]> blocks)
		{
			return blocks.Select((lineBlocks, i) => Line.Create(lineType, i, lineBlocks));
		}

		protected IEnumerable<Line> GetLines(ICrosswordDescription crosswordDescription)
		{
			return
				GetLines(LineType.Row, crosswordDescription.RowBlocks)
				.Concat(
				GetLines(LineType.Column, crosswordDescription.ColumnBlocks));
		}
	}
}