using System;
using System.IO;
using System.Linq;
using System.Text;

namespace JapaneseCrossword
{
	public class CrosswordSolver : ICrosswordSolver
	{
		private readonly Func<string, ICrossword> createCrossword;

		public CrosswordSolver(Func<string, ICrossword> createCrossword)
		{
			this.createCrossword = createCrossword;
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

			throw new NotImplementedException();
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