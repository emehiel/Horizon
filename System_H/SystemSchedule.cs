using HSFScheduler;
using System.Collections.Generic;
using Utilities;

namespace HSFSystem
{
    public class SystemSchedule
    {
        public Stack<AssetSchedule> AssetSched;
        public double ScheduleValue;

        public SystemSchedule(List<State> initialstates) 
        {
            ScheduleValue = 0;
            foreach(State stIt in initialstates)
            {
                AssetSched.Push(new AssetSchedule(stIt));
            }
        }

        public SystemSchedule(SystemSchedule oldSchedule, List<Task> newTaskList, double newTaskStartTime)
        {
            int i = 0;
            foreach(AssetSchedule assSchedIt in oldSchedule.AssetSched)
            {
                Task tIt = newTaskList[i];
                if (tIt == null)
                {
                    //TODO:
                   // shared_ptr<Event> eventToAdd(new Event(*tIt, new State((*assSchedIt)->getLastState(), newTaskStartTime)));
                   // assetscheds.push_back(new assetSchedule(*assSchedIt, eventToAdd));
                }
                else
                    AssetSched.Push(DeepCopy.Copy<AssetSchedule>(assSchedIt));
                i++;
            }
            
        }

        public bool canAddTasks(Stack<Task> newTaskList, double newTaskStartTime)
        {
            size_t count = 0;
                vector<assetSchedule*>::iterator asIt2 = assetscheds.begin();
	        for(vector<const Task*>::const_iterator tIt = newTaskList.begin(); tIt != newTaskList.end(); tIt++, asIt2++) {
		        if(*tIt != NULL) {
			        for(vector<assetSchedule*>::iterator asIt = assetscheds.begin(); asIt != assetscheds.end(); asIt++) {
				        count += (*asIt)->timesCompletedTask(*tIt);
            }
			        if(count >= (* tIt)->getMaxTimesPerformable())
				        return false;
			        if(!(* asIt2)->empty()) {
				        if((* asIt2)->getLastState()->getEventEnd() > newTaskStartTime)
					        return false;
			        }
		        }
	    }
	    return true;
        }

        const size_t systemSchedule::getTotalNumEvents()
        {
            size_t count = 0;
            for (vector<assetSchedule*>::iterator asIt = assetscheds.begin(); asIt != assetscheds.end(); asIt++)
                count += (*asIt)->size();
            return count;
        }

        State* systemSchedule::getSubNewState(size_t assetNum)
        {
            if (assetscheds[assetNum]->empty())
                return assetscheds[assetNum]->getInitialState();
            else
                return assetscheds[assetNum]->getLastState();
        }

        const Task* systemSchedule::getSubNewTask(size_t assetNum)
        {
            if (assetscheds[assetNum]->empty())
                return NULL;
            else
                return assetscheds[assetNum]->getLastTask();
        }

        const double systemSchedule::getLastTaskStart() const {
	        double lasttime = 0;
	        for(vector<assetSchedule*>::const_iterator aIt = assetscheds.begin(); aIt != assetscheds.end(); aIt++)
		        if(!(* aIt)->empty())
			        lasttime = lasttime > (* aIt)->getLastState()->getTaskStart() ? lasttime : (* aIt)->getLastState()->getTaskStart();
	        return lasttime;
        }

        const vector<State*> systemSchedule::getEndStates() const {
	        vector<State*> endStates;
	        for(vector<assetSchedule*>::const_iterator assSchedIt = assetscheds.begin(); assSchedIt != assetscheds.end(); assSchedIt++)
		        endStates.push_back((* assSchedIt)->getLastState());
	        return endStates;
        }

        void systemSchedule::setScheduleValue(double value)
        {
            scheduleValue = value;
        }

        bool schedGreater(systemSchedule* elem1, systemSchedule* elem2)
        {
            return elem1->getScheduleValue() > elem2->getScheduleValue();
        }

    }
}