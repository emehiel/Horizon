﻿using System;
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
    public class SunUnitTest
    {
        [Test]
        public void GetEarSunVecUnitTest()
        {
            Program programAct = new Program();
            programAct.SimulationInputFilePath = @"..\..\..\UnitTestInputs\UnitTestSimulationInput.xml";
            programAct.TargetDeckFilePath = @"..\..\..\UnitTestInputs\UnitTestTargets_Scheduler.xml";
            programAct.ModelInputFilePath = @"..\..\..\UnitTestInputs\UnitTestModel_TestSub.xml";

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

        public void CastShadowOnPos()
        {
            Assert.Inconclusive();
        }
    }
}
