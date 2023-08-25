// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    [Serializable]
    public class IntegratorOptions
    {
        #region Properties

        /** initial guess for h - delta t */
        public double h { get; set; }

        /** relative tolerance used by the integrator solver. */
        public double rtol { get; set; }

        /** absolute tolerance used by the integrator solver, (important when solution is close to zero). */
        public double atol { get; set; }

        /** a small number */
        public double eps { get; set; }

        /** integrator time step **/
        public int nSteps { get; set; }

        #endregion

        /**
        * Creates a new integratorArgs object with default values:
        * h = 0.1
        * rtol = 1e-3
        * atol = 1e-6
        * eps = double.Epsilon;
        * nSteps = 100
        */
        public IntegratorOptions()
        {
            h = 0.1;
            rtol = 0.001;
            atol = 0.000001;
            eps = double.Epsilon;
            nSteps = 100;
        }

    }
}
