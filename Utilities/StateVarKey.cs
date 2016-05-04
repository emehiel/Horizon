using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class StateVarKey<T>
    {
        #region Attributes
        public string VarName { get; set; }
        #endregion

        #region Constructors
        public StateVarKey(string varName)
        {
            VarName = varName;
        }
        #endregion

        #region Overrides
        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            StateVarKey<T> p = obj as StateVarKey<T>;
            return VarName == p.VarName;

        }

        public static bool operator ==(StateVarKey<T> p1, StateVarKey<T> p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(StateVarKey<T> p1, StateVarKey<T> p2)
        {
            return !(p1 == p2);
        }
        
        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            return -VarName.GetHashCode();
        }
        #endregion
    }
}