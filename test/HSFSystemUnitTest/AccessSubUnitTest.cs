using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Xml;
using MissionElements;
using HSFScheduler;
using UserModel;
using HSFSystem;
using Horizon;
using System.IO;

namespace HSFSystemUnitTest
{
    [TestFixture]
    public class AccessUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));


        [Test]
        public void AccessSubConstructor()
        {
            //arrange
            Program programAct = new Program();

            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_access.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");
            var modelInputXMLNode = XmlParser.GetModelNode(programAct.ModelInputFilePath);

            Stack<Task> systemTasks = programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
            }

            //act
            AccessSub A1 = new AccessSub(modelInputXMLNode.ChildNodes[1].ChildNodes[1],programAct.AssetList[0]);

            //assert
            Assert.AreSame(programAct.AssetList[0], A1.Asset);
            Assert.AreEqual("asset1.access", A1.Name);

        }
        [Test]
        public void AccessCanPerform()
        {
            //arrange
            Program programAct = new Program();

            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_access.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_access.xml");
            var modelInputXMLNode = XmlParser.GetModelNode(programAct.ModelInputFilePath);

            Stack<Task> systemTasks = programAct.LoadTargets();
            try
            {
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
                Assert.Fail("LoadSubsystems Failed the Unit test");
            }
            Dictionary<Asset, Task> eventDic = new Dictionary<Asset, Task>();
            eventDic.Add(programAct.AssetList[0], systemTasks.Pop());
            Event thing1 = new Event(eventDic, programAct.InitialSysState);


            //act + assert
            Assert.IsTrue(programAct.SubList[0].CanPerform(thing1,programAct.SystemUniverse));

            eventDic.Remove(programAct.AssetList[0]);
            eventDic.Add(programAct.AssetList[0], systemTasks.Pop());

            Event thing2 = new Event(eventDic, programAct.InitialSysState);// should this have access?
            Assert.IsFalse(programAct.SubList[0].CanPerform(thing2, programAct.SystemUniverse)); // should this have access?


        }
    }
}
