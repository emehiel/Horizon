using System;
using NUnit.Framework;
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
using Horizon;
using System.IO;

namespace HSFSchedulerUnitTest
{
    [TestFixture]
    public class SchedulerUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void GenerateSchedulesUnitTest()
        {

            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub.xml");

            Stack<Task> systemTasks = programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
            }
            try
            {
                programAct.LoadDependencies();
            }
            catch
            {
                programAct.log.Info("LoadDepenedencies Failed the Unit test");
            }

            programAct.CreateSchedules(systemTasks);
            programAct.EvaluateSchedules();
            //TEST SCHED COUNT
            double schedCountExp = 24;
            double schedCountAct = programAct.schedules.Count;
            Assert.AreEqual(schedCountExp, schedCountAct);

            //TEST SCHED SCORE & SORT
            //Expect 24 schedules, [0] with score of 5 and [1-4] with score of 4
            int diffExp = 1;
            double diffAct = programAct.schedules[0].ScheduleValue - programAct.schedules[1].ScheduleValue;
            Assert.AreEqual(diffExp, diffAct);

            //Assert.AreEqual(highestValTargetExp, highestValTargetAct);
            Task target3  = programAct.schedules[0].AllStates.Events.Pop().Tasks[programAct.AssetList[0]];
            Task target2  = programAct.schedules[0].AllStates.Events.Pop().Tasks[programAct.AssetList[0]];
            Task target11 = programAct.schedules[0].AllStates.Events.Pop().Tasks[programAct.AssetList[0]];
            Task target0  = programAct.schedules[0].AllStates.Events.Pop().Tasks[programAct.AssetList[0]];
            //var vals = programAct.AssetList;
            Task task3  = systemTasks.Pop();
            Task task2  = systemTasks.Pop();
            Task task11 = systemTasks.Pop();
            Task task1  = systemTasks.Pop();
            Task task0  = systemTasks.Pop();

            Assert.AreSame(target0, task0);
            Assert.AreEqual(target11, task11);
            Assert.AreEqual(target2, task2);
            Assert.AreEqual(target3, task3);

