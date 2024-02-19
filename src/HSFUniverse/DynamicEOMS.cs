using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Newtonsoft.Json;

namespace HSFUniverse
{
    [Serializable]
    public abstract class DynamicEOMS : EOMS
    {
        public Domain environment { get; set; }

        public DynamicEOMS()
        {
        }

        /// <summary>
        /// EOMS from Utilities calls onto this method. The method here includes the environment
        /// to be used by DynamicEOMS subclasses.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="y"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public override Matrix<double> this[double t, Matrix<double> y, IntegratorParameters param]
        {
            get
            {
                return this[t, y, param, environment];
            }
        }

        public abstract Matrix<double> this[double t, Matrix<double> y, IntegratorParameters param, Domain environment]
        {
            get;
        }

        public override Matrix<double> PythonAccessor(double t, Matrix<double> y, IntegratorParameters param)
        {
            return this[t, y, param, environment];
        }

        public void SetEnvironment(Domain environment)
        {
            this.environment = environment;
        }
    }
}
