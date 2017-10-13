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
    public class Quat
    {
        public double _eta { get; set; }
        public Vector _eps { get; set; }

        public Quat()
        {
            _eta = 1;
            _eps = new Vector(3);
        }
        public Quat(double eta, Vector eps)
        {
            _eta = eta;
            _eps = eps;
        }
        public Quat(double eta, double eps1, double eps2, double eps3)
        {
            _eta = eta;
            _eps = new Vector(3);
            _eps[1] = eps1;
            _eps[2] = eps2;
            _eps[3] = eps3;
        }

        public static Quat operator *(Quat q, Quat p)
        {
            double a = q._eta;
            double b = q._eps[1];
            double c = q._eps[2];
            double d = q._eps[3];
            double e = p._eta;
            double f = p._eps[1];
            double g = p._eps[2];
            double h = p._eps[3];
            double eta = e * a - b * f - c * g - d * h;
            double eps1 = a * f + b * e + c * h - d * g;
            double eps2 = a * g + c * e - b * h + d * f;
            double eps3 = a * h + d * e + b * g - c * f;
            return new Quat(eta, eps1, eps2, eps3);
        }

        public override string ToString()
        {
            return "[" + _eta + "," + _eps[1] + ", " + _eps[2] + ",  " + _eps[3] +"]";
        }
    }
}
