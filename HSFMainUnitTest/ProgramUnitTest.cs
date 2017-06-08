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
        /// <summary>
        /// Test to make sure the correct number of targets are created
        /// </summary>
        [TestMethod]
        public void TestLoadTargets()
        {
            string simulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";
            string targetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml";
            string modelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel.xml";

            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };

            Program TestProgram = new Program();

            TestProgram.InitInput(inputArg);
            Stack<Task> systemTasks = TestProgram.LoadTargets();

            double actual = systemTasks.Count;
            double expected = 17;

            Assert.AreEqual(expected, actual);

        }
        /// <summary>
        /// Test to verify the correct dependencies are loaded
        /// </summary>
        [TestMethod, Ignore]
        public void TestDependencies()
        {

            string simulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML"; // TODO: Update
            string targetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml"; // TODO: Update
            string modelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel.xml"; //TODO: Update

            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };

            Program program = new Program();
            program.InitInput(inputArg);
            Stack<Task> systemTasks = program.LoadTargets();
            program.LoadSubsystems();
            program.LoadDependencies();
            Assert.Inconclusive("Not Implemented");
        }
        /// <summary>
        /// Test to verify that the correct Constraints are created
        /// </summary>
        [TestMethod, Ignore]
        public void TestConstraints()
        {
            string simulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML"; // TODO: Update
            string targetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml"; // TODO: Update
            string modelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel.xml"; //TODO: Update

            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };

            Program TestProgram = new Program();

            Program program = new Program();
            program.InitInput(inputArg);
            Stack<Task> systemTasks = program.LoadTargets();
            program.LoadSubsystems();
            Assert.Inconclusive("Not Implemented");
        }
        /// <summary>
        /// Test to make sure the correct number of schedules are generated
        /// </summary>
        [TestMethod]
        public void TestScheduleCount()
        {
            string simulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML"; // TODO: Update
            string targetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml"; // TODO: Update
            string modelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel.xml"; //TODO: Update

            string[] inputArg = { "-s", simulationInputFilePath, "-t", targetDeckFilePath, "-m", modelInputFilePath };

            Program TestProgram = new Program();

            Program program = new Program();
            program.InitInput(inputArg);
            Stack<Task> systemTasks = program.LoadTargets();
            program.LoadSubsystems();
            program.LoadDependencies();
            program.CreateSchedules(systemTasks);

            double actual = program.schedules.Count;
            double expected = 19; 
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// Test to make sure that the maximum schedule score is generated
        /// </summary>
        [TestMethod]
        public void TestScheduleScore()
        {
            string simulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.XML";
            string targetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets.xml";
            string modelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel.xml";

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
    }
}
