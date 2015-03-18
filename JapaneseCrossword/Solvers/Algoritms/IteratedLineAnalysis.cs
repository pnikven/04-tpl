using System.Collections.Generic;
using System.Linq;
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

		public Cell[,] SolveCrossword(ICrosswordDescription crosswordDescription)
		{
			try
			{
				return SolveCrosswordUnsafe(crosswordDescription);
			}
			catch
			{
				return null;
			}
		}

		protected virtual Cell[,] SolveCrosswordUnsafe(ICrosswordDescription crosswordDescription)
		{
			var picture = CreatePicture(crosswordDescription.RowCount, crosswordDescription.ColumnCount);
			Enumerable.Range(0, crosswordDescription.RowCount)
				.Cartesian(Enumerable.Range(0, crosswordDescription.ColumnCount), (i, j) =>
					picture[i, j] = new Cell());
			var lines = CreateLinesFromCrosswordDescription(crosswordDescription).ToArray();
			while (true)
			{
				var invalidLines = GetLinesToBeRefreshed(lines);
				if (!invalidLines.Any())
					break;
				invalidLines.ForEach(line => AnalyzeLine(lines, line, picture));
			}

			return picture;
		}

		protected static Cell[,] CreatePicture(int rowCount, int columnCount)
		{
			var picture = new Cell[rowCount, columnCount];
			for (var i = 0; i < rowCount; i++)
				for (var j = 0; j < columnCount; j++)
					picture[i, j] = new Cell();
			return picture;
		}

		protected Line[] GetLinesToBeRefreshed(Line[] lines)
		{
			return lines
				.Where(line => line.NeedRefresh)
				.ToArray();
		}

		protected void AnalyzeLine(Line[] allLines, Line analyzedLine, Cell[,] currentPicture)
		{
			analyzedLine.Refresh();
			var cells = CreateCells(analyzedLine, currentPicture);
			var analysisResult = lineAnalyzer.Analyze(analyzedLine, cells);
			Enumerable.Range(0, cells.Length)
				.Where(cellIndex => TryUpdateCells(cells, cellIndex, analysisResult.Cells))
				.ForEach(cellIndex =>
				{
					InvalidateCrossLine(allLines, analyzedLine.Type, cellIndex);
					UpdatePicture(currentPicture, analyzedLine, cellIndex, cells);
				});
		}

		private static void UpdatePicture(Cell[,] pictureToUpdate, Line line, int cellIndex, Cell[] sourceCells)
		{
			if (line.Type == LineType.Row)
				pictureToUpdate[line.Index, cellIndex] = sourceCells[cellIndex];
			else
				pictureToUpdate[cellIndex, line.Index] = sourceCells[cellIndex];
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

		private bool TryUpdateCells(Cell[] cellsToUpdate, int cellIndex, Cell[] sourceCells)
		{
			if (cellsToUpdate[cellIndex].Equals(sourceCells[cellIndex]))
				return false;
			cellsToUpdate[cellIndex] = sourceCells[cellIndex];
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

		private IEnumerable<Line> CreateLinesFromBlocks(LineType lineType, IEnumerable<int[]> blocks)
		{
			return blocks.Select((lineBlocks, i) => Line.Create(lineType, i, lineBlocks));
		}

		protected IEnumerable<Line> CreateLinesFromCrosswordDescription(ICrosswordDescription crosswordDescription)
		{
			return
				CreateLinesFromBlocks(LineType.Row, crosswordDescription.RowBlocks)
				.Concat(
				CreateLinesFromBlocks(LineType.Column, crosswordDescription.ColumnBlocks));
		}
	}
}