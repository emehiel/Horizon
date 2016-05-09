using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissionElements;
using Utilities;

namespace HSFSystem
{
    public class GenericConstraint<T>: Constraint
    {
        T _value;
        StateVarKey<T> _key;
        ConstraintType Type;

        public override bool Accepts(SystemState state)
        {
            // use dynamic in getProfile()

            HSFProfile<T> prof = state.GetProfile(_key);
            
            switch (Type)
            {
                case ConstraintType.FAIL_IF_HIGHER:
                    return ((dynamic)prof.Max() < _value);
                case ConstraintType.FIAL_IF_HIGHER_OR_EQUAL:
                    return ((dynamic)prof.Max() <= _value);
                case ConstraintType.FAIL_IF_LOWER:
                    return ((dynamic)prof.Max() > _value);
                case ConstraintType.FAIL_IF_LOWER_OR_EQUAL:
                    return ((dynamic)prof.Max() >= _value);
                case ConstraintType.FAIL_IF_EQUAL:
                    return ((dynamic)prof.Max() != _value);
                case ConstraintType.FAIL_IF_NOT_EQUAL:
                    return ((dynamic)prof.Max() == _value);

            }
            return true;

        }

        enum ConstraintType { FAIL_IF_HIGHER, FIAL_IF_HIGHER_OR_EQUAL, FAIL_IF_LOWER, FAIL_IF_LOWER_OR_EQUAL, FAIL_IF_NOT_EQUAL, FAIL_IF_EQUAL}
    }

}
