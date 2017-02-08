﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Grib.Api;

namespace HSFUniverse
{

    public abstract class Atmosphere
    {
        protected SortedList<double, double> uVelocityData;
        protected SortedList<double, double> vVelocityData;
        protected SortedList<double, double> pressureData;
        protected SortedList<double, double> temperatureData;
        protected SortedList<double, double> densityData;

        protected const double GRAVITY = 9.80665;
        protected const double IDEAL_GAS = 286.9;

        abstract public double temperature(double height);
        abstract public double pressure(double height);
        abstract public double density(double height);
        abstract public double uVelocity(double height);
        abstract public double vVelocity(double height);

        abstract public void CreateAtmosphere();


    }

    /// <summary>
    /// The RealTimeWeather class generates near real time atmospheric conditions around the globe.
    /// It uses the GFS weather model outputs from the NWS to create lookup tables.
    /// </summary>
    /// <remarks> 
    /// The NWS has GFS model outputs from the past 2-3 months availible "online" <see cref="https://www.ncdc.noaa.gov/data-access/model-data/model-datasets/global-forcast-system-gfs"/>
    /// to use "offline" data you must manually download the data from the NWS website. The GFS
    /// forecasts 196 hours (10.5 days) into the future but the closer to the model run, the more
    /// accurate the results so this class finds the closest model runtime to the requested forecast time.
    /// </remarks>
    /// <see cref="https://www.ncdc.noaa.gov/data-access/model-data/model-datasets/global-forcast-system-gfs"/>
    public class RealTimeAtmosphere : Atmosphere
    {

        private string _fileName;
        private double _latitude;
        private double _longitude;
        private DateTime _date = DateTime.Now;
        private string _gfscode;

