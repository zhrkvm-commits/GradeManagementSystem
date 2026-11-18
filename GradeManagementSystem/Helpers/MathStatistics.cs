using System;
using System.Collections.Generic;
using System.Linq;

namespace GradeManagementSystem.Helpers
{
    public static class MathStatistics
    {
        public static double CalculateAverage(List<int> grades)
        {
            if (grades == null || grades.Count == 0)
                return 0;

            return grades.Average();
        }

        public static double CalculateVariance(List<int> grades)
        {
            if (grades == null || grades.Count == 0)
                return 0;

            double average = CalculateAverage(grades);
            double sumOfSquares = grades.Sum(grade => Math.Pow(grade - average, 2));
            return sumOfSquares / grades.Count;
        }

        public static double CalculateMedian(List<int> grades)
        {
            if (grades == null || grades.Count == 0)
                return 0;

            List<int> sortedGrades = grades.OrderBy(g => g).ToList();
            int count = sortedGrades.Count;
            int middle = count / 2;

            if (count % 2 == 0)
            {
                return (sortedGrades[middle - 1] + sortedGrades[middle]) / 2.0;
            }

            return sortedGrades[middle];
        }

        public static double CalculateStandardDeviation(List<int> grades)
        {
            double variance = CalculateVariance(grades);
            return Math.Sqrt(variance);
        }
    }
}
