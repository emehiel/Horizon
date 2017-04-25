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

class Controller(Subsystem):
    def __init__(self, node, asset):
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        return dep
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def CanPerform(self, event, universe):
        #return super(subsystem, self).CanPerform(event, universe)
        self.DependencyCollector(event)
        return True
    def CanExtend(self, event, universe, extendTo):
        return True
        #return super(Controller,self).CanExtend(event, universe, extendTo)
    def DependencyCollector(self, currentEvent):
        if (self.SubsystemDependencyFunctions.Count == 0):
            Exception 
            #MissingMemberException("You may not call the dependency collector in your can perform because you have not specified any dependency functions for " + Name);
        outProf = HSFProfile[Matrix[System.Double]]()
        for dep in self.SubsystemDependencyFunctions:
            if not (dep.Key.Equals("DepCollector")):
                temp = dep.Value.DynamicInvoke(currentEvent)
                outProf = outProf + temp
        return outProf
        #return super(Controller, self).DependencyCollector(currentEvent)