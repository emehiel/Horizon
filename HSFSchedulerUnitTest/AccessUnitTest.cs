using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Xml;
using Utilities;
using HSFUniverse;
using MissionElements;
using HSFScheduler;
using UserModel;
using HSFSystem;
using HSFSubsystem;
using Horizon;
using System.IO;

namespace HSFSchedulerUnitTest
{
    [TestFixture]
    public class AccessUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        /// <summary>
        /// Tests the only referenced constructor for the Access Class, Access(Asset asset, Task task)
        /// </summary>
        [Test]
        public void AccessConstructor()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_access.xml");
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

            //This is the only used constructor for the Access Class
            Access A1 = new Access(programAct.AssetList[0], systemTasks.Peek());

            Assert.IsInstanceOf(typeof(Access), A1);

            Assert.AreSame(systemTasks.Peek(), A1.Task);
            Assert.AreSame(programAct.AssetList[0], A1.Asset);


        }

        [Test]
        public void getCurrentAccesses()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_access.xml");
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

            Access A1 = new Access(programAct.AssetList[0], systemTasks.Pop());
            Access A2 = new Access(programAct.AssetList[0], systemTasks.Pop());
            Stack<Access> accesses = new Stack<Access>();
            accesses.Push(A2);
            accesses.Push(A1);

            Stack<Access> currentAcceses = Access.getCurrentAccesses(accesses, 0);
            Assert.AreEqual(A2, currentAcceses.Pop());
            Assert.AreEqual(A1, currentAcceses.Pop());
        }
        [Test]
        public void getCurrentAccessesForAsset()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub_accessmultiasset.xml");

            Stack<Task> systemTasks = programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
            }

            Access A1 = new Access(programAct.AssetList[0], systemTasks.Pop());
            Access A2 = new Access(programAct.AssetList[1], systemTasks.Pop());
            Stack<Access> accesses = new Stack<Access>();
            Stack<Access> empty = new Stack<Access>();
            accesses.Push(A2);
            accesses.Push(A1);

            Stack<Access> currentAcceses_Asset1 = Access.getCurrentAccessesForAsset(accesses, programAct.AssetList[0], 0);
            Stack<Access> currentAcceses_Asset2 = Access.getCurrentAccessesForAsset(accesses, programAct.AssetList[1], 0);

            Assert.AreEqual(A1, currentAcceses_Asset1.Pop());
            Assert.AreEqual(A2, currentAcceses_Asset2.Pop());
            Assert.AreEqual(empty, currentAcceses_Asset1);
            Assert.AreEqual(empty, currentAcceses_Asset2);
        }

        [Test]
        public void PregenAccessbyAsset()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_access.xml");
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

            SystemClass simSystem = new SystemClass(programAct.AssetList, programAct.SubList, programAct.ConstraintsList, programAct.SystemUniverse);

            Stack<Access> pregenAccess = Access.pregenerateAccessesByAsset(simSystem, systemTasks, 0, 1, 1);

            Stack<Access> AccessAct = Access.getCurrentAccesses(pregenAccess, 0);


            systemTasks.Pop(); //No access to last task, task4 so ot expected access
            Access Task3 = new Access(programAct.AssetList[0], systemTasks.Pop());
            Task3.AccessEnd = 2;
            Access Task2 = new Access(programAct.AssetList[0], systemTasks.Pop());
            Access Task1 = new Access(programAct.AssetList[0], systemTasks.Pop());
            Access Task0 = new Access(programAct.AssetList[0], systemTasks.Pop());


            Access ExpTask3 = AccessAct.Pop();
            Access ExpTask2 = AccessAct.Pop();

            //Asserts failed when comparing objects so ToString compares the imporant data
            Assert.AreEqual(ExpTask3.ToString(), Task3.ToString());
            Assert.AreEqual(ExpTask2.ToString(), Task2.ToString());
        }
    }
}