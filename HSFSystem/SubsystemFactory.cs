// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Xml;
using HSFSystem;
using MissionElements;
using log4net;

namespace HSFSubsystem
{
    public class SubsystemFactory
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// A method to interpret the Xml file and create subsystems
        /// </summary>
        /// <param name="SubsystemXmlNode"></param>
        /// <param name="enableScripting"></param>
        /// <param name="dependencies"></param>
        /// <param name="asset"></param>
        /// <param name="subDic"></param>
        /// <returns></returns>
        public static string GetSubsystem(XmlNode SubsystemXmlNode, Dependency dependencies, Asset asset, Dictionary<string, Subsystem> subDic)
        {
            string type = SubsystemXmlNode.Attributes["Type"].Value.ToString().ToLower();
            string name = Subsystem.parseNameFromXmlNode(SubsystemXmlNode, asset.Name);
            if (type.Equals("scripted"))
            {
                subDic.Add(name, new ScriptedSubsystem(SubsystemXmlNode, dependencies, asset));
            }
            else // not scripted subsystem
            {
                if (type.Equals("access"))
                {
                    subDic.Add(name, new AccessSub(SubsystemXmlNode, asset));
                }
                else if (type.Equals("adcs"))
                {
                   subDic.Add(name, new ADCS(SubsystemXmlNode, dependencies, asset));
                }
                else if (type.Equals("power"))
                {
                    subDic.Add(name, new Power(SubsystemXmlNode, dependencies, asset));
                }
                else if (type.Equals("eosensor"))
                {
                    subDic.Add(name, new EOSensor(SubsystemXmlNode, dependencies, asset));
                }
                else if (type.Equals("ssdr"))
                {
                    subDic.Add(name, new SSDR(SubsystemXmlNode, dependencies, asset));
                }
                else if (type.Equals("comm"))
                {
                    subDic.Add(name, new Comm(SubsystemXmlNode, dependencies, asset));
                }
                else if (type.Equals("imu"))
                {
                    subDic.Add(name, new IMU(SubsystemXmlNode, dependencies, asset));
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
            return name;
        }
    }
}
