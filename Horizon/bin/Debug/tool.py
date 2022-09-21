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

        instance.toolType = node.Attributes['toolType'].Value.ToString()       # TODO make these both lists instead!
        instance.servicingTime = float(node.Attributes['servicingTime'].Value) # TODO make these both lists instead!

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
            return True
            # TODO also set event time here... needs to take time to perform operation!
        else:
            return False

    def CanExtend(self, event, universe, extendTo):
        return super(tool, self).CanExtend(event, universe, extendTo)

    def DependencyCollector(self, currentEvent):
        return super(tool, self).DependencyCollector(currentEvent)