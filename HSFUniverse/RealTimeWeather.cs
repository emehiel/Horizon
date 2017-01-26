using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Grib.Api;

namespace HSFUniverse
{
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
    public class RealTimeWeather
    {
        public SortedList<double, double> uVelocity; 
        public SortedList<double, double> vVelocity;
        public SortedList<double, double> pressure;
        public SortedList<double, double> temperature;
        public SortedList<double, double> density;
        private string _fileName;
        public string fileName
        {
            get { return _fileName; }
        }

        /// <summary>
        /// Creates the atmosphere at the current time
        /// </summary>
        public RealTimeWeather()
        {
            DateTime date = DateTime.Now;
            CreateAtmosphere(date);
        }  
        /// <summary>
        /// Creates the atmosphere at the specfied time
        /// </summary>
        /// <param name="date"></param>
        public RealTimeWeather(DateTime date)
        {
            CreateAtmosphere(date);
        }
        /// <summary>
        /// Downloads (if necessary) the weather data and generates the atmospheric lookup tables
        /// </summary>
        /// <param name="date"></param>
        private void CreateAtmosphere(DateTime date)
        {
            string gfscode = ConvertToNearestGFS(date);
            CreateFilename(gfscode);
            if (!System.IO.File.Exists(@"C:\Horizon\" + fileName))
            {
                DownloadData(gfscode);
            }
            InterpretData();
            double airGasConstant = 286.9;
            foreach(double h in pressure.Keys)
            {
                density.Add(h, pressure[h] / temperature[h] / airGasConstant);
            }
        }
        /// <summary>
        /// Creates the GRIB2 filename on the NECP servers from the gfscode
        /// </summary>
        /// <param name="gfscode"></param>
        private void CreateFilename(string gfscode)
        {
            StringBuilder name = new StringBuilder("gfs.t");
            name.Append(gfscode, 8, 2);
            name.Append("z.pgrb2.0p50.f"); // use the .5 deg resolution data. can also use 1 or .25 deg if desired
            name.Append(gfscode, 11, 3);
            name.Append(".grb2");
            _fileName = name.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="gfscode"></param>
        public void DownloadData(string gfscode)
        {
            string directory = @"c:\Horizon\";
            System.IO.Directory.CreateDirectory(directory);
            StringBuilder url = null; 
            HttpWebResponse response;
            for (int i = 0; i < 3; i++) {
                url = new StringBuilder("http://www.ftp.ncep.noaa.gov/data/nccf/com/gfs/prod/gfs.");
                url.Append(gfscode, 0, 10);
                url.Append("/");
                CreateFilename(gfscode);
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
                        gfscode = UsePreviousDaysRun(gfscode);
                        CreateFilename(gfscode);
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
        public void InterpretData()
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
                                 where (m.Latitude.Equals(35)) && (m.Longitude.Equals(180+120))
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
                uVelocity = new SortedList<double, double>(u, Comparer<double>.Default);
                vVelocity = new SortedList<double, double>(v, Comparer<double>.Default);
                temperature = new SortedList<double, double>(t, Comparer<double>.Default);
                pressure = new SortedList<double, double>(h, Comparer<double>.Default);

                Console.WriteLine("Finished Sorting");
            }
        }
        public void InterpretData(string gfscode)
        {
            CreateFilename(gfscode);
            InterpretData();
        }
        /// <summary>
        /// Converts a DateTime object into a string that relates to the GFS output files
        /// </summary>
        /// <param name="date"></param>
        /// <returns name="gfscode"></returns>
        private string ConvertToNearestGFS(DateTime date)
        {
            int hoursInFuture = 0;
            date = date.ToUniversalTime();
            int gfsRun = (int)Math.Floor(date.Hour / 6.0) * 6;
            string day = date.ToString("yyyyMMdd");

            /* Check if time is in the future */
            if (DateTime.UtcNow < date)
            {
                /* Validate that model forecast is within range and then find nearest hourly forecast time */
                TimeSpan ts = date - DateTime.UtcNow;
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
                if (date.Hour % 6 != 0)
                {
                    hoursInFuture = date.Hour - gfsRun;
                }
                else
                {
                    hoursInFuture = 0;
                }
            }
            /* Convert to strings and return*/
            string gfs = gfsRun.ToString("d2");
            string future = hoursInFuture.ToString("d3");
            return (day  + gfs + "_" + future);
        }
        /// <summary>
        /// Updates the gfscode to use the same run number on the previous day. This is 
        /// meant for if the NWS servers are having issues, or the forcast for the needed time
        /// has not been generated by the most recent run yet.
        /// </summary>
        /// <param name="gfscode"></param>
        /// <returns name="gfscode">The updated gfscode</returns>
        private string UsePreviousDaysRun(string gfscode)
        {
            DateTime date = new DateTime(Convert.ToInt16(gfscode.Substring(0, 4)),
                Convert.ToInt16(gfscode.Substring(4, 2)),
                Convert.ToInt16(gfscode.Substring(6, 2)),
                Convert.ToInt16(gfscode.Substring(8, 2)) + Convert.ToInt16(gfscode.Substring(11, 3)),
                0, 0);
            date = date.AddDays(-1);
            gfscode = ConvertToNearestGFS(date.ToLocalTime());
            gfscode = gfscode.Replace(gfscode.Substring(11, 3), (((Convert.ToInt16(gfscode.Substring(11, 3)) + 24)).ToString("d3")));
            return gfscode;
        }
        //TODO: Write lat/ long interpreter
    }
}
