
import math

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