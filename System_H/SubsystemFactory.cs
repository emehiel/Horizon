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
        public void GetSubsystem(XmlNode SubsystemXmlNode, bool enableScripting, Dependencies dependencies, Asset asset, Dictionary<string, Subsystem> subDic)
        {
            string type = SubsystemXmlNode.Attributes["Type"].Value.ToString().ToLower();
            if (type.Equals("scripted") && enableScripting)
            {
                subDic.Add(type, new ScriptedSubsystem(SubsystemXmlNode, dependencies));
            }
            else // not scripted subsystem
            {
                if (type.Equals("access"))
                {
                    subDic.Add(type, new AccessSub(SubsystemXmlNode));
                }
                else if (type.Equals("adcs"))
                {
                   subDic.Add(type, new ADCS(SubsystemXmlNode));
                }
                else if (type.Equals("power"))
                {
                    subDic.Add(type, new Power(SubsystemXmlNode, dependencies, asset));
                }
                else if (type.Equals("eosensor"))
                {
                    subDic.Add(type, new EOSensor(SubsystemXmlNode, dependencies, asset));
                }
                else if (type.Equals("ssdr"))
                {
                    subDic.Add(type, new SSDR(SubsystemXmlNode, dependencies, asset));
                }
                else if (type.Equals("comm"))
                {
                    subDic.Add(type, new Comm(SubsystemXmlNode, dependencies, asset));
                }
                else if (type.Equals("networked"))
                {
                    throw new NotImplementedException("Networked Subsystem is a depreciated feature!");
                }
            }
           // throw new NotSupportedException("Horizon does not recognize the subsystem: " + type);
        }
    }
}
