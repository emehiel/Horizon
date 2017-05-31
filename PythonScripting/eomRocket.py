import sys
import clr
import System.Collections.Generic
import System

clr.AddReference('System.Core')
clr.AddReference('IronPython')
clr.AddReference('System.Xml')
clr.AddReferenceByName('Utilities')
clr.AddReferenceByName('HSFUniverse')
clr.AddReferenceByName('Horizon')
clr.AddReferenceByName('MissionElements')
clr.AddReferenceByName('UserModel')

import Utilities
import HSFUniverse
import MissionElements
import UserModel
import AeroPrediction

from UserModel import XmlParser
from MissionElements import Asset
from Utilities import *
from HSFUniverse import *
from System.Collections.Generic import Dictionary
from System import Array
from System import Xml, Math
from IronPython.Compiler import CallTarget0

class eomRocket(Utilities.EOMS):
    
    def __init__(self, scriptedNode):
        assetName = scriptedNode.ParentNode.ParentNode.Attributes["assetName"].Value
        # Aerodynamics
        self.drag = AeroPrediction.AeroPrediction(scriptedNode["Aerodynamics"]);
        self.printOnce = True #FIXME:
        #Mass Properties
        # Assume a linear change in Inertia Matrix 
        self.IxxInit = float(scriptedNode["MassProp"].Attributes["IxxInit"].Value)
        self.IyyInit = float(scriptedNode["MassProp"].Attributes["IyyInit"].Value)
        self.IzzInit = float(scriptedNode["MassProp"].Attributes["IzzInit"].Value)
        self.IxxFinal = float(scriptedNode["MassProp"].Attributes["IxxFinal"].Value)
        self.IyyFinal = float(scriptedNode["MassProp"].Attributes["IyyFinal"].Value)
        self.IzzFinal = float(scriptedNode["MassProp"].Attributes["IzzFinal"].Value)
        # Assume a linear mass loss and change in CG
        self.InitMass = float(scriptedNode["MassProp"].Attributes["InitMass"].Value)
        self.FinalMass = float(scriptedNode["MassProp"].Attributes["FinalMass"].Value)
        self.InitCG =  float(scriptedNode["MassProp"].Attributes["InitCG"].Value)
        self.FinalCG =  float(scriptedNode["MassProp"].Attributes["FinalCG"].Value)

        # Propulsion
        self.BurnTime = float(scriptedNode["Propulsion"].Attributes["BurnTime"].Value)
        if scriptedNode["Propulsion"].Attributes["Type"].Value == "Constant":
            Thrust = float(scriptedNode["Propulsion"].Attributes["Thrust"].Value)
            self.thrustData = []
            self.thrustData.append( [0, 0])
            self.thrustData.append( [.1, Thrust])
            self.thrustData.append( [self.BurnTime, Thrust])
            self.thrustData.append( [-1, 0])
            
        elif (scriptedNode["Propulsion"].Attributes["Type"].Value == "File"):
            self.thrustData = []
            text_file = open(scriptedNode["Propulsion"].Attributes["Filename"].Value, "r")
            for line in text_file:
                lines = [float(elt.strip()) for elt in line.split('\t')]
                self.thrustData.append(lines)
            text_file.close()
        # Atmosphere
        if("Standard" == scriptedNode["Atmosphere"].Attributes["Type"].Value):
            self.atmos = StandardAtmosphere()
        elif("RealTime" == scriptedNode["Atmosphere"].Attributes["Type"].Value):
            self.atmos = RealTimeAtmosphere()
            self.atmos.filePath = scriptedNode["Atmosphere"].Attributes["Filename"].Value
            lat = float(scriptedNode["Atmosphere"].Attributes["Latitude"].Value)
            long = float(scriptedNode["Atmosphere"].Attributes["Longitude"].Value)
            self.atmos.SetLocation(lat, long)
        else:
            Exception
        self.atmos.CreateAtmosphere()

        # Physical Properties
        self.groundlevel = float(scriptedNode.Attributes["Ground"].Value)
        self.lengthRef = float(scriptedNode.Attributes["ReferenceLength"].Value)
        self.areaRef = float(scriptedNode.Attributes["ReferenceArea"].Value)

        # Intgrator Parameter Keys
        self.DROGUE_DEPLOYED = StateVarKey[System.Boolean](assetName + "." + "drogue")
        self.MAIN_DEPLOYED = StateVarKey[System.Boolean](assetName + "." + "main")
        self.ACCELERATION = StateVarKey[Vector](assetName + "." + "accel")
        self.GYROSCOPE = StateVarKey[Vector](assetName + "." + "gyro")
        self.dcx= StateVarKey[System.Double](assetName + "." + "dcx");
        self.dcy = StateVarKey[System.Double](assetName + "." + "dcy")
        self.dcz = StateVarKey[System.Double](assetName + "." + "dcz")
        self.CX_KEY = StateVarKey[Matrix[System.Double]](assetName + "." + "cx")
        self.ALPHA_KEY = StateVarKey[System.Double](assetName + "." + "alpha")
        self.pressure = StateVarKey[System.Double](assetName + "." + "pressure")
        self.DEFLECTION_KEY = StateVarKey[Matrix[System.Double]](assetName + "." + "deflection")
    def PythonAccessor(self, t, y, param):
        # X -> Through the nose
        # bodyRollRate = bodyRollRate, bodyYawRate = bodyPitchRate, bodyYawRate = bodyYawRate
        # bodyPitchRate about y, bodyYawRate about z, bodyRollRate about x
        eulerRoll = y[7,1]
        eulerPitch = y[8,1]
        eulerYaw = y[9,1]
        bodyRollRate = y[10,1]
        bodyPitchRate = y[11,1]
        bodyYawRate = y[12,1]  

        if y[1,1] < self.groundlevel:
            print "Ground"
            return Matrix[System.Double](12, 1)
        if t < self.BurnTime:
            mass = self.InitMass - (self.InitMass - self.FinalMass)*(t/self.BurnTime)
            Ixx = self.IxxInit - (self.IxxInit - self.IxxFinal)*(t/self.BurnTime)
            Iyy = self.IyyInit - (self.IyyInit - self.IyyFinal)*(t/self.BurnTime)
            Izz = self.IzzInit - (self.IzzInit - self.IzzFinal)*(t/self.BurnTime)
            CG = self.InitCG - (self.InitCG - self.FinalCG)*(t/self.BurnTime)
        else:
            mass = self.FinalMass
            Ixx = self.IxxFinal
            Iyy = self.IyyFinal
            Izz = self.IzzFinal
            CG = self.FinalCG
        thrust = self.ThrustCalculation(t, mass)
        alt = y[1,1] - self.groundlevel
        vel = self.GetRelativeVelocity(alt, y)
        #velB = vel
        velB = self.GetBodyVelocity(y, vel)
        mach = Vector.Norm(vel)/343 #TODO: Calculate spdofsnd
        G = Vector(3)
        G[1] = -9.81
        G = Matrix[System.Double].Transpose(self.dcm)*G
        #vInfY = Math.Sqrt(Math.Pow(velB[1],2) + Math.Pow(velB[2],2))
        #vInfZ = Math.Sqrt(Math.Pow(velB[1],2) + Math.Pow(velB[3],2))
        #if vInfY != 0:
        #    alpha = Math.Acos(velB[1]/vInfY)
        #else:
        #    alpha = 0
        #if vInfZ != 0:
        #    beta = Math.Acos(velB[1]/vInfZ)
        #else:
        #    beta = 0
        alpha = Math.Atan2(velB[2],velB[1])
        beta = Math.Atan2(velB[3], velB[1])#Math.Sqrt(Math.Pow(velB[1],2) + Math.Pow(velB[2],2)))
        deflection = param.GetValue(self.DEFLECTION_KEY)
        #dcmAero =(CreateRotationMatrix(0,-alpha,beta))
        dcmAero = Matrix[System.Double].Eye(3)
        velA = dcmAero*velB
        aoa = Math.Acos(velB[1]/Vector.Norm(velB))
        self.Cx = self.drag.DragCoefficient(alt, velA);
        self.Cy = self.drag.NormalForceCoefficient(alt, velA, alpha, deflection)
        self.Cz = self.drag.SideForceCoefficient(alt, velA, beta, deflection)
        pitchDamping = self.drag.PitchDampingMoment(velA[2], bodyPitchRate, alpha)
        #bodyPitch = 1.1*self.lengthRef*3.86/self.areaRef*Math.Pow(Math.Sin(alpha),2)
        #finPitch = self.Cy*1.5*self.lengthRef
        self.Cm = self.drag.PitchingMoment(alt, velA, alpha, deflection, CG) - pitchDamping# FIXME
        self.Cl = 0*self.drag.RollingMoment(velA, bodyRollRate, alt, alpha, beta, deflection)
        yawDamping = self.drag.PitchDampingMoment(velA[3], bodyYawRate, beta)
        #bodyYaw = 1.1*self.lengthRef*3.86/self.areaRef*Math.Pow(Math.Sin(beta),2)
        #finYaw = self.Cz*1.5*self.lengthRef
        self.Cn = self.drag.YawingMoment(alt, velA, beta, deflection, CG) - yawDamping
        
        if param.GetValue(self.DROGUE_DEPLOYED):
            self.Cx = 0.62
            self.areaRef = 0.88
            self.Cy = 0.0
            self.Cz = 0.0
            self.Cm = 0.0
            self.Cl = 0.0
            self.Cn = 0.0
            bodyRollRate = 0.0
            bodyYawRate = 0.0
            bodyPitchRate = 0.0
            y[5,1] = self.atmos.uVelocity(alt)
            y[6,1] = self.atmos.vVelocity(alt)
            velB = y[MatrixIndex(4,6),1]
            velA = velB
            self.dcm = Matrix[System.Double].Eye(3)
            dcmAero = Matrix[System.Double].Eye(3)
            G = Vector(3)
            G[1] = -9.81
        if param.GetValue(self.MAIN_DEPLOYED):
            self.Cx = 1.4
            self.areaRef = 4.1202498 #6.68902 + 0.4022702
            self.Cy = 0.0
            self.Cz = 0.0
            self.Cm = 0.0
            self.Cl = 0.0
            self.Cn = 0.0
            y[5,1] = self.atmos.uVelocity(alt)
            y[6,1] = self.atmos.vVelocity(alt)
            G = Vector(3)
            G[1] = -9.81
        if alt < 12: # On Launch rail
            self.Cy = 0.0
            self.Cz = 0.0
            self.Cm = 0.0
            self.Cl = 0.0
            self.Cn = 0.0
        else:
            mtemp = 0
        forces = self.ForceCalculation(alt, velA)
        moments =  self.MomentCalculations(alt, velA)
        
        #dcmAero = CreateRotationMatrix(0,-alpha,beta)
        #print self.atmos.vVelocity(alt), alpha*180/Math.PI, self.Cm, moments[2]
        dy = Matrix[System.Double](12, 1)
        #Set the velocity equal to 0 once on the ground
        if y[1,1] <= self.groundlevel:
            #The position of the rocket is constant once on the ground
            vx = 0.0    
            vy = 0.0
            vz = 0.0        
            ax = 0.0
            ay = 0.0 
            az = 0.0
            bodyRollRateDot = 0.0
            bodyPitchRateDot = 0.0
            bodyYawRateDot = 0.0
            psidot = 0.0
            thetadot = 0.0
            phidot = 0.0
        else:
            #The position of the rocket is simply the velocity integrated
            #velInertial = Matrix[System.Double].Transpose(self.dcm) * vel
            
            vx = y[4,1] #velInertial[1]
            vy = y[5,1] #velInertial[2]
            vz = y[6,1] #velInertial[3]

            #The velocity of the rocket. Uses the orbital motion eqns for gravity
            acc = Vector(3)
            #forces = Matrix[System.Double].Transpose(dcmAero)*forces
            acc[1] = (G[1]*mass - Math.Sign(velB[1])*abs(forces[1]) + thrust)/mass
            acc[2] = (G[2]*mass - Math.Sign(velB[2])*abs(forces[2]))/mass 
            acc[3] = (G[3]*mass - Math.Sign(velB[3])*abs(forces[3]))/mass
            accInertial = (self.dcm) * acc  # Use Body to Inertial DCM
            #accInertial = acc
            ax = accInertial[1]
            ay = accInertial[2]
            az = accInertial[3]
            #moments =  Matrix[System.Double].Transpose(dcmAero)*moments
            bodyRollRateDot = (moments[1] - (Izz - Iyy)*bodyPitchRate*bodyYawRate)/Ixx
            bodyPitchRateDot = (-moments[2] - (Ixx - Izz)*bodyRollRate*bodyYawRate)/Iyy 
            bodyYawRateDot = (moments[3] - (Iyy - Ixx)*bodyRollRate*bodyPitchRate)/Izz
            bodyRates = Vector(3)
            bodyRates[1] = bodyRollRate
            bodyRates[2] = bodyPitchRate
            bodyRates[3] = bodyYawRate
            eulerRates = Vector(3)
            #eulerRates = self.dcm*bodyRates
            eulerRates[2] = (bodyPitchRate*Math.Sin(eulerRoll)+bodyYawRate*Math.Cos(eulerRoll))/Math.Cos(eulerPitch)
            eulerRates[3] = bodyPitchRate*Math.Cos(eulerRoll)-bodyYawRate*Math.Sin(eulerRoll)
            eulerRates[1] = bodyRollRate + eulerRates[1]*Math.Sin(eulerPitch)
            #eulerRates = bodyRates
        #print alpha*180/Math.PI, moments[2], bodyPitchRateDot, bodyRates[2], eulerRates[2], eulerPitch #, beta*180/Math.PI, moments[3], bodyYawRateDot
        accel = Vector(3)
        gyro = bodyRates
        accel[1] = ax
        accel[2] = ay
        accel[3] = az

        #print moments, eulerPitch, eulerYaw, eulerRoll
        cxTemp = Matrix[System.Double](1,2)
        cxTemp[1,1] = mach
        cxTemp[1,2] = self.Cx
        param.Add(self.CX_KEY, cxTemp)
        param.Add(self.ALPHA_KEY, alpha*180/Math.PI)
        param.Add(self.ACCELERATION, acc)
        param.Add(self.GYROSCOPE, gyro)
        press = self.atmos.pressure(alt)
        param.Add(self.pressure, press)

        dy[1,1] = vx
        dy[2,1] = vy
        dy[3,1] = vz
        dy[4,1] = ax
        dy[5,1] = ay
        dy[6,1] = az
        dy[7,1] = eulerRates[1]
        dy[8,1] = eulerRates[2]
        dy[9,1] = eulerRates[3]
        dy[10,1] = bodyRollRateDot
        dy[11,1] = bodyPitchRateDot
        dy[12,1] = bodyYawRateDot
        
        return dy 
    def ForceCalculation(self, alt, vel):

        dens = self.atmos.density(alt)
        F = Vector(3)
        F[1] = (0.5 * dens * Math.Pow(vel[1],2) * (self.Cx) * self.areaRef) # kg/m/s^2*m^2/ = [N]
        F[2] = (0.5 * dens * Math.Pow(vel[2],2) * (self.Cy) * self.areaRef) # kg/m/s^2*m^2/ = [N]
        F[3] = (0.5 * dens * Math.Pow(vel[3],2) * (self.Cz) * self.areaRef) # kg/m/s^2*m^2/ = [N]
        

        return F
    def MomentCalculations(self, alt, vel):
        M = Vector(3)
        dynamicPressure = 0.5 * self.atmos.density(alt)*Math.Pow(vel[1],2) # kg/m^3 * m^2/s^2 = [N/m^2]
        M[1] = self.Cl*dynamicPressure*self.areaRef
        dynamicPressure = 0.5 * self.atmos.density(alt)*Math.Pow(vel[2],2)
        M[2] = -self.Cm*dynamicPressure*self.areaRef
        dynamicPressure = 0.5 * self.atmos.density(alt)*Math.Pow(vel[3],2)
        M[3] = -self.Cn*dynamicPressure*self.areaRef

        return M
    def ThrustCalculation(self, t, mass):
        for datapoint in self.thrustData:
            if datapoint[0] == -1:
                if self.printOnce:
                    print "Motor burnout at time ", t
                    self.printOnce = False
                return 0.0
            if datapoint[0] > t:
                thrust = datapoint[1]
                break
        return thrust # kg*m/s^2/kg = [m/s^2]
        
    def GetRelativeVelocity(self, alt, y):
        # Rename variables for later
        u = self.atmos.uVelocity(alt)
        v = self.atmos.vVelocity(alt)

        vel = Vector(3)
        vel[1] = y[4,1]
        vel[2] = y[5,1]
        vel[3] = y[6,1]

        vel[2] = vel[2] + u
        vel[3] = vel[3] + v
   
        return vel

    def GetBodyVelocity(self, y, vel):
        
        eulerRoll = y[7,1]
        eulerPitch= y[8,1]
        eulerYaw = y[9,1]

        self.dcm = CreateRotationMatrix(eulerRoll, eulerPitch, eulerYaw) 

        return Matrix[System.Double].Transpose(self.dcm)*vel # Use the inertial to body dcm to get velcity in body frame

