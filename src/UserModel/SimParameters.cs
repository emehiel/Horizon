// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UserModel
{
    public static class SimParameters
    {
        #region Attributes
        public static double EARTH_RADIUS = 6378.137; //km
        public static double SimStartJD { get; private set; }
        public static double SimStartSeconds { get; private set; }
        public static double SimEndSeconds { get; private set; }
        public static double SimStepSeconds { get; private set; }
        public static string ScenarioName { get; private set; }
        public static string OutputDirectory { get; private set; }

        private static bool _isInitialized = false;
        #endregion

        public static bool LoadSimulationJson(JObject simulationJson, string name)
        {
            try
            {
                SimParameters.ScenarioName = name;
                Console.WriteLine($"Loading simulation parameters... for scenario {ScenarioName}");

                if (JsonLoader<double>.TryGetValue("startJD", simulationJson, out double simStartJD))
                {
                    SimStartJD = simStartJD;
                    Console.WriteLine($"\tSimulation Start Julian Date: {SimStartJD}");
                }
                else
                {
                    string msg = $"Simulation Start JD is not found and is required in scenario {ScenarioName}";
                    Console.WriteLine(msg);
                    throw new ArgumentException(msg);
                }

                if (JsonLoader<double>.TryGetValue("startSeconds", simulationJson, out double simStartSeconds))
                {
                    SimStartSeconds = simStartSeconds;
                    Console.WriteLine($"\tSimulation Start Seconds: {SimStartSeconds}");
                }
                else
                {
                    string msg = $"Simulation Start Seconds is not found and is required in scenario {ScenarioName}";
                    Console.WriteLine(msg);
                    throw new ArgumentException(msg);
                }
                if (JsonLoader<double>.TryGetValue("endSeconds", simulationJson, out double simEndSeconds))
                {
                    SimEndSeconds = simEndSeconds;
                    Console.WriteLine($"\tSimulation End Seconds: {SimEndSeconds}");
                }
                else
                {
                    string msg = $"Simulation End Seconds is not found and is required in scenario {ScenarioName}";
                    Console.WriteLine(msg);
                    throw new ArgumentException(msg);
                }
                if (JsonLoader<double>.TryGetValue("stepSeconds", simulationJson, out double simStepSeconds))
                {
                    SimStepSeconds = simStepSeconds;
                    Console.WriteLine($"\tSimulation Time Step Seconds: {SimStepSeconds}");
                }
                else
                {
                    string msg = $"Simulation Step Seconds is not found and is required in scenario {SimStepSeconds}";
                    Console.WriteLine(msg);
                    throw new ArgumentException(msg);
                }

                return true;
            }
            catch (Exception e) {
                // TO DO - Add log entry?
                Console.WriteLine($"Simulation Parameters not loaded. {e.Message}"); 
                return false;
            }
        }
        /// <summary>
        /// Load simulation parameters from the xml node 
        /// </summary>
        /// <param name="simulationXMLNode"></param>
        /// <param name="scenarioName"></param>
        /// <returns></returns>
        public static bool LoadSimParameters(XmlNode simulationXMLNode, string scenarioName)
        {
            try
            {

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

                SimStepSeconds = Convert.ToDouble(simulationXMLNode.Attributes["simStepSeconds"].Value);
                //OutputDirector = simulationXMLNode.Attributes["OutputDirectory"].Value;

                return true;
            }
            catch
            {
                Console.WriteLine("Simulation Parameters not loaded");
                return false;
            }

        }
    }
}
