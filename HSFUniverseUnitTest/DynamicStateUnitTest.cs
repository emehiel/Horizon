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
using System.IO;

namespace UniverseUnitTest
{
    [TestFixture]
    public class DynamicStateUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        [Test]
        public void ConstructDynamicState() //TODO:whats going on here jack
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_Scheduler.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_TestSub.xml");

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


        }
    }
}
