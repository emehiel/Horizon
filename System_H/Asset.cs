using HSFUniverse;
using System.Xml;

namespace HSFSystem
{
    public class Asset
    {
        #region Attributes
        public DynamicState AssetDynamicState{ get; private set; } //was protected, why?
        public string Name { get; private set; }
        //TODO:make isTaskable mean something
        public bool IsTaskable{ get; private set; }//was protected, why?
        #endregion

        #region Constructors
        public Asset() {
            IsTaskable = false;
        }
        public Asset(DynamicState dynamicState, string name)
        {
            Name = name;
            AssetDynamicState = dynamicState;
            IsTaskable = false;
        }

        public Asset(XmlNode positionXMLNode)
        {
            if(positionXMLNode.Attributes["name"] != null)
                Name = positionXMLNode.Attributes["name"].Value.ToString();
            AssetDynamicState =new DynamicState(positionXMLNode);  // XmlInput Change - position => DynamicState
            IsTaskable = false;
        }
        #endregion

    }
}
