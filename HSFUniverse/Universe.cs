// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace HSFUniverse{
    public class Universe{
        #region Attributes
        public Sun Sun { get; private set; }
        #endregion

        #region Constructors
        public Universe(){
            Sun = new Sun(false);
        }
        public Universe(Sun sun)
        {
            Sun = sun;
        }

        public Universe(XmlNode environmentNode){
            // Check the XMLNode for the presence of a child SUN node
            if (environmentNode["SUN"] != null)
            {
                // Create the Sun based on the XMLNode                
                XmlNode sunNode = environmentNode["SUN"];
                // Check the Sun XMLNode for the attribute
                if(sunNode.Attributes["isSunVectConstant"] != null)
                {
                    bool sunVecConst = Convert.ToBoolean(sunNode.Attributes["isSunVecConstant"]);
                    Sun = new Sun(sunVecConst);
                }
                Sun = new Sun();
            }
            else{
                Sun = new Sun();
            }
        }
        #endregion
    }
}