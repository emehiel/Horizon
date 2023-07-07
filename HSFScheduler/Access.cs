﻿// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using HSFSystem;
using MissionElements;
using UserModel;

namespace HSFScheduler
{
    /// <summary>
    /// A class used to calculate and store all access windows from all systems to all task targets
    /// Also used to retrieve all accesses currently available to a system at a given simulation time
    /// This class replaces the geomAccess class from Horizon 2.3
    /// </summary>
    public class Access
    {
        #region Attributes
        public Asset Asset { get; private set; }
        public Task Task { get; set; }
        public double AccessStart { get; set; }
        public double AccessEnd { get; set; }
        #endregion

        #region Constructors
        public Access(Asset asset, Task task)
        {
            Asset = asset;
            Task = task;
        }

        public Access(XmlNode AccessXmlNode)
        {

        }
        #endregion

        #region methods
        /// <summary>
        /// Find all accesses available to an asset at the current time
        /// </summary>
        /// <param name="accesses"></param>
        /// <param name="asset"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public static List<Access> getCurrentAccessesForAsset(List<Access> accesses, Asset asset, double currentTime)
        {
            // CASE 1:  returns the access that start before the current time and end after the current time
            //Stack<Access> allAccesses = Access.getCurrentAccesses(accesses, currentTime);

            // CASE 2:  this version will return all the access that were generated at the current timestep.
            // In case 1, the following will not get through - ES = 0, TS = 7, EE = TE = 30
            List<Access> allAccesses = accesses;
            return new List<Access>(allAccesses.Where(item => item.Asset == asset)); //what is important to test from this line?
        }

        /// <summary>
        /// Find all accesses available to all assets at the current time
        /// </summary>
        /// <param name="accesses"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public static Stack<Access> getCurrentAccesses(Stack<Access> accesses, double currentTime)
        {
              return new Stack<Access>(accesses.Where(item => (item.AccessStart <= currentTime && item.AccessEnd >= currentTime)));
        }

        /// <summary>
        /// PreGenerate all Accesses from all Assets to all Tasks based on a simple Line of Sight model.  Access is based on Line of Sight, a spherical Earth and the current ECI
        /// position of the Asset and the Task.Target.DynamicState
        /// </summary>
        /// <param name="system">The system underconsideration</param>
        /// <param name="tasks"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="stepTime"></param>
        public static Stack<Access> PregenerateAccessesByAsset(SystemClass system, Stack<Task> tasks, double startTime, double endTime, double stepTime)
        {
            Stack<Access> accessesByAsset = new Stack<Access>();
            // For all assets...
            foreach (Asset asset in system.Assets)
            {
                // ...for all tasks...
                foreach (Task task in tasks)
                {
                    // ...for all time....    
                    for (double accessTime = SimParameters.SimStartSeconds; accessTime <= SimParameters.SimEndSeconds; accessTime += SchedParameters.SimStepSeconds)
                    {
                        // create a new access, or extend the access endTime if this is an update to an existing access
                        bool hasAccess = Utilities.GeometryUtilities.hasLOS(asset.AssetDynamicState.PositionECI(accessTime), task.Target.DynamicState.PositionECI(accessTime));
                        if (hasAccess)
                        {
                            bool isNewAccess;
                            if (accessesByAsset.Count == 0 || accessTime == SimParameters.SimStartSeconds || accessesByAsset.Peek().Task.Target.Name != task.Target.Name)
                                isNewAccess = true;
                            else
                                isNewAccess = (accessTime - accessesByAsset.Peek().AccessEnd) > SchedParameters.SimStepSeconds;
                            if (isNewAccess)
                            {
                                Access newAccess = new Access(asset, task);
                                newAccess.AccessStart = accessTime;
                                newAccess.AccessEnd = accessTime;
                                accessesByAsset.Push(newAccess);
                            }
                            else  // extend the access
                                accessesByAsset.Peek().AccessEnd = accessTime;
                        }
                    }
                }
            }
            return accessesByAsset;
        }

        public static List<Access> AccessesByAsset(SystemClass system, List<Task> tasks, double startTime, double endTime, double stepTIme)
        {
            List<Access> accessesByAsset = new List<Access>();
            // For all assets...
            foreach (Asset asset in system.Assets)
            {
                // ...for all tasks...
                foreach (Task task in tasks)
                {
                    // ...for all time....
                    Access newAccess = new Access(asset, task);
                    bool existingAccess = false;
                    for (double accessTime = startTime; accessTime <= endTime; accessTime += stepTIme)//SchedParameters.SimStepSeconds)
                    {
                        // create a new access, or extend the access endTime if this is an update to an existing access
                        bool hasAccess = Utilities.GeometryUtilities.hasLOS(asset.AssetDynamicState.PositionECI(accessTime), task.Target.DynamicState.PositionECI(accessTime));
                        if (hasAccess)
                        {
                            if (!existingAccess)
                            {
                                newAccess.AccessStart = accessTime;
                                existingAccess = true;
                                accessesByAsset.Add(newAccess);
                            }
                            else
                                newAccess.AccessEnd = accessTime;
                            //bool isNewAccess;
                            //if (accessesByAsset.Count == 0 || accessTime == SimParameters.SimStartSeconds || accessesByAsset[0].Task.Target.Name != task.Target.Name)
                            //    isNewAccess = true;
                            //else
                            //    isNewAccess = (accessTime - accessesByAsset[0].AccessEnd) > SchedParameters.SimStepSeconds;
                            //if (isNewAccess)
                            //{
                            //    Access newAccess = new Access(asset, task);
                            //    newAccess.AccessStart = accessTime;
                            //    newAccess.AccessEnd = accessTime;
                            //    accessesByAsset.Add(newAccess);
                            //    //accessesByAsset.Push(newAccess);
                            //}
                            //else  // extend the access
                            //    accessesByAsset[0].AccessEnd = accessTime;
                            //    //accessesByAsset.Peek().AccessEnd = accessTime;
                        }
                    }
                    
                }
            }
            return accessesByAsset;
        }
        /// <summary>
        /// Override of the too string method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Asset.Name + "_to_" + Task.Target.Name;
        }

        /// <summary>
        /// Method to write out times targets are available
        /// </summary>
        /// <param name="pregeneratedAccesses"></param>
        public static void writeAccessReport(Stack<Access> pregeneratedAccesses)
        {
            string outputDir = SimParameters.OutputDirector;
            string filename = "HorizonLog\\AccessReport.csv";
            string fullFilename = outputDir + "\\" + filename;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fullFilename))
            {
                foreach (var access in pregeneratedAccesses)
                    //foreach (var access in accessByAsset)
                        file.WriteLine(access.Asset.Name + ',' + access.Task.Target.Name + ',' + access.AccessStart + ',' + access.AccessEnd);
            }
        }
        #endregion
    }
}
