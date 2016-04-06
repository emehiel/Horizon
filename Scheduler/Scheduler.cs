using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using HSFSystem;

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

        virtual List<SystemSchedule> generateSchedules(SystemClass system, Stack<Task> tasks, Stack<SystemState> initialStateList, Evaluator.Evaluator schedVals)
        {

            //system.setThreadNum(1);
            //DWORD startTickCount = GetTickCount();
            //accumSchedTimeMs = 0;

            // get the global dependencies object
            HSFSubsystem.Dependencies dependencies = HSFSubsystem.Dependencies.Instance();

            Console.WriteLine("SIMULATING... ");
            // Create empty systemSchedule with initial state set
            SystemSchedule emptySchedule = new SystemSchedule(initialStateList);
            List<SystemSchedule> systemSchedules = new List<SystemSchedule>();
            systemSchedules.Add(emptySchedule);

            // if all asset position types are not dynamic types, can pregenerate accesses for the simulation
            bool canPregenAccess = true;
            foreach (Asset asset in system.Assets)
                canPregenAccess &= asset.AssetDynamicState.Type != HSFUniverse.DynamicStateType.DynamicECI && asset.AssetDynamicState.Type != HSFUniverse.DynamicStateType.DynamicLLA;


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
            for (double currenttime = _startTime; currenttime < _endTime; currenttime += _stepLength)
            {
                // if accesses are pregenerated, look up the access information and update assetTaskList
                if (canPregenAccess)
                {
                    currentAccesses = Access.getCurrentAccesses(preGeneratedAccesses, currenttime);
                    scheduleCombos = generateExhaustiveSystemSchedules(assetTasks);
                }

                // Check if it's necessary to crop the systemSchedule list to a more managable number
                if (systemSchedules.Count > _maxNumSchedules)
                    cropSchedules(systemSchedules, schedVals, emptySchedule);

                // Create a new system schedule list by adding each of the new Task commands for the Assets onto each of the old schedules
                List<SystemSchedule> newSysScheds = new List<SystemSchedule>();
                for (list<systemSchedule*>::iterator oldSchedIt = systemSchedules.begin(); oldSchedIt != systemSchedules.end(); oldSchedIt++)
                { 
                    for (vector < vector <const Task*> >::const_iterator newTaskListIt = scheduleCombos.begin(); newTaskListIt != scheduleCombos.end(); newTaskListIt++)
                    {
                        // Checks whether the System is allowed to perform the Task again and the Tasks are scheduled after the previous Events end
                        if ((*oldSchedIt)->canAddTasks(*newTaskListIt, currenttime))
                        {
                            // Creates new systemSchedule based on previous systemSchedule and new Event list
                            newSysScheds.push_back(new systemSchedule(*oldSchedIt, *newTaskListIt, currenttime));
                        }
                    }



            // Start timing
            //DWORD startAccumCount = GetTickCount();

            // Generate an exhaustive list of new tasks possible from the combinations of Assets and Tasks
            /// \todo TODO: Parallelize this.

            Stack<bool> systemCanPerformList;
            for (list<systemSchedule*>::iterator newSchedIt = newSysScheds.begin(); newSchedIt != newSysScheds.end(); newSchedIt++)
            {
                //dependencies->updateStates(1, (*newSchedIt)->getEndStates());
                //dependencies->setThreadState(1);
                dependencies->updateStates((*newSchedIt)->getEndStates());
                systemCanPerformList.push_back(system.canPerform(*newSchedIt));
            }

            // End timing
            //DWORD endAccumCount = GetTickCount();
            //accumSchedTimeMs += endAccumCount - startAccumCount;

            // delete systemSchedules (and corresponding lower level classes) that are not possible
            list<systemSchedule*>::iterator eraseIt = newSysScheds.begin();
            for (vector<bool>::iterator successIt = systemCanPerformList.begin(); successIt != systemCanPerformList.end(); successIt++)
            {
                if (*successIt) { eraseIt++; }
                else
                {
                    delete* eraseIt;
                    eraseIt = newSysScheds.erase(eraseIt);
                }
            }

            // Merge old and new systemSchedules
            sysScheds.insert(sysScheds.begin(), newSysScheds.begin(), newSysScheds.end());

            // Print completion percentage in command window
            printf("Scheduler Status: %4.2f%% done; %i schedules generated.\r", 100 * currenttime / endTime, sysScheds.size());
        }

    cropSchedules(sysScheds, schedVals);

	// extend all schedules to the end of the simulation
	for(List<SystemSchedule*>::iterator schedIt = sysScheds.begin(); schedIt != sysScheds.end(); schedIt++)
	{
		dependencies->updateStates((*schedIt)->getEndStates());
		bool canExtendUntilEnd = true;
		// Iterate through Subsystem Nodes and set that they havent run
		for(vector<SubsystemNode*>::const_iterator subNodeIt = system.getSubsystemNodes().begin(); subNodeIt !=system.getSubsystemNodes().end(); subNodeIt++)
			(* subNodeIt)->reset();

        size_t subAssetNum;
		for(vector<SubsystemNode*>::const_iterator subNodeIt = system.getSubsystemNodes().begin(); subNodeIt != system.getSubsystemNodes().end(); subNodeIt++) {
			subAssetNum = (* subNodeIt)->getAssetNum();
        canExtendUntilEnd &= (** subNodeIt).canPerform((*schedIt)->getSubNewState(subAssetNum), (* schedIt)->getSubNewTask(subAssetNum), system.getEnvironment(), endTime, true);
		}
		// Iterate through constraints
		for(vector<const Constraint*>::const_iterator constraintIt = system.getConstraints().begin(); constraintIt != system.getConstraints().end(); constraintIt++)
			canExtendUntilEnd &= (* constraintIt)->accepts(*schedIt);

		if(!canExtendUntilEnd) {
			//delete *schedIt;
			cout << "Schedule may not be valid";
		}
	}

	DWORD endSchedTickCount = GetTickCount();
schedTimeMs = endSchedTickCount - startSchedTickCount;

	DWORD endTickCount = GetTickCount();
totalTimeMs = endTickCount - startTickCount;

	return sysScheds;
        }

        /*
        void cropSchedules(list<systemSchedule*>& schedsToCrop, const ScheduleEvaluator* schedVals, systemSchedule* emptySched = 0);

	void writeAccessReport(vector<vector<map<double, bool>>>& access_pregen, vector<const Task*>& tasks);
    */

        // Return all possible combinations of performing Tasks by Asset at current simulation time
        public static Stack<Stack<Access>> generateExhaustiveSystemSchedules(Stack<Access> currentAccess, SystemClass system, double currentTime)
        {
            // A stack of accesses stacked by asset
            Stack<Stack<Access>> currentAccessesByAsset = new Stack<Stack<Access>>();
            foreach (Asset asset in system.Assets)
                currentAccessesByAsset.Push(Access.getCurrentAccessesForAsset(currentAccess, asset, currentTime));

            return (Stack<Stack<Access>>)currentAccessesByAsset.CartesianProduct();

        }
    }
}

