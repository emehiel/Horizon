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

class adcs(HSFSubsystem.ADCS):
    def __init__(self, node, asset):
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        depFunc1 = Func[Event,  Utilities.HSFProfile[System.Double]](self.POWERSUB_PowerProfile_ADCSSUB)
        dep.Add("PowerfromADCS"+ "." + self.Asset.Name, depFunc1)
        return dep
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def POWERSUB_PowerProfile_ADCSSUB(self, event):
        prof1 = HSFProfile[System.Double]()
        prof1[event.GetEventStart(self.Asset)] = 30
        prof1[event.GetTaskStart(self.Asset)] = 60
        prof1[event.GetTaskEnd(self.Asset)] = 30
        return prof1
    def CanPerform(self, event, universe):
        timetoslew = 10
        es = event.GetEventStart(self.Asset)
        ts = event.GetTaskStart(self.Asset)
        te = event.GetTaskEnd(self.Asset)
        if (es + timetoslew > ts):
            if (es + timetoslew > te):
               #     HSFSystem.Logger.Report("ADCS: Not enough time to slew event start: "+ es + "task end" + te)
                    return False
            else:
                    ts = es + timetoslew
        position = self.Asset.AssetDynamicState
        m_SC_pos_at_ts_ECI = position.PositionECI(ts)
        m_target_pos_at_ts_ECI = event.GetAssetTask(self.Asset).Target.DynamicState.PositionECI(ts)
        m_pv = m_target_pos_at_ts_ECI - m_SC_pos_at_ts_ECI

        event.State.setProfile(self.POINTVEC_KEY, HSFProfile[Matrix[System.Double]](ts, m_pv))
        event.SetTaskStart(self.Asset, ts)
        return True
       # return super(adcs, self).canPerform(event, universe)
    def CanExtend(self, event, universe, extendTo):
        return super(adcs, self).CanExtend(event, universe, extendTo)
    def DependencyCollector(self, currentEvent):
        return super(adcs, self).DependencyCollector(currentEvent)
