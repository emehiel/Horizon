using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissionElements;
using Utilities;
using System.Xml;
using HSFSubsystem;

namespace HSFSystem
{
    public class GenericConstraint<T>: Constraint
    {
        private T _value;
        private StateVarKey<T> _key;
        public ConstraintType Type { get; private set; }

        public GenericConstraint(XmlNode constraintXmlNode, Subsystem sub)
        {
            Subsystem = sub;
            if (constraintXmlNode.ChildNodes[0] == null)
                throw new MissingMemberException("Missing StateVarKey for Constraint!");
            _key = new StateVarKey<T>(constraintXmlNode.ChildNodes[0], constraintXmlNode.ParentNode.Attributes["assetName"].Value.ToString());
            if (constraintXmlNode.Attributes["value"] == null)
                throw new MissingFieldException("Missing Value Field for Constraint!");
            _value = (T)Convert.ChangeType(constraintXmlNode.Attributes["value"].Value, typeof(T));
            if (constraintXmlNode.Attributes["type"] == null)
                throw new MissingFieldException("Missing Type Field for Constraint!");
            Type = (ConstraintType)Enum.Parse(typeof(ConstraintType), constraintXmlNode.Attributes["type"].Value);
        }

        public override bool Accepts(SystemState state)
        {
            // use dynamic in getProfile()

            HSFProfile<T> prof = state.getProfile(_key);
            
            switch (Type)
            {
                case ConstraintType.FAIL_IF_HIGHER:
                    return ((dynamic)prof.Max() < _value);
                case ConstraintType.FAIL_IF_HIGHER_OR_EQUAL:
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

        public enum ConstraintType { FAIL_IF_HIGHER, FAIL_IF_HIGHER_OR_EQUAL, FAIL_IF_LOWER, FAIL_IF_LOWER_OR_EQUAL, FAIL_IF_NOT_EQUAL, FAIL_IF_EQUAL}
    }

}
