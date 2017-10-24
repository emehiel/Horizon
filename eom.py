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
    def __new__(cls, node):
        instance = Utilities.EOMS.__new__(cls)

        instance._mu = 398600.4418
        instance._A = Matrix[System.Double](6)
        instance._A[1,4] = 1.0
        instance._A[2,5] = 1.0
        instance._A[3,6] = 1.0

        return instance

    def PythonAccessor(self, t, y, param):
        r3 = System.Math.Pow(Matrix[System.Double].Norm(y[MatrixIndex(1, 3), 1]), 3)
        mur3 = -self._mu / r3
        self._A[4, 1] = mur3
        self._A[5, 2] = mur3
        self._A[6, 3] = mur3
        dy = Matrix[System.Double]()
        dy = self._A * y
        return dy
