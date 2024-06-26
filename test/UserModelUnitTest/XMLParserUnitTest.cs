﻿using System;
using NUnit.Framework;
using Horizon;
using System.Xml;
using HSFUniverse;
using MissionElements;
using System.Collections.Generic;
using HSFSystem;
using HSFScheduler;
using UserModel;
using HSFMainUnitTest;
using System.IO;

namespace UserModelUnitTest
{
    [TestFixture]
    public class XMLParserUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void ParseScriptedSrc()
        {

            //arrange
            string pythonFilePath = "";
            string className = "";
            string filePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_ScriptUnivFact.xml");
            XmlNode modelNode = XmlParser.GetModelNode(filePath);

            //act
            XmlParser.ParseScriptedSrc(modelNode.FirstChild, ref pythonFilePath, ref className);

            //assert
            Assert.AreEqual("PythonSubs\\environment.py", pythonFilePath);
            Assert.AreEqual("environment", className);
        }
        [Test]
        public void ParseSimulationInput1()
        {
            //arrange
            string input = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput.xml");

            //act
            XmlNode nothing = XmlParser.ParseSimulationInput(input);

            //assert
            Assert.AreEqual(30, SimParameters.SimEndSeconds);
            Assert.AreEqual(0, SimParameters.SimStartSeconds);
            Assert.AreEqual(2454680.0, SimParameters.SimStartJD);
            Assert.AreEqual(6, SchedParameters.MaxNumScheds);
            Assert.AreEqual(5, SchedParameters.NumSchedCropTo);
            Assert.AreEqual(30, SchedParameters.SimStepSeconds);

        }
        [Test]
        public void GetTargetNode()
        {
            //arrange
            string input = Path.Combine(baselocation, @"UnitTestInputs\UnitTestTargets.xml");

            //act
            XmlNode targetNodes = XmlParser.GetTargetNode(input);

            //assert
            Assert.AreEqual(17, targetNodes.ChildNodes.Count);


        }
        [Test]
        public void GetModelNode()
        {
            //arrange
            string input = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");

            //act
            XmlNode modelNodes = XmlParser.GetModelNode(input);

            //assert
            Assert.AreEqual(9, modelNodes.ChildNodes[1].ChildNodes.Count);
        }
    }
}
