// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using Utilities;
using HSFSystem;
using UserModel;
using MissionElements;
using log4net;
using System.Threading.Tasks;

namespace HSFScheduler
{
    /// <summary>
    /// Creates valid schedules for a system
    /// </summary>
    [Serializable]
    public class Scheduler
    {
        //TODO:  Support monitoring of scheduler progress - Eric Mehiel
        #region Attributes
        private double _startTime;
        private double _stepLength;
        private double _endTime;
        private int _maxNumSchedules;
        private int _numSchedCropTo;
        public SystemSchedule EmptySchedule { get; private set; }

        public double TotalTime { get; }
        public double PregenTime { get; }
        public double SchedTime { get; }
        public double AccumSchedTime { get; }

        public Evaluator ScheduleEvaluator { get; private set; }
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        /// <summary>
        /// Creates a scheduler for the given system and simulation scenario
        /// </summary>
        /// <param name="scheduleEvaluator"></param>
        public Scheduler(Evaluator scheduleEvaluator, SystemSchedule emptySchedule)
        {
            ScheduleEvaluator = scheduleEvaluator;
            EmptySchedule = emptySchedule;
            _startTime = SimParameters.SimStartSeconds;
            _endTime = SimParameters.SimEndSeconds;
            _stepLength = SchedParameters.SimStepSeconds;
            _maxNumSchedules = SchedParameters.MaxNumScheds;
            _numSchedCropTo = SchedParameters.NumSchedCropTo;
        }

        /// <summary>
        /// Generate schedules by adding a new event to the end of existing ones
        /// Create a new system schedule list by adding each of the new Task commands for the Assets onto each of the old schedules
        /// </summary>
        /// <param name="system"></param>
        /// <param name="tasks"></param>
        /// <param name="initialStateList"></param>
        /// <returns></returns>
        public virtual List<SystemSchedule> GenerateSchedules(SystemClass system, List<MissionElements.Task> tasks)
        {
            log.Info("SIMULATING... ");
            // Create empty systemSchedule with initial state set
            List<SystemSchedule> systemSchedules = new List<SystemSchedule>();
            systemSchedules.Add(EmptySchedule);

            // if all asset position types are not dynamic types, can pregenerate accesses for the simulation
            bool canPregenAccess = true;

            foreach (var asset in system.Assets)
            {
                if(asset.AssetDynamicState != null)
                    canPregenAccess &= asset.AssetDynamicState.Type != HSFUniverse.DynamicStateType.DYNAMIC_ECI && asset.AssetDynamicState.Type != HSFUniverse.DynamicStateType.DYNAMIC_LLA && asset.AssetDynamicState.Type != HSFUniverse.DynamicStateType.NULL_STATE;
                else
                    canPregenAccess = false;
            }

            // REMOVED PREGEN ACCESS CODE IN FAVOR OF EVENT GENERATOR INSIDE TIME LOOP

            // TODO: Delete (or never create in the first place) schedules with inconsistent asset tasks (because of asset dependencies) - Not sure what to do with this -EAM-6/27/23


            List<SystemSchedule> potentialSystemSchedules = new List<SystemSchedule>();
            List<SystemSchedule> systemCanPerformList = new List<SystemSchedule>();
            for (double currentTime = _startTime; currentTime < _endTime; currentTime += _stepLength)
            {
                /* THIS IS A TEST OF A PROPOSED EVENT GENERATOR */
                potentialSystemSchedules = ProposedScheduleGenerator(system, tasks, systemSchedules, canPregenAccess, currentTime);

                // Check each proposed schedule by calling the Checker=->CanPerform() of each Subsystem
                int numSched = 0;
                foreach (var potentialSchedule in potentialSystemSchedules)
                {
                    
                    if (Checker.CheckSchedule(system, potentialSchedule))
                    {
                        systemCanPerformList.Add(potentialSchedule);
                        numSched++;
                    }
                }
                
                // Evaluate Each Schedule
                foreach (SystemSchedule systemSchedule in systemCanPerformList)
                    systemSchedule.ScheduleValue = ScheduleEvaluator.Evaluate(systemSchedule);

                systemCanPerformList.Sort((x, y) => x.ScheduleValue.CompareTo(y.ScheduleValue));
                systemCanPerformList.Reverse();

                // Merge old and new oldSchedules - MAKE SURE THIS CREATING A DEEP COPY?
                var oldSystemCanPerfrom = new List<SystemSchedule>(systemCanPerformList);
                systemSchedules.InsertRange(0, oldSystemCanPerfrom);//<--This was potentialSystemSchedule doubling stuff up
                potentialSystemSchedules.Clear();
                systemCanPerformList.Clear();

                // Print completion percentage in command window
                Console.WriteLine("Scheduler Status: {0:F}% done; {1} schedules generated.", 100 * currentTime / _endTime, systemSchedules.Count);
            }
            return systemSchedules;
        }

