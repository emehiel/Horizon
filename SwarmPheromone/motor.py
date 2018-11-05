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
from HSFSystem import *
from System.Xml import XmlNode
from Utilities import *
from HSFUniverse import *
from UserModel import *
from MissionElements import *
from System import Func, Delegate, Math
from System.Collections.Generic import Dictionary, KeyValuePair
from IronPython.Compiler import CallTarget0


class motor(HSFSubsystem.Subsystem):
	def __new__(cls, node, asset):
		instance = HSFSubsystem.Subsystem.__new__(cls)
		instance.Asset = asset
		
		instance.FUEL_KEY = Utilities.StateVarKey[System.Double](instance.Asset.Name + '.' + 'fuel')
		instance.addKey(instance.FUEL_KEY)
		
		return instance
		
	def GetDependencyDictionary(self):
		dep = Dictionary[str, Delegate]()
		return dep
		
	def GetDependencyCollector(self):
		return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
		
	def CanPerform(self, event, environment):
		es = event.GetEventStart(self.Asset)
		dt = SchedParameters.SimStepSeconds
		
		# Calculate fuel rate based on aircraft cruise speed
		alt = self.Asset.AssetDynamicState.PositionECI(es)[3]
		vec = self.Asset.AssetDynamicState.VelocityECI(es)
		Vvec = Vector(3)
		Vvec[1] = vec[1] + 36
		Vvec[2] = vec[2]
		Vvec[3] = vec[3]
		V = Vector.Norm(Vvec)

		rpm = self.CalcRPM(V)
		fuelRate = self.CalcFuelConsumption(rpm)
		fuelUsed = fuelRate*dt
		self._newState.AddValue(self.FUEL_KEY, HSFProfile[System.Double](es, fuelUsed))
		
		return True

	def CanExtend(self, event, environment, extendTo):
		return super(motor, self).CanExtend(event, environment, extendTo)
		
	def DependencyCollector(self, currentEvent):
		return super(motor, self).DependencyCollector(currentEvent)
		
	def CalcRPM(self, v):#power):
		# power is in Watts
		
		# calculate power in kW and return rpm
		# using trap speed method to estimate hp of motor
		m = 800 * 2.20462 # convert kg-> lb
		v = v * 2.23694 # convert m/s-> mph
		hp = m*(v/234)*(v/234)*(v/234)
		kW = hp*0.7457
		
		# return rpm using trendline equation from motor performance data plot
		return (int)((kW + 12.354)/0.0157)
		
	def CalcFuelConsumption(self, rpm):
		# return fuel used in gal/s
		return ((8E-7*rpm*rpm - 0.0049*rpm + 8.7569)/3600)
		
	def CalcTorque(self, rpm):
		# return torque in Nm
		return -5E-6*rpm*rpm + 0.0483*rpm + 4.1595