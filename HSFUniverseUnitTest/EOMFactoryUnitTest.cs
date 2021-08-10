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
    public class EOMFactoryUnitTest
    {
        [Test]
        public void GetEomClass()
        {

            DynamicEOMS dynamicEOMS = EOMFactory.GetEomClass(modelInput.ChildNodes[1].FirstChild);

            Assert.IsInstanceOf(typeof(OrbitalEOMS), dynamicEOMS);
        }
        [Test]
        public void GetScriptedEomClass()
        {
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_ScriptEOM.xml");
            XmlNode modelInput = XmlParser.GetModelNode(modelInputFilePath);

            DynamicEOMS dynamicEOMS = EOMFactory.GetEomClass(modelInput.ChildNodes[1].FirstChild);

            Assert.IsInstanceOf(typeof(ScriptedEOMS), dynamicEOMS);
        }
    }
}
