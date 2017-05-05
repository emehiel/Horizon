import sys
import clr
import System.Collections.Generic
import System
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
import AeroPrediction
from HSFSubsystem import *
from HSFSystem import *
from System.Xml import XmlNode
from Utilities import *
from HSFUniverse import *
from UserModel import *
from MissionElements import *
from System import Func, Delegate, Math
from System.Collections.Generic import Dictionary
from IronPython.Compiler import CallTarget0

class Controller(Subsystem):
    def __init__(self, node, asset):
        self.Asset = asset
        self.aero = AeroPrediction.AeroPrediction()
        self.DELTA_CX= StateVarKey[System.Double](asset.Name + "." + "dcx");
        self.DELTA_CY= StateVarKey[System.Double](asset.Name + "." + "dcy");
        self.DELTA_CZ = StateVarKey[System.Double](asset.Name + "." + "dcz");
        self.DEFLECTION_KEY = StateVarKey[Matrix[System.Double]](asset.Name + "." + "deflection")
        self.addKey(self.DELTA_CY)
        self.addKey(self.DELTA_CZ)
        self.addKey(self.DEFLECTION_KEY)
        self.Asset.AssetDynamicState.IntegratorParameters.Add(self.DELTA_CY, 0)
        self.Asset.AssetDynamicState.IntegratorParameters.Add(self.DELTA_CZ, 0)
        self.A = 0.510458340443047
        self.B = -0.489541659556953
        self.deltaDeflection = 50*.02 * Math.PI/180 # 200 deg/s * Time Step -> radians
        self.deflectionOld = Matrix[System.Double](4,1)
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        return dep
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def CanPerform(self, event, universe):
        #return super(subsystem, self).CanPerform(event, universe)
        spdOfSnd = 343 #Fixme: Calculate spdofsnd
        cx = 0
        state = self.DependencyCollector(event).LastValue()
        state = Matrix[System.Double].Transpose(state)
        controlState = state[MatrixIndex(7,12), 1]

        kest = Matrix[System.Double](4,6)
        ''' 
        kest[1,1] = -1.0351
        kest[2,1] = 1.1795
        kest[3,1] = -1.0351
        kest[4,1] = 1.1795
        
        kest[1,2] = 1.1761
        kest[2,2] = -1.0371
        kest[3,2] = 1.1761
        kest[4,2] = -1.0371

        kest[1,3] = 1.5098
        kest[2,3] = 1.5149
        kest[3,3] = 1.5098
        kest[4,3] = 1.5149
        
        
        kest[1,4] = 1.5310
        kest[2,4] = -4.3951
        kest[3,4] = 1.5310
        kest[4,4] = -4.3951
        
        kest[1,5] = -3.2225
        kest[2,5] = -54.9069
        kest[3,5] = -3.2225
        kest[4,5] = -54.9069
        
        kest[1,6] = -8.3853
        kest[2,6] = -59.4790
        kest[3,6] = -8.3853
        kest[4,6] = -59.4790
        '''
        '''
        kest[1,1] = -1.0864
        kest[2,1] = 1.1284
        kest[3,1] = -1.0864
        kest[4,1] = 1.1284
        
        kest[1,2] = 1.1302
        kest[2,2] = -1.0848
        kest[3,2] = 1.1302
        kest[4,2] = -1.0848

        kest[1,3] = 4.6685
        kest[2,3] = 4.6685
        kest[3,3] = 4.6685
        kest[4,3] = 4.6685

        kest[1,4] = 0.6221
        kest[2,4] = 1.1308
        kest[3,4] = 0.6221
        kest[4,4] = 1.1308
        
        kest[1,5] = 12.2138
        kest[2,5] = 2.0852
        kest[3,5] = 12.2138
        kest[4,5] = 2.0852
        
        kest[1,6] = 7.6379
        kest[2,6] = 7.0348
        kest[3,6] = 7.6379
        kest[4,6] = 7.0348
        '''
        deflection = kest*controlState
        
        cn = Vector(4)
        mach = state[4,1]/spdOfSnd
        for fin in range(4):
            if deflection[fin+1,1] - self.deflectionOld[fin+1,1] > self.deltaDeflection:
                deflection[fin+1,1] = self.deflectionOld[fin+1,1] +  self.deltaDeflection
            elif deflection[fin+1,1] - self.deflectionOld[fin+1,1] < -self.deltaDeflection:
                deflection[fin+1,1] = self.deflectionOld[fin+1,1] - self.deltaDeflection
            deflection[fin+1,1] =  deflection[fin+1,1]*180/Math.PI
            if deflection[fin+1,1] > 15:
                deflection[fin+1,1] = 15
            if deflection[fin+1,1] < -15:
                deflection[fin+1,1] = -15
            deflection[fin+1,1] =  deflection[fin+1,1] * Math.PI/180
            cn[fin+1] = self.aero.NormalForceFin(mach, 0, deflection[fin+1,1]) * Math.Sign(deflection[fin+1,1]) # Fixme: Assume alpha = 0 for now
            cx += self.aero.RollingForceFin(cn[fin+1], 0, mach/spdOfSnd) 
        dCx = cx
        dCy = cn[1] + cn[3]
        dCz = cn[2] + cn[4]
        '''
        dCx = 0
        dCy = 0
        dCz = 0
        '''
        self.Asset.AssetDynamicState.IntegratorParameters.Add(self.DELTA_CX, dCx)
        self.Asset.AssetDynamicState.IntegratorParameters.Add(self.DELTA_CY, dCy)
        self.Asset.AssetDynamicState.IntegratorParameters.Add(self.DELTA_CZ, dCz)
        #self.Asset.AssetDynamicState.IntegratorParameters.Add(self.CNTRL_MNT, moment)
        ts = event.GetEventStart(self.Asset)
        self._newState.AddValue(self.DEFLECTION_KEY, HSFProfile[Matrix[System.Double]](ts, deflection.Transpose(deflection)))
        self._newState.AddValue(self.DELTA_CY, HSFProfile[System.Double](ts, dCy))
        self._newState.AddValue(self.DELTA_CZ, HSFProfile[System.Double](ts, dCz))
        self.deflectionOld = deflection
        return True
    def CanExtend(self, event, universe, extendTo):
        return True
        #return super(Controller,self).CanExtend(event, universe, extendTo)
    def DependencyCollector(self, currentEvent):
        if (self.SubsystemDependencyFunctions.Count == 0):
            Exception 
            #MissingMemberException("You may not call the dependency collector in your can perform because you have not specified any dependency functions for " + Name);
        outProf = HSFProfile[Matrix[System.Double]]()
        for dep in self.SubsystemDependencyFunctions:
            if not (dep.Key.Equals("DepCollector")):
                outProf = dep.Value.DynamicInvoke(currentEvent)
                #outProf = outProf + temp
        return outProf
        #return super(Controller, self).DependencyCollector(currentEvent)