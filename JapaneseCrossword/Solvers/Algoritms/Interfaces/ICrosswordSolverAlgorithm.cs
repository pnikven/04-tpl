﻿using JapaneseCrossword.Interfaces;
using JapaneseCrossword.Solvers.Utils;

namespace JapaneseCrossword.Solvers.Algoritms.Interfaces
{
	public interface ICrosswordSolverAlgorithm
	{
		Cell[,] SolveCrossword(ICrosswordDescription crosswordDescription);
	}
}