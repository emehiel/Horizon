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
    #def __new__(self, node, asset):
    #    print("Initializing Scripted Subsystem ADCS")
    #    return HSFSubsystem.Power.__new__(self, node, asset)
    def __init__(self, node, asset):
        self.DOD_KEY = StateVarKey[System.Double](self.Asset.Name + "." + "depthofdischarge")
        self.POWIN_KEY = StateVarKey[System.Double](self.Asset.Name + "." + "solarpanelpowerin")
        super(power, self).addKey(self.DOD_KEY)
        super(power, self).addKey(self.POWIN_KEY)
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        return dep
    def CanPerform(self, event, universe):
        return super(power, self).canPerform(event, universe)
    def CanExtend(self, event, universe, extendTo):
        return super(power, self).canExtend(self, event, universe, extendTo)
    def GetSolarPanelPower(self, shadow):
        return super(power, self).getSolarPanelPower(shadow)
    def CalcSolarPanelPowerProfile(self, start, end, state, position, universe):
        return super(power, self).CalcSolarPanelPower(start, end, state, position, universe)