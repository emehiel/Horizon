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
from System.Collections.Generic import Dictionary
from IronPython.Compiler import CallTarget0

class power(HSFSubsystem.Power):
    def __init__(self, node, asset):
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        return dep
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def CanPerform(self, event, universe):
        es = event.GetEventStart(self.Asset)
        te = event.GetTaskEnd(self.Asset)
        ee = event.GetEventEnd(self.Asset)
        powerSubPowerOut = 10

        if (ee > SimParameters.SimEndSeconds):
            #Logger.Report("Simulation ended")
            return False

        olddod = self._newState.GetLastValue(self.Dkeys[0]).Value
        powerOut = self.DependencyCollector(event)
        powerOut = powerOut + powerSubPowerOut
        position = self.Asset.AssetDynamicState
        powerIn = self.CalcSolarPanelPowerProfile(es, te, self._newState, position, universe)
        dodrateofchange = HSFProfile[System.Double]()
        dodrateofchange = ((powerOut - powerIn) / self._batterySize)
        exceeded= False
        freq = 5.0
        dodProf = HSFProfile[System.Double]()
        dodProf = dodrateofchange.lowerLimitIntegrateToProf(es, te, freq, 0.0, exceeded, 0, olddod)
        self._newState.AddValue(self.DOD_KEY, dodProf[0])
        return True
    def CanExtend(self, event, universe, extendTo):
        return super(power, self).CanExtend(event, universe, extendTo)
    def GetSolarPanelPower(self, shadow):
        return super(power, self).GetSolarPanelPower(shadow)
    def CalcSolarPanelPowerProfile(self, start, end, state, position, universe):
        return super(power, self).CalcSolarPanelPowerProfile(start, end, state, position, universe)
    def DependencyCollector(self, currentEvent):
        return super(power, self).DependencyCollector(currentEvent)