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
        public string VarName { get; set; }
        #endregion

        #region Constructors
        public StateVariableKey(string varName)
        {
            VarName = varName.ToLower();
        }

        public StateVariableKey(XmlNode varXmlNode, string assetName)
        {
            if (varXmlNode.Attributes["key"] == null)
                throw new MissingMemberException("Missing key field in constraint!");
            VarName = assetName.ToLower() + "." + varXmlNode.Attributes["key"].Value.ToString().ToLower();
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

            StateVariableKey<T> p = obj as StateVariableKey<T>;
            return VarName.Equals(p.VarName);

        }
        
        public static implicit operator StateVariableKey<int>(StateVariableKey<T> i)
        {
            return new StateVariableKey<int>(i.VarName);
        }

        public static implicit operator StateVariableKey<double>(StateVariableKey<T> i)
        {
            return new StateVariableKey<double>(i.VarName);
        }

        public static implicit operator StateVariableKey<bool>(StateVariableKey<T> i)
        {
            return new StateVariableKey<bool>(i.VarName);
        }

        public static implicit operator StateVariableKey<Matrix<double>>(StateVariableKey<T> i)
        {
            return new StateVariableKey<Matrix<double>>(i.VarName);
        }

        public static implicit operator StateVariableKey<Quaternion>(StateVariableKey<T> i)
        {
            return new StateVariableKey<Quaternion>(i.VarName);
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
            return -VarName.GetHashCode();
        }
        #endregion
        #region Methods
        public StateVariableKey<T> DeepClone()
        {
            return new StateVariableKey<T>(VarName);
        }
        #endregion
    }
}