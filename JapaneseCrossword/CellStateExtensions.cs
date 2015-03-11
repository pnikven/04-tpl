namespace JapaneseCrossword
{
	public static class CellStateExtensions
	{
		public static bool IsKnown(this CellState cellState)
		{
			return cellState != CellState.Unknown;
		}

		public static bool IsUnknown(this CellState cellState)
		{
			return cellState == CellState.Unknown;
		}

		public static bool IsEmpty(this CellState cellState)
		{
			return cellState == CellState.Empty;
		}

		public static bool IsFilled(this CellState cellState)
		{
			return cellState == CellState.Filled;
		}

		public static bool IsNotFilled(this CellState cellState)
		{
			return cellState != CellState.Filled;
		}

	}
}