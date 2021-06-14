using System;
using NUnit.Framework;
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
    [TestFixture]
    public class ProgramUnitTest
    {
        /// <summary>
        /// Test to make sure the correct number of targets are created
        /// </summary>
        [Test]
        public void LoadTargetsUnitTest()
        {
            Program TestProgram = new Program();
            TestProgram.SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";
            TestProgram.TargetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml";
            TestProgram.ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel.xml";

            
            Stack<Task> systemTasks = TestProgram.LoadTargets();

            double actual = systemTasks.Count;
            double expected = 17;

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        /// Test to verify that the correct Constraints are created
        /// </summary>
        [Test]
        public void LoadConstraintsUnitTest()
        {
            Program program = new Program();
            program.SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";
            program.TargetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml";
            program.ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel.xml";

            Stack<Task> systemTasks = program.LoadTargets();
            program.LoadSubsystems();
            Assert.AreEqual("power",program.ConstraintsList[0].Name);
            Assert.AreEqual("ssdr", program.ConstraintsList[1].Name);

        }
        /// <summary>
        /// Test to make sure the correct number of schedules are generated
        /// </summary>
        [Test]
        public void ScheduleCountUnitTest()
        {
            string simulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";
            string targetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml";
            string modelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel.xml";
            //this is a test - declan comit

            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };

            Program program = new Program();
            program.InitInput(inputArg);
            Stack<Task> systemTasks = program.LoadTargets();
            program.LoadSubsystems();
            program.LoadDependencies();
            program.CreateSchedules(systemTasks);

            double actual = program.schedules.Count;
            double expected = 17; 
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// Test to make sure that the maximum schedule score is generated
        /// </summary>
        [Test]
        public void ScheduleScoreUnitTest()
        {
            string simulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";
            string targetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml";
            string modelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel.xml";
            //this is a test - declan comit

            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };

            Program program = new Program();
            program.InitInput(inputArg);
            Stack<Task> systemTasks = program.LoadTargets();
            program.LoadSubsystems();
            program.LoadDependencies();
            program.CreateSchedules(systemTasks);
            double maxSched = program.EvaluateSchedules();

            double actual = maxSched;
            double expected = 20; //TODO: Find Actual schedule score that should be generated

            Assert.AreEqual(expected, actual);

        }
        //TODO: InitInput
        [Test]
        public void InitInput_NonAeolus()
        {
            //if non-null files are 
            string simulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";
            string targetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml";
            string modelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel.xml";

            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };

            Program program = new Program();
            program.InitInput(inputArg);

            string actual_simpath = program.SimulationInputFilePath;
            string expected_simpath = simulationInputFilePath;
            Assert.AreEqual(expected_simpath,actual_simpath);

            string actual_targpath = program.TargetDeckFilePath;
            string expected_targpath = targetDeckFilePath;
            Assert.AreEqual(expected_targpath, actual_targpath);

            string actual_modpath = program.ModelInputFilePath;
            string expected_modpath = modelInputFilePath;
            Assert.AreEqual(expected_modpath, actual_modpath);

        }
        [Test]
        public void InitInput_Aeolus()
        {
            //if non-null files are 

            string simulationInputFilePath = @"..\..\..\SimulationInput.XML";
            string targetDeckFilePath = @"..\..\..\v2.2-300targets.xml";
            string modelInputFilePath = @"..\..\..\DSAC_Static.xml";

            Program program = new Program();
            string[] args = { };
            program.InitInput(args);

            string actual_simpath = program.SimulationInputFilePath;
            string expected_simpath = simulationInputFilePath;
            Assert.AreEqual(expected_simpath, actual_simpath);

            string actual_targpath = program.TargetDeckFilePath;
            string expected_targpath = targetDeckFilePath;
            Assert.AreEqual(expected_targpath, actual_targpath);

            string actual_modpath = program.ModelInputFilePath;
            string expected_modpath = modelInputFilePath;
            Assert.AreEqual(expected_modpath, actual_modpath);

        }
        
        [Test]
        public void InitOutputUnitTest() // really unsure of this implementation
        {
            var outputFileName = string.Format("output-{0:yyyy-MM-dd}-1", DateTime.Now);
            string expected = "C:\\HorizonLog\\";
            string txt = ".txt";
            expected += outputFileName + txt;

            Program program = new Program();
            string actual = program.InitOutput();
            Assert.AreEqual(expected, actual);

        }
        [Test]
        public void LoadSubsystemUnitTest()
        {
            //Start Actual method call
            Program programAct = new Program();
            programAct.SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";
            programAct.TargetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml";
            programAct.ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel_Simple.xml";

            var evaluatorNode = XmlParser.ParseSimulationInput(programAct.SimulationInputFilePath);
            programAct.LoadTargets();
            programAct.LoadSubsystems();
            var subsystemMapAct = programAct.SubsystemMap;

            Assert.AreEqual(programAct.AssetList[0].Name, "asset1");
            double numAsset = programAct.AssetList.Count;
            double numConstraints = programAct.ConstraintsList.Count;
            double numDependencies = programAct.DependencyMap.Count;
            double numDepFcn = programAct.DependencyFcnMap.Count;
            double numSubs = programAct.SubList.Count;
            // End Actual method call


            //Expected
            double expAsset = 1;
            double expConstraint = 1;
            double expDependencies = 2;
            double expDepFcn = 1;
            double expSubs = 3;

            //Assert
            Assert.AreEqual(expAsset, numAsset);
            Assert.AreEqual(expConstraint, numConstraints);
            Assert.AreEqual(expDependencies, numDependencies);
            Assert.AreEqual(expDepFcn, numDepFcn);
            Assert.AreEqual(expSubs, numSubs);

            // Start Expected Output construction
            // Now Construct without For loop in LoadSub()
            //Program programExp = new Program();
            //programExp.SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";
            //programExp.TargetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml";
            //programExp.ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel_Simple.xml";

            //var modelInputXMLNode = XmlParser.GetModelNode(programExp.ModelInputFilePath);
            //Dependency dependencies = Dependency.Instance;
            //Dictionary<string, Subsystem> subsystemMapExp = new Dictionary<string, Subsystem>();
            //List<XmlNode> ICNodes = new List<XmlNode>();
            //List<XmlNode> DepNodes = new List<XmlNode>();
            //List<Constraint> constraintsList = new List<Constraint>();

            //XmlNode ignored = modelInputXMLNode.FirstChild;
            //XmlNode assetModelNode = modelInputXMLNode.NextSibling;
            //Asset asset = new Asset(assetModelNode);

            ////ASSET
            //Domain SystemUniverse = new SpaceEnvironment();
            ////asset.AssetDynamicState.Eoms.SetEnvironment(SystemUniverse);
            //List<Asset> assetList = new List<Asset>();
            //assetList.Add(asset);

            ////ACCESS
            //XmlNode accessNode = assetModelNode.FirstChild;
            //string ACCESS = SubsystemFactory.GetSubsystem(accessNode, dependencies, asset, subsystemMapExp);

            ////ADCS
            //XmlNode adcsNode = accessNode.NextSibling;
            //string ADCS = SubsystemFactory.GetSubsystem(adcsNode, dependencies, asset, subsystemMapExp);
            //XmlNode adcsICNode = adcsNode.FirstChild;
            //ICNodes.Add(adcsICNode);
            //XmlNode adcsDepNode = adcsNode.LastChild;
            //DepNodes.Add(adcsDepNode);

            ////SSDR Sensor
            //XmlNode ssdrNode = adcsNode.NextSibling;
            //string SSDR = SubsystemFactory.GetSubsystem(ssdrNode, dependencies, asset, subsystemMapExp);
            //XmlNode ssdrICNode = ssdrNode.FirstChild;
            //ICNodes.Add(ssdrICNode);
            //XmlNode ssdrDepNode = ssdrNode.LastChild;
            //DepNodes.Add(ssdrDepNode);

            //XmlNode ssdrConstraint = ssdrNode.NextSibling;
            //constraintsList.Add(ConstraintFactory.GetConstraint(ssdrConstraint, subsystemMapExp, asset));

           

            //Assert.AreEqual(subsystemMapExp,subsystemMapAct);
            //Assert.AreEqual(programAct.AssetList, programExp.AssetList);
            //Assert.AreEqual(programExp.ConstraintsList, programAct.ConstraintsList);
            //Assert.AreEqual(programExp.DependencyFcnMap, programAct.DependencyFcnMap);
            //Assert.AreEqual(programExp.SubList, programAct.SubList);

        }
        [Test]
        public void LoadDependenciesUnitTest()
        {
            //Start Actual method call
            Program programAct = new Program();
            programAct.SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.xml";
            programAct.TargetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml";
            programAct.ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel.xml";

            //var evaluatorNode = XmlParser.ParseSimulationInput(programAct.SimulationInputFilePath);
            programAct.LoadTargets();
            programAct.LoadSubsystems();
            programAct.LoadDependencies();
            Assert.AreEqual(0, programAct.SubList[0].DependentSubsystems.Count);
            Assert.AreEqual(programAct.SubList[0], programAct.SubList[1].DependentSubsystems[0]);
            Assert.AreEqual(1, programAct.SubList[2].SubsystemDependencyFunctions.Count);
            Assert.AreEqual(2, programAct.SubList[3].SubsystemDependencyFunctions.Count);

        }
        //public void CreateSchedulesTest()
        //{
        //    Program programAct = new Program();
        //    programAct.SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.xml";
        //    programAct.TargetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml";
        //    programAct.ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel.xml";

        //    Stack<Task> systemTasks = programAct.LoadTargets();
        //    programAct.LoadSubsystems();
        //    programAct.LoadDependencies();
        //    programAct.CreateSchedules(systemTasks);

        //    Assert.AreEqual(0, programAct.schedules.Count);
        //}
    }
}
