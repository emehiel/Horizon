using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace HSFUniverse{
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

        public Universe(XMLNode environmentNode){
            // Check the XMLNode for the presence of a child SUN node
            int nSun = environmentNode.nChildNode("SUN");
            if (nSun >0){
                // Create the Sun based on the XMLNode                
                XMLNode sunNode = environmentNode.getChildNode("SUN");
                // Check the Sun XMLNode for the attribute
                if(sunNode.isAttributeSet("isSunVectConstant")){
                    bool sunVecConst = atob(sunNode.getAttribute("isSunVecConstant"));
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