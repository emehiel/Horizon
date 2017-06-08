// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    [Serializable]
    public class OrbitalEOMS: EOMS
    {
        protected double _mu;
        protected Matrix<double> _A;

        public OrbitalEOMS()
        {
            _mu = 398600.4418;
            _A = new Matrix<double>(6);
            _A[1, 4] = 1.0;
            _A[2, 5] = 1.0;
            _A[3, 6] = 1.0;

        }

        public override Matrix<double> this[double t, Matrix<double> y, IntegratorParameters param]
        {
            get
            {
                double r3 = System.Math.Pow(Matrix<double>.Norm(y[new MatrixIndex(1, 3), 1]), 3);
                double mur3 = -_mu / r3;

                _A[4, 1] = mur3;
                _A[5, 2] = mur3;
                _A[6, 3] = mur3;

                Matrix<double> dy = _A * y;

                return dy;
            }
        }
    }
}
