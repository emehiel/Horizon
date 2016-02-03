using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
#include "systemSchedule.h"
#include "horizon/SubsystemNode.h"
#include "horizon/log/Logger.h"

*/
namespace System_H
{
    class Constraint{
        private List<SubsystemNode> _subsystemNodes{get; private set;}
        public Constraint(){}
        public void addConstrainedSubNode(SubsystemNode node){
            subsystemNodes.push_back(node);
        }
        
        virtual bool accepts(systemSchedule sched){}
        
        virtual Constrain clone(){}

    }
}