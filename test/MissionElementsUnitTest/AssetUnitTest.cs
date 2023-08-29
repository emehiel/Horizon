using HSFUniverse;
using MissionElements;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UserModel;
using Utilities;

namespace MissionElementsUnitTest
{
    [TestFixture]
    public class AssetUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        Asset asset;
        Asset asset3;
        DynamicState dynamicState;
        [Test]
        public void ConstructorUnitTest()
        {
            //arrange + act for 'asset' in asset helper
            Asset asset2 = new Asset();

            //assert
            Assert.AreEqual("asset1", asset.Name);
            Assert.AreEqual(dynamicState, asset.AssetDynamicState);

            Assert.AreEqual(null, asset2.AssetDynamicState);
            Assert.AreEqual(null, asset2.Name);
        }
        [Test]
        public void Equals()
        {
            //arrange
            string ModelFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_checker.xml");
            XmlNode ModelFile = XmlParser.GetModelNode(ModelFilePath);
            asset3 = new Asset(ModelFile.FirstChild);
            //act
            bool asset1equalsasset3 = asset3.Equals(asset);
            //assert
            Assert.IsTrue(asset1equalsasset3);

        }
        [SetUp]
        public void AssetHelper()
        {
            string SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            var simulationInputNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);

            OrbitalEOMS expectedorb = new OrbitalEOMS();

            List<double> ICList = new List<double>();
            ICList.Add(3000.0);
            ICList.Add(4100.0);
            ICList.Add(3400.0);
            ICList.Add(0.0);
            ICList.Add(6.02088);
            ICList.Add(4.215866);
            Vector expIC = new Vector(ICList);

            DynamicStateType DST = new DynamicStateType();
            dynamicState = new DynamicState(DST, expectedorb, expIC);

            asset = new Asset(dynamicState, "asset1");

        }

        
    }
}
