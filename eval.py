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
        self.keychain = keychain  # Is this necessary?
        print(self.keychain)
        pass 

    # def SelectKey(keychain, keyName):
    #     for i in range(len(keychain)):
    #         if keychain[i].VariableName == keyName:
    #             return keychain[i]

    def Evaluate(self, sched):
        accumulatedSum = 0
        for eit in sched.AllStates.Events:
            for assetTask in eit.Tasks:
                print('Evaluating Asset Task:')
                print(assetTask)
                task = assetTask.Value
                asset = assetTask.Key
                accumulatedSum += task.Target.Value
                if task.Type == "comm":

                    def SelectKey(keychain, keyname):
                        for i in reversed(range(len(keychain))):
                            print('checking key variable name:')
                            print(keychain[i].VariableName)
                            if keychain[i].VariableName == keyname:
                                print('key selected:')
                                print(keychain[i])
                                print('key variable name:')
                                print(keychain[i].VariableName)
                                return keychain[i]

                    keyName = asset.Name + ".databufferfillratio"
                    print(keyName)

                    DATABUFFERRATIOKEY = SelectKey(self.keychain, keyName)
                    print(DATABUFFERRATIOKEY)
                    print(DATABUFFERRATIOKEY.VariableName)

                    startTime = eit.GetTaskStart(asset)
                    print(startTime)

                    endTime = eit.GetTaskEnd(asset)
                    print(endTime)

                    dataBufferRatioStart = eit.State.GetValueAtTime(DATABUFFERRATIOKEY, startTime).Value
                    print(dataBufferRatioStart)

                    dataBufferRatioEnd = eit.State.GetValueAtTime(DATABUFFERRATIOKEY, endTime).Value
                    print(dataBufferRatioEnd)

                    accumulatedSum += ((dataBufferRatioStart - dataBufferRatioEnd) * 50)
                    print(accumulatedSum)
        return accumulatedSum
