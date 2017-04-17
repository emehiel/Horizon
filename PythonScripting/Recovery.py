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
from HSFSubsystem import *
from HSFSystem import *
from System.Xml import XmlNode
from Utilities import *
from HSFUniverse import *
from UserModel import *
from MissionElements import *
from System import Func, Delegate
from System.Collections.Generic import Dictionary
from IronPython.Compiler import CallTarget0

class Recovery(Subsystem):
    def __init__(self, node, asset):
        self.drougueDeployed = False
        self.Asset = asset
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        return dep
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def CanPerform(self, event, universe):
        print self._task
        ts = event.GetTaskStart(self.Asset)
        position = self.Asset.AssetDynamicState
        state = position.PositionECI(ts)
        print state[1]
        if (self._task.Type == TaskType.RECOVERY):
            if self._task == "deployMain" and self.drougeDeployed:
                print "Main Parachute Deployed"
                return True
            if self._task == "deployDrogue" and state[1] > 6379 and state[4] < .010:
                print "Drogue Parachute Deployed"
                self.drougueDeployed = True
                return True
            return False
        return True
        
    def CanExtend(self, event, universe, extendTo):
        return super(Recovery, self).CanExtend(self, event, universe, extendTo)
    def DependencyCollector(self, currentEvent):
        return super(Recovery, self).DependencyCollector(currentEvent)