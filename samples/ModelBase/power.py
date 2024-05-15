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
from System import Func, Delegate
from System.Collections.Generic import Dictionary
from IronPython.Compiler import CallTarget0

class power(HSFSystem.Subsystem):
 
    def __init__(self, node, asset):
        super(power, self).__init__(node, asset)
        self.DOD_KEY = "dod"  # Example, should be initialized appropriately
        self.POWIN_KEY = "powerin"  # Example, should be initialized appropriately
        self.batterySize = float(node.Attributes["batterySize"].Value) if node.Attributes["batterySize"] else 1000000
        self.solarCellEfficiency = float(node.Attributes["solarCellEfficiency"].Value) if node.Attributes["solarCellEfficiency"] else 0.2
        self.solarCellArea = float(node.Attributes["solarCellArea"].Value) if node.Attributes["solarCellArea"] else 1.0
        self.fullSolarPower = float(node.Attributes["fullSolarPower"].Value) if node.Attributes["fullSolarPower"] else 150
        self.penumbraSolarPower = float(node.Attributes["penumbraSolarPower"].Value) if node.Attributes["penumbraSolarPower"] else 75
        self.initial_time = 0  # Initial time, should be set appropriately
        self.calculation_interval = 60  # Calculation interval in seconds

    def CanPerform(self, event, universe):
        es = event.GetEventStart(self.Asset)
        te = event.GetTaskEnd(self.Asset)
        ee = event.GetEventEnd(self.Asset)
        powerSubPowerOut = 10

        if ee > SimParameters.SimEndSeconds:
            print("Power Returned via time out")
            return False

        olddod = self._newState.GetLastValue(self.DOD_KEY).Item2
        
        # collect power profile out
        powerOut = self.DependencyCollector(event)
        powerOut = powerOut + powerSubPowerOut

        position = self.Asset.AssetDynamicState
        powerIn = self.calc_solar_panel_power_profiled(es, te, self._newState, position, universe)
        dodrateofchange = HSFProfile[System.Double]()
        dodrateofchange = (powerOut - powerIn) / self.batterySize

        exceeded = False
        freq = 1.0
        dodProf = dodrateofchange.lowerLimitIntegrateToProf(es, te, freq, 0.0, exceeded, 0, olddod)
        self._newState.AddValues(self.DOD_KEY, dodProf[0])
        return True
    
    def CanExtend(self, event, universe, extendTo):
        return super(power, self).CanExtend(event, universe, extendTo)

    def GetSolarPanelPower(self, shadow):
        if str(shadow) == 'UMBRA':
            return 0
        elif str(shadow) == 'PENUMBRA':
            return self.penumbraSolarPower
        else:
            return self.fullSolarPower

    def calc_solar_panel_power_profile(self, start, end, state, position, universe):
        freq = 5
        lastShadow = universe.Sun.castShadowOnPos(position, start)
        solarPanelSolarProfile = Utilities.HSFProfile[System.Double](start, self.GetSolarPanelPower(lastShadow))

        time = start
        while time <= end:
            shadow = universe.Sun.castShadowOnPos(position, time)
            if shadow != lastShadow:
                solarPanelSolarProfile[time] = self.GetSolarPanelPower(shadow)
                lastShadow = shadow
            time += freq
        state.AddValues(self.POWIN_KEY, solarPanelSolarProfile)
        return solarPanelSolarProfile

    def calc_solar_panel_power_profiled(self, start, end, state, position, universe):
        solar_output = Utilities.HSFProfile[System.Double]()
        time = start
        while time <= end:
            solar_intensity = self.get_solar_intensity(time, position, universe)
            cell_efficiency = self.get_cell_efficiency(time)
            degradation = self.get_degradation_factor(time)
            power = solar_intensity * cell_efficiency * degradation * self.solarCellArea
            solar_output[time] = power
            time += self.calculation_interval
        state.AddValues(self.POWIN_KEY, solar_output)
        return solar_output

    def get_solar_intensity(self, time, position, universe):
        return 1361

    def get_cell_efficiency(self, time):
        initial_efficiency = self.solarCellEfficiency
        years = (time - self.initial_time) / (365 * 24 * 3600)
        return initial_efficiency * (1 - 0.005 * years)

    def get_degradation_factor(self, time):
        years = (time - self.initial_time) / (365 * 24 * 3600)
        return max(0.8, 1 - 0.02 * years)

    def size_energy_storage(self, orbital_params, peak_power_demand, eclipse_period):
        pass

    def regulate_power(self, power_requirements):
        pass

    def DepFinder(self, depFnName):
        fnc = getattr(self, depFnName)
        dep = Dictionary[str, Delegate]()
        depFnToAdd = Func[Event, Utilities.HSFProfile[System.Double]](fnc)
        dep.Add(depFnName, depFnToAdd)
        return dep

    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)

    def DependencyCollector(self, currentEvent):
        return super(power, self).DependencyCollector(currentEvent)
