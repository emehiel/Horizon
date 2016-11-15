using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace UtilitiesUnitTest
{
    [TestClass]
    public class VectorUnitTest
    {
        [TestMethod]
        public void DotTest()
        {
            Vector a = new Vector(new double[] { 1, 2, 3 });
            Vector b = new Vector(new double[] { 1, 2, 3 });
            double c = Vector.Dot(a, b);
            double d = 14;

            Assert.AreEqual(d, c);
        }
        [TestMethod]
        public void CrossTest()
        {
            Vector a = new Vector(new double[] { 1, 2, 3 });
            Vector b = new Vector(new double[] { 3, 2, 1 });
            Vector c = Vector.Cross(a, b);
            Vector d = new Vector(new double[] { -4, 8, -4 });

            Assert.AreEqual(d, c);
        }
    }
}
