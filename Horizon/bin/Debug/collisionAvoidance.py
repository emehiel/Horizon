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
import math

## Constants
R_inx = Utilities.MatrixIndex(1, 3)
V_inx = Utilities.MatrixIndex(4, 6)


## CW Functions
# TODO - consider replacing matrix CW eqns w/ x,y,z component eqns, bcuz we don't need to propagate velocity!
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


## Collision Functions
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

## Extraction Functions
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
        if (isTransferring or isServicing):
            stillActiveAssets.append(asset)
        else: # Must be idling or drifting - not active!
            continue
    return (stillActiveAssets)

def getStates(event, assetNames, tNow_sec, n_radps):
    '''
    Return asset RIC states given the event, list of active asset names, and current time
    '''
    allStates = []
    for assetName in assetNames:
        # Extract last defined state - either RV1plus or RVTgt
        stateKey      = Utilities.StateVarKey[Utilities.Matrix[System.Double]](assetName + '.' + 'ric_state')
        assetRV0State = event.State.GetValueAtTime(stateKey, tNow_sec).Value
        assetT0       = event.State.GetValueAtTime(stateKey, tNow_sec).Key

        # Propagate forward unless it is servicing
        isServicingKey = Utilities.StateVarKey[System.Boolean](assetName + '.' + 'is_servicing')
        isServicing = event.State.GetValueAtTime(isServicingKey, tNow_sec).Value
        isIdlingKey = Utilities.StateVarKey[System.Boolean](assetName + '.' + 'is_idling')
        isIdling = event.State.GetValueAtTime(isIdlingKey, tNow_sec).Value
        if not (isServicing or isIdling): # is on transfer or NMC drift if not servicing/idling
            assetRVState = propCW(assetRV0State[R_inx, ":"], assetRV0State[V_inx, ":"], n_radps, tNow_sec - assetT0)
            allStates.append(assetRVState)
        else:
            allStates.append(assetRV0State)

    return allStates


