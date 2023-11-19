using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HSFUniverse;
using System.Collections.Generic;
using System.IO;
using Utilities;

namespace HSFUniverse.Tests
{
    [TestClass]
    public class RealTimeWeatherTest
    {
        // TODO: Not sure if I want to move to NUnit here. Discuss with Mehiel 
        public DateTime TestTime = new DateTime(2017,1,20);
        /// <summary>
        /// Tests that DownloadData runs without exceptiong. Doesn't check for results.
        /// </summary>
        /// <remarks>
        /// This test will normally return inconclusive. The if statement must be changed
        /// to true for it to actually download data. This because the file is ~70 MB and 
        /// we want to save bandwidth and time when running other tests.
        /// </remarks>
        [TestMethod, Ignore]
        public void DownloadDataTest()
        {
            string gfscode = "2017012518_060";
            /* Don't kill the network by accidentally running this a bunch */
            RealTimeAtmosphere weatherData = new RealTimeAtmosphere();
            PrivateObject obj = new PrivateObject(weatherData);
            obj.SetFieldOrProperty("_gfscode", gfscode);
            obj.Invoke("DownloadData");

        }
        /// <summary>
        /// Tests to make sure that the InterpretData method generates data for each of the defined pressure levels (31 total)
        /// </summary>
        [TestMethod]
        public void InterpretDataCountTest()
        {
            string gfscode = "2017012518_060";
            RealTimeAtmosphere weatherData = new RealTimeAtmosphere();
            
            PrivateObject obj = new PrivateObject(weatherData);
            obj.SetFieldOrProperty("_gfscode", gfscode);
            /* Download the file if it does not exist. This only needs to be done once */
            if (!System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\..\..\Data\gfs.t18z.pgrb2.0p50.f060.grb2"))
            {
                obj.Invoke("DownloadData");
            }

            obj.SetFieldOrProperty("_filePath", AppDomain.CurrentDomain.BaseDirectory + @"\..\..\Data\gfs.t18z.pgrb2.0p50.f060.grb2");
            obj.Invoke("InterpretData");
            int expectedCount = 32;
            //weatherData.temperature(1350);
            double pressure = ((SortedList<double, double>)obj.GetFieldOrProperty("pressureData")).Count;
            double temperature = ((SortedList<double, double>)obj.GetFieldOrProperty("temperatureData")).Count;
            double uVelocity = ((SortedList<double, double>)obj.GetFieldOrProperty("uVelocityData")).Count;
            double vVelocity = ((SortedList<double, double>)obj.GetFieldOrProperty("vVelocityData")).Count;
            Assert.AreEqual(expectedCount, pressure);
            Assert.AreEqual(expectedCount, temperature);
            Assert.AreEqual(expectedCount, uVelocity);
            Assert.AreEqual(expectedCount, vVelocity);
        }
        /// <summary>
        /// Tests that the right gfscode format is generated from a datetime object
        /// </summary>
        [TestMethod]
        public void GFSDatecodeTest() 
        {
            // TODO: Figure out how to test a future date. Have tested on the first release
            RealTimeAtmosphere weatherData = new RealTimeAtmosphere();
            PrivateObject obj = new PrivateObject(weatherData);
            obj.SetFieldOrProperty("_date", (new DateTime(2017, 01, 20, 11, 6, 1, DateTimeKind.Utc)));
            obj.Invoke("ConvertToNearestGFS");
            Assert.AreEqual("2017012006_005", ((string)obj.GetFieldOrProperty("_gfscode")));
        }
        /// <summary>
        /// Tests that the new generated string correspondes to the same forecast time but
        /// using the previous day's run.
        /// </summary>
        [TestMethod]
        public void UsePreviousDaysRunTest()
        {
            // TODO: Figure out how to test a future date. Have tested on the first release
            RealTimeAtmosphere weatherData = new RealTimeAtmosphere();
            PrivateObject obj = new PrivateObject(weatherData);
            obj.SetFieldOrProperty("_gfscode", "2017012106_001");
            obj.Invoke("UsePreviousDaysRun");
            Assert.AreEqual("2017012006_025", ((string)obj.GetFieldOrProperty("_gfscode")));
        }
    }

    [TestClass]
    public class HWMTest
    {
        /*
        [TestMethod]
        public void TestWind()
        {
            int day = 95000+150;
            float ut = 12;
            float alt = 0;
            float glat = -45;
            float glon = -85;
            float stl = 6;
            float[] ap = { 0, 80 };
            //float ap = 80;
            Vector wind = HorizontalWindModel14.hwm14Interface(day, ut, alt, glat, glon, stl, ap);
            Assert.AreEqual(0.031, wind[1]);
            Assert.AreEqual(6.271, wind[2]);
        }
        */
    }
}