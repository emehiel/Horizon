using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    class StateSpace_EOMS:EOMS
    {
        public override Matrix this[double t, Matrix y]
        {
            get
            {
                return new Matrix((int)y.Size[1, 1], (int)y.Size[1, 2]);
            }
        }
    }
}
