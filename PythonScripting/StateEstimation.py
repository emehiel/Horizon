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
from HSFSystem import *
from HSFSubsystem import Subsystem
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
        self.asset = asset
        self.qk = Quat()
        self.STATE_KEY = StateVarKey[Quat]("asset1.estimatedState")
        self.addKey(self.STATE_KEY)
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        depFunc1 = Func[Event,  Utilities.HSFProfile[Utilities.Matrix[System.Double]]](self.ADCSSub_State_STATESUB)
        dep.Add("StateFromStateEst", depFunc1)
        return dep
    def GetDependencyCollector(self):

        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def CanPerform(self, event, universe):
        # Quaternion Integration
        # ref: Strapdown Inertial Navigation Technology 2nd ed, Titterton + Weston, p319
        qk1 = Quat()


        measurements = Vector(6)
        measurements = self.DependencyCollector(event)
        wx = measurements[4]
        wy = measurements[5]
        wz = measurements[6]
        
        sigma = Vector(3)
        sigma[1] = wx * SchedParameters.SimStepSeconds
        sigma[2] = wy * SchedParameters.SimStepSeconds
        sigma[3] = wz * SchedParameters.SimStepSeconds

        #print sigma
        Ac = math.cos(Vector.Norm(sigma)/2);
        if not Vector.Norm(sigma) == 0:
            As = math.sin(Vector.Norm(sigma)/2)/Vector.Norm(sigma);
        else:
            As = 0

        rk = Quat(Ac, As*sigma)
        #print Ac, As, rk
        #print rk.ToString();
        
        self.qk = self.qk  * rk
        ts = event.GetTaskStart(self.asset)
        self._newState.AddValue(self.STATE_KEY, HSFProfile[Quat](ts, self.qk))

        #print self.qk
        #qk1 = Matrix.exp(SIGMA/2)*qk


        return True
    def ADCSSub_State_STATESUB(self, event):
        prof1 = HSFProfile[System.Double]()
        prof1[event.GetEventStart(self.Asset)] = 30
        prof1[event.GetTaskStart(self.Asset)] = 60
        prof1[event.GetTaskEnd(self.Asset)] = 30
        return prof1
    def CanExtend(self, event, universe, extendTo):
        return True
        #return super(StateEstimation, self).CanExtend(event, universe, extendTo)
    def DependencyCollector(self, currentEvent):
            if (self.SubsystemDependencyFunctions.Count == 0):
                Exception
            #HSFProfile<double> outProf = new HSFProfile<double>();
            i = 1
            outProf = Vector(6)
            for dep in self.SubsystemDependencyFunctions:
                if not dep.Key == "DepCollector":
                    outProf[i] = dep.Value.DynamicInvoke(currentEvent).LastValue()
                    i = i+1
            return outProf
                    
            