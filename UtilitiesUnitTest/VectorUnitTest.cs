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
    }
}
