using JapaneseCrossword.Enums;
using JapaneseCrossword.Solvers.Utils.Interfaces;

namespace JapaneseCrossword.Solvers.Algoritms.Interfaces
{
	public interface ICrosswordSolverAlgorithm
	{
		CellState[,] SolveCrossword(CellState[,] sourcePicture, ILine[] lines);
	}
}