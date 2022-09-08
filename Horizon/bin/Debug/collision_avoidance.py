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

from math import sin, cos
import math

#
## Constants
#

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
    phiRR_mat[1,1] = 4.0 - 3.0 * math.cos(nt)
    phiRR_mat[2,1] = 6.0 * (math.sin(nt) - nt)
    phiRR_mat[2,2] = 1.0
    phiRR_mat[3,3] = math.cos(nt)

    return phiRR_mat

def phiRV(n, t):
    '''
    Define phiRV CW Matrix, used to calculate R1 from V0
    '''
    nt = n*t

    phiRV_mat = Utilities.Matrix[System.Double](3,3)
    phiRV_mat[1,1] = math.sin(nt) / n
    phiRV_mat[1,2] = 2.0 * (1.0 - math.cos(nt)) / n
    phiRV_mat[2,1] = 2.0 * (math.cos(nt) - 1.0) / n
    phiRV_mat[2,2] = (4.0 * math.sin(nt) - 3.0 * nt) / n
    phiRV_mat[3,3] = math.sin(nt) / n

    return phiRV_mat

def phiVR(n, t):
    '''
    Define phiVR CW Matrix, used to calculate V1 from R0
    '''
    nt = n*t

    phiVR_mat = Utilities.Matrix[System.Double](3,3)
    phiVR_mat[1,1] = 3.0 * n * math.sin(nt)
    phiVR_mat[2,1] = 6.0 * n * (math.cos(nt) - 1)
    phiVR_mat[3,3] = -1.0 * n * math.sin(nt)

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

def checkCollision(allRV, minDistance):
    '''
    Check for collisions in allRV
    '''
    if (len(allRV) < 2):
        return True

    R_inx     = Utilities.MatrixIndex(1, 3)

    # for idx1 in range(0, len(allRV) - 1):
    #   RV1 = allRV[idx1]
    #   R1 = RV1[R_inx, ":"]
    #   for idx2 in range(idx1+1, len(allRV)):
    #       RV2 = allRV[idx2]
    #       R2 = RV2[R_inx, ":"]
    #       if (norm(R2 - R1) < minDistance):
    #           return False

    return True

#
## Class Definition
#
class collision_avoidance(HSFSubsystem.Subsystem):
    def __new__(cls, node, asset):
        instance       = HSFSubsystem.Subsystem.__new__(cls)
        instance.Asset = asset
        instance.Name  = instance.Asset.Name + '.' + node.Attributes['subsystemName'].Value.ToString().ToLower()

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
        # get task/event end times for all proposed task/target/asset/event

        # AN EVENT IS THE COLLECTION OF ALL ASSETS/TASKS... HAS DICT OF THE PAIRS!

        # get RV0+ for everything with a burn commanded, RV0 for NMC / drifters

        # step each with grid of N points up to __end of event, end of last event??

        #ISSUE: If go to end of event, this would allow colliding trajectories to pass as true!
        # If go to end of last epoch, this may fail collisions that aren't necessarily collisions, 
        #    as an early-finisher or drifter could get out of the way on the next step...


        # SOLUTION: check up to event end for all objects (assume step size less than any tStar, should be by design!),
        #  then, dont check against NMCs/drifters anymore, check for the remaining trajectories up to the point
        #  when each ends its event... keep going until only one object to check!

        ##
        # Check for double-tasking
        ##
        taskCheckisGood = checkTasks(event)
        if (not taskCheckisGood):
            return False

        ##
        # Check for phyiscal collisions
        ##
        to = event.GetEventStart(self.Asset)
        tf = event.GetEventEnd(self.Asset)
        dt = (tf - to) / self.gridPts

        # Extract list of all servicer assets
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
        #   active assets

        for ii in range(0, self.gridPts + 1):
            # Construct 2D array of states
            allServicerStates = []
            for asset in allAssets:
                stateKey     = Utilities.StateVarKey[Utilities.Matrix[System.Double]](asset + '.' + 'ric_state')
                assetRVState = event.State.GetLastValue(stateKey).Value
                allServicerStates.append(assetRVState)
                #assetRVState = event.State.GetValueAtTime(stateKey, event.GetTaskStart(self.Asset)).Value 
                # Get whole profile?

                # Propbably want to GET all the LVLH states and THEN propagate/check
                # DRIFTERS will have no task, their last RV state is the NMC (need to put in!) I want to check
                # ACTIVES I need their transfer path and end date for checking thru their active timeline...

            t = to + (dt * ii)

        # for all gridPts in step:
        #   get all states @ gridPt

        #   check for collisions
        #   isSafe = checkCollisions(allRV, minDist)
        #   if not isSafe:
        #       return False

        # end for loop

        # trim list to only active assets
        # while activeList > 1 asset:
        #   get all states @ gridPt

        #   check for collisions
        #   isSafe = checkCollisions(allRV, minDist)
        #   if not isSafe:
        #       return False

        #   step fwd to next gridPt
        #   re-trim activeList based on who is still active
        # end while loop

        return True

    def CanExtend(self, event, universe, extendTo):
        #return true
        return super(collision_avoidance, self).CanExtend(event, universe, extendTo)

    def DependencyCollector(self, currentEvent):
        return super(collision_avoidance, self).DependencyCollector(currentEvent)