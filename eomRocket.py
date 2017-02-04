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

class eom(Utilities.EOMS):
    def __init__(self):
        pass
    def PythonAccessor(self, t, y):
        _mu = 398600
        r3 = System.Math.Pow(Matrix[System.Double].Norm(y[MatrixIndex(1, 3), 1]), 3)
        mur3 = -_mu / r3
        _A = Vector[System.Double]()
        _A[1, 4] = 1.0
        _A[2, 5] = 1.0
        _A[3, 6] = 1.0
        _A[4, 1] = mur3
        _A[5, 2] = mur3
        _A[6, 3] = mur3
        dy = Matrix[System.Double]()
        dy = _A * y
        return dy