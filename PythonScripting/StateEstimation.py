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
from System import Func, Delegate, Math
from System.Collections.Generic import Dictionary
from IronPython.Compiler import CallTarget0

class StateEstimation(Subsystem):
    def __init__(self, node, asset):
        self.asset = asset
        self.state = Vector(18)
        self.state[13] = .4
        self.state[14] = .1
        self.state[15] = .1
        self.state[16] = 0
        self.state[17] = .02
        self.state[18] = .02
        self.Pk = Matrix[System.Double](18)
        self.Pk[13,13] = .1
        self.Pk[14,14] = .1
        self.Pk[15,15] = .1
        self.Pk[16,16] = .05
        self.Pk[17,17] = .05
        self.Pk[18,18] = .05
        self.STATE_KEY = StateVarKey[Matrix[System.Double]](asset.Name + "." + "estimatedState")
        self.KALMAN_KEY = StateVarKey[Matrix[System.Double]](asset.Name + "." + "Pk")
        self.DROGUE_DEPLOYED = StateVarKey[System.Boolean](asset.Name + "." + "drogue")
        self.addKey(self.STATE_KEY)
        self.addKey(self.KALMAN_KEY)
        # Propulsion
        self.BurnTime = float(node["Propulsion"].Attributes["BurnTime"].Value)
        if node["Propulsion"].Attributes["Type"].Value == "Constant":
            Thrust = float(node["Propulsion"].Attributes["Thrust"].Value)
            self.thrustData = []
            self.thrustData.append( [0, 0])
            self.thrustData.append( [.1, Thrust])
            self.thrustData.append( [self.BurnTime, Thrust])
            self.thrustData.append( [-1, 0])
            
        elif (node["Propulsion"].Attributes["Type"].Value == "File"):
            self.thrustData = []
            text_file = open(node["Propulsion"].Attributes["Filename"].Value, "r")
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
        if not (self.asset.AssetDynamicState.IntegratorParameters.GetValue(self.DROGUE_DEPLOYED)):
        #if(False):
        #if(True):
            state = Vector(18)
            if not (ts == 0): # Use the previous time steps state, 1st time step doesn't have this so use inital conidtions
                state = self._newState.GetLastValue(self.STATE_KEY).Value
                state = clr.Convert(state, Vector);
                Pk_Previous = self._newState.GetLastValue(self.KALMAN_KEY).Value
            else:
                Pk_Previous = self.Pk
                state[13] = .4
                state[14] = .1
                state[15] = .1
                state[16] = 0
                state[17] = 0
                state[18] = 0
            g = 9.81

            # Redefine the state variables below to make eqns more readable
            x = state[1]
            y = state[2]
            z = state[3]
            xd = state[4]
            yd = state[5]
            zd = state[6]
            eulerRoll= state[7]
            eulerPitch = state[8]
            eulerYaw = state[9]
            bodyRoll = state[10]
            bodyPitch = state[11]
            bodyYaw = state[12]
            Cx = state[13]
            Cy = state[14]
            Cz = state[15]
            Cl = state[16]
            Cm = state[17]
            Cn = state[18]
            
            # Define "constants" Fixme: Load in via xml
            initMass = 54.5
            finalMass = 35
            burnTime = 17

            if ts < burnTime:
                mass = initMass - (initMass - finalMass)*(ts/burnTime)
            else:
                mass = finalMass

            
            area = 0.0385
            Ixx = 0.19
            Iyy = 58.79
            Izz = 58.79
            dia = .2214

            # Generate the F matrix
            rho = 1.225*Math.Exp(-x/8640)
            F = Matrix[System.Double](18)
            F[1,4] = 1
            F[2,5] = 1
            F[3,6] = 1
            F[4,1] = -rho*Math.Pow(xd,2)/16840*Cx/mass*area
            F[5,1] = -rho*Math.Pow(yd,2)/16840*Cy/mass*area
            F[6,1] = -rho*Math.Pow(zd,2)/16840*Cz/mass*area
            F[4,4] = rho * xd * Cx/mass*area
            F[5,5] = rho * yd * Cy/mass*area
            F[6,6] = rho * zd * Cz/mass*area
            '''
                    F[7,8] = math.sin(tht)*(r*math.cos(phi)+q*math.sin(phi))/math.pow(math.cos(tht),2)
        F[7,9] = (q*math.cos(phi) - r*math.sin(phi))/math.cos(tht)
        F[8,9] = r*math.cos(phi)-q*math.sin(phi)
        F[9,8] = r*math.cos(phi)+q*math.sin(phi) + (math.pow(math.sin(tht),2)*(r*math.cos(phi)+q*math.sin(phi)))/math.pow(math.cos(tht), 2)
        F[9,9] = (math.sin(tht)*(q*math.cos(phi)-r*math.sin(phi)))/math.cos(tht)
        F[7,11] = math.sin(phi)/math.cos(tht)
        F[7,12] = math.cos(phi)/math.cos(tht)
        F[8,11] = math.cos(phi)
        F[8,12] = math.sin(phi)
        F[9,10] = 1
        F[9,11] = math.sin(phi)*math.sin(tht)/math.cos(tht)
        F[9,12] = math.cos(phi)*math.sin(tht)/math.cos(tht)
        F[10,1] = -rho*math.pow(xd,2)/16840*Cl/mass*area/Ixx
        F[11,1] = -rho*math.pow(yd,2)/16840*Cm/mass*area/Iyy
        F[12,1] = -rho*math.pow(zd,2)/16840*Cn/mass*area/Izz
        F[10,4] = rho*math.pow(xd,2)*Cl/mass*area/Ixx
        F[11,5] = rho*math.pow(yd,2)*Cm/mass*area/Iyy
        F[12,6] = rho*math.pow(zd,2)*Cn/mass*area/Izz
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
        F[12,18] = rho*math.pow(zd,2)*area/Izz/mass'''
            F[7,10] = Math.Cos(eulerRoll)*Math.Cos(eulerPitch)
            F[7,11] = Math.Cos(eulerPitch)*Math.Sin(eulerRoll)
            F[7,12] = -Math.Sin(eulerPitch);
            F[8,10] = Math.Cos(eulerRoll)*Math.Sin(eulerPitch)*Math.Sin(eulerPitch) -Math.Cos(eulerPitch)*Math.Sin(eulerRoll)
            F[8,11] = Math.Cos(eulerRoll)*Math.Cos(eulerPitch) + Math.Sin(eulerPitch)*Math.Sin(eulerRoll)*Math.Sin(eulerPitch)
            F[8,12] = Math.Cos(eulerPitch)*Math.Sin(eulerPitch)
            F[9,10] = Math.Sin(eulerRoll)*Math.Sin(eulerPitch) +Math.Cos(eulerRoll)*Math.Cos(eulerPitch)*Math.Sin(eulerPitch)
            F[9,11] = Math.Cos(eulerPitch)*Math.Sin(eulerPitch)*Math.Sin(eulerRoll) -Math.Cos(eulerRoll)*Math.Sin(eulerPitch)
            F[9,12] = Math.Cos(eulerPitch)*Math.Cos(eulerPitch)
            F[7,7] = bodyPitch*Math.Cos(eulerPitch)*Math.Cos(eulerRoll) - bodyRoll*Math.Cos(eulerPitch)*Math.Sin(eulerRoll)
            F[7,8] = -bodyYaw*Math.Cos(eulerPitch) - bodyRoll*Math.Cos(eulerRoll)*Math.Sin(eulerPitch) - bodyPitch*Math.Sin(eulerPitch)*Math.Sin(eulerRoll)
            F[8,7] = -bodyRoll*(Math.Cos(eulerRoll)*Math.Cos(eulerYaw) + Math.Sin(eulerPitch)*Math.Sin(eulerRoll)*Math.Sin(eulerYaw)) - bodyPitch*(Math.Cos(eulerYaw)*Math.Sin(eulerRoll) - Math.Cos(eulerRoll)*Math.Sin(eulerPitch)*Math.Sin(eulerYaw))
            F[8,8] = bodyRoll*Math.Cos(eulerPitch)*Math.Cos(eulerRoll)*Math.Sin(eulerYaw) - bodyYaw*Math.Sin(eulerPitch)*Math.Sin(eulerYaw) + bodyPitch*Math.Cos(eulerPitch)*Math.Sin(eulerRoll)*Math.Sin(eulerYaw)
            F[8,9] = bodyRoll*(Math.Sin(eulerRoll)*Math.Sin(eulerYaw) + Math.Cos(eulerRoll)*Math.Cos(eulerYaw)*Math.Sin(eulerPitch)) - bodyPitch*(Math.Cos(eulerRoll)*Math.Sin(eulerYaw) - Math.Cos(eulerYaw)*Math.Sin(eulerPitch)*Math.Sin(eulerRoll)) + bodyYaw*Math.Cos(eulerPitch)*Math.Cos(eulerYaw)
            F[9,7] = bodyRoll*(Math.Cos(eulerRoll)*Math.Sin(eulerYaw) - Math.Cos(eulerYaw)*Math.Sin(eulerPitch)*Math.Sin(eulerRoll)) + bodyPitch*(Math.Sin(eulerRoll)*Math.Sin(eulerYaw) + Math.Cos(eulerRoll)*Math.Cos(eulerYaw)*Math.Sin(eulerPitch))
            F[9,8] = bodyRoll*Math.Cos(eulerPitch)*Math.Cos(eulerRoll)*Math.Cos(eulerYaw) - bodyYaw*Math.Cos(eulerYaw)*Math.Sin(eulerPitch) + bodyPitch*Math.Cos(eulerPitch)*Math.Cos(eulerYaw)*Math.Sin(eulerRoll)
            F[9,9] = bodyRoll*(Math.Cos(eulerYaw)*Math.Sin(eulerRoll) - Math.Cos(eulerRoll)*Math.Sin(eulerPitch)*Math.Sin(eulerYaw)) - bodyPitch*(Math.Cos(eulerRoll)*Math.Cos(eulerYaw) + Math.Sin(eulerPitch)*Math.Sin(eulerRoll)*Math.Sin(eulerYaw)) - bodyYaw*Math.Cos(eulerPitch)*Math.Sin(eulerYaw)
            F[10,1] = -rho*Math.Pow(xd,2)/16840*Cl/mass*area/Ixx
            F[11,1] = -rho*Math.Pow(yd,2)/16840*Cm/mass*area/Iyy
            F[12,1] = -rho*Math.Pow(zd,2)/16840*Cn/mass*area/Izz
            F[10,4] = rho*xd*Cl/mass*area*dia/Ixx
            F[11,5] = rho*yd*Cm/mass*area*dia/Iyy
            F[12,6] = rho*zd*Cn/mass*area*dia/Izz
            F[10,10] = -(bodyYaw*(Izz-Iyy))/Ixx
            F[10,11] = -(bodyPitch*(Izz-Iyy))/Ixx
            F[11,11] = -(bodyYaw*(Ixx-Izz))/Iyy
            F[11,12] = -(bodyRoll*(Ixx-Izz))/Iyy
            F[12,10] = -(bodyPitch*(Iyy-Ixx))/Izz
            F[12,12] = -(bodyRoll*(Iyy-Ixx))/Izz
            F[10,16] = rho*Math.Pow(xd,2)*area*dia/Ixx/mass
            F[11,17] = rho*Math.Pow(yd,2)*area*dia/Iyy/mass
            F[12,18] = rho*Math.Pow(zd,2)*area*dia/Izz/mass
            I = Matrix[System.Double].Eye(18);

            Phi = Matrix[System.Double](18)
            Phi = I + F*SchedParameters.SimStepSeconds
            Q = Matrix[System.Double](18)
        
            # FIXME: Just pulled out of thin air
        
            Q[1,1] = 0.2
            Q[2,2] = 1.2
            Q[3,3] = 1.2
            Q[4,4] = .1
            Q[5,5] = .1
            Q[6,6] = .1
            Q[7,7] = .1
            Q[8,8] = .1
            Q[9,9] = .1
            Q[10,10] = .01
            Q[11,11] = .01
            Q[12,12] = .01
        
            Q[13,13] = .1 #Math.Pow(.05,2)
            Q[14,14] = .1 #Math.Pow(.05,2)
            Q[15,15] = .1 #Math.Pow(.05,2)
            Q[16,16] = .1 #Math.Pow(.1,2)
            Q[17,17] = .1 #Math.Pow(.1,2)
            Q[18,18] = .1 #Math.Pow(.1,2)
        
            #FIXME: Figure out if this necessary
            #Q = DiscreteQ( Q, SchedParameters.SimStepSeconds, F)

            H = Matrix[System.Double](18)
            H[1,1] = 1
            H[4,4] = SchedParameters.SimStepSeconds # The acceleration is measured, v = a*t
            H[5,5] = SchedParameters.SimStepSeconds
            H[6,6] = SchedParameters.SimStepSeconds

            H[10,10] = 1
            H[11,11] = 1
            H[12,12] = 1
        
        
            R =Matrix[System.Double].Eye(18)*.0001
            R[1,1] = 0.1
            R[4,4] = 0.1
            R[5,5] = 0.1
            R[6,6] = 0.1

            R[10,10] = 1e8
            R[11,11] = 100
            R[12,12] = 100

            M = Phi*Pk_Previous*Matrix[System.Double].Transpose(Phi) + Q 
            K = M*Matrix[System.Double].Transpose(H)*Matrix[System.Double].Inverse((H*M*Matrix[System.Double].Transpose(H)+R));

            Pk = (Matrix[System.Double].Eye(18) - K*H)*M
        
            measure = Vector(18)
            measurements = self.DependencyCollector(event)
            measure[4] = measurements[1]*9.81
            measure[5] = measurements[2]*9.81
            measure[6] = measurements[3]*9.81
            measure[10] = measurements[4]*Math.PI/180
            measure[11] = measurements[5]*Math.PI/180
            measure[12] = measurements[6]*Math.PI/180
            measure[1] = measurements[7]

            
        
            y = self.Propagate(state, .01, ts) #Fixme: Make not self, pass things in
            res = measure-H*y
            #print res
            eststate = y + K*(res)
            self._newState.AddValue(self.STATE_KEY, HSFProfile[Matrix[System.Double]](ts, eststate))
            self._newState.AddValue(self.KALMAN_KEY, HSFProfile[Matrix[System.Double]](ts, Pk))
            prof1 = HSFProfile[Matrix[System.Double]]()
            prof1[ts] = eststate
            #print self.state, stated
            return prof1
        self._newState.AddValue(self.STATE_KEY, HSFProfile[Matrix[System.Double]](ts, Vector(18)))
        return HSFProfile[Matrix[System.Double]](ts, Vector(18))
    def RECOVERYSUB_State_STATESUB(self, event):
        ts = event.GetEventStart(self.asset)
        prof1 = HSFProfile[Matrix[System.Double]]()
        prof1[ts] = self._newstate.GetLastValue()
        return prof1
    def Propagate(self, y, ts, t):
        

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
        Ixx = 0.33
        Iyy = 45
        Izz = 45

        step = 0
        dt = ts
        #dt = .02

        while(step<SchedParameters.SimStepSeconds):
            #Rename angles to aid 
            eulerRoll = y[7]
            eulerPitch = y[8]
            eulerYaw = y[9]
            bodyRoll = y[10]
            bodyPitch = y[11]
            bodyYaw = y[12]
            dcm = CreateRotationMatrix(eulerPitch, eulerYaw, eulerRoll)
            #Estimate the gravity vector position
            g = 9.81
            G = Vector(3)
            G[1] = -9.81
            G = Matrix[System.Double].Transpose(dcm)*G 
            #G[1] = -g*Math.Cos(eulerPitch)*Math.Cos(eulerYaw)
            #G[2] = g*(Math.Cos(eulerRoll)*Math.Sin(eulerPitch) - Math.Cos(eulerPitch)*Math.Sin(eulerRoll)*Math.Sin(eulerYaw))
            #G[3] = -g*(Math.Sin(eulerRoll)*Math.Sin(eulerPitch) + Math.Cos(eulerRoll)*Math.Cos(eulerPitch)*Math.Sin(eulerYaw))
        
            #Estimate dynamic pressure
            rho = 1.225*Math.Exp(-y[1]/8420)
        
            dy = Vector(18)
            #Position is the velocity
            dy[1] = y[4]
            dy[2] = y[5]
            dy[3] = y[6]
        
            #Sum the forces/mass to get the accelerations
            a = Vector(3)
            Qp = 0.5*rho*Math.Pow(y[4],2) 
            a[1] = (G[1] + (thrust/mass) - Math.Sign(y[4])*Qp*A/mass*y[13])
            Qp = 0.5*rho*Math.Pow(y[5],2)
            a[2] = (G[2] - Math.Sign(y[5])*Qp*A/mass*y[14])
            Qp = 0.5*rho*Math.Pow(y[6],2)
            a[3] = (G[3] - Math.Sign(y[6])*Qp*A/mass*y[15])
            a = dcm * a
            dy[4] = a[1]
            dy[5] = a[2]
            dy[6] = a[3]
            # Use body angles to get the euler rates
            eulerRates = Vector(3)
            eulerRates = dcm*y[MatrixIndex(10,12)]
            dy[7] = eulerRates[1]
            dy[8] = eulerRates[2]
            dy[9] = eulerRates[3]

            m = Vector(3)
            # Use ang momentum eqns to calculate body rates  
            Qp = 0.5*rho*Math.Pow(y[4],2)       
            m[1] = ((Qp*A/mass*y[16]*D - (Izz - Iyy)*bodyPitch*bodyYaw)/Ixx)
            Qp = 0.5*rho*Math.Pow(y[5],2) 
            m[2] = (((Qp*A/mass*y[17]*D ) - (Ixx - Izz)*bodyRoll*bodyYaw)/Iyy)
            Qp = 0.5*rho*Math.Pow(y[6],2) 
            m[3] = (((Qp*A/mass*y[18]*D) - (Iyy - Ixx)*bodyRoll*bodyPitch)/Izz)

            dy[10] = m[1]
            dy[11] = m[2]
            dy[12] = m[3]
            # Propagate forward to the next time step
                  
            y = y + dy*dt
            step = step + ts
        return y
         
