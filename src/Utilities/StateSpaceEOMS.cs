// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class StateSpaceEOMS:EOMS
    {
        public override Matrix<double> this[double t, Matrix<double> y, IntegratorParameters param]
        {
            get
            {
                Matrix<double> dy = new Matrix<double>(y.Size[1], y.Size[2]);
                dy[1,1] = y[2,1];
                dy[2,1] = -1 * y[2,1] - 4 * y[1,1];

                return dy;
            }
        }
    }
}
