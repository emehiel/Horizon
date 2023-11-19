using NUnit.Framework;
using HSFUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace HSFUniverse.Tests
{
    [TestFixture()]
    public class WMMTests
    {
        [Test()]
        [TestCaseSource(typeof(WMMData), "TestCases")]
        public void GeoMagTest(DateTime date, double alt, double lat, double lon, double bx, double by, double bz, double inc, double dec)
        {
            WMM gm = new WMM();
            gm.GeoMag(lat, lon, alt, date);
            Assert.Multiple(() =>
            {
                Assert.That(() => gm.bx, Is.EqualTo(bx).Within(1));
                Assert.That(() => gm.by, Is.EqualTo(by).Within(1));
                Assert.That(() => gm.bz, Is.EqualTo(bz).Within(1));
                Assert.That(() => gm.dec, Is.EqualTo(dec).Within(0.1));
                Assert.That(() => gm.dip, Is.EqualTo(inc).Within(0.1));
            });
        }
    }
    public class WMMData
    {
        public static IEnumerable TestCases
        {
            get
            { 
                yield return new TestCaseData(new DateTime(2015, 1, 1), 0, 80, 0, 6627.1, -445.9, 54432.2, 83.04, -3.85);
                yield return new TestCaseData(new DateTime(2015, 1, 1), 0, 0, 120, 39518.2, 392.9, -11252.4, -15.89, 0.57);
                yield return new TestCaseData(new DateTime(2015, 1, 1), 0, -80, 240, 5797.3, 15761.1, -52919.1, -72.39, 69.81);
                yield return new TestCaseData(new DateTime(2015, 1, 1), 100, 80, 0, 6314.3, -471.6, 52269.8, 83.09, -4.27);
                yield return new TestCaseData(new DateTime(2015, 1, 1), 100, 0, 120, 37535.6, 364.4, -10773.4, -16.01, 0.56);
                yield return new TestCaseData(new DateTime(2015, 1, 1), 100, -80, 240, 5613.1, 14791.5, -50378.6, -72.57, 69.22);
                yield return new TestCaseData(new DateTime(2017, 7, 2), 0, 80, 0, 6599.4, -317.1, 54459.2, 83.08, -2.75);
                yield return new TestCaseData(new DateTime(2017, 7, 2), 0, 0, 120, 39571.4, 222.5, -11030.1, -15.57, 0.32);
                yield return new TestCaseData(new DateTime(2017, 7, 2), 0, -80, 240, 5873.8, 15781.4, -52687.9, -72.28, 69.58);
                yield return new TestCaseData(new DateTime(2017, 7, 2), 100, 80, 0, 6290.5, -348.5, 52292.7, 83.13, -3.17);
                yield return new TestCaseData(new DateTime(2017, 7, 2), 100, 0, 120, 37585.5, 209.5, -10564.2, -15.70, 0.32);
                yield return new TestCaseData(new DateTime(2017, 7, 2), 100, -80, 240, 5683.5, 14808.8, -50163.0, -72.45, 69.00);
            }
        }
    }
}