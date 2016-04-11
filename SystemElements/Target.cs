using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HSFUniverse;


namespace MissionElements
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
        public TargetType Type { get; private set; }

        /** The position of the target */
        public DynamicState DynamicState { get; private set; }

        /** The value of the target */
        public int Value { get; private set; }

        private int MinQualCM;

        protected int Freq_days;
        protected string CC;
        protected string WX_Reg;
        protected string Comment { get; }

        public Target(String name, TargetType type, DynamicState dynamicState, int value)
        {
            Name = name;
            Type = type;
            DynamicState = dynamicState;
            Value = value;
        }

        /**
        * Creates a new target from the xmlNode data
        * @param targetNode the xmlNode which contains the relevant target information
        */
        public Target(XmlNode targetXmlNode)
        {
            Name = targetXmlNode.Attributes["TargetName"].Value;
            string typeString = targetXmlNode.Attributes["TargetType"].Value;
            Type = (TargetType)Enum.Parse(typeof(TargetType), typeString);
            DynamicState = new DynamicState(targetXmlNode["DynamicState"]);
            Value = Convert.ToInt32(targetXmlNode.Attributes["Value"].Value);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public enum TargetType { FacilityTarget, LocationTarget }
}
