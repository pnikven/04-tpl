using System.IO;
using System.Linq;
using System.Text;
using JapaneseCrossword.Enums;
using JapaneseCrossword.Solvers.Algoritms.Interfaces;
using JapaneseCrossword.Solvers.Interfaces;
using JapaneseCrossword.Solvers.Utils;
using MoreLinq;

namespace JapaneseCrossword.Solvers
{
	public class CrosswordSolver : ICrosswordSolver
	{
		private readonly ICrosswordSolverAlgorithm algorithm;

		public CrosswordSolver(ICrosswordSolverAlgorithm algorithm)
		{
			this.algorithm = algorithm;
		}

		public SolutionStatus Solve(string inputFilePath, string outputFilePath)
		{
			// Если сделать как в комментарии ниже, будет 
			//string crossword;
			//if (TryReadFile(inputFilePath, out crossword))
			//	return SolutionStatus.BadInputFilePath;
			var crossword = TryReadFile(inputFilePath);
			if (crossword == null)
				return SolutionStatus.BadInputFilePath;

			var crosswordDescription = CrosswordDescription.Create(crossword);
			if (crosswordDescription == null || !crosswordDescription.IsCorrect)
				return SolutionStatus.IncorrectCrossword;

			var solvedCrossword = algorithm.SolveCrossword(crosswordDescription);
			if (solvedCrossword == null)
				return SolutionStatus.Error;

			var outputResult = ConvertPictureToString(solvedCrossword);
			if (!TryWriteFile(outputFilePath, outputResult))
				return SolutionStatus.BadOutputFilePath;

			return outputResult.Contains('?') ? SolutionStatus.PartiallySolved : SolutionStatus.Solved;
		}

		//todo: private
		public static string ConvertPictureToString(Cell[,] picture)
		{
			return string.Format("{0}\r\n",
				Enumerable.Range(0, picture.GetLength(0))
				.Cartesian(Enumerable.Range(0, picture.GetLength(1)), (i, j) => picture[i, j].ToChar())
				.Batch(picture.GetLength(1))
				.Select(x => x.ToDelimitedString(""))
				.ToDelimitedString("\r\n"));
		}

		// Общепринятая сигнатура для методов типа Try(...) такая:
		// bool Try(...)(manyParams, out TResult something)
		// Соответственно, если не получилось, возвращаем false и null в качестве результата
		private string TryReadFile(string filePath)
		{
			try
			{
				return File.ReadAllText(filePath, Encoding.UTF8);
			}
			catch
			{
				return null;
			}
		}

		private bool TryWriteFile(string filePath, string contents)
		{
			try
			{
				File.WriteAllText(filePath, contents, Encoding.UTF8);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}