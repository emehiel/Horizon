using System.Xml;


namespace HSFUniverse{
    //Old implementation of universe. To be modified drastically by trent's thesis.
    public class Universe{
        public Sun Sun { get; private set; }
        
        public Universe(){
            Sun = new Sun(false);
        }
        public Universe(Sun sun)
        {
            Sun = sun;
        }

        public Universe(XmlNode environmentNode)
        {
            bool nSun = false;
            XmlNode sunNode;
            foreach (XmlNode child in environmentNode.ChildNodes)
            {   //no idea if this works
                if (child.Equals("SUN")) // Check the XMLNode for the presence of a child SUN node
                {
                    // Create the Sun based on the XMLNode                
                    sunNode = child;
                    foreach(XmlAttribute att in sunNode.Attributes)
                    {
                        if (att.Value.Equals("true")) //use the old isSunVectConstant value
                            nSun = true;
                        Sun = new Sun(nSun);
                        return; //done
                    }
                }
            }
            //didn't find the sun or it didn't have the isSunVectConstant attribute set
            Sun = new Sun();
        }
        

        
        
    }
}