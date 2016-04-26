using System;
using System.Xml;

namespace UserModel
{
    public static class SchedParameters
    {
        public static double SimStepSeconds { get; private set; }
        public static int MaxNumScheds { get; private set; }
        public static int NumSchedCropTo { get; private set; }

        private static bool _isInitialized = false;

        public static bool LoadSchedParameters(XmlNode schedulerXMLNode)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;

                Console.WriteLine("Loading scheduler parameters... ");

                SimStepSeconds = Convert.ToDouble(schedulerXMLNode.Attributes["simStepSeconds"].Value);
                Console.WriteLine("  Scheduler time step: {0} seconds", SimStepSeconds);

                MaxNumScheds = Convert.ToInt32(schedulerXMLNode.Attributes["maxNumSchedules"].Value);
                Console.WriteLine("  Maximum number of schedules: {0}", MaxNumScheds);

                NumSchedCropTo = Convert.ToInt32(schedulerXMLNode.Attributes["numSchedCropTo"].Value);
                Console.WriteLine("  Number of schedules to crop to: {0}", NumSchedCropTo);

                return true;
            }
            else
                return false;
        }

    }
}
