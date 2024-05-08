using System;
using NUnit.Framework;
using System.Xml;
using UserModel;
using System.Collections.Generic;
using HSFSystem;
using MissionElements;
using Horizon;
using System.IO;

namespace HSFSystemUnitTest
{
    
    [TestFixture]
    public class SubsystemFactoryUnitTest
    {
        public Dependency dependencies = Dependency.Instance;
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        
        
        /// <summary>
        /// Test that the GetSubsystem takes empty dependencies, an asset, and Sub XML node
        /// and populates sub dic (aka subsystemMap in program.cs) with the subsystems
        /// TODO: Test the exception catch for unknown type
        /// </summary>
        [Test]
        public void GetSubsystem()
        {
            //arrange
            Program programAct = new Program();
            programAct.SimulationFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler.xml");
            programAct.TaskDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.ModelFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");

            try
            {
                Stack<Task> systemTasks = programAct.LoadTasks();
                programAct.LoadSubsystems();
            }
            catch
            {
                programAct.log.Info("LoadSubsystems Failed the Unit test");
            }


            var modelInputXMLNode = XmlParser.GetModelNode(programAct.ModelFilePath);
            Dictionary<string, Subsystem> subDic = new Dictionary<string, Subsystem>();
           
            //act
            foreach (XmlNode child in modelInputXMLNode.ChildNodes[1])
            {
                if (child.Name.Equals("SUBSYSTEM"))
                {
                    string names = SubsystemFactory.GetSubsystem(child, dependencies, programAct.AssetList[0], subDic);
                }
            }
            AccessSub expAccessSub = new AccessSub(modelInputXMLNode.ChildNodes[1].ChildNodes[1], programAct.AssetList[0]);
            ADCS expAdcsSub = new ADCS(modelInputXMLNode.ChildNodes[1].ChildNodes[2], dependencies, programAct.AssetList[0]);
            EOSensor  expEoSub = new EOSensor(modelInputXMLNode.ChildNodes[1].ChildNodes[3], dependencies, programAct.AssetList[0]);
            SSDR expSsdrSub = new SSDR(modelInputXMLNode.ChildNodes[1].ChildNodes[4], dependencies, programAct.AssetList[0]);
            Comm expCommSub = new Comm(modelInputXMLNode.ChildNodes[1].ChildNodes[5], dependencies, programAct.AssetList[0]);
            Power expPowerSub = new Power(modelInputXMLNode.ChildNodes[1].ChildNodes[6], dependencies, programAct.AssetList[0]);



            //Assert
            Assert.AreEqual(expAccessSub.Name, subDic["asset1.access"].Name);
            Assert.IsInstanceOf(typeof(AccessSub), subDic["asset1.access"]);

            Assert.AreEqual(expAdcsSub.Name, subDic["asset1.adcs"].Name);
            Assert.IsInstanceOf(typeof(ADCS), subDic["asset1.adcs"]);

            Assert.AreEqual(expEoSub.Name, subDic["asset1.eosensor"].Name);
            Assert.IsInstanceOf(typeof(EOSensor), subDic["asset1.eosensor"]);

            Assert.AreEqual(expSsdrSub.Name, subDic["asset1.ssdr"].Name);
            Assert.IsInstanceOf(typeof(SSDR), subDic["asset1.ssdr"]);

            Assert.AreEqual(expCommSub.Name, subDic["asset1.comm"].Name);
            Assert.IsInstanceOf(typeof(Comm), subDic["asset1.comm"]);

            Assert.AreEqual(expPowerSub.Name, subDic["asset1.power"].Name);
            Assert.IsInstanceOf(typeof(Power), subDic["asset1.power"]);
        }
    }
}
