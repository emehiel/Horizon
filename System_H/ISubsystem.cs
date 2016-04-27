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
        bool canPerform(SystemState oldState, SystemState newState,
                      Task task, DynamicState position,
                      Universe environment);//Dependencies dep); doesn't need dependecies anymoere
        bool canExtend(SystemState newState, DynamicState position, Universe environment, double evalToTime); // Dependencies dep);

        void CollectDependencyFuncs(Dependencies Deps, List<string> FuncNames);

    //    void DependencyCollector();
    }
}
