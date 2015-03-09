﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace JapaneseCrossword
{
	public class CrosswordSolver : ICrosswordSolver
	{
		private readonly Func<string, ICrossword> createCrossword;
		private readonly Func<LineType, IEnumerable<int[]>, IEnumerable<ILine>> getLines;

		public CrosswordSolver(Func<string, ICrossword> createCrossword, 
			Func<LineType, IEnumerable<int[]>, IEnumerable<ILine>> getLines)
		{
			this.createCrossword = createCrossword;
			this.getLines = getLines;
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

			var lines = GetLines(crossword);



			throw new NotImplementedException();
		}

		private IEnumerable<ILine> GetLines(ICrossword crossword)
		{
			return getLines(LineType.Row, crossword.RowBlocks)
				.Concat(getLines(LineType.Column, crossword.ColumnBlocks));
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
	}
}