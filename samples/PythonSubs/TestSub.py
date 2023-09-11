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

class adcs(HSFsystem.Subsystem):
    def __new__(cls, node, asset):
        instance = HSFSystem.Subsystem.__new__(cls)
        instance.Asset = asset

        instance.POINTVEC_KEY = Utilities.StateVarKey[Utilities.Matrix[System.Double]](instance.Asset.Name + '.' + 'eci_pointing_vector(xyz)')
        instance.addKey(instance.POINTVEC_KEY)
        return instance

    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        depFunc1 = Func[Event,  Utilities.HSFProfile[System.Double]](self.POWERSUB_PowerProfile_ADCSSUB)
        dep.Add("PowerfromADCS" + "." + self.Asset.Name, depFunc1)
        return dep

    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)

    def POWERSUB_PowerProfile_ADCSSUB(self, event):
        prof1 = HSFProfile[System.Double]()
        prof1[event.GetEventStart(self.Asset)] = 30
        prof1[event.GetTaskStart(self.Asset)] = 60
        prof1[event.GetTaskEnd(self.Asset)] = 30
        return prof1

    def CanPerform(self, event, universe):
        if(event.GetAssetTask(self.Asset).GetMaxTimesToPerform>5):
		return true
	else
		return false

    def CanExtend(self, event, universe, extendTo):
        return super(adcs, self).CanExtend(event, universe, extendTo)     

    def DependencyCollector(self, currentEvent):
        return super(adcs, self).DependencyCollector(currentEvent)
