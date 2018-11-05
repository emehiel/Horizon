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


class camera(HSFSubsystem.Subsystem):
	def __new__(cls, node, asset):
		instance = HSFSubsystem.Subsystem.__new__(cls)
		instance.Asset = asset
		
		if (node.Attributes['Radius'] != None):
			instance.Radius = float(node.Attributes['Radius'].Value)
		
		instance.TARGETFOUND_KEY = Utilities.StateVarKey[System.Double](instance.Asset.Name + '.' + 'targetfound')
		instance.TARGETDISTANCE_KEY = Utilities.StateVarKey[System.Double](instance.Asset.Name + '.' + 'targetdistance')
		instance.addKey(instance.TARGETFOUND_KEY)
		instance.addKey(instance.TARGETDISTANCE_KEY)
		
		return instance
		
	def GetDependencyDictionary(self):
		dep = Dictionary[str, Delegate]()
		return dep
		
	def GetDependencyCollector(self):
		return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
		
	def CanPerform(self, event, environment):
		task = event.GetAssetTask(self.Asset)
		targetType = task.Type
		
		# events have a duration of 1 time step. Tasks occur during events
		es = event.GetEventStart(self.Asset)
		dt = SchedParameters.SimStepSeconds
		
		pi = System.Math.PI
		Found = 0.0
		
		# Get position in deg
		assetState = self.Asset.AssetDynamicState.PositionECI(es)
		targetState = task.Target.DynamicState.InitialConditions()
		
		# Get distance to target
		LatChange = targetState[1] - assetState[1]
		LonChange = targetState[2] - assetState[2]

		# Convert coordinate change to meters
		North = LatChange * 110946
		East = LonChange * (System.Math.Cos(assetState[1]*pi/180) * 111319)
		targetDistance = Vector(2)
		targetDistance[1] = North 
		targetDistance[2] = East
		Distance = Vector.Norm(targetDistance)

		# Check if target is within range
		Found = Distance <= self.Radius
		
		self._newState.AddValue(self.TARGETFOUND_KEY, HSFProfile[System.Double](es, float(Found)))
		self._newState.AddValue(self.TARGETDISTANCE_KEY, HSFProfile[System.Double](es, Distance))
		
			
		if (targetType == TaskType.FLYALONG):
			return True
		
		return Found		

	def CanExtend(self, event, environment, extendTo):
		return super(camera, self).CanExtend(event, environment, extendTo)
		
	def DependencyCollector(self, currentEvent):
		return super(camera, self).DependencyCollector(currentEvent)
		
		