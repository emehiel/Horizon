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
        self.state[14] = .1
        self.state[15] = .1
        self.state[16] = .01
        self.state[17] = .02
        self.state[18] = .02
        self.Pk = Matrix[System.Double](18)
        #self.Pk[1,1] = 1
        #self.Pk[4,4] = 100
        self.Pk[13,13] = .1
        self.STATE_KEY = StateVarKey[Matrix[System.Double]]("asset1.estimatedState")
        self.KALMAN_KEY = StateVarKey[Matrix[System.Double]]("asset1.Pk")
        self.addKey(self.STATE_KEY)
        self.addKey(self.KALMAN_KEY)
        #FIXME: make better
        self.thrustData = []
        text_file = open("C:\Users\steve\Resilio Sync\Documents\MATLAB\Thesis\AeroTech_L952.txt", "r")
        for line in text_file:
            lines = [float(elt.strip()) for elt in line.split('\t')]
            self.thrustData.append(lines)
        text_file.close()

        self.InitMass = 21.462
        self.FinalMass = 18.734

        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        depFunc1 = Func[Event,  Utilities.HSFProfile[Utilities.Matrix[System.Double]]](self.CONTROLSUB_State_STATESUB)
        dep.Add("StateFromStateEst", depFunc1)
        return dep
    def GetDependencyCollector(self):

        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def CanPerform(self, event, universe):
        
        return True
    
    def CanExtend(self, event, universe, extendTo):
        return True
        #return super(StateEstimation, self).CanExtend(event, universe, extendTo)
    def DependencyCollector(self, currentEvent):
            if (self.SubsystemDependencyFunctions.Count == 0):
                Exception
            #HSFProfile<double> outProf = new HSFProfile<double>();
            i = 1
            outProf = Vector(7)
            for dep in self.SubsystemDependencyFunctions:
                if not dep.Key == "DepCollector":
                    outProf[i] = dep.Value.DynamicInvoke(currentEvent).LastValue()
                    i = i+1
            return outProf
    def CONTROLSUB_State_STATESUB(self, event):
        ts = event.GetEventStart(self.asset)#-SchedParameters.SimStepSeconds # Fixme: Want to let the EOMS propogate to the 1st time step.
        state = Vector(18)
        '''if ts < 0:
            state[13] = .4
            state[14] = 27875
            state[15] = 27875
            state[16] = 99999999
            state[17] = 27875
            state[18] = 27875
            prof1 = HSFProfile[Matrix[System.Double]]()
            prof1[0] = state
            return prof1
        '''
        if not (ts == 0): # Use the previous time steps state, 1st time step doesn't have this so use inital conidtions
            #print self._newState.GetLastValue(self.STATE_KEY).Value()
            state = self._newState.GetLastValue(self.STATE_KEY).Value
            #stateArr = Matrix[System.Double].ToArray(state) # Fixme: Make a better conversion
            state = clr.Convert(state, Vector);
            Pk_Previous = self._newState.GetLastValue(self.KALMAN_KEY).Value
        else:
            Pk_Previous = self.Pk
            state[13] = .4
            state[14] = .1
            state[15] = .1
            state[16] = 0.05
            state[17] = 0.05
            state[18] = 0.05
        g = 9.81
        #Qp = 0.5 * 0.0034 * math.exp(-self.state[1]/22000)

        # Redefine the state variables below to make eqns more readable
        x = state[1]
        y = state[2]
        z = state[3]
        xd = state[4]
        yd = state[5]
        zd = state[6]
        psi = state[7]
        tht = state[8]
        phi = state[9]
        p = state[10]
        q = state[11]
        r = state[12]
        Cx = state[13]
        Cy = state[14]
        Cz = state[15]
        Cl = state[16]
        Cm = state[17]
        Cn = state[18]
        
        # Define "constants" Fixme: Load in via xml
        mass = 20
        area = 0.0385
        Ixx = 0.167
        Iyy = 14.548
        Izz = 14.548
        dia = .2214
        # Generate the F matrix
        rho = 1.225*math.exp(-xd/8640)
        F = Matrix[System.Double](18)
        F[1,4] = 1
        F[2,5] = 1
        F[3,6] = 1
        F[4,1] = -rho*math.pow(xd,2)/16840*Cx/mass*area
        F[5,1] = -rho*math.pow(yd,2)/16840*Cy/mass*area
        F[6,1] = -rho*math.pow(zd,2)/16840*Cz/mass*area
        F[4,4] = rho * xd * Cx/mass*area
        F[5,5] = rho * yd * Cy/mass*area
        F[6,6] = rho * zd * Cz/mass*area
        F[10,10] = -rho*math.pow(xd,2)/16840*Cl/mass*area
        F[11,11] = -rho*math.pow(xd,2)/16840*Cm/mass*area
        F[12,12] = -rho*math.pow(xd,2)/16840*Cn/mass*area
        F[7,8] = math.sin(tht)*(r*math.cos(phi)+q*math.sin(phi))/math.pow(math.cos(tht),2)
        F[7,9] = (q*math.cos(phi) - r*math.sin(phi))/math.cos(tht)
        F[8,9] = r*math.cos(phi)-q*math.sin(phi)
        F[9,8] = r*math.cos(phi)+q*math.sin(phi) + (math.pow(math.sin(tht),2)*(r*math.cos(phi)+q*math.sin(phi)))/math.pow(math.cos(tht), 2)
        F[9,9] = (math.sin(tht)*(r*math.cos(phi)-q*math.sin(phi)))/math.cos(tht)
        F[7,11] = math.sin(phi)/math.cos(tht)
        F[7,12] = math.cos(phi)/math.cos(tht)
        F[8,11] = math.cos(phi)
        F[8,12] = math.sin(phi)
        F[9,10] = 1
        F[9,11] = math.sin(phi)*math.sin(tht)/math.cos(tht)
        F[9,12] = math.cos(phi)*math.sin(tht)/math.cos(tht)
        F[10,1] = rho*math.pow(xd,2)/16840*Cx/mass*area/Ixx
        F[11,1] = rho*math.pow(yd,2)/16840*Cy/mass*area/Iyy
        F[12,1] = rho*math.pow(zd,2)/16840*Cz/mass*area/Izz
        F[10,4] = rho*math.pow(xd,2)*Cl/mass*area/Ixx
        F[11,4] = rho*math.pow(yd,2)*Cm/mass*area/Iyy
        F[12,4] = rho*math.pow(zd,2)*Cn/mass*area/Izz
        F[10,11] = -(r*(Izz-Iyy))/Ixx
        F[10,12] = -(q*(Izz-Iyy))/Ixx
        F[11,10] = -(r*(Ixx-Izz))/Iyy
        F[11,12] = -(p*(Ixx-Izz))/Iyy
        F[12,10] = -(q*(Iyy-Ixx))/Izz
        F[12,11] = -(p*(Iyy-Ixx))/Izz
        F[11,14] = 0.75*rho*math.pow(yd,2)*area*dia/mass
        F[12,15] = 0.75*rho*math.pow(zd,2)*area*dia/mass
        F[10,16] = rho*math.pow(xd,2)*area/Ixx/mass
        F[11,17] = rho*math.pow(yd,2)*area/Iyy/mass
        F[12,18] = rho*math.pow(zd,2)*area/Izz/mass
        I = Matrix[System.Double].Eye(18);

        Phi = Matrix[System.Double](18)
        Phi = I + F*SchedParameters.SimStepSeconds

        Q = Matrix[System.Double](18)
        
        # FIXME: Just pulled out of thin air
        Q[1,1] = 0.2
        Q[2,2] = 1.2
        Q[3,3] = 1.2
        Q[4,4] = 10
        Q[5,5] = 10
        Q[6,6] = 10
        Q[7,7] = .1
        Q[8,8] = .1
        Q[9,9] = .1
        Q[10,10] = 1
        Q[11,11] = 1
        Q[12,12] = 1
        Q[13,13] = math.pow(.05,2)
        Q[14,14] = math.pow(.05,2)
        Q[15,15] = math.pow(.05,2)
        Q[16,16] = math.pow(.1,2)
        Q[17,17] = math.pow(.1,2)
        Q[18,18] = math.pow(.1,2)
        
        #Q = Q * math.pow(300,2)
        #FIXME: Figure out if this necessary

        #Q = DiscreteQ( Q, SchedParameters.SimStepSeconds, F)
        #print Q

        H = Matrix[System.Double](18)
        H[1,1] = 1
        #H[4,4] = 1/SchedParameters.SimStepSeconds # The acceleration is measured, a = v*t
        #H[5,5] = 1/SchedParameters.SimStepSeconds
        #H[6,6] = 1/SchedParameters.SimStepSeconds

        H[10,10] = 1
        H[11,11] = 1
        H[12,12] = 1
        
        
        R =Matrix[System.Double].Eye(18)*.0001
        #R =Matrix[System.Double](18)
        R[1,1] = 10
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
        #kDiag = Matrix[System.Double](18)
        #for ii in range(17):
            #kDiag[ii+1,ii+1] = K[ii+1,ii+1]

        #print kDiag, Matrix[System.Double].Eye(18)
        Pk = (Matrix[System.Double].Eye(18) - K*H)*M

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
        measure[1] = measurements[7];

        #stated = Phi*self.state + G*u + K*(measure -  H*Phi*self.state -  H*G*u)
        
        y = self.Propagate(state, .001, ts) #Fixme: Make not self, pass things in
        #y = y + dy * SchedParameters.SimStepSeconds
        res = measure-H*y;
        #print res
        eststate = y + K*(res)
        #print eststate
        #self.state = self.state + (stated * SchedParameters.SimStepSeconds); #1st order integration 
        #ts = event.GetTaskStart(self.asset)
        self._newState.AddValue(self.STATE_KEY, HSFProfile[Matrix[System.Double]](ts, eststate))
        self._newState.AddValue(self.KALMAN_KEY, HSFProfile[Matrix[System.Double]](ts, Pk))
        prof1 = HSFProfile[Matrix[System.Double]]()
        prof1[ts] = eststate
        #print self.state, stated
        return prof1
    def Propagate(self, y, ts, t):
        #Rename angles to aid 
        psi = y[7]
        tht = y[8]
        phi = y[9]
        p = y[10]
        q = y[11]
        r = y[12]

        # Calculate/set "knowns"
        for datapoint in self.thrustData:
            if datapoint[0] == -1:
                mass = self.FinalMass
                thrust = 0.0
            if datapoint[0] > t:
                mass = self.InitMass - (self.InitMass - self.FinalMass)*(t/7)
                thrust = datapoint[1]
                break
        A = 0.0385
        D = 0.2214
        Ixx = 0.167
        Iyy = 14.548
        Izz = 14.548  

        #Estimate the gravity vector position
        g = 9.81
        G = Vector(3)
        G[1] = -g*math.cos(psi)*math.cos(tht)
        G[2] = g*(math.cos(phi)*math.sin(psi) - math.cos(psi)*math.sin(phi)*math.sin(tht))
        G[3] = -g*(math.sin(phi)*math.sin(psi) + math.cos(phi)*math.cos(psi)*math.sin(tht))
        
        #Estimate dynamic pressure
        rho = 1.225*math.exp(-y[1]/8420)
        Qp = 0.5*rho*math.pow(y[4],2) # Fixme: make a vector
        
        dy = Vector(18)
        #Position is the velocity
        dy[1] = y[4]
        dy[2] = y[5]
        dy[3] = y[6]
        
        #Sum the forces/mass to get the accelerations
        dy[4] = G[1] + (thrust/mass) - Qp*A/mass*y[13]
        dy[5] = G[2] - Qp*A/mass*y[14]
        dy[6] = G[3] - Qp*A/mass*y[15]
        # Use body angles to get the euler rates
        dy[7] = (q*math.sin(phi) + r*math.cos(phi))/math.cos(tht)
        dy[8] = q*math.cos(phi) - r*math.sin(phi)
        dy[9] = p + dy[7]*math.sin(tht)
        
        # Use ang momentum eqns to calculate body rates        
        dy[10] = (Qp*A/mass*y[16] - (Izz - Iyy)*q*r)/Ixx
        dy[11] = ((Qp*A/mass*y[17] + Qp*A/mass*y[14]*1.5*D) - (Ixx - Izz)*p*r)/Iyy
        dy[12] = ((Qp*A/mass*y[18] + Qp*A/mass*y[15]*1.5*D) - (Iyy - Ixx)*p*q)/Izz
        #Qp = 0.5*rho*math.pow(y[5],2) 
        
        #Qp = 0.5*rho*math.pow(y[6],2) 
        
        # Propagate forward to the next time step
        step = 0
        dt = ts
        while(step<SchedParameters.SimStepSeconds):
            y = y + dy*dt
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

