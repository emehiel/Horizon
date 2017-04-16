import sys
import clr
import System.Collections.Generic
import System
import math
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

class StateEstimation(Subsystem):
    def __init__(self, node, asset):
        self.dt = 1/100
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        return dep
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def CanPerform(self, event, universe):

        # Quaternion Integration
        # ref: Strapdown Inertial Navigation Technology 2nd ed, Titterton + Weston, p319
        sigma = Vector(3)
        sigma[1] = wx * self.dt
        sigma[2] = wy * self.dt
        sigma[3] = wz * self.dt


        SIGMA = Matrix[System.Double](4)
        SIGMA[1,1] = 0
        SIGMA[1,2] = -sigma[1]
        SIGMA[1,3] = -sigma[2]
        SIGMA[1,4] = -sigma[3]
        SIGMA[2,1] =  sigma[1]
        SIGMA[2,2] = 0
        SIGMA[2,3] =  sigma[3]
        SIGMA[2,4] = -sigma[2]
        SIGMA[3,1] =  sigma[2]
        SIGMA[3,2] = -sigma[3]
        SIGMA[3,3] = 0
        SIGMA[3,4] =  sigma[1]
        SIGMA[4,1] =  sigma[3]
        SIGMA[4,2] =  sigma[2]
        SIGMA[4,3] = -sigma[1]
        SIGMA[4,4] = 0

        qk1 = math.exp(SIGMA/2)*qk


        return super(subsystem, self).CanPerform(event, universe)
    def CanExtend(self, event, universe, extendTo):
        return super(subsystem, self).CanExtend(self, event, universe, extendTo)
    def DependencyCollector(self, currentEvent):
        return super(subsystem, self).DependencyCollector(currentEvent)