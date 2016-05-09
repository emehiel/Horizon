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
                if (!checkSubs(constraint.Subsystems, proposedSchedule, system.Environment))
                    return false;
                if (!CheckConstraints(system, proposedSchedule, constraint))
                    return false;
                //if (!constraint.accepts(proposedSchedule))
                //    return false;
            }
            // Check the remaining Subsystems that aren't included in any Constraints
            if (!checkSubs(system.Subsystems, proposedSchedule, system.Environment))
                return false;

            return true;
        }

        private static bool checkSubs(List<Subsystem> subsystems, SystemSchedule proposedSchedule, Universe environment)
        {
            foreach (var subsystem in subsystems)
            {
                // new system state of the proposedSchedule (by asset).  Empty to start, fully populated after Checker.CheckSchedule is done
                SystemState newState = proposedSchedule.AllStates.GetLastState();
                // the system state at the end of the last event on the proposedSchedule
                SystemState oldState = newState.Previous;
                // the proposed Task
                Dictionary<Asset, Task> proposedTask = new Dictionary<Asset, Task>();
                proposedTask.Add(subsystem.Asset, proposedSchedule.getSubsytemNewTask(subsystem.Asset));
                // the dynamicState of the proposedSchedule
                DynamicState assetDynamicState = subsystem.Asset.AssetDynamicState;

                // Check all subsystems to see if they canPerform the task
                // Recursion of the subsystem dependencies is managed by the subsystems
                if (!subsystem.canPerform(oldState, newState, proposedTask, environment))
                    return false;


            }
            return true;
        }

        private static bool CheckConstraints(SystemClass system, SystemSchedule proposedSchedule, Constraint constraint)
        {
            // pass the state for checking
            if (!constraint.Accepts(proposedSchedule.AllStates.GetLastState()))
                return false;

            return true;
        }
    }
}
