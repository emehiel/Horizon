using HSFUniverse;
using System.Xml;

namespace HSFSystem
{
    public class Asset
    {
        public DynamicState AssetDynamicState{ get; private set; } //was protected, why?
        //TODO:make isTaskable mean something
        public bool IsTaskable{ get; private set; }//was protected, why?

        public Asset() {
            IsTaskable = false;
        }
        public Asset(DynamicState dynamicState)
        {
            AssetDynamicState = dynamicState;
            IsTaskable = false;
        }

        public Asset(XmlNode assetXMLNode)
        {
            AssetDynamicState =new DynamicState(assetXMLNode["DynamicState"]);  // XmlInput Change - position => DynamicState
            IsTaskable = false;
        }

    }
}
