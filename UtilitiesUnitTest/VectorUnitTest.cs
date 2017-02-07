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
            Vector a = new Vector(new double[] { 7378, 0, 0 });
            Vector b = new Vector(new double[] { -12525.2938790736, 1346.43186132083, -3517.5443929026 });
            double c = Vector.Dot(a, b);
            double d = -92411618;

            Assert.AreEqual(d, c, 1);
        }
        [TestMethod]
        public void CrossTest()
        {
            Vector a = new Vector(new double[] { 7378, 0, 0 });
            Vector b = new Vector(new double[] { -12525.2938790736, 1346.43186132083, -3517.5443929026 });
            Vector c = Vector.Cross(a, b);
            Vector d = new Vector(new double[] { 0, 25952442, 9933974 });

            Assert.AreEqual(d[1], c[1], 1);
            Assert.AreEqual(d[2], c[2], 1);
            Assert.AreEqual(d[3], c[3], 1);
        }
        [TestMethod]
        public void NormTest()
        {
            Vector a = new Vector(new double[] { -12525.2938790736, 1346.43186132083, -3517.5443929026 });
            double b = Vector.Norm(a);
            double c = 13079;
            Assert.AreEqual(c, b, 1);
        }
    }
}