            // check if equal means reference
        }
        [Test]
        public void cropSchedulesUnitTest()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub_crop.xml");

            Stack<Task> systemTasks = programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
            }
            try
            {
                programAct.LoadDependencies();
            }
            catch
            {
                programAct.log.Info("LoadDepenedencies Failed the Unit test");
            }

            try
            {
                programAct.CreateSchedules(systemTasks);
            }
            catch
            {
                programAct.log.Info("CreateSchedules Failed the Unit test");
            }

            try
            {
                double maxSched = programAct.EvaluateSchedules();
            }
            catch
            {
                programAct.log.Info("EvaluateSchedules Failed the Unit test");
            }
            


            //TEST SCHED COUNT
            int schedCountExp = 8;
            double schedCountAct = programAct.schedules.Count;
            Assert.AreEqual(schedCountExp, schedCountAct);

            //TEST SCHED SCORE & SORT
            //Expect 8 schedules, [0] with score of 3 [max possible] and [1] with score of 2
            int diffExp = 1;
            double diffAct = programAct.schedules[0].ScheduleValue - programAct.schedules[1].ScheduleValue;
            Assert.AreEqual(diffExp, diffAct);

            double emptySchedEventCount = programAct.schedules[7].AllStates.Events.Count();
            Assert.AreEqual(0, emptySchedEventCount);

        }

        [Test]
        public void generateSchedulesUnitTest_NoPregen()
        {
            //Tests the same generate Schedules (main scheduler loop) but if access cannot be pre-generated
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub_DynamicECI.xml");

            Stack<Task> systemTasks = programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
            }
            try
            {
                programAct.LoadDependencies();
            }
            catch
            {
                programAct.log.Info("LoadDepenedencies Failed the Unit test");
            }

            programAct.CreateSchedules(systemTasks);
            programAct.EvaluateSchedules();
            //TEST SCHED COUNT
            int schedCountExp = 24;
            double schedCountAct = programAct.schedules.Count;
            Assert.AreEqual(schedCountExp, schedCountAct);

            //TEST SCHED SCORE & SORT
            //Expect 24 schedules, [0] with score of 5 and [1-4] with score of 4
            int diffExp = 1;
            double diffAct = programAct.schedules[0].ScheduleValue - programAct.schedules[1].ScheduleValue;
            Assert.AreEqual(diffExp, diffAct);

            //TEST EVENTS SCHEDULED

            Task target3 = programAct.schedules[0].AllStates.Events.Pop().Tasks[programAct.AssetList[0]];
            Task target2 = programAct.schedules[0].AllStates.Events.Pop().Tasks[programAct.AssetList[0]];
            Task target11 = programAct.schedules[0].AllStates.Events.Pop().Tasks[programAct.AssetList[0]];
            Task target0 = programAct.schedules[0].AllStates.Events.Pop().Tasks[programAct.AssetList[0]];

            Task task3 = systemTasks.Pop();
            Task task2 = systemTasks.Pop();
            Task task11 = systemTasks.Pop();
            Task task1 = systemTasks.Pop();
            Task task0 = systemTasks.Pop();

            Assert.AreEqual(target0, task0);
            Assert.AreEqual(target11, task11);
            Assert.AreEqual(target2, task2);
            Assert.AreEqual(target3, task3);

        }
        [Test]
        public void cropSchedulesUnitTest_NoPregen()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub_crop_DynamicECI.xml");

            Stack<Task> systemTasks = programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
            }
            try
            {
                programAct.LoadDependencies();
            }
            catch
            {
                programAct.log.Info("LoadDepenedencies Failed the Unit test");
            }

            try
            {
                programAct.CreateSchedules(systemTasks);
            }
            catch
            {
                programAct.log.Info("CreateSchedules Failed the Unit test");
            }

            try
            {
                double maxSched = programAct.EvaluateSchedules();
            }
            catch
            {
                programAct.log.Info("EvaluateSchedules Failed the Unit test");
            }



            //TEST SCHED COUNT
            int schedCountExp = 8;
            double schedCountAct = programAct.schedules.Count;
            Assert.AreEqual(schedCountExp, schedCountAct);

            //TEST SCHED SCORE & SORT
            //Expect 2 schedules, [0] with score of 3 [max possible] and [1] with score of 0
            int diffExp = 1;
            double diffAct = programAct.schedules[0].ScheduleValue - programAct.schedules[1].ScheduleValue;
            Assert.AreEqual(diffExp, diffAct);

            double emptySchedEventCount = programAct.schedules[7].AllStates.Events.Count();
            Assert.AreEqual(0, emptySchedEventCount);
        }
        [Test]
        public void EvaluatorFactoryUnitTest()
        {
            Program programAct = new Program();
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");


            Stack<Task> systemTasks = programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
            }
            try
            {
                programAct.LoadDependencies();
            }
            catch
            {
                programAct.log.Info("LoadDepenedencies Failed the Unit test");
            }

            XmlNode evaluatorNode = null;
            Evaluator schedEvaluator = EvaluatorFactory.GetEvaluator(evaluatorNode, programAct._dependencies);
            double ExpDepCount = 9;
            string ActDepCount = schedEvaluator.ToString();
            Assert.Inconclusive("Not Implemented");
        }
    }
}



// From Prev Students

//    // Get the input filenames
//    //string simulationInputFilePath = args[1];
//    //string targetDeckFilePath = args[2];
//    //string modelInputFileName = args[3];
//    //string outputPath = args[4];
//    var simulationInputFilePath = Path.Combine(baselocation, @"SimulationInput.XML"); // @"C:\Users\admin\Documents\Visual Studio 2015\Projects\Horizon-Simulation-Framework\Horizon_v2_3\io\SimulationInput.XML");
//    var targetDeckFilePath = Path.Combine(baselocation, @"v2.2-300targets.xml");
//    var modelInputFilePath = Path.Combine(baselocation, @"Model_Static.xml");
//    // Initialize critical section for dependencies ??Morgan Doesn't know what this does
//    // InitializeCriticalSection(&horizon::sub::dep::NodeDependencies::cs);


//    // Find the main input node from the XML input files
//    //var XmlDoc = new XmlDocument();
//    //XmlDoc.Load(simulationInputFilePath);
//    //XmlNodeList simulationInputXMLNodeList = XmlDoc.GetElementsByTagName("SCENARIO");
//    //var XmlEnum = simulationInputXMLNodeList.GetEnumerator();
//    //XmlEnum.MoveNext();
//    //var simulationInputXMLNode = (XmlNode)XmlEnum.Current;
//    //var scenarioName = simulationInputXMLNode.Attributes["scenarioName"].InnerXml;
//    //Console.Write("EXECUITING SCENARIO: ");
//    //Console.WriteLine(scenarioName);

