// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Xml;
using HSFSystem;
using MissionElements;
using log4net;
using System.Reflection;
using Utilities;

namespace HSFSubsystem
{
    public class SubsystemFactory
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// A method to interpret the Xml file and create subsystems
        /// </summary>
        /// <param name="SubsystemXmlNode"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static Subsystem GetSubsystem(XmlNode SubsystemXmlNode, Asset asset)
        {
            string type = SubsystemXmlNode.Attributes["type"].Value.ToString().ToLower();
            //string name = Subsystem.parseNameFromXmlNode(SubsystemXmlNode, asset.Name);
            //Subsystem sub = null;
            Subsystem sub;
            if (type.Equals("scripted"))
            {sub = new ScriptedSubsystem(SubsystemXmlNode, asset);
                sub.Asset = asset;
                sub.GetSubNameFromXmlNode(SubsystemXmlNode);
                sub.DependentSubsystems = new List<Subsystem>();
                sub.SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
            }
            else // not scripted subsystem
            {
                if (type.Equals("access"))
                {sub = new AccessSub(SubsystemXmlNode);
                    sub.Asset = asset;
                    sub.GetSubNameFromXmlNode(SubsystemXmlNode);
                    sub.DependentSubsystems = new List<Subsystem>();
                    sub.SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
                }
                else if (type.Equals("adcs"))
                {sub = new ADCS(SubsystemXmlNode);
                    sub.Asset = asset;
                    sub.GetSubNameFromXmlNode(SubsystemXmlNode);
                    sub.DependentSubsystems = new List<Subsystem>();
                    sub.SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
                    //sub.DefaultSubName = "Adcs";
                }
                else if (type.Equals("power"))
                {sub = new Power(SubsystemXmlNode);
                    sub.Asset = asset;
                    sub.GetSubNameFromXmlNode(SubsystemXmlNode);
                    sub.DependentSubsystems = new List<Subsystem>();
                    sub.SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
                }
                else if (type.Equals("eosensor"))
                {sub = new EOSensor(SubsystemXmlNode);
                    sub.Asset = asset;
                    sub.GetSubNameFromXmlNode(SubsystemXmlNode);
                    sub.DependentSubsystems = new List<Subsystem>();
                    sub.SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
                }
                else if (type.Equals("ssdr"))
                {sub = new SSDR(SubsystemXmlNode);
                    sub.Asset = asset;
                    sub.GetSubNameFromXmlNode(SubsystemXmlNode);
                    sub.DependentSubsystems = new List<Subsystem>();
                    sub.SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
                }
                else if (type.Equals("comm"))
                {sub = new Comm(SubsystemXmlNode);
                    sub.Asset = asset;
                    sub.GetSubNameFromXmlNode(SubsystemXmlNode);
                    sub.DependentSubsystems = new List<Subsystem>();
                    sub.SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
                }
                else if (type.Equals("imu"))
                {
                    //sub = new IMU(SubsystemXmlNode, asset);
                    throw new NotImplementedException("Removed after the great Subsystem Factory update.");
                }
                else if (type.Equals("subtest"))
                {
                    //sub = new SubTest(SubsystemXmlNode, asset);
                    throw new NotImplementedException("Removed after the great Subsystem Factory update.");
                }
                else if (type.Equals("networked"))
                {
                    throw new NotImplementedException("Networked Subsystem is a depreciated feature!");
                }
                else
                {
                    log.Fatal("Horizon does not recognize the subsystem: " + type);
                    throw new MissingMemberException("Unknown Subsystem Type " + type);
                }
            }
            sub.AddDependencyCollector();
            
            return sub;
        }

        public static void SetDependencies(XmlNode DepNode, List<Subsystem> SubList)
        {
            // Find names of asset, sub, dep asset, and dep sub
            string assetName = DepNode.Attributes["assetName"].Value.ToString().ToLower();
            string subName = DepNode.Attributes["subsystemName"].Value.ToString().ToLower();
            string subNameUnchanged = DepNode.Attributes["depSubsystemName"].Value.ToString();
            string depAssetName = DepNode.Attributes["depAssetName"].Value.ToString().ToLower();
            string depSubName = DepNode.Attributes["depSubsystemName"].Value.ToString().ToLower();
            //string depFnName = null;

            // Add dep sub to sub's list of dep subs
            var sub = SubList.Find(s => s.Name == assetName + "." + subName);
            var depSub = SubList.Find(s => s.Name == depAssetName + "." + depSubName);
            sub.DependentSubsystems.Add(depSub);

            // XML specified depfn
            if (DepNode.Attributes["fcnName"] != null)
            {
                // Get dep fn name
                string depFnName = DepNode.Attributes["fcnName"].Value.ToString();

                // Find method that matches name & add to sub's dep fns
                var TypeIn = Type.GetType("HSFSubsystem." + subNameUnchanged).GetMethod(depFnName);
                var fnc = Delegate.CreateDelegate(typeof(Func<Event, HSFProfile<double>>), depSub, TypeIn);

                //dependencies.Add(depFnName, fnc);
                sub.SubsystemDependencyFunctions.Add(depFnName,fnc);
            }
            return;
        }
        public static string SetStateKeys(XmlNode StateNode, Subsystem subsys)
        {
            string type = StateNode.Attributes["type"].Value.ToLower();
            string keyName = StateNode.Attributes["key"].Value.ToLower();
            string assetName = subsys.Asset.Name;
            string key = assetName + "." + keyName;
            if (type.Equals("int"))
            {
                StateVarKey<Int32> stateKey = new StateVarKey<Int32>(key);
                subsys.addKey(stateKey);
            }
            else if (type.Equals("double"))
            {
                StateVarKey<Double> stateKey = new StateVarKey<Double>(key);
                subsys.addKey(stateKey);
            }
            else if (type.Equals("bool"))
            {
                StateVarKey<bool> stateKey = new StateVarKey<bool>(key);
                subsys.addKey(stateKey);
            }
            else if (type.Equals("matrix"))
            {
                StateVarKey<Matrix<double>> stateKey = new StateVarKey<Matrix<double>>(key);
                subsys.addKey(stateKey);
            }
            else if (type.Equals("quat"))
            {
                StateVarKey<Quat> stateKey = new StateVarKey<Quat>(key);
                subsys.addKey(stateKey);
            }
            //else if (type.Equals("vector"))
            //{
            //    StateVarKey<Vector> stateKey = new StateVarKey<Vector>(key);
            //    subsys.addKey(stateKey);
            //}
            return key;
        }
    }
}
