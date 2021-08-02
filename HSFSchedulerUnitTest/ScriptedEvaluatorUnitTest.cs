﻿using Horizon;
using HSFScheduler;
using HSFSystem;
using HSFUniverse;
using MissionElements;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UserModel;

namespace HSFSchedulerUnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestFixture]
    public class ScriptedEvaluatorUnitTest
    {
        public Evaluator eval;

        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void ScriptedEvaluatorCtor()
        {
            string SimulationInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestSimulationInput_Scripted.xml");
            Dependency dep = Dependency.Instance;
            XmlNode simNode = XmlParser.ParseSimulationInput(SimulationInputFilePath);

            ScriptedEvaluator s = new ScriptedEvaluator(simNode, dep);

        }

    }
}
