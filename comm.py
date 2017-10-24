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

class comm(HSFSubsystem.Subsystem):
    def __new__(cls, node, asset):
        instance = HSFSubsystem.Subsystem.__new__(cls)
        instance.Asset = asset
        instance.Name = instance.Asset.Name + '.' + node.Attributes['subsystemName'].Value.ToString().ToLower()

        instance.DATARATE_KEY = Utilities.StateVarKey[System.Double](instance.Asset.Name + '.' + 'datarate(mb/s)')
        instance.addKey(instance.DATARATE_KEY)

        return instance
		
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        depFunc1 = Func[Event,  Utilities.HSFProfile[System.Double]](self.POWERSUB_PowerProfile_COMMSUB)
        dep.Add("PowerfromComm" + "." + self.Asset.Name, depFunc1)
        return dep

    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)

    def CanPerform(self, event, universe):
        if (self._task.Type == TaskType.COMM):
            newProf = self.DependencyCollector(event)
            if (newProf.Empty() == False):
                event.State.SetProfile(self.DATARATE_KEY, newProf)
        return True

    def CanExtend(self, event, universe, extendTo):
        return super(comm, self).CanExtend(event, universe, extendTo)

    def POWERSUB_PowerProfile_COMMSUB(self, event):
        return event.State.GetProfile(self.DATARATE_KEY) * 20

    def DependencyCollector(self, currentEvent):
        return super(comm, self).DependencyCollector(currentEvent)
