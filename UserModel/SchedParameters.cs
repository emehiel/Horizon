// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Xml;

namespace UserModel
{
    /// <summary>
    /// Static class to maintain simulation parameters
    /// </summary>
    public static class SchedParameters
    {
        #region Attributes
        public static double SimStepSeconds { get; private set; }
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

                SimStepSeconds = Convert.ToDouble(schedulerXMLNode.Attributes["simStepSeconds"].Value);
                Console.WriteLine("  Scheduler time step: {0} seconds", SimStepSeconds);

                MaxNumScheds = Convert.ToInt32(schedulerXMLNode.Attributes["maxNumSchedules"].Value);
                Console.WriteLine("  Maximum number of schedules: {0}", MaxNumScheds);

                NumSchedCropTo = Convert.ToInt32(schedulerXMLNode.Attributes["numSchedCropTo"].Value);

                Console.WriteLine("  Number of schedules to crop to: {0}", NumSchedCropTo);
                return true;
            }
            catch
            {
                Console.WriteLine("Failed to load SchedParameters.");
                return false;
            }
        }
    }
}
