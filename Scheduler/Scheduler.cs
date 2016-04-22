using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using HSFSystem;
using UserModel;
using MissionElements;

namespace HSFScheduler
{
    /// <summary>
    /// Creates valid schedules for a system
    /// @author Cory O'Connor
    /// @author Einar Pehrson
    /// @author Eric Mehiel
    /// </summary>
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

        /// <summary>
        /// Creates a scheduler for the given system and simulation scenario
        /// </summary>
        public Scheduler()
        {
            _startTime = SimParameters.SimStartSeconds;
            _endTime = SimParameters.SimEndSeconds;
            _stepLength = SchedParameters.SimStepSeconds;
            _maxNumSchedules = SchedParameters.MaxNumScheds;
            _numSchedCropTo = SchedParameters.NumSchedCropTo;
        }

        public virtual List<SystemSchedule> generateSchedules(SystemClass system, Stack<MissionElements.Task> tasks, List<SystemState> initialStateList, ScheduleEvaluator scheduleEvaluator)
        {

            //system.setThreadNum(1);
            //DWORD startTickCount = GetTickCount();
            //accumSchedTimeMs = 0;

            // get the global dependencies object
            Dependencies dependencies = new Dependencies().Instance();

            Console.WriteLine("SIMULATING... ");
            // Create empty systemSchedule with initial state set
            SystemSchedule emptySchedule = new SystemSchedule(initialStateList);
            List<SystemSchedule> systemSchedules = new List<SystemSchedule>();
            systemSchedules.Add(emptySchedule);

            // if all asset position types are not dynamic types, can pregenerate accesses for the simulation
            bool canPregenAccess = true;
            foreach (Asset asset in system.Assets)
                canPregenAccess &= asset.AssetDynamicState.Type != HSFUniverse.DynamicStateType.DYNAMIC_ECI && asset.AssetDynamicState.Type != HSFUniverse.DynamicStateType.DYNAMIC_LLA;


            // if accesses can be pregenerated, do it now
            Stack<Access> preGeneratedAccesses = new Stack<Access>();

            if (canPregenAccess)
            {
                Console.Write("Pregenerating Accesses...");
                //DWORD startPregenTickCount = GetTickCount();

                preGeneratedAccesses = Access.pregenerateAccessesByAsset(system, tasks, _startTime, _endTime, _stepLength);
                //DWORD endPregenTickCount = GetTickCount();
                //pregenTimeMs = endPregenTickCount - startPregenTickCount;
                //writeAccessReport(access_pregen, tasks); - TODO:  Finish this code - EAM
                Console.WriteLine(" DONE!");
            }
            // otherwise generate an exhaustive list of possibilities for assetTaskList
            else {
                Console.Write("Generating Exhaustive Task Combinations... ");
                //vector < vector <const Task*> > exhaustive(system.getAssets().size(), tasks);
                //assetTaskList = genExhaustiveSystemSchedules(exhaustive);
                Console.WriteLine(" DONE!");
            }

            /// \todo TODO: Delete (or never create in the first place) schedules with inconsistent asset tasks (because of asset dependencies)

            // Find the next timestep for the simulation
            //DWORD startSchedTickCount = GetTickCount();
            Stack<Access> currentAccesses = new Stack<Access>();
            Stack<Stack<Access>> scheduleCombos = new Stack<Stack<Access>>();

            for (double currentTime = _startTime; currentTime < _endTime; currentTime += _stepLength)
            {
                // if accesses are pregenerated, look up the access information and update assetTaskList
                if (canPregenAccess)
                    scheduleCombos = generateExhaustiveSystemSchedules(preGeneratedAccesses, system, currentTime);

                // Check if it's necessary to crop the systemSchedule list to a more managable number
                if (systemSchedules.Count > _maxNumSchedules)
                    cropSchedules(systemSchedules, scheduleEvaluator, emptySchedule);

                // Create a new system schedule list by adding each of the new Task commands for the Assets onto each of the old schedules
                List<SystemSchedule> newSystemSchedules = new List<SystemSchedule>();

                foreach (var oldSystemSchedule in systemSchedules)
                    foreach (var newAccessStack in scheduleCombos)
                        if (oldSystemSchedule.canAddTasks(newAccessStack, currentTime))
                            newSystemSchedules.Add(new SystemSchedule(oldSystemSchedule, newAccessStack, currentTime));

                // Start timing
                // Generate an exhaustive list of new tasks possible from the combinations of Assets and Tasks
                //TODO: Parallelize this.
                Stack<bool> systemCanPerformList = new Stack<bool>();
                //for (list<systemSchedule*>::iterator newSchedIt = newSysScheds.begin(); newSchedIt != newSysScheds.end(); newSchedIt++)
                foreach(var newSchedule in newSystemSchedules)
                {
                    dependencies.updateStates(newSchedule.getEndStates());
                    systemCanPerformList.Push(system.canPerform(newSchedule));
                }

                // End timing


                // Merge old and new systemSchedules
                systemSchedules.InsertRange(0, newSystemSchedules);

                // Print completion percentage in command window
                Console.Write("Scheduler Status: {0} done; {1} schedules generated.\r", 100 * currentTime / _endTime, systemSchedules.Count);
            }


            if (systemSchedules.Count > _maxNumSchedules)
                cropSchedules(systemSchedules, scheduleEvaluator, emptySchedule);

            // extend all schedules to the end of the simulation
            foreach(var schedule in systemSchedules)
            {
                dependencies.updateStates(schedule.getEndStates());
                bool canExtendUntilEnd = true;
                // Iterate through Subsystem Nodes and set that they havent run
                foreach (var subsystemNode in system.SubsystemNodes)
                    subsystemNode.reset();

                int subAssetNum;
                foreach(var subsystemNode in system.SubsystemNodes)
                {
                    subAssetNum = subsystemNode.NAsset;
                    canExtendUntilEnd &= subsystemNode.canPerform(schedule.getSubNewState(subAssetNum), schedule.getSubNewTask(subAssetNum), system.Environment, _endTime, true);
                }

                // Iterate through constraints
                foreach (var constraint in system.Constraints)
                {
                    canExtendUntilEnd &= constraint.accepts(schedule)
                }
                //                for (vector <const Constraint*>::const_iterator constraintIt = system.getConstraints().begin(); constraintIt != system.getConstraints().end(); constraintIt++)
                //            canExtendUntilEnd &= (*constraintIt)->accepts(*schedIt);
                if (!canExtendUntilEnd) {
                    //delete *schedIt;
                    Console.WriteLine("Schedule may not be valid");
                }
            }

            //DWORD endSchedTickCount = GetTickCount();
            //schedTimeMs = endSchedTickCount - startSchedTickCount;

            //DWORD endTickCount = GetTickCount();
            //totalTimeMs = endTickCount - startTickCount;

            return systemSchedules;
        }