def CreateRotationMatrix(euler1, euler2, euler3):
    # Returns the Body to Inertial dcm using a 3-2-1 Euler sequence
    # eulerPitch = rotation about z (1)
    # eulerYaw = rotation about y (2)
    # eulerRoll = rotation about x (3)

    # Pre-do all of the trig 
    c1 = Math.Cos(euler1)
    c2 = Math.Cos(euler2)
    c3 = Math.Cos(euler3)

    s1 = Math.Sin(euler1)
    s2 = Math.Sin(euler2)
    s3 = Math.Sin(euler3)

    C1 = Matrix[System.Double](3)
    C2 = Matrix[System.Double](3)
    C3 = Matrix[System.Double](3)
    C3[1,1] = c3
    C3[1,2] = s3
    C3[2,1] = -s3
    C3[2,2] = c3
    C3[3,3] = 1

    C2[2,2] = 1
    C2[1,1] = c2
    C2[3,1] = s2
    C2[1,3] = -s2
    C2[3,3] = c2

    C1[1,1] = 1
    C1[2,2] = c1
    C1[2,3] = s1
    C1[3,2] = -s1
    C1[3,3] = c1

    return  Matrix[System.Double].Transpose(C3*C2*C1)
        
def LinearInterpolate(x, v, xq):
    if len(x) != len(v):
        Exception
    for index in range(len(x)):
        if xq < x[index]:
            below = x[index-1]
            above = x[index]
            break
        if index == len(x): #Clip at the highest value
            return v[index]
    vq = v[index-1]+(xq-below)*(v[index]-v[index-1])/(above-below)
    return vq