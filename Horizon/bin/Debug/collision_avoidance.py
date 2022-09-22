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

from math import sin, cos, sqrt

#
## Constants
#
R_inx = Utilities.MatrixIndex(1, 3)
V_inx = Utilities.MatrixIndex(4, 6)

# 
## CW Functions
#

### REPLACE MATRIX EQUATIONS WITH SIMPLE x,y,z components
# Don't need to propagate Velocity!
# Analytical soln to minimum distance?
# consider delX = x1(t) - x2(t)
#           delXprime = vx1(t) - vx2(t)
#           delXprime = 0 when vx1(t) = v2x(t)
#           ... is this useful? type it up in latex?
# norm of distance is ugly, but maybe by looking at when components (xyz) each reach a minimum we can get some pts of interest?
#   but this could miss the times when none are at a minimum, but the total distance is!
#   BUT, we don't care about minimum in an optimization sense, just about if obj's are within minDistance!
#   so we can find when each component is less than minDist away, and this gives pts of interest?!
#   study the inequality for delX < minDist... anything easy/analytical come out of it?

def phiRR(n, t):
    '''
    Define phiRR CW Matrix, used to calculate R1 from R0
    '''
    nt = n*t

    phiRR_mat = Utilities.Matrix[System.Double](3,3)
    phiRR_mat[1,1] = 4.0 - 3.0 * cos(nt)
    phiRR_mat[2,1] = 6.0 * (sin(nt) - nt)
    phiRR_mat[2,2] = 1.0
    phiRR_mat[3,3] = cos(nt)

    return phiRR_mat

def phiRV(n, t):
    '''
    Define phiRV CW Matrix, used to calculate R1 from V0
    '''
    nt = n*t

    phiRV_mat = Utilities.Matrix[System.Double](3,3)
    phiRV_mat[1,1] = sin(nt) / n
    phiRV_mat[1,2] = 2.0 * (1.0 - cos(nt)) / n
    phiRV_mat[2,1] = 2.0 * (cos(nt) - 1.0) / n
    phiRV_mat[2,2] = (4.0 * sin(nt) - 3.0 * nt) / n
    phiRV_mat[3,3] = sin(nt) / n

    return phiRV_mat

def phiVR(n, t):
    '''
    Define phiVR CW Matrix, used to calculate V1 from R0
    '''
    nt = n*t

    phiVR_mat = Utilities.Matrix[System.Double](3,3)
    phiVR_mat[1,1] = 3.0 * n * sin(nt)
    phiVR_mat[2,1] = 6.0 * n * (cos(nt) - 1)
    phiVR_mat[3,3] = -1.0 * n * sin(nt)

    return phiVR_mat

def phiVV(n, t):
    '''
    Define phiVV CW Matrix, used to calculate V1 from V0
    '''
    nt = n*t

    phiVV_mat = Utilities.Matrix[System.Double](3,3)
    phiVV_mat[1,1] = cos(nt)
    phiVV_mat[1,2] = 2.0 * sin(nt)
    phiVV_mat[2,1] = -2.0 * sin(nt)
    phiVV_mat[2,2] = 4.0 * cos(nt) - 3.0
    phiVV_mat[3,3] = cos(nt)

    return phiVV_mat

def propCW(R0, V0, n, t):
    '''
    Propagate RV0 with CW equations
    '''
    phiRR_matrix = phiRR(n, t)
    phiRV_matrix = phiRV(n, t)
    phiVR_matrix = phiVR(n, t)
    phiVV_matrix = phiVV(n, t)

    R1 = phiRR_matrix * R0 + phiRV_matrix * V0
    V1 = phiVR_matrix * R0 + phiVV_matrix * V0
    RV1 = Utilities.Matrix[System.Double].Vertcat(R1, V1)
    
    return RV1


#
## Collision Functions
#

def hsfMatNorm(R):
    '''
    Return 2-Norm of R, expressed as an HSF Matrix Object, which is 1-indexed
    '''
    return sqrt(R[1]**2 + R[2]**2 + R[3]**2)

def checkTasks(event):
    '''
    Check for colliding tasks (2+ assets attempting to service a single target)
    Scheduler enforces that target can be serviced on 1 time step, 
      but does not enforce multiple assets servicing it on common timestep
    '''
    tasksCdict = event.Tasks
    targetList = []
    for assetTask in tasksCdict:
        targetList.append(assetTask.Value.Target.Name.ToString())

    numAssets = len(targetList)
    for idx1 in range(0, numAssets - 1):
        tgt1 = targetList[idx1]
        if (tgt1 == 'EmptyTarget'):
            continue
        for idx2 in range(idx1 + 1, numAssets):
            tgt2 = targetList[idx2]
            if (tgt1 == tgt2):
                return False
    return True

def checkCollisions(allRV, minDistance):
    '''
    Check for collisions in allRV (poor man's conjunction analysis)
    
    INPUTS
        allRV       - list of all active asset's RV expressed as HSF Matrix Objects
        minDistance - float of minimum safe spacing, effectively assuming all
                      assets have a constant, spherical position uncertainty

    Return True if Safe, False if there is a Collision
    '''
    if (len(allRV) < 2):
        return True

    for idx1 in range(0, len(allRV) - 1):
        RV1 = allRV[idx1]
        R1 = RV1[R_inx, ":"]
        for idx2 in range(idx1 + 1, len(allRV)):
            RV2 = allRV[idx2]
            R2 = RV2[R_inx, ":"]
            if (hsfMatNorm(R2 - R1) < minDistance):
                return False

    return True


