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
    R0, V0, and RV1 are all HSF Matrix Objects
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

#
## Extraction Functions
#

def getActives(event, activeAssets, tNow_sec):
    '''
    Return updated list of active assets
    '''
    stillActiveAssets = []

    for asset in activeAssets:
        transferKey    = Utilities.StateVarKey[System.Boolean](asset + '.' + 'is_transferring')
        serviceKey     = Utilities.StateVarKey[System.Boolean](asset + '.' + 'is_servicing')
        isTransferring = event.State.GetValueAtTime(transferKey, tNow_sec).Value
        isServicing    = event.State.GetValueAtTime(serviceKey, tNow_sec).Value
        if  (isTransferring or isServicing):
            stillActiveAssets.append(asset)
        else:
            continue
    return (stillActiveAssets)

def getStates(event, assetNames, tNow_sec, n_radps):
    '''
    Return asset RIC states given the event, list of active asset names, and current time
    '''
    allStates = []
    for assetName in assetNames:
        # Extract last defined state - either RV1plus or RVTgt or RVTgt
        stateKey      = Utilities.StateVarKey[Utilities.Matrix[System.Double]](assetName + '.' + 'ric_state')
        assetRV0State = event.State.GetValueAtTime(stateKey, tNow_sec).Value
        assetT0       = event.State.GetValueAtTime(stateKey, tNow_sec).Key

        # Propagate forward unless it is servicing
        isServicingKey = Utilities.StateVarKey[System.Boolean](assetName + '.' + 'is_servicing')
        isServicing = event.State.GetValueAtTime(isServicingKey, tNow_sec).Value
        if (not isServicing):
            # asset is drifting if its not servicing
            assetRVState = propCW(assetRV0State[R_inx, ":"], assetRV0State[V_inx, ":"], n_radps, tNow_sec - assetT0)
            allStates.append(assetRVState)
        else:
            allStates.append(assetRV0State)

    return allStates

#
## Class Definition
#
class collisionAvoidance(HSFSubsystem.Subsystem):
    def __new__(cls, node, asset):
        instance       = HSFSubsystem.Subsystem.__new__(cls)
        instance.Asset = asset
        instance.Name  = instance.Asset.Name + '.' + node.Attributes['subsystemName'].Value.ToString().ToLower()

        # Set Mean Motion
        instance.meanMotion_radps = 7.2e-5 # hardcoded, roughly GEO
        if (node.Attributes['meanMotion_radps'] != None):
            instance.meanMotion_radps = float(node.Attributes['meanMotion_radps'].Value)

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

        # Extract global timing information
        t0_sec   = event.GetEventStart(self.Asset)
        tf_sec   = event.GetEventEnd(self.Asset)
        dt_sec   = (tf_sec - t0_sec) / self.gridPts

        # Extract all servicer assets and their task end times
        tasksCdict = event.Tasks # this is a C# object
        allAssetData = {}
        for assetTask in tasksCdict:
            assetName  = assetTask.Key.Name.ToString()
            #targetName = assetTask.Value.Target.Name.ToString()
            if (assetName == self.Asset.Name.ToString()):
                continue # skip asset if its this mothership
            else:
                allAssetData[assetName] = {
                    'taskEndTime' : event.GetTaskEnd(assetTask.Key)
                }
        # TODO - NOTE - HACK - what is the "tasks" if something is busy because of canExtend()????
        #    Hopefully it is the correct tasks as set from the prior timestep, but I don't really know.....
    
        # Check for collisions among initial states
        allStates = getStates(event, allAssetData.keys(), t0_sec, self.meanMotion_radps)
        isSafe = checkCollisions(allStates, self.MinSpacing)
        if (not isSafe):
            return False


        # Find all mode change times
        allModeChangeTimes = []
        for assetName in allAssetData.keys():
            allAssetData[assetName]['modeChangeTimes'] = []

            # isServicing mode changes
            isServicingKey = Utilities.StateVarKey[System.Boolean](assetName + '.' + 'is_servicing')
            isServicingCdict = event.State.GetProfile(isServicingKey).Data # this is a C# Sorted Dictionary
            for cKey in isServicingCdict.Keys:
                allAssetData[assetName]['modeChangeTimes'].append(cKey)
                allModeChangeTimes.append(cKey)
            
            # isTransferring mode changes
            isTransferringKey = Utilities.StateVarKey[System.Boolean](assetName + '.' + 'is_transferring')
            isTransferringCdict = event.State.GetProfile(isTransferringKey).Data # this is a C# Sorted Dictionary
            for cKey in isTransferringCdict.Keys:
                allAssetData[assetName]['modeChangeTimes'].append(cKey)
                allModeChangeTimes.append(cKey)
            
        # Filter mode changes based on if they happen before/after the end of the nominal event (fundamental time step)
        nominalModeChangeTimes = filter(lambda t_sec : t_sec > t0_sec and t_sec < tf_sec, allModeChangeTimes)
        lateModeChangeTimes = filter(lambda t_sec : t_sec > tf_sec, allModeChangeTimes)

        # put these in with nominal timesteps to ensure nominal steps and mode changes are all evaluated
        tVec_sec = [t0_sec + dt_sec]
        for _ in range(1, self.gridPts):
            tVec_sec.append(tVec_sec[-1] + dt_sec)
        
        tVec_sec.extend(nominalModeChangeTimes)
        tVec_sec = list(set(tVec_sec))
        tVec_sec.sort()

        # Check for collisions in the nominal event timeframe
        for tNow_sec in tVec_sec:
            allStates = getStates(event, allAssetData.keys(), tNow_sec, self.meanMotion_radps) # NOTE - this repeats the get/check/propagate process, quicker to stepCW dT unless mode change happened...
            isSafe    = checkCollisions(allStates, self.MinSpacing)
            if (not isSafe):
                return False
        
        # Check all remaining active servicers until no more are active
        activeAssets = getActives(event, allAssetData.keys(), tNow_sec)
        lateModeChangeTimes = list(set(lateModeChangeTimes))
        lateModeChangeTimes.sort()
        while (len(activeAssets) > 1):
            # Step forward dt_sec or to the next mode change if one occurs mid-step
            if ((tNow_sec + dt_sec) > lateModeChangeTimes[0]):
                tNow_sec = lateModeChangeTimes[0]
                del lateModeChangeTimes[0]
            else:
                tNow_sec += dt_sec
            
            # check 'em
            allStates = getStates(event, activeAssets, tNow_sec, self.meanMotion_radps) # NOTE - this repeats the get/check/propagate process, quicker to stepCW dT unless mode change happened...
            isSafe    = checkCollisions(allStates, self.MinSpacing)
            if (not isSafe):
                return False
            
            # redefine activeAssets
            activeAssets = getActives(event, activeAssets, tNow_sec)

        # Return True if all checks have passed!
        return True

    def CanExtend(self, event, universe, extendTo):
        return True

    def DependencyCollector(self, currentEvent):
        return super(collision_avoidance, self).DependencyCollector(currentEvent)
