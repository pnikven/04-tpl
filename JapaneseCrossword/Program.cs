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
		static void Main(string[] args)
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
			var solutionStatus = multiThreadedSolver.Solve(inputFilePath, outputFilePath);
			Console.WriteLine("Done! Solution status: {0}", solutionStatus);
		}
	}
}
