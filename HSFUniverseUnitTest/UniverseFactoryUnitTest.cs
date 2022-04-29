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
        /// Tests both UniverseFactory GetUniverseClass for Scripted universe and ScriptedUniverse constructor
        /// </summary>
        [Test]
        public void UniverseFactoryScriptedTest()
        {
            //arrange
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_ScriptUnivFact.xml");
            XmlNode modelInput = XmlParser.GetModelNode(modelInputFilePath);
            //act
            Domain universeAct = UniverseFactory.GetUniverseClass(modelInput.FirstChild);
            //assert
            Assert.IsInstanceOf(typeof(ScriptedUniverse), universeAct);

        }
        /// <summary>
        /// Tests Space Environment construction by checking that objectType == SpaceEnvironment
        /// </summary>
        [Test]
        public void UniverseFactorySpaceEnvironmentTest()
        {
            //arrange
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_SpaceEnvUnivFact.xml");
            XmlNode modelInput = XmlParser.GetModelNode(modelInputFilePath);
            //act
            Domain universe = UniverseFactory.GetUniverseClass(modelInput);
            //assert
            Assert.IsInstanceOf(typeof(SpaceEnvironment), universe);
        }
    }
}
