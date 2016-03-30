using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HSFUniverse;

namespace HSFSystem
{
    public class Asset
    {
        public DynamicState AssetPosition{ get; private set; } //was protected, why?
        //TODO:make isTaskable mean something
        public bool IsTaskable{ get; private set; }//was protected, why?

        public Asset() {
            IsTaskable = false;
        }
        public Asset(DynamicState pos)
        {
            AssetPosition = pos;
            IsTaskable = false;
        }

        public Asset(XmlNode assetXMLNode)
        {
            foreach(XmlNode xIt in assetXMLNode.ChildNodes)// xmlnode node = assetXMLNode["Position"] dynamic state has constructor to take in xml node
            {
                if (xIt.Value.Equals("POSITION"))
                {
                    AssetPosition = new DynamicState(xIt);
                }
            }
          //  AssetPosition =new DynamicState(assetXMLNode.getChildNode("POSITION"));
            IsTaskable = false;
        }

    }
}
