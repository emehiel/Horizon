using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HSFUniverse;
using Utilities;

namespace UniverseUnitTest
{
    [TestClass]
    public class StandardAtmosphereUnitTest
    {
        [TestMethod]
        public void TemperatureTest()
        {
            StandardAtmosphere atmos = new StandardAtmosphere();
            atmos.CreateAtmosphere();
            Assert.AreEqual(288.1, atmos.temperature(0), 0.1);
            Assert.AreEqual(249.2, atmos.temperature(6000), 0.1);
            Assert.AreEqual(216.6, atmos.temperature(14000), 0.1);
            Assert.AreEqual(218.6, atmos.temperature(22000), 0.1);
            Assert.AreEqual(226.5, atmos.temperature(30000), 0.1);
            Assert.AreEqual(244.8, atmos.temperature(38000), 0.1);
            Assert.AreEqual(266.9, atmos.temperature(46000), 0.1);
            Assert.AreEqual(263.5, atmos.temperature(54000), 0.1);
            Assert.AreEqual(241.5, atmos.temperature(62000), 0.1);
            Assert.AreEqual(219.6, atmos.temperature(70000), 0.1);
            Assert.AreEqual(202.5, atmos.temperature(78000), 0.1);
            Assert.AreEqual(190.8, atmos.temperature(84000), 0.1);
        }
        [TestMethod]
        public void PressureTest()
        {
            StandardAtmosphere atmos = new StandardAtmosphere();
            atmos.CreateAtmosphere();
            Assert.AreEqual(1.013E+5, atmos.pressure(0), 100);
            Assert.AreEqual(4.722E+4, atmos.pressure(6000), 10);
            Assert.AreEqual(1.417E+4, atmos.pressure(14000), 10);
            Assert.AreEqual(4.047E+3, atmos.pressure(22000), 1);
            Assert.AreEqual(1.197E+3, atmos.pressure(30000), 1);
            Assert.AreEqual(3.771E+2, atmos.pressure(38000), 0.1);
            Assert.AreEqual(1.313E+2, atmos.pressure(46000), 0.1);
            Assert.AreEqual(4.833E+1, atmos.pressure(54000), 0.01);
            Assert.AreEqual(1.669E+1, atmos.pressure(62000), 0.01);
            Assert.AreEqual(5.220E+0, atmos.pressure(70000), 0.001);
            Assert.AreEqual(1.467E+0, atmos.pressure(78000), 0.001);
            Assert.AreEqual(5.308E-1, atmos.pressure(84000), 0.0001);
        }
        [TestMethod]
        public void DensityTest()
        {
            StandardAtmosphere atmos = new StandardAtmosphere();
            atmos.CreateAtmosphere();
            Assert.AreEqual(1.225E+0, atmos.density(0), 1E-3);
            Assert.AreEqual(6.601E-1, atmos.density(6000), 1E-4);
            Assert.AreEqual(2.279E-1, atmos.density(14000), 1E-4);
            Assert.AreEqual(6.451E-2, atmos.density(22000), 1E-5);
            Assert.AreEqual(1.841E-2, atmos.density(30000), 1E-5);
            Assert.AreEqual(5.366E-3, atmos.density(38000), 1E-6);
            Assert.AreEqual(1.714E-3, atmos.density(46000), 1E-6);
            Assert.AreEqual(6.389E-4, atmos.density(54000), 1E-7);
            Assert.AreEqual(2.407E-4, atmos.density(62000), 1E-7);
            Assert.AreEqual(8.281E-5, atmos.density(70000), 1E-8);
            Assert.AreEqual(2.523E-5, atmos.density(78000), 1E-8);
            Assert.AreEqual(9.690E-6, atmos.density(84000), 1E-9);
        }
    }
}
