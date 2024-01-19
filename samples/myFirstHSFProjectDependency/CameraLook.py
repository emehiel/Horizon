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
from System.Collections.Generic import Dictionary
from IronPython.Compiler import CallTarget0

class CameraLook(HSFSystem.Subsystem):
         

    def CanPerform(self, event, universe):
        ts = event.GetTaskStart(self.Asset)
      
        position = self.Asset.AssetDynamicState
        scPositionECI = position.PositionECI(ts)
        targetPositionECI = event.GetAssetTask(self.Asset).Target.DynamicState.PositionECI(ts)
        pointingVectorECI = targetPositionECI - scPositionECI
        
        scPos_norm = -scPositionECI / Matrix[System.Double].Norm(-scPositionECI)
        pv_norm = pointingVectorECI / Matrix[System.Double].Norm(pointingVectorECI)
        lookAngle = Math.Acos(Matrix[System.Double].Dot(scPos_norm, pv_norm))
        
        event.State.AddValue(self.LookAngle, ts, lookAngle)
        event.State.AddValue(self.POINTVEC_KEY, ts, pointingVectorECI)
        
        event.SetTaskStart(self.Asset, ts)
        event.SetTaskEnd(self.Asset, ts + self.imageCaptureTime)
      
        return True
    
    def CanExtend(self, event, universe, extendTo):
        return super(CameraLook, self).CanExtend(event, universe, extendTo)
      
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
  
    def DependencyCollector(self, currentEvent):
        return super(CameraLook, self).DependencyCollector(currentEvent)
    
    def DataBuffer_asset1_from_CameraLook_asset1(self, event):
        return event.State.GetProfile(self.PIXELS_KEY) / 500

    def DepFinder(self, depFnName):  # Search for method from string input
        fnc = getattr(self, depFnName)
        dep = Dictionary[str, Delegate]()
        depFnToAdd = Func[Event, Utilities.HSFProfile[System.Double]](fnc)
        dep.Add(depFnName, depFnToAdd)
        return dep