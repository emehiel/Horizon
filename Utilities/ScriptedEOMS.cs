using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class ScriptedEOMS : EOMS
    {
        public ScriptedEOMS(string filePath)
        {

        }
        public override Matrix<double> this[double t, Matrix<double> y]
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
