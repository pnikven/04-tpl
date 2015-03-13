using System;
using System.Linq;

namespace JapaneseCrossword.Enums
{
	public class Cell
	{
		private readonly bool isUnknown;
		private readonly bool isEmpty;
		private readonly bool isFilled;

		public Cell()
		{
			isUnknown = true;
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
			get { return !isUnknown; }
		}

		public bool IsUnknown
		{
			get { return isUnknown; }
		}

		public bool IsEmpty
		{
			get { return isEmpty; }
		}

		public bool IsFilled
		{
			get { return isFilled; }
		}

		public bool IsNotFilled
		{
			get { return !isFilled; }
		}

		public char ToChar()
		{
			if (isUnknown) return '?';
			if (isEmpty) return '.';
			if (isFilled) return '*';
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
				isEmpty == other.isEmpty && 
				isFilled == other.isFilled && 
				isUnknown == other.isUnknown;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = isUnknown.GetHashCode();
				hashCode = (hashCode * 397) ^ isEmpty.GetHashCode();
				hashCode = (hashCode * 397) ^ isFilled.GetHashCode();
				return hashCode;
			}
		}

		private Cell(bool isUnknown, bool isEmpty, bool isFilled)
		{
			this.isUnknown = isUnknown;
			this.isEmpty = isEmpty;
			this.isFilled = isFilled;
		}
	}
}