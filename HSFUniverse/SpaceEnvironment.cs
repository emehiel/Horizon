using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Utilities;


namespace HSFUniverse
{
    [Serializable]
    public class SpaceEnvironment : Domain
    {
        #region Attributes
        public Sun Sun { get; private set; }
        public Atmosphere Atmos { get; private set; }
        #endregion

        #region Constructors
        public SpaceEnvironment()
        {
            CreateUniverse();
        }
        /*public SpaceEnvironment(Sun sun)
        {
            Sun = sun;
        }*/

        public SpaceEnvironment(XmlNode environmentNode)
        {
            CreateUniverse(environmentNode);
        }
        #endregion

        #region Methods
        protected override void CreateUniverse()
        {
            Sun = new Sun(false);
            Atmos = new StandardAtmosphere();
        }

        protected override void CreateUniverse(XmlNode environmentNode)
        {
            // Check the XMLNode for the presence of a child SUN node
            if (environmentNode["SUN"] != null)
            {
                // Create the Sun based on the XMLNode                
                XmlNode sunNode = environmentNode["SUN"];
                // Check the Sun XMLNode for the attribute
                if (sunNode.Attributes["isSunVectConstant"] != null)
                {
                    bool sunVectConst = Convert.ToBoolean(sunNode.Attributes["isSunVectConstant"].Value);
                    Sun = new Sun(sunVectConst);
                }
                else
                {
                    Sun = new Sun();
                }
            }
            else
            {
                Sun = new Sun();
            }
            if (environmentNode["ATMOSPHERE"] != null)
            {
                // Create the Sun based on the XMLNode                
                XmlNode atmosNode = environmentNode["ATMOSPHERE"];
                //string atmotype = atmosNode.Attributes["type"].ToString;
                // Check the Sun XMLNode for the attribute
                string s = Convert.ToString(atmosNode.Attributes["type"].Value);
                switch (s)
                {
                    case "StandardAtmosphere":
                        Atmos = new StandardAtmosphere();
                        break;
                    case "RealTimeAtmosphere":
                        Atmos = new RealTimeAtmosphere();
                        break;
                }


            }
            else
            {
                Atmos = new StandardAtmosphere();
            }

        }

        /// <summary>
        /// Returns object specified by string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public override T GetObject<T>(string s)
        {
            switch (s.ToLower())
            {
                case "sun":
                    return (T)(object)(Sun);
                case "atmos":
                    return (T)(object)(Atmos);
            }
            return (T)(object)-1;
        }
        #endregion
    }
}