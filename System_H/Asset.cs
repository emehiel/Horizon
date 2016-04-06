using HSFUniverse;
using System.Xml;

namespace HSFSystem
{
    public class Asset
    {
        public DynamicState AssetDynamicState{ get; private set; } //was protected, why?
        public string Name { get; private set; }
        //TODO:make isTaskable mean something
        public bool IsTaskable{ get; private set; }//was protected, why?

        public Asset() {
            IsTaskable = false;
        }
        public Asset(DynamicState dynamicState, string name)
        {
            Name = name;
            AssetDynamicState = dynamicState;
            IsTaskable = false;
        }

        public Asset(XmlNode assetXMLNode)
        {
            Name = assetXMLNode.Attributes["name"].Value;
            AssetDynamicState =new DynamicState(assetXMLNode["DynamicState"]);  // XmlInput Change - position => DynamicState
            IsTaskable = false;
        }

    }
}
