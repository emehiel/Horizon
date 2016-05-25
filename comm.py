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

class comm(HSFSubsystem.Comm):
    #def __new__(self, node, asset):
    #    print("Initializing Scripted Subsystem ADCS")
    #    return HSFSubsystem.Comm.__new__(self, node, asset)
    def __init__(self, node, asset):
        self.DATARATE_KEY = StateVarKey[System.Double](self.Asset.Name + "." + "datarate(mb/s)")
        super(comm, self).addKey(self.DATARATE_KEY)
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        depFunc1 = Func[Event,  Utilities.HSFProfile[System.Double]](self.POWERSUB_PowerProfile_COMMSUB)
        dep.Add("PowerfromCOMM", depFunc1)
        return dep
    def CanPerform(self, event, universe):
        return super(comm, self).canPerform(event, universe)
    def CanExtend(self, event, universe, extendTo):
        return super(comm, self).canExtend(self, event, universe, extendTo)
    def POWERSUB_PowerProfile_COMMSUB(self, event):
        return super(comm, self).POWERSUB_PowerProfile_COMMSUB(event)