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

##
# Units are seconds, meters, kilograms, and radians
##

##
# Constants
##
singular_nt_array = [
   3.141592653589793,
   6.283185307179586,
   8.838742844152041,
   9.424777960769379,
  12.566370614359172,
  15.364261290786979,
  15.707963267948966,
  18.849555921538759,
  21.747123605878745,
  21.991148575128552,
  25.132741228718345,
  28.085001796594980,
  28.274333882308138,
  31.415926535897931
] # first several nt that produce singularities in the cost function
bracketPairs = [[0, 1], [1, 2], [2, 3], [3, 4], [4, 5], [6, 7], [7, 8], [9, 10], [10, 11], [12, 13]]
# first 10 meaningful indice pairs to examine, skip the tiny brackets
maxIters = 100

##
# Utility Functions
# TODO splitoff
##

def myMin(ls):
    '''
    Find minimum value and associated index in a list with a single pass
    '''
    min_val = ls[0]
    min_val_idx = 0
    for ii in range(1, len(ls)):
        if ls[ii] < min_val:
            min_val = ls[ii]
            min_val_idx = ii
    return (min_val, min_val_idx)


##
# Fuel Functions
##

def calcFuelCost(deltaV_mps, mass0_kg, Isp_sec):
    '''
    Use rocket equation to determine fuel mass used for a given deltaV
    Assume units of kg, m/s
    '''    
    return mass0_kg * (1 - math.exp(-1 * deltaV_mps / (Isp_sec * 9.81)))

def constantBurnHoldFuelCost(R0_m, tHold_sec, n, mass0_kg, Isp_sec):
    '''
    Compute fuel cost for performing a constant burn hold at R0 for tHold
    Assume distributed thrust (dV cost is SUM of components, NOT norm)

    NOTE - R0_m is HSF Matrix
    '''
    deltaV_mps = tHold_sec * n**2 * (3 * R0_m[1] + R0_m[3]) # 1-based index becasue this is a HSF Matrix
    return calcFuelCost(deltaV_mps, mass0_kg, Isp_sec)


## 
# CW Functions
# TODO split CW stuff off to other file
##

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

def invPhiRV(n, t):
    '''
    Define inverse of PhiRV matrix, used for finding V0_plus
    '''
    nt = n*t
    def denom(nt):
        return 3*nt*sin(nt) + 8*cos(nt) - 8

    invPhiRV_mat = Utilities.Matrix[System.Double](3,3)
    invPhiRV_mat[1,1] = n * (3*nt - 4*sin(nt) ) / denom(nt)
    invPhiRV_mat[1,2] = n * 2 * (1 - cos(nt)) / denom(nt)
    invPhiRV_mat[2,1] = n * -2 * (1 - cos(nt)) / denom(nt)
    invPhiRV_mat[2,2] = n * -1 * sin(nt) / denom(nt)
    invPhiRV_mat[3,3] = n / sin(nt)

    return invPhiRV_mat

def stepCW(RV0, n, t):
    '''
    Step a state (RV0) forward in time (t)
    Note that here RV0 is a list and this is non-matrix form
    '''
    nt = n * t
    # Extract Initial Elements for Convenience
    x0  = RV0[0]
    y0  = RV0[1]
    z0  = RV0[2]
    vx0 = RV0[3]
    vy0 = RV0[4]
    vz0 = RV0[5]

    # Compute Each Element
    xf  = ((4 - 3 * cos(nt)) * x0) + (sin(nt) * vx0 / n) + ((1 - cos(nt)) * 2 * vy0 / n)
    yf  = (6 * (sin(nt) - nt) * x0) + y0 + ((cos(nt) - 1) * 2 * vx0 / n) + ((4 * sin(nt) - 3 * nt) * vy0 / n)
    zf  = cos(nt) * z0 + (sin(nt) * vz0 / n)
    vxf = (3 * n * sin(nt) * x0) + (cos(nt) * vx0) + (2 * sin(nt) * vy0)
    vyf = (6 * n * (cos(nt) - 1) * x0) - (2 * sin(nt) * vx0) + (4 * cos(nt) - 3) * vy0
    vzf = (-n * sin(nt) * z0) + (cos(nt) * vz0)

    # Return List
    RVf = [xf, yf, zf, vxf, vyf, vzf]
    return RVf

