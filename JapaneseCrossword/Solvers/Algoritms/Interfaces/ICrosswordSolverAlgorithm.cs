namespace JapaneseCrossword
{
	public interface ICrosswordSolverAlgorithm
	{
		CellState[,] SolveCrossword(CellState[,] sourcePicture, ILine[] lines);
	}
}