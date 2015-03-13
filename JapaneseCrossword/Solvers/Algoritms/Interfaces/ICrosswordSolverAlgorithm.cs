using JapaneseCrossword.Enums;
using JapaneseCrossword.Interfaces;

namespace JapaneseCrossword.Solvers.Algoritms.Interfaces
{
	public interface ICrosswordSolverAlgorithm
	{
		Cell[,] SolveCrossword(ICrosswordDescription crosswordDescription);
	}
}