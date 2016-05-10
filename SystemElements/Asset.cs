using HSFUniverse;
using System.Xml;
using System;

namespace MissionElements
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

        public Asset(XmlNode assetXMLNode)
        {
            if (assetXMLNode.Attributes["assetName"] != null)
                Name = assetXMLNode.Attributes["assetName"].Value.ToString();
            else
                throw new MissingMemberException("Missing name for Asset!");
            foreach(XmlNode child in assetXMLNode.ChildNodes)
            {
                if (child.Name.ToString().Equals("DynamicState"))
                    AssetDynamicState = new DynamicState(child);
            }  
            IsTaskable = false;
        }
        #endregion

    }
}
