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
#import Horizon
import Utilities
import HSFUniverse
import math
import MissionElements
import UserModel
import Aerodynamics
#import csv

from UserModel import XmlParser
from MissionElements import Asset
#from Horizon import Program
from Utilities import *
from HSFUniverse import *
from System.Collections.Generic import Dictionary
from System import Array
from System import Xml
from IronPython.Compiler import CallTarget0

class eomRocket(Utilities.EOMS):
    
    def __init__(self, scriptedNode):
        # Aerodynamics
        self.aero = Aerodynamics.Aerodynamics()
        self.printOnce = True
        #Mass Properties
        # Assume a constant Inertia Matrix for now
        self.Ixx = float(scriptedNode["MassProp"].Attributes["Ixx"].Value)
        self.Iyy = float(scriptedNode["MassProp"].Attributes["Iyy"].Value)
        self.Izz = float(scriptedNode["MassProp"].Attributes["Izz"].Value)

        # Assume a linear mass loss
        self.InitMass = float(scriptedNode["MassProp"].Attributes["InitMass"].Value)
        self.FinalMass = float(scriptedNode["MassProp"].Attributes["FinalMass"].Value)

        # Propulsion
        self.Thrust = float(scriptedNode["Propulsion"].Attributes["Thrust"].Value)
        self.BurnTime = float(scriptedNode["Propulsion"].Attributes["BurnTime"].Value)
        self.thrustData = []
        text_file = open("C:\Users\steve\Resilio Sync\Documents\MATLAB\Thesis\AeroTech_L952.txt", "r")
        for line in text_file:
            lines = [float(elt.strip()) for elt in line.split('\t')]
            self.thrustData.append(lines)
        text_file.close()
        # Atmosphere
        if("Standard" == scriptedNode["Atmosphere"].Attributes["Type"].Value):
            self.atmos = StandardAtmosphere()
        elif("RealTime" == scriptedNode["Atmosphere"].Attributes["Type"].Value):
            self.atmos = RealTimeAtmosphere()
            self.atmos.filePath = "C:\\Horizon\\041517\\gfs.t18z.pgrb2.0p50.f003.grb2"
            lat = float(scriptedNode["Atmosphere"].Attributes["Latitude"].Value)
            long = float(scriptedNode["Atmosphere"].Attributes["Longitude"].Value)
            self.atmos.SetLocation(lat, long)
        else:
            Exception;
        self.atmos.CreateAtmosphere()

        # Physical Properties
        self.groundlevel = float(scriptedNode.Attributes["Ground"].Value)
        self.lengthRef = float(scriptedNode.Attributes["ReferenceLength"].Value)
        self.areaRef = float(scriptedNode.Attributes["ReferenceArea"].Value)

        # Intgrator Parameter Keys
        self.DROGUE_DEPLOYED = StateVarKey[System.Boolean]("asset1.drogue")
        self.MAIN_DEPLOYED = StateVarKey[System.Boolean]("asset1.main")
        self.ACCELERATION = StateVarKey[Vector]("asset1.accel")
        self.GYROSCOPE = StateVarKey[Vector]("asset1.gyro")
        self.dcx= StateVarKey[System.Double]("asset1.dcx");
        self.dcy = StateVarKey[System.Double]("asset1.dcy")
        self.dcz = StateVarKey[System.Double]("asset1.dcz")
    def PythonAccessor(self, t, y, param):
        # X -> Through the nose
        # p = roll, q = pitch, r = yaw
        # pitch about y, yaw about z, roll about x

        _mu = 3.986e14
        r3 = System.Math.Pow(Matrix[System.Double].Norm(y[MatrixIndex(1, 3), 1]), 3)
        mur3 = -_mu / r3
        if y[1,1] < self.groundlevel:
            y[1,1] = self.groundlevel #Can't be below ground
        if t < self.BurnTime:
            mass = self.InitMass - (self.InitMass - self.FinalMass)*(t/self.BurnTime)
        else:
            mass = self.FinalMass
        thrust = self.ThrustCalculation(t, mass)
        alt = y[1,1] - self.groundlevel
        vel = self.GetRelativeVelocity(alt, y)
        velB = self.GetBodyVelocity(y, vel)
        mach = Vector.Norm(vel)/343 #TODO: Calculate spdofsnd
        G = Vector(3)
        G[1] = -9.81
        G = self.dcm*G
        aero = self.aero.CurrentAero(mach)
       # print mach
        self.Cx = aero[0] +.1
        self.Cy = aero[1] 
        self.Cz = aero[2]
        self.Cm = aero[3]
        self.Cl = aero[4]
        self.Cn = aero[5]
        if param.GetValue(self.DROGUE_DEPLOYED):
            self.Cx = 0.62
            self.areaRef = 0.4022702
            self.Cy = 0.0
            self.Cz = 0.0
            self.Cm = 0.0
            self.Cl = 0.0
            self.cn = 0.0
        if param.GetValue(self.MAIN_DEPLOYED):
            self.Cx = 1.4
            self.areaRef = 4.1202498 #6.68902 + 0.4022702
            self.Cy = 0.0
            self.Cz = 0.0
            self.Cm = 0.0
            self.Cl = 0.0
            self.cn = 0.0

        forces = self.ForceCalculation(alt,velB, mass)
        self.Cx += param.GetValue(self.dcx)
        self.Cy += param.GetValue(self.dcy)
        self.Cz += param.GetValue(self.dcz)
        forceControl = self.ForceCalculation(alt, velB, mass)
        forceControl[0] -= forces[0]
        forceControl[1] -= forces[1]
        forceControl[2] -= forces[2]
        moments = self.MomentCalculations(alt, velB)
        #print forceControl, moments
        psi = y[7,1]
        theta = y[8,1]
        phi = y[9,1]
        p = y[10,1]
        q = y[11,1]
        r = y[12,1]  
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
            pdot = 0.0
            qdot = 0.0
            rdot = 0.0
            psidot = 0.0
            thetadot = 0.0
            phidot = 0.0
        else:
            #The position of the rocket is simply the velocity integrated
            velInertial = vel * Matrix[System.Double].Transpose(self.dcm)
            
            vx = velInertial[1]
            vy = velInertial[2]
            vz = velInertial[3]
            #vx = y[4,1]
            #vy = y[5,1] 
            #vz = y[6,1]
            #The velocity of the rocket. Uses the orbital motion eqns for gravity
            acc = Vector(3)
            acc[1] = (G[1]*mass + (thrust - math.copysign(forces[0], y[4,1])))/mass
            acc[2] = (G[2]*mass - (math.copysign(forces[1], y[5,1])))/mass #TODO: Make sign correct
            acc[3] = (G[3]*mass - (math.copysign(forces[2], y[6,1])))/mass
            accInertial = acc * (self.dcm) # Use Body to Inertial DCM
            ax = accInertial[1]
            ay = accInertial[2]
            az = accInertial[3]
            pdot = ((moments[0] + forceControl[0]*self.lengthRef) - (self.Izz - self.Iyy)*q*r)/self.Ixx
            qdot = ((moments[1] + (forces[1])*1.5*self.lengthRef  + forceControl[1]*1) - (self.Ixx - self.Izz)*p*r)/self.Iyy #Fixme: Find canard to CG distance
            rdot = ((moments[2] + (forces[2])*1.5*self.lengthRef  + forceControl[2]*1) - (self.Iyy - self.Ixx)*p*q)/self.Izz
            psidot = (q*math.sin(phi) + r*math.cos(phi))/math.cos(theta)
            thetadot = q*math.cos(phi) - r*math.sin(phi)
            phidot = p + psidot*math.sin(theta)
        #print moments, forces[1]*1.5*self.areaRef, forces[2]*1.5*self.areaRef
        #print self.dcm
        accel = Vector(3)
        gyro = Vector(3)
        accel[1] = ax
        accel[2] = ay
        accel[3] = az
        gyro[1] = pdot
        gyro[2] = qdot
        gyro[3] = rdot
        param.Add(self.ACCELERATION, accel)
        param.Add(self.GYROSCOPE, gyro)
        dy[1,1] = vx
        dy[2,1] = vy
        dy[3,1] = vz
        dy[4,1] = ax
        dy[5,1] = ay
        dy[6,1] = az
        dy[7,1] = psidot
        dy[8,1] = thetadot
        dy[9,1] = phidot
        dy[10,1] = pdot
        dy[11,1] = qdot
        dy[12,1] = rdot
        
        return dy 
    def ForceCalculation(self, alt, vel, mass):
        # Currently assume 0 alpha for everything
        dens = self.atmos.density(alt)
        #print mass, vel[1]
        Fx = (0.5 * dens * math.pow(vel[1],2) * (self.Cx) * self.areaRef) # kg/m/s^2*m^2/ = [N]
        Fy = (0.5 * dens * math.pow(vel[2],2) * (self.Cy) * self.areaRef) # kg/m/s^2*m^2/ = [N]
        Fz = (0.5 * dens * math.pow(vel[3],2) * (self.Cz) * self.areaRef) # kg/m/s^2*m^2/ = [N]
        return [Fx, Fy, Fz]
    def MomentCalculations(self, alt, vel):
        # Currently assume 0 alpha for everything
        #area = math.pow(.1106,2) * math.pi #Reference area is the cross sectional area
        dynamicPressure = 0.5 * self.atmos.density(alt)*math.pow(vel[1],2) # kg/m^3 * m^2/s^2 = [N/m^2]
        Mx = self.Cl*dynamicPressure*self.areaRef                          # N/m^2 * m^2 = [N]  
        #area = self.lengthRef * .1106;
        dynamicPressure = 0.5 * self.atmos.density(alt)*math.pow(vel[2],2)
        My = self.Cm*dynamicPressure*self.areaRef
        dynamicPressure = 0.5 * self.atmos.density(alt)*math.pow(vel[3],2)
        Mz = self.Cn*dynamicPressure*self.areaRef

        return [Mx, My, Mz]
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
        #print thrust
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
        
        psi = y[7,1]
        theta = y[8,1]
        phi = y[9,1]

        self.dcm = self.CreateRotationMatrix(psi, theta, phi) 

        return self.dcm*vel # Use the inertial to body dcm to get velcity in body frame

    def CreateRotationMatrix(self, psi, tht, phi):
        # Returns the Body to Inertial dcm using a 3-2-1 Euler sequence
        # psi = rotation about z (1)
        # tht = rotation about y (2)
        # phi = rotation about x (3)

        # Pre-do all of the trig 
        cp = math.cos(psi)
        cq = math.cos(tht)
        cr = math.cos(phi)

        sp = math.sin(psi)
        sq = math.sin(tht)
        sr = math.sin(phi)



        C1 = Matrix[System.Double](3)
        C2 = Matrix[System.Double](3)
        C3 = Matrix[System.Double](3)
        C1[1,1] = cp
        C1[1,2] = sp
        C1[2,1] = -sp
        C1[2,2] = cp
        C1[3,3] = 1

        C2[2,2] = 1
        C2[1,1] = cq
        C2[3,1] = sq
        C2[1,3] = -sq
        C2[3,3] = cq

        C3[1,1] = 1
        C3[2,2] = cr
        C3[2,3] = sr
        C3[3,2] = -sr
        C3[3,3] = cr

        return Matrix[System.Double].Transpose(C3*C2* C1)
        
        '''
        C = Matrix[System.Double](3)   
        C[1,1] = cp*cq
        C[2,1] = cp*sq*sr -sp*cr
        C[3,1] = cp*sq*cr+sp*sr
        C[1,2] = sp*cq
        C[2,2] = cp*cr-sp*sq*sr
        C[3,2] = sp*sq*cr-cp*sr
        C[1,3] = -sq
        C[2,3] = cq*sr
        C[3,3] = cq*cr
        
        return Matrix[System.Double].Transpose(C)
        '''
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