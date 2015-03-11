namespace JapaneseCrossword
{
	public static class LineTypeExtensions
	{
		public static LineType Reverse(this LineType lineType)
		{
			return lineType == LineType.Row ? LineType.Column : LineType.Row;
		}

		public static bool IsRow(this LineType lineType)
		{
			return lineType == LineType.Row;
		}

		public static bool IsColumn(this LineType lineType)
		{
			return lineType == LineType.Column;
		}
	}
}