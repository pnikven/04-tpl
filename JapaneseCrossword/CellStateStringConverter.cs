using System;
using System.IO;
using System.Linq;
using System.Text;
using MoreLinq;

namespace JapaneseCrossword
{
	static class CellStateStringConverter
	{
		public static CellState ConvertCharToCellState(char c)
		{
			switch (c)
			{
				case '.':
					return CellState.Empty;
				case '*':
					return CellState.Filled;
				case '?':
					return CellState.Unknown;
				default:
					throw new ArgumentOutOfRangeException(string.Format("Unknown char {0}", c));
			}
		}

		public static CellState[] ConvertStringToCells(string s)
		{
			return s.Select(ConvertCharToCellState).ToArray();
		}

		public static string ConvertCellsToString(CellState[] cells)
		{
			return string.Join("", cells.Select(ConvertCellStateToChar));
		}

		public static char ConvertCellStateToChar(CellState cellState)
		{
			switch (cellState)
			{
				case CellState.Unknown:
					return '?';
				case CellState.Empty:
					return '.';
				case CellState.Filled:
					return '*';
				default:
					throw new ArgumentOutOfRangeException("cellState");
			}
		}

		public static string ConvertPictureToString(CellState[,] picture)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < picture.GetLength(0); i++)
			{
				sb.Append(string.Format("{0}:", i.ToString().PadLeft(2)));
				for (var j = 0; j < picture.GetLength(1); j++)
					sb.Append(ConvertCellStateToChar(picture[i, j]));
				sb.AppendLine();
			}
			return sb.ToString();
		}

		public static CellState[,] ConvertPictureToCellStateArray(string picturePath)
		{
			var rows = File.ReadAllLines(picturePath);
			var rowCount = rows.Length;
			var colCount = rows.First().Length;
			var i = 0;
			var j = 0;
			var result = new CellState[rowCount, colCount];
			rows.ForEach(row =>
			{
				row.ToCharArray().ForEach(c => result[i, j++] = CellStateStringConverter.ConvertCharToCellState(c));
				j = 0;
				i++;
			});
			return result;
		}
	}
}
