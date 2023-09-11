using System;
using NUnit.Framework;
using Horizon;
using HSFUniverse;
using MissionElements;
using System.Collections.Generic;
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
            //arrange
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            string targetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");

            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };
            int Expect_zero = 1;
            //act
            try
            {
                Expect_zero = Program.Main(inputArg);
            }
            //assert
            catch(DirectoryNotFoundException e)
            { Assert.Inconclusive(e.ToString()); }

            Assert.AreEqual(0, Expect_zero);            
        }

        /// <summary>
        /// tests that the input argument string initialization works.  Same as assigning programAct.~FilePath
        /// </summary>
        [Test]
        public void InitInput_NonAeolus()
        {
            //arrange
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            string targetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");
            string expected_simpath = simulationInputFilePath;
            string expected_targpath = targetDeckFilePath;
            string expected_modpath = modelInputFilePath;
            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };
            Program program = new Program();

            //act
            program.InitInput(inputArg);
            string actual_simpath = program.SimulationInputFilePath;
            string actual_targpath = program.TargetDeckFilePath;
            string actual_modpath = program.ModelInputFilePath;

            //assert
            Assert.AreEqual(expected_modpath, actual_modpath);
            Assert.AreEqual(expected_simpath, actual_simpath);
            Assert.AreEqual(expected_targpath, actual_targpath);
        }
        /// <summary>
        /// empty input arguments to initInput, tests that aeolus files load by default DOES THIS NEED TO EXIST?
        /// </summary>
        [Test]
        public void InitInput_Aeolus()
        {
            //if non-null files are 
            //arrange
            string simulationInputFilePath = @"..\..\..\SimulationInput.xml";
            string targetDeckFilePath = @"..\..\..\v2.2-300targets.xml";
            string modelInputFilePath = @"..\..\..\DSAC_Static.xml";
            string expected_simpath = simulationInputFilePath;
            string expected_targpath = targetDeckFilePath;
            string expected_modpath = modelInputFilePath;
            Program program = new Program();
            string[] args = { };

            //act
            program.InitInput(args);
            string actual_simpath = program.SimulationInputFilePath;
            string actual_targpath = program.TargetDeckFilePath;
            string actual_modpath = program.ModelInputFilePath;

            //assert
            Assert.AreEqual(expected_modpath, actual_modpath);
            Assert.AreEqual(expected_simpath, actual_simpath);
            Assert.AreEqual(expected_targpath, actual_targpath);

        }



        /// <summary>
        /// Tests Output initialization files, cant figure a good way to test this on git servers, or to test the version control (which works 9.15.21)
        /// </summary>
        [Test]
        public void InitOutputUnitTest() 
        {
            //arrange
            var outputFileName = string.Format("output-{0:yyyy-MM-dd}-", DateTime.Now);
            string expected = "C:\\HorizonLog\\";
            string txt = ".txt";
            expected += outputFileName + txt;
            Program program = new Program();
            string actual = "";
            //act
            try
            {
                actual = program.InitOutput();

                actual = actual.Remove(32);
                actual = actual + ".txt";//removes the version, version is an issue if running ProgramMain test multiple times per day
            }
            //assert
            catch (DirectoryNotFoundException e)
            { Assert.Inconclusive(e.ToString()); }


            Assert.AreEqual(expected, actual);

        }
        /// <summary>
        /// Test to make sure the correct number of targets are created, and target names and values are loaded
        /// </summary>
        /// 
        [Test]
        public void LoadTargetsUnitTest()
        {
            //arrange
            Program TestProgram = new Program();
            TestProgram.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            TestProgram.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            TestProgram.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");
            double exp_TaskCount = 17;
            double exp_TaskVal = 10;
            
            //act
            Stack<Task> systemTasks = TestProgram.LoadTargets();
            double act_TaskCount = systemTasks.Count;
            Task task = systemTasks.Pop();
            //assert
            Assert.AreEqual(exp_TaskCount, act_TaskCount);
            Assert.AreEqual("imagetarget16", task.Target.Name);
            Assert.AreEqual(exp_TaskVal, task.Target.Value);
        }

        /// <summary>
        /// Tests the four main parts of LoadSubsystem: Universe, Assets, Subsystems, IC's, Dependencies, Dep Funcs, and Constraints
        /// </summary>
        [Test]
        public void LoadSubsystemUnitTest()
        {
            //arrange
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_Simple.xml");
            double expAsset = 1;
            double expConstraint = 1;
            double expDependencies = 2;
            double expDepFcn = 1;
            double expSubs = 3;            
            programAct.LoadTargets();


            //act
            programAct.LoadSubsystems();
            var subsystemMapAct = programAct.SubsystemMap;
            double numAsset = programAct.AssetList.Count;
            double numConstraints = programAct.ConstraintsList.Count;
            double numDependencies = programAct.DependencyList.Count;
            double numDepFcn = programAct.DependencyFcnList.Count;
            double numSubs = programAct.SubList.Count;

            //assert
            Assert.AreEqual(programAct.AssetList[0].Name, "asset1");
            Assert.IsInstanceOf(typeof(SpaceEnvironment), programAct.SystemUniverse);
            Assert.AreEqual(expAsset, numAsset);
            Assert.AreEqual("asset1", programAct.AssetList[0].Name);
            Assert.AreEqual(expConstraint, numConstraints);
            Assert.AreEqual("ssdr", programAct.ConstraintsList[0].Name);
            Assert.AreEqual(expDependencies, numDependencies);
            Assert.AreEqual("asset1.access", programAct.DependencyList[0].Value);
            Assert.AreEqual("asset1.adcs", programAct.DependencyList[0].Key);
            Assert.AreEqual(expDepFcn, numDepFcn);
            Assert.AreEqual("SSDRfromEOSensor.asset1", programAct.DependencyFcnList[0].Value);
            Assert.AreEqual(expSubs, numSubs);
            Assert.AreEqual("asset1.access", programAct.SubList[0].Name);
            //Assert.AreEqual, programAct.InitialSysState.Ddata.Values); //maybe test this? so many objects created in this method
        }
        /// <summary>
        /// Tests an example with more interdependences
        /// </summary>
        [Test]
        public void LoadDependenciesUnitTest()
        {
            //arrange
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");
            programAct.LoadTargets();
            programAct.LoadSubsystems();

            //act
            programAct.LoadDependencies();

            //assert
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
            //arrange
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            string targetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");

            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };

            Program program = new Program();
            program.InitInput(inputArg);
            Stack<Task> systemTasks = program.LoadTargets();
            program.LoadSubsystems();
            program.LoadDependencies();

            //act
            program.CreateSchedules(systemTasks);

            //assert
            double actual = program.Schedules.Count;
            double expected = 17;
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// Test to make sure that the maximum schedule score is generated
        /// </summary>
        [Test]
        public void EvaluateSchedulesUnitTest()
        {
            //arrange
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
            double exp_MaxSched= 20;

            //act
            double act_maxSched = program.EvaluateSchedules();

            //assert
            Assert.AreEqual(exp_MaxSched, act_maxSched);
            Assert.AreEqual(act_maxSched, program.Schedules[0].ScheduleValue);
            Assert.IsTrue(act_maxSched >= program.Schedules[program.Schedules.Count - 1].ScheduleValue);

        }
    }
}
