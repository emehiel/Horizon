using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Horizon;

namespace Universe{
    public class Environment{
        private _sun {get;}
        
        public Environment(){
            _sun = new Sun(false);
        }
        
        public Environment(XMLNode environmentNode){
            // Check the XMLNode for the presence of a child SUN node
            int nSun = environmentNode.nChildNode("SUN");
            if (nSun){
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
        
        public Environment(Sun sun){
            _sun = sun;
        }
        
        
    }
}