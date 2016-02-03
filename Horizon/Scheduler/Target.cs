using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Horizon.Universe;

namespace System_H
{
    /**
    * A named location denoting where a task is to be performed.
    * @author Einar Pehrson
    */
    class Target
    {
        /** The name of the target */
        private string _name { get; set; }

        /** The type of the target */
        private string _targetType { get; set; }

        /** The position of the target */
        protected DynamicState _dynamicState { get; set; }

        /** The value of the target */
        protected int _value { get; set; }

        protected int minQualCM;

        protected int freq_days;
        protected string CC;
        protected string WX_Reg;
        protected string Comment { get }

        public Target(String name, TargetType type, DynamicState dynamicState, int value)
        {
            _Name = name;
            _TargetType = type;
            _DynamicState = dynamicState;
            _Value = value;
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
