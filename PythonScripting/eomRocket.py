import sys
import clr
import System.Collections.Generic
import System
from operator import abs
clr.AddReference('System.Core')
clr.AddReference('IronPython')
clr.AddReference('System.Xml')
clr.AddReferenceByName('Utilities')
clr.AddReferenceByName('HSFUniverse')
clr.AddReferenceByName('Horizon')
clr.AddReferenceByName('MissionElements')
clr.AddReferenceByName('UserModel')
import Horizon
import Utilities
import HSFUniverse
import math
import MissionElements
import UserModel
import Aerodynamics
#import csv

from UserModel import XmlParser
from MissionElements import Asset
from Horizon import Program
from Utilities import *
from HSFUniverse import *
from System.Collections.Generic import Dictionary
from System import Array
from System import Xml
from IronPython.Compiler import CallTarget0

class eomRocket(Utilities.EOMS):
    
    def __init__(self, scriptedNode):
        # Load individula aero coeffs for testing purposes
        self.aero = Aerodynamics.Aerodynamics();
        #self.Cx = float(scriptedNode["Aerodynamics"].Attributes["Cx"].Value)
        #self.Cy = float(scriptedNode["Aerodynamics"].Attributes["Cy"].Value)
        #self.Cz = float(scriptedNode["Aerodynamics"].Attributes["Cz"].Value)
        #self.Cl = float(scriptedNode["Aerodynamics"].Attributes["Cl"].Value)
        #self.Cm = float(scriptedNode["Aerodynamics"].Attributes["Cm"].Value)
        #self.Cn = float(scriptedNode["Aerodynamics"].Attributes["Cn"].Value)
        self.groundlevel = float(scriptedNode.Attributes["Ground"].Value)
        #print self.aero.CurrentAero(.5, 0);
        #self.aero = aero.__init__
        #print self.aero.CurrentAero(0.51)
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

        # Use the standard atmosphere model for now because it is much faster to load
        self.atmos = StandardAtmosphere()
        #self.atmos = RealTimeAtmosphere()
        #self.atmos.filePath = "C:\\Horizon\\gfs.t06z.pgrb2.0p50.f006.grb2"
        #lat = float(scriptedNode["Atmosphere"].Attributes["Latitude"].Value)
        #lon = float(scriptedNode["Atmosphere"].Attributes["Longitude"].Value)
        #lat = 33
        #long = -107
        #self.atmos.SetLocation(lat, long)
        self.atmos.CreateAtmosphere()
    def PythonAccessor(self, t, y):
        _mu = 398600
        r3 = System.Math.Pow(Matrix[System.Double].Norm(y[MatrixIndex(1, 3), 1]), 3)
        mur3 = -_mu / r3
        if y[1,1] < self.groundlevel:
            y[1,1] = self.groundlevel #Can't be below ground
        if t < self.BurnTime:
            mass = self.InitMass - (self.InitMass - self.FinalMass)*(t/self.BurnTime)
        else:
            mass = self.FinalMass
        thrust = self.ThrustCalculation(t, mass)
        mach = math.sqrt(math.pow(y[4,1],2) + math.pow(y[5,1],2) + math.pow(y[6,1],2))/340 #TODO: Calculate spdofsnd
        aero = self.aero.CurrentAero(mach)
        self.Cx = aero[0]
        self.Cy = aero[1]
        self.Cz = aero[2]
        self.Cm = aero[3]
        self.Cl = aero[4]
        self.Cn = aero[5]
        forces = self.ForceCalculation(y, mass)
        moments = self.MomentCalculations(y)
        p = y[7,1]
        q = y[8,1]
        r = y[9,1]  
        translation = Matrix[System.Double]()
        #Set the velocity equal to 0 once on the ground
        if y[1,1] < self.groundlevel:
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
        else:
            #The position of the rocket is simply the velocity integrated
            vx = y[4,1]    
            vy = y[5,1]
            vz = y[6,1]
            #The velocity of the rocket. Uses the orbital motion eqns for gravity
            ax = (mur3 * y[1,1]) + thrust - math.copysign(forces[0], y[4,1])
            ay = (mur3 * y[2,1]) - math.copysign(forces[1], y[5,1]) #TODO: Make sign correct
            az = (mur3 * y[3,1]) - math.copysign(forces[2], y[6,1])
            pdot = (moments[0] - (self.Izz - self.Iyy)*q*r)/self.Ixx
            qdot = (moments[1] - (self.Ixx - self.Izz)*p*r)/self.Iyy
            rdot = (moments[2] - (self.Iyy - self.Ixx)*p*q)/self.Izz
       
        dy = Matrix[System.Double]()
        
        dy[1,1] = vx
        dy[2,1] = vy
        dy[3,1] = vz
        dy[4,1] = ax
        dy[5,1] = ay
        dy[6,1] = az
        dy[7,1] = pdot
        dy[8,1] = qdot
        dy[9,1] = rdot
        
        return dy 
    def ForceCalculation(self, y, mass):
        # TODO: Frame Change for wind.
        alt = (y[1,1]-6378)*1000;
        dens = self.atmos.density(alt)
        Fx = (0.5 * dens * math.pow(y[4,1]*1000,2) * self.Cx *  math.pow(.1106,2) * math.pi)/mass/1000 # kg/m/s^2*m^2/kg = m/s^2/1000 -> [km/s^2]
        Fy = (0.5 * dens * math.pow(y[5,1]*1000 + self.atmos.uVelocity(alt),2) * self.Cy * .2302*4.5)/mass/1000 # kg/m/s^2*m^2/kg = m/s^2/1000 -> [km/s^2]
        Fz = (0.5 * dens * math.pow(y[6,1]*1000 + self.atmos.vVelocity(alt),2) * self.Cz * .2302*4.5)/mass/1000 # kg/m/s^2*m^2/kg = m/s^2/1000 -> [km/s^2]
        return [Fx, Fy, Fz]
    def MomentCalculations(self, y):
        area = math.pow(.1106,2) * math.pi #Reference area is the cross sectional area
        dynamicPressure = 0.5 * self.atmos.density((y[1,1]-6378)*1000) * self.atmos.uVelocity((y[1,1]-6378)*1000)
        Mx = self.Cm*dynamicPressure*area
        My = self.Cl*dynamicPressure*area
        Mz = self.Cn*dynamicPressure*area
        return [Mx, My, Mz]
    def ThrustCalculation(self, t, mass):
        # Return the average thrust in Newtons for the entire burn time
        # Not accurate but will give the approximate solution 
        if t < self.BurnTime:
            return self.Thrust/mass/1000 # kg*m/s^2/kg = m/s^2/100 -> [km/s^2]
        else:
            return 0.0 



        
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