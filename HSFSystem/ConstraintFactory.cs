// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Xml;
using Utilities;
using MissionElements;

namespace HSFSystem
{
    public class ConstraintFactory
    {
        /// <summary>
        /// Constraint generator to translate XML nodes to actual constraints
        /// </summary>
        /// <param name="constraintXmlNode"></param>
        /// <param name="subList"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static Constraint GetConstraint(XmlNode constraintXmlNode, List<Subsystem> subList, Asset asset)
        {
            Subsystem constrainedSub = null;
            string subName = Subsystem.parseNameFromXmlNode(constraintXmlNode, asset.Name);
            constrainedSub = subList.Find(s => s.Name == subName);
            if (constrainedSub == null)
                throw new MissingMemberException("Missing Subsystem Name in Constraint");

            string type = constraintXmlNode["STATEVAR"].Attributes["type"].Value.ToLower();

            if (type.Equals("int"))
                return new SingleConstraint<int>(constraintXmlNode, constrainedSub);
            else if (type.Equals("double"))
                return new SingleConstraint<double>(constraintXmlNode, constrainedSub);
            else if (type.Equals("bool"))
                return new SingleConstraint<bool>(constraintXmlNode, constrainedSub);
            else if (type.Equals("Matrix"))
                return new SingleConstraint<Matrix<double>>(constraintXmlNode,constrainedSub);
            else //TODO: Add functionality to create scripted constraints
                throw new NotSupportedException("Unsupported type of constraint!");
        }
    }
}
