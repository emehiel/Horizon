using System;
using System.Xml;

namespace Utilities
{
    public static class SimParameters
    {
        public static double SimStartJD { get; private set; }

        public static double SimStartSeconds { get; private set; }

        public static double SimEndSeconds { get; private set; }

        public static string ScenarioName { get; private set; }
        
        public static string OutputDirector { get; private set; }

        private static bool _isInitialized = false;

        public static bool LoadSimParameters(XmlNode simulationXMLNode, string scenarioName)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;

                ScenarioName = scenarioName;
                Console.WriteLine( "Loading simulation parameters... " );

                SimStartJD = Convert.ToDouble(simulationXMLNode.Attributes["SimStartJD"].Value);

                Console.WriteLine("\tSimulation Start Julian Date: {0}",SimStartJD);

                if (simulationXMLNode.Attributes["SimStartSeconds"] != null)
                    SimStartSeconds = Convert.ToDouble(simulationXMLNode.Attributes["SimStartSeconds"].Value);
                else
                    SimStartSeconds = 0;
                Console.WriteLine("\tStart Epoch: {0} seconds", SimStartSeconds);

                SimEndSeconds = Convert.ToDouble(simulationXMLNode.Attributes["SimEndSeconds"].Value);
                Console.WriteLine("\tEnd Epoch: {0} seconds", SimEndSeconds);

                //OutputDirector = simulationXMLNode.Attributes["OutputDirectory"].Value;

                return true;
            }
            else
                return false;
        }

        public static double EARTH_RADIUS = 6378.137; //km
    }
}
