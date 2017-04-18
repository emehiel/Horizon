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
        self.mainDeployed = False
        self.Asset = asset
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        return dep
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def CanPerform(self, event, universe):
        #print self._task
        ts = event.GetEventStart(self.Asset)
        position = self.Asset.AssetDynamicState
        pos = position.PositionECI(ts)
        vel = position.VelocityECI(ts)
        #print state[1]
        if (self._task.Type == TaskType.RECOVERY):
            drogueTask = (self._task.Target.Name == "deployDrogue")
            aboveAlt = (pos[1] > 6379)
            velLow = vel[1] < 0
            #print self._task, drogueTask, aboveAlt, velLow, self.drougueDeployed, pos[1], vel[1]
            if drogueTask and aboveAlt and velLow and not self.drougueDeployed:
                print "Drogue Parachute Deployed"
                self.drougueDeployed = True
                return True
            if self._task.Target.Name == "deployMain" and self.drougueDeployed and pos[1] < 6379 and not self.mainDeployed:
                print "Main Parachute Deployed"
                self.mainDeployed = True
                return True

            return False
        return True
        
    def CanExtend(self, event, universe, extendTo):
        return True
        #return super(Recovery, self).CanExtend(event, universe, extendTo)
    def DependencyCollector(self, currentEvent):
        return super(Recovery, self).DependencyCollector(currentEvent)