import sys
import clr
import System.Collections.Generic
import System
clr.AddReference('System.Core')
clr.AddReference('IronPython')
clr.AddReference('System.Xml')
clr.AddReferenceByName('Utilities')

import Utilities
from Utilities import *
from System.Collections.Generic import Dictionary
from IronPython.Compiler import CallTarget0

class eom(Utilities.OrbitalEOMS):
    def __init__(self):
        pass
    def PythonAccessor(self, t, y):
        return super(eom, self).PythonAccessor(t, y)