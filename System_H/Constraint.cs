using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSFSubsystem;

/*
#include "systemSchedule.h"
#include "horizon/SubsystemNode.h"
#include "horizon/log/Logger.h"

*/
namespace HSFSystem
{
    class Constraint{
        public Stack<SubsystemNode> SubsystemNodes{get; private set;}
        public Constraint(){}
        public void addConstrainedSubNode(SubsystemNode node){
            SubsystemNodes.Push(node);
        }

        public virtual bool accepts(SystemSchedule sched);

        public virtual Constraint clone();

    }
}