        private List<SystemSchedule> ProposedScheduleGenerator(SystemClass system, List<MissionElements.Task> tasks, List<SystemSchedule> oldSchedules, bool canPregenAccess, double currentTime)
        {
            List<List<Access>> accessCombos = new List<List<Access>>();

            if (canPregenAccess)
            {
                // Reshape accesses (by assest) for the engeration of access combinations
                List<Access> Accessess = Access.AccessesByAsset(system, tasks, currentTime, currentTime + _stepLength, 1);//_stepLength);
                // if accesses are pregenerated, look up the access information and update assetTaskList
                accessCombos = GenerateAccesCombinations(Accessess, system, currentTime);
            }
            // generate accessess based on some other algorithm (lines 97-119) - NEED TO TEST THIS APPROACH
            else
            {

                log.Info("Generating Exhaustive Task Combinations for non-geometric case... ");
                Stack<Stack<Access>> exhaustive = new Stack<Stack<Access>>();

                foreach (var asset in system.Assets)
                {
                    Stack<Access> allAccesses = new Stack<Access>(tasks.Count);
                    foreach (var task in tasks)
                        allAccesses.Push(new Access(asset, task));
                    //allAccesses.Push(new Access(asset, null));
                    exhaustive.Push(allAccesses);
                    //allAccesses.Clear();
                }

                IEnumerable<IEnumerable<Access>> allScheduleCombos = exhaustive.CartesianProduct();

                foreach (var accessStack in allScheduleCombos)
                {
                    List<Access> someOfThem = new List<Access>(accessStack);
                    accessCombos.Add(someOfThem);
                }

                log.Info("Done generating exhaustive task combinations");
            }
            // Check if it's necessary to crop the systemSchedule list to a more managable number
            if (oldSchedules.Count > _maxNumSchedules)
            {
                log.Info("Cropping " + oldSchedules.Count + " Schedules.");
                CropSchedules(oldSchedules, ScheduleEvaluator, EmptySchedule);
                oldSchedules.Add(EmptySchedule);
            }

            // Generate an exhaustive list of new possible schedules from the combinations of Old Schedules and AccessCombos(Asset+Tasks)
            //TODO: Parallelize this.
            //Parallel.ForEach(oldSchedules, (oldSystemSchedule) =>
            List<SystemSchedule> potentialSystemSchedules = new List<SystemSchedule>();
            foreach (var oss in oldSchedules)
            {
                //potentialSystemSchedules.Add(new SystemSchedule( new StateHistory(oldSystemSchedule.AllStates)));
                foreach (var cac in accessCombos)
                {
                    if (oss.CanAddTasks(cac, currentTime))
                    {
                        // NOT A DEEP COPY OF THE STATES
                        StateHistory AllStates = new StateHistory(oss.AllStates);
                        // NOT a deep copy of the previous 
                        potentialSystemSchedules.Add(new SystemSchedule(AllStates, cac, currentTime));
                        // oldSched = new SystemSchedule(CopySchedule);
                    }

                }
            }
            // END OF PROPOSED EVENT GENERATOR - RETURN potentialSystemSchedules
            return potentialSystemSchedules;
        }

        /// <summary>
        /// Remove Schedules with the worst scores from the List of SystemSchedules so that there are only _maxNumSchedules.
        /// </summary>
        /// <param name="schedulesToCrop"></param>
        /// <param name="scheduleEvaluator"></param>
        /// <param name="emptySched"></param>
        public void CropSchedules(List<SystemSchedule> schedulesToCrop, Evaluator scheduleEvaluator, SystemSchedule emptySched)
        {
            // Evaluate the schedules and set their values
            foreach (SystemSchedule systemSchedule in schedulesToCrop)
                systemSchedule.ScheduleValue = scheduleEvaluator.Evaluate(systemSchedule);

            // Sort the sysScheds by their values
            schedulesToCrop.Sort((x, y) => x.ScheduleValue.CompareTo(y.ScheduleValue));

            // Delete the sysScheds that don't fit
            int numSched = schedulesToCrop.Count;
            for (int i = 0; i < numSched - _numSchedCropTo; i++)
            {
                schedulesToCrop.Remove(schedulesToCrop[0]);
            }

            //schedulesToCrop.TrimExcess();
        }

        /// <summary>
        /// Return all possible combinations of performing Tasks by Asset at current simulation time
        /// </summary>
        /// <param name="currentAccessForAllAssets"></param>
        /// <param name="system"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public static List<List<Access>> GenerateAccesCombinations(List<Access> currentAccessForAllAssets, SystemClass system, double currentTime)
        {
            // A stack of accesses stacked by asset
            List<List<Access>> currentAccessesByAsset = new List<List<Access>>();
            foreach (Asset asset in system.Assets)
                currentAccessesByAsset.Add(Access.getCurrentAccessesForAsset(currentAccessForAllAssets, asset, currentTime));
                //currentAccessesByAsset.Push(Access.getCurrentAccessesForAsset(currentAccessForAllAssets, asset, currentTime));

            IEnumerable<IEnumerable<Access>> allScheduleCombos = currentAccessesByAsset.CartesianProduct();

            // Use this code to cast allScheduleCombos to Stack<Stack<Access>>.  Explicit cast does not seem to work...
            List<List<Access>> allOfThem = new List<List<Access>>();
            foreach (var accessStack in allScheduleCombos)
            {
                List<Access> someOfThem = new List<Access>(accessStack);
                allOfThem.Add(someOfThem);
            }
            return allOfThem;
        }
    }
}

