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
        Asset asset;
        List<Asset> assets;
        Dependency dep;
        XmlNode modelInput;
        /// <summary>
        /// Tests two subs which depend on eachother for information.
        /// Expect CheckForCircularDependencies to return true: there is a circular dep
        /// </summary>
        [Test]
        public void CheckForCircularDependencies()
        {
            //arrange
            Subsystem sub4 = new SubTest(modelInput.ChildNodes[0].ChildNodes[4], dep, asset);
            Subsystem sub5 = new SubTest(modelInput.ChildNodes[0].ChildNodes[5], dep, asset);

            List<Subsystem> subList = new List<Subsystem>();
            subList.Add(sub4);
            subList.Add(sub5);

            sub4.DependentSubsystems.Add(sub5);
            sub5.DependentSubsystems.Add(sub4);

            List<Constraint> constraints = new List<Constraint>();
            Domain enviro = new SpaceEnvironment();

            SystemClass sysclass = new SystemClass(assets, subList, constraints, enviro);
            //act
            bool isTrue = sysclass.CheckForCircularDependencies();

            //assert
            Assert.IsTrue(isTrue);
        }
        /// <summary>
        /// Tests three subs which depend on eachother for information. sub1 depends on sub2, sub2 on sub3, and sub3 on sub1
        /// Expect CheckForCircularDependencies to return true: there is a circular dep
        /// </summary>
        [Test]
        public void CheckForCircularDependencies2()
        {
            //arrange
            Subsystem sub1 = new SubTest(modelInput.ChildNodes[0].ChildNodes[1], dep, asset);
            Subsystem sub2 = new SubTest(modelInput.ChildNodes[0].ChildNodes[2], dep, asset);
            Subsystem sub3 = new SubTest(modelInput.ChildNodes[0].ChildNodes[3], dep, asset);

            List<Subsystem> subList = new List<Subsystem>();
            subList.Add(sub1);
            subList.Add(sub2);
            subList.Add(sub3);


            sub1.DependentSubsystems.Add(sub2);
            sub2.DependentSubsystems.Add(sub3);
            sub3.DependentSubsystems.Add(sub1);

            List<Constraint> constraints = new List<Constraint>();

            Domain enviro = new SpaceEnvironment();

            SystemClass sysclass = new SystemClass(assets, subList, constraints, enviro);
            bool isTrue = sysclass.CheckForCircularDependencies();
            Assert.IsTrue(isTrue);
        }
        /// <summary>
        /// Tests 3 subs not circularly dependent on eachother
        /// Expect CheckforCircularDependencies to return false: no circular dep
        /// </summary>
        [Test]
        public void CheckForCircularDependencies_notcircular()
        {
            //arrange
            Subsystem sub1 = new SubTest(modelInput.ChildNodes[0].ChildNodes[1], dep, asset);
            Subsystem sub2 = new SubTest(modelInput.ChildNodes[0].ChildNodes[2], dep, asset);
            Subsystem sub3 = new SubTest(modelInput.ChildNodes[0].ChildNodes[5], dep, asset);

            List<Subsystem> subList = new List<Subsystem>();
            subList.Add(sub1);
            subList.Add(sub2);
            subList.Add(sub3);


            sub1.DependentSubsystems.Add(sub2);
            sub2.DependentSubsystems.Add(sub3);
            

            List<Constraint> constraints = new List<Constraint>();

            Domain enviro = new SpaceEnvironment();

            SystemClass sysclass = new SystemClass(assets, subList, constraints, enviro);
            //act
            bool isFalse = sysclass.CheckForCircularDependencies();

            //assert
            Assert.IsFalse(isFalse);
        }
        /// <summary>
        /// Tests referenced constructor for SystemClass by 
        /// </summary>
        [Test]
        public void SystemClassCtor()
        {
            //arrange
            Subsystem access = new SubTest(modelInput.ChildNodes[0].ChildNodes[1], asset);
            Subsystem adcs = new SubTest(modelInput.ChildNodes[0].ChildNodes[2], asset);
            Subsystem eosensor = new SubTest(modelInput.ChildNodes[0].ChildNodes[3], asset);

            List<Subsystem> subList = new List<Subsystem>();
            subList.Add(access);
            subList.Add(adcs);
            subList.Add(eosensor);

            List<Constraint> constraints = new List<Constraint>();

            Domain enviro = new SpaceEnvironment();
            //act
            SystemClass sysclass = new SystemClass(assets, subList, constraints, enviro);

            //assert
            Assert.IsInstanceOf(typeof(SystemClass), sysclass);
            Assert.AreEqual(enviro, sysclass.Environment);
            Assert.AreEqual(subList, sysclass.Subsystems);
            Assert.AreEqual(constraints, sysclass.Constraints);
            Assert.AreEqual(assets, sysclass.Assets);
        }
        [SetUp]
        public void systemClassHelper()
        {
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_Circular.xml");
            string simulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scheduler_crop.xml");
            XmlNode simNode = XmlParser.ParseSimulationInput(simulationInputFilePath);
            modelInput = XmlParser.GetModelNode(modelInputFilePath);

            asset = new Asset(modelInput["ASSET"]);
            assets = new List<Asset>();
            assets.Add(asset);
            dep = Dependency.Instance;
        }
    }
}
