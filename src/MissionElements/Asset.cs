// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using HSFUniverse;
using System.Xml;
using System;
using Utilities;
using Newtonsoft.Json.Linq;
using UserModel;

namespace MissionElements
{
    [Serializable]
    public class Asset
    {
        #region Attributes
        public DynamicState AssetDynamicState{ get; private set; } //was protected, why?
        public string Name { get; private set; }
        //TODO:make isTaskable mean something
        public bool IsTaskable{ get; private set; }//was protected, why? do we need this?
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
        /// <summary>
        /// Standard constructor from JSON Object
        /// </summary>
        /// <param name="assetJson"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Asset(JObject assetJson)
        {
            if(JsonLoader<string>.TryGetValue("name", assetJson, out string name))
                Name = name.ToLower();
            else
                throw new ArgumentOutOfRangeException("AssetJson", assetJson.ToString(),"Asset JSON is missing Name element");

            if (JsonLoader<JObject>.TryGetValue("dynamicState", assetJson, out JObject dynamicStateJson))
                AssetDynamicState = new DynamicState(dynamicStateJson);
            else
                throw new ArgumentOutOfRangeException("assetJson", assetJson.ToString(), $"Asset, {Name}, is missing Dynamic State element.");

            //if (assetJson.TryGetValue("dynamicState", stringCompare, out JToken dynamicStateJson))
            //    AssetDynamicState = new DynamicState((JObject)dynamicStateJson);

            IsTaskable = false;
        }
        public Asset(XmlNode assetXMLNode)
        {
            if (assetXMLNode.Attributes["assetName"] != null)
                Name = assetXMLNode.Attributes["assetName"].Value.ToString().ToLower();
            else
                throw new ArgumentException("Missing name for Asset!");
            if (assetXMLNode["DynamicState"] != null)
                AssetDynamicState = new DynamicState(assetXMLNode["DynamicState"]);
            IsTaskable = false;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Override of the Object Equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            return Name.Equals(((Asset)obj).Name);  
        }

        /// <summary>
        /// Override of the Object GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
