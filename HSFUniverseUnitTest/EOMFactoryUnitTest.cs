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
    public class EOMFactoryUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void GetEomClass()
        {
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel.xml");
            XmlNode modelInput = XmlParser.GetModelNode(modelInputFilePath);

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
