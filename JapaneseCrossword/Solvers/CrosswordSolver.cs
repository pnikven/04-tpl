using System;
using System.Collections.Generic;
using System.Linq;
using JapaneseCrossword.Enums;
using JapaneseCrossword.Extensions;
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
		private readonly Func<string, string> readFile;
		private readonly Func<string, string, bool> writeFile;

		public CrosswordSolver(Func<string, ICrossword> createCrossword,
			Func<ICrossword, IEnumerable<ILine>> getLines,
			Func<CellState[,], ILine[], CellState[,]> solveCrossword,
			Func<CellState[,], string> createOutputTextResult,
			Func<string, string> readFile,
			Func<string, string, bool> writeFile)
		{
			this.createCrossword = createCrossword;
			this.getLines = getLines;
			this.solveCrossword = solveCrossword;
			this.createOutputTextResult = createOutputTextResult;
			this.readFile = readFile;
			this.writeFile = writeFile;
		}

		public SolutionStatus Solve(string inputFilePath, string outputFilePath)
		{
			var crosswordAsPlainText = readFile(inputFilePath);
			if (crosswordAsPlainText == null)
				return SolutionStatus.BadInputFilePath;

			var crossword = createCrossword(crosswordAsPlainText);
			if (!crossword.IsCorrect)
				return SolutionStatus.IncorrectCrossword;

			var sourcePicture = new CellState[crossword.RowCount, crossword.ColumnCount];
			var lines = getLines(crossword).ToArray();
			var solvedCrossword = solveCrossword(sourcePicture, lines);
			var outputResult = createOutputTextResult(solvedCrossword);

			if (!writeFile(outputFilePath, outputResult))
				return SolutionStatus.BadOutputFilePath;

			return outputResult.Contains('?') ? SolutionStatus.PartiallySolved : SolutionStatus.Solved;
		}
	}
}