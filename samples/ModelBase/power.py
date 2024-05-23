import sys
import clr
import System.Collections.Generic
import System
import math
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
    def CanPerform(self, event, universe):
        es = event.GetEventStart(self.Asset)
        te = event.GetTaskEnd(self.Asset)
        ee = event.GetEventEnd(self.Asset)
        powerSubPowerOut = 10

        if ee > SimParameters.SimEndSeconds:
            print("Power Returned via time out")
            return False

        olddod = self._newState.GetLastValue(self.DOD_KEY).Item2
        
        # Determine if in sunlit or eclipse period based on target (time window)
        sunlit = self.is_sunlit_period(es, ee, universe)
        
        # collect power profile out
        powerOut = self.DependencyCollector(event)
        powerOut = powerOut + powerSubPowerOut

        # collect power profile in
        position = self.Asset.AssetDynamicState
        powerIn = self.CalcSolarPanelPowerProfile(es, te, self._newState, position, universe, sunlit)
        
        # calculate dod rate
        dodrateofchange = HSFProfile[System.Double]()
        dodrateofchange = ((powerOut - powerIn) / self.batterySize)

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

    def CalcSolarPanelPowerProfile(self, start, end, state, position, universe, sunlit):
        freq = 5
        solarPanelSolarProfile = Utilities.HSFProfile[System.Double](start, self.fullSolarPower if sunlit else 0)

        time = start
        while time <= end:
            solarPanelSolarProfile[time] = self.fullSolarPower if sunlit else 0
            time += freq
        state.AddValues(self.POWIN_KEY, solarPanelSolarProfile)
        return solarPanelSolarProfile

    def is_sunlit_period(self, start, end, universe):
        # Get position of the satellite at the start and end times
        position_start = self.Asset.AssetDynamicState.PositionAtTime(start) #Location in framework? No asset declared within DynamicState.cs
        position_end = self.Asset.AssetDynamicState.PositionAtTime(end)
        
        # Get the position of the Sun 
        sun_position = universe.Sun.Position #need to find this

        # Check if the satellite is in the Earth's shadow at the start and end times
        sunlit_start = self.is_in_sunlight(position_start, sun_position, universe)
        sunlit_end = self.is_in_sunlight(position_end, sun_position, universe)

        return sunlit_start or sunlit_end

    def is_in_sunlight(self, satellite_position, sun_position, universe):
        # shadow checks, considering the Earth's umbra and penumbra
        # For simplicity, using a basic geometric check here
        earth_position = universe.Earth.Position
        satellite_to_earth = [satellite_position[i] - earth_position[i] for i in range(3)]
        sun_to_earth = [sun_position[i] - earth_position[i] for i in range(3)]

        angle = self.angle_between_vectors(satellite_to_earth, sun_to_earth)
        return angle > 90  # Satellite is in sunlight if the angle is greater than 90 degrees

    def angle_between_vectors(self, v1, v2):
        # Calculate the angle between two vectors
        dot_product = sum(a * b for a, b in zip(v1, v2))
        magnitude_v1 = sum(a ** 2 for a in v1) ** 0.5
        magnitude_v2 = sum(b ** 2 for b in v2) ** 0.5
        return math.degrees(math.acos(dot_product / (magnitude_v1 * magnitude_v2)))

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