def solveTwoImp(RV0, RVf, n, dt):
    '''
    Solve Inverted CW Two-Impulsive Maneuver from RV0 to RVf with transfer time dt
    '''

    # Extract Individual R, V
    R_inx = Utilities.MatrixIndex(1, 3)
    V_inx = Utilities.MatrixIndex(4, 6)
    R0 = RV0[R_inx, ":"]
    V0 = RV0[V_inx, ":"]
    Rf = RVf[R_inx, ":"]
    Vf = RVf[V_inx, ":"]

    # # Build CW Matrices
    phiRV_mat = phiRV(n, dt)
    phiRV_inv = invPhiRV(n, dt)
    phiRR_mat = phiRR(n, dt)
    phiVR_mat = phiVR(n, dt)
    phiVV_mat = phiVV(n, dt)


    # Compute V0_plus and Construct RV0_plus
    V0_plus = phiRV_inv * (Rf - phiRR_mat * R0)
    RV0_plus = Utilities.Matrix[System.Double].Vertcat(R0, V0_plus)

    # Compute Vf_minus and Delta_V's
    Vf_minus = phiVR_mat * R0 + phiVV_mat * V0_plus
    DV1_vec  = V0_plus - V0
    DV2_vec  = Vf - Vf_minus
    DV1_val  = Utilities.Matrix[System.Double].Norm(DV1_vec)
    DV2_val  = Utilities.Matrix[System.Double].Norm(DV2_vec)
    DVtot    = DV1_val + DV2_val

    return (RV0_plus, DV1_vec, DV2_vec, DVtot)


##
# Solver Functions
# TODO Split off solvers to other file
##

def fprime(f, x, k):
    '''
    Forward Difference Approximate fdot(x)
    k is a tuple of addittional constants to evaluate f
    '''
    # Set finite difference step size to be sqrt(eps) (Source: MathWorks)
    epsVal = 7.0/3.0 - 4.0/3.0 - 1.0
    h = math.sqrt(epsVal)

    f0 = f(x, k)
    f1 = f(x+h, k)
    fprime = (f1 - f0) / h
    return (fprime, f0)

def secantMinimizer(x0, x1, f, k, tol):
    '''
    Secant Method single variable minimization
    '''
    (g_n_minus_1, f_n_minus_1) = fprime(f, x0, k)
    (g_n, f_n) = fprime(f, x1, k)
    
    magFprime = min(abs(g_n), abs(g_n_minus_1))
    errX = abs(g_n - g_n_minus_1)
    err = min(magFprime, errX)

    x_n_minus_1 = x0
    x_n = x1

    ii = 0
    while (err > tol):
        # compute next step
        x_n_plus_1 = x_n - g_n * (x_n - x_n_minus_1) / (g_n - g_n_minus_1)

        # update variables
        x_n_minus_1 = x_n
        x_n = x_n_plus_1
        g_n_minus_1 = g_n
        (g_n, f_n) = fprime(f, x_n, k)

        magFprime = abs(g_n)
        errX = abs(g_n - g_n_minus_1)
        err = min(magFprime, errX)
        ii += 1

    return (x_n, ii, f_n, magFprime, errX)

def impCost(x, k):
    '''
    Call solveTwoImp and return only the cost
    x is the optimization variable (dt)
    k is the tuple of additional constants (RV0, RVf, n)
    '''
    RV0 = k[0]
    RVf = k[1]
    n   = k[2]
    dt  = x
    (RV0_plus, DV1_vec, DV2_vec, DVtot) = solveTwoImp(RV0, RVf, n, dt)
    return DVtot

def solveForMinDV_Secant(RV0, RVf, n, tol):
    '''
    generate initial guess and call secant method
    '''

    # Initial Guess is n*t = pi/2
    t0 = math.pi / (n * 2)
    t1 = t0 * 1.01

    k = (RV0, RVf, n) # tuple of constants
    (tStar, numIts, cost, magDeriv, magStep) = secantMinimizer(t0, t1, impCost, k, tol)
    tPiRatio = tStar * n / math.pi
    # print('\ttStar = ' + str(tStar) + ' (' + str(tPiRatio) + '*pi), ' + str(numIts) + ' iterations, cost (f) = ' + str(cost) + ', fprime = ' + str(magDeriv) + ', final step size = ' + str(magStep))
    return tStar

