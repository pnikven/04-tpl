using JapaneseCrossword.Enums;

namespace JapaneseCrossword.Solvers.Interfaces
{
    public interface ICrosswordSolver
    {
        SolutionStatus Solve(string inputFilePath, string outputFilePath);
    }
}