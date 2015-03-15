using System;
using System.Collections.Generic;
using System.Linq;
using JapaneseCrossword.Solvers.Utils.Enums;

namespace JapaneseCrossword.Solvers.Utils
{
	public class Cell
	{
		public bool IsUnknown { get { return CellState == CellState.Unknown; } }
		public bool IsKnown { get { return !IsUnknown; } }
		public bool IsFilled { get { return CellState == CellState.Filled; } }
		public bool IsNotFilled { get { return !IsFilled; } }
		public bool IsEmpty { get { return CellState == CellState.Empty; } }

		public Cell()
		{
			CellState = CellState.Unknown;
		}

		public Cell(CellState cellState)
		{
			CellState = cellState;
		}

		public static Cell Create(char symbol)
		{
			if (!CellStateToCharMap.ContainsValue(symbol))
				throw new Exception(
					string.Format("Symbol {0} not recognized", symbol));
			var cellState = CellStateToCharMap.First(pair => pair.Value.Equals(symbol)).Key;
			return new Cell(cellState);
		}

		public static Cell[] CreateCellsFromString(string cellStates)
		{
			return cellStates.Select(Create).ToArray();
		}

		public char ToChar()
		{
			if (CellStateToCharMap.ContainsKey(CellState))
				return CellStateToCharMap[CellState];
			throw new Exception(
				string.Format("There is now corresponding symbol for cell state {0}", CellState));
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((Cell)obj);
		}

		protected bool Equals(Cell other)
		{
			return
				CellState == other.CellState;
		}

		public override int GetHashCode()
		{
			return (int)CellState;
		}

		private CellState CellState { get; set; }

		private readonly static Dictionary<CellState, char> CellStateToCharMap =
			new Dictionary<CellState, char>()
			{
				{ CellState.Unknown, '?' },
				{ CellState.Filled, '*' },
				{ CellState.Empty, '.' }
			};
	}
}