def bisectionMinimizer(a, b, f, k, tol):
    '''
    Perform bisection method to find minimum between a0 and b1
    f is the function to evaluate, k is extra parameters
    h used for fprime(), tol dictates the stopping criteria
    '''
    x = (a + b) / 2
    (fprimeX, fX) = fprime(f, x, k)
    ii = 0
    while ((abs(fprimeX) > tol) and (ii < maxIters)):
        if (fprimeX > 0):
            b = x
        else:
            a = x
        x = (a + b) / 2
        (fprimeX, fX) = fprime(f, x, k)
        ii += 1
    
    if (ii >= maxIters):
        print('Exited bisectionMinimizer due to iterations exceeding maximum')


    return (x, ii, fX, fprimeX, b - a)

def solveForMinDV_Bisect(RV0, RVf, n, KOZ, gridPts, nBracks, tol):
    '''
    check nBrack brackets via bisect method
    return global minimum found
    '''
    tStarArr      = []
    numItsArr     = []
    costArr       = []
    magDerivArr   = []
    brackWidthArr = []

    for bb in range(0, nBracks):
        # construct bracket
        brackIdx = bracketPairs[bb]
        rawA     = singular_nt_array[brackIdx[0]]
        rawB     = singular_nt_array[brackIdx[1]]
        rawWidth = rawB - rawA
        a0       = (rawA + (0.01 * rawWidth)) / n
        b0       = (rawB - (0.01 * rawWidth)) / n

        # solve for local bracket solution
        k = (RV0, RVf, n, KOZ, gridPts) # tuple of constants
        (tStar, numIts, cost, magDeriv, brackWidth) = bisectionMinimizer(a0, b0, impCost, k, tol)
        # print('unconstrained bisection arrived at tStar = ' + str(tStar) + ' after ' + str(numIts) + ' iterations, cost = ' + str(cost))
        (validBracket, tStar, numIts, cost, magDeriv, brackWidth) = modifiedBisectionMinimizer(a0, b0, impCost, isValidTOF, k, tol)
        # print('modifiedBisectionMinimizer arrived at tStar = ' + str(tStar) + ' after ' + str(numIts) + ' iterations, cost = ' + str(cost))
        # print('')

        if validBracket:
            tStarArr.append(tStar)
            numItsArr.append(numIts)
            costArr.append(cost)
            magDerivArr.append(magDeriv)
            brackWidthArr.append(brackWidth)
            tPiRatio = tStar * n / math.pi
        # print('\t\tSol ' + str(bb) + ' tStar = ' + str(tStar) + ' (' + str(tPiRatio) + '*pi), ' + str(numIts) + ' iterations, cost (f) = ' + str(cost) + ', fprime = '  + str(magDeriv) + ', bracket width = ' + str(brackWidth))    
    
    if costArr == []:
        print('No Valid Trajectory Found!!')
        return(False, 0)

    (globalMinCost, solIdx) = myMin(costArr)    
    globalTstar = tStarArr[solIdx]
    # print('\tGlobal tStar = ' + str(globalTstar) + ', Cost = ' + str(globalMinCost))
    return (True, globalTstar)

def __isRwithinKOZ(R, KOZ):
    '''
    Evaluate if a given state (R) is within constraint volume (KOZ)
    KOZ = [x, y, z] definig semi-*-axis in LVLH/RIC
    '''
    normalizedDistance = (R[0] / KOZ[0])**2 + (R[1] / KOZ[1])**2 + (R[2] / KOZ[2])**2
    if (normalizedDistance < 1.0):
        return True
    else:
        return False

def __isValidTrajectory(RV0, n, KOZ, tof, gridPts):
    ''''
    Evaluate if a given trajectory (RV0) is within the constraint volume (KOZ) at any point (gridPts) in the transfer (tof)
    '''
    dT = tof / gridPts
    for ii in range(1, gridPts):
        RV = stepCW(RV0, n, dT * ii)
        isWithinKOZ = __isRwithinKOZ(RV[0:3], KOZ)
        if isWithinKOZ:
            return False
    return True

