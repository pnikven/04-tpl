using System;

namespace JapaneseCrossword
{
	public static class CharExtensions
	{
		public static CellState ToCellState(this char c)
		{
			switch (c)
			{
				case '.':
					return CellState.Empty;
				case '*':
					return CellState.Filled;
				case '?':
					return CellState.Unknown;
				default:
					throw new ArgumentOutOfRangeException(string.Format("Unknown char {0}", c));
			}
		}
	}
}