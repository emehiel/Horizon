using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Horizon;
using System.Xml;
using HSFUniverse;
using HSFSubsystem;
using MissionElements;
using System.Collections.Generic;
using HSFSystem;
using HSFScheduler;
using UserModel;
using HSFMainUnitTest;

namespace HSFMainUnitTest
{
    [TestClass]
    public class ProgramUnitTest
    {
        [TestMethod]
        public void TestScheduleScore()
        {
            var simulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";
            var targetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml";
            var modelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel.xml";

            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };

            MainTest TestProgram = new MainTest();

            TestProgram.SetInputPaths(inputArg);
            TestProgram.SetUpOutput();
            TestProgram.LoadTargets();
            TestProgram.SetUpDep();
            TestProgram.SetUpSubsystems();
            TestProgram.SetUpSchedules();
            TestProgram.EvaluateSchedule();
            //Program.Main(inputArg);
            //Console.WriteLine("Temp");
            //int index = 
        }


    }

    // Helper Functions


	public class MainTest
	{
        public string outputPath;
        public string simulationInputFilePath;
        public string targetDeckFilePath;
        public string modelInputFilePath;
        public XmlNode modelInputXMLNode;
        public Stack<Task> systemTasks;
        public List<Asset> assetList;
        public Universe SystemUniverse;
        XmlNode evaluatorNode;
        SystemClass simSystem;
        List<SystemSchedule> schedules;
        Evaluator schedEvaluator;
        List<Subsystem> subList;
        //Create singleton dependency dictionary
        Dependency dependencies = Dependency.Instance;
        // Maps used to set up preceeding nodes
        public Dictionary<ISubsystem, XmlNode> subsystemXMLNodeMap = new Dictionary<ISubsystem, XmlNode>();
        public Dictionary<string, Subsystem> subsystemMap = new Dictionary<string, Subsystem>();
        public List<KeyValuePair<string, string>> dependencyMap = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> dependencyFcnMap = new List<KeyValuePair<string, string>>();
        // Create Constraint list 
        List<Constraint> constraintsList = new List<Constraint>();
        //Create Lists to hold all the initial condition and dependency nodes to be parsed later
        List<XmlNode> ICNodes = new List<XmlNode>();
        List<XmlNode> DepNodes = new List<XmlNode>();
        SystemState initialSysState = new SystemState();
        public double maxSched;

        public void LoadTargets()
        {
            // Find the main input node from the XML input files
            evaluatorNode = XmlParser.ParseSimulationInput(simulationInputFilePath);

            // Load the target deck into the targets list from the XML target deck input file
            systemTasks = new Stack<Task>();
            bool targetsLoaded = Task.loadTargetsIntoTaskList(XmlParser.GetTargetNode(targetDeckFilePath), systemTasks);
            if (!targetsLoaded)
            {
                throw new Exception("Targets were not loaded.");
            }
            modelInputXMLNode = XmlParser.GetModelNode(modelInputFilePath);
        }


        public void EvaluateSchedule()
        {
            foreach (SystemSchedule systemSchedule in schedules)
            {
                systemSchedule.ScheduleValue = schedEvaluator.Evaluate(systemSchedule);
                //bool canExtendUntilEnd = true;
                // Extend the subsystem states to the end of the simulation 
                foreach (var subsystem in simSystem.Subsystems)
                {
                    if (systemSchedule.AllStates.Events.Count > 0)
                        if (!subsystem.CanExtend(systemSchedule.AllStates.Events.Peek(), simSystem.Environment, SimParameters.SimEndSeconds)) ;
                            //log.Error("Cannot Extend " + subsystem.Name + " to end of simulation");
                }
            }

            // Sort the sysScheds by their values
            schedules.Sort((x, y) => x.ScheduleValue.CompareTo(y.ScheduleValue));
            schedules.Reverse();
            maxSched = schedules[0].ScheduleValue;
        }

        public void SetUpSubsystems()
        {
            assetList = new List<Asset>();
            subList = new List<Subsystem>();
            // Set up Subsystem Nodes, first loop through the assets in the XML model input file
            foreach (XmlNode modelChildNode in modelInputXMLNode.ChildNodes)
            {
                if (modelChildNode.Name.Equals("ENVIRONMENT"))
                {
                    // Create the Environment based on the XMLNode
                    SystemUniverse = new Universe(modelChildNode);
                }
                if (modelChildNode.Name.Equals("ASSET"))
                {
                    Asset asset = new Asset(modelChildNode);
                    assetList.Add(asset);
                    // Loop through all the of the ChildNodess for this Asset
                    foreach (XmlNode childNode in modelChildNode.ChildNodes)
                    {
                        // Get the current Subsystem XML Node, and create it using the SubsystemFactory
                        if (childNode.Name.Equals("SUBSYSTEM"))
                        {  //is this how we want to do this?
                           // Check if the type of the Subsystem is scripted, networked, or other
                            string subName = SubsystemFactory.GetSubsystem(childNode, dependencies, asset, subsystemMap);
                            foreach (XmlNode ICorDepNode in childNode.ChildNodes)
                            {
                                if (ICorDepNode.Name.Equals("IC"))
                                    ICNodes.Add(ICorDepNode);
                                if (ICorDepNode.Name.Equals("DEPENDENCY"))
                                {
                                    string depSubName = "", depFunc = "";
                                    depSubName = Subsystem.parseNameFromXmlNode(ICorDepNode, asset.Name);
                                    dependencyMap.Add(new KeyValuePair<string, string>(subName, depSubName));

                                    if (ICorDepNode.Attributes["fcnName"] != null)
                                    {
                                        depFunc = ICorDepNode.Attributes["fcnName"].Value.ToString();
                                        dependencyFcnMap.Add(new KeyValuePair<string, string>(subName, depFunc));
                                    }
                                }
                            }
                        }
                        //Create a new Constraint
                        if (childNode.Name.Equals("CONSTRAINT"))
                        {
                            constraintsList.Add(ConstraintFactory.GetConstraint(childNode, subsystemMap, asset));
                        }
                    }
                    if (ICNodes.Count > 0)
                        initialSysState.Add(SystemState.setInitialSystemState(ICNodes, asset));
                    ICNodes.Clear();
                }
            }
            if (SystemUniverse == null)
                SystemUniverse = new Universe();
            foreach (KeyValuePair<string, Subsystem> sub in subsystemMap)
            {
                if (!sub.Value.GetType().Equals(typeof(ScriptedSubsystem)))//let the scripted subsystems add their own dependency collector
                    sub.Value.AddDependencyCollector();
                subList.Add(sub.Value);
            }
        }
        
        public void SetUpDep()
        {
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
                subToAddDep.SubsystemDependencyFunctions.Add(depFunc.Value, dependencies.GetDependencyFunc(depFunc.Value));
            }
        }

        public void SetUpSchedules()
        {
            simSystem = new SystemClass(assetList, subList, constraintsList, SystemUniverse);

            if (simSystem.CheckForCircularDependencies())
                throw new NotFiniteNumberException("System has circular dependencies! Please correct then try again.");

            schedEvaluator = EvaluatorFactory.GetEvaluator(evaluatorNode, dependencies);
            Scheduler scheduler = new Scheduler(schedEvaluator);
            schedules = scheduler.GenerateSchedules(simSystem, systemTasks, initialSysState);
            // Evaluate the schedules and set their values
        }

        public void SetInputPaths(string[] args)
        { 
            // Set Defaults
            simulationInputFilePath = @"..\..\..\SimulationInput.XML";
            targetDeckFilePath = @"..\..\..\v2.2-300targets.xml";
            modelInputFilePath = @"..\..\..\DSAC_Static.xml";

            // Get the input filenames
            int i = 0;
                foreach(var input in args)
                {
                    i++;
                    switch (input)
                    {
                        case "-s":
                            simulationInputFilePath = args[i];
                            break;
                        case "-t":
                            targetDeckFilePath = args[i];
                            break;
                        case "-m":
                            modelInputFilePath = args[i];
                            break;
                    }
                }
        }
        public void SetUpOutput()
        {
            // Initialize Output File
            var outputFileName = string.Format("output-{0:yyyy-MM-dd}-*", DateTime.Now);
            outputPath = @"C:\HorizonLog\";
            var txt = ".txt";
            string[] fileNames = System.IO.Directory.GetFiles(outputPath, outputFileName, System.IO.SearchOption.TopDirectoryOnly);
            double number = 0;
            foreach (var fileName in fileNames)
            {
                char version = fileName[fileName.Length - txt.Length - 1];
                if(number<Char.GetNumericValue(version))
                    number = Char.GetNumericValue(version);
            }
            number++;
            outputFileName = outputFileName.Remove(outputFileName.Length - 1) + number;
            outputPath += outputFileName + txt;
        }
    }


}
