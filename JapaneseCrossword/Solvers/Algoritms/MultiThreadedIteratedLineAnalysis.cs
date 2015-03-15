using System.Linq;
using System.Threading.Tasks;
using JapaneseCrossword.Interfaces;
using JapaneseCrossword.Solvers.Algoritms.Utils.Interfaces;
using JapaneseCrossword.Solvers.Utils;
using JapaneseCrossword.Solvers.Utils.Enums;

namespace JapaneseCrossword.Solvers.Algoritms
{
	public class MultiThreadedIteratedLineAnalysis : IteratedLineAnalysis
	{
		public MultiThreadedIteratedLineAnalysis(ILineAnalyzer lineAnalyzer)
			: base(lineAnalyzer)
		{
		}

		protected override Cell[,] SolveCrosswordUnsafe(ICrosswordDescription crosswordDescription)
		{
			var picture = CreatePicture(crosswordDescription.RowCount, crosswordDescription.ColumnCount);
			var lines = CreateLinesFromCrosswordDescription(crosswordDescription).ToArray();
			while (true)
			{
				var invalidLines = GetLinesToBeRefreshed(lines);
				if (!invalidLines.Any())
					break;
				AnalyzeInvalidLinesOfTheSameType(LineType.Row, invalidLines, lines, picture);
				AnalyzeInvalidLinesOfTheSameType(LineType.Column, invalidLines, lines, picture);
			}
			return picture;
		}

		private void AnalyzeInvalidLinesOfTheSameType(LineType lineType, Line[] invalidLines, Line[] allLines, Cell[,] picture)
		{
			var tasks = invalidLines
				.Where(line => line.Type == lineType)
				.Select(line => Task.Factory.StartNew(() => AnalyzeLine(allLines, line, picture)))
				.ToArray();
			Task.WaitAll(tasks);
		}
	}
}