def DiscreteQ( Q, Ts, a):
        # Adapted from Matlabs kalmd()

        Nx = a.NumRows
        M = Matrix[System.Double].Vertcat(Matrix[System.Double].Horzcat(-a, Q), Matrix[System.Double].Horzcat(Matrix[System.Double](Nx), Matrix[System.Double].Transpose(a)))
        eulerRoll = Matrix[System.Double].exp(M*Ts);
        phi12 = eulerRoll[MatrixIndex(1,Nx),MatrixIndex(Nx+1,2*Nx)]
        phi22 = eulerRoll[MatrixIndex(Nx+1,2*Nx), MatrixIndex(Nx+1,2*Nx)]
        Qd = Matrix[System.Double].Transpose(phi22)*phi12
        Qd = (Qd+Matrix[System.Double].Transpose(Qd))/2; # Make sure Qd is symmetric
        return Qd
def CreateRotationMatrix(eulerPitch, eulerYaw, eulerRoll):
    # Returns the Body to Inertial dcm using a 3-2-1 Euler sequence
    # eulerPitch = rotation about z (1)
    # eulerYaw = rotation about y (2)
    # eulerRoll = rotation about x (3)

    # Pre-do all of the trig 
    cp = Math.Cos(eulerPitch)
    cq = Math.Cos(eulerYaw)
    cr = Math.Cos(eulerRoll)

    sp = Math.Sin(eulerPitch)
    sq = Math.Sin(eulerYaw)
    sr = Math.Sin(eulerRoll)

    C1 = Matrix[System.Double](3)
    C2 = Matrix[System.Double](3)
    C3 = Matrix[System.Double](3)
    C1[1,1] = cq
    C1[1,2] = sq
    C1[2,1] = -sq
    C1[2,2] = cq
    C1[3,3] = 1

    C2[2,2] = 1
    C2[1,1] = cp
    C2[3,1] = sp
    C2[1,3] = -sp
    C2[3,3] = cp

    C3[1,1] = 1
    C3[2,2] = cr
    C3[2,3] = sr
    C3[3,2] = -sr
    C3[3,3] = cr

    return Matrix[System.Double].Transpose(C3*C2*C1)

