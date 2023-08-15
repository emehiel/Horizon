// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using HSFSystem;
using Utilities;

namespace Logging
{
    public class HSFAnalyzer
    {/*
        class LogAnalyzer   // Calculates constraint violation rates, subsystem failure rates, and task failure rates 
        {
            public List<string> AssetNames { get; private set; }       // Asset which subsystems resides in 
            public string SubName { get; private set; }                // Failed subsystem name 
            public double SubFailRate { get; private set; }            // Failure rate of subsystem 
            public List<string> TaskNames { get; private set; }        // Task names 
            public List<string> TarNames { get; private set; }         // Target names 
            public string ConName { get; private set; }                // Constraint name 
            public double ConVioRate { get; private set; }             // Violation rate of constraint 
            public List<string> VioNames { get; private set; }         // Names of state variables violating constraints 
            public List<string> ConSubNames { get; private set; }      // Names of subsystems violating constraints 
            public List<double> ConSubRates { get; private set; }      // Rates of violating subsystems 
            public double TotalScheds { get; private set; }            // Total number of failed Schedules 


            public LogAnalyzer(List<string> assetNames, string subName, double subFailRate, List<string> taskNames, List<string> tarNames, string conName, double conVioRate, List<string> vioNames,
                               List<string> conSubNames, List<double> conSubRates, double totalScheds)
            {
                AssetNames = assetNames;
                SubName = subName;
                SubFailRate = subFailRate;
                TaskNames = taskNames;
                TarNames = tarNames;
                ConName = conName;
                ConVioRate = conVioRate;
                VioNames = vioNames;
                ConSubNames = conSubNames;
                ConSubRates = conSubRates;
                TotalScheds = totalScheds;
            }


            // Logger shall have two modes: RunTime and PostProcess 
            // Each mode shall call the LogAnalyzer method 
            // RunTime will update failure Retes. 
            // PostProcess will update failure rates and periodically 
            // write it to a file for the Human user. 


            public static List<LogAnalyzer> Analyze(List<LogAnalyzer> prevAnalytics, HSFLog log, List<Constraint> constraintsList, List<Subsystem> subsysList,
                                                    List<StateVarKey<double>> stateVarList)
            {
                List<LogAnalyzer> dataOutput = new List<LogAnalyzer>();                                              // New LogAnalyzer list 
                LogAnalyzer logAnalyzer = new LogAnalyzer(null, null, 0, null, null, null, 0, null, null, null, 0);  // New LogAnalyzer class 


                string assetName = null;
                string subName = null;
                double subFailRate = 0;
                double oldSubFailRate = 0;
                string taskName = null;
                string targetName = null;
                List<string> taskNames = new List<string>();
                List<string> tarNames = new List<string>();
                List<string> oldTaskNames = new List<string>();
                List<string> oldTarNames = new List<string>();
                List<HSFLogData> subFails = new List<HSFLogData>();                    // New LogData list 
                List<string> assetNames = new List<string>();
                subFails = log.Data.FindAll(x => x.ConName == null);             // Find all subsystem failures     


                double prevFailScheds = 0;


                if (prevAnalytics.Count > 0)
                {
                    prevFailScheds = prevAnalytics.First().TotalScheds;                            // Extract the previous number of failed schedules 
                }


                double currentFailScheds = prevFailScheds + log.Data.Count;                        // Extract the current total number of failed schedules         


                foreach (Subsystem subsystem in subsysList)
                {
                    assetName = subsystem.Asset.Name;
                    assetNames.Add(assetName);
                    subName = subsystem.Name;
                    subFailRate = subFails.FindAll(x => x.SubName == subsystem.Name).Count / currentFailScheds;


                    if (prevAnalytics.Count > 0)
                    {
                        oldSubFailRate = prevAnalytics.Find(x => x.SubName == subsystem.Name).SubFailRate * prevFailScheds / currentFailScheds;
                        subFailRate = subFailRate + oldSubFailRate;
                        oldTaskNames = prevAnalytics.Find(x => x.SubName == subsystem.Name).TaskNames;
                        oldTarNames = prevAnalytics.Find(x => x.SubName == subsystem.Name).TarNames;
                    }


                    foreach (string oldTask in oldTaskNames)
                    {
                        taskName = oldTask;
                        taskNames.Add(taskName);
                    }


                    foreach (string oldTar in oldTarNames)
                    {
                        targetName = oldTar;
                        tarNames.Add(targetName);
                    }


                    foreach (HSFLogData subFailure in subFails)
                    {
                        if (subFailure.SubName == subsystem.Name)
                        {
                            taskName = subFailure.TaskName;
                            targetName = subFailure.TarName;
                            taskNames.Add(taskName);
                            tarNames.Add(targetName);
                        }
                    }


                    logAnalyzer = new LogAnalyzer(assetNames, subName, subFailRate, taskNames, tarNames, null, 0, null, null, null, currentFailScheds);
                    dataOutput.Add(logAnalyzer);
                    taskNames = new List<string>();
                    tarNames = new List<string>();
                    assetNames = new List<string>();
                }


                List<LogData> conVios = new List<LogData>();                         // New LogData list 
                List<string> conSubNames = new List<string>();                       // New string List 
                List<string> vioNames = new List<string>();                          // New string list 
                List<string> oldVioNames = new List<string>();                       // New string list 
                List<double> oldConSubVioRates = new List<double>();                 // New double list 
                List<string> oldConSubNames = new List<string>();                    // New double list 
                List<double> updatedOldConSubVioRates = new List<double>();          // New double list 
                List<double> conSubRates = new List<double>();                       // New double list           


                string ConName = null;
                string ConSubName;


                int i = 0;
                double ConVioRate = 0;
                double oldConVioRate = 0;
                double ConSubRate = 0;
                double oldConSubVioRate = 0;
                string vioName = null;


                // For each constraint 
                foreach (Constraint constraint in constraintsList)
                {
                    // Calculate constraint violation rates 
                    ConSubName = null;
                    ConName = constraint.ConName;                                                   // Populate constraint name 
                    conVios = log.Data.FindAll(x => x.ConName == constraint.ConName);               // Find all log data for the constraint violated 
                    ConVioRate = conVios.Count / currentFailScheds;                                 // Count all the constraint violations and record violation rate 


                    if (prevAnalytics.Count > 0)
                    {
                        oldConVioRate = prevAnalytics.Find(x => x.ConName == constraint.ConName).ConVioRate * prevFailScheds / currentFailScheds;
                        ConVioRate = ConVioRate + oldConVioRate;
                    }
                    // Done calculating constraint violation rates 

                    // Update old subsystem failure rates 
                    if (prevAnalytics.Count > 0)
                    {
                        oldConSubVioRates = prevAnalytics.Find(x => x.ConName == constraint.ConName).ConSubRates;
                        oldConSubNames = prevAnalytics.Find(x => x.ConName == constraint.ConName).ConSubNames;
                        if (oldConSubVioRates != null)
                        {
                            foreach (double prevConSubVioRate in oldConSubVioRates)
                            {
                                oldConSubVioRate = prevConSubVioRate * prevFailScheds / currentFailScheds;
                                updatedOldConSubVioRates.Add(oldConSubVioRate);
                            }
                        }
                    }
                    // Done updating old subsystem failure rates 


                    // Calculate subsystem violation rates 
                    foreach (SubsystemClass subsystem in subsysList)
                    {
                        if (oldConSubNames.Count > 0)
                        {
                            foreach (string oldConSubName in oldConSubNames)
                            {
                                if (oldConSubName == subsystem.SubName)
                                {
                                    ConSubName = subsystem.SubName;
                                    ConSubRate = conVios.FindAll(x => x.SubName == subsystem.SubName).Count / currentFailScheds;
                                    ConSubRate = ConSubRate + updatedOldConSubVioRates[i];
                                    conSubNames.Add(ConSubName);
                                    conSubRates.Add(ConSubRate);
                                    assetName = subsysList.Find(x => x.SubName == ConSubName).AssetName;
                                    assetNames.Add(assetName);
                                    i++;
                                }
                            }
                        }


                        else if (conVios.FindAll(x => x.SubName == subsystem.SubName).Count > 0)
                        {
                            ConSubName = subsystem.SubName;
                            ConSubRate = conVios.FindAll(x => x.SubName == subsystem.SubName).Count / currentFailScheds;
                            ConSubRate = ConSubRate + oldConSubVioRate;
                            conSubNames.Add(ConSubName);
                            conSubRates.Add(ConSubRate);
                            assetName = subsysList.Find(x => x.SubName == ConSubName).AssetName;
                            assetNames.Add(assetName);
                        }
                        i = 0;
                    }
                    // Done calculating subsystem violation rates 


                    if (assetNames.Count == 0)
                    {
                        if (prevAnalytics.Count > 0)
                        {
                            assetNames = prevAnalytics.Find(x => x.ConName == ConName).AssetNames;
                        }


                        else
                        {
                            assetName = "None";
                            assetNames.Add(assetName);
                        }
                    }


                    // Find violating state variables 
                    if (prevAnalytics.Count > 0)
                    {
                        oldVioNames = prevAnalytics.Find(x => x.ConName == ConName).VioNames;
                        foreach (string prevState in oldVioNames)
                        {
                            vioName = prevState;
                            vioNames.Add(vioName);
                        }
                    }


                    foreach (StateVar item in stateVarList)
                    {
                        if (prevAnalytics.Count > 0 && conVios.FindAll(x => x.VioName == item.StateName).Count > 0)
                        {
                            foreach (string prevState in oldVioNames)
                            {
                                if (vioName != prevState)
                                {
                                    vioName = item.StateName;
                                    vioNames.Add(vioName);
                                }
                            }


                            if (conVios.FindAll(x => x.VioName == item.StateName).Count > 0 && oldVioNames.Count == 0)
                            {
                                vioName = item.StateName;
                                vioNames.Add(vioName);
                            }
                        }


                        else if (conVios.FindAll(x => x.VioName == item.StateName).Count > 0)
                        {
                            vioName = item.StateName;
                            vioNames.Add(vioName);
                        }
                    }
                    // Done finding violating state variables 


                    logAnalyzer = new LogAnalyzer(assetNames, null, 0, null, null, ConName, ConVioRate, vioNames, conSubNames, conSubRates, currentFailScheds);
                    conSubNames = new List<string>();
                    conSubRates = new List<double>();
                    vioNames = new List<string>();
                    oldVioNames = new List<string>();
                    updatedOldConSubVioRates.Clear();
                    assetNames = new List<string>();
                    oldConSubNames = new List<string>();
                    oldConSubVioRate = 0;
                    dataOutput.Add(logAnalyzer);
                }
                // Done calculating subsystem failures and constraint violations with relative data 


                return dataOutput;
            }


            public static List<LogAnalyzer> sortBySubFails(List<LogAnalyzer> logAnalyzerData)
            {


                List<LogAnalyzer> dataOutput = new List<LogAnalyzer>();


                IEnumerable<LogAnalyzer> orgData = logAnalyzerData.OrderByDescending(x => x.SubFailRate);     // Convert logger list to IEnumerable and sort by subsystem failure rate 


                return dataOutput = orgData.ToList();                                                         // Back to list 


            }


            public static List<LogAnalyzer> sortByConVios(List<LogAnalyzer> logAnalyzerData)
            {


                List<LogAnalyzer> dataOutput = new List<LogAnalyzer>();


                IEnumerable<LogAnalyzer> orgData = logAnalyzerData.OrderByDescending(x => x.ConVioRate);     // Convert logger list to IEnumerable and sort by constraint violation rate 


                return dataOutput = orgData.ToList();                                                        // Back to list 


            }


            public static void PostProcess(List<LogAnalyzer> logAnalyzerData, List<SubsystemClass> subList)
            {
                int i = 0;
                int j = 0;
                int k = 0;


                List<LogAnalyzer> subOrgData = LogAnalyzer.sortBySubFails(logAnalyzerData);
                List<LogAnalyzer> conOrgData = LogAnalyzer.sortByConVios(logAnalyzerData);
                List<string> stateVarList = new List<string>();


                // Output Log Data Analyzer to .csv file 


                StreamWriter subOrgDataWrite = new StreamWriter(@"subsystemFailures.csv");      // Creates a new .csv file to write in 


                subOrgDataWrite.Write("Failed Subsystem");                                      // Headers 
                subOrgDataWrite.Write(",Subsystem Failure Rate");                               // Headers 
                subOrgDataWrite.Write(",Asset Name");                                           // Headers 
                subOrgDataWrite.Write(",Task Names");                                           // Headers 
                subOrgDataWrite.Write(",Target Names");                                         // Headers 
                subOrgDataWrite.Write(Environment.NewLine);                                     // Writes new line to the comma-delimited .csv file 


                foreach (LogAnalyzer item in subOrgData)
                {
                    if (item.ConName == null)
                    {
                        subOrgDataWrite.Write(item.SubName);
                        subOrgDataWrite.Write("," + item.SubFailRate);
                        subOrgDataWrite.Write("," + item.AssetNames[0]);


                        if (item.TaskNames.Count > 0)
                        {
                            foreach (string item2 in item.TaskNames)
                            {
                                if (i >= 1)
                                {
                                    subOrgDataWrite.Write(",,");
                                }
                                subOrgDataWrite.Write("," + item2);
                                subOrgDataWrite.Write("," + item.TarNames[i]);
                                subOrgDataWrite.Write(Environment.NewLine);
                                i++;
                            }
                            i = 0;
                        }


                        else
                        {
                            subOrgDataWrite.Write(Environment.NewLine);
                        }
                    }
                }
                subOrgDataWrite.Close();                                                        // Closes the log analyzer data .csv file 


                StreamWriter conOrgDataWrite = new StreamWriter(@"constraintViolations.csv");   // Creates a new .csv file to write in 


                conOrgDataWrite.Write("Violated Constraint");                                   // Headers 
                conOrgDataWrite.Write(",Constraint Violation Rate");                            // Headers 
                conOrgDataWrite.Write(",Asset Names");                                          // Headers 
                conOrgDataWrite.Write(",Violating Subsystems");                                 // Headers 
                conOrgDataWrite.Write(",Violating Subsystem Rates");                            // Headers 
                conOrgDataWrite.Write(",Violating State Variables");                            // Headers 
                conOrgDataWrite.Write(Environment.NewLine);                                     // Writes new line to the comma-delimited .csv file 


                foreach (LogAnalyzer item in conOrgData)
                {
                    if (item.ConName != null)
                    {
                        conOrgDataWrite.Write(item.ConName);
                        conOrgDataWrite.Write("," + item.ConVioRate);


                        if (item.ConSubNames.Count > 0)
                        {
                            foreach (string conSubName in item.ConSubNames)
                            {
                                if (i == 0)
                                {
                                    conOrgDataWrite.Write("," + item.AssetNames[i]);
                                    conOrgDataWrite.Write("," + item.ConSubNames[i]);
                                    conOrgDataWrite.Write("," + item.ConSubRates[i]);
                                    SubsystemClass subsys = subList.Find(x => x.SubName == item.ConSubNames[i]);
                                    foreach (string item3 in item.VioNames)
                                    {
                                        if (subsys.StateVars.Contains(item3))
                                        {
                                            if (j > 0 && stateVarList.Contains(item3) == false)
                                            {
                                                conOrgDataWrite.Write(",,,,," + item3);
                                                stateVarList.Add(item3);
                                                conOrgDataWrite.Write(Environment.NewLine);
                                            }
                                            else if (j == 0)
                                            {
                                                stateVarList.Clear();
                                                conOrgDataWrite.Write("," + item3);
                                                stateVarList.Add(item3);
                                                conOrgDataWrite.Write(Environment.NewLine);
                                            }
                                            j++;
                                        }
                                    }
                                    stateVarList.Clear();
                                    j = 0;
                                }


                                else
                                {
                                    conOrgDataWrite.Write(",," + item.AssetNames[i]);
                                    conOrgDataWrite.Write("," + item.ConSubNames[i]);
                                    conOrgDataWrite.Write("," + item.ConSubRates[i]);
                                    SubsystemClass subsys = subList.Find(x => x.SubName == item.ConSubNames[i]);
                                    foreach (string item3 in item.VioNames)
                                    {
                                        if (subsys.StateVars.Contains(item3))
                                        {
                                            if (k == 0)
                                            {
                                                conOrgDataWrite.Write("," + item3);
                                                stateVarList.Add(item3);
                                                conOrgDataWrite.Write(Environment.NewLine);
                                            }
                                            else if (j > 0 && stateVarList.Contains(item3) == false)
                                            {
                                                conOrgDataWrite.Write(",,,,," + item3);
                                                stateVarList.Add(item3);
                                                conOrgDataWrite.Write(Environment.NewLine);
                                            }
                                            j++;
                                            k++;
                                        }
                                    }
                                    stateVarList.Clear();
                                }
                                k = 0;
                                i++;
                            }
                            i = 0;
                            j = 0;
                        }


                        else
                        {
                            conOrgDataWrite.Write(Environment.NewLine);
                        }
                    }
                }
                conOrgDataWrite.Close();                                                        // Closes the logger data .csv file 
            }


            public static List<Constraint> reorderCons(List<LogAnalyzer> logAnalyzerData, List<Constraint> constraintsList)
            {
                Constraint ConstraintClass;
                List<Constraint> newConstraintList = new List<Constraint>();
                List<LogAnalyzer> newLogAnalyzerList = new List<LogAnalyzer>();
                LogAnalyzer logAnalyzer;


                foreach (Constraint constraint in constraintsList)
                {
                    logAnalyzer = logAnalyzerData.Find(x => x.ConName == constraint.ConName);
                    newLogAnalyzerList.Add(logAnalyzer);
                }


                IEnumerable<LogAnalyzer> orgData = newLogAnalyzerList.OrderByDescending(x => x.ConVioRate);     // Convert log analyzer list to IEnumerable and sort by constraint violation rate 


                newLogAnalyzerList = orgData.ToList();                                                          // Back to list 


                foreach (LogAnalyzer logAnalyzerDataObject in newLogAnalyzerList)                               // Create new order constraint list  
                {
                    ConstraintClass = new Constraint(logAnalyzerDataObject.ConName, 0, null);
                    newConstraintList.Add(ConstraintClass);
                }


                return newConstraintList;
            }


            public static List<SubsystemClass> reorderSubs(List<LogAnalyzer> logAnalyzerData, List<SubsystemClass> subsystemList)
            {
                SubsystemClass Subsystem;
                List<SubsystemClass> newSubsystemList = new List<SubsystemClass>();
                List<LogAnalyzer> newLogAnalyzerList = new List<LogAnalyzer>();
                LogAnalyzer logAnalyzer;


                foreach (SubsystemClass subsystem in subsystemList)
                {
                    logAnalyzer = logAnalyzerData.Find(x => x.SubName == subsystem.SubName);
                    newLogAnalyzerList.Add(logAnalyzer);
                }


                IEnumerable<LogAnalyzer> orgData = newLogAnalyzerList.OrderByDescending(x => x.SubFailRate);     // Convert log analyzer list to IEnumerable and sort by subsystem failure rate 


                newLogAnalyzerList = orgData.ToList();                                                           // Back to list 


                foreach (LogAnalyzer logAnalyzerDataObject in newLogAnalyzerList)                                // Create new order subsystem list  
                {
                    Subsystem = new SubsystemClass(logAnalyzerDataObject.SubName, null, null);
                    newSubsystemList.Add(Subsystem);
                }


                return newSubsystemList;
            }
        }
        */
    }
    
}

