from math import sin, cos, acos, atan2, sqrt, pi
# Convert coordinates

def Spherical2Cartesian(r, theta, phi):
	'''
	Transforms spherical coordinates to Cartesian
	r = radius of sphere
	theta = angle measured east along the equatorial  plane [deg]
	phi = measured from north to south [deg]
	'''
	theta = theta * (pi/180)
	phi = phi * (pi/180)
	
	x = r*sin(phi)*cos(theta)
	y = r*sin(phi)*sin(theta)
	z = r*cos(phi)
	
	return [x, y, z]
	
def Cartesian2Spherical(x, y, z):
	'''
	Transforms Cartesian coordinates to spherical.
	Returns angles in radians
	x = coordinate along the x-axis
	y = coordinate along the y-axis
	z = coordinate along the z-axis
	'''
	r = sqrt(x*x + y*y + z*z)
	theta = atan2(y/x)
	phi = atan2( sqrt(x*x + y*y)/z )
	
	return [r, theta, phi]
	
def V1toV2arcLength(r, v1, v2):
	'''
	Calculates the arc length between 2 vectors.
	r = radius of sphere
	v1 = vector A, position 1
	v2 = vector B, position 2
	'''
	AdotB = v1[0]*v2[0] + v1[1]*v2[1] + v1[2]*v2[2]
	Amag = sqrt(v1[0]**2 + v1[1]**2 + v1[2]**2)
	Bmag = sqrt(v2[0]**2 + v2[1]**2 + v2[2]**2)
	angle = acos(AdotB/(Amag*Bmag))
	
	# calculate the arc length distance between v1 and v2
	return angle*r