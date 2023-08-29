using NUnit.Framework;
using Utilities;
using System;
using System.Collections.Generic;

namespace UtilitiesUnitTest
{
    [TestFixture]
    public class HSFProfileIdeaUnitTest
    {
        [Test]
        public void TestConstruction()
        {
            var hsfProfile = new HSFProfileIdea<int>();
            var hsfP2 = new HSFProfileIdea<Matrix<double>>();
            var hsfP3 = new HSFProfile<double>();

            hsfProfile.Add(0, 10);
            hsfProfile[2] = 20;
            hsfProfile[1] = 30;

            hsfProfile[2] = 10;
            hsfProfile.Add(2, 10);

            var dataPoint = hsfProfile[0.5];

            hsfP2[0] = new Matrix<double>(3, 1);
        }

        [Test]
        public void TestIntegration()
        {
            List<double> t = new List<double>(new double[] { 0, 1, 4, 5, 8 });
            List<double> v = new List<double>(new double[] { 10, 20, 10, 15, 30 });
            var p1 = new HSFProfileIdea<double>(t, v);

            Assert.AreEqual(185, p1.Integrate(0, 10, 0));
            Assert.AreEqual(110, p1.Integrate(0, 7, 0));
            Assert.AreEqual(80, p1.Integrate(2, 7, 0));
            Assert.AreEqual(100, p1.Integrate(2, 7, 20));
        }
    }
}
