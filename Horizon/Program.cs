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
            List<Subsystem> subNodeList = new List<Subsystem>();

            // Maps used to set up preceeding nodes
            Dictionary<ISubsystem, XmlNode> subsystemXMLNodeMap = new Dictionary<ISubsystem, XmlNode>();
            Dictionary<string, Subsystem> subsystemMap = new Dictionary<string, Subsystem>();
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
                            assetList.Add(new Asset(childNodeAsset.ChildNodes[0]));

                        // Get the current Subsystem XML Node, and create it using the SubsystemFactory
                        if (childNode.Name.Equals("SUBSYSTEM"))
                        {  //is this how we want to do this?
                            // Check if the type of the Subsystem is scripted, networked, or other
                            subsystemFactory.GetSubsystem(childNode, enableScripting, dependencies, asset, subsystemMap);
                            foreach (XmlNode ICorDepNode in childNode.ChildNodes)
                            {
                                if(ICorDepNode.Name.Equals("IC"))
                                    ICNodes.Add(ICorDepNode);
                                if (ICorDepNode.Name.Equals("DEPENDENCY"))
                                    DepNodes.Add(ICorDepNode);

                            }
                            //Parse the initial condition nodes


                        }
                        //Create a new Constraint
                        if (childNode.Name.Equals("CONSTRAINT"))
                        {

                        }
                    }
                    if (ICNodes.Count > 0)
                        initialSysStates.Add(SystemState.setInitialSystemState(ICNodes));
                    ICNodes.Clear();
                }
            }
            Console.ReadKey();
                /*
     
		else {
            // Check if the XMLNode for this subsystem has attribute defaultConstructor="true".
            // If it does, call the create method of the subsystemAdapter that will call the default
            // constructor for the subsystem. Otherwise call the create method and pass in the XMLNode
            // for the current subsystem
            if (currSubsystemXMLNode.isAttributeSet("defaultConstructor"))
            {
                if (atob(currSubsystemXMLNode.getAttribute("defaultConstructor")))
                    currSubsystem = subAdapter.create(currSubsystemXMLNode.getAttribute("Type"));
            }
            else
                currSubsystem = subAdapter.create(currSubsystemXMLNode.getAttribute("Type"), currSubsystemXMLNode);
        }
        // Initialize the new SubsystemNode
        SubsystemNode* currNode = new SubsystemNode(currSubsystem, currAsset);
        if (enableScripting)
        {
            currNode->enableScriptingSupport();
            currNode->setLuaState(lua::L);
        }
        subNodeList.push_back(currNode);
        // Get the current Subsystem's SubId and set up the maps used to set up the preceeding nodes in each SubsystemNode
        int subId = atoi(currSubsystemXMLNode.getAttribute("SubId"));
        subsystemNodeXMLNodeMap.insert(make_pair(currNode, currSubsystemXMLNode));
        subsystemNodeMap.insert(make_pair(subId, currNode));
    }
        // Loop through all the of the constraints for this Asset
        m = currAssetXMLNode.nChildNode("CONSTRAINT");
		for(int j = 0; j<m; j++) {
			// Get the Constraint XMLNode and its corresponding StateVar Node
			XMLNode currConstraintXMLNode = currAssetXMLNode.getChildNode("CONSTRAINT", j);
        XMLNode currStateVarXMLNode = currConstraintXMLNode.getChildNode("STATEVAR");
        int subId = atoi(currConstraintXMLNode.getAttribute("SubId"));
        // Determine the type of the Constraint, create the Generic Constraint
        string type = currStateVarXMLNode.getAttribute("type");
        string key = currStateVarXMLNode.getAttribute("key");
        Constraint* constraint;
			// Integer Constraint
			if(_strcmpi(type.c_str(),"Int")==0) {
				int val = atoi(currConstraintXMLNode.getAttribute("value"));
        StateVarKey<int> svk(key);				
				if(currConstraintXMLNode.isAttributeSet("type"))
					constraint = new SingleAssetGenericConstraint<int>(svk, val, currConstraintXMLNode.getAttribute("type"), "int", i+1);
				else
					constraint = new SingleAssetGenericConstraint<int>(svk, val, "int", i+1);
			}
			// Float Constraint
			else if(_strcmpi(type.c_str(),"Float")==0) {
				float val = atof(currConstraintXMLNode.getAttribute("value"));
    StateVarKey<float> svk(key);				
				if(currConstraintXMLNode.isAttributeSet("type"))
					constraint = new SingleAssetGenericConstraint<float>(svk, val, currConstraintXMLNode.getAttribute("type"), "float", i+1);
				else
					constraint = new SingleAssetGenericConstraint<float>(svk, val, "float", i+1);
			}
			// Double Constraint
			else if(_strcmpi(type.c_str(),"Double")==0) {
				double val = atof(currConstraintXMLNode.getAttribute("value"));
StateVarKey<double> svk(key);				
				if(currConstraintXMLNode.isAttributeSet("type"))
					constraint = new SingleAssetGenericConstraint<double>(svk, val, currConstraintXMLNode.getAttribute("type"), "double", i+1);
				else
					constraint = new SingleAssetGenericConstraint<double>(svk, val, "double", i+1);
			}
			// Not a generic constriant, pass to ConstraintAdapter
			else {
				constraint = constraintAdapter.create(type);
			}
			if(constraint != NULL) {
				// Get the subsystem node that this constraint constrains and set it
				SubsystemNode* constrainedNode = subsystemNodeMap.find(subId)->second;
constraint->addConstrianedSubNode(constrainedNode);
constraintsList.push_back(constraint);
			}
		}
	}

	// Loop through each SubsystemNode so that the preceeding nodes can be added
	for(map<SubsystemNode*, XMLNode>::iterator subNodeIt = subsystemNodeXMLNodeMap.begin(); subNodeIt != subsystemNodeXMLNodeMap.end(); subNodeIt++) {
		// Get the XML Node used to initialize the subsystem contained in the current SubsystemNode
		XMLNode currNode = subNodeIt->second;
// Loop through each dependency in the XML Node
int n = currNode.nChildNode("DEPENDENCY");
		for(int i = 0; i<n; i++) {
			// Get the subId of the preceeding node, look up that SubsystemNode in the map, and add it to the current SubsystemNode's preceeding nodes
			XMLNode currDepXMLNode = currNode.getChildNode("DEPENDENCY", i);
subNodeIt->first->addPreceedingNode(subsystemNodeMap.find(atoi(currDepXMLNode.getAttribute("subID")))->second);
		}
		// Loop through each dependency function in the XML Node
		n = currNode.nChildNode("DEPENDENCY_FCN");
		for(int i = 0; i<n; i++) {
			XMLNode currDepFcnXMLNode = currNode.getChildNode("DEPENDENCY_FCN", i);
bool scripted = atob(currDepFcnXMLNode.getAttribute("scripted"));
string key = currDepFcnXMLNode.getAttribute("key");
varType type = strToVarType(currDepFcnXMLNode.getAttribute("type"));
string callKey = currDepFcnXMLNode.getAttribute("callKey");
			if(!scripted){
				switch(type) {
					case varType::INT_:
						subNodeIt->first->addDependency(callKey, depAdapter.getIntDependency(key));
						break;
					case DOUBLE_:
						subNodeIt->first->addDependency(callKey, depAdapter.getDoubleDependency(key));
						break;
					case FLOAT_:
						subNodeIt->first->addDependency(callKey, depAdapter.getFloatDependency(key));
						break;
					case BOOL_:
						subNodeIt->first->addDependency(callKey, depAdapter.getBoolDependency(key));
						break;
					case MATRIX_:
						subNodeIt->first->addDependency(callKey, depAdapter.getMatrixDependency(key));
						break;
					case QUAT_:
						subNodeIt->first->addDependency(callKey, depAdapter.getQuatDependency(key));
						break;
				} // end switch
			} // end if
			else {
				switch(type) {
					case varType::INT_:
						subNodeIt->first->addIntScriptedDependency(callKey, key);
						break;
					case DOUBLE_:
						subNodeIt->first->addDoubleScriptedDependency(callKey, key);
						break;
					case FLOAT_:
						subNodeIt->first->addFloatScriptedDependency(callKey, key);
						break;
					case BOOL_:
						subNodeIt->first->addBoolScriptedDependency(callKey, key);
						break;
					case MATRIX_:
						subNodeIt->first->addMatrixScriptedDependency(callKey, key);
						break;
					case QUAT_:
						subNodeIt->first->addQuatScriptedDependency(callKey, key);
						break;
				}
			} // end else (scripted)
		} // end for
	}

	// Finalize the subsystem adapter
	subAdapter.finalize();

	// Finalize the constraint adapter
	constraintAdapter.finalize();
	
	// Load the initial state as defined in the XML input file
	vector<State*>* systemInitialStateList = new vector<State*>;
bool initialStateSet = setInitialSystemState(modelInputXMLNode, systemInitialStateList);

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