        public string fileName
        {
            get { return _fileName; }
        }
        public double latitude
        {
            get { return _latitude; }
        }
        public double longitude
        {
            get { return _longitude; }
        }

        
        public void SetLocation(double latitude, double longitude)
        {
            /* Round to nearest 0.5 deg because that is what our data is in */
            latitude = Math.Round(latitude / 0.5) * 0.5;
            longitude = Math.Round(longitude / 0.5) * 0.5;
            /* Wrap location to between 0 and 360 */
            latitude = WrapTo360(latitude);
            longitude = WrapTo360(longitude);
            /* Set the values in the atmosphere class */
            _latitude = latitude;
            _longitude = longitude;
        }
        public void SetDate(DateTime date)
        {
            _date = date;
            ConvertToNearestGFS();
        }
        #region Overrides
        /// <summary>
        /// Downloads (if necessary) the weather data and generates the atmospheric lookup tables 
        /// at the specified time and location
        /// </summary>
        /// <param name="date"></param>
        public override void CreateAtmosphere()
        {
            ConvertToNearestGFS();
            CreateFilename();
            if (!System.IO.File.Exists(@"C:\Horizon\" + fileName))
            {
                DownloadData();
            }
            InterpretData();
            double airGasConstant = 286.9;
            foreach(double h in pressureData.Keys)
            {
                densityData.Add(h, pressureData[h] / temperatureData[h] / airGasConstant);
            }
        }
        public override double temperature(double height)
        {
            return LinearInterpolate(temperatureData, height);
        }
        public override double pressure(double height)
        {
            return LinearInterpolate(pressureData, height);
        }
        public override double density(double height)
        {
            return pressure(height) / temperature(height) / IDEAL_GAS;
        }
        public override double uVelocity(double height)
        {
            return LinearInterpolate(uVelocityData, height);
        }
        public override double vVelocity(double height)
        {
            return LinearInterpolate(vVelocityData, height);
        }
        #endregion
        /// <summary>
        /// Creates the GRIB2 filename on the NECP servers from the gfscode
        /// </summary>
        /// <param name="gfscode"></param>
        private void CreateFilename()
        {
            StringBuilder name = new StringBuilder("gfs.t");
            name.Append(_gfscode, 8, 2);
            name.Append("z.pgrb2.0p50.f"); // use the .5 deg resolution data. can also use 1 or .25 deg if desired
            name.Append(_gfscode, 11, 3);
            name.Append(".grb2");
            _fileName = name.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="gfscode"></param>
        private void DownloadData()
        {
            string directory = @"c:\Horizon\";
            System.IO.Directory.CreateDirectory(directory);
            StringBuilder url = null; 
            HttpWebResponse response;
            for (int i = 0; i < 3; i++) {
                url = new StringBuilder("http://www.ftp.ncep.noaa.gov/data/nccf/com/gfs/prod/gfs.");
                url.Append(_gfscode, 0, 10);
                url.Append("/");
                CreateFilename();
                url.Append(fileName);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url.ToString()));
                request.Method = "HEAD";
                
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    break;
                }
                catch (WebException e) 
                {
                    /* Catch 404 and go back 1 day in case NWS was having issues or file wasn't yet posted */
                    HttpWebResponse errorResponse = e.Response as HttpWebResponse;
                    if (errorResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        UsePreviousDaysRun();
                        CreateFilename();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
            
            WebClient webClient = new WebClient();
            webClient.DownloadFile(new Uri(url.ToString()), directory + fileName); // Maybe want to make Async. Want to see what other initial tasks need to be done.
            
        }
        /// <summary>
        /// Generates the values for pressure, temperatures, and u and v wind velocities 
        /// that can be used for lookup tables in atmospheric models. Created from a GFS GRIB2 file.
        /// </summary>
        /// <remarks>
        /// It generates temperature and u and v velocities at specific 
        /// atmospheric pressure levels. It also gets the geopotential height at the pressure
        /// level and converts pressure levels into the heights for all the variables. This 
        /// results in SortedLists sorted with respect to height from ~100m to ~50km
        /// </remarks>
        private void InterpretData()
        {
            Dictionary<double, double> u = new Dictionary<double, double>();
            Dictionary<double, double> v = new Dictionary<double, double>();
            Dictionary<double, double> h = new Dictionary<double, double>();
            Dictionary<double, double> t = new Dictionary<double, double>();
            // FIXME: Get relative path
            Environment.SetEnvironmentVariable("GRIB_API_DIR_ROOT", @"C:\Users\steve\Source\Repos\Horizon\packages\Grib.Api.0.7.1", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(fileName))
            {
                throw new System.IO.FileNotFoundException("Filename not defined");
            }
            using (GribFile file = new GribFile(@"c:/Horizon/" + fileName))
            {
                /* Get the data for the values we want (u and v wind velocitys and height) */ 
                var weatherData = from m in file
                                where (m.TypeOfLevel.Equals("isobaricInhPa")) && ((m.ShortName.Equals("u")) || (m.ShortName.Equals("v")) || (m.ShortName.Equals("gh")) || (m.ShortName.Equals("t")))
                                select m;
                foreach (GribMessage msg in weatherData)
                { 
                    /* Get the GribMessage at the specific location */
                    // FIXME: Allow the location to be changed
                    var msgLoc = from m in msg.GeoSpatialValues
                                 where (m.Latitude.Equals(latitude)) && (m.Longitude.Equals(longitude))
                                 select m;
                    /* Get what pressure level the value is for */
                    string pressureLevel = msg["level"].AsString(); 

                    /* Put the value in the corresponding dictionary by using the short name */
                    switch (msg.ShortName)
                        {
                        case "u": u.Add(Convert.ToDouble(pressureLevel), msgLoc.Last().Value);
                            break;
                        case "v": v.Add(Convert.ToDouble(pressureLevel), msgLoc.Last().Value);
                            break;
                        case "t": t.Add(Convert.ToDouble(pressureLevel), msgLoc.Last().Value);
                            break;
                        case "gh": h.Add(Convert.ToDouble(pressureLevel), msgLoc.Last().Value);
                            break;
                        }
                }
                /* Convert the pressure levels into geopotential heights */
                u = u.ToDictionary(kp => h[kp.Key], kp => kp.Value);
                v = v.ToDictionary(kp => h[kp.Key], kp => kp.Value);
                t = t.ToDictionary(kp => h[kp.Key], kp => kp.Value);
                /* http://stackoverflow.com/questions/2968356/linq-transform-dictionarykey-value-to-dictionaryvalue-key */
                h = h.ToDictionary(kp => kp.Value, kp => kp.Key);

                /* Put the values in to the sorted lists */
                uVelocityData = new SortedList<double, double>(u, Comparer<double>.Default);
                vVelocityData = new SortedList<double, double>(v, Comparer<double>.Default);
                temperatureData = new SortedList<double, double>(t, Comparer<double>.Default);
                pressureData = new SortedList<double, double>(h, Comparer<double>.Default);

                Console.WriteLine("Finished Sorting");
            }
        }
        /// <summary>
        /// Converts a DateTime object into a string that relates to the GFS output files
        /// </summary>
        /// <param name="date"></param>
        /// <returns name="gfscode"></returns>
        private void ConvertToNearestGFS()
        {
            int hoursInFuture = 0;
            _date = _date.ToUniversalTime();
            int gfsRun = (int)Math.Floor(_date.Hour / 6.0) * 6;
            string day = _date.ToString("yyyyMMdd");

            /* Check if time is in the future */
            if (DateTime.UtcNow < _date)
            {
                /* Validate that model forecast is within range and then find nearest hourly forecast time */
                TimeSpan ts = _date - DateTime.UtcNow;
                if (ts.Hours < 196)
                {
                    hoursInFuture = ts.Hours + ts.Days * 24;
                    gfsRun = (int)Math.Floor(DateTime.UtcNow.Hour / 6.0) * 6;
                    day = DateTime.UtcNow.ToString("yyyyMMdd");
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The requested time is too far in the future for the GFS model. Need to use the Standard Atmosphere model");
                }
            }
            else
            {
                if (_date.Hour % 6 != 0)
                {
                    hoursInFuture = _date.Hour - gfsRun;
                }
                else
                {
                    hoursInFuture = 0;
                }
            }
            /* Convert to strings and return*/
            string gfs = gfsRun.ToString("d2");
            string future = hoursInFuture.ToString("d3");
            _gfscode =  day  + gfs + "_" + future;
        }
        /// <summary>
        /// Updates the gfscode to use the same run number on the previous day. This is 
        /// meant for if the NWS servers are having issues, or the forcast for the needed time
        /// has not been generated by the most recent run yet.
        /// </summary>
        /// <param name="gfscode"></param>
        /// <returns name="gfscode">The updated gfscode</returns>
        private void UsePreviousDaysRun()
        {
            DateTime requestedDate = new DateTime(Convert.ToInt16(_gfscode.Substring(0, 4)),
                Convert.ToInt16(_gfscode.Substring(4, 2)),
                Convert.ToInt16(_gfscode.Substring(6, 2)),
                Convert.ToInt16(_gfscode.Substring(8, 2)) + Convert.ToInt16(_gfscode.Substring(11, 3)),
                0, 0);
            _date = requestedDate.AddDays(-1).ToLocalTime();
            ConvertToNearestGFS();
            _gfscode = _gfscode.Replace(_gfscode.Substring(11, 3), (((Convert.ToInt16(_gfscode.Substring(11, 3)) + 24)).ToString("d3")));
        }
        private double WrapTo360(double angle)
        {
            if (angle < 0)
            {
                angle = angle + 360;
                WrapTo360(angle);
            }
            else if (angle > 360)
            {
                angle = angle - 360;
                WrapTo360(angle);
            }
            return angle;
        }
        /// <summary>
        /// Linear interpolation using the height as the key in the data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="height"></param>
        /// <returns> Interpolated value</returns>
        private double LinearInterpolate(SortedList<double, double> data, double height)
        {
            IEnumerable<KeyValuePair<double, double>> dataBelow = temperatureData.TakeWhile(x => x.Key <= height);
            double keyBelow = dataBelow.Last().Key;
            double keyAbove = temperatureData.ElementAt(dataBelow.Count() + 1).Key;
            return (temperatureData[keyAbove] + temperatureData[keyBelow]) / (keyAbove - keyBelow) * (height - keyBelow);
        }
      
    }

    /// <summary>
    /// Implementation of the 1976 standard atmosphere for altitudes below 84 km
    /// </summary>
    //TODO: Check input altitude
    public class StandardAtmosphere : Atmosphere
    {
        SortedList<double, double[]> lookUpTable = new SortedList<double, double[]>();

        public override void CreateAtmosphere()
        {
            lookUpTable.Add(0, (new double[] { 101325, 288.15, -0.0065 }));
            lookUpTable.Add(11000, (new double[] { 22632.1, 216.65, 0.0 }));
            lookUpTable.Add(20000, (new double[] { 5474.89, 216.65, 0.001 }));
            lookUpTable.Add(32000, (new double[] { 868.019, 228.65, 0.0028 }));
            lookUpTable.Add(47000, (new double[] { 110.906, 270.65, 0.0 }));
            lookUpTable.Add(51000, (new double[] { 66.9389, 270.65, -0.0028 }));
            lookUpTable.Add(71000, (new double[] { 3.95642, 214.65, -0.002 }));
        }
        public override double density(double height)
        {
           return pressure(height) / temperature(height) / IDEAL_GAS;
        }
        public override double pressure(double height)
        {
            double key = lookUpTable.TakeWhile(x => x.Key <= height).Last().Key;
            if (lookUpTable[key].ElementAt(2) != 0.0)
            {
                return lookUpTable[key].ElementAt(0) * Math.Pow(temperature(height) /
                    lookUpTable[key].ElementAt(1), -GRAVITY / IDEAL_GAS / lookUpTable[key].ElementAt(2));
            }
            else
            {
                return lookUpTable[key].ElementAt(0) * Math.Exp(-GRAVITY * (height - key) /
                    IDEAL_GAS / lookUpTable[key].ElementAt(1));
            }
        }
        public override double temperature(double height)
        {
            double key = lookUpTable.TakeWhile(x => x.Key <= height).Last().Key;
            return lookUpTable[key].ElementAt(1) + lookUpTable[key].ElementAt(2) * (height - key);
        }
        public override double uVelocity(double height)
        {
            throw new NotImplementedException();
        }
        public override double vVelocity(double height)
        {
            throw new NotImplementedException();
        }
    }
}