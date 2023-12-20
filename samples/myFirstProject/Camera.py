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
from System import Func, Delegate
from System.Collections.Generic import Dictionary
from IronPython.Compiler import CallTarget0

class Camera(HSFSystem.Subsystem):
  def __new__(cls, node, asset):
    instance = HSFSystem.Subsystem.__new__(cls)
    instance.Asset = asset
    instance.Name = instance.Asset.Name + '.' + node.Attributes['subsystemName'].Value.ToString().ToLower()
    
    instance.POINTVEC_KEY = Utilities.StateVariableKey[Utilities.Matrix[System.Double]](instance.Asset.Name + '.' + 'eci_pointing_vector(xyz)')
    instance.addKey(instance.POINTVEC_KEY)
    
    return instance
    
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        return dep
      
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
      
    def CanPerform(self, event, universe):
      ts = event.GetTaskStart(self.Asset)
      
      position = self.Asset.AssetDynamicState
      scPositionECI = position.PositionECI(ts)
      targetPositionECI = event.GetAssetTask(self.Asset).Target.DynamicState.PositionECI(ts)
      pointingVectorECI = targetPositionECI - scPositionECI
      
      event.State.SetProfile(self.POINTVEC_KEY, HSFProfile[Matrix[System.Double]](ts, pointingVectorECI))
      event.SetTaskStart(self.Asset, ts)
      
      return True
    
    def CanExtend(self, event, universe, extendTo):
        return super(GenericSubsystem, self).CanExtend(event, universe, extendTo)
      
    def DependencyCollector(self, currentEvent):
        return super(GenericSubsystem, self).DependencyCollector(currentEvent)
