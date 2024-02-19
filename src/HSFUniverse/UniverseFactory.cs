
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HSFUniverse
{
    public class UniverseFactory
    {
        public static Domain GetUniverseClass(JObject environmentJson)
        {
            StringComparison stringCompare = StringComparison.CurrentCultureIgnoreCase;
            Domain universe = null;
            string type = environmentJson.GetValue("type", stringCompare).ToString().ToLower();

            if (type.Equals("scripted"))
            {
                universe = (Domain)new ScriptedUniverse(environmentJson);
            }
            else if (type.Equals("spaceenvironment"))
            {
                universe = (Domain)new SpaceEnvironment(environmentJson);
            }
            else if (type.Equals("airborneenvironment"))
            {
                throw new NotImplementedException("Airborne Environment needs to be implemented!");
            }
            else
            {
                throw new ArgumentException("Evironment is not set to a HSF Environment type");
            }

            return universe;
        }
        /// <summary>
        /// A method to interpret the Xml file and create a universe instance
        /// </summary>
        /// <param name="modelXmlNodel"></param>
        /// <returns></returns>
        public static Domain GetUniverseClass(XmlNode modelXmlNodel)
        {
            Domain universe = (Domain)new SpaceEnvironment(); // cannot initialize variable inside conditional
            string universeType = modelXmlNodel.Attributes["UniverseType"].Value.ToString().ToLower();

            if (universeType.Equals("scripted"))
            {
                universe = (Domain)new ScriptedUniverse(modelXmlNodel);
            }
            else // non-scripted universes
            {
                if (universeType.Equals("spaceenvironment"))
                {
                    universe = (Domain)new SpaceEnvironment(modelXmlNodel);
                }
                else if (universeType.Equals("airborneenvironment"))
                {
                    throw new NotImplementedException("Airborne Environment needs to be implemented!");
                }
            }

            return universe;
        }
    }
}
