using System.Linq;
using System.Threading.Tasks;
using JapaneseCrossword.Enums;
using JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces;
using JapaneseCrossword.Solvers.Utils.Enums;
using JapaneseCrossword.Solvers.Utils.Interfaces;

namespace JapaneseCrossword.Solvers.Algoritms
{
	public class MultiThreadedIteratedLineAnalysis : IteratedLineAnalysis
	{
		public MultiThreadedIteratedLineAnalysis(ILineAnalyzer lineAnalyzer)
			: base(lineAnalyzer)
		{
		}

		public override CellState[,] SolveCrossword(CellState[,] sourcePicture, ILine[] lines)
		{
			var picture = new CellState[sourcePicture.GetLength(0), sourcePicture.GetLength(1)];
			while (true)
			{
				var invalidLines = GetInvalidLines(lines);
				if (!invalidLines.Any())
					break;
				AnalyzeInvalidLinesOfTheSameType(LineType.Row, invalidLines, lines, picture);
				AnalyzeInvalidLinesOfTheSameType(LineType.Column, invalidLines, lines, picture);
			}
			return picture;
		}

		private void AnalyzeInvalidLinesOfTheSameType(LineType lineType, ILine[] invalidLines, ILine[] allLines, CellState[,] picture)
		{
			var tasks = invalidLines
				.Where(line => line.Type == lineType)
				.Select(line => Task.Factory.StartNew(() => AnalyzeLine(allLines, line, picture)))
				.ToArray();
			Task.WaitAll(tasks);
		}
	}
}