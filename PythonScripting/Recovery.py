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
        self.Asset = asset
        self.DROGUE_DEPLOYED = StateVarKey[System.Boolean](asset.Name + "." + "drogue");
        self.MAIN_DEPLOYED = StateVarKey[System.Boolean](asset.Name + "." + "main");
        self.Asset.AssetDynamicState.IntegratorParameters.Add(self.DROGUE_DEPLOYED, False)
        self.Asset.AssetDynamicState.IntegratorParameters.Add(self.MAIN_DEPLOYED, False)
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        return dep
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def CanPerform(self, event, universe):
        ts = event.GetEventStart(self.Asset)
        #state = self.DependencyCollector(event).LastValue()
        #pos = state[1,MatrixIndex(1,3)]
        #vel = state[1,MatrixIndex(4,6)]
        state = self.Asset.AssetDynamicState
        pos = state.PositionECI(ts)
        vel = state.VelocityECI(ts)
        if (self._task.Type == TaskType.RECOVERY):
            drogueTask = (self._task.Target.Name == "deployDrogue")
            aboveAlt = (pos[1] > 6379638)
            velLow = vel[1] < 0
            mainDeployed = self.Asset.AssetDynamicState.IntegratorParameters.GetValue(self.MAIN_DEPLOYED)
            drogueDeployed = self.Asset.AssetDynamicState.IntegratorParameters.GetValue(self.DROGUE_DEPLOYED)
            if drogueTask and aboveAlt and velLow and not drogueDeployed:
                print "Drogue Parachute Deployed at ", pos[1]-6378633, "m"
                self.Asset.AssetDynamicState.IntegratorParameters.Add(self.DROGUE_DEPLOYED, True)
                return True
            elif self._task.Target.Name == "deployMain" and drogueDeployed and pos[1] < 6378.938 and not mainDeployed:
                print "Main Parachute Deployed"
                self.Asset.AssetDynamicState.IntegratorParameters.Add(self.MAIN_DEPLOYED, True)
                return True
            return False
        return True
        
    def CanExtend(self, event, universe, extendTo):
        return True
        #return super(Recovery, self).CanExtend(event, universe, extendTo)
    def DependencyCollector(self, currentEvent):
        if (self.SubsystemDependencyFunctions.Count == 0):
            Exception
            #MissingMemberException("You may not call the dependency collector in your can perform because you have not specified any dependency functions for " + Name);
        outProf = HSFProfile[Matrix[System.Double]]()
        for dep in self.SubsystemDependencyFunctions:
            if not (dep.Key.Equals("DepCollector")):
                outProf = dep.Value.DynamicInvoke(currentEvent)
        return outProf