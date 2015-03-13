using System.Collections.Generic;
using System.Linq;
using JapaneseCrossword.Enums;
using JapaneseCrossword.Interfaces;
using JapaneseCrossword.Solvers.Algoritms.Interfaces;
using JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces;
using JapaneseCrossword.Solvers.Utils;
using JapaneseCrossword.Solvers.Utils.Enums;
using JapaneseCrossword.Solvers.Utils.Enums.Extensions;
using JapaneseCrossword.Solvers.Utils.Interfaces;
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

		protected ILine[] GetInvalidLines(ILine[] lines)
		{
			return lines
				.Where(line => line.NeedRefresh)
				.ToArray();
		}

		protected void AnalyzeLine(ILine[] lines, ILine line, Cell[,] picture)
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

		private void UpdatePicture(ILine line, Cell[,] picture, int cellIndex, Cell[] cells)
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

		private bool UpdateCell(Cell[] cells, int cellIndex, ILineAnalysisResult analysisResult)
		{
			if (cells[cellIndex].Equals(analysisResult.Cells[cellIndex]))
				return false;
			cells[cellIndex] = analysisResult.Cells[cellIndex];
			return true;
		}

		private Cell[] CreateCells(ILine line, Cell[,] picture)
		{
			return Enumerable.Range(0, line.Type.IsRow() ? picture.GetLength(1) : picture.GetLength(0))
				.Select(i => line.Type.IsRow()
					? picture[line.Index, i]
					: picture[i, line.Index])
				.ToArray();
		}

		private IEnumerable<ILine> GetLines(LineType lineType, IEnumerable<int[]> blocks)
		{
			return blocks.Select((lineBlocks, i) => new Line(lineType, i, lineBlocks));
		}

		protected IEnumerable<ILine> GetLines(ICrosswordDescription crosswordDescription)
		{
			return
				GetLines(LineType.Row, crosswordDescription.RowBlocks)
				.Concat(
				GetLines(LineType.Column, crosswordDescription.ColumnBlocks));
		}
	}
}