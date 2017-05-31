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
        matrixstr = node.Attributes["GainMatrix"].Value
        kest = Matrix[System.Double](matrixstr)
        self.kest = kest
        self.DEFLECTION_KEY = StateVarKey[Matrix[System.Double]](asset.Name + "." + "deflection")
        self.DROGUE_DEPLOYED = StateVarKey[System.Boolean](asset.Name + "." + "drogue")

        self.addKey(self.DEFLECTION_KEY)
        self.Asset.AssetDynamicState.IntegratorParameters.Add(self.DEFLECTION_KEY, Matrix[System.Double](4,1))
        self.deltaDeflection = 50*.02 * Math.PI/180 # 200 deg/s * Time Step -> radians TODO: Allow once per schedule
        self.deflectionOld = Matrix[System.Double](4,1)
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        return dep
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def CanPerform(self, event, universe):
        ts = event.GetEventStart(self.Asset)
        state = self.DependencyCollector(event).LastValue()
        if not (self.Asset.AssetDynamicState.IntegratorParameters.GetValue(self.DROGUE_DEPLOYED)):
        #if(True):
        #if(False):
            state = Matrix[System.Double].Transpose(state)
            controlState = state[MatrixIndex(7,12), 1]

            deflection = -self.kest*controlState
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

            self.Asset.AssetDynamicState.IntegratorParameters.Add(self.DEFLECTION_KEY, deflection)
            
            self._newState.AddValue(self.DEFLECTION_KEY, HSFProfile[Matrix[System.Double]](ts, deflection.Transpose(deflection)))
        
            self.deflectionOld = deflection
            return True
        deflection = Matrix[System.Double](4,1)
        self._newState.AddValue(self.DEFLECTION_KEY, HSFProfile[Matrix[System.Double]](ts, deflection.Transpose(deflection)))
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