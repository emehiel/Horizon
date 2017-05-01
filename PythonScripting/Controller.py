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
from AeroPrediction import *
from HSFSubsystem import *
from HSFSystem import *
from System.Xml import XmlNode
from Utilities import *
from HSFUniverse import *
from UserModel import *
from MissionElements import *
from System import Func, Delegate
from System.Collections.Generic import Dictionary
from IronPython.Compiler import CallTarget0

class Controller(Subsystem):
    def __init__(self, node, asset):
        self.aero = AeroPrediction()
        self.DELTA_CY= StateVarKey[System.Double](asset.Name + "." + "dcy");
        self.DELTA_CZ = StateVarKey[System.Double](asset.Name + "." + "dcz");
        self.Asset.AssetDynamicState.IntegratorParameters.Add(self.DELTA_CY, 0)
        self.Asset.AssetDynamicState.IntegratorParameters.Add(self.DELTA_CZ, 0)
        pass
    def GetDependencyDictionary(self):
        dep = Dictionary[str, Delegate]()
        return dep
    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
    def CanPerform(self, event, universe):
        #return super(subsystem, self).CanPerform(event, universe)
        spdOfSnd = 343 #Fixme: Calculate spdofsnd
        cz = 0
        state = self.DependencyCollector(event)

        controlState = state[MatrixIndex(7,12),MatrixIndex(7,12)]
        kest = Matrix[System.Double](4,6)
        kest[1,1] = -1.0443
        kest[2,1] = 1.1918
        kest[3,1] = -1.0443
        kest[4,1] = 1.1918
        
        kest[1,2] = 1.1918
        kest[2,2] = -1.0443
        kest[3,2] = 1.1918
        kest[4,2] = -1.0443

        kest[1,3] = 1.5777
        kest[2,3] = 1.5777
        kest[3,3] = 1.5777
        kest[4,3] = 1.5777
        
        kest[1,4] = 0.9920
        kest[2,4] = 0.9920
        kest[3,4] = 0.9920
        kest[4,4] = 0.9920
        
        kest[1,5] = 2.4387
        kest[2,5] = 2.7064
        kest[3,5] = 2.4387
        kest[4,5] = 2.7064
        
        kest[1,6] = 3.1623
        kest[2,6] = 2.4387
        kest[3,6] = 3.1623
        kest[4,6] = 2.7064
        mach = state[4,4]/spdOfSnd

        for fin in range(4):
            cn[fin] = self.aero.NormalForceFin(mach, 0, deflection(fin)) # Fixme: Assume alpha = 0 for now
            cz += self.aero.RollingForceFin(cn(fin), 0, mach/spdOfSnd) 
        dCy = cn[1] + cn[3]
        dCz = cn[2] + cn[4]
        self.Asset.AssetDynamicState.IntegratorParameters.Add(self.DELTA_CY, dCy)
        self.Asset.AssetDynamicState.IntegratorParameters.Add(self.DELTA_CZ, dCz)
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
                temp = dep.Value.DynamicInvoke(currentEvent)
                #outProf = outProf + temp
        return outProf
        #return super(Controller, self).DependencyCollector(currentEvent)