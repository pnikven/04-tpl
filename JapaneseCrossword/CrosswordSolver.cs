using System;
using System.IO;

namespace JapaneseCrossword
{
    public class CrosswordSolver : ICrosswordSolver
    {
        public SolutionStatus Solve(string inputFilePath, string outputFilePath)
        {
            if(!File.Exists(inputFilePath))
				return SolutionStatus.BadInputFilePath;

			throw new NotImplementedException();
        }
    }
}