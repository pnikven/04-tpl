using System;
using System.IO;
using System.Reflection;
using JapaneseCrossword.Enums.Extensions;
using JapaneseCrossword.Extensions;
using JapaneseCrossword.Solvers;
using JapaneseCrossword.Solvers.Algoritms;
using JapaneseCrossword.Solvers.Algoritms.Interfaces;
using JapaneseCrossword.Solvers.Algoritms.Utils;
using JapaneseCrossword.Solvers.Interfaces;
using JapaneseCrossword.Solvers.Utils;

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
			var lineProvider = new LineProvider();
			var lineAnalyzer = new LineAnalyzer();
			Func<string, string> readFile = inputPath => inputPath.TryReadUtf8FileFromThisPath();
			Func<string, string, bool> writeFile = (outputPath, contents) => outputPath.TryWriteUtf8FileToThisPath(contents);
			ICrosswordSolverAlgorithm multiThreadedSolverAlgorithm = new MultiThreadedIteratedLineAnalysis(lineAnalyzer);
			ICrosswordSolver multiThreadedSolver = new CrosswordSolver(
				crosswordAsPlainText => new Crossword(crosswordAsPlainText),
				lineProvider.GetLines,
				(picture, lines) => multiThreadedSolverAlgorithm.SolveCrossword(picture, lines),
				picture => picture.AsString(),
				readFile,
				writeFile
			);
			var solutionStatus = multiThreadedSolver.Solve(inputFilePath, outputFilePath);
			Console.WriteLine("Done! Solution status: {0}", solutionStatus);
		}
	}
}
