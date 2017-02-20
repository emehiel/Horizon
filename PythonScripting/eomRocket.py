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
import Horizon
import Utilities
import HSFUniverse
import math
import MissionElements
from MissionElements import Asset
from Horizon import Program
from Utilities import *
from HSFUniverse import *
from System.Collections.Generic import Dictionary
from System import Array
from IronPython.Compiler import CallTarget0

class eomRocket(Utilities.EOMS):
    def __init__(self):
        self.atmos = StandardAtmosphere()
        #self.atmos.filePath = "C:\\Horizon\\gfs.t06z.pgrb2.0p50.f006.grb2"
        self.atmos.CreateAtmosphere()
        self.HSFinput = Horizon.Program()
        input = Array[str](['-s', '..\\..\\..\\SimulationInput_RocketScripted.xml', '-m',  '..\\..\\..\\Model_Scripted_RocketEOMICs.xml'])
        self.HSFinput.InitInput(input)
        self.HSFinput.LoadSubsystems()
        self.groundlevel = self.HSFinput.assetList[0].AssetDynamicState.InitialConditions()[3]
        print self.groundlevel
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
            
        dy = Matrix[System.Double]()
        dy = translation
        if (y[3,1] + dy[3,1] < self.groundlevel):
            print t
            dy[3,1] = abs(y[3,1] - self.groundlevel)
            
        return dy 
    def DragCalculation(self, y):
        Cd = 0.5 #Approximate Cd of a rocket
        area = math.pow(.2214,2) * math.pi #Reference area is the cross sectional area
        dynamicPressure = 0.5 * self.atmos.density((y[3,1]-6378)*1000) * math.pow(y[6,1]*1000,2)
        return (dynamicPressure * Cd * area/ 68)/1000
       
    def ThrustCalculation(self, t):
        # Return the average thrust in Newtons for the entire burn time
        # Not accurate but will give the approximate solution 
        if t < 18: #Burn for 18 seconds
            return 6*9.81/1000 # 6g thrust
        else:
            return 0.0    



        
