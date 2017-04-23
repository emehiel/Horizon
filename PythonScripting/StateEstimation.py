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
        self.state = Vector(18)
        self.state[13] = 1240
        self.state[14] = 27875
        self.state[15] = 27875
        self.state[16] = 99999999
        self.state[17] = 27875
        self.state[18] = 27875
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
        g = 9.81
        #Qp = 0.5 * 0.0034 * math.exp(-self.state[1]/22000)

        # Redefine the state variables below to make eqns more readable
        x = self.state[1]
        y = self.state[2]
        z = self.state[3]
        xd = self.state[4]
        yd = self.state[5]
        zd = self.state[6]
        psi = self.state[7]
        tht = self.state[8]
        phi = self.state[9]
        p = self.state[10]
        q = self.state[11]
        r = self.state[12]
        ballisticCoeffX = self.state[13]
        ballisticCoeffY = self.state[14]
        ballisticCoeffZ = self.state[15]
        ballisticCoeffMX = self.state[16]
        ballisticCoeffMY = self.state[17]
        ballisticCoeffMZ = self.state[18]

        #self.stateDerivative = Vector(15)

        #self.stateDerivative[1] = self.state[1]
        #self.stateDerivative[2] = self.state[2]
        #self.stateDerivative[3] = self.state[3]
        #self.stateDerivative[4] = -g*math.cos(tht) - T/m - Qp/ballisticCoeffX
        #self.stateDerivative[5] = g*math.sin(tht)*math.sin(phi) - Qp/ballisticCoeffY
        #self.stateDerivative[6] = g*math.sin(tht)*math.cos(phi) - Qp/ballisticCoeffZ
        #self.stateDerivative[7] = 0
        
        
        
        
        rho = 0.0034*math.exp(-xd/22000)
        F = Matrix[System.Double](18)
        F[1,4] = 1
        F[2,5] = 1
        F[3,6] = 1
        F[1,4] = -rho*g*math.pow(xd,2)/44000/ballisticCoeffX
        F[1,5] = -rho*g*math.pow(xd,2)/44000/ballisticCoeffY
        F[1,6] = -rho*g*math.pow(xd,2)/44000/ballisticCoeffZ
        F[4,4] = rho * g * xd / ballisticCoeffX
        F[4,5] = rho * g * xd / ballisticCoeffY
        F[4,6] = rho * g * xd / ballisticCoeffZ
        F[10,10] = -rho*g*math.pow(xd,2)/44000/ballisticCoeffMX
        F[11,11] = -rho*g*math.pow(xd,2)/44000/ballisticCoeffMY
        F[12,12] = -rho*g*math.pow(xd,2)/44000/ballisticCoeffMZ
        F[4,10] = rho * g * xd / ballisticCoeffMX
        F[5,11] = rho * g * xd / ballisticCoeffMY
        F[6,12] = rho * g * xd / ballisticCoeffMZ
        F[13,13] = 1
        F[14,14] = 1
        F[15,15] = 1
        F[16,16] = 1
        F[17,17] = 1
        F[18,18] = 1
        F[4,7] = g*math.sin(psi)*math.cos(tht)
        F[4,8] = g*math.sin(tht)*math.cos(psi)
        F[5,7] = g*(math.cos(phi)*math.cos(psi) + math.sin(phi)*math.sin(psi)*math.sin(tht))
        F[5,8] = -g*math.cos(psi)*math.cos(tht)*math.sin(phi)
        F[5,9] = -g*(math.sin(phi)*math.sin(psi) + math.cos(phi)*math.cos(psi)*math.sin(tht))
        F[6,7] = -g*(math.cos(psi)*math.sin(phi) - math.cos(phi)*math.sin(psi)*math.sin(tht))
        F[6,8] = -g*math.cos(phi)*math.cos(psi)*math.cos(tht)
        F[6,9] = -g*(math.cos(phi)*math.sin(psi) - math.cos(psi)*math.sin(phi)*math.sin(tht))
        F[7,8] = (math.sin(tht)*(r*math.cos(phi) + q*math.sin(phi)))/math.pow(math.cos(tht),2)
        F[7,9] = (q*math.cos(phi) - r*math.sin(phi))/math.cos(tht)
        F[8,9] = -r*math.cos(phi) - q*math.sin(phi)
        F[9,8] = r*math.cos(phi) + q*math.sin(phi) + (math.pow(math.sin(tht),2)*(r*math.cos(phi) + q*math.sin(phi)))/math.pow(math.cos(tht),2)
        F[9,9] = (math.sin(tht)*(q*math.cos(phi) - r*math.sin(phi)))/math.cos(tht)

        
        I = Matrix[System.Double].Eye(18);

        Psi = Matrix[System.Double](18)
        Psi = I + F*SchedParameters.SimStepSeconds

        Q = Matrix[System.Double](18)
        
        # FIXME: Just pulled out of thin air
        Q[13,13] = math.pow(300,2)
        Q[14,14] = math.pow(300,2)
        Q[15,15] = math.pow(300,2)
        Q[16,16] = math.pow(300,2)
        Q[17,17] = math.pow(300,2)
        Q[18,18] = math.pow(300,2)
        
        H = Matrix[System.Double](18)

        H[4,4] = SchedParameters.SimStepSeconds # The acceleration is measured, a = v*t
        H[5,5] = SchedParameters.SimStepSeconds
        H[6,6] = SchedParameters.SimStepSeconds

        H[10,10] = 1
        H[11,11] = 1
        H[12,12] = 1
        R = 0.03

        M = Psi*Pk_Previous*Matrix[System.Double].Transpose(Psi) + Q 
        K = M*Matrix[System.Double].Transpose(H)
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
                    
            