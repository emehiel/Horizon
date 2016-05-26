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

class ssdr(HSFSubsystem.SSDR):
    #def __new__(self, node, asset):
    #    print("Initializing Scripted Subsystem ADCS")
    #    return HSFSubsystem.Comm.__new__(self, node, asset)
    def __init__(self, node, asset):
        self.DATABUFFERRATIO_KEY = StateVarKey[System.Double](self.Asset.Name + "." + "databufferfillratio")
        super(ssdr, self).addKey(self.DATABUFFERRATIO_KEY)
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        depFunc1 = Func[Event,  Utilities.HSFProfile[System.Double]](self.POWERSUB_PowerProfile_SSDRSUB)
        dep.Add("PowerfromSSDR", depFunc1)
        depFunc1 = Func[Event,  Utilities.HSFProfile[System.Double]](self.COMMSUB_DataRateProfile_SSDRSUB)
        dep.Add("COMMfromSSDR", depFunc1)
        depFunc1 = Func[Event,  System.Double](self.EVALSUB_DataRateProfile_SSDRSUB)
        dep.Add("EvalfromSSDR", depFunc1)
        return dep
    def CanPerform(self, event, universe):
        return super(ssdr, self).canPerform(event, universe)
    def CanExtend(self, event, universe, extendTo):
        return super(ssdr, self).canExtend(self, event, universe, extendTo)
    def POWERSUB_PowerProfile_SSDRSUB(self, event):
        return super(ssdr, self).POWERSUB_PowerProfile_SSDRSUB(event)
    def COMMSUB_DataRateProfile_SSDRSUB(self, event):
        return super(ssdr, self).COMMSUB_DataRateProfile_SSDRSUB(event)
    def EVAL_DataRateProfile_SSDRSUB(self, event):
        return super(ssdr, self).EVAL_DataRateProfile_SSDRSUB(event)