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
    def __new__(cls, node, asset):
        instance = HSFSystem.Subsystem.__new__(cls)
        instance.Asset = asset
        instance.Name = instance.Asset.Name + '.' + node.Attributes['subsystemName'].Value.ToString().ToLower()

        instance.DOD_KEY = Utilities.StateVariableKey[System.Double](instance.Asset.Name + '.' + 'depthofdischarge')
        instance.POWIN_KEY = Utilities.StateVariableKey[System.Double](instance.Asset.Name + '.' + 'solarpanelpowerin')
        instance.addKey(instance.DOD_KEY)
        instance.addKey(instance.POWIN_KEY)

        # default values if variables not defined in xml file
        instance._batterySize = 1000000
        instance._fullSolarPanelPower = 150
        instance._penumbraSolarPanelPower = 75

        # values read from the xml file		
        if (node.Attributes['batterySize'] != None):
            instance._batterySize = float(node.Attributes['batterySize'].Value)
        if (node.Attributes['fullSolarPower'] != None):
            instance._fullSolarPanelPower = float(node.Attributes['fullSolarPower'].Value)
        if (node.Attributes['penumbraSolarPower'] != None):
            instance._penumbraSolarPanelPower = float(node.Attributes['penumbraSolarPower'].Value)

        return instance

    def CanPerform(self, event, universe):
        #print("Entry of Power CanPreform")
        es = event.GetEventStart(self.Asset)
        te = event.GetTaskEnd(self.Asset)
        ee = event.GetEventEnd(self.Asset)
        powerSubPowerOut = 10

        if (ee > SimParameters.SimEndSeconds):
            #Logger.Report("Simulation ended")
            print("Power Returned via time out")
            return False

        olddod = self._newState.GetLastValue(self.Dkeys[0]).Item2

        # collect power profile out
        powerOut = self.DependencyCollector(event)
        powerOut = powerOut + powerSubPowerOut

        # collect power profile in
        position = self.Asset.AssetDynamicState
        powerIn = self.CalcSolarPanelPowerProfile(es, te, self._newState, position, universe)
        #print("Power CanPreform, powerIN")
        #print(powerIn)
        # calculate dod rate
        dodrateofchange = HSFProfile[System.Double]()
        dodrateofchange = ((powerOut - powerIn) / self._batterySize)

        exceeded = False
        freq = 1.0
        # function returns HSFProfile[System.Double]() object but python reads it as tuple with [0] element as
        # the desired object type
        dodProf = dodrateofchange.lowerLimitIntegrateToProf(es, te, freq, 0.0, exceeded, 0, olddod)
        self._newState.AddValues(self.DOD_KEY, dodProf[0])
        #print("Power CanPreform, DoD")
        #print(dodProf[0])
        return True

    def CanExtend(self, event, universe, extendTo):
        ee = event.GetEventEnd(self.Asset)
        if (ee > SimParameters.SimEndSeconds):
            return False

        sun = universe.Sun
        print(event.State.GetLastValue(self.DOD_KEY))
        te = event.State.GetLastValue(self.DOD_KEY).Item1
        if (event.GetEventEnd(self.Asset) < extendTo):
            event.SetEventEnd(self.Asset, extendTo)
        # get the dod initial conditions
        olddod = event.State.GetValueAtTime(self.DOD_KEY, te).Item2
        # collect power profile out
        powerOut = self.DependencyCollector(event)
        # collect power profile in
        position = self.Asset.AssetDynamicState
        powerIn = self.CalcSolarPanelPowerProfile(te, ee, event.State, position, universe)
        # calculated dod rate
        dodrateofchange = ((powerOut - powerIn) / self._batterySize)

        exceeded_lower = False
        exceeded_upper = False
        freq = 1.0
        dodProf = dodrateofchange.limitIntegrateToProf(te, ee, freq, 0.0, 1.0, exceeded_lower, exceeded_upper, 0, olddod)
        # dodProf is a tuple where the [0] element contains the HSFProfile[System.Double]() object desired
        dodProf = dodProf[0]
        if (exceeded_upper):
            return False
        if (dodProf.LastTime() != ee and ee == SimParameters.SimEndSeconds):
            dodProf[ee] = dodProf.LastValue()
        event.State.AddValues(self.DOD_KEY, dodProf)
        return True

    def GetSolarPanelPower(self, shadow):
        if (str(shadow) == 'UMBRA'):
            return 0
        elif (str(shadow) == 'PENUMBRA'):
            return self._penumbraSolarPanelPower
        else:
            return self._fullSolarPanelPower

    def CalcSolarPanelPowerProfile(self, start, end, state, position, universe):
        # create solar panel profile for this event
        freq = 5
        lastShadow = universe.Sun.castShadowOnPos(position, start)
        solarPanelSolarProfile = Utilities.HSFProfile[System.Double](start, self.GetSolarPanelPower(lastShadow))

        time = start
        while time <= end:
            shadow = universe.Sun.castShadowOnPos(position, time)
            # if the shadow state changes during this step, save the power data
            if (shadow != lastShadow):
                solarPanelSolarProfile[time] = self.GetSolarPanelPower(shadow)
                lastShadow = shadow
            time += freq
        state.AddValues(self.POWIN_KEY, solarPanelSolarProfile)
        return solarPanelSolarProfile

    def DepFinder(self, depFnName):  # Search for method from string input
        fnc = getattr(self, depFnName)
        dep = Dictionary[str, Delegate]()
        depFnToAdd = Func[Event, Utilities.HSFProfile[System.Double]](fnc)
        dep.Add(depFnName, depFnToAdd)
        return dep

    def GetDependencyCollector(self):
        return Func[Event,  Utilities.HSFProfile[System.Double]](self.DependencyCollector)

    def DependencyCollector(self, currentEvent):
        return super(power, self).DependencyCollector(currentEvent)

#    def GetDependencyDictionary(self):
#        dep = Dictionary[str, Delegate]()
#        return dep