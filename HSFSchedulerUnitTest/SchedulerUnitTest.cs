using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HSFScheduler;
using System.Xml;
using System.Linq;
using UserModel;
using System.Collections.Generic;
using HSFSystem;
using HSFSubsystem;
using Utilities;
using MissionElements;
using HSFUniverse;

namespace HSFSchedulerUnitTest
{
    [TestClass]
    public class SchedulerUnitTest
    {
        [TestMethod]
        public void GenerateSchedulesUnitTest()
        {
            var simulationInputFilePath = @"..\..\..\SimulationInput.XML"; // @"C:\Users\admin\Documents\Visual Studio 2015\Projects\Horizon-Simulation-Framework\Horizon_v2_3\io\SimulationInput.XML";
            var targetDeckFilePath = @"..\..\..\v2.2-300targets.xml";
            var modelInputFilePath = @"..\..\..\Model_Static.xml";
            // Initialize critical section for dependencies ??Morgan Doesn't know what this does
            // InitializeCriticalSection(&horizon::sub::dep::NodeDependencies::cs);


            // Find the main input node from the XML input files
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
            //Scheduler systemScheduler = new Scheduler();
            //bool schedParamsLoaded = loadSchedulerParams(schedParametersXMLNode, systemScheduler);

            bool paramsLoaded = SchedParameters.LoadSchedParameters(schedParametersXMLNode);
            Scheduler systemScheduler = new Scheduler();
            //MultiThreadedScheduler* systemScheduler = new MultiThreadedScheduler;



            // Load the target deck into the targets list from the XML target deck input file
            //var XmlDoc = new XmlDocument();
            XmlDoc.Load(targetDeckFilePath);
            XmlNodeList targetDeckXMLNodeList = XmlDoc.GetElementsByTagName("TARGETDECK");
            int numTargets = XmlDoc.GetElementsByTagName("TARGET").Count;
            XmlEnum = targetDeckXMLNodeList.GetEnumerator();
            XmlEnum.MoveNext();
            var targetDeckXMLNode = (XmlNode)XmlEnum.Current;
            Stack<Task> systemTasks = new Stack<Task>();
            bool targetsLoaded = Task.loadTargetsIntoTaskList(targetDeckXMLNode, ref systemTasks);
            Console.WriteLine("Initial states set");


            // Find the main model node from the XML model input file
            XmlDoc.Load(modelInputFilePath);
            XmlNodeList modelXMLNodeList = XmlDoc.GetElementsByTagName("MODEL");
            XmlEnum = modelXMLNodeList.GetEnumerator();
            XmlEnum.MoveNext();
            var modelInputXMLNode = (XmlNode)XmlEnum.Current;


            // Load the environment. First check if there is an ENVIRONMENT XMLNode in the input file
            Universe SystemUniverse = null;
            foreach (XmlNode node in modelInputXMLNode.ChildNodes)
            {
                if (node.Attributes["ENVIRONMENT"] != null)
                {
                    // Create the Environment based on the XMLNode
                    SystemUniverse = new Universe(node);
                }
            }
            if (SystemUniverse == null)
                SystemUniverse = new Universe();

            //Create singleton dependency dictionary
            Dependencies dependencies = Dependencies.Instance;

            // Initialize List to hold assets and subsystem nodes
            List<Asset> assetList = new List<Asset>();
            List<Subsystem> subNodeList = new List<Subsystem>();

            // Maps used to set up preceeding nodes
            Dictionary<ISubsystem, XmlNode> subsystemXMLNodeMap = new Dictionary<ISubsystem, XmlNode>();
            Dictionary<string, Subsystem> subsystemMap = new Dictionary<string, Subsystem>();
            Dictionary<string, XmlNode> dependencyMap = new Dictionary<string, XmlNode>();
            // Dictionary<string, ScriptedSubsystem> scriptedSubNames = new Dictionary<string, ScriptedSubsystem>();

            // Create Constraint list 
            List<Constraint> constraintsList = new List<Constraint>();

            // Create new Subsystem Factory
            SubsystemFactory subsystemFactory = new SubsystemFactory();

            //Create Lists to hold all the initial condition and dependency nodes to be parsed later
            List<XmlNode> ICNodes = new List<XmlNode>();
            List<XmlNode> DepNodes = new List<XmlNode>();
            List<SystemState> initialSysStates = new List<SystemState>();

            // Enable Python scripting support, add additional functions defined in input file
            bool enableScripting = false;
            // Set up Subsystem Nodes, first loop through the assets in the XML model input file
            foreach (XmlNode childNodeAsset in modelInputXMLNode.ChildNodes)
            {
                if (childNodeAsset.Name.Equals("PYTHON"))
                {
                    if (childNodeAsset.Attributes["enableScripting"] != null)
                    {
                        if (childNodeAsset.Attributes["enableScripting"].Value.ToString().ToLower().Equals("true"))
                            enableScripting = true;
                    }
                    // Loop through all the of the file nodes -- TODO (Morgan) What other types of things might be scripted
                    foreach (XmlNode fileXmlNode in childNodeAsset.ChildNodes)
                    {
                        // If scripting is enabled, parse the script file designated by the attribute
                        if (enableScripting)
                        {
                            // Parse script file if the attribute exists
                            if (fileXmlNode.ChildNodes[0].Name.Equals("EOMS_FILE"))
                            {
                                string fileName = fileXmlNode.ChildNodes[0].Attributes["src"].Value.ToString();
                                ScriptedEOMS eoms = new ScriptedEOMS(fileName);
                            }
                        }
                    }
                }
                if (childNodeAsset.Name.Equals("ASSET"))
                {
                    Asset asset = new Asset();
                    // Loop through all the of the ChildNodess for this Asset
                    foreach (XmlNode childNode in childNodeAsset.ChildNodes)
                    {
                        //Create a new asset from the position and add it to the list
                        if (childNode.Name.Equals("POSITION"))
                            assetList.Add(new Asset(childNode));

                        // Get the current Subsystem XML Node, and create it using the SubsystemFactory
                        if (childNode.Name.Equals("SUBSYSTEM"))
                        {  //is this how we want to do this?
                            // Check if the type of the Subsystem is scripted, networked, or other
                            subsystemFactory.GetSubsystem(childNode, enableScripting, dependencies, asset, subsystemMap);
                            foreach (XmlNode ICorDepNode in childNode.ChildNodes)
                            {
                                if (ICorDepNode.Name.Equals("IC"))
                                    ICNodes.Add(ICorDepNode);
                                if (ICorDepNode.Name.Equals("DEPENDENCY"))
                                    dependencyMap.Add(childNode.Attributes["subsystemName"].Value.ToString(), ICorDepNode);
                            }
                            //Parse the initial condition nodes


                        }
                        //Create a new Constraint
                        if (childNode.Name.Equals("CONSTRAINT"))
                        {
                            constraintsList.Add(ConstraintFactory.getConstraint(childNode));
                            Subsystem constrainedSub;
                            subsystemMap.TryGetValue(childNode.Attributes["subsystemName"].Value.ToString(), out constrainedSub);
                            constraintsList.Last().AddConstrainedSub(constrainedSub);
                        }
                    }
                    if (ICNodes.Count > 0)
                        initialSysStates.Add(SystemState.setInitialSystemState(ICNodes));
                    ICNodes.Clear();
                }
            }
            //Add all the dependent subsystems to tge dependent subsystem list of the subsystems
            foreach (KeyValuePair<string, XmlNode> depSubXmlNode in dependencyMap)
            {
                Subsystem subToAddDep, depSub;
                string depSubName = depSubXmlNode.Value.Attributes["subsystemName"].Value.ToString();
                subsystemMap.TryGetValue(depSubXmlNode.Key, out subToAddDep);
                subsystemMap.TryGetValue(depSubName, out depSub);
                subToAddDep.DependentSubsystems.Add(depSub);
            }
            Console.ReadKey();
        }
    }
}
