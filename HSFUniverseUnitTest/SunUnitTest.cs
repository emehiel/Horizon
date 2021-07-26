using System;
using Horizon;
using NUnit.Framework;

using System.Xml;
using System.Linq;
using UserModel;
using System.Collections.Generic;
using static System.Math;
using Utilities;
using MissionElements;
using HSFUniverse;
using System.IO;

namespace UniverseUnitTest
{
    [TestFixture]
    public class SunUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void GetEarSunVecUnitTest()
        {
            Program programAct = new Program();
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_SysScheduler.xml");
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");

            Domain SystemUniverse = new SpaceEnvironment();
            var modelInputXMLNode = XmlParser.GetModelNode(programAct.ModelInputFilePath);
            var simInputXMLNode = XmlParser.ParseSimulationInput(programAct.SimulationInputFilePath);

            Sun s = new Sun();
            Matrix<double> actVect = s.getEarSunVec(0);

            Matrix<double> expVect = new Matrix<double>(3, 1, 0.0);
            expVect.SetValue(1, 1, -96543097.3);
            expVect.SetValue(2, 1, 107516829.75);
            expVect.SetValue(3, 1, 46612155.63);

            Assert.AreEqual(-96540000, actVect[1,1],250000);
            Assert.AreEqual(107516829.75, actVect[2, 1], 250000);
            Assert.AreEqual(46612155.63, actVect[3, 1], 250000);
            

        }
        /// <summary>
        /// Tests a few positions with known shadow state, courtasy of Vallado's actual published matlab code
        /// </summary>
        [Test]
        public void CastShadowOnPos()
        {
            Program programAct = new Program();
            programAct.TargetDeckFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets_SysScheduler.xml");
            programAct.SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");

            Domain SystemUniverse = new SpaceEnvironment();
            var modelInputXMLNode = XmlParser.GetModelNode(programAct.ModelInputFilePath);
            var simInputXMLNode = XmlParser.ParseSimulationInput(programAct.SimulationInputFilePath);

            Sun s = new Sun();

            DynamicState pos = new DynamicState(modelInputXMLNode.ChildNodes[1].FirstChild);
            ShadowState shadowAct = s.castShadowOnPos(pos, 0);
            ShadowState shadowExp = ShadowState.NO_SHADOW;

            Assert.AreEqual(shadowExp, shadowAct);
            // make another which is in shadow


            programAct.ModelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_Integrator.xml");
            var modelInputXMLNode2 = XmlParser.GetModelNode(programAct.ModelInputFilePath);
            DynamicState pos2 = new DynamicState(modelInputXMLNode2.FirstChild.FirstChild);
            ShadowState shadowAct2 = s.castShadowOnPos(pos2, 0);
            ShadowState shadowExp2 = ShadowState.UMBRA;
            Assert.AreEqual(shadowExp2, shadowAct2);
        }
    }
}
