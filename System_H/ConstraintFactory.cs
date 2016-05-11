using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HSFSubsystem;
using Utilities;
using MissionElements;

namespace HSFSystem
{
    public class ConstraintFactory
    {
        public static Constraint getConstraint(XmlNode constraintXmlNode, Dictionary<string, Subsystem> subsystemMap, Asset asset)
        {
            Subsystem constrainedSub = null;
            string subName = Subsystem.parseNameFromXmlNode(constraintXmlNode, asset.Name);
            subsystemMap.TryGetValue(subName, out constrainedSub);
            if (constrainedSub == null)
                throw new MissingMemberException("Missing Subsystem Name in Constraint");
            string type = constraintXmlNode.ChildNodes[0].Attributes["type"].Value.ToLower();
            if (type.Equals("int"))
                return new GenericConstraint<int>(constraintXmlNode, constrainedSub);
            else if (type.Equals("double"))
                return new GenericConstraint<double>(constraintXmlNode, constrainedSub);
            else if (type.Equals("bool"))
                return new GenericConstraint<bool>(constraintXmlNode, constrainedSub);
            else if (type.Equals("Matrix"))
                return new GenericConstraint<Matrix<double>>(constraintXmlNode,constrainedSub);
            else
                throw new NotSupportedException("Unsupported type of constraint!");
        }
    }
}
