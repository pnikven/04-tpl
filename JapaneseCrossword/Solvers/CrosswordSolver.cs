using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JapaneseCrossword.Enums;
using JapaneseCrossword.Interfaces;
using JapaneseCrossword.Solvers.Interfaces;
using JapaneseCrossword.Solvers.Utils.Interfaces;

namespace JapaneseCrossword.Solvers
{
	public class CrosswordSolver : ICrosswordSolver
	{
		private readonly Func<string, ICrossword> createCrossword;
		private readonly Func<ICrossword, IEnumerable<ILine>> getLines;
		private readonly Func<CellState[,], ILine[], CellState[,]> solveCrossword;
		private readonly Func<CellState[,], string> createOutputTextResult;

		public CrosswordSolver(Func<string, ICrossword> createCrossword,
			Func<ICrossword, IEnumerable<ILine>> getLines,
			Func<CellState[,], ILine[], CellState[,]> solveCrossword,
			Func<CellState[,], string> createOutputTextResult)
		{
			this.createCrossword = createCrossword;
			this.getLines = getLines;
			this.solveCrossword = solveCrossword;
			this.createOutputTextResult = createOutputTextResult;
		}

		public SolutionStatus Solve(string inputFilePath, string outputFilePath)
		{
			if (Path.GetInvalidFileNameChars().Any(outputFilePath.Contains))
				return SolutionStatus.BadOutputFilePath;

			var crosswordAsPlainText = TryReadInputFile(inputFilePath);
			if (crosswordAsPlainText == null)
				return SolutionStatus.BadInputFilePath;

			var crossword = createCrossword(crosswordAsPlainText);
			if (!crossword.IsCorrect)
				return SolutionStatus.IncorrectCrossword;

			var sourcePicture = new CellState[crossword.RowCount, crossword.ColumnCount];
			var lines = getLines(crossword).ToArray();
			var solvedCrossword = solveCrossword(sourcePicture, lines);
			var outputResult = createOutputTextResult(solvedCrossword);

			if (!TryWriteOutputFile(outputFilePath, outputResult))
				return SolutionStatus.BadOutputFilePath;

			return outputResult.Contains('?') ? SolutionStatus.PartiallySolved : SolutionStatus.Solved;
		}

		private string TryReadInputFile(string inputFilePath)
		{
			try
			{
				return File.ReadAllText(inputFilePath, Encoding.UTF8);
			}
			catch
			{
				return null;
			}
		}

		private bool TryWriteOutputFile(string outputFilePath, string contents)
		{
			try
			{
				File.WriteAllText(outputFilePath, contents, Encoding.UTF8);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}