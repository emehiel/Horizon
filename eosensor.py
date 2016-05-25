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

class eosensor(HSFSubsystem.EOSensor):
    #def __new__(self, node, asset):
    #    print("Initializing Scripted Subsystem ADCS")
    #    return HSFSubsystem.EOSensor.__new__(self, node, asset)
    def __init__(self, node, asset):
        self.PIXELS_KEY =  StateVarKey[double](self.Asset.Name +"." + "numpixels");
        self.INCIDENCE_KEY =  StateVarKey[double](self.Asset.Name + "." + "incidenceangle");
        self.EOON_KEY =  StateVarKey[bool](self.Asset.Name + "." + "eosensoron");
        super(eosensor, self).addkey(self.PIXELS_KEY)
        super(eosensor, self).addkey(self.INCIDENCE_KEY)
        super(eosensor, self).addkey(self.EOON_KEY)
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        depFunc1 = Func[Event,  Utilities.HSFProfile[System.Double]](self.(POWERSUB_PowerProfile_EOSENSORSUB))
        dep.Add("PowerfromEOSensor", depFunc1)
        depFunc1 = Func[Event,  Utilities.HSFProfile[System.Double]](self.(SSDRSUB_NewDataProfile_EOSENSORSUB))
        dep.Add("SSDRfromEOSensor", depFunc1)
        return dep
    def CanPerform(self, event, universe):
        return super(eosensor, self).canPerform(event, universe)
    def CanExtend(self, event, universe, extendTo):
        return super(eosensor, self).canExtend(self, event, universe, extendTo)
    def POWERSUB_PowerProfile_EOSENSORSUB(self, event):
        return super(eosensor, self).POWERSUB_PowerProfile_EOSENSORSUB(event)
    def SSDRSUB_NewDataProfile_EOSENSORSUB(self, event):
        return super(eosensor, self).SSDRSUB_NewDataProfile_EOSENSORSUB(event)