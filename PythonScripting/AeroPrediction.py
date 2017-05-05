import sys
import clr
import System.Collections.Generic
import System

clr.AddReference('System.Core')
clr.AddReference('IronPython')
clr.AddReference('System.Xml')
clr.AddReferenceByName('Utilities')

#import Math
import Utilities
from Utilities import *
from System import Math

class AeroPrediction:
    def __init__(self):
        self.dia = .1106*2
        self.len = 2.09
        self.height = 0.127
        self.Cr = .254
        self. Ct = 0.076
        self.l = 0.21866
        self.Aref = Math.PI/4*Math.Pow(self.dia, 2)
        self.t = .00375
        
          
    def SkinFrictionDrag(self, spdOfSound, kinematicViscosity, vel):
        K = 0.0004 #Surface Roughness Coefficient
        velocity = Vector.Norm(vel) * 3.28084

        length = self.l * 39.3701 # Convert to inches
        dia = self.dia * 39.3701 # Convert to inches
        Sb = length*dia

        mach = velocity/spdOfSnd
        
        compressRe = spdOfSnd * mach * length / 12 / kinematicViscosity * (1 \
            + 0.028300 * Math.Pow(mach, 1) \
            - 0.043000 * Math.Pow(mach, 2) \
            + 0.210700 * Math.Pow(mach, 3) \
            - 0.038290 * Math.Pow(mach, 4) \
            + 0.002709 * Math.Pow(mach, 5))
        incompressCF =  0.037036 * Math.Pow(compressRe, -0.155079)
        compressCF = incompressCF * ( 1 \
            + 0.007980 * Math.Pow(mach, 1) \
            - 0.181300 * Math.Pow(mach, 2) \
            + 0.063200 * Math.Pow(mach, 3) \
            - 0.009330 * Math.Pow(mach, 4) \
            + 0.000549 * Math.Pow(mach, 5))
        incompressCFrough = 1 / Math.Pow(1.89 + 1.62 * Math.Log10(length/K), 2.5)
        compressCFrough = incompressCFrough / (1 + 0.2044*Math.Pow(mach, 2))

        Cf = max(compressCF, compressCFrough)

        Cdf = Cf * (1 + 60/Math.Pow(length/ dia, 3) + 0.0025 *(length/dia)) * 4 * Sb/ Math.PI / Math.Pow(dia, 2)

        return Cdf
    def FinFrictionDrag(self, spdOfSound, kinematicViscosity, vel):
        K = 0.0004 #Surface Roughness Coefficient
        velocity = Vector.Norm(vel) * 3.28084
        mach = velocity/spdOfSnd
        Ct = self.Ct * 39.3701
        Cr = self.Cr * 39.3701
        height = self.height * 39.3701
        t = self.t * 39.3701
        Sf = height*(Cr+Ct)
        dia = self.dia * 39.3701
        X = X * 39.3701
        Xbar = X/Cr
        compressRe = spdOfSnd * mach * Cr / 12 / kinematicViscosity * (1 \
            + 0.028300 * Math.Pow(mach, 1) \
            - 0.043000 * Math.Pow(mach, 2) \
            + 0.210700 * Math.Pow(mach, 3) \
            - 0.038290 * Math.Pow(mach, 4) \
            + 0.002709 * Math.Pow(mach, 5))
        incompressCF =  0.037036 * Math.Pow(compressRe, -0.155079)
        compressCF = incompressCF * ( 1 \
            + 0.007980 * Math.Pow(mach, 1) \
            - 0.181300 * Math.Pow(mach, 2) \
            + 0.063200 * Math.Pow(mach, 3) \
            - 0.009330 * Math.Pow(mach, 4) \
            + 0.000549 * Math.Pow(mach, 5))
        incompressCFrough = 1 / Math.Pow(1.89 + 1.62 * Math.Log10(Cr/K), 2.5)
        compressCFrough = incompressCFrough / (1 + 0.2044*Math.Pow(mach, 2))

        Cf = max(compressCF, compressCFrough)

        Re = spdOfSnd * mach * Cr / 12 / kinematicViscosity
        finRatio = Ct/Cr

        CfFin = Cf*Math.Pow(Math.Log10(Re),2.6)/(Math.Pow(finRatio,2) - 1) * \
            Math.Pow(finRatio, 2)/Math.Pow(Math.Log10(finRatio*Re),2.6) - \
            1 / Math.Pow(Math.Log10(Re),2.6) + \
            .5646*(Math.Pow(finRatio, 2)/Math.Pow(Math.Log10(finRatio*Re),3.6) - \
            1 / Math.Pow(Math.Log10(Re),3.6))

        CfAll = CfFin*(1 + 60*Math.Pow(t/Cr, 4) + 0.8*(1+5*Math.Pow(Xbar, 2))*(t/Cr))*4*self.Nf*Sf/Math.PI/Math.Pow(dia, 2)

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

        kinematicViscosity = 0.000157*Math.exp(a*height + b)
        return kinematicViscosity

    def NormalForceFinBodyInteraction(self, mach, alpha):
        K = 1.1
        Aplan = self.dia * self.len
        Cn = K * Aplan/self.Aref * Math.Pow(Math.Sin(alpha), 2)
        return Cn

    def RollingForceFin(self, Cn_alpha, deflection, vel):
        Ymac = self.height/3 * (self.Cr + 2*self.Ct)/(self.Cr + self.Ct)
        return (Ymac + self.dia/2)*Cn_alpha*deflection/self.dia

    def RollDamPIngFin(self, Cn_alpha, omega, vel):
        sum = (self.Cr + self.Ct)/2 * Math.Pow(self.dia/2, 2) * self.height \
            + (self.Cr +2*self.Ct)/3 * self.dia * Math.Pow(self.height, 2) \
            + (self.Cr +3*self.Ct)/12* Math.Pow(self.height, 3)
        Cld = Cn_alpha * omega * sum / self.Aref / self.dia / vel
        return Cld

    def NormalForceFin(self, mach, alpha, deflection):
        gamma = 1.4
        Afin = (self.Cr+self.Ct)/2*self.height
        if mach <= 1:
            Cn_alpha = 8 * Math.Pow(self.height/self.dia, 2)/ (1 + Math.Sqrt(1 + Math.Pow((2*self.l/(self.Cr+self.Ct)), 2)))
        else:
            beta = Math.Sqrt(1-Math.Pow(mach, 2))
            beta2 = Math.Pow(beta, 2)
            mach4 = Math.Pow(mach, 4)
            beta4 = Math.Pow(beta, 4)
            mach6 = Math.Pow(mach, 6)
            beta7 = Math.Pow(beta, 7)
            mach8 = Math.Pow(mach, 8)
            K1 = 2/beta
            K2 = ((gamma+1)*mach4 - 4*beta2)/(4*beta4)
            K3 = ((gamma+1)*mach8 + (2*Math.Pow(gamma,2) - 7*gamma -5)*mach6 + 10*(gamma+1)*mach4 + 8)/ (6*beta7)
            Cn_alpha = Afin/self.Aref * (K1 + K2 * alpha + K3 *Math.Pow(alpha,2))
        Kt = 1 + (self.dia/2 / (self.height * self.dia/2)) #correction factor for fin body interaction
        Cn_alpha = Kt * Cn_alpha * Math.Pow(Math.Sin(alpha+deflection),2)
            #Cn_alpha = 2 * Math.PI * Math.Pow(self.height, 2) / Aref / (1 + Math.Sqrt(1 + Math.Pow((beta*Math.Pow(self.height,2)/Cr+Ct), 2)))
        return Cn_alpha