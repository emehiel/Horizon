using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    /// <summary>
    /// An equation of motion object with the parameters governing some
    /// sort of motion of an object.
    /// @author Cory O'Connor
    /// </summary>
    [Serializable]
    public abstract class EOMS
    {
        /// <summary>
        /// Computes one timestep of the equations of motion object from a specified
		/// state after a specified period of time
        /// </summary>
        /// <param name="t">the time at which to evaluate the function</param>
        /// <param name="y">the current state of the object</param>
        /// <returns>dy the matrix result</returns>
        public abstract Matrix<double> this[double t, Matrix<double> y]
        {
            get;
        }
        public virtual Matrix<double> PythonAccessor(double t, Matrix<double> y)
        {
            return this[t, y];
        }
    }
}
