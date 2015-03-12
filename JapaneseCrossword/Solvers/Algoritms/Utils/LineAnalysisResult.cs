using System.Linq;
using JapaneseCrossword.Enums;
using JapaneseCrossword.Enums.Extensions;
using JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces;

namespace JapaneseCrossword.Solvers.Algoritms.Utils
{
	public class LineAnalysisResult : ILineAnalysisResult
	{
		private int Length { get { return canBeFilled.Length; } }
		private readonly bool[] canBeFilled;
		private readonly bool[] canBeEmpty;

		public CellState[] Cells
		{
			get
			{
				return Enumerable.Range(0, Length)
					.Select(i => canBeFilled[i] ^ canBeEmpty[i] ?
						(canBeFilled[i] ? CellState.Filled : CellState.Empty) :
						CellState.Unknown)
					.ToArray();
			}
		}

		public LineAnalysisResult(bool[] canBeFilled, bool[] canBeEmpty)
		{
			this.canBeFilled = canBeFilled;
			this.canBeEmpty = canBeEmpty;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((LineAnalysisResult)obj);
		}

		protected bool Equals(LineAnalysisResult other)
		{
			return Cells.SequenceEqual(other.Cells);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((canBeFilled != null ? canBeFilled.GetHashCode() : 0) * 397) ^
					(canBeEmpty != null ? canBeEmpty.GetHashCode() : 0);
			}
		}

		public override string ToString()
		{
			return string.Join("", Enumerable.Range(0, Length)
				.Select(i => Cells[i].ToChar()));
		}
	}
}