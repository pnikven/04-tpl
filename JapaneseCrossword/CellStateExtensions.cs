using System;
using System.Linq;
using MoreLinq;

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

		public static char ToChar(this CellState cellState)
		{
			switch (cellState)
			{
				case CellState.Unknown:
					return '?';
				case CellState.Empty:
					return '.';
				case CellState.Filled:
					return '*';
				default:
					throw new ArgumentOutOfRangeException("cellState");
			}
		}

		public static string AsString(this CellState[] cells)
		{
			return cells.Select(cell => cell.ToChar()).ToDelimitedString("");
		}

		public static string AsString(this CellState[,] picture)
		{
			return string.Format("{0}\r\n",
				Enumerable.Range(0, picture.GetLength(0))
				.Cartesian(Enumerable.Range(0, picture.GetLength(1)), (i, j) => picture[i, j].ToChar())
				.Batch(picture.GetLength(1))
				.Select(x => x.ToDelimitedString(""))
				.ToDelimitedString("\r\n"));
		}
	}
}