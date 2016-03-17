using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace Universe{
    //Old implementation of universe. To be modified drastically by trent's thesis.
    public class Universe{
        private Sun _sun { get; set; }
        
        public Universe(){
            _sun = new Sun(false);
        }
        public Universe(Sun sun)
        {
            _sun = sun;
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
                    _sun = new Sun(sunVecConst);
                }
                _sun = new Sun();
            }
            else{
                _sun = new Sun();
            }
        }
        

        
        
    }
}