def isValidTOF(x, k):
    '''
    Solve for and Evaluate Trajectory
    '''
    tof     = x
    RV0     = k[0]
    RVtgt   = k[1]
    n       = k[2]
    KOZ     = k[3]
    gridPts = k[4]
    (RV1_plus, DV1_vec, DV2_vec, DVtot)  = solveTwoImp(RV0, RVtgt, n, tof)

    # Convert HSF Matrix into list
    RV0plus = []
    strMatrix = RV1_plus.ToString()
    RV0plus = [float(stateStr) for stateStr in strMatrix[1:-1].split(';')]
    return __isValidTrajectory(RV0plus, n, KOZ, tof, gridPts)

def modifiedBisectionMinimizer(a, b, f, c, k, tol):
    '''
    Modified Bisection Method to "handle" constraint
    a, b is the initial bracket
    f is the (Scalar) cost function handle
    c is the (Boolean) constraint function handle
    k is the tuple of constants for evaluating f & c
    tol is exit criteria tolerance
    '''
    # Set Aside Upper Limit Value
    upperLim = b

    # Solve Unconstrained First
    # Know that at the start fprime(b) > 0 and fprime(a) < 0
    x = (a + b) / 2
    (fprimeX, fX) = fprime(f, x, k)
    ii = 0
    while ((abs(fprimeX) > tol) and (ii < maxIters)):
        if (fprimeX > 0):
            b = x
        else:
            a = x
        x = (a + b) / 2
        (fprimeX, fX) = fprime(f, x, k)
        ii += 1

    if (ii >= maxIters):
        print('Exited modifiedBisectionMinimizer unconstrained minimizer due to iterations exceeding maximum')
    
    # If Fails Constraint, Walk "Up" with increasing TOF
    isXvalid = c(x, k)
    if isXvalid:
        # Minimum solution already satisfies constraint
        return (True, x, ii, fX, fprimeX, b - a)
    else:
        # Find Next Closest Interval
        dT = (upperLim - x) / 8.0 # Hardcoded to 8 regions out of fear that bisect will miss things!
        isValid = False
        for bb in range(1, 9):
            ii += 1
            newX = x + dT * bb
            if c(newX, k):
                isValid = True
                break
        if isValid:
            # Now have closest interval, bisect "down" to smallest valid tof
            # Know that it is invalid on left boundary (newX - dT * ii)
            # Know that it is valid on right boundary (newX)
            # Exit when bracket width is smaller than 10x the fprime tolerance
            a = newX - dT * bb
            b = newX
            thisMaxIters = maxIters + ii
            while (((b - a) > (10 * tol)) and (ii < thisMaxIters)):
                ii += 1
                x = (a + b) / 2
                isXvalid = c(x, k)
                if isXvalid:
                    b = x
                else:
                    a = x
            if (ii >= thisMaxIters):
                print('Exited modifiedBisectionMinimizer constraint solver due to iterations exceeding maximum')
            validX = b # know the upper boundary is valid
            (fprimeX, fX) = fprime(f, validX, k)
            return (True, validX, ii, fX, fprimeX, b - a)
        
        else:
            # No Valid Solution Found!
            return (False, 0, 0, 0, 0, 0)

