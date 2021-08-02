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


class environment(HSFUniverse.Domain):
	def __new__(cls, node):
		instance = HSFUniverse.Domain.__new__(cls)

		# create instance of atmosphere model
		instance.atmos = StandardAtmosphere()

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
	