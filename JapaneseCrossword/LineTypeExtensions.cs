namespace JapaneseCrossword
{
	public static class LineTypeExtensions
	{
		public static LineType Reverse(this LineType lineType)
		{
			return lineType == LineType.Row ? LineType.Column : LineType.Row;
		}
	}
}