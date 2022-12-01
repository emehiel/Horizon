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
    def __init__(self, keychain):
        keychain = keychain # Is this necesssary?
        pass 

    def SelectKey(keychain, keyName):
    for i in range(len(keychain))
        if keychain[i].VariableName == keyName:
            return keychain[i]

    def Evaluate(self, sched):
        sum = 0
        for eit in sched.AllStates.Events:
            for assetTask in eit.Tasks:
                task = assetTask.Value
                asset = assetTask.Key
                sum += task.Target.Value
                if(task.Type == TaskType.COMM):
                    KeyName = asset.Name + ".databufferfillratio"
                    DATABUFFERRATIOKEY = SelectKey(keychain, keyName)
                    startTime = eit.GetTaskStart(asset)
                    endTime = eit.GetTaskEnd(asset)
                    dataBufferRatioStart = eit.State.GetValueAtTime(DATABUFFERRATIOKEY, startTime)
                    dataBufferRatioEnd = eit.State.GetValueAtTime(DATABUFFERRATIOKEY, endTime)
                    sum += ((dataBufferRatioStart - dataBufferRatioEnd)*50)
                    #callKey = "EvalfromSSDR" + "." + assetTask.Key.Name
                    #foo = self.Dependencies.GetDependencyFunc(callKey)
                    #sum += System.Double(foo(eit))
        return sum