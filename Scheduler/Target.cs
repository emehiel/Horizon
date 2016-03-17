using System;
using System.Collections.Generic;
using System.Xml;
using HSFUniverse;

namespace HSFScheduler
{
    /**
    * A named location denoting where a task is to be performed.
    * @author Einar Pehrson
    */
    public class Target
    {
        /** The name of the target */
        public string Name { get; private set; }

        /** The type of the target */
        public string TargetType { get; private set; }

        /** The position of the target */
        protected DynamicState CurrentDynamicState { get; private set; }

        /** The value of the target */
        protected int Value { get; private set; }

        protected int MinQualCM;

        protected int Freq_days;
        protected string CC;
        protected string WX_Reg;
        protected string Comment { get; }

        public Target(String name, string type, DynamicState dynamicState, int value)
        {
            Name = name;
            TargetType = type;
            CurrentDynamicState = dynamicState;
            Value = value;
        }

        /**
        * Creates a new target from the xmlNode data
        * @param targetNode the xmlNode which contains the relevant target information
        */
        public Target(XmlNode targetXmlNode)
        {

        }

    }

    public enum TargetType { FACILITY, LOCATION }
}
