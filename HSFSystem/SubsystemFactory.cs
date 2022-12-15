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
            Subsystem sub;
            if (type.Equals("scripted"))
            {
                sub = new ScriptedSubsystem(SubsystemXmlNode, asset);
            }
            else // not scripted subsystem
            {
                if (type.Equals("access"))
                {
                    sub = new AccessSub(SubsystemXmlNode);
                }
                else if (type.Equals("adcs"))
                {
                    sub = new ADCS(SubsystemXmlNode);
                }
                else if (type.Equals("power"))
                {
                    sub = new Power(SubsystemXmlNode);
                }
                else if (type.Equals("eosensor"))
                {
                    sub = new EOSensor(SubsystemXmlNode);
                }
                else if (type.Equals("ssdr"))
                {
                    sub = new SSDR(SubsystemXmlNode);
                }
                else if (type.Equals("comm"))
                {
                    sub = new Comm(SubsystemXmlNode);
                }
                else if (type.Equals("imu"))
                {
                    //sub = new IMU(SubsystemXmlNode, asset);
                    throw new NotImplementedException("Removed after the great SubsystemFactory update.");
                }
                else if (type.Equals("subtest"))
                {
                    sub = new SubTest(SubsystemXmlNode);
                    //sub = new SubTest(SubsystemXmlNode, asset);
                    //throw new NotImplementedException("Removed after the great SubsystemFactory update.");
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
                // Below assignment should NOT happen when sub is scripted, that is handled in ScriptedSubsystem
                sub.DependentSubsystems = new List<Subsystem>();
                sub.SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
                sub.Asset = asset;
                sub.GetSubNameFromXmlNode(SubsystemXmlNode);
                sub.AddDependencyCollector();
                //sub.inputType = SubsystemXmlNode.Attributes["type"].Value.ToString().ToLower();
            }
            return sub;
        }

        public void SetDependencies(XmlNode DepNode, List<Subsystem> SubList) // was static to not req object
        {
            // Find names of asset, sub, dep asset, and dep sub
            string assetName = DepNode.Attributes["assetName"].Value.ToString().ToLower();
            string subName = DepNode.Attributes["subsystemName"].Value.ToString().ToLower();
            string depSubName = DepNode.Attributes["depSubsystemName"].Value.ToString(); // NOT lowercase
            string depAssetName = DepNode.Attributes["depAssetName"].Value.ToString().ToLower();
            //string depSubName = DepNode.Attributes["depSubsystemName"].Value.ToString().ToLower();

            // Add dep sub to sub's list of dep subs
            var sub = SubList.Find(s => s.Name == assetName + "." + subName);
            var depSub = SubList.Find(s => s.Name == depAssetName + "." + depSubName.ToLower());
            sub.DependentSubsystems.Add(depSub);

            if (DepNode.Attributes["fcnName"] != null)
            {
                // Get dep fn name
                string depFnName = DepNode.Attributes["fcnName"].Value.ToString();

                // Determine in what type of sub the depFn lives
                Type depSubType = depSub.GetType();
                
                if (depSubType.Name == "ScriptedSubsystem") // If depFn lives in Python subsystem
                {
                    // Cast depSub to Scripted so compiler does not get mad (it should be scripted to reach here?)
                    ScriptedSubsystem depSubCasted = (ScriptedSubsystem) depSub;
                    // Get method from python script & add to sub's dep fns
                    Delegate fnc = depSubCasted.GetDepFn(depFnName, depSubCasted); 
                    sub.SubsystemDependencyFunctions.Add(depFnName, fnc);
                }
                else // If depFn lives in C# subsystem
                {
                    // Find method that matches name via reflection & add to sub's dep fns
                    var TypeIn = Type.GetType("HSFSubsystem." + depSubName).GetMethod(depFnName);
                    Delegate fnc = Delegate.CreateDelegate(typeof(Func<Event, HSFProfile<double>>), depSub, TypeIn);
                    sub.SubsystemDependencyFunctions.Add(depFnName, fnc);
                }
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
                StateVariableKey<Int32> stateKey = new StateVariableKey<Int32>(key);
                subsys.addKey(stateKey);
            }
            else if (type.Equals("double"))
            {
                StateVariableKey<Double> stateKey = new StateVariableKey<Double>(key);
                subsys.addKey(stateKey);
            }
            else if (type.Equals("bool"))
            {
                StateVariableKey<bool> stateKey = new StateVariableKey<bool>(key);
                subsys.addKey(stateKey);
            }
            else if (type.Equals("matrix"))
            {
                StateVariableKey<Matrix<double>> stateKey = new StateVariableKey<Matrix<double>>(key);
                subsys.addKey(stateKey);
            }
            else if (type.Equals("quat"))
            {
                StateVariableKey<Quaternion> stateKey = new StateVariableKey<Quaternion>(key);
                subsys.addKey(stateKey);
            }
            else if (type.Equals("vector"))
            {
                StateVariableKey<Vector> stateKey = new StateVariableKey<Vector>(key);
                subsys.addKey(stateKey);
            }
            return key;
        }
    }
}
