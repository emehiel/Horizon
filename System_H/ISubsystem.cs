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
        bool canPerform(Event proposedEvent, Universe environment);
        bool canExtend(Event proposedEvent, Universe environment, double evalToTime); 

        void CollectDependencyFuncs(Dependencies Deps, List<string> FuncNames);

    //    void DependencyCollector();
    }
}