//    //// Load the simulation parameters from the XML simulation input file
//    //XmlNode simParametersXMLNode = simulationInputXMLNode["SIMULATION_PARAMETERS"];
//    //bool simParamsLoaded = SimParameters.LoadSimParameters(simParametersXMLNode, scenarioName);
//    XmlNode evaluatorNode = XmlParser.ParseSimulationInput(simulationInputFilePath);
//    // Load the scheduler parameters defined in the XML simulation input file
//    //XmlNode schedParametersXMLNode = simulationInputXMLNode["SCHEDULER_PARAMETERS"];
//    //Scheduler systemScheduler = new Scheduler();
//    //bool schedParamsLoaded = loadSchedulerParams(schedParametersXMLNode, systemScheduler);

//    //bool paramsLoaded = SchedParameters.LoadSchedParameters(schedParametersXMLNode);
//    Scheduler systemScheduler = new Scheduler();
//    //MultiThreadedScheduler* systemScheduler = new MultiThreadedScheduler;



//    // Load the target deck into the targets list from the XML target deck input file
//    //var XmlDoc = new XmlDocument();
//    //XmlDoc.Load(targetDeckFilePath);
//    //XmlNodeList targetDeckXMLNodeList = XmlDoc.GetElementsByTagName("TARGETDECK");
//    //int numTargets = XmlDoc.GetElementsByTagName("TARGET").Count;
//    //XmlEnum = targetDeckXMLNodeList.GetEnumerator();
//    //XmlEnum.MoveNext();
//    //var targetDeckXMLNode = (XmlNode)XmlEnum.Current;
//    //Stack<Task> systemTasks = new Stack<Task>();
//    //bool targetsLoaded = Task.loadTargetsIntoTaskList(targetDeckXMLNode, systemTasks);
//    //Console.WriteLine("Initial states set");

//    // Load the target deck into the targets list from the XML target deck input file
//    Stack<Task> systemTasks = new Stack<Task>();
//    bool targetsLoaded = Task.loadTargetsIntoTaskList(XmlParser.GetTargetNode(targetDeckFilePath), systemTasks);
//    if (!targetsLoaded)
//    {
//        Assert.Fail("Failed to load targets");
//    }
//    // Find the main model node from the XML model input file
//    var modelInputXMLNode = XmlParser.GetModelNode(modelInputFilePath);

//    // Find the main model node from the XML model input file
//    //xmldoc.load(modelinputfilepath);
//    //xmlnodelist modelxmlnodelist = xmldoc.getelementsbytagname("model");
//    //xmlenum = modelxmlnodelist.getenumerator();
//    //xmlenum.movenext();
//    //var modelinputxmlnode = (xmlnode)xmlenum.current;


//    // Load the environment. First check if there is an ENVIRONMENT XMLNode in the input file
//    Universe SystemUniverse = null;
//    foreach (XmlNode modelChileNode in modelInputXMLNode.ChildNodes)
//    {

//    }
//    if (SystemUniverse == null)
//        SystemUniverse = new Universe();

//    //Create singleton dependency dictionary
//    Dependency dependencies = Dependency.Instance;

//    // Initialize List to hold assets and subsystem nodes
//    List<Asset> assetList = new List<Asset>();
//    List<Subsystem> subNodeList = new List<Subsystem>();

//    // Maps used to set up preceeding nodes
//    Dictionary<ISubsystem, XmlNode> subsystemXMLNodeMap = new Dictionary<ISubsystem, XmlNode>();
//    Dictionary<string, Subsystem> subsystemMap = new Dictionary<string, Subsystem>();
//    List<KeyValuePair<string, string>> dependencyMap = new List<KeyValuePair<string, string>>();
//    List<KeyValuePair<string, string>> dependencyFcnMap = new List<KeyValuePair<string, string>>();
//    // Dictionary<string, ScriptedSubsystem> scriptedSubNames = new Dictionary<string, ScriptedSubsystem>();

//    // Create Constraint list 
//    List<Constraint> constraintsList = new List<Constraint>();

//    //Create Lists to hold all the initial condition and dependency nodes to be parsed later
//    List<XmlNode> ICNodes = new List<XmlNode>();
//    List<XmlNode> DepNodes = new List<XmlNode>();
//    SystemState initialSysState = new SystemState();

