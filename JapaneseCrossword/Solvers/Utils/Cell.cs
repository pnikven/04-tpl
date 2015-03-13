using System;
using System.Linq;
using JapaneseCrossword.Solvers.Utils.Enums;

namespace JapaneseCrossword.Solvers.Utils
{
	public class Cell
	{
		public bool IsUnknown { get; private set; }
		public bool IsEmpty { get; private set; }
		public bool IsFilled { get; private set; }

		public Cell()
		{
			IsUnknown = true;
		}

		public static Cell Create(CellState cellState)
		{
			switch (cellState)
			{
				case CellState.Unknown:
					return new Cell(true, false, false);
				case CellState.Empty:
					return new Cell(false, true, false);
				case CellState.Filled:
					return new Cell(false, false, true);
				default:
					return null;
			}
		}

		public static Cell Create(char cellState)
		{
			switch (cellState)
			{
				case '?':
					return new Cell(true, false, false);
				case '.':
					return new Cell(false, true, false);
				case '*':
					return new Cell(false, false, true);
				default:
					return null;
			}
		}

		public static Cell[] Create(string cellStates)
		{
			return cellStates.Select(Create).ToArray();
		}


		public bool IsKnown
		{
			get { return !IsUnknown; }
		}

		public bool IsNotFilled
		{
			get { return !IsFilled; }
		}

		public char ToChar()
		{
			if (IsUnknown) return '?';
			if (IsEmpty) return '.';
			if (IsFilled) return '*';
			throw new Exception("cellState not set");
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
				IsEmpty == other.IsEmpty &&
				IsFilled == other.IsFilled &&
				IsUnknown == other.IsUnknown;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = IsUnknown.GetHashCode();
				hashCode = (hashCode * 397) ^ IsEmpty.GetHashCode();
				hashCode = (hashCode * 397) ^ IsFilled.GetHashCode();
				return hashCode;
			}
		}

		private Cell(bool isUnknown, bool isEmpty, bool isFilled)
		{
			IsUnknown = isUnknown;
			IsEmpty = isEmpty;
			IsFilled = isFilled;
		}
	}
}