def getActives(event, activeAssets, tNow):
    '''
    Return updated list of active assets and their states
    '''
    stillActiveAssets = []
    activeStates      = []

    for asset in activeAssets:
        modeKey = Utilities.StateVarKey[System.Boolean](asset + '.' + 'is_transferring')
        isAssetActive = event.State.GetValueAtTime(modeKey, tNow).Value
        if isAssetActive:
            stillActiveAssets.append(asset)

            # get state
            stateKey = Utilities.StateVarKey[Utilities.Matrix[System.Double]](asset + '.' + 'ric_state')
            assetRVState = event.State.GetLastValue(stateKey).Value
            activeStates.append(assetRVState)
        else:
            continue
    return (stillActiveAssets, activeStates)

#
## Class Definition
#
class collision_avoidance(HSFSubsystem.Subsystem):
    def __new__(cls, node, asset):
        instance       = HSFSubsystem.Subsystem.__new__(cls)
        instance.Asset = asset
        instance.Name  = instance.Asset.Name + '.' + node.Attributes['subsystemName'].Value.ToString().ToLower()

        # Set Mean Motion
        instance.mean_motion = 7.2e-5 # hardcoded, roughly GEO
        if (node.Attributes['Mean_Motion'] != None):
            instance.mean_motion = float(node.Attributes['Mean_Motion'].Value)

        # Set Spacing
        instance.MinSpacing = 10.0 # meters, minimum allowed spacing between assets at any time
        if (node.Attributes['MinSpacing'] != None):
            instance.MinSpacing = float(node.Attributes['MinSpacing'].Value)

        # Set number of gridPts to evaluate
        instance.gridPts = 100
        if (node.Attributes['GridPoints'] != None):
            instance.gridPts = int(node.Attributes['GridPoints'].Value)
        
        return instance

    def GetDependencyDictionary(self):
        dep = System.Collections.Generic.Dictionary[str, System.Delegate]()
        return dep

    def GetDependencyCollector(self):
        return System.Func[MissionElements.Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)

    def CanPerform(self, event, universe):
        ##
        # Check for double-tasking
        ##
        taskCheckisGood = checkTasks(event)
        if (not taskCheckisGood):
            return False

        ##
        # Check for collisions
        ##
        to   = event.GetEventStart(self.Asset)
        tf   = event.GetEventEnd(self.Asset)
        dt   = (tf - to) / self.gridPts
        tNow = to

        # Extract list of all servicer assets and all active servicer assets
        tasksCdict   = event.Tasks
        allAssets    = []
        activeAssets = []
        for assetTask in tasksCdict:
            asset  = assetTask.Key.Name.ToString()
            target = assetTask.Value.Target.Name.ToString()
            # Check if asset is this mothership
            if (asset == self.Asset.Name.ToString()):
                continue
            else:
                allAssets.append(asset)
            
            # Check if target is EmptyTarget
            if (target == 'EmptyTarget'):
                continue
            else:
                activeAssets.append(asset)

        # Extract all RIC states at start of event
        allServicerStates = []
        for asset in allAssets:
            stateKey     = Utilities.StateVarKey[Utilities.Matrix[System.Double]](asset + '.' + 'ric_state')
            assetRVState = event.State.GetLastValue(stateKey).Value
            allServicerStates.append(assetRVState)
        
        # Check for initial collisions
        isSafe = checkCollisions(allServicerStates, self.MinSpacing)
        if (not isSafe):
            return False

        # Check all objects through duration of event
        for idx in range(0, self.gridPts + 1):
            # step all servicers
            for servicerState in allServicerStates:
                servicerState = propCW(servicerState[R_inx, ":"], servicerState[V_inx, ":"], self.mean_motion, dt)
            
            # check for collisions
            isSafe = checkCollisions(allServicerStates, self.MinSpacing)
            if (not isSafe):
                return False
            
            # Increment Time
            tNow += dt
        
        # Check all remaining active servicers until no more are active
        (activeAssets, activeStates) = getActives(event, activeAssets, tNow)
        while (len(activeAssets) > 1):
            # Step Forward all Active Assets
            for servicerState in activeStates:
                servicerState = propCW(servicerState[R_inx, ":"], servicerState[V_inx, ":"], self.mean_motion, dt)

            # Check for collisions
            isSafe = checkCollisions(allServicerStates, self.MinSpacing)
            if (not isSafe):
                return False

            # Update activeAssets and activeStates
            tNow += dt
            (activeAssets, activeStates) = getActives(event, activeAssets, tNow)

        # Return True if all checks have passed!
        return True

    def CanExtend(self, event, universe, extendTo):
        #return true
        return super(collision_avoidance, self).CanExtend(event, universe, extendTo)

    def DependencyCollector(self, currentEvent):
        return super(collision_avoidance, self).DependencyCollector(currentEvent)
