using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using HSFSystem;

namespace HSFSubsystem
{
    public class SubsystemFactory
    {
        public void GetSubsystem(XmlNode SubsystemXmlNode, bool enableScripting, Dependencies dependencies, Dictionary<string, Subsystem> subDic)
        {
            string type = SubsystemXmlNode.Attributes["Type"].Value.ToString();
            if (type.Equals("scripted") && enableScripting)
            {
                subDic.Add(type, new ScriptedSubsystem(SubsystemXmlNode, dependencies));
            }
            else // not scripted subsystem
            {
                if (type.Equals("Access"))
                {
                    //Access AccessSub = new Access(SubsystemXmlNode);
                }
                else if (type.Equals("Adcs"))
                {
                   subDic.Add(type, new ADCS(SubsystemXmlNode));
                }
                else if (type.Equals("Power"))
                {
                    subDic.Add(type, new Power(SubsystemXmlNode, dependencies));
                }
            }
           // throw new NotSupportedException("Horizon does not recognize the subsystem: " + type);
        }
    }
}
