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
using Horizon;

namespace HSFSchedulerUnitTest
{
    [TestClass]
    public class CheckerUnitTest
    {
        [TestMethod]
        public void CheckScheduleUnitTest()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml";
            programAct.TargetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets_Scheduler.xml";
            programAct.ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel_TestSub_crop.xml";

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

            //SystemState initialStateList = new SystemState();
            Task unused = systemTasks.Pop();
            Task unused2 = systemTasks.Pop();
            Task task3 = systemTasks.Pop();  // should be task 1.1, which can only perform at time 1, not time 0
            Task task4 = systemTasks.Pop();


            Stack<Access> targ1 = new Stack<Access>(1);
            Stack<Access> targ2 = new Stack<Access>(1);
            targ1.Push(new Access(programAct.AssetList[0], task4));
            targ2.Push(new Access(programAct.AssetList[0], task3));

            SystemSchedule emptySchedule = new SystemSchedule(programAct.InitialSysState);

            var initialSchedule = new StateHistory(programAct.InitialSysState);
            SystemSchedule firstSchedule = new SystemSchedule(initialSchedule, targ1, 0);

            SystemSchedule secondSchedule = new SystemSchedule(initialSchedule, targ2, 0);

            SystemClass simSystem = new SystemClass(programAct.AssetList, programAct.SubList, programAct.ConstraintsList, programAct.SystemUniverse);
            bool check1 = Checker.CheckSchedule(simSystem, emptySchedule);
            bool check2 = Checker.CheckSchedule(simSystem, firstSchedule);
            bool check3 = Checker.CheckSchedule(simSystem, secondSchedule);

            Assert.IsTrue(check1);
            Assert.IsTrue(check2);
            Assert.IsFalse(check3);



        }
        [TestMethod]
        public void checkSubUnitTest() //Method is private, do i need to test?
        {
            {
                Program programAct = new Program();
                programAct.SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml";
                programAct.TargetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets_Scheduler.xml";
                programAct.ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel_TestSub_crop.xml";

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

                //SystemState initialStateList = new SystemState();
                Task unused = systemTasks.Pop();
                Task unused2 = systemTasks.Pop();
                Task task3 = systemTasks.Pop();  // should be task 1.1, which can only perform at time 1, not time 0
                Task task4 = systemTasks.Pop();
                Dictionary<Asset, Task> taskdic1 = new Dictionary<Asset, Task>();
                taskdic1.Add(programAct.AssetList[0], task4);

                Stack<Access> targ1 = new Stack<Access>(1);
                Stack<Access> targ2 = new Stack<Access>(1);
                targ1.Push(new Access(programAct.AssetList[0], task4));
                targ2.Push(new Access(programAct.AssetList[0], task3));

                SystemSchedule emptySchedule = new SystemSchedule(programAct.InitialSysState);

                var initialSchedule = new StateHistory(programAct.InitialSysState);
                SystemSchedule firstSchedule = new SystemSchedule(initialSchedule, targ1, 0);

                SystemSchedule secondSchedule = new SystemSchedule(initialSchedule, targ2, 0);

                SystemClass simSystem = new SystemClass(programAct.AssetList, programAct.SubList, programAct.ConstraintsList, programAct.SystemUniverse);
                bool check1 = Checker.CheckSchedule(simSystem, emptySchedule);
                bool check2 = Checker.CheckSchedule(simSystem, firstSchedule);
                bool check3 = Checker.CheckSchedule(simSystem, secondSchedule);

                Assert.IsTrue(check1);
                Assert.IsTrue(check2);
                Assert.IsFalse(check3);

            }
        }
    }
}
