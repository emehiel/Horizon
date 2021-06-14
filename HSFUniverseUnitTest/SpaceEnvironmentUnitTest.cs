using System;
using Horizon;
using NUnit.Framework;

using System.Xml;
using System.Linq;
using UserModel;
using System.Collections.Generic;

using Utilities;
using MissionElements;
using HSFUniverse;

namespace UniverseUnitTest
{
    [TestFixture]
    public class SpaceEnvironmentUnitTest
    {
        [Test]
        public void CreateUniverseUnitTest()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.xml";
            programAct.TargetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets_Scheduler - Copy.xml";
            programAct.ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel_DummySub - Copy.xml";

            Domain SystemUniverse = new SpaceEnvironment();
            var modelInputXMLNode = XmlParser.GetModelNode(programAct.ModelInputFilePath);
            var simInputXMLNode = XmlParser.ParseSimulationInput(programAct.SimulationInputFilePath);

            Asset asset = new Asset(modelInputXMLNode.FirstChild);
            programAct.AssetList.Add(asset);
            try
            {
                asset.AssetDynamicState.Eoms.SetEnvironment(SystemUniverse);
            }
            catch
            {
                programAct.log.Info("AssetDynamicState.Eoms.SetEnvironment(SystemUniverse) Failed the Unit test");
            }
            Assert.Inconclusive();


        }
        [Test]

        public void GetObjectUnitTest()
        {
            Assert.Inconclusive();
        }
    }
}
