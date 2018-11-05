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

class aircraft(HSFSubsystem.Subsystem):
	def __new__(cls, node, asset):
		instance = HSFSubsystem.Subsystem.__new__(cls)
		instance.Asset = asset
		
		# swarm attributes
		if (node.Attributes['UAVstartCol'] != None):
			instance._UAVstartCol = int(node.Attributes['UAVstartCol'].Value)
		if (node.Attributes['UAVstartRow'] != None):
			instance._UAVstartRow = int(node.Attributes['UAVstartRow'].Value)
		if (node.Attributes['UAVattitude'] != None):
			instance._UAVattitude = int(node.Attributes['UAVattitude'].Value)
		if (node.Attributes['visitUpdate'] != None):
			instance._visitUpdate = int(node.Attributes['visitUpdate'].Value)
		if (node.Attributes['visitProp'] != None):
			instance._visitProp = float(node.Attributes['visitProp'].Value)
		if (node.Attributes['visitEvap'] != None):
			instance._visitEvap = float(node.Attributes['visitEvap'].Value)
		if (node.Attributes['visitThreshold'] != None):
			instance._visitThreshold = float(node.Attributes['visitThreshold'].Value)
		if (node.Attributes['visitDeposit'] != None):
			instance._visitDeposit = float(node.Attributes['visitDeposit'].Value)
		if (node.Attributes['radius'] != None):
			instance.radius = int(node.Attributes['radius'].Value)
		
		# create uav instance
		instance.visit = pheromone.Pheromone('visit', instance._visitDeposit, instance._visitUpdate, instance._visitProp, instance._visitEvap, instance._visitThreshold)
		instance.start_pt = gridpoint.GridPoint(instance._UAVstartRow-1, instance._UAVstartCol-1)
		interval = 2*instance.radius
		instance.start_pt_m = gridpoint.GridPoint(0, 0)
		instance.pos_pt_m = gridpoint.GridPoint(0, 0)
		instance.uav = vehicle.Vehicle(instance.Asset.Name , instance.start_pt, instance.visit, 
					instance._UAVattitude, instance.start_pt_m, instance.pos_pt_m)
		
		return instance
		
	def GetDependencyDictionary(self):
		dep = Dictionary[str, Delegate]()
		return dep
		
	def GetDependencyCollector(self):
		return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)
		
	def CanPerform(self, event, environment):
		ts = event.GetTaskStart(self.Asset)
		te = event.GetTaskEnd(self.Asset)
		es = event.GetEventStart(self.Asset)
		ee = event.GetEventEnd(self.Asset)
		self.uav.dt = ee - es
		
		#=========================================================
		# Get Current Pheromone Map Objects
		#=========================================================
		t_map = environment.GetObject[System.Double]("TIME_MAP")
		t_map_visit = environment.GetObject[System.Double]("TIME_MAP_VISIT")
		world = environment.GetObject[System.Object]("MAP")
		lawn = environment.GetObject[System.Object]("LAWN")
		
		#=========================================================
		# Choose Target Space for Next Move
		#=========================================================
		if (self.uav.needTarget):
			map.pick_move(world, self.uav)
			self.uav.needTarget = False
			self.uav.stage1 = True
			self.uav.stage2 = True
			self.uav.stage3 = True
			self.uav.start_pt_m.col = (self.uav.start_pt_m.col) + (self.uav.pos_pt_m.col)
			self.uav.start_pt_m.row = (self.uav.start_pt_m.row) + (self.uav.pos_pt_m.row)
			self.uav.pos_pt_m.col = 0
			self.uav.pos_pt_m.row = 0
			
			if (self.uav.init):
				self.uav.t_start = 0
				self.uav.init = 0
				
			else:
				self.uav.t_start = ee
				
			environment.SetObject[System.Object](self.Asset.Name, self.uav)
			
			# get interval size for maneuver search area
			TgtSpace = grid.get_cell(world.searched_grid, self.uav.pos_pt)
			radius = self.radius
			uav_posX = (self.start_pt.col*2*radius+radius) + (self.uav.start_pt_m.col) + (self.uav.pos_pt_m.col)
			uav_posY = (self.start_pt.row*2*radius+radius) + (self.uav.start_pt_m.row) + (self.uav.pos_pt_m.row)
			
			# calculate interval from current uav position on map to target point
			x_int = abs(TgtSpace.bounds_target_pt.col - (uav_posX))
			y_int = abs(TgtSpace.bounds_target_pt.row - (uav_posY))
			# use the smaller distance to target so vehicle doesn't overshoot target
			if (x_int < y_int):
				# make sure it's not a 90 deg turn where interval is ~0
				if (x_int < radius):
					self.uav.interval = y_int
				else:
					self.uav.interval = (x_int + y_int)*0.5
					
			elif (y_int < x_int):
				if (y_int < radius):
					self.uav.interval = x_int
				else:
					self.uav.interval = (x_int + y_int)*0.5
					
			else:
				# x and y intervals are the same size
				self.uav.interval = x_int
				
		
		#=========================================================
		# Propagate Pheromones
		#=========================================================
		# update map once every 10 sec (only at asset 1)
		# use condition to make sure there is only one propagation
		# per time step
		#=========================================================
		if (es > t_map + lawn.update):
			map.propagate_flavor(world, world.track_grid, lawn)
			environment.SetObject[System.Double]("TIME_MAP", es)
			
		if (es > t_map_visit + self.uav.repel.update):
			map.propagate_flavor(world, world.visit_grid, self.visit)
			environment.SetObject[System.Double]("TIME_MAP_VISIT", es)
			
		if (es%1 == 0):
			map.searched_grid_count_update(world, world.searched_grid)
			
			
		return True

	def CanExtend(self, event, environment, extendTo):
		return super(aircraft, self).CanExtend(event, environment, extendTo)
		
	def DependencyCollector(self, currentEvent):
		return super(aircraft, self).DependencyCollector(currentEvent)
		