using NUnit.Framework;
namespace Dragablz.Dockablz.Tests
{
    [TestFixture]
    public class TilerCalculatorFixture
    {
        [TestCase(0, new int[0])]
        [TestCase(1, new[] { 1 })]
        [TestCase(2, new[] { 1, 1 })]
        [TestCase(3, new[] { 1, 2 })]
        [TestCase(4, new[] { 2, 2 })]
        [TestCase(5, new[] { 2, 3 })]
        [TestCase(6, new[] { 3, 3 })]
        [TestCase(7, new[] { 2, 2, 3 })]
        [TestCase(9, new[] { 3, 3, 3 })]
        [TestCase(10, new[] { 3, 3, 4 })]
        [TestCase(25, new[] { 5, 5, 5, 5, 5 })]
        [TestCase(26, new[] { 5, 5, 5, 5, 6 })]
        [TestCase(29, new[] { 5, 6, 6, 6, 6 })]
        [TestCase(30, new[] { 6, 6, 6, 6, 6 })]
        public void WillCalculateCellsPerColumn(int totalCells, int[] expectedCellsPerColumn)
        {
            var result = TilerCalculator.GetCellCountPerColumn(totalCells);

            CollectionAssert.AreEqual(expectedCellsPerColumn, result);
        }
    }
}