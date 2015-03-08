using System;
using System.IO;
using System.Linq;

namespace JapaneseCrossword
{
	public class CrosswordSolver : ICrosswordSolver
	{
		public SolutionStatus Solve(string inputFilePath, string outputFilePath)
		{
			if (!File.Exists(inputFilePath))
				return SolutionStatus.BadInputFilePath;

			if(Path.GetInvalidFileNameChars().Any(outputFilePath.Contains))
				return SolutionStatus.BadOutputFilePath;

			throw new NotImplementedException();
		}
	}
}