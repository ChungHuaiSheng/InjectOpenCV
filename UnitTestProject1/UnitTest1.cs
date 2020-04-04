using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Threading;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            CultureInfo original = Thread.CurrentThread.CurrentCulture;

            // This example uses strings representing numbers in the US locale
            // so change the current culture info.  For example, "0.446"
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            // Calculate the slope and intercept of the linear least squares fit
            // through the five points:
            // (20, .446) (30, .601), (40, .786), (50, .928), (60, .950)
            var A = new DoubleMatrix("5x1[20.0  30.0  40.0  50.0  60.0]");
            var y = new DoubleVector("[0.446 0.601 0.786 0.928 0.950]");

            // Back to your original culture.
            Thread.CurrentThread.CurrentCulture = original;

            // We want our straight line to be of the form y = mx + b, where b is
            // not necessarily equal to zero. Thus we will set the third 
            // constructor argument to true so that we calculate the intercept
            // parameter. 
            var lsq = new DoubleLeastSquares(A, y, true);

            Console.WriteLine();

            Console.WriteLine("Y-intercept = {0}", lsq.X[0]);
            Console.WriteLine("Slope = {0}", lsq.X[1]);

            // We can look at the residuals which are the difference between the 
            // actual value of y at a point x, and the corresponding point y on 
            // line for the same x.
            Console.WriteLine("Residuals = {0}", lsq.Residuals.ToString("F3"));

            // Finally, we can look at the residual sum of squares, which is the
            // sum of the squares of the elements in the residual vector.
            Console.WriteLine("Residual Sum of Squares (RSS) = {0}", lsq.ResidualSumOfSquares.ToString("F3"));

            // The least squares class can also be used to solve "rank-deficient" least 
            // square problems:
            A = new DoubleMatrix("6x4 [0 9 -6 3  -3 0 -3 0  1 3 -1 1  1 3 -1 1  -2 0 -2 0  3 6 -1 2]");
            y = new DoubleVector("[-3 5 -2 2 1 -2]");

            // For this problem we will specify a tolerance for computing the effective rank
            // of the matrix A, and we will not have the class add an intercept parameter
            // for us.
            lsq = new DoubleLeastSquares(A, y, 1e-10);
            Console.WriteLine("Least squares solution = {0}", lsq.X.ToString("F3"));
            Console.WriteLine("Rank computed using a tolerance of {0}, = {1}",
              lsq.Tolerance, lsq.Rank);

            // You can even use the least squares class to solve under-determined systems
            // (the case where A has more columns than rows).
            A = new DoubleMatrix("6x4 [-3 -1 6 -5  5 4 -6 8  7 5 0 -4  -7 4 0 3  -7 7 -8 2  3 4 2 -4]");
            y = new DoubleVector("[-3 1 8 -2]");
            lsq = new DoubleLeastSquares(A.Transpose(), y, 1e-8);
            Console.WriteLine("Solution to under-determined system = {0}", lsq.X.ToString("F3"));
            Console.WriteLine("Rank computed using a tolerance of {0}, = {1}",
              lsq.Tolerance, lsq.Rank);

            Console.WriteLine();
            Console.WriteLine("Press Enter Key");
            Console.Read();
        }
    }
}
