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
        public Scheduler(Evaluator scheduleEvaluator)
        {
            ScheduleEvaluator = scheduleEvaluator;
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
        public virtual List<SystemSchedule> GenerateSchedules(SystemClass system, Stack<MissionElements.Task> tasks, SystemState initialStateList)
        {
            log.Info("SIMULATING... ");
            // Create empty systemSchedule with initial state set
            SystemSchedule emptySchedule = new SystemSchedule(initialStateList);
            List<SystemSchedule> systemSchedules = new List<SystemSchedule>();
            systemSchedules.Add(emptySchedule);

            // if all asset position types are not dynamic types, can pregenerate accesses for the simulation
            bool canPregenAccess = true;

            foreach (var asset in system.Assets)
            {
                if(asset.AssetDynamicState != null)
                    canPregenAccess &= asset.AssetDynamicState.Type != HSFUniverse.DynamicStateType.DYNAMIC_ECI && asset.AssetDynamicState.Type != HSFUniverse.DynamicStateType.DYNAMIC_LLA;
                else
                    canPregenAccess = false;
            }

            // if accesses can be pregenerated, do it now
            Stack<Access> preGeneratedAccesses = new Stack<Access>();
            Stack<Stack<Access>> scheduleCombos = new Stack<Stack<Access>>();

            if (canPregenAccess)
            {
                log.Info("Pregenerating Accesses...");
                //DWORD startPregenTickCount = GetTickCount();

                preGeneratedAccesses = Access.pregenerateAccessesByAsset(system, tasks, _startTime, _endTime, _stepLength);
                //DWORD endPregenTickCount = GetTickCount();
                //pregenTimeMs = endPregenTickCount - startPregenTickCount;
                Access.writeAccessReport(preGeneratedAccesses); //- TODO:  Finish this code - EAM
                log.Info("Done pregenerating accesses. There are " + preGeneratedAccesses.Count + " accesses.");
            }
            // otherwise generate an exhaustive list of possibilities for assetTaskList
            else
            {
                log.Info("Generating Exhaustive Task Combinations... ");
                Stack<Stack<Access>> exhaustive = new Stack<Stack<Access>>();
                Stack<Access> allAccesses = new Stack<Access>(tasks.Count);

                foreach (var asset in system.Assets)
                {
                    foreach (var task in tasks)
                        allAccesses.Push(new Access(asset, task));
                    allAccesses.Push(new Access(asset, null));
                    exhaustive.Push(allAccesses);
                    allAccesses.Clear();
                }

                scheduleCombos = (Stack<Stack<Access>>)exhaustive.CartesianProduct();
                log.Info("Done generating exhaustive task combinations");
            }

            /// TODO: Delete (or never create in the first place) schedules with inconsistent asset tasks (because of asset dependencies)

            // Find the next timestep for the simulation
            //DWORD startSchedTickCount = GetTickCount();
            // int i = 1;
            List<SystemSchedule> potentialSystemSchedules = new List<SystemSchedule>();
            List<SystemSchedule> systemCanPerformList = new List<SystemSchedule>();
            for (double currentTime = _startTime; currentTime < _endTime; currentTime += _stepLength)
            {
                log.Info("Simulation Time " + currentTime);
                // if accesses are pregenerated, look up the access information and update assetTaskList
                if (canPregenAccess)
                    scheduleCombos = GenerateExhaustiveSystemSchedules(preGeneratedAccesses, system, currentTime);

                // Check if it's necessary to crop the systemSchedule list to a more managable number
                if (systemSchedules.Count > _maxNumSchedules)
                {
                    log.Info("Cropping " + systemSchedules.Count + " Schedules.");
                    CropSchedules(systemSchedules, ScheduleEvaluator, emptySchedule);
                    systemSchedules.Add(emptySchedule);
                }

                // Generate an exhaustive list of new tasks possible from the combinations of Assets and Tasks
                //TODO: Parallelize this.
                int k = 0;

                //Parallel.ForEach(systemSchedules, (oldSystemSchedule) =>
                foreach(var oldSystemSchedule in systemSchedules)
                {
                    potentialSystemSchedules.Add(new SystemSchedule( new StateHistory(oldSystemSchedule.AllStates)));
                    foreach (var newAccessStack in scheduleCombos)
                    {
                        k++;
                        if (oldSystemSchedule.CanAddTasks(newAccessStack, currentTime))
                        {
                            var CopySchedule = new StateHistory(oldSystemSchedule.AllStates);
                            potentialSystemSchedules.Add(new SystemSchedule(CopySchedule, newAccessStack, currentTime));
                            // oldSched = new SystemSchedule(CopySchedule);
                        }

                    }
                }

                int numSched = 0;
                foreach (var potentialSchedule in potentialSystemSchedules)
                {


                    if (Checker.CheckSchedule(system, potentialSchedule))
                        systemCanPerformList.Add(potentialSchedule);
                    numSched++;
                }
                foreach (SystemSchedule systemSchedule in systemCanPerformList)
                    systemSchedule.ScheduleValue = ScheduleEvaluator.Evaluate(systemSchedule);

                systemCanPerformList.Sort((x, y) => x.ScheduleValue.CompareTo(y.ScheduleValue));
                systemCanPerformList.Reverse();
                // Merge old and new systemSchedules
                var oldSystemCanPerfrom = new List<SystemSchedule>(systemCanPerformList);
                systemSchedules.InsertRange(0, oldSystemCanPerfrom);//<--This was potentialSystemSchedules
                potentialSystemSchedules.Clear();
                systemCanPerformList.Clear();
                // Print completion percentage in command window
                Console.WriteLine("Scheduler Status: {0}% done; {1} schedules generated.", 100 * currentTime / _endTime, systemSchedules.Count);
            }
            return systemSchedules;
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
        public static Stack<Stack<Access>> GenerateExhaustiveSystemSchedules(Stack<Access> currentAccessForAllAssets, SystemClass system, double currentTime)
        {
            // A stack of accesses stacked by asset
            Stack<Stack<Access>> currentAccessesByAsset = new Stack<Stack<Access>>();
            foreach (Asset asset in system.Assets)
                currentAccessesByAsset.Push(Access.getCurrentAccessesForAsset(currentAccessForAllAssets, asset, currentTime));

            IEnumerable<IEnumerable<Access>> allScheduleCombos = currentAccessesByAsset.CartesianProduct();

            Stack<Stack<Access>> allOfThem = new Stack<Stack<Access>>();
            foreach (var accessStack in allScheduleCombos)
            {
                Stack<Access> someOfThem = new Stack<Access>(accessStack);
                allOfThem.Push(someOfThem);
            }

            return allOfThem;
        }
    }
}

