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

class adcs(HSFSubsystem.ADCS):
    def __init__(self, node, asset):
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        depFunc1 = Func[Event,  Utilities.HSFProfile[System.Double]](self.POWERSUB_PowerProfile_ADCSSUB)
        dep.Add("PowerfromADCS", depFunc1)
        return dep
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def POWERSUB_PowerProfile_ADCSSUB(self, event):
        return super(adcs, self).POWERSUB_PowerProfile_ADCSSUB( event)
    def CanPerform(self, event, universe):
        return super(adcs, self).canPerform(event, universe)
    def CanExtend(self, event, universe, extendTo):
        return super(adcs, self).canExtend(self, event, universe, extendTo)
    def DependencyCollector(self, currentEvent):
        return super(adcs, self).DependencyCollector(currentEvent)
