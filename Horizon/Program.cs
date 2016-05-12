using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
//using System.IO;
using HSFScheduler;
//using HSFSystem;
using Utilities;
using MissionElements;
using UserModel;
using HSFUniverse;
using HSFSubsystem;
using HSFSystem;

namespace Horizon
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Get the input filenames
            //string simulationInputFilePath = args[1];
            //string targetDeckFilePath = args[2];
            //string modelInputFileName = args[3];
            //string outputPath = args[4];
            var simulationInputFilePath = @"..\..\..\SimulationInput.XML"; // @"C:\Users\admin\Documents\Visual Studio 2015\Projects\Horizon-Simulation-Framework\Horizon_v2_3\io\SimulationInput.XML";
            var targetDeckFilePath = @"..\..\..\v2.2-300targets.xml";
            var modelInputFilePath = @"..\..\..\DSAC_Static.xml";
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
            bool targetsLoaded = Task.loadTargetsIntoTaskList(targetDeckXMLNode, systemTasks);
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

      //      var ADCSSub = new ADCS();
       //     var PowerSub = new Power(dependencies);


            // Initialize NetworkDataClient
            //int nDataClient = modelInputXMLNode.nChildNode("NETWORK_DATA_CLIENT");
            //if (nDataClient != 0)
            //{
            //    XMLNode NetworkDataClientNode = modelInputXMLNode.getChildNode("NETWORK_DATA_CLIENT");
            //    if (NetworkDataClientNode.isAttributeSet("host"))
            //    {
            //        NetworkDataClient::setHost(NetworkDataClientNode.getAttribute("host"));
            //    }
            //    if (NetworkDataClientNode.isAttributeSet("port"))
            //    {
            //        NetworkDataClient::setPort(atoi(NetworkDataClientNode.getAttribute("port")));
            //    }
            //    if (NetworkDataClientNode.isAttributeSet("connect"))
            //    {
            //        if (atob(NetworkDataClientNode.getAttribute("connect")))
            //            NetworkDataClient::Connect();
            //    }
            //}

            /*
            // Initialize the SubsystemAdapter
            SubsystemAdapter subAdapter;
            subAdapter.initialize();

            // Initialize the ConstraintAdapter
            ConstraintAdapter constraintAdapter;
            constraintAdapter.initialize();
            
            // Initialize the Dependency Adapter (for static dependencies)
            DependencyAdapter depAdapter;
            depAdapter.addDoubleDependency("Asset1_COMMSUB_getDataRateProfile", &Dependencies::Asset1_COMMSUB_getDataRateProfile);
            depAdapter.addDoubleDependency("Asset1_POWERSUB_getPowerProfile", &Dependencies::Asset1_POWERSUB_getPowerProfile);
            depAdapter.addDoubleDependency("Asset1_SSDRSUB_getNewDataProfile", &Dependencies::Asset1_SSDRSUB_getNewDataProfile);
            depAdapter.addDoubleDependency("Asset2_COMMSUB_getDataRateProfile", &Dependencies::Asset2_COMMSUB_getDataRateProfile);
            depAdapter.addDoubleDependency("Asset2_POWERSUB_getPowerProfile", &Dependencies::Asset2_POWERSUB_getPowerProfile);
            depAdapter.addDoubleDependency("Asset2_SSDRSUB_getNewDataProfile", &Dependencies::Asset2_SSDRSUB_getNewDataProfile);
            */
            // Initialize List to hold assets and subsystem nodes
            List<Asset> assetList = new List<Asset>();
            List<Subsystem> subList = new List<Subsystem>();

            // Maps used to set up preceeding nodes
            Dictionary<ISubsystem, XmlNode> subsystemXMLNodeMap = new Dictionary<ISubsystem, XmlNode>();
            Dictionary<string, Subsystem> subsystemMap = new Dictionary<string, Subsystem>();
            List<KeyValuePair<string, string>> dependencyMap = new List<KeyValuePair<string, string>>();
            List<KeyValuePair<string, string>> dependencyFcnMap = new List<KeyValuePair<string, string>>();
            // Dictionary<string, ScriptedSubsystem> scriptedSubNames = new Dictionary<string, ScriptedSubsystem>();

            // Create Constraint list 
            List<Constraint> constraintsList = new List<Constraint>();

            //Create Lists to hold all the initial condition and dependency nodes to be parsed later
            List<XmlNode> ICNodes = new List<XmlNode>();
            List<XmlNode> DepNodes = new List<XmlNode>();
            SystemState initialSysState = new SystemState();

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
                    Asset asset = new Asset(childNodeAsset);
                    assetList.Add(asset);
                    // Loop through all the of the ChildNodess for this Asset
                    foreach (XmlNode childNode in childNodeAsset.ChildNodes)
                    {
                        // Get the current Subsystem XML Node, and create it using the SubsystemFactory
                        if (childNode.Name.Equals("SUBSYSTEM"))
                        {  //is this how we want to do this?
                            // Check if the type of the Subsystem is scripted, networked, or other
                            string subName = SubsystemFactory.GetSubsystem(childNode, enableScripting, dependencies, asset, subsystemMap);
                            foreach (XmlNode ICorDepNode in childNode.ChildNodes)
                            {
                                if(ICorDepNode.Name.Equals("IC"))
                                    ICNodes.Add(ICorDepNode);
                                if (ICorDepNode.Name.Equals("DEPENDENCY"))
                                {
                                    string depSubName = "", depFunc = "";
                                    depSubName = Subsystem.parseNameFromXmlNode(ICorDepNode, asset.Name) ;
                                    dependencyMap.Add(new KeyValuePair<string, string>(subName, depSubName));

                                    if (ICorDepNode.Attributes["fcnName"] != null)
                                        depFunc = ICorDepNode.Attributes["fcnName"].Value.ToString();
                                    else
                                        throw new MissingMemberException("Missing dependency function for subsystem" + subName);
                                    dependencyFcnMap.Add(new KeyValuePair<string, string>(subName, depFunc));
                                }  
                              //  if (ICorDepNode.Name.Equals("DEPENDENCY_FCN"))
                               //     dependencyFcnMap.Add(childNode.Attributes["subsystemName"].Value.ToString(), ICorDepNode.Attributes["fcnName"].Value.ToString());

                            }
                            //Parse the initial condition nodes


                        }
                        //Create a new Constraint
                        if (childNode.Name.Equals("CONSTRAINT"))
                        {
                            constraintsList.Add(ConstraintFactory.getConstraint(childNode, subsystemMap, asset));
                        }
                    }
                    if (ICNodes.Count > 0)
                        initialSysState.Add(SystemState.setInitialSystemState(ICNodes, asset));
                    ICNodes.Clear();
                }
            }
            foreach (KeyValuePair<string, Subsystem> sub in subsystemMap)
            {
                subList.Add(sub.Value);
            }
            Console.WriteLine("Subsystems and Constraints Loaded");
            //Add all the dependent subsystems to the dependent subsystem list of the subsystems
            foreach (KeyValuePair<string, string> depSubPair in dependencyMap)
            {
                Subsystem subToAddDep, depSub;
                subsystemMap.TryGetValue(depSubPair.Key, out subToAddDep);
                subsystemMap.TryGetValue(depSubPair.Value, out depSub);
                subToAddDep.DependentSubsystems.Add(depSub);
            }

            //give the dependency functions to all the subsytems that need them
            foreach (KeyValuePair<string, string> depFunc in dependencyFcnMap)
            {
                Subsystem subToAddDep;
                subsystemMap.TryGetValue(depFunc.Key, out subToAddDep);
                subToAddDep.SubsystemDependencyFunctions.Add(depFunc.Value, dependencies.getDependencyFunc(depFunc.Value));
            }
            Console.WriteLine("Dependencies Loaded");

            //Need to make this parse Xml
            Evaluator schedEvaluator = new TargetValueEvaluator(dependencies);

            SystemClass simSystem = new SystemClass(assetList, subList, constraintsList, SystemUniverse);

            if (simSystem.checkForCircularDependencies())
                throw new NotFiniteNumberException("System has circular dependencies! Please correct then try again.");

            Scheduler scheduler = new Scheduler();
            List<SystemSchedule> schedules = scheduler.GenerateSchedules(simSystem, systemTasks, initialSysState, schedEvaluator);

            Console.ReadKey();
                /*
// USER - Specify data output parameters
scheduleDataWriter dataOut(outputPath, true, 2);
cout << endl;
	
	// Setup schedule evaluator method
	ScheduleEvaluator* systemEvaluator;
int nSchedEval = modelInputXMLNode.nChildNode("SCHEDULE_EVALUATOR");
	if(nSchedEval != 0) {
		// Create the ScheduleEvaluator based on the XMLNode
		systemEvaluator = horizon::util::adapt::createScheduleEvaluator(lua::L, modelInputXMLNode.getChildNode("SCHEDULE_EVALUATOR"));
	}
	else {
		// If node doesnt exist, pass empty node in and the 
		// default Schedule Evaluator will be used
		systemEvaluator = createScheduleEvaluator(lua::L, XMLNode::emptyXMLNode);
	}

	// Load the environment, subsystems, and constraints into a system to simulate
	System* simSystem = new System(assetList, subNodeList, constraintsList, systemEnvironment);

//------------------------------ RUN THE MAIN ALGORITHM -------------------------------- // 
list<systemSchedule*> schedules;
	if(!simSystem->checkForCircularDependencies())
		schedules = systemScheduler->generateSchedules(*simSystem, * systemTasks, * systemInitialStateList, systemEvaluator);
	else
		cout << "System model contains circular dependencies, cannot run simulation" << endl;
    //-------------------------------------------------------------------------------------- //	

	cout << endl << endl;
	cout << "Total Time: " << systemScheduler->totalTimeMs << endl;
	cout << "Pregen Time: " << systemScheduler->pregenTimeMs << endl;
	cout << "Sched Time: " << systemScheduler->schedTimeMs << endl;
	cout << "Accum Time: " << systemScheduler->accumSchedTimeMs << endl;

    // Delete critical section for dependencies
    DeleteCriticalSection(&NodeDependencies::cs);

// Write error log
Logger::write("SubFailures.log");
	Logger::writeConstraint("ConFailures.log");

	// *********************************Output selected data*************************************
	bool schedOutput = dataOut.writeAll(schedules, simSystem);
// ******************************************************************************************

// Clean up memory
delete systemScheduler;
// Delete the initial state vector
delete systemInitialStateList;
	// Delete the tasks
	for(vector<const Task*>::iterator tIt = systemTasks->begin(); tIt != systemTasks->end(); tIt++) { delete* tIt; }
	// Delete the task vector
	delete systemTasks;
// Delete the Evaluator
delete systemEvaluator;
// Delete the System
delete simSystem;
	// Delete the possible generated schedules
	for(list<systemSchedule*>::iterator sIt = schedules.begin(); sIt != schedules.end();) {
		delete* sIt;
schedules.erase(sIt++);
	}	

    getchar();
	return 0;
*/
        }
    }
}
