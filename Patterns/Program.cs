namespace Patterns
{
    class Program
    {
        static void Main(string[] args)
        {
            var longRunningTasks = new LongRunningTasks();
            longRunningTasks.LoggingTask();
        }
    }
}
