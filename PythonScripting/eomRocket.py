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
    #Cd = 0.0
    def __new__(cls, scriptedNode):
        obj = Utilities.ScriptedEOMS.__new__(cls)
        cls.Cd = float(scriptedNode.Attributes["Cd"].Value)
        cls.groundlevel =float(scriptedNode.Attributes["Ground"].Value)
        return obj
    def __init__(self, scriptedNode):
        self.atmos = StandardAtmosphere()
        #self.atmos.filePath = "C:\\Horizon\\gfs.t06z.pgrb2.0p50.f006.grb2"
        self.atmos.CreateAtmosphere()
    def PythonAccessor(self, t, y):
        _mu = 398600
        r3 = System.Math.Pow(Matrix[System.Double].Norm(y[MatrixIndex(1, 3), 1]), 3)
        mur3 = -_mu / r3
        if y[3,1] < self.groundlevel:
            y[3,1] = self.groundlevel #Can't be below ground
        thrust = self.ThrustCalculation(t)
        drag = self.DragCalculation(y)  
        translation = Matrix[System.Double]()
        #Set the velocity equal to 0 once on the ground
        if y[3,1] < self.groundlevel:
            #The position of the rocket is constant once on the ground
            translation[1,1] = 0.0    
            translation[2,1] = 0.0
            translation[3,1] = 0.0        
            translation[4,1] = 0.0
            translation[5,1] = 0.0 
            translation[6,1] = 0.0
        else:
            #The position of the rocket is simply the velocity integrated
            translation[1,1] = y[4,1]    
            translation[2,1] = y[5,1]
            translation[3,1] = y[6,1]
            #The velocity of the rocket. Uses the orbital motion eqns for gravity
            translation[4,1] = mur3 * y[1,1] 
            translation[5,1] = mur3 * y[2,1] 
            translation[6,1] = (mur3 * y[3,1]) + thrust - math.copysign(drag, y[6,1])  
        rotation = Matrix[System.Double]()    
        dy = Matrix[System.Double]()
        dy = translation
        return dy 
    def DragCalculation(self, y):
        #Cd = 0.4296 #Approximate Cd of a rocket at 45 m/s based on prelim CFD
        area = math.pow(.2214,2) * math.pi #Reference area is the cross sectional area
        dynamicPressure = 0.5 * self.atmos.density((y[3,1]-6378)*1000) * math.pow(y[6,1]*1000,2)
        return (dynamicPressure * self.Cd * area/ 68)/1000
    def MomentCalculations(self, y): 
            pass   
    def ThrustCalculation(self, t):
        # Return the average thrust in Newtons for the entire burn time
        # Not accurate but will give the approximate solution 
        if t < 18: #Burn for 18 seconds
            return 6*9.81/1000 # 6g thrust
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
    vq = v[index-1]+(above-below)*(xq-v[index-1])/(above-below)
    return vq



        
