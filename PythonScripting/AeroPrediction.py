import sys
import clr
import System.Collections.Generic
import System

clr.AddReference('System.Core')
clr.AddReference('IronPython')
clr.AddReference('System.Xml')
clr.AddReference('Utilities')

#import Math
import Utilities
from Utilities import *
from System import Math

class AeroPrediction:
    def __init__(self):
        # Inputs are all in meters. Because the reference uses inches, these values are converted in the functions
        self.dia = .221
        self.diaBase = self.dia
        self.len = 3.28
        self.height = 0.127
        self.Cr = .254
        self. Ct = 0.076
        self.l = 0.127
        self.Aref = Math.PI/4*Math.Pow(self.dia, 2)
        self.t = .003
        self.X = 0
        self.Nf = 4
        self.bodyLen = 2.879
    def DragCoefficient(self, alt, vel):
        Kf = 1.04
        
        spdOfSnd = CalcSpdOfSnd(alt)
        kineViscosity = CalcKinematicViscosity(alt)

        mach = Vector.Norm(vel) * 3.28084/spdOfSnd

        skinDrag = self.SkinFrictionDrag(spdOfSnd, kineViscosity, vel)
        finDrag = self.FinFrictionDrag(spdOfSnd, kineViscosity, vel)
        excresDrag = self.ExcrescenciesDrag(mach)
        proDrag = self.ProtuberenceDrag(spdOfSnd, kineViscosity, vel)
        Cdf = skinDrag + Kf*finDrag +Kf*proDrag+ excresDrag

        if mach < 0.6:
            baseDrag = self.BaseDrag(mach, Cdf)       
        else:
            skinDrag = self.SkinFrictionDrag(spdOfSnd, kineViscosity, 0.6*spdOfSnd*vel/Vector.Norm(vel))
            finDrag = self.FinFrictionDrag(spdOfSnd, kineViscosity, 0.6*spdOfSnd*vel/Vector.Norm(vel))
            baseDrag = self.BaseDrag(mach, skinDrag + Kf*finDrag)


        return baseDrag + Cdf

    def NormalForceCoefficient(self, alt, vel, alpha, deflection):
        spdOfSnd = CalcSpdOfSnd(alt)
        kineViscosity = CalcKinematicViscosity(alt)
        
        mach = Vector.Norm(vel) * 3.28084/spdOfSnd
        Cn = self.NormalForceFin(mach, alpha, deflection[1]) +\
             self.NormalForceFin(mach, alpha, deflection[3]) +\
             self.NormalForceFinBodyInteraction(mach, alpha)
        return Cn
    def SideForceCoefficient(self, alt, vel, beta, deflection):
        spdOfSnd = CalcSpdOfSnd(alt)
        kineViscosity = CalcKinematicViscosity(alt)
        
        mach = Vector.Norm(vel) * 3.28084/spdOfSnd
        Cn = self.NormalForceFin(mach, beta, deflection[2]) +\
             self.NormalForceFin(mach, beta, deflection[4]) +\
             self.NormalForceFinBodyInteraction(mach, beta)
        return Cn
    def BaseDrag(self, mach, Cdf):
        Cdb =  1.02*Cdf*(1.5/Math.Pow(self.len/self.dia, 3/2) + \
        7/Math.Pow(self.len/self.dia, 3)) / \
        Math.PI*self.len*self.dia/self.Aref
        # print Cdb
        return Cdb
        
        #stuff = 0.3086*mach - 0.1085 -0.02411*Math.Pow(mach,2)
        #return 0.559*stuff / Math.Pow(mach,2)

        #return .12+.13*Math.Pow(mach, 2)

        # Don't need to convert to inches because everything is a ratio
        Kb = 0.0274*Math.Atan(self.bodyLen/self.dia + 0.0116)
        n = 3.6542*Math.Pow(self.bodyLen/self.dia, -0.2733)
        Cdb = Kb*Math.Pow(self.diaBase/self.dia, n)/Math.Sqrt(Cdf)
        
        fb = 1
        if mach >= 0.6 and mach <= 1.0:
            fb = 1.0 + 215.8*Math.Pow(mach-0.6, 6)
        elif mach > 1.0 and mach <= 2.0:
            fb = 2.0881*Math.Pow(mach-1, 3) - 3.7938*Math.Pow(mach-1,2) + \
            1.4618*(mach-1)+1.883917
        elif mach > 2.0:
            fb = 0.297*Math.Pow(mach-1, 3) - 0.7937*Math.Pow(mach-1,2) - \
            0.1115*(mach-1)+1.64006

        return Cdb*fb

    def SkinFrictionDrag(self, spdOfSnd, kinematicViscosity, vel):
        K = 0.0016 #Surface Roughness Coefficient
        velocity = Vector.Norm(vel) * 3.28084

        length = self.len * 39.3701 # Convert to inches
        dia = self.dia * 39.3701 # Convert to inches
        Sb = 2*Math.PI*length*dia/2

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

    def FinFrictionDrag(self, spdOfSnd, kinematicViscosity, vel):
        K = 0.0016 #Surface Roughness Coefficient
        velocity = Vector.Norm(vel) * 3.28084
        mach = velocity/spdOfSnd
        Ct = self.Ct * 39.3701
        Cr = self.Cr * 39.3701
        height = self.height * 39.3701
        t = self.t * 39.3701
        Sf = height*(Cr+Ct)
        dia = self.dia * 39.3701
        X = self.X * 39.3701
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
            (Math.Pow(finRatio, 2)/Math.Pow(Math.Log10(finRatio*Re),2.6) - \
            1 / Math.Pow(Math.Log10(Re),2.6) + \
            .5646*(Math.Pow(finRatio, 2)/Math.Pow(Math.Log10(finRatio*Re),3.6) - \
            1 / Math.Pow(Math.Log10(Re),3.6)))
        
        return CfFin*(1 + 60*Math.Pow(t/Cr, 4) + 0.8*(1+5*Math.Pow(Xbar, 2))*(t/Cr))*4*self.Nf*Sf/Math.PI/Math.Pow(dia, 2)

    def ExcrescenciesDrag(self,mach):
        dia = self.dia * 39.3701 # Convert to inches
        length = self.len * 39.3701 # Convert to inches
        Cr = self.Cr* 39.3701 # Convert to inches
        Ct = self.Ct* 39.3701 # Convert to inches
        height = self.height * 39.3701 # Convert to inches
        Sr = 2*Math.PI*dia/2*length + (Cr + Ct)*height*self.Nf # Don't divide by 2 because both sides of fin

        if mach < 0.78:
            Ke = 0.00038
        elif mach >= 0.78 and mach <= 1.04:
            Ke = -0.4501*Math.Pow(mach,4) + 1.5954 * Math.Pow(mach,3) - \
            2.1062*Math.Pow(mach,2) + 1.2288*mach - 0.26717
        else:
            Ke = 0.0002*Math.Pow(mach, 2) - 0.0012*mach + 0.0018
        return Ke * 4*Sr/Math.PI/Math.Pow(dia,2)

    def ProtuberenceDrag(self, spdOfSnd, kinematicViscosity, vel):
        K = 0.0016 #Surface Roughness Coefficient
        velocity = Vector.Norm(vel) * 3.28084

        length = self.len * 39.3701 # Convert to inches
        dia = self.dia * 39.3701 # Convert to inches
        
        aPro = Math.PI*.56/4
        lPro = 1.5
        Sp = aPro*lPro
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

        CfPro = 0.8151*Cf*Math.Pow(2/.02, -.1243)

        return  CfPro*(1+1.798*Math.Pow(Math.Sqrt(aPro)/lPro,3/2))*4*Sp/Math.PI/Math.Pow(dia,2)

    def NormalForceFinBodyInteraction(self, mach, alpha):
        K = 1.1
        Aplan = self.dia * self.len
        Cn = K * Aplan/self.Aref * Math.Pow(Math.Sin(alpha), 2)
        return Cn

    def RollingForceFin(self, Cn_alpha, deflection, vel):
        Ymac = self.height/3 * (self.Cr + 2*self.Ct)/(self.Cr + self.Ct)
        return (Ymac + self.dia/2)*Cn_alpha*deflection/self.dia

    def RollDampingFin(self, Cn_alpha, omega, vel):
        sum = (self.Cr +1*self.Ct)/2 * Math.Pow(self.dia/2, 2) * self.height \
            + (self.Cr +2*self.Ct)/3 * self.dia/2 * Math.Pow(self.height, 2) \
            + (self.Cr +3*self.Ct)/12* Math.Pow(self.height, 3)
        Cld = Cn_alpha * omega * sum / self.Aref / self.dia / vel
        return Cld

    def NormalForceFin(self, mach, alpha, deflection):
        gamma = 1.4
        beta = Math.Sqrt(1-Math.Pow(mach, 2))
        Afin = (self.Cr+self.Ct)/2*self.height
        if mach <= 1:
            #Cn_alpha = 8 * Math.Pow(self.height/self.dia, 2)/ (1 + Math.Sqrt(1 + Math.Pow((2*self.l/(self.Cr+self.Ct)), 2)))
            pass
        else:
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
        #Cn_alpha = Kt * Cn_alpha * Math.Pow(Math.Sin(alpha+deflection),2)
        Cn_alpha = 2 * Math.PI * Math.Pow(self.height, 2) / self.Aref / (1 + Math.Sqrt(1 + Math.Pow((beta*Math.Pow(self.height,2)/self.Cr+self.Ct), 2)))
        return Cn_alpha

def CalcSpdOfSnd(alt):
    height = alt * 3.28084 # Convert to ft because that is what emperical model uses
    if height < 37000:
        spdOfSnd = -0.004*height + 1116.45
    elif height < 64000:
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

    kinematicViscosity = 0.000157*Math.Exp(a*height + b)
    return kinematicViscosity
