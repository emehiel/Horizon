using System;
using NUnit.Framework;
using HSFScheduler;
using System.Xml;
using System.Collections.Generic;
using HSFSystem;
using MissionElements;
using Horizon;
using System.IO;

namespace HSFSchedulerUnitTest
{
    [TestFixture]
    public class CheckerUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        /// <summary>
        /// Tests the CheckSchedule which uses CheckSub, CheckSubs and CheckConstraints
        /// </summary>
        [Test]
        public void CheckScheduleUnitTest()
        {
            Program programAct = new Program();
            CheckerHelper(ref programAct);

            programAct.SystemTasks.Pop();
            programAct.SystemTasks.Pop();
            Task task3 = programAct.SystemTasks.Pop();  // should be task 1.1, which can only perform at time 1, not time 0
            Task task4 = programAct.SystemTasks.Pop();


            Stack<Access> targ1 = new Stack<Access>(1);
            Stack<Access> targ2 = new Stack<Access>(1);
            targ1.Push(new Access(programAct.AssetList[0], task4));
            targ2.Push(new Access(programAct.AssetList[0], task3));

            SystemSchedule emptySchedule = new SystemSchedule(programAct.InitialSysState);

            var initialSchedule = new StateHistory(programAct.InitialSysState);
            SystemSchedule firstSchedule = new SystemSchedule(initialSchedule, targ1, 0);

            SystemSchedule secondSchedule = new SystemSchedule(initialSchedule, targ2, 0);

            SystemState initialSched2 = new SystemState();
            string ICString = Path.Combine(baselocation, @"UnitTestInputs\ICNode.xml");
            var XmlDoc = new XmlDocument();

            XmlDoc.Load(ICString);
            XmlNodeList ICNodeList = XmlDoc.GetElementsByTagName("IC");
            var XmlEnum = ICNodeList.GetEnumerator();
            XmlEnum.MoveNext();
            XmlNode ICNode = (XmlNode)XmlEnum.Current;
            //List<XmlNode> ICNodes = new List<XmlNode>();
            //ICNodes.Add(ICNode);
            
            initialSched2.SetInitialSystemState(ICNode, "majorKey");
            //initialSched2.Add(SystemState.SetInitialSystemState(ICNode, programAct.AssetList[0]));
            SystemSchedule thirdSchedule = new SystemSchedule(initialSched2);

            SystemClass simSystem = new SystemClass(programAct.AssetList, programAct.SubList, programAct.ConstraintsList, programAct.SystemUniverse);
           
            //act
            bool check1 = Checker.CheckSchedule(simSystem, emptySchedule); //can accept empty sched
            bool check2 = Checker.CheckSchedule(simSystem, firstSchedule); //can accept valid schedule
            bool check3 = Checker.CheckSchedule(simSystem, secondSchedule); // cannot accept, no access (subcheck fails)
            bool check4 = Checker.CheckSchedule(simSystem, thirdSchedule); //cannot accept, IC breaks constraints
            programAct.SubList[0].IsEvaluated = true;
            bool check5 = Checker.CheckSchedule(simSystem, secondSchedule); //subcheck should fail, but sub isEvaluated, so can accept this schedule

            //assert
            Assert.IsTrue(check1);
            Assert.IsTrue(check2);
            Assert.IsFalse(check3);
            Assert.IsFalse(check4);

        }
       
        public void CheckerHelper(ref Program programAct)
        {

            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_Checker.xml");

            programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
                Assert.Fail();
            }
        }
    }
}
