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
	}
}