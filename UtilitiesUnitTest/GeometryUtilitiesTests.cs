using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Tests
{
    [TestClass()]
    public class GeometryUtilitiesTests
    {
        [TestMethod()]
        public void hasLOSTest()
        {
            Vector sat = new Vector(new List<double>(new double[] { 7378, 0, 0 }));
            Vector ground = new Vector(new List<double>(new double[] { -33.47, -70.65, 0 }));
            ground = GeometryUtilities.LLA2ECI(ground, 2457709);

            bool vector = GeometryUtilities.hasLOS(sat, ground);
            bool matrix = GeometryUtilities.hasLOS((Matrix<double>)sat, (Matrix<double>)ground);

            Assert.IsFalse(vector);
            Assert.IsFalse(matrix);
        }
    }
}