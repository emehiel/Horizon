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


class tool(HSFSubsystem.Subsystem):
    def __new__(cls, node, asset):
        instance = HSFSubsystem.Subsystem.__new__(cls)
        instance.Asset = asset
        instance.Name = instance.Asset.Name + '.' + node.Attributes['subsystemName'].Value.ToString().ToLower()

        instance.toolType = node.Attributes['toolType'].Value.ToString()       # TODO make this a list!
        instance.servicingTime = float(node.Attributes['servicingTime'].Value) # TODO make this a dictionary where tool type dictates time!

        instance.SERVICING_TIME_KEY = Utilities.StateVarKey[System.Double](instance.Asset.Name + '.' + 'servicing_time')
        instance.addKey(instance.SERVICING_TIME_KEY)

        return instance

    def GetDependencyDictionary(self):
        dep = System.Collections.Generic.Dictionary[str, System.Delegate]()
        return dep

    def GetDependencyCollector(self):
        return System.Func[MissionElements.Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)

    def CanPerform(self, event, universe):
        # shorcut check for empty target
        tgtName = event.GetAssetTask(self.Asset).Target.Name.ToString()
        if (tgtName == 'EmptyTarget'):
            return True

        if (self._task.Type == self.toolType.ToLower()):
            event.State.AddValue(self.SERVICING_TIME_KEY, Utilities.HSFProfile[System.Double](event.GetTaskStart(self.Asset), self.servicingTime))
            # TODO - improve this by using a dictionary, the tool type maps to a task time, not same-same for all tools!
            return True
        else:
            return False

    def CanExtend(self, event, universe, extendTo):
        return True

    def DependencyCollector(self, currentEvent):
        return super(tool, self).DependencyCollector(currentEvent)
