using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HSFUniverse;

namespace UniverseUnitTest
{
    [TestClass]
    public class RealTimeWeatherTest
    {
        /// <summary>
        /// Tests that DownloadData runs without exceptiong. Doesn't check for results.
        /// </summary>
        /// <remarks>
        /// This test will normally return inconclusive. The if statement must be changed
        /// to true for it to actually download data. This because the file is ~70 MB and 
        /// we want to save bandwidth and time when running other tests.
        /// </remarks>
        [TestMethod]
        public void DownloadDataTest()
        {
            string gfscode = "2017012518_060";
            /* Don't kill the network by accidentally running this a bunch */
            RealTimeWeather weatherData = new RealTimeWeather();
            PrivateObject obj = new PrivateObject(weatherData);
            if (false)
            {
                obj.Invoke("DownloadData", gfscode);
            }
            else
            {
                Assert.Inconclusive("Did not run test. Must change if statement to true because of large file size");
            }

        }
        /// <summary>
        /// Tests to make sure that the InterpretData method generates data for each of the defined pressure levels (31 total)
        /// </summary>
        [TestMethod]
        public void InterpretDataCountTest()
        {
            string gfscode = "2017012518_060";
            RealTimeWeather weatherData = new RealTimeWeather();
            /* Download the file if it does not exist. This only needs to be done once */
            if (!System.IO.File.Exists(@"C:\Horizon\gfs.t18z.pgrb2.0p50.f060.grb2"))
            {
                PrivateObject obj = new PrivateObject(weatherData);
                obj.Invoke("DownloadData", gfscode);
            }
            weatherData.InterpretData(gfscode);
            int expectedCount = 31;
            Assert.AreEqual(expectedCount, weatherData.pressure.Count);
            Assert.AreEqual(expectedCount, weatherData.temperature.Count);
            Assert.AreEqual(expectedCount, weatherData.uVelocity.Count);
            Assert.AreEqual(expectedCount, weatherData.vVelocity.Count);
        }
        /// <summary>
        /// Tests that the right gfscode format is generated from a datetime object
        /// </summary>
        [TestMethod]
        public void GFSDatecodeTest() 
        {
            // TODO: Figure out how to test a future date. Have tested on the first release
            RealTimeWeather weatherData = new RealTimeWeather();
            PrivateObject obj = new PrivateObject(weatherData);
            var datecode = obj.Invoke("ConvertToNearestGFS", (new DateTime(2017, 01, 20, 3, 6, 1)));
            Assert.AreEqual("2017012006_005", datecode.ToString());
        }
        /// <summary>
        /// Tests that the new generated string correspondes to the same forecast time but
        /// using the previous day's run.
        /// </summary>
        [TestMethod]
        public void UsePreviousDaysRunTest()
        {
            // TODO: Figure out how to test a future date. Have tested on the first release
            RealTimeWeather weatherData = new RealTimeWeather();
            PrivateObject obj = new PrivateObject(weatherData);
            var gfscode = obj.Invoke("UsePreviousDaysRun", "2017012106_001");
            Assert.AreEqual("2017012006_025", gfscode.ToString());
        }
    }
}