        void cropSchedules(List<SystemSchedule> schedulesToCrop, ScheduleEvaluator scheduleEvaluator, SystemSchedule emptySched)
        {
            // Evaluate the schedules and set their values
            foreach(SystemSchedule systemSchedule in schedulesToCrop)
                systemSchedule.ScheduleValue = scheduleEvaluator.evaluate(systemSchedule);

            // Sort the sysScheds by their values
            schedulesToCrop.Sort((x,y) => x.ScheduleValue.CompareTo(y.ScheduleValue));
            // Delete the sysScheds that don't fit
            int i = 1;
            foreach (SystemSchedule systemSchedule in schedulesToCrop)
            {
                if (i > _maxNumSchedules && systemSchedule != emptySched)
                    schedulesToCrop.Remove(systemSchedule);
                i++;
            }
        }
    /*
	void writeAccessReport(vector<vector<map<double, bool>>>& access_pregen, vector<const Task*>& tasks);
    */

        // Return all possible combinations of performing Tasks by Asset at current simulation time
        public static Stack<Stack<Access>> generateExhaustiveSystemSchedules(Stack<Access> currentAccessForAllAssets, SystemClass system, double currentTime)
        {
            // A stack of accesses stacked by asset
            Stack<Stack<Access>> currentAccessesByAsset = new Stack<Stack<Access>>();
            foreach (Asset asset in system.Assets)
                //public static Stack<Access> getCurrentAccessesForAsset(Stack<Access> accesses, Asset asset, double currentTime)
                currentAccessesByAsset.Push(new Stack<Access>(currentAccessForAllAssets.Where(item => item.Asset == asset)));

            IEnumerable<IEnumerable<Access>> allScheduleCombos = currentAccessesByAsset.CartesianProduct();

            return (Stack<Stack<Access>>)allScheduleCombos;
        }
    }
}

