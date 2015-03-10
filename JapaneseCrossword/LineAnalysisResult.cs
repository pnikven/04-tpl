using System.Linq;

namespace JapaneseCrossword
{
	class LineAnalysisResult : ILineAnalysisResult
	{
		public bool[] CanBeFilled { get; private set; }
		public bool[] CanBeEmpty { get; private set; }

		public LineAnalysisResult(bool[] canBeFilled, bool[] canBeEmpty)
		{
			CanBeFilled = canBeFilled;
			CanBeEmpty = canBeEmpty;
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
			return 
				CanBeFilled.SequenceEqual(other.CanBeFilled) && 
				CanBeEmpty.SequenceEqual(other.CanBeEmpty);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((CanBeFilled != null ? CanBeFilled.GetHashCode() : 0) * 397) ^ 
					(CanBeEmpty != null ? CanBeEmpty.GetHashCode() : 0);
			}
		}
	}
}