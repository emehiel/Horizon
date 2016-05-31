using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Utilities
{
    public class EOMFactory
    {
        public static EOMS GetEomClass(XmlNode dynamicStateXMLNode)
        {
            EOMS Eoms = new OrbitalEOMS(); //default is orbitaleoms
            string eomsType = dynamicStateXMLNode["EOMS"].GetAttribute("EOMSType");
            if (eomsType == "scripted")
                Eoms = new ScriptedEOMS(dynamicStateXMLNode["EOMS"]);
            return Eoms;
        }
    }
}
