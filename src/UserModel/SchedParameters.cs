// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Text.Json.Nodes;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UserModel
{
    /// <summary>
    /// Static class to maintain simulation parameters
    /// </summary>
    public static class SchedParameters
    {
        #region Attributes
        //public static double SimStepSeconds { get; private set; }
        public static int MaxNumScheds { get; private set; }
        public static int NumSchedCropTo { get; private set; }
        #endregion

        /// <summary>
        /// Load the schedule parameters from the XML file
        /// </summary>
        /// <param name="schedulerXMLNode"></param>
        /// <returns></returns>
        public static bool LoadSchedParameters(XmlNode schedulerXMLNode)
        {
            try
            {
                Console.WriteLine("Loading scheduler parameters... ");

                //SimStepSeconds = Convert.ToDouble(schedulerXMLNode.Attributes["simStepSeconds"].Value);
                //Console.WriteLine("  Scheduler time step: {0} seconds", SimStepSeconds);

                MaxNumScheds = Convert.ToInt32(schedulerXMLNode.Attributes["maxNumSchedules"].Value);
                Console.WriteLine("  Maximum number of schedules: {0}", MaxNumScheds);

                NumSchedCropTo = Convert.ToInt32(schedulerXMLNode.Attributes["numSchedCropTo"].Value);
                Console.WriteLine("  Number of schedules to crop to: {0}", NumSchedCropTo);

                return true;
            }
            catch 
            {
                Console.WriteLine("Schedule Parameters Failed to Load");
                return false; 
            }
        }

        public static bool LoadScheduleJson(JObject scheduleJson)
        {
            try
            {
                Console.WriteLine($"Loading scheduling parameters... for scenario {SimParameters.ScenarioName}");

                if (JsonLoader<int>.TryGetValue("maxSchedules", scheduleJson, out int maxSchedules))
                {
                    MaxNumScheds = maxSchedules;
                    Console.WriteLine($"\tScheduler max schedules: {MaxNumScheds}");
                }
                else
                {
                    string msg = $"Scheduler max schedules is not found and is required in scenario {SimParameters.ScenarioName}";
                    Console.WriteLine(msg);
                    throw new ArgumentException(msg);
                }

                if (JsonLoader<int>.TryGetValue("cropTo", scheduleJson, out int cropTo))
                {
                    NumSchedCropTo = cropTo;
                    Console.WriteLine($"\tScheduler Crop To: {NumSchedCropTo}");
                }
                else
                {
                    string msg = $"Scheduler Crop To is not found and is required in scenario {SimParameters.ScenarioName}";
                    Console.WriteLine(msg);
                    throw new ArgumentException(msg);
                }
                
                return true;
            }
            catch (Exception e)
            {
                // TO DO - Add log entry?
                Console.WriteLine($"Simulation Parameters not loaded. {e.Message}");
                return false;
            }
        }

    }
}
