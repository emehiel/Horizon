using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class StateVarKey<T>
    {
        private string _varName { get; set; }

        public StateVarKey(string varName)
        {
            _varName = varName;

        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            StateVarKey p = obj as StateVarKey;
            return _varName == p._varName;

        }

        public static bool operator ==(StateVarKey p1, StateVarKey p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(StateVarKey p1, StateVarKey p2)
        {
            return !(p1 == p2);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            return -_varName.GetHashCode();
        }
    }
}
