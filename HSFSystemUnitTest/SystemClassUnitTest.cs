using HSFSystem;
using MissionElements;
using NUnit.Framework;
using System;
using System.IO;
using System.Xml;
using UserModel;
using System.Collections.Generic;
using HSFSubsystem;
using HSFUniverse;

namespace HSFSystemUnitTest
{
    [TestFixture]
    public class SystemClassUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void CheckForCircularDependencies()//buuuuuug
        {
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_Circular.xml");
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            XmlNode simNode = XmlParser.ParseSimulationInput(simulationInputFilePath);
            XmlNode modelInput = XmlParser.GetModelNode(modelInputFilePath);

            Asset asset = new Asset(modelInput["ASSET"]);
            List<Asset> assets = new List<Asset>();
            assets.Add(asset);

            Subsystem access = new AccessSub(modelInput.ChildNodes[1].ChildNodes[1],asset);
            Subsystem adcs = new AccessSub(modelInput.ChildNodes[1].ChildNodes[2], asset);
            Subsystem eosensor = new AccessSub(modelInput.ChildNodes[1].ChildNodes[3], asset);

            List<Subsystem> subList = new List<Subsystem>();
            subList.Add(access);
            subList.Add(adcs);
            subList.Add(eosensor);

            List<Constraint> constraints = new List<Constraint>();

            Domain enviro = new SpaceEnvironment();

            SystemClass sysclass = new SystemClass(assets, subList, constraints, enviro);
            bool isTrue = sysclass.CheckForCircularDependencies();
            Assert.IsTrue(isTrue);
        }
        [Test]
        public void CheckForCircularDependencies2()//buuuuuug
        {
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_CircularV2.xml");
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            XmlNode simNode = XmlParser.ParseSimulationInput(simulationInputFilePath);
            XmlNode modelInput = XmlParser.GetModelNode(modelInputFilePath);

            Asset asset = new Asset(modelInput["ASSET"]);
            List<Asset> assets = new List<Asset>();
            assets.Add(asset);

            Subsystem access = new AccessSub(modelInput.ChildNodes[1].ChildNodes[1], asset);
            Subsystem adcs = new AccessSub(modelInput.ChildNodes[1].ChildNodes[2], asset);
            Subsystem eosensor = new AccessSub(modelInput.ChildNodes[1].ChildNodes[3], asset);

            List<Subsystem> subList = new List<Subsystem>();
            subList.Add(access);
            subList.Add(adcs);
            subList.Add(eosensor);

            List<Constraint> constraints = new List<Constraint>();

            Domain enviro = new SpaceEnvironment();

            SystemClass sysclass = new SystemClass(assets, subList, constraints, enviro);
            bool isTrue = sysclass.CheckForCircularDependencies();
            Assert.IsTrue(isTrue);
        }
        /// <summary>
        /// Tests referenced constructor for SystemClass by 
        /// </summary>
        [Test]
        public void SystemClassCtor()
        {
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_CircularV2.xml");
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            XmlNode simNode = XmlParser.ParseSimulationInput(simulationInputFilePath);
            XmlNode modelInput = XmlParser.GetModelNode(modelInputFilePath);

            Asset asset = new Asset(modelInput["ASSET"]);
            List<Asset> assets = new List<Asset>();
            assets.Add(asset);

            Subsystem access = new AccessSub(modelInput.ChildNodes[1].ChildNodes[1], asset);
            Subsystem adcs = new AccessSub(modelInput.ChildNodes[1].ChildNodes[2], asset);
            Subsystem eosensor = new AccessSub(modelInput.ChildNodes[1].ChildNodes[3], asset);

            List<Subsystem> subList = new List<Subsystem>();
            subList.Add(access);
            subList.Add(adcs);
            subList.Add(eosensor);

            List<Constraint> constraints = new List<Constraint>();

            Domain enviro = new SpaceEnvironment();

            SystemClass sysclass = new SystemClass(assets, subList, constraints, enviro);
            Assert.IsInstanceOf(typeof(SystemClass), sysclass);
            Assert.AreEqual(enviro, sysclass.Environment);
            Assert.AreEqual(subList, sysclass.Subsystems);
            Assert.AreEqual(constraints, sysclass.Constraints);
            Assert.AreEqual(assets, sysclass.Assets);
        }
    }
}
