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
        self.state[13] = .4
        self.state[14] = 27875
        self.state[15] = 27875
        self.state[16] = 99999999
        self.state[17] = 27875
        self.state[18] = 27875
        self.Pk = Matrix[System.Double](18,18,1000)
        self.STATE_KEY = StateVarKey[Quat]("asset1.estimatedState")
        self.addKey(self.STATE_KEY)

        #FIXME: make better
        self.thrustData = []
        text_file = open("C:\Users\steve\BitTorrent Sync\Documents\MATLAB\Thesis\AeroTech_L952.txt", "r")
        for line in text_file:
            lines = [float(elt.strip()) for elt in line.split('\t')]
            self.thrustData.append(lines)
        text_file.close()

        self.InitMass = 21.462
        self.FinalMass = 18.734

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

        #Other redefinitions 
        Pk_Previous = self.Pk

        # Generate the F matrix
        rho = 1.225*math.exp(-xd/8640)
        F = Matrix[System.Double](18)
        F[1,4] = 1
        F[2,5] = 1
        F[3,6] = 1
        F[4,1] = -rho*g*math.pow(xd,2)/16840/ballisticCoeffX
        F[5,1] = -rho*g*math.pow(xd,2)/16840/ballisticCoeffY
        F[6,1] = -rho*g*math.pow(xd,2)/16840/ballisticCoeffZ
        F[4,4] = rho * g * xd / ballisticCoeffX
        F[4,5] = rho * g * xd / ballisticCoeffY
        F[4,6] = rho * g * xd / ballisticCoeffZ
        #F[10,10] = -rho*g*math.pow(xd,2)/16840/ballisticCoeffMX
        #F[11,11] = -rho*g*math.pow(xd,2)/16840/ballisticCoeffMY
        #F[12,12] = -rho*g*math.pow(xd,2)/16840/ballisticCoeffMZ
        #F[4,10] = rho * g * xd / ballisticCoeffMX
        #F[5,11] = rho * g * xd / ballisticCoeffMY
        #F[6,12] = rho * g * xd / ballisticCoeffMZ
        '''
        F[13,13] = 1
        F[14,14] = 1
        F[15,15] = 1
        F[16,16] = 1
        F[17,17] = 1
        F[18,18] = 1
        '''
        '''
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
        '''
        
        I = Matrix[System.Double].Eye(18);

        Phi = Matrix[System.Double](18)
        Phi = I + F*SchedParameters.SimStepSeconds

        Q = Matrix[System.Double](18)
        
        # FIXME: Just pulled out of thin air
        
        Q[13,13] = math.pow(100,2)
        Q[14,14] = math.pow(100,2)
        Q[15,15] = math.pow(100,2)
        Q[16,16] = math.pow(100,2)
        Q[17,17] = math.pow(100,2)
        Q[18,18] = math.pow(100,2)
        
        #Q = Q * math.pow(300,2)
        #FIXME: Figure out if this necessary

        #Q = DiscreteQ( Q, SchedParameters.SimStepSeconds, F)
        #print Q

        H = Matrix[System.Double](18)

        H[4,4] = SchedParameters.SimStepSeconds # The acceleration is measured, a = v*t
        H[5,5] = SchedParameters.SimStepSeconds
        H[6,6] = SchedParameters.SimStepSeconds

        H[10,10] = 1
        H[11,11] = 1
        H[12,12] = 1
        
        
        R =Matrix[System.Double].Eye(18)
        R[4,4] = 0.1 * 0.1
        R[5,5] = 0.1 * 0.1
        R[6,6] = 0.1 * 0.1

        R[10,10] = 0.03*0.03
        R[11,11] = 0.03*0.03
        R[12,12] = 0.03*0.03
        #print R
        M = Phi*Pk_Previous*Matrix[System.Double].Transpose(Phi) + Q 
        #HMHTR = (H*M*Matrix[System.Double].Transpose(H)+R);
        K = M*Matrix[System.Double].Transpose(H)*Matrix[System.Double].Inverse((H*M*Matrix[System.Double].Transpose(H)+R));


        self.Pk = (Matrix[System.Double].Eye(18) - K*H)*M

        G = Matrix[System.Double](18)
        u = Vector(18)
        
        measure = Vector(18)
        measurements = self.DependencyCollector(event)
        measure[4] = measurements[1];
        measure[5] = measurements[2];
        measure[6] = measurements[3];
        measure[10] = measurements[4];
        measure[11] = measurements[5];
        measure[12] = measurements[6];

        #stated = Phi*self.state + G*u + K*(measure -  H*Phi*self.state -  H*G*u)
        ts = event.GetTaskStart(self.asset)
        y = self.Propogate(self.state, .001, ts) #Fixme: Make not self, pass things in

        self.state = y + K*measure;
        #print K
        #self.state = self.state + (stated * SchedParameters.SimStepSeconds); #1st order integration 
        ts = event.GetTaskStart(self.asset)
        self._newState.AddValue(self.STATE_KEY, HSFProfile[Matrix[System.Double]](ts, self.state))
        #print self.state, stated


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
    def Propogate(self, y, ts, t):
        #Rename angles to aid in readbility
        psi = y[7]
        tht = y[8]
        phi = y[9]
        p = y[10]
        q = y[11]
        r = y[12]

        for datapoint in self.thrustData:
            if datapoint[0] == -1:
                mass = self.FinalMass
                thrust = 0.0
            if datapoint[0] > t:
                mass = self.InitMass - (self.InitMass - self.FinalMass)*(t/7)
                thrust = datapoint[1]
                break
        A = 0.0385
            

        #Estimate the gravity vector position
        g = 9.81
        G = Vector(3)
        #G[1] = -g*math.cos(psi)*math.cos(tht)
        #G[2] = g*(math.cos(phi)*math.sin(psi) - math.cos(psi)*math.sin(phi)*math.sin(tht))
        #G[3] = -g*(math.sin(phi)*math.sin(psi) + math.cos(phi)*math.cos(psi)*math.sin(tht))
        G[1] = -9.81

        Qp = 1.225*math.exp(-y[1]/8420)*math.pow(y[4],2)/2 #Estimate dynamic pressure
        
        dy = Vector(18)
        dy[1] = y[4]
        #dy[2] = y[5]
        #dy[3] = y[6]
        dy[4] = G[1] + (thrust/mass) - Qp*A/mass/y[13]
        #dy[5] = G[2] - Qp/y[14]
        #dy[6] = G[3] - Qp/y[15]
        '''
        dy[7] = (q*math.sin(phi) + r*math.cos(phi))/math.cos(tht)
        dy[8] = q*math.cos(phi) - r*math.sin(phi)
        dy[9] = p + dy[7]*math.sin(tht)
        '''           
        #dy[10] = ((moments[0]) - (self.Izz - self.Iyy)*q*r)/self.Ixx
        #dy[11] = ((moments[1] + forces[1]*1.5*self.areaRef) - (self.Ixx - self.Izz)*p*r)/self.Iyy
        #dy[12] = ((moments[2] + forces[2]*1.5*self.areaRef) - (self.Iyy - self.Ixx)*p*q)/self.Izz
        step = 0
        while(step<SchedParameters.SimStepSeconds):
            y = y + dy*step
            step = step + ts

        return y

            
            
def DiscreteQ( Q, Ts, a):
        # Adapted from Matlabs kalmd()

        Nx = a.NumRows
        M = Matrix[System.Double].Vertcat(Matrix[System.Double].Horzcat(-a, Q), Matrix[System.Double].Horzcat(Matrix[System.Double](Nx), Matrix[System.Double].Transpose(a)))
        phi = Matrix[System.Double].exp(M*Ts);
        phi12 = phi[MatrixIndex(1,Nx),MatrixIndex(Nx+1,2*Nx)]
        phi22 = phi[MatrixIndex(Nx+1,2*Nx), MatrixIndex(Nx+1,2*Nx)]
        Qd = Matrix[System.Double].Transpose(phi22)*phi12
        Qd = (Qd+Matrix[System.Double].Transpose(Qd))/2; # Make sure Qd is symmetric
        return Qd

