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


class comm(HSFSystem.Subsystem):
    def __new__(cls, node, asset):
        instance = HSFSystem.Subsystem.__new__(cls)
        instance.Asset = asset
        instance.Name = instance.Asset.Name + '.' + node.Attributes['subsystemName'].Value.ToString().ToLower()

        instance.DATARATE_KEY = Utilities.StateVariableKey[System.Double](instance.Asset.Name + '.' + 'datarate(mb/s)')
        instance.addKey(instance.DATARATE_KEY)
        return instance

    def CanPerform(self, event, universe):
        #print("Entry of Comm CanPreform")
        #print(self._task.Type)

        # shorcut check for empty target
        tgtName = event.GetAssetTask(self.Asset).Target.Name.ToString()
        if (tgtName == 'EmptyTarget'):
            return True

        if self._task.Type == "comm":
            newProf = self.DependencyCollector(event)
            if (newProf.Empty() == False):
                event.State.SetProfile(self.DATARATE_KEY, newProf)
                print("Comm CanPreform, Datarate")
                print(newProf[0])
        return True

    def CanExtend(self, event, universe, extendTo):
        return super(comm, self).CanExtend(event, universe, extendTo)

    def Power_asset1_from_Comm_asset1(self, event):
        # print('In Python Dep Fn')
        return event.State.GetProfile(self.DATARATE_KEY) * 20

    def DepFinder(self, depFnName):  # Search for method from string input
        fnc = getattr(self, depFnName)
        dep = Dictionary[str, Delegate]()
        depFnToAdd = Func[Event, Utilities.HSFProfile[System.Double]](fnc)
        dep.Add(depFnName, depFnToAdd)
        return dep

    # Called from C#, calls Func on Python method DependencyCollector
    def GetDependencyCollector(self):
        return Func[Event, Utilities.HSFProfile[System.Double]](self.DependencyCollector)

    # Called by GetDependencyCollector, Calls C#'s Subsystem.DependencyCollector w/ event
    def DependencyCollector(self, currentEvent):
        return super(comm, self).DependencyCollector(currentEvent)

#    def DepFinder(self, depFnName):
#        fnc = getattr(self, depFnName)
#        print(fnc)
#        return fnc

#    def DepFinder2(self, depFnName):
#        globe = globals()
#        print(globe)
#        fnc = globals()[depFnName]()
#        print(fnc)
#        return fnc

#    def DepFinder3(self, depFnName):  # Hard Coded Search for method
#        fnc = getattr(self, depFnName)
#        dep = Dictionary[str, Delegate]()
#        depFnToAdd = Func[Event, Utilities.HSFProfile[System.Double]](self.POWERSUB_PowerProfile_COMMSUB)
#        dep.Add(depFnName, depFnToAdd)
#        return dep

#    def GetDependencyDictionary(self):
#        dep = Dictionary[str, Delegate]()
#        depFunc1 = Func[Event, Utilities.HSFProfile[System.Double]](self.POWERSUB_PowerProfile_COMMSUB)
#        dep.Add("PowerfromComm" + "." + self.Asset.Name, depFunc1)
#        return dep

#    def POWERSUB_PowerProfile_COMMSUB(self, event):
#        return event.State.GetProfile(self.DATARATE_KEY) * 20


