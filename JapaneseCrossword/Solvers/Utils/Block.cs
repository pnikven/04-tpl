namespace JapaneseCrossword.Solvers.Utils
{
	public class Block
	{
		public int Length { get; private set; }
		public int Index { get; private set; }

		public Block(int length, int index)
		{
			Length = length;
			Index = index;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((Block)obj);
		}

		protected bool Equals(Block other)
		{
			return Length == other.Length && Index == other.Index;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Length * 397) ^ Index;
			}
		}

	}
}