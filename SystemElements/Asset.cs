using HSFUniverse;
using System.Xml;
using System;

namespace MissionElements
{
    [Serializable]
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

        public Asset(XmlNode assetXMLNode)
        {
            if (assetXMLNode.Attributes["assetName"] != null)
                Name = assetXMLNode.Attributes["assetName"].Value.ToString().ToLower();
            else
                throw new MissingMemberException("Missing name for Asset!");
            if (assetXMLNode["DynamicState"] != null)
                AssetDynamicState = new DynamicState(assetXMLNode["DynamicState"]);


            IsTaskable = false;
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            return Name.Equals(((Asset)obj).Name);  
        }

    }
}
