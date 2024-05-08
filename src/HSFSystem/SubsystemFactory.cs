// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Xml;
using MissionElements;
using log4net;
using System.Reflection;
using Utilities;
using IronPython.Hosting;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Newtonsoft.Json.Linq;
using Microsoft.Scripting.Utils;
using System.Runtime.InteropServices;
using System.Linq;
using UserModel;

namespace HSFSystem
{
    public class SubsystemFactory
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private static readonly ScriptedSubsystemHelper HSFHelper;
        static SubsystemFactory()
        {
            HSFHelper = new ScriptedSubsystemHelper();
        }
        /// <summary>
        /// A method to interpret the Xml file and create subsystems
        /// </summary>
        /// <param name="SubsystemJson"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static Subsystem GetSubsystem(JObject SubsystemJson, Asset asset)
        {
            //StringComparison stringCompare = StringComparison.CurrentCultureIgnoreCase;

            //string type = "";

            string msg;
            if (JsonLoader<string>.TryGetValue("name", SubsystemJson, out string name))
                name = asset.Name + "." + name.ToLower();
            else
            {
                msg = $"Missing a subsystem 'name' attribute for subsystem in {asset.Name}";
                Console.WriteLine(msg);
                log.Error(msg);
                throw new ArgumentOutOfRangeException(msg);
            }

            if (JsonLoader<string>.TryGetValue("type", SubsystemJson, out string type))
                type = type.ToLower();
            else
            {
                msg = $"Missing a subsystem 'type' attribute for subsystem in {asset.Name}";
                Console.WriteLine(msg);
                log.Error(msg);
                throw new ArgumentOutOfRangeException(msg);
            }

            Subsystem subsystem;

            if (type.Equals("scripted"))
            {
                subsystem = new ScriptedSubsystem(SubsystemJson, asset)
                {
                    Type = type,
                    Name = name
                };
            }
            else // not scripted subsystem
            {
                if (type.Equals("access"))
                {
                    subsystem = new AccessSub(SubsystemJson);
                }
                else if (type.Equals("adcs"))
                {
                    subsystem = new ADCS(SubsystemJson);
                }
                else if (type.Equals("power"))
                {
                    subsystem = new Power(SubsystemJson);
                }
                else if (type.Equals("eosensor"))
                {
                    subsystem = new EOSensor(SubsystemJson);
                }
                else if (type.Equals("ssdr"))
                {
                    subsystem = new SSDR(SubsystemJson);
                }
                else if (type.Equals("comm"))
                {
                    subsystem = new Comm(SubsystemJson);
                }
                else if (type.Equals("imu"))
                {
                    //sub = new IMU(SubsystemXmlNode, asset);
                    throw new NotImplementedException("Removed after the great SubsystemFactory update.");
                }
                else if (type.Equals("subtest"))
                {
                    subsystem = new SubTest(SubsystemJson);
                    //sub = new SubTest(SubsystemXmlNode, asset);
                    //throw new NotImplementedException("Removed after the great SubsystemFactory update.");
                }
                else if (type.Equals("networked"))
                {
                    throw new NotImplementedException("Networked Subsystem is a depreciated feature!");
                }
                else
                {
                    msg = $"Horizon does not recognize the subsystem: {type}";
                    Console.WriteLine(msg);
                    log.Fatal(msg);
                    throw new ArgumentOutOfRangeException(msg);
                }
                // Below assignment should NOT happen when sub is scripted, that is handled in ScriptedSubsystem
                subsystem.DependentSubsystems = new List<Subsystem>();
                subsystem.SubsystemDependencyFunctions = new Dictionary<string, Delegate>();
                subsystem.AddDependencyCollector();
                subsystem.Asset = asset;
                subsystem.Name = name;
                subsystem.Type = type;
            }
            return subsystem;
        }

