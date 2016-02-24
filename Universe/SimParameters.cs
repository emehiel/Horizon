using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Universe
{
    public static class SimParameters
    {
        public static double _simStartJD { get; private set; }

        public static double _simStartSeconds { get; private set; }

        public static double _simEndSeconds { get; private set; }

        public static string _scenarioName { get; private set; }
        
        public static string _outputDirector { get; private set; }

        public static bool isInitialized { get; private set; }

        public static bool LoadSimParameters(XmlNode simulationXMLNode, string scenarioName)
        {
            if (!isInitialized)
            {
                isInitialized = true;

                _scenarioName = scenarioName;
                Console.WriteLine( "Loading simulation parameters... " );

                _simStartJD = Convert.ToDouble(simulationXMLNode.Attributes["SimStartJD"]);
                Console.WriteLine("\tSimulation Start Julian Date: {0}",_simStartJD);

                if (simulationXMLNode.Attributes["SimStartSeconds"] != null)
                    _simStartSeconds = Convert.ToDouble(simulationXMLNode.Attributes["SimStartSeconds"]);
                else
                    _simStartSeconds = 0;
                Console.WriteLine("\tStart Epoch: {0} seconds", _simStartSeconds);

                _simEndSeconds = Convert.ToDouble(simulationXMLNode.Attributes["SimEndSeconds"]);
                Console.WriteLine("\tEnd Epoch: {0} seconds", _simEndSeconds);

                _outputDirector = Convert.ToString(simulationXMLNode.Attributes["OutputDirectory"]);

                return true;
            }
            else
                return false;
        }
    }
}
