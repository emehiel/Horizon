using System;
using NUnit.Framework;
using HSFUniverse;
using Utilities;

namespace UniverseUnitTest
{
    [TestFixture]
    public class CloudCoverUnitTest
    {
        // TODO: Figure out what this was meant for.
        //[Test]
        public void TestMethod1()
        {
            Sun mysun = new Sun();
            Matrix<double> esVec = new Matrix<double>();

            esVec = mysun.getEarSunVec(20.0);

            esVec.GetRow(4);

        }
    }
}
