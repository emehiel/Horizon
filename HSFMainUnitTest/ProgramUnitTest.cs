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
using System.IO;

namespace HSFMainUnitTest
{
    [TestFixture]
    public class ProgramUnitTest
    {
        // this string is for finding the path of the highest folder for Horizon on whatever is running the tests
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        /// <summary>
        /// Test runs Main with Aeolus input arguments.  0 is returned if main runs with no runtime errors.
        /// </summary>
        [Test]
        public void MainTest()
        {
            string simulationInputFilePath = Path.Combine(baselocation, @"SimulationInput.xml");
            string targetDeckFilePath = Path.Combine(baselocation, @"v2.2-300targets.xml");
            string modelInputFilePath = Path.Combine(baselocation, @"DSAC_Static.xml");

            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };

            int zero = Program.Main(inputArg);

            Assert.AreEqual(0, zero);            
        }
        /// <summary>
        /// Runs the primary functions in main with aeolus to check the output.
        /// Primarily looks at the highest performing schedule score and number of events
        /// </summary>
        [Test]
        public void AeolusTestRun()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Aeolus.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Aeolus.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_Aeolus.xml");
            Stack<Task> systemTasks = programAct.LoadTargets();
            programAct.LoadSubsystems();
            programAct.LoadDependencies();
            programAct.CreateSchedules(systemTasks);
            double maxSched = programAct.EvaluateSchedules();
            Assert.AreEqual(245, maxSched);
            Assert.AreEqual(26, programAct.schedules.Count);
            Assert.AreEqual(16, programAct.schedules[0].AllStates.Events.Count);


        }
        /// <summary>
        /// tests that the input argument string initialization works.  Same as assigning programAct.~FilePath
        /// </summary>
        [Test]
        public void InitInput_NonAeolus()
        {
            //if non-null files are 
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            string targetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");

            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };

            Program program = new Program();
            program.InitInput(inputArg);

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
        /// <summary>
        /// empty input arguments to initInput, tests that aeolus files load by default DOES THIS NEED TO EXIST?
        /// </summary>
        [Test]
        public void InitInput_Aeolus()
        {
            //if non-null files are 

            string simulationInputFilePath = @"..\..\..\SimulationInput.xml";
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



        /// <summary>
        /// Tests Output initialization files
        /// </summary>
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
        /// <summary>
        /// Test to make sure the correct number of targets are created, and target names and values are loaded
        /// </summary>
        /// 
        [Test]
        public void LoadTargetsUnitTest()
        {
            Program TestProgram = new Program();
            TestProgram.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            TestProgram.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            TestProgram.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");

            
            Stack<Task> systemTasks = TestProgram.LoadTargets();

            double actual = systemTasks.Count;
            Task task = systemTasks.Pop();
            double expected = 17;
            Assert.AreEqual(expected, actual);
            Assert.AreEqual("imagetarget16", task.Target.Name);
            Assert.AreEqual(10, task.Target.Value);
        }

        /// <summary>
        /// Tests the four main parts of LoadSubsystem: Universe, Assets, Subsystems, IC's, Dependencies, Dep Funcs, and Constraints
        /// </summary>
        [Test]
        public void LoadSubsystemUnitTest()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_Simple.xml");
            //programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_Simple.xml");

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
            Assert.IsInstanceOf(typeof(SpaceEnvironment), programAct.SystemUniverse);
            Assert.AreEqual(expAsset, numAsset);
            Assert.AreEqual("asset1", programAct.AssetList[0].Name);
            Assert.AreEqual(expConstraint, numConstraints);
            Assert.AreEqual("ssdr", programAct.ConstraintsList[0].Name);
            Assert.AreEqual(expDependencies, numDependencies);
            Assert.AreEqual("asset1.access", programAct.DependencyMap[0].Value);
            Assert.AreEqual("asset1.adcs", programAct.DependencyMap[0].Key);
            Assert.AreEqual(expDepFcn, numDepFcn);
            Assert.AreEqual("SSDRfromEOSensor.asset1", programAct.DependencyFcnMap[0].Value);
            Assert.AreEqual(expSubs, numSubs);
            Assert.AreEqual("asset1.access", programAct.SubList[0].Name);
            //Assert.AreEqual, programAct.InitialSysState.Ddata.Values);
        }
        /// <summary>
        /// Tests an example with more interdependences
        /// </summary>
        [Test]
        public void LoadDependenciesUnitTest()
        {
            //Start Actual method call
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");

            //var evaluatorNode = XmlParser.ParseSimulationInput(programAct.SimulationInputFilePath);
            programAct.LoadTargets();
            programAct.LoadSubsystems();
            programAct.LoadDependencies();
            Assert.AreEqual(0, programAct.SubList[0].DependentSubsystems.Count);
            Assert.AreEqual(programAct.SubList[0], programAct.SubList[1].DependentSubsystems[0]);
            Assert.AreEqual(1, programAct.SubList[2].SubsystemDependencyFunctions.Count);
            Assert.AreEqual(2, programAct.SubList[3].SubsystemDependencyFunctions.Count);
        }
        /// <summary>
        /// Test to make sure the correct number of schedules are generated
        /// </summary>
        [Test]
        public void CreateSchedulesUnitTest()
        {
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            string targetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");
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
        public void EvaluateSchedulesUnitTest()
        {
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            string targetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");

            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };

            Program program = new Program();
            program.InitInput(inputArg);
            Stack<Task> systemTasks = program.LoadTargets();
            program.LoadSubsystems();
            program.LoadDependencies();
            program.CreateSchedules(systemTasks);
            double maxSched = program.EvaluateSchedules();

            double actual = maxSched;
            double expected = 20;

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(maxSched, program.schedules[0].ScheduleValue);
            Assert.IsTrue(maxSched >= program.schedules[program.schedules.Count - 1].ScheduleValue);

        }
    }
}
