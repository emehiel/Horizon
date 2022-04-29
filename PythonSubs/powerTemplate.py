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

class power(HSFSubsystem.Power):
    def __init__(self, node, asset):
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        return dep
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def CanPerform(self, event, universe):
        return super(power, self).CanPerform(event, universe)
    def CanExtend(self, event, universe, extendTo):
        return super(power, self).CanExtend(self, event, universe, extendTo)
    def GetSolarPanelPower(self, shadow):
        return super(power, self).GetSolarPanelPower(shadow)
    def CalcSolarPanelPowerProfile(self, start, end, state, position, universe):
        return super(power, self).CalcSolarPanelPower(start, end, state, position, universe)
    def DependencyCollector(self, currentEvent):
        return super(power, self).DependencyCollector(currentEvent)