## Class Definition
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
        
        # Set Short-Circuit
        instance.shortCircuit = False
        if (node.Attributes['DoShortCircuit'] != None):
            instance.shortCircuit = True

        return instance

    def GetDependencyDictionary(self):
        dep = System.Collections.Generic.Dictionary[str, System.Delegate]()
        return dep

    def GetDependencyCollector(self):
        return System.Func[MissionElements.Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)

    def CanPerform(self, event, universe):
        #print('\nattempting CA for event = \n' + event.ToString())
        ## Check for double-tasking
        taskCheckisGood = checkTasks(event)
        if (not taskCheckisGood):
            return False

        # Short-Circuit here if customized to ONLY do task checking
        if self.shortCircuit:
            return True

        ## Check for collisions
        # Extract global timing information
        t0_sec = event.GetEventStart(self.Asset)
        tf_sec = event.GetEventEnd(self.Asset) # I think this is wrong! TODO
        #print('t0_sec = ' + str(t0_sec) + ', tf_sec = ' + str(tf_sec))
        fundTimeStep_sec = tf_sec - t0_sec
        dt_sec = fundTimeStep_sec / self.gridPts

        # Extract all servicer assets and their task end times
        tasksCdict = event.Tasks # this is a C# object
        allTaskEndTimes = []
        allAssetNames = []
        for assetTask in tasksCdict:
            assetName  = assetTask.Key.Name.ToString()
            #targetName = assetTask.Value.Target.Name.ToString()
            if (assetName != self.Asset.Name.ToString()): # skip asset if its the Watchdog itself
                allAssetNames.append(assetName)
                allTaskEndTimes.append(event.GetTaskEnd(assetTask.Key))
        # TODO - NOTE - HACK - what if the "task" of something is busy because of canExtend()????
        #    Hopefully it is the correct tasks as set from the prior timestep, but I don't really know...
        # TODO - a good deal of debug printouts to make sure this whole thing is working correctly...
        if len(allAssetNames) < 2:
            print('NOTICE: NO Collision Avoidance to perform for single asset!')
            return True
    
        # Check for collisions among initial states
        allStates = getStates(event, allAssetNames, t0_sec, self.meanMotion_radps)
        isSafe = checkCollisions(allStates, self.MinSpacing)
        if (not isSafe):
            return False


        # Find all mode change times
        allModeChangeTimes = []
        for assetName in allAssetNames:
            # only care about when it starts/stops transferring, it is on Drift or constant hold for all other times
            isTransferringKey = Utilities.StateVarKey[System.Boolean](assetName + '.' + 'is_transferring')
            isTransferringCdict = event.State.GetProfile(isTransferringKey).Data # this is a C# Sorted Dictionary
            for cKey in isTransferringCdict.Keys:
                allModeChangeTimes.append(cKey)
            
        # check if first fundamental timestep with ALL assets is safe
        isFirstStepSafe = self.evaluateTimeStep(event, allAssetNames, allModeChangeTimes, t0_sec, fundTimeStep_sec) # for first timestep
        if not isFirstStepSafe:
            return False

        # determine how many extra fundamental timesteps require evaluation as well, if any
        if max(allTaskEndTimes) < (t0_sec + fundTimeStep_sec): # no further steps needed
            return True

        #print('did fundamental time steps, now trying the extension... allTaskEndTimes = ' + str(allTaskEndTimes))
        allTaskEndTimes.sort()
        secondLastEndTime = allTaskEndTimes[-2] # second-to-last end time, the last timestep needed to evaluate thru
        #print('uhh, secondLastEndTime = ' + str(secondLastEndTime) + ', t0_sec = ' + str(t0_sec) + ', fundTimeStep_sec = ' + str(fundTimeStep_sec))
        numStepsToEval = math.ceil((secondLastEndTime - t0_sec)/ fundTimeStep_sec)
        #print('last eval epoch is ' + str(secondLastEndTime) + ', t0 is' + str(t0_sec), ' fund dt = ' + str(fundTimeStep_sec), ' so eval an extra ' + str(numStepsToEval) + ' steps')

        if secondLastEndTime < (t0_sec + fundTimeStep_sec): # no further steps needed
            #print('\nonly 1 object rolls over to the next timestep, no CA for this step...\n')
            return True


        # evaluate all extra fundamental timesteps
        for idx in range(1, int(numStepsToEval) + 1):
            stepStartTime_sec = t0_sec + idx * fundTimeStep_sec
            #print('evaluating at bonus step idx ' + str(idx) + ', which starts at t=' + str(stepStartTime_sec))
            activeAssets = getActives(event, allAssetNames, stepStartTime_sec)
            isSafe = self.evaluateTimeStep(event, activeAssets, allModeChangeTimes, stepStartTime_sec, fundTimeStep_sec)
            if not isSafe:
                return False

        # Return True if all checks have passed!
        return True

    def CanExtend(self, event, universe, extendTo):
        return True

    def DependencyCollector(self, currentEvent):
        return super(collisionAvoidance, self).DependencyCollector(currentEvent)

    def evaluateTimeStep(self, event, theActiveAssets, allModeChangeTimes, startTime_sec, fundTimeStep_sec):
        '''
        Evaluate a given fundamental time step for collisions, given:
        event - the HSF Event object...
        theActiveAssets    - name of all active assets for this fundamental time step
        allModeChangeTimes - big list of all mode change times, need to filter down
        startTime_sec      - start time (from epoch0) of the given fundamental time step
        fundTimeStep_sec   - length of the fundamental time step (a global constant)
        '''
        dt_sec = fundTimeStep_sec / self.gridPts

        # base grid pts
        np = int(math.floor(fundTimeStep_sec / dt_sec))
        tVec_sec = [startTime_sec]
        for _ in range(1, np + 1):
            tVec_sec.append(tVec_sec[-1] + dt_sec)

        endTime_sec = startTime_sec + fundTimeStep_sec
        theModeChanges = filter(lambda t_sec : t_sec >= startTime_sec and t_sec < endTime_sec, allModeChangeTimes)

        # put these in with nominal grid points to ensure grid pts and mode changes are all evaluated    
        tVec_sec.extend(theModeChanges)
        tVec_sec = list(set(tVec_sec))
        tVec_sec.sort()

        #print('for t0 = ' + str(startTime_sec) + ', tf = ' + str(endTime_sec) + ', dt = ' + str(dt_sec) + ', modeChanges = ' + str(allModeChangeTimes) + ', the tVec = ' + str(tVec_sec))

        # Check for collisions in the current fundamental time step
        for tNow_sec in tVec_sec:
            #print('trying the getStates() call with event = ' + event.ToString() + ', active assets = ' + str(theActiveAssets) + ', tNow = ' + str(tNow_sec) + ', n = ' + str(self.meanMotion_radps))
            theStates = getStates(event, theActiveAssets, tNow_sec, self.meanMotion_radps) # NOTE - this repeats the get/check/propagate process, might be quicker to stepCW dT unless mode change happened...
            isSafe = checkCollisions(theStates, self.MinSpacing)
            if (not isSafe):
                #print('\nFound a collision! event = ' + event.ToString() + ', tNow_sec = ' + str(tNow_sec) + '\n')
                return False
        
        return True
