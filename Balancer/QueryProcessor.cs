namespace Balancer
{
	class QueryProcessor
	{
		public static string Process(string query)
		{
			return string.Format("{0}&processed=true", query);
		}
	}
}
