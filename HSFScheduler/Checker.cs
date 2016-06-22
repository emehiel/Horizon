// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HSFSystem;
using HSFSubsystem;
using MissionElements;
using HSFUniverse;

namespace HSFScheduler
{
    public static class Checker
    {
        public static bool CheckSchedule(SystemClass system, SystemSchedule proposedSchedule)
        {
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

        private static bool checkSub(Subsystem subsystem, SystemSchedule proposedSchedule, Universe environment)
        {
            //if (subsystem.IsEvaluated)
            //    return true;
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
        private static bool checkSubs(List<Subsystem> subsystems, SystemSchedule proposedSchedule, Universe environment)
        {
            foreach (var sub in subsystems)
            {
                if (!sub.IsEvaluated && !checkSub(sub, proposedSchedule, environment))
                    return false;
            }
            return true;
        }
        private static bool CheckConstraints(SystemClass system, SystemSchedule proposedSchedule, Constraint constraint)
        {
            // pass the state for checking
            if (!constraint.Accepts(proposedSchedule.AllStates.GetLastState()))
            {
                Logger.Report("Constraint Failed: " + constraint.GetType().ToString());
                return false;
            }
            

            return true;
        }
    }
}
