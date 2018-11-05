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
            DynamicEOMS Eoms = (eomsType == "scripted") ? (DynamicEOMS)(new ScriptedEOMS(dynamicStateXMLNode["EOMS"])) : new OrbitalEOMS();
            return Eoms;
        }
    }
}