//    // Enable Python scripting support, add additional functions defined in input file
//    bool enableScripting = false;
//    // Set up Subsystem Nodes, first loop through the assets in the XML model input file
//    foreach (XmlNode modelChildNode in modelInputXMLNode.ChildNodes)
//    {
//        if (modelChildNode.Attributes["ENVIRONMENT"] != null)
//        {
//            // Create the Environment based on the XMLNode
//            SystemUniverse = new Universe(modelChildNode);
//        }
//        if (modelChildNode.Name.Equals("ASSET"))
//        {
//            Asset asset = new Asset(modelChildNode);
//            assetList.Add(asset);
//            // Loop through all the of the ChildNodess for this Asset
//            foreach (XmlNode childNode in modelChildNode.ChildNodes)
//            {
//                // Get the current Subsystem XML Node, and create it using the SubsystemFactory
//                if (childNode.Name.Equals("SUBSYSTEM"))
//                {  //is this how we want to do this?
//                   // Check if the type of the Subsystem is scripted, networked, or other
//                    string subName = SubsystemFactory.GetSubsystem(childNode, dependencies, asset, subsystemMap);
//                    foreach (XmlNode ICorDepNode in childNode.ChildNodes)
//                    {
//                        if (ICorDepNode.Name.Equals("IC"))
//                            ICNodes.Add(ICorDepNode);
//                        if (ICorDepNode.Name.Equals("DEPENDENCY"))
//                        {
//                            string depSubName = "", depFunc = "");
//                            if (ICorDepNode.Attributes["subsystemName"] != null)
//                                depSubName = ICorDepNode.Attributes["subsystemName"].Value.ToString();
//                            else
//                                throw new MissingMemberException("Missing subsystem name in " + asset.Name);
//                            dependencyMap.Add(new KeyValuePair<string, string>(subName, depSubName));

//                            if (ICorDepNode.Attributes["fcnName"] != null)
//                                depFunc = ICorDepNode.Attributes["fcnName"].Value.ToString();
//                            else
//                                throw new MissingMemberException("Missing dependency function for subsystem" + subName);
//                            dependencyFcnMap.Add(new KeyValuePair<string, string>(subName, depFunc));
//                        }
//                        //  if (ICorDepNode.Name.Equals("DEPENDENCY_FCN"))
//                        //     dependencyFcnMap.Add(childNode.Attributes["subsystemName"].Value.ToString(), ICorDepNode.Attributes["fcnName"].Value.ToString());

//                    }
//                    //Parse the initial condition nodes


//                }
//                //Create a new Constraint
//                if (childNode.Name.Equals("CONSTRAINT"))
//                {
//                    constraintsList.Add(ConstraintFactory.GetConstraint(childNode, subsystemMap, asset));
//                }
//            }
//            if (ICNodes.Count > 0)
//                initialSysState.Add(SystemState.setInitialSystemState(ICNodes, asset));
//            ICNodes.Clear();
//        }
//    }
//    if (SystemUniverse == null)
//        SystemUniverse = new Universe();
//    //Add all the dependent subsystems to tge dependent subsystem list of the subsystems
//    foreach (KeyValuePair<string, string> depSubPair in dependencyMap)
//    {
//        Subsystem subToAddDep, depSub;
//        subsystemMap.TryGetValue(depSubPair.Key, out subToAddDep);
//        subsystemMap.TryGetValue(depSubPair.Value, out depSub);
//        subToAddDep.DependentSubsystems.Add(depSub);
//    }

//    //give the dependency functions to all the subsytems that need them
//    foreach (KeyValuePair<string, string> depFunc in dependencyFcnMap)
//    {
//        Subsystem subToAddDep;
//        subsystemMap.TryGetValue(depFunc.Key, out subToAddDep);
//        subToAddDep.SubsystemDependencyFunctions.Add(depFunc.Value, dependencies.GetDependencyFunc(depFunc.Value));
//    }
//    Console.WriteLine("Dependencies Loaded");

//    List<Constraint> constraintList = new List<Constraint>();
//    SystemClass system = new SystemClass(assetList, subNodeList, constraintList, SystemUniverse);
//    TargetValueEvaluator scheduleEvaluator = new TargetValueEvaluator(dependencies);

//    systemScheduler.GenerateSchedules(system, systemTasks, initialSysState);

//    Assert.AreEqual(1, 1);