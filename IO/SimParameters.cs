using System;
using System.Xml;
using System.Collections.Generic;

namespace UserModel
{
    public static class SimParameters
    {
        public static double EARTH_RADIUS = 6378.137; //km

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


/* TODO: Morgan

        bool loadSchedulerParams(XMLNode& schedParametersNodeXML, Scheduler* scheduler)
        {
            cout << endl << "Loading scheduler parameters... " << endl;
            scheduler->setMaxNumSchedules(atoi(schedParametersNodeXML.getAttribute("MaxSchedules")));
            cout << "  Maximum number of schedules: " << atoi(schedParametersNodeXML.getAttribute("MaxSchedules")) << endl;
            scheduler->setSchedCropTo(atoi(schedParametersNodeXML.getAttribute("CropTo")));
            cout << "  Number of schedules to crop to: " << atoi(schedParametersNodeXML.getAttribute("CropTo")) << endl;
            scheduler->setStepLength(atof(schedParametersNodeXML.getAttribute("SimStep")));
            cout << "  Scheduler time step: " << atof(schedParametersNodeXML.getAttribute("SimStep")) << " seconds" << endl;
            return true;
        }

        const bool loadTargetsIntoTaskList(XMLNode & targetDeckXMLNode, vector <const Task*>* tasks)
{
	cout << endl << "Loading target deck..." << endl;
	int n = targetDeckXMLNode.nChildNode("TARGET"); // this gets the number of "TARGET" tags:
        bool allLoaded = true;

        XMLNode targetNode;
	for(int i = 0; i<n; i++) {
		targetNode = targetDeckXMLNode.getChildNode("TARGET", i);
		string targetType, taskType;

		if(targetNode.isAttributeSet("TaskType"))
			taskType = targetNode.getAttribute("TaskType");
		else {
			allLoaded = false;
			continue;
		}
		if(targetNode.isAttributeSet("MaxTimes"))
			tasks->push_back(new Task(taskType, new Target(targetNode), atoi(targetNode.getAttribute("MaxTimes"))));
		else
			tasks->push_back(new Task(taskType, new Target(targetNode)));

		// USER - Add other target properties in Target Constructor using XML input
	}
cout << "Number of Targets Loaded: " << n << endl;
	
	return allLoaded;
}
*/
    }
}
