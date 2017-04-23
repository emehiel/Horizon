import sys
import clr
import System.Collections.Generic
import System

clr.AddReference('System.Core')
clr.AddReference('IronPython')
clr.AddReference('System.Xml')
clr.AddReferenceByName('Utilities')

import math
import Utilities
from Utilities import *

class AeroPrediction:
    def __init__(self):
        self.dia = .1106
        self.len = 2.09
        self.height = 0.127
        self.Cr = .254
        self. Ct = 0.076
        self.l = 0.21866
        self.Aref = math.pi/4*math.pow(self.dia, 2)
        pass


    def SkinFrictionDrag(self, spdOfSound, kinematicViscosity, vel):
        
        velocity = Vector.Norm(vel) * 3.28084

        length = self.l * 39.3701 # Convert to inches
        dia = self.dia * 39.3701 # Convert to inches
        Sb = length*dia

        mach = velocity/spdOfSnd

        compressRe = spdOfSnd * mach * length / 12 / kinematicViscosity * (1 \
            + 0.028300 * math.pow(mach, 1) \
            - 0.043000 * math.pow(mach, 2) \
            + 0.210700 * math.pow(mach, 3) \
            - 0.038290 * math.pow(mach, 4) \
            + 0.002709 * math.pow(mach, 5))
        incompressCF =  0.037036 * math.pow(compressRe, -0.155079)
        compressCF = incompressCF * ( 1 \
            + 0.007980 * math.pow(mach, 1) \
            - 0.181300 * math.pow(mach, 2) \
            + 0.063200 * math.pow(mach, 3) \
            - 0.009330 * math.pow(mach, 4) \
            + 0.000549 * math.pow(mach, 5))
        incompressCFrough = 1 / math.pow(1.89 + 1.62 * math.log10(length/K), 2.5)
        compressCFrough = incompressCFrough / (1 + 0.2044*math.pow(mach, 2))

        Cf = max(compressCF, compressCFrough)

        Cdf = Cf * (1 + 60/math.pow(length/ dia, 3) + 0.0025 *(length/dia)) * 4 * Sb/ math.pi / math.pow(dia, 2)

        return Cdf

    def CalcSpdOfSnd(alt):
        height = alt * 3.28084 # Convert to ft because that is what emperical model uses
        if height < 37000:
            spdOfSnd = -0.004*height + 1116.45
        elif heigh < 64000:
            spdOfSnd = 968.08
        else:
            spdOfSnd = 0.0007*height + 924.99
        return spdOfSnd

    def CalcKinematicViscosity(alt):
        height = alt * 3.28084 # Convert to ft because that is what emperical model uses
        if height < 15000:
            a = 0.00002503
            b = 0.0
        elif height < 30000:
            a = 0.00002760
            b = -0.03417
        else:
            a = 0.00004664
            b = -0.6882

        kinematicViscosity = 0.000157*math.exp(a*height + b)
        return kinematicViscosity

    def NormalForceFinBodyInteraction(self, mach, alpha):
        K = 1.1
        Aplan = self.dia * self.len
        Cn = K * Aplan/self.Aref * pow(math.sin(alpha), 2)
        return Cn

    def RollingForceFin(self, Cn_alpha, deflection, vel):
        Ymac = self.height/3 * (self.Cr + 2*self.Ct)/(self.Cr + self.Ct)
        return (Ymac + self.dia/2)*Cn_alpha*deflection/self.dia

    def RollDampingFin(self, Cn_alpha, omega, vel):
        sum = (self.Cr + self.Ct)/2 * math.pow(self.dia/2, 2) * self.height \
            + (self.Cr +2*self.Ct)/3 * self.dia * math.pow(self.height, 2) \
            + (self.Cr +3*self.Ct)/12* math.pow(self.height, 3)
        Cld = Cn_alpha * omega * sum / self.Aref / self.dia / vel
        return Cld

    def NormalForceFin(self, mach, alpha, deflection):
        Afin = (self.Cr+self.Ct)/2*self.height
        if mach <= 1:
            Cn_alpha = 8 * pow(self.height/self.dia, 2)/ (1 + math.sqrt(1 + math.pow((2*self.l/(self.Cr+self.Ct)), 2)))
        else:
            beta = math.sqrt(1-math.pow(mach, 2))
            beta2 = math.pow(beta, 2)
            mach4 = math.pow(mach, 4)
            beta4 = math.pow(beta, 4)
            mach6 = math.pow(mach, 6)
            beta7 = math.pow(beta, 7)
            mach8 = math.pow(mach, 8)
            K1 = 2/beta
            K2 = ((gamma+1)*mach4 - 4*beta2)/(4*beta4)
            K3 = ((gamma+1)*mach8 + (2*math.pow(gamma,2) - 7*gamma -5)*mach6 + 10*(gamma+1)*mach4 + 8)/ (6*beta7)
            Cn_alpha = Afin/self.Aref * (K1 + K2 * alpha + K3 *math.pow(alpha,2))
        Kt = 1 + (self.dia/2 / (self.height * self.dia/2)) #correction factor for fin body interaction
        Cn_alpha = Kt * Cn_alpha * math.pow(math.sin(alpha+deflection),2)
            #Cn_alpha = 2 * math.PI * math.pow(self.height, 2) / Aref / (1 + math.sqrt(1 + math.pow((beta*math.pow(self.height,2)/Cr+Ct), 2)))
        return Cn_alpha