// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Xml;

namespace Utilities
{
    [Serializable]
    public class StateVariableKey<T>
    {
        #region Attributes
        public string VariableName { get; set; }
        #endregion

        #region Constructors
        public StateVariableKey(string varName)
        {
            VariableName = varName.ToLower();
        }

        public StateVariableKey(XmlNode varXmlNode, string assetName)
        {
            if (varXmlNode.Attributes["key"] == null)
                throw new MissingMemberException("Missing key field in constraint!");
            VariableName = assetName.ToLower() + "." + varXmlNode.Attributes["key"].Value.ToString().ToLower();
        }
        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{VariableName} of type {typeof(T).ToString()}";
        }
        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            StateVariableKey<T> p = obj as StateVariableKey<T>;
            return VariableName.Equals(p.VariableName);

        }

        public static implicit operator StateVariableKey<int>(StateVariableKey<T> i)
        {
            return new StateVariableKey<int>(i.VariableName);
        }

        public static implicit operator StateVariableKey<double>(StateVariableKey<T> i)
        {
            return new StateVariableKey<double>(i.VariableName);
        }

        public static implicit operator StateVariableKey<bool>(StateVariableKey<T> i)
        {
            return new StateVariableKey<bool>(i.VariableName);
        }

        public static implicit operator StateVariableKey<Matrix<double>>(StateVariableKey<T> i)
        {
            return new StateVariableKey<Matrix<double>>(i.VariableName);
        }

        public static implicit operator StateVariableKey<Quaternion>(StateVariableKey<T> i)
        {
            return new StateVariableKey<Quaternion>(i.VariableName);
        }

        public static bool operator ==(StateVariableKey<T> p1, StateVariableKey<T> p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(StateVariableKey<T> p1, StateVariableKey<T> p2)
        {
            return !(p1 == p2);
        }
        
        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: write your implementation of GetHashCode() here
            return -VariableName.GetHashCode();
        }
        #endregion
        #region Methods
        public StateVariableKey<T> DeepClone()
        {
            return new StateVariableKey<T>(VariableName);
        }
        #endregion
    }
}