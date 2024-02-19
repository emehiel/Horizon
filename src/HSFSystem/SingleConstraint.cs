// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using MissionElements;
using Utilities;
using System.Xml;
using Newtonsoft.Json.Linq;
using UserModel;

namespace HSFSystem
{
    public class SingleConstraint<T>: Constraint
    {
        private T _value;
        private StateVariableKey<T> _key;
        public ConstraintType Type { get; private set; }

        public SingleConstraint(JObject constraintJson, Subsystem sub)
        {
            Subsystems = new List<Subsystem> { sub };

            if (!JsonLoader<string>.TryGetValue("name", constraintJson, out Name))
                throw new ArgumentException($"Missing constraint name");

            _key = new StateVariableKey<T>(sub.Asset.Name + "." + constraintJson["state"]["key"]);
            if (!JsonLoader<double>.TryGetValue("value", constraintJson, out _value))
                throw new ArgumentException($"Missing constraint value");

            if (JsonLoader<string>.TryGetValue("type", constraintJson, out string type))
            {
                Enum.TryParse(type, out ConstraintType temp);
                Type = temp;
            }
            else
                throw new ArgumentException($"Missing constraint type");

        }
        /// <summary>
        /// Constraint to check a single value of the state of a group or single subsystem
        /// </summary>
        /// <param name="constraintXmlNode"></param>
        /// <param name="sub"></param>
        public SingleConstraint(XmlNode constraintXmlNode, Subsystem sub)
        {
            Subsystems = new List<Subsystem>() { sub };

            if (constraintXmlNode.ChildNodes[0] == null)
                throw new MissingMemberException("Missing StateVarKey for Constraint!");
            
            _key = new StateVariableKey<T>(constraintXmlNode.ChildNodes[0], constraintXmlNode.ParentNode.Attributes["assetName"].Value.ToString());
            
            if (constraintXmlNode.Attributes["value"] == null)
                throw new MissingFieldException("Missing Value Field for Constraint!");
            
            _value = (T)Convert.ChangeType(constraintXmlNode.Attributes["value"].Value, typeof(T));
            
            if (constraintXmlNode.Attributes["type"] == null)
                throw new MissingFieldException("Missing Type Field for Constraint!");
            
            Type = (ConstraintType)Enum.Parse(typeof(ConstraintType), constraintXmlNode.Attributes["type"].Value);
            
            if (constraintXmlNode.Attributes["subsystemName"] == null)
                throw new MissingMemberException("Missing Constraint Name");
            
            Name = constraintXmlNode.Attributes["subsystemName"].Value.ToLower();
        }

        public override bool Accepts(SystemState state) //fix this to be a dependency function
        {
            HSFProfile<double> prof = state.GetProfile((StateVariableKey<double>)_key);
            //TODO try catch
            switch (Type)
            {
                case ConstraintType.FAIL_IF_HIGHER:
                    return ((dynamic)prof.Max() <= _value);
                case ConstraintType.FAIL_IF_HIGHER_OR_EQUAL:
                    return ((dynamic)prof.Max() < _value);
                case ConstraintType.FAIL_IF_LOWER:
                    return ((dynamic)prof.Max() >= _value);
                case ConstraintType.FAIL_IF_LOWER_OR_EQUAL:
                    return ((dynamic)prof.Max() > _value);
                case ConstraintType.FAIL_IF_EQUAL:
                    return ((dynamic)prof.Max() != _value);
                case ConstraintType.FAIL_IF_NOT_EQUAL:
                    return ((dynamic)prof.Max() == _value);

            }
            return true;
        }

        // The types of constraints supported by HSF
        public enum ConstraintType { FAIL_IF_HIGHER, FAIL_IF_HIGHER_OR_EQUAL, FAIL_IF_LOWER, FAIL_IF_LOWER_OR_EQUAL, FAIL_IF_NOT_EQUAL, FAIL_IF_EQUAL}
    }

}
