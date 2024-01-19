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
from System import Func, Delegate, Math
from System.Collections.Generic import Dictionary
from IronPython.Compiler import CallTarget0

class DataBuffer(HSFSystem.Subsystem):
         

    def CanPerform(self, event, universe):
        return True
    
    def CanExtend(self, event, universe, extendTo):
        return super(DataBuffer, self).CanExtend(event, universe, extendTo)
      
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
  
    def DependencyCollector(self, currentEvent):
        return super(DataBuffer, self).DependencyCollector(currentEvent)
    
    def DepFinder(self, depFnName):  # Search for method from string input
        fnc = getattr(self, depFnName)
        dep = Dictionary[str, Delegate]()
        depFnToAdd = Func[Event, Utilities.HSFProfile[System.Double]](fnc)
        dep.Add(depFnName, depFnToAdd)
        return dep
      
        
