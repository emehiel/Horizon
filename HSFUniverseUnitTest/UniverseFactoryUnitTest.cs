using HSFUniverse;
using NUnit.Framework;
using System;
using System.IO;
using System.Xml;
using UserModel;

namespace UniverseUnitTest
{
    [TestFixture]
    public class UniverseFactoryUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void UniverseFactoryScriptedTest()
        {
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_ScriptUnivFact.xml");
            XmlNode modelInput = XmlParser.GetModelNode(modelInputFilePath);
            Domain universeAct = UniverseFactory.GetUniverseClass(modelInput);

            Assert.IsInstanceOf(typeof(ScriptedUniverse), universeAct);

        }
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void UniverseFactorySpaceEnvironmentTest()
        {
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_SpaceEnvUnivFact.xml");
            XmlNode modelInput = XmlParser.GetModelNode(modelInputFilePath);
            Domain universe = UniverseFactory.GetUniverseClass(modelInput);
            Assert.IsInstanceOf(typeof(SpaceEnvironment), universe);
        }
    }
}
