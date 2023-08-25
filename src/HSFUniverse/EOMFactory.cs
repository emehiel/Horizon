// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Utilities;


namespace HSFUniverse
{
    public class EOMFactory
    {
        public static DynamicEOMS GetEomClass(XmlNode dynamicStateXMLNode)
        {
            string eomsType = dynamicStateXMLNode["EOMS"].GetAttribute("EOMSType");

            if (eomsType == "scripted")
            {
                var eoms = (DynamicEOMS)(new ScriptedEOMS(dynamicStateXMLNode["EOMS"]));
                return eoms;
            }
            else if (eomsType == "EarthPerts")
            {

                string J2Switch = "";
                string J3Switch = "";
                string J4Switch = "";
                string J5Switch = "";
                string J6Switch = "";
                string SRPSwitch = "";
                string DragSwitch = "";
                string NSunSwitch = "";
                string Reflectivity = "";
                string AvgArea = "";
                string Mass = "";
                string CD = "";
                try
                {
                    J2Switch = dynamicStateXMLNode.Attributes["J2Switch"].Value.ToString();
                    J3Switch = dynamicStateXMLNode.Attributes["J3Switch"].Value.ToString();
                    J4Switch = dynamicStateXMLNode.Attributes["J4Switch"].Value.ToString();
                    J5Switch = dynamicStateXMLNode.Attributes["J5Switch"].Value.ToString();
                    J6Switch = dynamicStateXMLNode.Attributes["J6Switch"].Value.ToString();
                    SRPSwitch = dynamicStateXMLNode.Attributes["SRPSwitch"].Value.ToString();
                    DragSwitch = dynamicStateXMLNode.Attributes["DragSwitch"].Value.ToString();
                    NSunSwitch = dynamicStateXMLNode.Attributes["NSunSwitch"].Value.ToString();
                    Reflectivity = dynamicStateXMLNode.Attributes["Reflectivity"].Value.ToString();
                    AvgArea = dynamicStateXMLNode.Attributes["AvgArea"].Value.ToString();
                    Mass = dynamicStateXMLNode.Attributes["Mass"].Value.ToString();
                    CD = dynamicStateXMLNode.Attributes["CD"].Value.ToString();
                }
                catch
                {

                }
                DynamicEOMS eoms = new OrbitalPertEOMS(J2Switch, J3Switch, J4Switch, J5Switch, J6Switch, SRPSwitch, DragSwitch, NSunSwitch, Reflectivity, AvgArea, Mass, CD);
                return eoms;
            }
            else
            {
                DynamicEOMS Eoms = new OrbitalEOMS();
                return Eoms;
            }
        }
    }
}
