using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace Universe{
    public class Environment{
        private Sun _sun { get; set; }
        
        public Environment(){
            _sun = new Sun(false);
        }
        public Environment(Sun sun)
        {
            _sun = sun;
        }

        public Environment(XMLNode environmentNode){
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