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
    public class SystemScheduleUnitTest
    {
        public string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        public bool sysSchedInit = false;
        public List<XmlNode> ICNodes = new List<XmlNode>();
        public Asset asset;
        public Stack<Task> systemTasks;
        public SystemState systemState;
        public StateHistory initialHist;
        /// <summary>
        /// Tests first 3 constructors of SystemSchedule
        /// </summary>
        [Test]
        public void Constructors()
        {
            //arrange
            Program programAct = new Program();
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_EnviroRTA.xml");
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.ModelInputFilePath = modelInputFilePath;
            programAct.SimulationInputFilePath = simulationInputFilePath;

            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.LoadTargets();

            //XmlNode simNode = XmlParser.ParseSimulationInput(simulationInputFilePath);
            //XmlNode modelNode = XmlParser.GetModelNode(modelInputFilePath);
            //Asset asset = new Asset(modelNode.ChildNodes[1]);
            //SystemState systemState = new SystemState();
            
            ////List<XmlNode> ICNodes = new List<XmlNode>();
            ////ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[2].FirstChild);
            
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[2].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[1], asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[2], asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[4].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[5].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[6].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[6].ChildNodes[1], asset));

            ////SystemState systemState = SystemState.SetInitialSystemState(ICNodes, asset);
            //StateHistory expHist = new StateHistory(systemState);

            ////First Constructor test SystemSchedule(SystemState initialstates) 
            //SystemSchedule newSched = new SystemSchedule(systemState);

            ////Second Constructor test SystemSchedule(StateHistory allStates)
            //SystemSchedule newSched2 = new SystemSchedule(initialHist);

            ////Third Constructor test SystemSchedule(SystemState initialstates)
            //Dictionary<Asset, Task> eventDic = new Dictionary<Asset, Task>();
            //eventDic.Add(asset, systemTasks.Pop());
            //Event event1 = new Event(eventDic, systemState);
            //SystemSchedule newSched3 = new SystemSchedule(newSched2, event1);

            ////assert
            //Assert.That(newSched.AllStates, Has.Property("InitialState").EqualTo(initialHist.InitialState));
            //Assert.That(newSched2.AllStates, Has.Property("InitialState").EqualTo(initialHist.InitialState));
            //Assert.That(newSched3.AllStates, Has.Property("InitialState").EqualTo(initialHist.InitialState));
            //Assert.AreEqual(event1, newSched3.AllStates.Events.Pop());
        }
        /// <summary>
        /// Tests the final and largest constructor for SystemSchedule
        /// </summary>
        [Test]
        public void BigConstructor()
        {
            Program programAct = new Program();
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_EnviroRTA.xml");
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.ModelInputFilePath = modelInputFilePath;
            programAct.SimulationInputFilePath = simulationInputFilePath;

            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.LoadTargets();

            //XmlNode simNode = XmlParser.ParseSimulationInput(simulationInputFilePath);
            //XmlNode modelNode = XmlParser.GetModelNode(modelInputFilePath);
            //Asset asset = new Asset(modelNode.ChildNodes[1]);
            //SystemState systemState = new SystemState();

            ////List<XmlNode> ICNodes = new List<XmlNode>();
            ////ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[2].FirstChild);

            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[2].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[1], asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[2], asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[4].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[5].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[6].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[6].ChildNodes[1], asset));

            ////SystemState systemState = SystemState.SetInitialSystemState(ICNodes, asset);
            //StateHistory initialHist = new StateHistory(systemState);
            //Dictionary<Asset, Task> tasks = new Dictionary<Asset, Task>();

            //tasks.Add(asset, systemTasks.Peek());

            //Event event1 = new Event(tasks, systemState);
            //event1.SetEventEnd(asset, 1);
            //event1.SetTaskEnd(asset, 0);
            //event1.SetTaskStart(asset, 0);
            //event1.SetEventStart(asset, 0);

            //Event event2 = new Event(null, systemState);
            //event2.SetEventEnd(asset, 1);
            //event2.SetTaskEnd(asset, 0);
            //event2.SetTaskStart(asset, 0);
            //event2.SetEventStart(asset, 0);

            //StateHistory expHist1 = new StateHistory(initialHist, event1);
            //Stack<Access> access = new Stack<Access>();
            //Access newAccess = new Access(asset, systemTasks.Pop());
            //access.Push(newAccess);
            
            ////act
            //SystemSchedule newSched3 = new SystemSchedule(expHist1, access, 0);

            ////arrange a bit more
            //access.Pop(); //remove the task from above test to add a null one

            //StateHistory expHist2 = new StateHistory(initialHist, event2);
            //Access noAccess = new Access(asset, null);
            //access.Push(noAccess);

            ////act...again
            //SystemSchedule newSched4 = new SystemSchedule(expHist2, access, 0);

            ////assert
            //Assert.That(newSched3.AllStates, Has.Property("InitialState").EqualTo(expHist1.InitialState));
            //Assert.That(newSched4.AllStates, Has.Property("InitialState").EqualTo(expHist2.InitialState));
            //try
            //{
            //    WriteScheduleTest(newSched3);
            //}
            //catch
            //{
            //    Assert.Inconclusive("WriteSchedule not fully run");
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void CanAddTasks()
        {
            Program programAct = new Program();
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_SysScheduler.xml");
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_EnviroRTA.xml");

            XmlNode modelNode = XmlParser.GetModelNode(programAct.ModelInputFilePath);
            programAct.LoadTargets();
            Asset asset = new Asset(modelNode.ChildNodes[1]);
            SystemState systemState = new SystemState();

            //List<XmlNode> ICNodes = new List<XmlNode>();
            //ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[2].FirstChild);

            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[2].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[1], asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[2], asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[4].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[5].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[6].FirstChild, asset));
            //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[6].ChildNodes[1], asset));

            ////SystemState systemState = SystemState.SetInitialSystemState(ICNodes, asset);
            //StateHistory initialHist = new StateHistory(systemState);

            //Stack<Access> accessList = new Stack<Access>();
            //accessList.Push(new Access(asset, systemTasks.Pop()));
            //systemTasks.Pop();
            //systemTasks.Pop();

            //StateHistory expHist1 = new StateHistory(initialHist);

            //SystemSchedule sched1 = new SystemSchedule(expHist1);

            //Dictionary<Asset, Task> tasks = new Dictionary<Asset, Task>();

            //tasks.Add(asset, systemTasks.Peek());

            //Event event1 = new Event(tasks, systemState);
            //event1.SetEventEnd(asset, 1);
            //event1.SetTaskEnd(asset, 0);
            //event1.SetTaskStart(asset, 0);
            //event1.SetEventStart(asset, 0);

            //StateHistory eventHist = new StateHistory(initialHist, event1);
            //SystemSchedule sched2 = new SystemSchedule(eventHist, accessList, 0);

            ////act
            //bool AccessSchedCanAdd = sched1.CanAddTasks(accessList, 0);
            //bool noAccessSchedCantAdd = sched2.CanAddTasks(accessList, 0);//has event added during time 0-1 so shouldn't add

            ////assert
            //Assert.IsTrue(AccessSchedCanAdd);
            //Assert.IsFalse(noAccessSchedCantAdd); //need to figure out how to break this test
        }
        /// <summary>
        /// Tests the ability to write schedule into file stored at a desired path
        /// </summary>
        //[Test] TODO:  implement with Ci compatibility
        public void WriteScheduleTest(SystemSchedule schedule) //TODO test other data types, more than just initial conditions
        {
            //string path = Path.Combine(baselocation, @"UnitTestInputs\testWrite.xml");

            string path = @"testWrite";

            SystemSchedule.WriteSchedule(schedule, path);
            string file1 = Path.Combine(path, "asset1_databufferfillratio.csv");
            string file2 = Path.Combine(path, "asset1_datarate_mb_s_.csv");
            string file3 = Path.Combine(path, "asset1_depthofdischarge.csv");
            string file4 = Path.Combine(path, "asset1_eci_pointing_vector_xyz_.csv");
            string file5 = Path.Combine(path, "asset1_eosensoron.csv");
            string file6 = Path.Combine(path, "asset1_incidenceangle.csv");
            string file7 = Path.Combine(path, "asset1_numpixels.csv");
            string file8 = Path.Combine(path, "asset1_solarpanelpowerin.csv");



            string text1 = System.IO.File.ReadAllText(@file1);
            string expText1 = "time,asset1_databufferfillratio\r\n0,0\r\n";
            System.IO.File.Delete(file1);

            string text2 = System.IO.File.ReadAllText(@file2);
            string expText2 = "time,asset1_datarate_mb_s_\r\n0,0\r\n";
            System.IO.File.Delete(file2);

            string text3 = System.IO.File.ReadAllText(@file3);
            string expText3 = "time,asset1_depthofdischarge\r\n0,0\r\n";
            System.IO.File.Delete(file3);

            string text4 = System.IO.File.ReadAllText(@file4);
            string expText4 = "time,asset1_eci_pointing_vector_xyz_\r\n0,[0; 0; 0]\r\n";
            System.IO.File.Delete(file4);

            string text5 = System.IO.File.ReadAllText(@file5);
            string expText5 = "time,asset1_eosensoron\r\n0,0\r\n";
            System.IO.File.Delete(file5);

            string text6 = System.IO.File.ReadAllText(@file6);
            string expText6 = "time,asset1_incidenceangle\r\n0,0\r\n";
            System.IO.File.Delete(file6);

            string text7 = System.IO.File.ReadAllText(@file7);
            string expText7 = "time,asset1_numpixels\r\n0,0\r\n";
            System.IO.File.Delete(file7);

            string text8 = System.IO.File.ReadAllText(@file8);
            string expText8 = "time,asset1_solarpanelpowerin\r\n0,0\r\n";
            System.IO.File.Delete(file8);

            Assert.AreEqual(expText1, text1);
            Assert.AreEqual(expText2, text2);
            Assert.AreEqual(expText3, text3);
            Assert.AreEqual(expText4, text4);
            Assert.AreEqual(expText5, text5);
            Assert.AreEqual(expText6, text6);
            Assert.AreEqual(expText7, text7);
            Assert.AreEqual(expText8, text8);

        }
        [SetUp]
        public void systemScheduleInitializer()
        {
            if (!sysSchedInit)
            {
                Program programAct = new Program();
                string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_EnviroRTA.xml");
                string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
                programAct.ModelInputFilePath = modelInputFilePath;
                programAct.SimulationInputFilePath = simulationInputFilePath;

                programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
                programAct.LoadTargets();

                //XmlNode simNode = XmlParser.ParseSimulationInput(simulationInputFilePath);
                //XmlNode modelNode = XmlParser.GetModelNode(modelInputFilePath);
                //asset = new Asset(modelNode.ChildNodes[1]);
                //SystemState systemState = new SystemState();

                ////List<XmlNode> ICNodes = new List<XmlNode>();
                ////ICNodes.Add(modelNode.ChildNodes[1].ChildNodes[2].FirstChild);

                //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[2].FirstChild, asset));
                //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].FirstChild, asset));
                //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[1], asset));
                //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[3].ChildNodes[2], asset));
                //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[4].FirstChild, asset));
                //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[5].FirstChild, asset));
                //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[6].FirstChild, asset));
                //systemState.Add(SystemState.SetInitialSystemState(modelNode.ChildNodes[1].ChildNodes[6].ChildNodes[1], asset));

                ////SystemState systemState = SystemState.SetInitialSystemState(ICNodes, asset);
                //initialHist = new StateHistory(systemState);

                sysSchedInit = true;
            }
            else
            {
            }
        }
    }
}
