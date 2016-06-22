// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using HSFSystem;
using UserModel;
using MissionElements;
using log4net;

namespace HSFScheduler
{
    /// <summary>
    /// Creates valid schedules for a system
    /// @author Cory O'Connor
    /// @author Einar Pehrson
    /// @author Eric Mehiel
    /// </summary>
    [Serializable]
    public class Scheduler
    {
        //TODO:  Support monitoring of scheduler progres - Eric Mehiel

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

        /// <summary>
        /// Creates a scheduler for the given system and simulation scenario
        /// </summary>
        public Scheduler(Evaluator scheduleEvaluator)
        {
            ScheduleEvaluator = scheduleEvaluator;
            _startTime = SimParameters.SimStartSeconds;
            _endTime = SimParameters.SimEndSeconds;
            _stepLength = SchedParameters.SimStepSeconds;
            _maxNumSchedules = SchedParameters.MaxNumScheds;
            _numSchedCropTo = SchedParameters.NumSchedCropTo;
        }

        public virtual List<SystemSchedule> GenerateSchedules(SystemClass system, Stack<MissionElements.Task> tasks, SystemState initialStateList)
        {

            //system.setThreadNum(1);
            //DWORD startTickCount = GetTickCount();
            //accumSchedTimeMs = 0;

            // get the global dependencies object
            //Dependencies dependencies = new Dependencies().Instance();

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
                //writeAccessReport(access_pregen, tasks); - TODO:  Finish this code - EAM
                log.Info("Done pregenerating accesses");
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

            /// \todo TODO: Delete (or never create in the first place) schedules with inconsistent asset tasks (because of asset dependencies)

            // Find the next timestep for the simulation
            //DWORD startSchedTickCount = GetTickCount();
            int i = 1;
            List<SystemSchedule> potentialSystemSchedules = new List<SystemSchedule>();
            List<SystemSchedule> systemCanPerformList = new List<SystemSchedule>();
            for (double currentTime = _startTime; currentTime < _endTime; currentTime += _stepLength)
            {
                log.Info("Simulation Time " + currentTime);
                // if accesses are pregenerated, look up the access information and update assetTaskList
                if (canPregenAccess)
                    scheduleCombos = GenerateExhaustiveSystemSchedules(preGeneratedAccesses, system, currentTime);

                // Check if it's necessary to crop the systemSchedule list to a more managable number
                if (systemSchedules.Count > _numSchedCropTo)
                {
                    log.Info("Cropping Schedules...");
                    CropSchedules(systemSchedules, ScheduleEvaluator, emptySchedule);
                    systemSchedules.Add(emptySchedule);
                }

                // Create a new system schedule list by adding each of the new Task commands for the Assets onto each of the old schedules
                // Start timing
                // Generate an exhaustive list of new tasks possible from the combinations of Assets and Tasks
                //TODO: Parallelize this.
                int k = 0;

                foreach (var oldSystemSchedule in systemSchedules)
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
                    //Stack<Access> emptyAccess = new Stack<Access>();
                    //emptyAccess.Push()
                    //potentialSystemSchedules.Add(new SystemSchedule(oldSystemSchedule, emptyEvent));
                    //   potentialSystemSchedules.Add(new SystemSchedule(oldSystemSchedule)); //deep copy

                }

                // TODO EAM: Remove this and only add new SystemScedule if canAddTasks and CanPerform are both true.  That way we don't need to delete SystemSchedules after the fact below.


                //for (list<systemSchedule*>::iterator newSchedIt = newSysScheds.begin(); newSchedIt != newSysScheds.end(); newSchedIt++)
                // The parallel version
                // Should we use a Partitioner?
                // Need to test this...
                /*
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                // The Scheduler has to call the CanPerform for a SystemClass, SystemSchedule combo.  The SystemClass 
                Parallel.ForEach(potentialSystemSchedules, (currentSchedule) =>
                {
                    // dependencies.updateStates(newSchedule.getEndStates());
                    if (Checker.CheckSchedule(system, currentSchedule))
                        systemCanPerformList.Add(currentSchedule);
                    Console.WriteLine("Processing {0} on thread {1}", currentSchedule.ToString(), Thread.CurrentThread.ManagedThreadId);
                });
                stopWatch.Stop();
                
                TimeSpan ts = stopWatch.Elapsed;
                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine("Parallel Scheduler RunTime: " + elapsedTime);
                */
                int numSched = 0;
                foreach (var potentialSchedule in potentialSystemSchedules)
                {


                    if (Checker.CheckSchedule(system, potentialSchedule))
                        systemCanPerformList.Add(potentialSchedule);
                    //else
                    //{
                    //    Console.WriteLine("Can't add sched " + numSched);
                    //}
                    numSched++;
                    //dependencies.updateStates(newSchedule.getEndStates());
                    //systemCanPerformList.Push(system.canPerform(potentialSchedule));
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
            for (int i = 0; i < numSched - _maxNumSchedules; i++)
            {
                schedulesToCrop.Remove(schedulesToCrop[0]);
            }
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

