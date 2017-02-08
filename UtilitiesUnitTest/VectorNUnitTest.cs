
using System;
using NUnit.Framework;
using Utilities;
namespace UtilitiesUnitTest

{
    [TestFixture]
    public class VectorTest
    {
        [Test]
        public void DotTest()
        {
            Vector a = new Vector(new double[] { 7378, 0, 0 });
            Vector b = new Vector(new double[] { -12525.2938790736, 1346.43186132083, -3517.5443929026 });
            double c = Vector.Dot(a, b);
            double d = -92411618;

            Assert.AreEqual(d, c, 1);
        }
    }
}