##
# Class Definition
##
class guidance(HSFSubsystem.Subsystem):
    def __new__(cls, node, asset):
        instance = HSFSubsystem.Subsystem.__new__(cls)
        instance.Asset = asset
        instance.Name = instance.Asset.Name + '.' + node.Attributes['subsystemName'].Value.ToString().ToLower()

        instance.STATEVEC_KEY = Utilities.StateVarKey[Utilities.Matrix[System.Double]](instance.Asset.Name + '.' + 'ric_state')
        instance.addKey(instance.STATEVEC_KEY)

        instance.DELTAV_i_KEY = Utilities.StateVarKey[System.Double](instance.Asset.Name + '.' + 'deltaV_i')
        instance.addKey(instance.DELTAV_i_KEY)

        instance.DELTAV_f_KEY = Utilities.StateVarKey[System.Double](instance.Asset.Name + '.' + 'deltaV_f')
        instance.addKey(instance.DELTAV_f_KEY)

        instance.DELTAV_nmc_KEY = Utilities.StateVarKey[System.Double](instance.Asset.Name + '.' + 'deltaV_nmc')
        instance.addKey(instance.DELTAV_nmc_KEY)

        instance.PROPELLANT_MASS_KEY = Utilities.StateVarKey[System.Double](instance.Asset.Name + '.' + 'propellant_mass_kg')
        instance.addKey(instance.PROPELLANT_MASS_KEY)

        instance.MODE_KEY = Utilities.StateVarKey[System.Boolean](instance.Asset.Name + '.' + 'is_transfering')

        # Define Constants
        instance.dryMass_kg = float(node.Attributes['dryMassKg'].Value)
        instance.Isp_sec    = float(node.Attributes['Isp'].Value)

        instance.mean_motion = 7.2e-5 # hardcoded, roughly GEO
        if (node.Attributes['Mean_Motion'] != None):
            instance.mean_motion = float(node.Attributes['Mean_Motion'].Value)

        instance.KOZ = [50, 100, 20] # Defines semi-axis in LVLH/RIC x, y, z (radial, in track, cross track)
        if (node.Attributes['KOZ'] != None):
            kozStr = node.Attributes['KOZ'].Value
            instance.KOZ = [float(koz) for koz in kozStr[1:-1].split(',')]
        
        instance.gridPts = 100
        if (node.Attributes['Grid_Points'] != None):
            instance.gridPts = int(node.Attributes['Grid_Points'].Value)

        instance.numBracks = 5
        if (node.Attributes['Number_Brackets'] != None):
            instance.numBracks = int(node.Attributes['Number_Brackets'].Value)
        return instance

    def GetDependencyDictionary(self):
        dep = System.Collections.Generic.Dictionary[str, System.Delegate]()
        return dep

    def GetDependencyCollector(self):
        return System.Func[MissionElements.Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)

    def CanPerform(self, event, universe):
        # check for empty target
        tgtName = event.GetAssetTask(self.Asset).Target.Name.ToString()
        if (tgtName == 'EmptyTarget'):
            return True

        n = self.mean_motion

        # Extract Timing Information
        es = event.GetEventStart(self.Asset)
        ee = event.GetEventEnd(self.Asset)
        ts = event.GetTaskStart(self.Asset)
        te = event.GetTaskEnd(self.Asset)
        # print('    Event Start: ' + es.ToString() + ' Event End (default): '+ ee.ToString() + ' Task Start: ' + ts.ToString() + ' Task End (default): ' + te.ToString())

        # Extract Last State Data
        lastState = event.State.GetLastValue(self.STATEVEC_KEY) # Returns matrix Object
        lastTime  = lastState.Key
        RV0       = lastState.Value
        R_inx     = Utilities.MatrixIndex(1, 3)
        V_inx     = Utilities.MatrixIndex(4, 6)
        R0        = RV0[R_inx, ":"]
        V0        = RV0[V_inx, ":"]
        # print('Pulled off R0 = ' + str(R0) + ', V0 = ' + str(V0) + ' at time = ' + str(lastTime))

        # Free-Flight Propagate Forwards to Current Time Step
        dt1 = ts - lastTime
        if (dt1 > 0):
            RV1 = Utilities.Matrix[System.Double](6,1)
            phiRR_matrix = phiRR(n, dt1)
            phiRV_matrix = phiRV(n, dt1)
            phiVR_matrix = phiVR(n, dt1)
            phiVV_matrix = phiVV(n, dt1)

            R1 = phiRR_matrix * R0 + phiRV_matrix * V0
            V1 = phiVR_matrix * R0 + phiVV_matrix * V0
            RV1 = Utilities.Matrix[System.Double].Vertcat(R1, V1)
        else:
            RV1 = RV0

        # print("Last State: " + RV0.ToString() + " New State: " + RV1.ToString())

        # Perform Optimal 2-Impulse Rendezvous With Static Target
        Rtgt_dyn = event.GetAssetTask(self.Asset).Target.DynamicState.DynamicStateECI(ts)
        Rtgt     = Utilities.Matrix[System.Double](3,1)
        Rtgt[1]  = Rtgt_dyn[1]
        Rtgt[2]  = Rtgt_dyn[2]
        Rtgt[3]  = Rtgt_dyn[3]
        Vtgt     = Utilities.Matrix[System.Double](3,1) # Zero velocity for static target
        RVtgt    = Utilities.Matrix[System.Double].Vertcat(Rtgt, Vtgt)
        # print('RV of Tgt = ' + RVtgt.ToString())

        optTol = 1e-4
        (isValid, tStarBisect) = solveForMinDV_Bisect(RV1, RVtgt, n, self.KOZ, self.gridPts, self.numBracks, optTol)
        if not isValid:
            # Cannot Perform Transfer with any TOF brackets explored!
            return False

        # Reconstruct
        (RV1_plus, DV1_vec, DV2_vec, DVtot) = solveTwoImp(RV1, RVtgt, n, tStarBisect)

        event.State.AddValue(self.DELTAV_i_KEY, Utilities.HSFProfile[Utilities.Matrix[System.Double]](ts, DV1_vec))
        event.State.AddValue(self.DELTAV_f_KEY, Utilities.HSFProfile[Utilities.Matrix[System.Double]](ts + tStarBisect, DV2_vec))
        #event.State.AddValue(self.DELTAV_t_KEY, Utilities.HSFProfile[System.Double](ts, DVtot)) REPLACE WITH NMC/HOLD

        # Compute Fuel Cost for delta-V's
        fuelMass0_kg = event.State.GetLastValue(self.PROPELLANT_MASS_KEY).Value
        m0_kg = self.dryMass_kg + fuelMass0_kg
        fuelBurned_kg = calcFuelCost(DVtot, m0_kg, self.Isp_sec)
        fuelMassLeft_kg = fuelMass0_kg - fuelBurned_kg
        event.State.AddValue(self.PROPELLANT_MASS_KEY, Utilities.HSFProfile[System.Double](ts + tStarBisect, fuelMassLeft_kg))
        # print('Had ' + str(fuelMass0_kg) + 'kg of Fuel at time = ' + str(ts) + ', now have ' + str(fuelMassLeft_kg) + 'kg of Fuel at time = ' + str(ts + tStarBisect))

        # Assign mode
        event.State.AddValue(self.MODE_KEY, Utilities.HSFProfile[System.Boolean](ts, True))
        event.State.AddValue(self.MODE_KEY, Utilities.HSFProfile[System.Boolean](ts + tStarBisect, False))

        # Extract Servicing time from tool.py
        stateKey     = Utilities.StateVarKey[System.Double](self.Asset.Name.ToString() + '.' + 'servicing_time')
        tService_sec = event.State.GetLastValue(stateKey).Value

        # Compute fuel cost to hold position while serivicing, TODO - consider just leaving as "latched on"
        m0_kg           = self.dryMass_kg + fuelMassLeft_kg
        fuelBurned_kg   = constantBurnHoldFuelCost(Rtgt, tService_sec, n, m0_kg, self.Isp_sec)
        fuelMassLeft_kg = fuelMassLeft_kg - fuelBurned_kg
        event.State.AddValue(self.PROPELLANT_MASS_KEY, Utilities.HSFProfile[System.Double](ts + tStarBisect + tService_sec, fuelMassLeft_kg))

        # TODO plan/compute exit strategy (dV to NMC, hop to Vbar, or hold), this currently assumes u "latch" on

        #event.State.AddValue(self.STATEVEC_KEY, Utilities.HSFProfile[Utilities.Matrix[System.Double]](ts, RV1_plus))
        event.State.AddValue(self.STATEVEC_KEY, Utilities.HSFProfile[Utilities.Matrix[System.Double]](ts + tStarBisect, RVtgt))
        event.SetTaskEnd(self.Asset, ts + tStarBisect)
        event.SetEventEnd(self.Asset, ts + tStarBisect)
        return True

    def CanExtend(self, event, universe, extendTo):
        #return true
        return super(guidance, self).CanExtend(event, universe, extendTo)

    def DependencyCollector(self, currentEvent):
        #return {}
        # pass
        return super(guidance, self).DependencyCollector(currentEvent)
