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
gridpoint = imp.load_source('gridpoint.py', '..\\..\\..\\SwarmPheromone\\gridpoint.py')
pheromone = imp.load_source('pheromone.py', '..\\..\\..\\SwarmPheromone\\pheromone.py')


class environment(HSFUniverse.Domain):
	def __new__(cls, node):
		instance = HSFUniverse.Domain.__new__(cls)

		# create instance of atmosphere model
		instance.atmos = StandardAtmosphere()
		
		# platform attributes read from XML file
		if (node.Attributes['totalCols'] != None):
			instance._totalCols = int(node.Attributes['totalCols'].Value)
		if (node.Attributes['totalRows'] != None):
			instance._totalRows = int(node.Attributes['totalRows'].Value)

		# pheromone attribute
		if (node.Attributes['lawnUpdate'] != None):
			instance._lawnUpdate = int(node.Attributes['lawnUpdate'].Value)
		if (node.Attributes['lawnProp'] != None):
			instance._lawnProp = float(node.Attributes['lawnProp'].Value)
		if (node.Attributes['lawnEvap'] != None):
			instance._lawnEvap = float(node.Attributes['lawnEvap'].Value)
		if (node.Attributes['lawnThreshold'] != None):
			instance._lawnThreshold = float(node.Attributes['lawnThreshold'].Value)
		if (node.Attributes['lawnDeposit'] != None):
			instance._lawnDeposit = float(node.Attributes['lawnDeposit'].Value)
		if (node.Attributes['visitUpdate'] != None):
			instance._visitUpdate = int(node.Attributes['visitUpdate'].Value)

		# target location and range
		if (node.Attributes['lawnCol'] != None):
			instance._lawnCol = int(node.Attributes['lawnCol'].Value)
		if (node.Attributes['lawnRow'] != None):
			instance._lawnRow = int(node.Attributes['lawnRow'].Value)
		if (node.Attributes['lawnWidth'] != None):
			instance._lawnWidth = int(node.Attributes['lawnWidth'].Value)
		if (node.Attributes['lawnHeight'] != None):
			instance._lawnHeight = int(node.Attributes['lawnHeight'].Value)
		if (node.Attributes['radius'] != None):
			instance.radius = int(node.Attributes['radius'].Value)
		
		# current time of simulation (used for propagation when dt is taken)
		instance._tmap_lawn = 0.0
		instance._tmap_visit = 0.0
		instance._tsim = 0.0
		
		# create object instances
		instance.world = map.Map(instance._totalRows, instance._totalCols)
		map.searched_grid_agents(instance.world, instance.world.searched_grid, instance._visitUpdate, instance.radius)
		instance.lawn = pheromone.Pheromone('lawn', instance._lawnDeposit, instance._lawnUpdate, instance._lawnProp, instance._lawnEvap, instance._lawnThreshold)

		# define target position and uav starting position
		instance.targetPos = gridpoint.GridPoint(instance._lawnRow-1, instance._lawnCol-1)
		
		# assign pheromone pump location and create UAVs at starting location
		map.drop_flavor(instance.world, instance.world.track_grid, instance.targetPos, instance._lawnWidth, instance._lawnHeight, instance.lawn)
		
		# dictionary for next uav trajectory to be used by eoms
		instance.uav_list = {}
		
		# propagate lawn pheromone to create initial gradient for vehicles
		print 'Creating pheromone gradient'
		for x in range(instance._totalCols/2):
			map.propagate_flavor(instance.world, instance.world.track_grid, instance.lawn)
		print 'Initial pheromone propagation complete...'
		
		return instance

	def get_pheromoneMap(self):
		return self.world

	def get_lawnPheromone(self):
		return self.lawn

	def get_Object(self, s):
		""" choose  function based on string """
		if (s == 'time_map'):
			return self._tmap_lawn
		elif (s == 'time_sim'):
			return self._tsim
		elif (s == 'map'):
			return self.world
		elif (s == 'lawn'):
			return self.lawn
		elif (s == 'time_map_visit'):
			return self._tmap_visit
		elif (s == 'radius'):
			return self.radius
		else:
			# use dict key to find requested value
			return self.uav_list[s]

	def set_Object(self, s, val):
		""" set variable to a given value """
		if (s == 'time_map'):
			self._tmap_lawn = val
		elif (s == 'time_map_visit'):
			self._tmap_visit = val
		elif (s == 'time_sim'):
			self._tsim = val
		else:
			# assign value to dict key
			self.uav_list[s] = val

	def get_Atmosphere(self, s, h):
		""" get density, pressure, or temp for a given height (meters) """
		if (s == 'density'):
			return self.atmos.density(h)
		elif (s == 'pressure'):
			return self.atmos.pressure(h)
		elif (s == 'temperature'):
			return self.atmos.temperature(h)
			