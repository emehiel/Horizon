using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UserModel
{
    public class XmlParser
    {

        public static void ParseScriptedSrc(XmlNode node, ref string pythonFilePath, ref string className)
        {
            if (node.Attributes["src"] == null)
                throw new MissingFieldException("No source file location found in XmlNode");
            pythonFilePath = node.Attributes["src"].Value.ToString();
            //if(scriptedSubXmlNode.Attributes["collectorType"] == null)
            //    CollectorType = Type.GetType(scriptedSubXmlNode.Attributes["CollectorType"].Value.ToString());
            if (node.Attributes["className"] == null)
                throw new MissingFieldException("No class name found in XmlNode" );
            className = node.Attributes["className"].Value.ToString();
        }

        public static XmlNode ParseSimulationInput(string simulationInputFilePath)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.Load(simulationInputFilePath);
            XmlNodeList simulationInputXMLNodeList = XmlDoc.GetElementsByTagName("SCENARIO");
            var XmlEnum = simulationInputXMLNodeList.GetEnumerator();
            XmlEnum.MoveNext();
            var simulationInputXMLNode = (XmlNode)XmlEnum.Current;
            var scenarioName = simulationInputXMLNode.Attributes["scenarioName"].InnerXml;
            Console.Write("EXECUITING SCENARIO: ");
            Console.WriteLine(scenarioName);

            // Load the simulation parameters from the XML simulation input file
            XmlNode simParametersXMLNode = simulationInputXMLNode["SIMULATION_PARAMETERS"];
            bool simParamsLoaded = SimParameters.LoadSimParameters(simParametersXMLNode, scenarioName);

            // Load the scheduler parameters defined in the XML simulation input file
            XmlNode schedParametersXMLNode = simulationInputXMLNode["SCHEDULER_PARAMETERS"];

            bool paramsLoaded = SchedParameters.LoadSchedParameters(schedParametersXMLNode);

            foreach (XmlNode child in schedParametersXMLNode.ChildNodes)
            {
                if (child.Name.Equals("SCRIPTED_EVALUATOR"))
                    return child;                    
            }
            return null;
        }

        public static XmlNode GetTargetNode(string targetDeckFilePath)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.Load(targetDeckFilePath);
            XmlNodeList targetDeckXMLNodeList = XmlDoc.GetElementsByTagName("TARGETDECK");
            var XmlEnum = targetDeckXMLNodeList.GetEnumerator();
            XmlEnum.MoveNext();
            return (XmlNode)XmlEnum.Current;
        }

        public static XmlNode GetModelNode(string modelInputFilePath)
        {
            var XmlDoc = new XmlDocument();
            try {
                XmlDoc.Load(modelInputFilePath);
            }
            catch(Exception e)
            {
                Console.WriteLine("Could not find input file!");
                throw;
            }
            XmlNodeList modelXMLNodeList = XmlDoc.GetElementsByTagName("MODEL");
            var XmlEnum = modelXMLNodeList.GetEnumerator();
            XmlEnum.MoveNext();
            return (XmlNode)XmlEnum.Current;
        }
    }
}
