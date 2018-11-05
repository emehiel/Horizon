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

import Utilities
import HSFUniverse
import UserModel
from Utilities import *
from HSFUniverse import *
from UserModel import *
from System.Collections.Generic import Dictionary
from IronPython.Compiler import CallTarget0

import imp
map = imp.load_source('map.py', '..\\..\\..\\UAVPheromones\\map.py')
vehicle = imp.load_source('map.py', '..\\..\\..\\UAVPheromones\\vehicle.py')
grid = imp.load_source('grid.py', '..\\..\\..\\UAVPheromones\\grid.py')
gridpoint = imp.load_source('gridpoint.py', '..\\..\\..\\UAVPheromones\\gridpoint.py')
pheromone = imp.load_source('pheromone.py', '..\\..\\..\\UAVPheromones\\pheromone.py')

class eom(Utilities.EOMS):
	def __new__(cls, node):
		instance = Utilities.EOMS.__new__(cls)
		
		if (node.Attributes['Name'] != None):
			instance.AssetName = str(node.Attributes['Name'].Value)
			
			
		return instance

	def PythonAccessor(self, t, y, param, environment):
		
		pi = System.Math.PI
		
		#=========================================================
		# Steady State Flight Disturbance
		#=========================================================
		xned  = y[1]  # NED same as xi Latitude [deg]
		yned  = y[2]  # NED same as yi Longitude [deg]
		zned  = y[3]  # NED same as zi [m]
		u     = y[4]  # axial velocity [body] [m/s]
		v     = y[5]  # transverse velocity [body] [m/s]
		w     = y[6]  # normal velocity [body] [m/s]
		p     = y[7]  # roll rate [body] [rad/s]
		q     = y[8]  # pitch rate [body] [rad/s]
		r     = y[9]  # yaw rate [body] [rad/s]
		xi    = y[10]  # range [m] N
		yi    = y[11]  # lateral distance [m] E
		zi    = y[12]  # -altitude [m] D
		phi   = y[13] # roll angle [rad]
		theta = y[14] # pitch angle [rad]
		psi   = y[15] # yaw angle [rad]
		tau   = y[16] # thrust
		
		# set heading range
		if (psi < 0):
			psi = ( psi%(2*pi) )
		elif (psi > 0):
			psi = psi%(2*pi)
		
		
		param.Add(StateVarKey[System.Double]("SIM_TIME"), t)
		
		Q_ned2body = self.NED2body(phi, theta, psi)
		Q_body2ned = Matrix[System.Double](3,3).Transpose(Q_ned2body)
		
		V = Matrix[System.Double](3,1)
		V[1] = u + 36
		V[2] = v
		V[3] = w
		Vi = Q_body2ned*V
		
		
		#=========================================================
		# Get Vehicle Object and Distance from Target Space
		#=========================================================
		uav = environment.GetObject[System.Object](self.AssetName)
		world = environment.GetObject[System.Object]("MAP")
		turn = uav.turnAngle # turn angle to target space from current attitude
		
		# distance travelled since start of maneuver
		uav.pos_pt_m.col = yi - uav.start_pt_m.col # range traveled East - start pos
		uav.pos_pt_m.row = xi - uav.start_pt_m.row # range traveled North - start pos
		

		#=========================================================
		# Begin Execution of Turning Maneuvers
		#=========================================================
		if (abs(turn) == 90):
			
			# direction of trajectory path from current attitude
			if (abs(turn)/turn < 0):
				if ( uav.stage1 ):
					
					# rotate current uav path position to match predefined trajectory
					if (uav.prevAttitude == 0):
						rot = 0*pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 2):
						rot = -90 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 4):
						rot = -180 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 6):
						rot = -270 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
					
					# uav path components and radius
					dx_p = uav.interval*0.5 + PosVec_ned[1]
					dy_p = PosVec_ned[2]
					r_p = System.Math.Sqrt(dx_p*dx_p + dy_p*dy_p)
					r_t = uav.interval*0.5
					
					# trajectory angle using uav path
					theta_t = System.Math.Atan( dy_p/dx_p )
					# change in radii from trajectory curve to path curve
					# positive dr refer to uav traveling outside curve. Negative dr means uav is inside curve
					dr = r_p - r_t
					
					# trajectory tangent vector at future location (theta_t + m)
					m = 0.07
					Tvec_ned = Matrix[System.Double]("[%s; %s]" %(System.Math.Sin(theta_t + m), System.Math.Cos(theta_t + m)))
					# velocity vector in the same reference frame (body to eci)
					Vvec = Matrix[System.Double]("[%s; %s]" %(v, u + 36))
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(psi, Vvec)))
					# rotate to initial position to match trajectory
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, Vvec_ned)))
					
					# control heading angle from current heading to desired heading from trajectory
					arg = ( Matrix[System.Double].Dot(Tvec_ned, Vvec_ned)/(Matrix[System.Double].Norm(Tvec_ned)*Matrix[System.Double].Norm(Vvec_ned)) )
					psi_c = -( System.Math.Acos( round(arg, 10) ))
					
					# check if psi_c is above(+) or below(-) Tvec by checking cross product vector
					v1 = Matrix[System.Double]("[%s; %s; %s]" %(Tvec_ned[1], Tvec_ned[2], 0))
					v2 = Matrix[System.Double]("[%s; %s; %s]" %(Vvec_ned[1], Vvec_ned[2], 0))
					v3 = Matrix[System.Double].Cross(v1, v2)
					if (v3[3] < 0):
						psi_c = -psi_c

					
					# supplemental control heading angle based on dr distance from target trajectory
					c = 0

					if (dr == 0):
						psi_c = psi_c + c*0
					else:
						boost = 0
						if ( abs(dr) > 3 ):
							boost = 0
						psi_c = psi_c + c*dr/abs(dr) + boost
					

					if dx_p <= 1E-13*uav.interval*0.5:
						uav.stage1 = False
						
						
				elif ( uav.stage2 ):
					#print 'STAGE 2'
					
					# rotate current uav path position to match predefined trajectory
					if (uav.prevAttitude == 0):
						rot = 0*pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 2):
						rot = -90 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 4):
						rot = -180 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 6):
						rot = -270 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					# uav path components and radius with respect to size of curve 25% of interval
					dx_p = -PosVec_ned[1] - uav.interval*0.5
					dy_p = PosVec_ned[2] - uav.interval*0.25
					r_p = System.Math.Sqrt(dx_p*dx_p + dy_p*dy_p)
					r_t = uav.interval*0.25
					
					# trajectory angle using uav path
					if (dx_p <= 0):
						dx_p = 0.001
					theta_t = pi/2 - System.Math.Atan( dy_p/dx_p )
					
					# change in radii from trajectory curve to path curve
					# positive dr refer to uav traveling outside curve. Negative dr means uav is inside curve
					dr = r_p - r_t
					
					# trajectory tangent vector at future location (theta_t + m)
					m = 0.14
					Tvec_ned = Matrix[System.Double]("[%s; %s]" %(System.Math.Cos(theta_t + m), -System.Math.Sin(theta_t + m)))
					# velocity vector in the same reference frame (body to eci)
					Vvec = Matrix[System.Double]("[%s; %s]" %(v, u + 36))
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(psi, Vvec)))
					# rotate to initial position to match trajectory
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, Vvec_ned)))
					
					
					# control heading angle from current heading to desired heading from trajectory
					arg = ( Matrix[System.Double].Dot(Tvec_ned, Vvec_ned)/(Matrix[System.Double].Norm(Tvec_ned)*Matrix[System.Double].Norm(Vvec_ned)) )
					psi_c = -( System.Math.Acos( round(arg, 10) ))
					
					# conditional to check if psi_c is above(+) or below(-) Tvec
					v1 = Matrix[System.Double]("[%s; %s; %s]" %(Tvec_ned[1], Tvec_ned[2], 0))
					v2 = Matrix[System.Double]("[%s; %s; %s]" %(Vvec_ned[1], Vvec_ned[2], 0))
					v3 = Matrix[System.Double].Cross(v1, v2)
					
					if (v3[3] < 0):
						psi_c = -psi_c
					
					# supplemental control heading angle based on dr distance from target trajectory
					c = -0.03 - 0.03*(360-uav.interval)/360
					
					if (dr == 0):
						psi_c = psi_c + c*0
					else:
						boost = 0
						if ( abs(dr) > 3 ):
							boost = -0.02*dr/3
						psi_c = psi_c + c*dr/abs(dr) + boost
					
					if dy_p <= 1E-13*uav.interval*0.25:
						uav.stage2 = False
						
					
				elif ( uav.stage3 ):
					#print 'STAGE 3'
					
					# rotate current uav path position to match predefined trajectory
					if (uav.prevAttitude == 0):
						rot = 0*pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 2):
						rot = -90 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 4):
						rot = -180 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 6):
						rot = -270 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					# uav path components and radius with respect to size of curve 25% of interval
					dx_p = -uav.interval*0.25 - (PosVec_ned[1] + uav.interval*0.75)
					dy_p = -(PosVec_ned[2] - uav.interval*0.25)
					r_p = System.Math.Sqrt(dx_p*dx_p + dy_p*dy_p)
					r_t = uav.interval*0.25
					
					# trajectory angle using uav path
					if (dy_p <= 0):
						dy_p = 0.001
					theta_t = System.Math.Atan( abs(dy_p/dx_p) )
					
					# change in radii from trajectory curve to path curve
					# positive dr refer to uav traveling outside curve. Negative dr means uav is inside curve
					dr = r_p - r_t
					
					# trajectory tangent vector at future location (theta_t + m)
					m = 0.10
					Tvec_ned = Matrix[System.Double]("[%s; %s]" %(System.Math.Sin(theta_t + m), -System.Math.Cos(theta_t + m)))
					# velocity vector in the same reference frame (body to eci)
					Vvec = Matrix[System.Double]("[%s; %s]" %(v, u + 36))
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(psi, Vvec)))
					# rotate to initial position to match trajectory
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, Vvec_ned)))
					
					
					# control heading angle from current heading to desired heading from trajectory
					arg = ( Matrix[System.Double].Dot(Tvec_ned, Vvec_ned)/(Matrix[System.Double].Norm(Tvec_ned)*Matrix[System.Double].Norm(Vvec_ned)) )
					psi_c = ( System.Math.Acos( round(arg, 10) ) )
					
					# conditional to check if psi_c is above(+) or below(-) Tvec
					v1 = Matrix[System.Double]("[%s; %s; %s]" %(Tvec_ned[1], Tvec_ned[2], 0))
					v2 = Matrix[System.Double]("[%s; %s; %s]" %(Vvec_ned[1], Vvec_ned[2], 0))
					v3 = Matrix[System.Double].Cross(v1, v2)
					
					if (v3[3] > 0):
						psi_c = -psi_c
					
					# supplemental control heading angle based on dr distance from target trajectory
					c = 0.04 + 0.04*(360-uav.interval)/360
					
					if (dr == 0):
						psi_c = psi_c + c*0
					else:
						boost = 0
						if ( abs(dr) > 3 ):
							boost = 0.02*dr/3
						psi_c = psi_c + c*dr/abs(dr) + boost
					
					if dx_p >= -1E-13*uav.interval*0.25:
						uav.stage3 = False
						uav.needTarget = True
						
					
				else:
					if (uav.attitude == 0):
						if (psi < -pi):
							psi_c = (-360*pi/180 - psi)
						elif (psi > pi):
							psi_c = (360*pi/180 - psi)
						else:
							psi_c = (0*pi/180 - psi)
						
					elif (uav.attitude == 2):
						if (psi < 0):
							psi_c = (-270*pi/180 - psi)
						elif (psi > 0):
							psi_c = (90*pi/180 - psi)
						else:
							psi_c = 0
							
					elif (uav.attitude == 4):
						if (psi < 0):
							psi_c = (-180*pi/180 - psi)
						elif (psi > 0):
							psi_c = (180*pi/180 - psi)
						else:
							psi_c = 0
							
					elif (uav.attitude == 6):
						if (psi < 0):
							psi_c = (-90*pi/180 - psi)
						elif (psi > 0):
							psi_c = (270*pi/180 - psi)
						else:
							psi_c = 0
					
				
			else:
				if ( uav.stage1 ):
					
					# rotate current uav path position to match predefined trajectory
					if (uav.prevAttitude == 0):
						rot = 0*pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 2):
						rot = -90 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						X, Y = self.rotateCurve(-rot, PosVec_ned)
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(X, Y))
						
					elif (uav.prevAttitude == 4):
						rot = -180 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 6):
						rot = -270 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					
					# uav path components and radius
					dx_p = uav.interval*0.5 - PosVec_ned[1]
					dy_p = PosVec_ned[2]
					r_p = System.Math.Sqrt(dx_p*dx_p + dy_p*dy_p)
					r_t = uav.interval*0.5
					
					# trajectory angle using uav path
					theta_t = System.Math.Atan(dy_p/dx_p)
					
					# change in radii from trajectory curve to path curve
					# positive dr refer to uav traveling outside curve. Negative dr means uav is inside curve
					dr = r_p - r_t
					
					# trajectory tangent vector at future location (theta_t + m)
					m = 0.06
					Tvec_ned = Matrix[System.Double]("[%s; %s]" %(-System.Math.Sin(theta_t + m), System.Math.Cos(theta_t + m)))
					# velocity vector in the same reference frame (body to eci)
					Vvec = Matrix[System.Double]("[%s; %s]" %(v, u + 36))
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(psi, Vvec)))
					# rotate to initial position to match trajectory
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, Vvec_ned)))
					
					# control heading angle from current heading to desired heading from trajectory
					arg = ( Matrix[System.Double].Dot(Tvec_ned, Vvec_ned)/(Matrix[System.Double].Norm(Tvec_ned)*Matrix[System.Double].Norm(Vvec_ned)) )
					psi_c = System.Math.Acos( round(arg, 10) )
					
					# conditional to check if psi_c is above(+) or below(-) Tvec
					v1 = Matrix[System.Double]("[%s; %s; %s]" %(Tvec_ned[1], Tvec_ned[2], 0))
					v2 = Matrix[System.Double]("[%s; %s; %s]" %(Vvec_ned[1], Vvec_ned[2], 0))
					v3 = Matrix[System.Double].Cross(v1, v2)
					if (v3[3] > 0):
						psi_c = -psi_c
					
					# supplemental control heading angle based on dr distance from target trajectory
					c = 0.02 + 0.02*(360-uav.interval)/360
					
					if (dr == 0):
						psi_c = psi_c + c*0
					else:
						boost = 0
						if ( abs(dr) > 3 ):
							boost = 0.02*dr/3
						psi_c = psi_c + c*dr/abs(dr) + boost
					
					if dx_p <= 1E-13*uav.interval*0.5:
						uav.stage1 = False
						
						
				elif ( uav.stage2 ):
					#print 'STAGE 2'
					
					# rotate current uav path position to match predefined trajectory
					if (uav.prevAttitude == 0):
						rot = 0*pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 2):
						rot = -90 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						X, Y = self.rotateCurve(-rot, PosVec_ned)
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(X, Y))
						
					elif (uav.prevAttitude == 4):
						rot = -180 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 6):
						rot = -270 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					# uav path components and radius with respect to size of curve 25% of interval
					dx_p = PosVec_ned[1] - uav.interval*0.5
					dy_p = PosVec_ned[2] - uav.interval*0.25
					r_p = System.Math.Sqrt(dx_p*dx_p + dy_p*dy_p)
					r_t = uav.interval*0.25
					
					# trajectory angle using uav path
					if (dx_p <= 0):
						dx_p = 0.001
					theta_t = pi/2 - System.Math.Atan(dy_p/dx_p)
					
					# change in radii from trajectory curve to path curve
					# positive dr refer to uav traveling outside curve. Negative dr means uav is inside curve
					dr = r_p - r_t
					
					# trajectory tangent vector at future location (theta_t + m)
					m = 0.12
					Tvec_ned = Matrix[System.Double]("[%s; %s]" %(-System.Math.Cos(theta_t + m), -System.Math.Sin(theta_t + m)))
					# velocity vector in the same reference frame (body to eci)
					Vvec = Matrix[System.Double]("[%s; %s]" %(v, u + 36))
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(psi, Vvec)))
					# rotate to initial position to match trajectory
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, Vvec_ned)))
					
					
					# control heading angle from current heading to desired heading from trajectory
					arg = ( Matrix[System.Double].Dot(Tvec_ned, Vvec_ned)/(Matrix[System.Double].Norm(Tvec_ned)*Matrix[System.Double].Norm(Vvec_ned)) )
					psi_c = System.Math.Acos( round(arg, 10) )
					
					# conditional to check if psi_c is above(+) or below(-) Tvec
					v1 = Matrix[System.Double]("[%s; %s; %s]" %(Tvec_ned[1], Tvec_ned[2], 0))
					v2 = Matrix[System.Double]("[%s; %s; %s]" %(Vvec_ned[1], Vvec_ned[2], 0))
					v3 = Matrix[System.Double].Cross(v1, v2)
					if (v3[3] > 0):
						psi_c = -psi_c
					
					# supplemental control heading angle based on dr distance from target trajectory
					c = 0.02 + 0.02*(360-uav.interval)/360
					
					if (dr == 0):
						psi_c = psi_c + c*0
					else:
						boost = 0
						if ( abs(dr) > 3 ):
							boost = 0.02*dr/3
						psi_c = psi_c + c*dr/abs(dr) + boost
					
					if dy_p <= 1E-13*uav.interval*0.25:
						uav.stage2 = False
						
					
				elif ( uav.stage3 ):
					#print 'STAGE 3'
					
					# rotate current uav path position to match predefined trajectory
					if (uav.prevAttitude == 0):
						rot = 0*pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 2):
						rot = -90 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						X, Y = self.rotateCurve(-rot, PosVec_ned)
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(X, Y))
						
					elif (uav.prevAttitude == 4):
						rot = -180 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 6):
						rot = -270 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					# uav path components and radius with respect to size of curve 25% of interval
					dx_p = uav.interval*0.25 - (PosVec_ned[1] - uav.interval*0.75)
					dy_p = -(PosVec_ned[2] - uav.interval*0.25)
					r_p = System.Math.Sqrt(dx_p*dx_p + dy_p*dy_p)
					r_t = uav.interval*0.25
					
					# trajectory angle using uav path
					if (dy_p <= 0):
						dy_p = 0.001
						
					theta_t = System.Math.Atan(dy_p/dx_p)
					
					# change in radii from trajectory curve to path curve
					# positive dr refer to uav traveling outside curve. Negative dr means uav is inside curve
					dr = r_p - r_t
					
					# trajectory tangent vector at future location (theta_t + m)
					m = 0.08
					Tvec_ned = Matrix[System.Double]("[%s; %s]" %(-System.Math.Sin(theta_t + m), -System.Math.Cos(theta_t + m)))
					# velocity vector in the same reference frame (body to eci)
					Vvec = Matrix[System.Double]("[%s; %s]" %(v, u + 36))
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(psi, Vvec)))
					# rotate to initial position to match trajectory
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, Vvec_ned)))
					
					
					# control heading angle from current heading to desired heading from trajectory
					arg = ( Matrix[System.Double].Dot(Tvec_ned, Vvec_ned)/(Matrix[System.Double].Norm(Tvec_ned)*Matrix[System.Double].Norm(Vvec_ned)) )
					psi_c = -( System.Math.Acos( round(arg, 10) ) )
					
					# conditional to check if psi_c is above(+) or below(-) Tvec
					v1 = Matrix[System.Double]("[%s; %s; %s]" %(Tvec_ned[1], Tvec_ned[2], 0))
					v2 = Matrix[System.Double]("[%s; %s; %s]" %(Vvec_ned[1], Vvec_ned[2], 0))
					v3 = Matrix[System.Double].Cross(v1, v2)
					if (v3[3] < 0):
						psi_c = -psi_c
					
					# supplemental control heading angle based on dr distance from target trajectory
					c = -0.03 - 0.03*(360-uav.interval)/360
					
					if (dr == 0):
						psi_c = psi_c + c*0
					else:
						boost = 0
						if ( abs(dr) > 3 ):
							boost = -0.02*dr/3
						psi_c = psi_c + c*dr/abs(dr) + boost
					
					if dx_p <= 1E-13*uav.interval*0.25:
						uav.stage3 = False
						uav.needTarget = True
						
					
				else:
					if (uav.attitude == 0):
						if (psi < -pi):
							psi_c = (-360*pi/180 - psi)
						elif (psi > pi):
							psi_c = (360*pi/180 - psi)
						else:
							psi_c = (0*pi/180 - psi)
							
					elif (uav.attitude == 2):
						if (psi < 0):
							psi_c = (-270*pi/180 - psi)
						elif (psi > 0):
							psi_c = (90*pi/180 - psi)
						else:
							psi_c = 0
							
					elif (uav.attitude == 4):
						if (psi < 0):
							psi_c = (-180*pi/180 - psi)
						elif (psi > 0):
							psi_c = (180*pi/180 - psi)
						else:
							psi_c = 0
							
					elif (uav.attitude == 6):
						if (psi < 0):
							psi_c = (-90*pi/180 - psi)
						elif (psi > 0):
							psi_c = (270*pi/180 - psi)
						else:
							psi_c = 0
								
		
		elif (abs(turn) == 45):
			#print 'enter 45 deg turn', uav.turnIndex
			
			# direction of trajectory path from current attitude
			if (abs(turn)/turn < 0):
				
				if ( uav.stage1 ):
					
					# rotate current uav path position to match predefined trajectory
					if (uav.prevAttitude == 0):
						rot = 0*pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 2):
						rot = -90 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						X, Y = self.rotateCurve(-rot, PosVec_ned)
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(X, Y))
						
					elif (uav.prevAttitude == 4):
						rot = -180 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 6):
						rot = -270 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					# uav path components and radius
					dx_p = uav.interval*0.5 + PosVec_ned[1]
					dy_p = PosVec_ned[2]
					r_p = System.Math.Sqrt(dx_p*dx_p + dy_p*dy_p)
					r_t = uav.interval*0.5
					
					# trajectory angle using uav path
					theta_t = System.Math.Atan(dy_p/dx_p)
					
					# change in radii from trajectory curve to path curve
					# positive dr refer to uav traveling outside curve. Negative dr means uav is inside curve
					dr = r_p - r_t
					
					# trajectory tangent vector at future location (theta_t + m)
					m = 0.06
					Tvec_ned = Matrix[System.Double]("[%s; %s]" %(System.Math.Sin(theta_t + m), System.Math.Cos(theta_t + m)))
					# velocity vector in the same reference frame (body to eci)
					Vvec = Matrix[System.Double]("[%s; %s]" %(v, u + 36))
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(psi, Vvec)))
					# rotate to initial position to match trajectory
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, Vvec_ned)))
					
					
					# control heading angle from current heading to desired heading from trajectory
					arg = ( Matrix[System.Double].Dot(Tvec_ned, Vvec_ned)/(Matrix[System.Double].Norm(Tvec_ned)*Matrix[System.Double].Norm(Vvec_ned)) )
					psi_c = -System.Math.Acos( round(arg, 10) )
					
					# conditional to check if psi_c is above(+) or below(-) Tvec
					v1 = Matrix[System.Double]("[%s; %s; %s]" %(Tvec_ned[1], Tvec_ned[2], 0))
					v2 = Matrix[System.Double]("[%s; %s; %s]" %(Vvec_ned[1], Vvec_ned[2], 0))
					v3 = Matrix[System.Double].Cross(v1, v2)
					if (v3[3] < 0):
						psi_c = -psi_c
					

					# supplemental control heading angle based on dr distance from target trajectory
					c = -0.02 - 0.02*(360-uav.interval)/360
					
					if (dr == 0):
						psi_c = psi_c + c*0
					else:
						boost = 0
						if ( abs(dr) > 3 ):
							boost = -0.02*dr/3
						psi_c = psi_c + c*dr/abs(dr) + boost
					
					if dx_p <= 1E-13*uav.interval*0.5:
						uav.stage1 = False
						
						
				elif ( uav.stage2 ):
					#print 'STAGE 2'
					
					# rotate current uav path position to match predefined trajectory
					if (uav.prevAttitude == 0):
						rot = 0*pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 2):
						rot = -90 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						X, Y = self.rotateCurve(-rot, PosVec_ned)
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(X, Y))
						
					elif (uav.prevAttitude == 4):
						rot = -180 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 6):
						rot = -270 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					# uav path components and radius
					dx_p = -(PosVec_ned[1] + uav.interval*0.5)
					if (dx_p <= 0):
						dx_p = abs(dx_p)
					dy_p = uav.interval - PosVec_ned[2]
					r_p = System.Math.Sqrt(dx_p*dx_p + dy_p*dy_p)
					r_t = uav.interval*0.5
					
					# trajectory angle using uav path
					theta_t = System.Math.Atan(dy_p/dx_p)
					
					# change in radii from trajectory curve to path curve
					# positive dr refer to uav traveling outside curve. Negative dr means uav is inside curve
					dr = r_p - r_t
					
					# trajectory tangent vector at future location (theta_t + m)
					m = -0.08

					Tvec_ned = Matrix[System.Double]("[%s; %s]" %(System.Math.Sin(theta_t + m), System.Math.Cos(theta_t + m)))
					# velocity vector in the same reference frame (body to eci)
					Vvec = Matrix[System.Double]("[%s; %s]" %(v, u + 36))
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(psi, Vvec)))
					# rotate to initial position to match trajectory
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, Vvec_ned)))
					
					
					# control heading angle from current heading to desired heading from trajectory
					arg = ( Matrix[System.Double].Dot(Tvec_ned, Vvec_ned)/(Matrix[System.Double].Norm(Tvec_ned)*Matrix[System.Double].Norm(Vvec_ned)) )
					psi_c = System.Math.Acos( round(arg, 10) )
					
					# conditional to check if psi_c is above(+) or below(-) Tvec
					v1 = Matrix[System.Double]("[%s; %s; %s]" %(Tvec_ned[1], Tvec_ned[2], 0))
					v2 = Matrix[System.Double]("[%s; %s; %s]" %(Vvec_ned[1], Vvec_ned[2], 0))
					v3 = Matrix[System.Double].Cross(v1, v2)
					if (v3[3] > 0):
						psi_c = -psi_c
					

					# supplemental control heading angle based on dr distance from target trajectory
					c = 0.02 + 0.02*(360-uav.interval)/360
					
					if (dr == 0):
						psi_c = psi_c + c*0
					else:
						boost = 0
						if ( abs(dr) > 3 ):
							boost = 0.02*dr/3
						psi_c = psi_c + c*dr/abs(dr) + boost
					
					
					if dy_p <= 1E-13*uav.interval*0.5:
						uav.stage2 = False
						uav.stage3 = False
						uav.needTarget = True
					
					
				else:
					if (uav.attitude == 0):
						if (psi < -pi):
							psi_c = (-360*pi/180 - psi)
						elif (psi > pi):
							psi_c = (360*pi/180 - psi)
						else:
							psi_c = (0*pi/180 - psi)
							
					elif (uav.attitude == 2):
						if (psi < 0):
							psi_c = (-270*pi/180 - psi)
						elif (psi > 0):
							psi_c = (90*pi/180 - psi)
						else:
							psi_c = 0
							
					elif (uav.attitude == 4):
						if (psi < 0):
							psi_c = (-180*pi/180 - psi)
						elif (psi > 0):
							psi_c = (180*pi/180 - psi)
						else:
							psi_c = 0
							
					elif (uav.attitude == 6):
						if (psi < 0):
							psi_c = (-90*pi/180 - psi)
						elif (psi > 0):
							psi_c = (270*pi/180 - psi)
						else:
							psi_c = 0
			
			
			else:
				#print 'positive'
				
				if ( uav.stage1 ):
					
					# rotate current uav path position to match predefined trajectory
					if (uav.prevAttitude == 0):
						rot = 0*pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 2):
						rot = -90 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						X, Y = self.rotateCurve(-rot, PosVec_ned)
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(X, Y))
						
					elif (uav.prevAttitude == 4):
						rot = -180 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 6):
						rot = -270 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
					
					# uav path components and radius
					dx_p = uav.interval*0.5 - PosVec_ned[1]
					dy_p = PosVec_ned[2]
					r_p = System.Math.Sqrt(dx_p*dx_p + dy_p*dy_p)
					r_t = uav.interval*0.5
					
					# trajectory angle using uav path
					theta_t = System.Math.Atan(dy_p/dx_p)
					
					# change in radii from trajectory curve to path curve
					# positive dr refer to uav traveling outside curve. Negative dr means uav is inside curve
					dr = r_p - r_t
					
					# trajectory tangent vector at future location (theta_t + m)
					m = 0.06
					Tvec_ned = Matrix[System.Double]("[%s; %s]" %(-System.Math.Sin(theta_t + m), System.Math.Cos(theta_t + m)))
					# velocity vector in the same reference frame (body to eci)
					Vvec = Matrix[System.Double]("[%s; %s]" %(v, u + 36))
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(psi, Vvec)))
					# rotate to initial position to match trajectory
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, Vvec_ned)))
					
					
					# control heading angle from current heading to desired heading from trajectory
					arg = ( Matrix[System.Double].Dot(Tvec_ned, Vvec_ned)/(Matrix[System.Double].Norm(Tvec_ned)*Matrix[System.Double].Norm(Vvec_ned)) )
					psi_c = System.Math.Acos( round(arg, 10) )
					
					# conditional to check if psi_c is above(+) or below(-) Tvec
					v1 = Matrix[System.Double]("[%s; %s; %s]" %(Tvec_ned[1], Tvec_ned[2], 0))
					v2 = Matrix[System.Double]("[%s; %s; %s]" %(Vvec_ned[1], Vvec_ned[2], 0))
					v3 = Matrix[System.Double].Cross(v1, v2)
					if (v3[3] > 0):
						psi_c = -psi_c

					# supplemental control heading angle based on dr distance from target trajectory
					c = 0.02 + 0.02*(360-uav.interval)/360
					
					if (dr == 0):
						psi_c = psi_c + c*0
					else:
						boost = 0
						if ( abs(dr) > 3 ):
							boost = 0.02*dr/3
						psi_c = psi_c + c*dr/abs(dr) + boost
					

					if dx_p <= 1E-13*uav.interval*0.5:
						uav.stage1 = False
						
						
				elif ( uav.stage2 ):
					#print 'STAGE 2'
					
					# rotate current uav path position to match predefined trajectory
					if (uav.prevAttitude == 0):
						rot = 0*pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 2):
						rot = -90 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						X, Y = self.rotateCurve(-rot, PosVec_ned)
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(X, Y))
						
					elif (uav.prevAttitude == 4):
						rot = -180 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
						
					elif (uav.prevAttitude == 6):
						rot = -270 * pi/180
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
						PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
					
					# uav path components and radius
					dx_p = PosVec_ned[1] - uav.interval*0.5
					if (dx_p <= 0):
						dx_p = abs(dx_p)
					dy_p = uav.interval - PosVec_ned[2]
					r_p = System.Math.Sqrt(dx_p*dx_p + dy_p*dy_p)
					r_t = uav.interval*0.5
					
					# trajectory angle using uav path
					theta_t = System.Math.Atan(dy_p/dx_p)
					
					# change in radii from trajectory curve to path curve
					# positive dr refer to uav traveling outside curve. Negative dr means uav is inside curve
					dr = r_p - r_t
					
					# trajectory tangent vector at future location (theta_t + m)
					m = -0.06

					Tvec_ned = Matrix[System.Double]("[%s; %s]" %(-System.Math.Sin(theta_t + m), System.Math.Cos(theta_t + m)))
					# velocity vector in the same reference frame (body to eci)
					Vvec = Matrix[System.Double]("[%s; %s]" %(v, u + 36))
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(psi, Vvec)))
					# rotate to initial position to match trajectory
					Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, Vvec_ned)))
					
					
					# control heading angle from current heading to desired heading from trajectory
					arg = ( Matrix[System.Double].Dot(Tvec_ned, Vvec_ned)/(Matrix[System.Double].Norm(Tvec_ned)*Matrix[System.Double].Norm(Vvec_ned)) )
					psi_c = System.Math.Acos( round(arg, 10) )
					
					# conditional to check if psi_c is above(+) or below(-) Tvec
					v1 = Matrix[System.Double]("[%s; %s; %s]" %(Tvec_ned[1], Tvec_ned[2], 0))
					v2 = Matrix[System.Double]("[%s; %s; %s]" %(Vvec_ned[1], Vvec_ned[2], 0))
					v3 = Matrix[System.Double].Cross(v1, v2)
					if (v3[3] > 0):
						psi_c = -psi_c
					

					# supplemental control heading angle based on dr distance from target trajectory
					c = -0.03 - 0.03*(360-uav.interval)/360
					
					if (dr == 0):
						psi_c = psi_c + c*0
					else:
						boost = 0
						if ( abs(dr) > 3 ):
							boost = -0.02*dr/3
						psi_c = psi_c + c*dr/abs(dr) + boost
					
					
					if dy_p <= 1E-13*uav.interval*0.5:
						uav.stage2 = False
						uav.stage3 = False
						uav.needTarget = True
					
					
				else:
					if (uav.attitude == 0):
						if (psi < -pi):
							psi_c = (-360*pi/180 - psi)
						elif (psi > pi):
							psi_c = (360*pi/180 - psi)
						else:
							psi_c = (0*pi/180 - psi)
							
					elif (uav.attitude == 2):
						if (psi < 0):
							psi_c = (-270*pi/180 - psi)
						elif (psi > 0):
							psi_c = (90*pi/180 - psi)
						else:
							psi_c = 0
							
					elif (uav.attitude == 4):
						if (psi < 0):
							psi_c = (-180*pi/180 - psi)
						elif (psi > 0):
							psi_c = (180*pi/180 - psi)
						else:
							psi_c = 0
							
					elif (uav.attitude == 6):
						if (psi < 0):
							psi_c = (-90*pi/180 - psi)
						elif (psi > 0):
							psi_c = (270*pi/180 - psi)
						else:
							psi_c = 0
					
				
		elif (abs(turn) == 0):
			
			if (uav.prevAttitude == 0):
				rot = 0*pi/180
				PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
				PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, PosVec_ned)))
				
			elif (uav.prevAttitude == 2):
				rot = -90 * pi/180
				PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
				X, Y = self.rotateCurve(-rot, PosVec_ned)
				PosVec_ned = Matrix[System.Double]("[%s; %s]" %(X, Y))
				
			elif (uav.prevAttitude == 4):
				rot = -180 * pi/180
				PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
				PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
				
			elif (uav.prevAttitude == 6):
				rot = -270 * pi/180
				PosVec_ned = Matrix[System.Double]("[%s; %s]" %(uav.pos_pt_m.col, uav.pos_pt_m.row))
				PosVec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(-rot, PosVec_ned)))
			
			dx_p = PosVec_ned[1]
			dy_p = uav.interval - PosVec_ned[2]
			
			Tvec_ned = Matrix[System.Double]("[%s; %s]" %(0, 1))
			# velocity vector in the same reference frame (body to eci)
			Vvec = Matrix[System.Double]("[%s; %s]" %(v, u + 36))
			Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(psi, Vvec)))
			# rotate to initial position to match trajectory
			Vvec_ned = Matrix[System.Double]("[%s; %s]" %(self.rotateCurve(rot, Vvec_ned)))
			
			# control heading angle from current heading to desired heading from trajectory
			arg = ( Matrix[System.Double].Dot(Tvec_ned, Vvec_ned)/(Matrix[System.Double].Norm(Tvec_ned)*Matrix[System.Double].Norm(Vvec_ned)) )
			psi_c = System.Math.Acos( round(arg, 10) )
			
			# conditional to check if psi_c is above(+) or below(-) Tvec
			v1 = Matrix[System.Double]("[%s; %s; %s]" %(Tvec_ned[1], Tvec_ned[2], 0))
			v2 = Matrix[System.Double]("[%s; %s; %s]" %(Vvec_ned[1], Vvec_ned[2], 0))
			v3 = Matrix[System.Double].Cross(v1, v2)
			if (v3[3] > 0):
				psi_c = -psi_c
			
			
			dr = -dx_p
			# supplemental control heading angle based on dr distance from target trajectory
			c = 0.0 + 0.0*(360-uav.interval)/360
			
			if (dr == 0):
				psi_c = psi_c + c*0
			else:
				boost = 0
				if ( abs(dr) > 0.5 ):
					boost = 0
				psi_c = psi_c + c*dr/abs(dr) + boost
			
			
			if (dy_p <= 1E-13*uav.interval*0.5):
				uav.needTarget = True
			
				
		#=========================================================
		# State Transition Matrix for Equations of Motion
		#=========================================================
		A = param.GetValue(StateVarKey[Matrix[System.Double]]("A"))
		B = param.GetValue(StateVarKey[Matrix[System.Double]]("B"))
		# Controller Gains and Matrices
		K = param.GetValue(StateVarKey[Matrix[System.Double]]("K"))
		F = param.GetValue(StateVarKey[Matrix[System.Double]]("F"))
		N = param.GetValue(StateVarKey[Matrix[System.Double]]("N"))
		
		#=========================================================
		# Set-point Control [ n/a, n/a, heading, alt ]
		#=========================================================
		if t < 0:
			uc = Matrix[System.Double]('[0; 0; %s; %s]' %(param.GetValue(StateVarKey[System.Double]("HEADING")), uav.alt_c))
			
		else:
			uc = Matrix[System.Double]('[0; 0; %s; %s]' %(psi + psi_c, uav.alt_c))
			
		# state matrix
		x = Matrix[System.Double]('[%s; %s; %s; %s; %s; %s; %s; %s; %s; %s; %s]' %(u, w, q, theta, v, p, r, phi, psi, zi, tau))
		dx = (A - B*K)*x + B*(K*F + N)*uc
		
		du = dx[1]
		dv = dx[5]
		dw = dx[2]
		dp = dx[6]
		dq = dx[3]
		dr = dx[7]
		dxi = Vi[1]
		dyi = Vi[2]
		dzi = Vi[3]
		dphi = dx[8]
		dtheta = dx[4]
		dpsi = dx[9]
		dtau = dx[11]
		
		
		#=========================================================
		# Return Derivative States to Integrator
		#=========================================================
		dy = Matrix[System.Double](16,1)
		# calculated dist/deg Latitude using polar radius 6356.7523km (WGS-84)
		dy[1] = dxi / 110946 # conversion from meters to degrees
		# calculated dist/degree Longitude using equatorial radius 6378.1370km (WGS-84)
		dy[2] = dyi / (System.Math.Cos(xned*pi/180) * 111319) # conversion form meters to degrees
		dy[3] = dzi
		dy[4] = du
		dy[5] = dv
		dy[6] = dw
		dy[7] = dp
		dy[8] = dq
		dy[9] = dr
		dy[10] = dxi
		dy[11] = dyi
		dy[12] = -dzi
		dy[13] = dphi
		dy[14] = dtheta
		dy[15] = dpsi
		dy[16] = dtau

		return dy

	def NED2body(self, phi, theta, psi):
		sinPhi   = System.Math.Sin(phi)
		sinTheta = System.Math.Sin(theta)
		sinPsi   = System.Math.Sin(psi)

		cosPhi   = System.Math.Cos(phi)
		cosTheta = System.Math.Cos(theta)
		cosPsi   = System.Math.Cos(psi)

		dcm = Matrix[System.Double](3,3)
		dcm[1,1] = cosTheta*cosPsi
		dcm[1,2] = cosTheta*sinPsi
		dcm[1,3] = -sinTheta
		dcm[2,1] = sinPhi*sinTheta*cosPsi - cosPhi*sinPsi
		dcm[2,2] = sinPhi*sinTheta*sinPsi + cosPhi*cosPsi
		dcm[2,3] = sinPhi*cosTheta
		dcm[3,1] = cosPhi*sinTheta*cosPsi + sinPhi*sinPsi
		dcm[3,2] = cosPhi*sinTheta*sinPsi - sinPhi*cosPsi
		dcm[3,3] = cosPhi*cosTheta
		return dcm
	
	def Roll(self, phi):
		X = Matrix[System.Double](3,3)
		X[1,1] = 1
		X[1,2] = 0
		X[1,3] = 0
		X[2,1] = 0
		X[2,2] = System.Math.Cos(phi)
		X[2,3] = -System.Math.Sin(phi)
		X[3,1] = 0
		X[3,2] = System.Math.Sin(phi)
		X[3,3] = System.Math.Cos(phi)
		return X

	def Pitch(self, theta):
		Y = Matrix[System.Double](3,3)
		Y[1,1] = System.Math.Cos(theta)
		Y[1,2] = 0
		Y[1,3] = -System.Math.Sin(theta)
		Y[2,1] = 0
		Y[2,2] = 1
		Y[2,3] = 0
		Y[3,1] = System.Math.Sin(theta)
		Y[3,2] = 0
		Y[3,3] = System.Math.Cos(theta)
		return Y
		
	def Yaw(self, psi):
		Z = Matrix[System.Double](3,3)
		Z[1,1] = System.Math.Cos(psi)
		Z[1,2] = System.Math.Sin(psi)
		Z[1,3] = 0
		Z[2,1] = -System.Math.Sin(psi)
		Z[2,2] = System.Math.Cos(psi)
		Z[2,3] = 0
		Z[3,1] = 0
		Z[3,2] = 0
		Z[3,3] = 1
		return Z
		
	def rotateCurve(self, psi, pts):
		#=========================================================
		# Rotate Curve Points X, Y in pts
		#---------------------------------------------------------
		# psi is the angle the points are rotated
		# pts is a [2x1] Matrix containing points x and y
		#=========================================================
		Z = Matrix[System.Double](2,2)
		Z[1,1] = round( System.Math.Cos(-psi), 10)
		Z[1,2] = round( System.Math.Sin(-psi), 10)
		Z[2,1] = -round( System.Math.Sin(-psi), 10)
		Z[2,2] = round( System.Math.Cos(-psi), 10)
		
		pts = Z*pts
		return pts[1], pts[2]
	