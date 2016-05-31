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
from System import Func, Delegate, Math
from System.Collections.Generic import Dictionary, KeyValuePair
from IronPython.Compiler import CallTarget0

class eosensor(HSFSubsystem.EOSensor):
    def __init__(self, node, asset):
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        depFunc1 = Func[Event,  Utilities.HSFProfile[System.Double]](self.POWERSUB_PowerProfile_EOSENSORSUB)
        dep.Add("PowerfromEOSensor", depFunc1)
        depFunc1 = Func[Event,  Utilities.HSFProfile[System.Double]](self.SSDRSUB_NewDataProfile_EOSENSORSUB)
        dep.Add("SSDRfromEOSensor", depFunc1)
        return dep
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def CanPerform(self, event, universe):
         if (self._task.Type == TaskType.IMAGING):
             value = self._task.Target.Value
             pixels = self._lowQualityPixels
             timetocapture = self._lowQualityTime
             if (value <= self._highQualityTime and value >= self._midQualityTime):
                    pixels = self._midQualityPixels
                    timetocapture = self._midQualityTime
             if (value > self._highQualityTime):
                pixels = self._highQualityPixels;
                timetocapture = self._highQualityTime

             es = event.GetEventStart(self.Asset)
             ts = event.GetTaskStart(self.Asset)
             te = event.GetTaskEnd(self.Asset)
             if (ts > te):
                # Logger.Report("EOSensor lost access")
                 return False
             te = ts + timetocapture
             event.SetTaskEnd(self.Asset, te)

             position = self.Asset.AssetDynamicState
             timage = ts + timetocapture / 2
             m_SC_pos_at_tf_ECI = position.PositionECI(timage)
             m_target_pos_at_tf_ECI = self._task.Target.DynamicState.PositionECI(timage)
             m_pv = m_target_pos_at_tf_ECI - m_SC_pos_at_tf_ECI
             pos_norm = -m_SC_pos_at_tf_ECI / Matrix[System.Double].Norm(-m_SC_pos_at_tf_ECI)
             pv_norm = m_pv / Matrix[System.Double].Norm(m_pv)

             incidenceang = 90 - 180 / Math.PI * Math.Acos(Matrix[System.Double].Dot(pos_norm, pv_norm))
             self._newState.addValue(self.INCIDENCE_KEY, KeyValuePair[System.Double, System.Double](timage, incidenceang))
             self._newState.addValue(self.INCIDENCE_KEY, KeyValuePair[System.Double, System.Double](timage + 1, 0.0))

             self._newState.addValue(self.PIXELS_KEY, KeyValuePair[System.Double, System.Double](timage, pixels))
             self._newState.addValue(self.PIXELS_KEY, KeyValuePair[System.Double, System.Double](timage + 1, 0.0))

             self._newState.addValue(self.EOON_KEY, KeyValuePair[System.Double, System.Boolean](ts, True))
             self._newState.addValue(self.EOON_KEY, KeyValuePair[System.Double, System.Boolean](te, False))
             return True     
    def CanExtend(self, event, universe, extendTo):
        return super(eosensor, self).CanExtend(self, event, universe, extendTo)
    def POWERSUB_PowerProfile_EOSENSORSUB(self, event):
        return super(eosensor, self).POWERSUB_PowerProfile_EOSENSORSUB(event)
    def SSDRSUB_NewDataProfile_EOSENSORSUB(self, event):
        return super(eosensor, self).SSDRSUB_NewDataProfile_EOSENSORSUB(event)
    def DependencyCollector(self, currentEvent):
        return super(eosensor, self).DependencyCollector(currentEvent)