        public static void SetDependencies(JObject dependencyJson, List<Subsystem> SubsystemList)
        {
            StringComparison stringCompare = StringComparison.CurrentCultureIgnoreCase;
            // Find names of asset, sub, dep asset, and dep sub

            var name = dependencyJson.Select(text => dependencyJson.TryGetValue("assetName", stringCompare, out JToken asset) ?
                new { ok = true, value = asset } : null)
                .Where(t => t.ok)
                .Select(t => t.value.ToString());


            string assetName = dependencyJson.GetValue("assetName", stringCompare).ToString().ToLower();
            string subName = dependencyJson.GetValue("subsystemName", stringCompare).ToString().ToLower();
            string depSubName = dependencyJson.GetValue("depSubsystemName", stringCompare).ToString(); // NOT lowercase
            string depAssetName = dependencyJson.GetValue("depAssetName", stringCompare).ToString().ToLower();
            //string depSubName = DepNode.Attributes["depSubsystemName"].Value.ToString().ToLower();

            // Add dependent subsystem to subsystem's list of dep subs
            var sub = SubsystemList.Find(s => s.Name == assetName + "." + subName);
            var depSub = SubsystemList.Find(s => s.Name == depAssetName + "." + depSubName.ToLower());
            sub.DependentSubsystems.Add(depSub);

            if (dependencyJson.TryGetValue("fcnName", stringCompare, out JToken depFncJToken ))
            {
                // Get dep fn name
                string depFncName = dependencyJson["fcnName"].ToString();

                // Determine in what type of sub the depFn lives
                Type depSubType = depSub.GetType();
                
                if (depSubType.Name == "ScriptedSubsystem") // If depFn lives in Python subsystem
                {
                    // Cast depSub to Scripted so compiler does not get mad (it should be scripted to reach here?)
                    ScriptedSubsystem depSubCasted = (ScriptedSubsystem) depSub;
                    // Get method from python script & add to sub's dep fns
                    Delegate fnc = depSubCasted.GetDepFn(depFncName, depSubCasted); 
                    sub.SubsystemDependencyFunctions.Add(depFncName, fnc);
                }
                else // If depFn lives in C# subsystem
                {
                    // Find method that matches name via reflection & add to sub's dep fns
                    var TypeIn = Type.GetType("HSFSystem." + depSubName).GetMethod(depFncName);
                    Delegate fnc = Delegate.CreateDelegate(typeof(Func<Event, HSFProfile<double>>), depSub, TypeIn);
                    sub.SubsystemDependencyFunctions.Add(depFncName, fnc);
                }
            }  
        }
        public static string SetStateKeys(JObject StateNodeJson, Subsystem subsys)
        {
            StringComparison stringCompare = StringComparison.CurrentCultureIgnoreCase;

            string type = "";
            if(StateNodeJson.TryGetValue("Type", stringCompare, out JToken typeJson))
                type = typeJson.Value<string>().ToLower();

            string keyName = "";
            if (StateNodeJson.TryGetValue("Key", stringCompare, out JToken keyJson))
                keyName = keyJson.Value<string>().ToLower();

            string assetName = subsys.Asset.Name;
            string key = assetName + "." + keyName;
            dynamic stateKey = null;
            if (type.Equals("int"))
            {
                stateKey = new StateVariableKey<int>(key);
                //subsys.addKey(stateKey);
            }
            else if (type.Equals("double"))
            {
                stateKey = new StateVariableKey<double>(key);
                //subsys.addKey(stateKey);
            }
            else if (type.Equals("bool"))
            {
                stateKey = new StateVariableKey<bool>(key);
                //subsys.addKey(stateKey);
            }
            else if (type.Equals("matrix"))
            {
                stateKey = new StateVariableKey<Matrix<double>>(key);
            }
            else if (type.Equals("quaternion"))
            {
                stateKey = new StateVariableKey<Quaternion>(key);
                //subsys.addKey(stateKey);
            }
            else if (type.Equals("vector"))
            {
                stateKey = new StateVariableKey<Vector>(key);
                //subsys.addKey(stateKey);
            }

            subsys.addKey(stateKey);
            if (subsys.Type == "scripted")
            {
                string stateName = "";
                if (StateNodeJson.TryGetValue("Name", stringCompare, out JToken nameJson))
                    stateName = nameJson.Value<string>().ToLower();

                ((ScriptedSubsystem)subsys).SetStateVariable(HSFHelper, stateName, stateKey);
            }

            return key;
        }

        public static void SetParameters(JObject parameterJson, Subsystem subsystem)
        {
            StringComparison stringCompare = StringComparison.CurrentCultureIgnoreCase;
            string name = parameterJson.GetValue("name", stringCompare).ToString();
            // TODO:  Check to make sure name is a valid python variable name
            string value = parameterJson.GetValue("value", stringCompare).ToString();
            string type = parameterJson.GetValue("type", stringCompare).ToString().ToLower();

            SubsystemFactory.InitParameter(name, value, type, subsystem);

        }

        public static void SetParameters(XmlNode ParameterNode, Subsystem subsystem)
        {
            string name = ParameterNode.Attributes["name"].Value;
            // TODO:  Check to make sure name is a valid python variable name
            string value = ParameterNode.Attributes["value"].Value.ToLower();
            string type = ParameterNode.Attributes["type"].Value.ToLower();
            SubsystemFactory.InitParameter(name, value, type, subsystem);
        }

        private static void InitParameter(string name, string value, string type, Subsystem subsystem)
        { 
            dynamic paramValue = null;

            switch (type)
            {
                case ("double"):
                    paramValue = Convert.ToDouble(value);
                    break;
                case ("int"):
                    paramValue = Convert.ToInt32(value);
                    break;
                case ("string"):
                    paramValue = Convert.ToString(value);
                    break;
                case ("bool"):
                    paramValue = Convert.ToBoolean(value);
                    break;
                case ("matrix"):
                    paramValue = new Matrix<double>(value);
                    break;
                case ("quaterion"):
                    paramValue = new Quaternion(value);
                    break;
                case ("vector"):
                    paramValue = new Vector(value);
                    break;
            }

            ((ScriptedSubsystem)subsystem).SetSubsystemParameter(HSFHelper, name, paramValue);

        }
    }
}
