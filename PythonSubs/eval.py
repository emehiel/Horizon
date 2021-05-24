import sys
import clr
import System.Collections.Generic
import System
clr.AddReference('System.Core')
clr.AddReference('IronPython')
clr.AddReference('System.Xml')
clr.AddReferenceByName('HSFScheduler')
clr.AddReferenceByName('MissionElements')
clr.AddReferenceByName('HSFSystem')

import System.Xml
import HSFSystem
import MissionElements
import HSFScheduler
from HSFSystem import *
from System.Xml import XmlNode
from MissionElements import *
from HSFScheduler import *
from System.Collections.Generic import Dictionary
from IronPython.Compiler import CallTarget0

class eval(HSFScheduler.TargetValueEvaluator):
    def __init__(self, deps):
        pass
    def Evaluate(self, sched):
        sum = 0
        for eit in sched.AllStates.Events:
            for assetTask in eit.Tasks:
                task = assetTask.Value
                sum += task.Target.Value
                if(task.Type == TaskType.COMM):
                    callKey = "EvalfromSSDR" + "." + assetTask.Key.Name
                    foo = self.Dependencies.GetDependencyFunc(callKey)
                    sum += System.Double(foo(eit))
        return sum