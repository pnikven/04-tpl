using System;
using JapaneseCrossword.Solvers.Utils;
using JapaneseCrossword.Solvers.Utils.Enums;
using NUnit.Framework;

namespace JapaneseCrosswordTests
{
	[TestFixture]
	public class CellTests
	{
		[Test]
		public void Create_wrongSymbol_Throws()
		{
			Assert.Throws(typeof(Exception), () => Cell.CreateCellsFromString("#"));
		}

		[TestCase(CellState.Unknown, '?')]
		[TestCase(CellState.Filled, '*')]
		[TestCase(CellState.Empty, '.')]
		public void Create_correctSymbol_ReturnsCorrespondingCell(CellState cellState, char symbol)
		{
			Assert.AreEqual(new Cell(cellState), Cell.Create(symbol));
		}		
	}
}