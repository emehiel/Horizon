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
        public Matrix A { get; set; }

        public Orbital_EOMS(double Mu, Matrix A)
        {
            Mu = 398600.4418;
            A = new Matrix(6);
            A[1, 4] = 1.0;
            A[2, 5] = 1.0;
            A[3, 6] = 1.0;

        }

        public override Matrix this[double t, Matrix y]
        {
            get
            {
                double r3 = Math.Pow(Matrix.Norm(y[1, new MatrixIndex(1, 3)]), 3);
                double mur3 = -Mu / r3;

                A[4, 1] = mur3;
                A[5, 2] = mur3;
                A[6, 3] = mur3;

                Matrix dy = A * y;

                return dy;
            }
        }
    }
}
