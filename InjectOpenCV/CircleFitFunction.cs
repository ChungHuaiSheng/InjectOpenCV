using CenterSpace.NMath.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectOpenCV
{
    public class CircleFitFunction : DoubleMultiVariableFunction
    {
        public CircleFitFunction(DoubleVector x, DoubleVector y) : base(3, x.Length)
        {
            if (x.Length != y.Length)
                throw new Exception("Unequal number of x,y values.");

            X = x;
            Y = y;
        }

        public DoubleVector X { get; internal set; }
        public DoubleVector Y { get; internal set; }

        public override void Evaluate(DoubleVector parameters, ref DoubleVector residuals)
        {
            // parameters of circle with center (a,b) and radius r
            double a = parameters[0];
            double b = parameters[1];
            double r = parameters[2];

            for (int i = 0; i < X.Length; i++)
            {
                // distance of point from circle center
                double d = Math.Sqrt(Math.Pow(X[i] - a, 2.0) + Math.Pow(Y[i] - b, 2.0));

                residuals[i] = d - r;
            }
        }
    }
}
