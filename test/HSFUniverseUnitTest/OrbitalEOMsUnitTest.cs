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
    public class OrbitalEOMsUnitTest
    {
        string baselocation = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

        [Test]
        public void OrbitalConstructorUnitTest()
        {
            //arrange
            Matrix<double> _A = new Matrix<double>(6);
            _A[1, 4] = 1.0;
            _A[2, 5] = 1.0;
            _A[3, 6] = 1.0;

            //act
            OrbitalEOMS orb = new OrbitalEOMS();

            //assert
            Assert.AreEqual(_A, orb.getA_UnitTestOnly());
        }
        [Test]

        public void MatrixUnitTest()
        {
            Assert.Inconclusive();
        }
    }
}
