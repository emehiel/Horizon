// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Xml;
using log4net;

namespace UserModel
{
    public class XmlParser//todo catch exceptions with logger
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Parse scripted subsystem xml
        /// </summary>
        /// <param name="node"></param>
        /// <param name="src"></param>
        /// <param name="className"></param>
        public static void ParseScriptedSrc(XmlNode node, ref string src, ref string className)
        {
            if (node.Attributes["src"] == null)
                throw new MissingFieldException("No source file location found in XmlNode");
            src = node.Attributes["src"].Value.ToString();
            //if(scriptedSubXmlNode.Attributes["collectorType"] == null)
            //    CollectorType = Type.GetType(scriptedSubXmlNode.Attributes["CollectorType"].Value.ToString());
            if (node.Attributes["className"] == null)
                throw new MissingFieldException("No class name found in XmlNode" );
            className = node.Attributes["className"].Value.ToString();
        }

        /// <summary>
        /// Get the simulation input XML node from the to be used in the SimParameters constructor
        /// </summary>
        /// <param name="simulationInputFilePath"></param>
        /// <returns></returns>
        public static void ParseSimulationInput(string simulationInputFilePath)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.Load(simulationInputFilePath);

            // Load the scenario parameters forom the XML simulation input file
            XmlNode simulationInputXMLNode = XmlDoc.GetElementsByTagName("SCENARIO")[0];
            var scenarioName = simulationInputXMLNode.Attributes["scenarioName"].Value;
            log.Info("EXECUITING SCENARIO: "+ scenarioName);

            // Load the simulation parameters from the XML simulation input file
            XmlNode simParametersXMLNode = simulationInputXMLNode["SIMULATION_PARAMETERS"];
            bool simParamsLoaded = SimParameters.LoadSimParameters(simParametersXMLNode, scenarioName);

            // Load the scheduler parameters defined in the XML simulation input file
            XmlNode schedParametersXMLNode = simulationInputXMLNode["SCHEDULER_PARAMETERS"];
            bool paramsLoaded = SchedParameters.LoadSchedParameters(schedParametersXMLNode);
        }

        /// <summary>
        /// Get target node from input file to be passed to target constructor
        /// </summary>
        /// <param name="targetDeckFilePath"></param>
        /// <returns></returns>
        public static XmlNode GetTargetNode(string targetDeckFilePath)
        {
            var XmlDoc = new XmlDocument();
            try
            {
                XmlDoc.Load(targetDeckFilePath);
            }
            catch(Exception e)
            {
                log.Fatal(e);
                return null;
            }
            XmlNodeList targetDeckXMLNodeList = XmlDoc.GetElementsByTagName("TARGETDECK");
            var XmlEnum = targetDeckXMLNodeList.GetEnumerator();
            XmlEnum.MoveNext();
            return (XmlNode)XmlEnum.Current;
        }

        /// <summary>
        /// Get model node from input file to be used to create subsysetms
        /// </summary>
        /// <param name="targetDeckFilePath"></param>
        /// <returns></returns>
        public static XmlNode GetModelNode(string modelInputFilePath)
        {
            var XmlDoc = new XmlDocument();
            try {
                XmlDoc.Load(modelInputFilePath);
            }
            catch(Exception e)
            {
                log.Error("Could not find input file!");
                throw;
            }
            XmlNodeList modelXMLNodeList = XmlDoc.GetElementsByTagName("MODEL");
            var XmlEnum = modelXMLNodeList.GetEnumerator();
            XmlEnum.MoveNext();
            return (XmlNode)XmlEnum.Current;
        }
    }
}
