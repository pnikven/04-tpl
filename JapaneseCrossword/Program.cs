using System;
using System.IO;
using System.Reflection;
using JapaneseCrossword.Solvers;
using JapaneseCrossword.Solvers.Algoritms;
using JapaneseCrossword.Solvers.Algoritms.Interfaces;
using JapaneseCrossword.Solvers.Algoritms.Utils;
using JapaneseCrossword.Solvers.Interfaces;

namespace JapaneseCrossword
{
	class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				Run(args);
			}
			catch
			{
				Environment.Exit(0);
			}
		}

		private static void Run(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine("Usage: {0} inputFilePath outputFilePath",
					Path.GetFileName(Assembly.GetExecutingAssembly().Location));
				return;
			}
			var inputFilePath = args[0];
			var outputFilePath = args[1];
			var lineAnalyzer = new LineAnalyzer();
			ICrosswordSolverAlgorithm multiThreadedSolverAlgorithm = new MultiThreadedIteratedLineAnalysis(lineAnalyzer);
			ICrosswordSolver multiThreadedSolver = new CrosswordSolver(multiThreadedSolverAlgorithm);
			try
			{
				var solutionStatus = multiThreadedSolver.Solve(inputFilePath, outputFilePath);
				Console.WriteLine("Done! Solution status: {0}", solutionStatus);
			}
			catch
			{
				Console.WriteLine("Sorry, an error occurred");
			}
		}
	}
}
