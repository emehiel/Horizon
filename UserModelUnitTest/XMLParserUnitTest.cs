using System;
using NUnit.Framework;
using Horizon;
using System.Xml;
using HSFUniverse;
using HSFSubsystem;
using MissionElements;
using System.Collections.Generic;
using HSFSystem;
using HSFScheduler;
using UserModel;
using HSFMainUnitTest;
using System.IO;
using log4net;

namespace UserModelUnitTest
{
    [TestFixture]
    public class XMLParserUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void ParseScriptedSrc()
        {
            Assert.Inconclusive();
        }
        [Test]
        public void ParseSimulationInput1()
        {
            string input = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");
            XmlNode nothing = XmlParser.ParseSimulationInput(input);

            Assert.AreEqual(30, SimParameters.SimEndSeconds);
            Assert.AreEqual(0, SimParameters.SimStartSeconds);
            Assert.AreEqual(2454680.0, SimParameters.SimStartJD);
            Assert.AreEqual(10, SchedParameters.MaxNumScheds);
            Assert.AreEqual(10, SchedParameters.NumSchedCropTo);
            Assert.AreEqual(30, SchedParameters.SimStepSeconds);          

        }
        [Test]
        public void GetTargetNode()
        {
            string input = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");
            XmlNode nothing = XmlParser.GetTargetNode(input);



        }
        [Test]
        public void GetModelNode()
        {
            Assert.Inconclusive();
        }
    }
}
