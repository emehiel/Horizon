import sys
import clr
import System.Collections.Generic
import System
clr.AddReference('System.Core')
clr.AddReference('IronPython')
clr.AddReference('System.Xml')
clr.AddReferenceByName('Utilities')
clr.AddReferenceByName('HSFUniverse')
clr.AddReferenceByName('UserModel')
clr.AddReferenceByName('MissionElements')
clr.AddReferenceByName('HSFSystem')

import System.Xml
import HSFSystem
import HSFSubsystem
import MissionElements
import Utilities
import HSFUniverse
import UserModel
from HSFSystem import *
from System.Xml import XmlNode
from Utilities import *
from HSFUniverse import *
from UserModel import *
from MissionElements import *
from System import Func, Delegate
from System.Collections.Generic import *
from IronPython.Compiler import CallTarget0

class ssdr(HSFSubsystem.Subsystem):
    def __new__(cls, node, asset):
        instance = HSFSubsystem.Subsystem.__new__(cls)
        instance.Asset = asset
        instance.Name = instance.Asset.Name + '.' + node.Attributes['subsystemName'].Value.ToString().ToLower()

        if (node.Attributes['bufferSize'] != None):
            instance._bufferSize = float(node.Attributes['bufferSize'].Value.ToString())
        instance.DATABUFFERRATIO_KEY = Utilities.StateVarKey[System.Double](instance.Asset.Name + '.' + 'databufferfillratio')
        instance.addKey(instance.DATABUFFERRATIO_KEY)

        return instance
		
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        depFunc1 = Func[Event,  Utilities.HSFProfile[System.Double]](self.POWERSUB_PowerProfile_SSDRSUB)
        dep.Add("PowerfromSSDR", depFunc1)
        depFunc2 = Func[Event,  Utilities.HSFProfile[System.Double]](self.COMMSUB_DataRateProfile_SSDRSUB)
        dep.Add("CommfromSSDR", depFunc2)
        depFunc3 = Func[Event,  System.Double](self.EVAL_DataRateProfile_SSDRSUB)
        dep.Add("EvalfromSSDR", depFunc3)
        return dep

    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)

    def CanPerform(self, event, universe):
        if (self._task.Type == TaskType.IMAGING):
            ts = event.GetTaskStart(self.Asset)
            te = event.GetTaskEnd(self.Asset)
            oldbufferratio = self._newState.GetLastValue(self.Dkeys[0]).Value
            newdataratein = HSFProfile[System.Double]()
            newdataratein = self.DependencyCollector(event) / self._bufferSize
            exceeded = False
            newdataratio = HSFProfile[System.Double]()
            newdataratio = newdataratein.upperLimitIntegrateToProf(ts, te, 5, 1, exceeded, 0, oldbufferratio)
            if (exceeded == False):
                self._newState.AddValue(self.DATABUFFERRATIO_KEY, newdataratio[0])
                return True
          #  Logger.Report("SSDR")
            return False
        if(self/_task.Type == TaskType.COMM):
             ts = event.GetTaskStart(self.Asset)
             event.SetTaskEnd(self.Asset, ts + 60.0)
             te = event.GetTaskEnd(self.Asset)
             data = self._bufferSize * self._newState.GetLastValue(self.Dkeys[0]).Value
             if( data / 2 > 50):
                 dataqueout = data/2
             else:
                 dataqueout = data
             if (data - dataqueout < 0):
                 dataqueout = data
             if (dataqueout > 0):
                 self._newState.AddValue(self.DATABUFFERRATIO_KEY, KeyValuePair[System.Double, System.Double](te, (data - dataqueout) / _bufferSize))
             return True
        return True

    def CanExtend(self, event, universe, extendTo):
        return super(ssdr, self).CanExtend(event, universe, extendTo)

    def POWERSUB_PowerProfile_SSDRSUB(self, event):
        prof1 = HSFProfile[System.Double]()
        prof1[event.GetEventStart(self.Asset)] = 15
        return prof1

    def COMMSUB_DataRateProfile_SSDRSUB(self, event):
        datarate = 5000 * (event.State.GetValueAtTime(self.DATABUFFERRATIO_KEY, event.GetTaskStart(self.Asset)).Value - event.State.GetValueAtTime(self.DATABUFFERRATIO_KEY, event.GetTaskEnd(self.Asset)).Value) / (event.GetTaskEnd(self.Asset) - event.GetTaskStart(self.Asset))
        prof1 = HSFProfile[System.Double]()
        if (datarate != 0):
            prof1[event.GetTaskStart(self.Asset)] = datarate
            prof1[event.GetTaskEnd(self.Asset)] = 0
        return prof1

    def EVAL_DataRateProfile_SSDRSUB(self, event):
        return (event.State.GetValueAtTime(DATABUFFERRATIO_KEY, event.GetTaskEnd(self.Asset)).Value - event.State.GetValueAtTime(DATABUFFERRATIO_KEY, event.GetTaskEnd(self.Asset)).Value) * 50

    def DependencyCollector(self, currentEvent):
        return super(ssdr, self).DependencyCollector(currentEvent)
