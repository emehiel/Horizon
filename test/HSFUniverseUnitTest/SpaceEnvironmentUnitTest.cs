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
    public class SpaceEnvironmentUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        /// <summary>
        /// Creates a universe, checks sun vec is const and standard atmosphere 
        /// </summary>

        [Test]
        public void CreateUniverse_ConstSunSA() // no xml input option for changing atmosphere
        {
            //arrange
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_EnviroSun.xml");
            XmlNode modelInput = XmlParser.GetModelNode(modelInputFilePath);
            
            //act
            //Const Sun Vec
            Domain SystemUniverse = new SpaceEnvironment(modelInput.FirstChild);
            var sunConst = SystemUniverse.GetObject<Sun>("sun");


            //assert
            Assert.AreEqual(true, sunConst._isSunVecConstant);
            //Standard Atmosphere type check (SA has only one constructor, empty input)
            Assert.IsInstanceOf(typeof(StandardAtmosphere), SystemUniverse.GetObject<Atmosphere>("atmos"));

        }
        [Test]
        public void CreateUniverseUnitTest_NonConstSunRTA() // no xml input option for changing atmosphere
        {
            //arrange
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_EnviroRTA.xml");
            XmlNode modelInput = XmlParser.GetModelNode(modelInputFilePath);

            //act
            //Const Sun Vec
            Domain SystemUniverse = new SpaceEnvironment(modelInput.FirstChild);
            var sunNonConst = SystemUniverse.GetObject<Sun>("sun");

            //assert
            Assert.AreEqual(false, sunNonConst._isSunVecConstant);
            //Standard Atmosphere type check (SA has only one constructor, empty input)
            Assert.IsInstanceOf(typeof(RealTimeAtmosphere), SystemUniverse.GetObject<Atmosphere>("atmos"));

        }
        [Test]

        public void GetObjectUnitTest()
        {
            //arrange
            string modelInputFilePath = Path.Combine(baselocation, @"UnitTestInputs\UnitTestModel_EnviroRTA.xml");
            XmlNode modelInput = XmlParser.GetModelNode(modelInputFilePath);
            //act
            //Const Sun Vec
            Domain SystemUniverse = new SpaceEnvironment(modelInput.FirstChild);
            var sun = SystemUniverse.GetObject<Sun>("sun");
            var atmos = SystemUniverse.GetObject<Atmosphere>("atmos");

            //assert
            Assert.IsInstanceOf(typeof(Sun), sun);
            Assert.IsInstanceOf(typeof(Atmosphere), atmos);

        }
    }
}
