// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Xml;
using HSFUniverse;

namespace MissionElements
{
    [Serializable]
    public class Target
    {
        // The name of the target 
        public string Name { get; private set; }

        // The type of the target 
        public TargetType Type { get; private set; }

        // The position of the target 
        public DynamicState DynamicState { get; private set; }

        // The value of the target 
        public int Value { get; private set; }

        #region Constructors
        public Target(String name, TargetType type, DynamicState dynamicState, int value)
        {
            Name = name;
            Type = type;
            DynamicState = dynamicState;
            Value = value;
        }
        #endregion

        /// <summary>
        /// Creates a new target from the xmlNode data
        /// </summary>
        /// <param name="targetXmlNode"></param>
        public Target(XmlNode targetXmlNode)
        {
            Name = targetXmlNode.Attributes["TargetName"].Value;
            string typeString = targetXmlNode.Attributes["TargetType"].Value.ToString();
            Type = (TargetType)Enum.Parse(typeof(TargetType), typeString);
            DynamicState = new DynamicState(targetXmlNode.ChildNodes.Item(0));
            Value = Convert.ToInt32(targetXmlNode.Attributes["Value"].Value);
        }

        /// <summary>
        /// Override of the Object ToString Method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }

    // Types of targets that HSF supports
    public enum TargetType { FacilityTarget, LocationTarget, FlyingAlong, Recovery }
}
