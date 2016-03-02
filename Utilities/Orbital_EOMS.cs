using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    class Orbital_EOMS: EOMS
    {
        public double Mu { get; set; }
        public Matrix<double> A { get; set; }

        public Orbital_EOMS(double Mu, Matrix<double> A)
        {
            Mu = 398600.4418;
            A = new Matrix<double>(6);
            A[1, 4] = 1.0;
            A[2, 5] = 1.0;
            A[3, 6] = 1.0;

        }

        public override Matrix<double> this[double t, Matrix<double> y]
        {
            get
            {
                double r3 = System.Math.Pow(Matrix<double>.Norm(y[1, new MatrixIndex(1, 3)]), 3);
                double mur3 = -Mu / r3;

                A[4, 1] = mur3;
                A[5, 2] = mur3;
                A[6, 3] = mur3;

                Matrix<double> dy = A * y;

                return dy;
            }
        }
    }
}
