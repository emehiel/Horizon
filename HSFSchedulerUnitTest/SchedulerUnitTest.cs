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

            Stack<Task> systemTasks = SchedulerHelper(ref programAct);

            XmlNode evalNode = XmlParser.ParseSimulationInput(programAct.SimulationInputFilePath);

            Evaluator schedEvaluator = EvaluatorFactory.GetEvaluator(evalNode, programAct._dependencies);
           
            programAct.simSystem = new SystemClass(programAct.AssetList, programAct.SubList, programAct.ConstraintsList, programAct.SystemUniverse);
            Scheduler scheduler = new Scheduler(schedEvaluator);
            
            List<SystemSchedule> schedules = scheduler.GenerateSchedules(programAct.simSystem, systemTasks, programAct.InitialSysState);
            schedules.Sort((x, y) => x.ScheduleValue.CompareTo(y.ScheduleValue));
            schedules.Reverse();

            //TEST SCHED COUNT
            double schedCountExp = 24;
            double schedCountAct = schedules.Count;
            Assert.AreEqual(schedCountExp, schedCountAct);

            //TEST SCHED SCORE & SORT
            //Expect 24 schedules, [0] with score of 5 and [1-4] with score of 4
            int diffExp = 1;
            double diffAct = schedules[0].ScheduleValue - schedules[1].ScheduleValue;
            Assert.AreEqual(diffExp, diffAct);

            //Assert.AreEqual(highestValTargetExp, highestValTargetAct);
            Task target3  = schedules[0].AllStates.Events.Pop().Tasks[programAct.AssetList[0]];
            Task target2  = schedules[0].AllStates.Events.Pop().Tasks[programAct.AssetList[0]];
            Task target11 = schedules[0].AllStates.Events.Pop().Tasks[programAct.AssetList[0]];
            Task target0  = schedules[0].AllStates.Events.Pop().Tasks[programAct.AssetList[0]];
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
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_Checker.xml");

            Stack<Task> systemTasks = SchedulerHelper(ref programAct);
            programAct.CreateSchedules(systemTasks);

            //TEST SCHED COUNT before crop,  if fail, then doesnt test the right scenario which is when the schedule count exceeds maxNumScheds
            double schedCountAct = programAct.schedules.Count;
            Assert.IsTrue(schedCountAct > SchedParameters.MaxNumScheds);

            //Crop Schedules
            Evaluator eval = new TargetValueEvaluator(programAct._dependencies);
            Scheduler scheduler = new Scheduler(eval);
            SystemSchedule empty = new SystemSchedule(programAct.schedules[0].AllStates);
            
            scheduler.CropSchedules(programAct.schedules, eval, programAct.schedules[0]);

            Assert.AreEqual(SchedParameters.NumSchedCropTo, programAct.schedules.Count());

        }

        [Test]
        public void generateSchedulesUnitTest_NoPregen()
        {
            //Tests the same generate Schedules (main scheduler loop) but if access cannot be pre-generated
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub_DynamicECI.xml");
            Stack<Task> systemTasks = SchedulerHelper(ref programAct);

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

            Stack<Task> systemTasks = SchedulerHelper(ref programAct);
            programAct.CreateSchedules(systemTasks);

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
        public void PreGenGenerateExhaustiveSystemSchedulesUnitTest()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_access.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub.xml");
            Stack<Task> systemTasks = SchedulerHelper(ref programAct);
            
            SystemClass simSystem = new SystemClass(programAct.AssetList, programAct.SubList, programAct.ConstraintsList, programAct.SystemUniverse);
            Stack<Access> preGeneratedAccesses = Access.pregenerateAccessesByAsset(simSystem, systemTasks, 0, 1, 1);
            Stack<Stack<Access>> scheduleCombos = new Stack<Stack<Access>>();
            scheduleCombos = Scheduler.GenerateExhaustiveSystemSchedules(preGeneratedAccesses, simSystem, 0);

            
            string actAccess1 = scheduleCombos.Pop().Pop().ToString();
            string actAccess2 = scheduleCombos.Pop().Pop().ToString();


            string expAccess1 = "asset1_to_target3";
            string expAccess2 = "asset1_to_target2";

            Assert.AreEqual(expAccess1, actAccess1);
            Assert.AreEqual(expAccess2, actAccess2);
        }

        public Stack<Task> SchedulerHelper(ref Program programAct)
        {

            Stack<Task> systemTasks = programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
                Assert.Fail();
                return systemTasks;

            }
            try
            {
                programAct.LoadDependencies();
            }
            catch
            {
                programAct.log.Info("LoadDepenedencies Failed the Unit test");
                return systemTasks;
                Assert.Fail();

            }
            return systemTasks;
        }

    }
}