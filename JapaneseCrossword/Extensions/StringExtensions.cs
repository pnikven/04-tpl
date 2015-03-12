using System;
using System.IO;
using System.Linq;
using System.Text;
using JapaneseCrossword.Enums;
using MoreLinq;

namespace JapaneseCrossword.Extensions
{
	// Вообще, расширения - очень удобный инструмент, но важно не путать методы, которые действительно расширяют функционал базового класса
	// и методы, которые просто принимают в качестве аргумента единственный параметр определенного типа.
	// В данном классе ни один метод не имеет отношения к строкам в общем, все они должны быть реализованы в конкретных классах, которые 
	// знают что-то про контекст текущей задачи.
	// Хорошая статья на тему: http://blogs.msdn.com/b/vbteam/archive/2007/03/10/extension-methods-best-practices-extension-methods-part-6.aspx
	public static class StringExtensions
	{
		public static CellState[] ToCellStateLine(this string s)
		{
			return s
				.Select(c => c.ToCellState())
				.ToArray();
		}

		public static CellState[,] ToCellStateMatrix(this string crosswordAsPlainText)
		{
			var rows = crosswordAsPlainText
				.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			var rowCount = rows.Length;
			var colCount = rows.First().Length;
			var i = 0;
			var j = 0;
			var result = new CellState[rowCount, colCount];
			rows.ForEach(row =>
			{
				row.ToCharArray().ForEach(c => result[i, j++] = c.ToCellState());
				j = 0;
				i++;
			});
			return result;
		}

		public static string TryReadUtf8FileFromThisPath(this string filePath)
		{
			if (!File.Exists(filePath))
				return null;
			try
			{
				return File.ReadAllText(filePath, Encoding.UTF8);
			}
			catch
			{
				return null;
			}
		}

		public static bool TryWriteUtf8FileToThisPath(this string filePath, string contents)
		{
			if (Path.GetInvalidFileNameChars().Any(filePath.Contains))
				return false;
			try
			{
				File.WriteAllText(filePath, contents, Encoding.UTF8);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}