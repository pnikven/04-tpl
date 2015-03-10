namespace JapaneseCrossword
{
	public interface ILineAnalysisResult
	{
		bool[] CanBeFilled { get; }
		bool[] CanBeEmpty { get; }
	}
}