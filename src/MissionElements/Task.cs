// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Xml;
using log4net;

namespace MissionElements
{
    /// <summary>
    /// An action to be performed at a target, with limitations and suggestions for scheduling.
    /// </summary>
    [Serializable]
    public class Task
    {
        // the type of task being performed (will always be converted to a lowercase string in the constructor
         public string Type { get; private set; }

        // the target associated with the task 
        public Target Target { get; private set; }

        // The maximum number of times the task should be performed by the ENTIRE SYSTEM (all assets count towards this)
        public int MaxTimesToPerform { get; private set; }

        // Logger for log file
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Constructor that creates a new task to be performed at the given target, with the given scheduling limitations
        /// </summary>
        /// <param name="type"></param>
        /// <param name="target"></param>
        /// <param name="maxTimesToPerform"></param>
        public Task(string type, Target target, int maxTimesToPerform)
        {
            Type = type.ToLower();
            Target = target;
            MaxTimesToPerform = maxTimesToPerform;
        }

        /// <summary>
        /// Load Targets into a list to be passed to scheduler
        /// </summary>
        /// <param name="targetDeckXMLNode"></param>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static bool loadTargetsIntoTaskList(XmlNode targetDeckXMLNode, Stack<Task> tasks)
        {
            if (targetDeckXMLNode == null)
                return false;
            log.Info("Loading target deck...");
            int maxTimesPerform = 1;
            bool allLoaded = true;
            string taskType;
            foreach (XmlNode targetNode in targetDeckXMLNode.ChildNodes)
            {
                if (targetNode.Attributes["TaskType"] != null)
                    taskType = targetNode.Attributes["TaskType"].Value.ToString();
                else {
                    log.Fatal("Missing Task Type");
                    return false;
                }
                //var taskTypeEnum = (TaskType)Enum.Parse(typeof(TaskType), taskType);
                if (targetNode.Attributes["MaxTimes"] != null)
                {
                    Int32.TryParse(targetNode.Attributes["MaxTimes"].Value.ToString(), out maxTimesPerform);
                    tasks.Push(new Task(taskType, new Target(targetNode), maxTimesPerform));
                }
                else
                    tasks.Push(new Task(taskType, new Target(targetNode), maxTimesPerform));
            }
            log.Info("Number of Targets Loaded: "+ tasks.Count);

            return allLoaded;
        }
        #region Overrides
        /// <summary>
        /// Override of the ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Target.Name;
        }
        #endregion
    }

    // This was changed to a string type instead of an enum to support custom taskTypes on 1/31/2022
    // The three types of tasks supported by Horizon
    //public enum TaskType { EMPTY, COMM, IMAGING, FLYALONG, RECOVERY }

  
}
