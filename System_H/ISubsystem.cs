using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MissionElements;
using HSFUniverse;
using Utilities;

namespace HSFSystem
{
    public interface ISubsystem
    {
        bool canPerform(List<SystemState> oldState, List<SystemState> newState,
                      Task task, DynamicState position,
                      Universe environment);
        bool canExtend(SystemState newState, DynamicState position, Universe environment, double evalToTime); 

        void CollectDependencyFuncs(Dependencies Deps, List<string> FuncNames);

    //    void DependencyCollector();
    }
}
