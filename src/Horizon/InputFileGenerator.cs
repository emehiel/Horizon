using System;
using System.IO;
using Newtonsoft.Json; 

namespace Horizon
{
    public class InputFileGenerator
    {
        private readonly string modelBasePath;

        public InputFileGenerator(string modelBasePath)
        {
            this.modelBasePath = modelBasePath;
        }

        public void GenerateInputFile(string[] args)
        {
            var parameters = new
            {
                ScenarioName = "",
                SimStartJD = "",
                SimStartSeconds = "",
                SimEndSeconds = "",
                MaxNumSchedules = "",
                NumSchedCropTo = "",
                SimStepSeconds = ""
            };

            // Check if command-line arguments are provided
            if (args.Length == 7)
            {
                parameters = new
                {
                    ScenarioName = args[0],
                    SimStartJD = args[1],
                    SimStartSeconds = args[2],
                    SimEndSeconds = args[3],
                    MaxNumSchedules = args[4],
                    NumSchedCropTo = args[5],
                    SimStepSeconds = args[6]
                };
            }
            else if (File.Exists(Path.Combine(modelBasePath, "ModelBaseSimulationInput.json")))
            {
                // Read from configuration file if it exists and command-line arguments are not provided
                string configContent = File.ReadAllText(Path.Combine(modelBasePath, "ModelBaseSimulationInput.json"));
                parameters = JsonConvert.DeserializeObject<dynamic>(configContent);
            }
            else
            {
                // Use hardcoded default values (taken from Aeolus)
                parameters = new
                {
                    ScenarioName = "ModelBase",
                    SimStartJD = "2459295.0",
                    SimStartSeconds = "0.0",
                    SimEndSeconds = "5504",
                    MaxNumSchedules = "10",
                    NumSchedCropTo = "5",
                    SimStepSeconds = "15"
                };
            }

            // Path where the new XML file will be saved
            string outputPath = Path.Combine(modelBasePath, "ModelBaseScenario.xml");
            // Generate the XML file
            GenerateXml(outputPath, parameters);

            // Check if it worked
            Console.WriteLine("XML file generated successfully at: " + outputPath);
        }

        // Method to generate the XML content
        private void GenerateXml(string filePath, dynamic parameters)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineOnAttributes = true
            };

            using (XmlWriter writer = XmlWriter.Create(filePath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("SCENARIO");
                writer.WriteAttributeString("scenarioName", (string)parameters.ScenarioName);

                writer.WriteStartElement("SIMULATION_PARAMETERS");
                writer.WriteAttributeString("SimStartJD", (string)parameters.SimStartJD);
                writer.WriteAttributeString("SimStartSeconds", (string)parameters.SimStartSeconds);
                writer.WriteAttributeString("SimEndSeconds", (string)parameters.SimEndSeconds);
                writer.WriteEndElement();

                writer.WriteStartElement("SCHEDULER_PARAMETERS");
                writer.WriteAttributeString("maxNumSchedules", (string)parameters.MaxNumSchedules);
                writer.WriteAttributeString("numSchedCropTo", (string)parameters.NumSchedCropTo);
                writer.WriteAttributeString("simStepSeconds", (string)parameters.SimStepSeconds);
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}