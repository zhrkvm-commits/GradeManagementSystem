using System.Collections.Generic;
using NUnit.Framework;
using GradeManagementSystem.Helpers;

namespace GradeManagementSystem.Tests
{
    [TestFixture]
    public class MathStatisticsTests
    {
        [Test]
        public void CalculateAverage_KnownGrades_ReturnsCorrectValue()
        {
            List<int> grades = new List<int> { 5, 4, 3, 4, 5 };
            Assert.AreEqual(4.2, MathStatistics.CalculateAverage(grades), 0.001);
        }

        [Test]
        public void CalculateVariance_IdenticalGrades_ReturnsZero()
        {
            List<int> grades = new List<int> { 4, 4, 4, 4 };
            Assert.AreEqual(0, MathStatistics.CalculateVariance(grades), 0.001);
        }

        [Test]
        public void CalculateVariance_KnownGrades_ReturnsCorrectValue()
        {
            List<int> grades = new List<int> { 2, 4, 4, 4, 5, 5, 7, 9 };
            Assert.AreEqual(4.0, MathStatistics.CalculateVariance(grades), 0.001);
        }

        [Test]
        public void CalculateMedian_OddCount_ReturnsMiddleElement()
        {
            List<int> grades = new List<int> { 5, 3, 4 };
            Assert.AreEqual(4, MathStatistics.CalculateMedian(grades), 0.001);
        }

        [Test]
        public void CalculateMedian_EvenCount_ReturnsAverageOfTwoMiddle()
        {
            List<int> grades = new List<int> { 3, 4, 4, 5 };
            Assert.AreEqual(4, MathStatistics.CalculateMedian(grades), 0.001);
        }
        [Test]
        public void CalculateAverage_EmptyList_ReturnsZero()
        {
            Assert.AreEqual(0, MathStatistics.CalculateAverage(new List<int>()));
        }
        [Test]
        public void CalculateVariance_NullList_ReturnsZero()
        {
            Assert.AreEqual(0, MathStatistics.CalculateVariance(null));
        }
    }
}
