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

import imp
map = imp.load_source('map.py', '..\\..\\..\\SwarmPheromone\\map.py')
vehicle = imp.load_source('map.py', '..\\..\\..\\SwarmPheromone\\vehicle.py')
grid = imp.load_source('grid.py', '..\\..\\..\\SwarmPheromone\\grid.py')
gridpoint = imp.load_source('gridpoint.py', '..\\..\\..\\SwarmPheromone\\gridpoint.py')
pheromone = imp.load_source('pheromone.py', '..\\..\\..\\SwarmPheromone\\pheromone.py')

class controller(HSFSubsystem.Subsystem):
	def __new__(cls, node, asset):
		instance = HSFSubsystem.Subsystem.__new__(cls)
		instance.Asset = asset
		
		# aircraft steady state and controller
		if (node.Attributes['A'] != None):
			instance.A = Matrix[System.Double](node.Attributes['A'].Value)
		if (node.Attributes['B'] != None):
			instance.B = Matrix[System.Double](node.Attributes['B'].Value)
		if (node.Attributes['GainsMtx'] != None):
			instance.K = Matrix[System.Double](node.Attributes['GainsMtx'].Value)
		if (node.Attributes['F'] != None):
			instance.F = Matrix[System.Double](node.Attributes['F'].Value)
		if (node.Attributes['N'] != None):
			instance.N = Matrix[System.Double](node.Attributes['N'].Value)
		
		# load transition and controller matrices into IntegratorParameters for use with EOMS
		instance.Asset.AssetDynamicState.IntegratorParameters.Add(StateVarKey[Matrix[System.Double]]("A"), instance.A)
		instance.Asset.AssetDynamicState.IntegratorParameters.Add(StateVarKey[Matrix[System.Double]]("B"), instance.B)
		instance.Asset.AssetDynamicState.IntegratorParameters.Add(StateVarKey[Matrix[System.Double]]("K"), instance.K)
		instance.Asset.AssetDynamicState.IntegratorParameters.Add(StateVarKey[Matrix[System.Double]]("F"), instance.F)
		instance.Asset.AssetDynamicState.IntegratorParameters.Add(StateVarKey[Matrix[System.Double]]("N"), instance.N)
		
		return instance
		
	def GetDependencyDictionary(self):
		dep = Dictionary[str, Delegate]()
		return dep
		
	def GetDependencyCollector(self):
		return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
		
	def CanPerform(self, event, environment):
		# events have a duration of 1 time step. Tasks occur during events
		es = event.GetEventStart(self.Asset)
		dt = SchedParameters.SimStepSeconds
		
		pi = System.Math.PI
		
		#=========================================================
		# Get Vehicle Object and Distance from Target Space
		#=========================================================
		uav = environment.GetObject[System.Object](self.Asset.Name)
		world = environment.GetObject[System.Object]("MAP")
		turn = uav.turnAngle # turn angle to target space from current attitude
		
		if uav.needTarget:
			TgtSpace = grid.get_cell(world.searched_grid, uav.pos_pt)
			radius = environment.GetObject[System.Int32]("RADIUS")
			x_int = abs(TgtSpace.bounds_target_pt.col - (uav.prev_pt.col*2*radius+radius))
			y_int = abs(TgtSpace.bounds_target_pt.row - (uav.prev_pt.row*2*radius+radius))
			# use the smaller distance to target so vehicle doesn't overshoot target
			if (x_int < y_int):
				# make sure it's not a 90 deg turn where interval is ~0
				if (x_int < radius):
					uav.interval = y_int
				else:
					uav.interval = x_int
			elif (y_int < x_int):
				if (y_int < radius):
					uav.interval = x_int
				else:
					uav.interval = y_int
			else:
				uav.interval = x_int
			
			uav.needTarget = False
		
		return True

	def CanExtend(self, event, environment, extendTo):
		return super(controller, self).CanExtend(event, environment, extendTo)
		
	def DependencyCollector(self, currentEvent):
		return super(controller, self).DependencyCollector(currentEvent)
		
	def rotateCurve(self, psi, pts):
		#=========================================================
		# Rotate Curve Points X, Y in pts
		#=========================================================
		# psi is the angle the points are rotated
		# pts is a [2x1] containing points x and y
		#=========================================================
		Z = Matrix[System.Double](2,2)
		Z[1,1] = System.Math.Cos(-psi)
		Z[1,2] = System.Math.Sin(-psi)
		Z[2,1] = -System.Math.Sin(-psi)
		Z[2,2] = System.Math.Cos(-psi)
		
		pts = Z*pts
		return pts[1], pts[2]
		
		