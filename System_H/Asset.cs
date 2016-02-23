using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Universe;
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

        public Asset(XMLNode assetXMLNode)
        {
            AssetPosition =new DynamicState(assetXMLNode.getChildNode("POSITION"));
            IsTaskable = false;
        }

    }
}
