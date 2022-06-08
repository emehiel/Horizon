// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using HSFSystem;
using HSFSubsystem;
using HSFUniverse;
using MissionElements;
using Logging;

namespace HSFScheduler
{
    public static class Checker
    {
        /// <summary>
        /// Determine if the system can execute the proposed schedule
        /// </summary>
        /// <param name="system"></param>
        /// <param name="proposedSchedule"></param>
        /// <returns></returns>
        public static bool CheckSchedule(SystemClass system, SystemSchedule proposedSchedule)
        {
            // Check that any Assets with IsTaskable = false only tasked to EmptyTarget 
            var aaaaa = 0;
            foreach (KeyValuePair<Asset, Task> taskDict in proposedSchedule.AllStates.Events.Peek().Tasks)
            {
                if ((!taskDict.Key.IsTaskable) && (taskDict.Value.Type != "empty"))
                {
                    return false;
                }
            }
            // Iterate through Subsystem Nodes and set that they havent run
            foreach (var subsystem in system.Subsystems)
                subsystem.IsEvaluated = false;

            // Iterate through constraints
            foreach (var constraint in system.Constraints)
            {
                foreach (Subsystem sub in constraint.Subsystems)
                {
                    if (!checkSub(sub, proposedSchedule, system.Environment))
                        return false;
                    if (!CheckConstraints(system, proposedSchedule, constraint))
                        return false;
                    //if (!constraint.accepts(proposedSchedule))
                    //    return false;
                }
            }
            // Check the remaining Subsystems that aren't included in any Constraints
            if (!checkSubs(system.Subsystems, proposedSchedule, system.Environment))
                return false;

            return true;
        }

        /// <summary>
        /// Method to check if the subsystem can perform the most recent task that was added to the schedule
        /// </summary>
        /// <param name="subsystem"></param>
        /// <param name="proposedSchedule"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        private static bool checkSub(Subsystem subsystem, SystemSchedule proposedSchedule, Domain environment)
        {
            if (subsystem.IsEvaluated)
                return true;
            var events = proposedSchedule.AllStates.Events;
            if (events.Count != 0)
            {
                //if (events.Count > 1)
                //    subsystem._oldState = events.ElementAt(events.Count - 2).State;
                //else
                //    subsystem._oldState = null;

                    if (!subsystem.CanPerform(events.Peek(), environment))
                        return false;
                    events.Peek().isEvaluated +=1;

            }
            return true;
        }

        /// <summary>
        /// See if all the subsystems can perform the most recent task that was added to the schedule
        /// </summary>
        /// <param name="subsystems"></param>
        /// <param name="proposedSchedule"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        private static bool checkSubs(List<Subsystem> subsystems, SystemSchedule proposedSchedule, Domain environment)
        {
            foreach (var sub in subsystems)
            {
                if (!sub.IsEvaluated && !checkSub(sub, proposedSchedule, environment))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if the constraint has been violated in progressing the state of the system
        /// </summary>
        /// <param name="system"></param>
        /// <param name="proposedSchedule"></param>
        /// <param name="constraint"></param>
        /// <returns></returns>
        private static bool CheckConstraints(SystemClass system, SystemSchedule proposedSchedule, Constraint constraint)
        {
            if (constraint.Accepts(proposedSchedule.AllStates.GetLastState()))
            {
                return true;
            }
            else
            {
                // TODO: Change this to logger
                // HSFLogger.Log(new HSFLogData(constraint, subsystem, task, value, time));
                Console.WriteLine("Constraint Failed");
                return false;
            }
        }
    }
}
