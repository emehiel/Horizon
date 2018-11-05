import sys
import math
from coordtransform import *

deg2rad = math.pi/180
rad2deg = 1/deg2rad

if ( len(sys.argv) > 1):
	# [0]: script name
	# [1]: swarm size (1-5)
	# [2]: visited pheromone update value (8-12)
	argv_uavs = int(sys.argv[1])
	argv_vupdate = int(sys.argv[2])
	argv_vdeposit = int(sys.argv[3])
	argv_vthresh = float(sys.argv[4])


numUAV = argv_uavs

UniverseType = 'scripted'
UniverseSrc = '..\..\..\SwarmPheromone\environment.py'

#AssetPos = [37, 122] # deg N and W (Santa Cruz)
AssetPos = [34.726, 120.577] # deg N and W (Vandenberg AFB)
CruiseSpeed = 36 # 70 knots
Altitude  = 1801 # 5908.8 ft level flight at 70 knots (36 m/s)
DyamicStateType = 'DYNAMIC_LLA' # 'DYNAMIC_ECI'
EOMSType = 'scripted'
eomSrc = '..\..\..\SwarmPheromone\eomSwarm.py'

Radius = 180 # m radius based on distance traveled per maneuver time
CamRadius = Radius * 1.4 # m camera view radius based on grid square diagonal, sqrt(2)


# use distance to target to get map size
#TargetPos = [38.6, 121.5] # Sacramento (~120 miles)
#TargetPos = [37.3, 121.9] # San Jose (~30 miles)
#TargetPos = [37.12, 122.12] # Boulder Creek(~12 miles)
TargetPos = [34.804, 120.603] # Narlon, CA
# get vector for target and asset
AssetVec = Spherical2Cartesian(6378000, 360-AssetPos[1], 90-AssetPos[0])
TargetVec = Spherical2Cartesian(6378000, 360-TargetPos[1], 90-TargetPos[0])
# get the arc length between both vectors
arcLength = V1toV2arcLength(6378000, AssetVec, TargetVec)
# get number of grids for map based on camera view (Radius)
GridNumber = int( arcLength/(Radius*2) * 1.0)
# get component distance using the Target as base
TargetLonCompVec = Spherical2Cartesian(6378000, 360-AssetPos[1], 90-TargetPos[0]) # horizontal comp at target
TargetLatCompVec = Spherical2Cartesian(6378000, 360-TargetPos[1], 90-AssetPos[0]) # vertical comp at target 
LatMeters = V1toV2arcLength(6378000, TargetLatCompVec, TargetVec) # vertical length at target Rows
LonMeters = V1toV2arcLength(6378000, TargetLonCompVec, TargetVec) # horizontal length at target Cols
# calculate grid separation components between asset and target
Rows = int(LatMeters/(Radius*2))
Cols = int(LonMeters/(Radius*2))
# calculate direction of separation components
signRow = int((AssetPos[0] - TargetPos[0]) / abs(AssetPos[0] - TargetPos[0]))
signCol = int((AssetPos[1] - TargetPos[1]) / abs(AssetPos[1] - TargetPos[1]))


# platform attributes
totalCols = GridNumber*2
totalRows = GridNumber*2

#pheromone values
lawnUpdate = 10
lawnProp = 0.75
lawnEvap = 0.03
lawnThreshold = 1e-300
lawnDeposit = 20.0
visitUpdate = argv_vupdate
visitProp = 0.0 # no propagation for visited space
visitEvap = 0.3
visitThreshold = argv_vthresh #1e-35
visitDeposit = argv_vdeposit
# target location and range
lawnCol = GridNumber
lawnRow = GridNumber
lawnWidth = 1
lawnHeight = 1

# swarm attributes
UAVstartCol = GridNumber + signCol*Cols
UAVstartRow = GridNumber + signRow*Rows
UAVattitude = 0 # 0-7 (clockwise direction North = 0)

# calculate the NED starting point for asset
xi0 = ( UAVstartRow * Radius*2 ) + Radius
yi0 = ( UAVstartCol * Radius*2 ) + Radius
ICs = ( '[' + str(AssetPos[0]) + '; ' + str(AssetPos[1]) + '; ' + str(Altitude) + 
		'; 0.0; 0.0; 0.0; 0.0; 0.0; 0.0; 0.0; 0.0; 0.0; 0.0; 0.0; 0.0; 0.0]' )

# set up A state transition matrix
A1 = "[-0.0780,  0.0578, -0.3073, -9.8067, -0.0235,  0.1071,   0.0453,  0,       0,  0,  0.0144;"
A2 = " -0.1109, -1.8798, 33.9237,  0,       0.0100,  0.0005,   0.2491,  0,       0,  0,  0;"
A3 = " -0.0257, -0.0136, -0.2240,  0,      -0.0043,  0.0345,   0.0603,  0,       0,  0,  0;"
A4 = "  0,       0,       1,       0,       0,       0,        0,       0,       0,  0,  0;"
A5 = "  0.1747, -0.0040,  0.6763,  0,      -0.1226,  0.4794, -35.6559,  9.8067,  0,  0,  0;"
A6 = " -0.1114, -0.0057, -0.4548,  0,       0.0708, -3.4288,   0.0514,  0,       0,  0,  0;"
A7 = " -0.0225, -0.0013, -0.0910,  0,       0.0095, -0.0714,  -0.0697,  0,       0,  0,  0;"
A8 = "  0,       0,       0,       0,       0,       1,        0,       0,       0,  0,  0;"
A9 = "  1,       0,       0,       0,       0,       0,        0,       0,       0,  0,  0;"
A10= "  0,      -1,       0,      36,       0,       0,        0,       0,       0,  0,  0;"
A11= "  0,       0,       0,       0,       0,       0,        0,       0,       0,  0, -1]"

# set up B state transition matrix
B1 = "[ 4.73E-03,  5.95E-02,  2.54E-03, 0;"
B2 = " -0.3673,    1.43E-03, -3.98E-03, 0;"
B3 = " -5.61E-05,  1.61E-02, -1.21E-04, 0;"
B4 = "  0,         0,         0,        0;"
B5 = " -4.62E-04, -6.61E-02,  2.12E-02, 0;"
B6 = " -2.92E-04,  4.19E-02, -8.63E-03, 0;"
B7 = " -2.57E-04,  8.91E-03, -2.77E-03, 0;"
B8 = "  0,         0,         0,        0;"
B9 = "  0,         0,         0,        0;"
B10= "  0,         0,         0,        0;"
B11= "  0,         0,         0,        1]"

# set up Controller gains
GainsMtx1 = "[7.6569,    -18.8079,   29.1745,     881.4351,   -0.0351,  -0.4843,    2.6611,    -1.5458,   182.8348,    99.6041,  -0.0156;"
GainsMtx2 = " 345.3983,   1.9796,    5.5087,     -190.8778,   -0.4164,   0.5056,    8.6668,    -3.7181,   3502.0197,  -5.5551,    0.2175;"
GainsMtx3 = " 29.2709,    0.2286,   -33.3854,    -97.9460,     0.7599,   23.4717,  -146.5891,   71.5036,  170.8522,    1.1848,    0.0779;"
GainsMtx4 = " 614.3180,   9.2832,   -1929.8570,  -3330.0058,  -2.1829,   8.6991,   -86.3439,    24.5245,  944.4795,    26.5166,   3.1716]"

# set-point control matrices
F1 = "[0,  0.500000000000000, 0, 0;"
F2 = " 0,  0.029194324007005, 0, 0;"
F3 = " 0,  0,                 0, 0;"
F4 = " 0,  0.000810953444639, 0, 0;"
F5 = " 0,  0.235589577880307, 0, 0;"
F6 = " 0,  0,                 0, 0;"
F7 = " 0, -0.006271580615290, 0, 0;"
F8 = " 0, -0.022297078091798, 0, 0;"
F9 = " 0,  0,                 1, 0;"
F10= " 0,  0,                 0, 1;"
F11= " 0, -0.074556723460457, 0, 0]"

N1 = "[0, -0.292902136516323, 0, 0;"
N2 = " 0,  0.906949080372875, 0, 0;"
N3 = " 0, -0.164812385290060, 0, 0;"
N4 = " 0, -0.074556723460457, 0, 0]"

# create xml file
#fileName = '..\Pheromone_Swarm%s.xml' %str(numUAV)
fileName = '..\Pheromone_Swarm.xml'
f = open(fileName, 'w+')

f.write('<MODEL>\n\n')

f.write('\t<ENVIRONMENT\n')
f.write('\t\tUniverseType = "%s"\n' %UniverseType )
f.write('\t\t\tsrc = "%s"\n' %UniverseSrc )
f.write('\t\t\tclassName = "environment"\n' )

f.write('\t\ttotalCols = "%s"\n' %totalCols)
f.write('\t\ttotalRows = "%s"\n' %totalRows)
f.write('\t\tlawnUpdate = "%s"\n' %lawnUpdate)
f.write('\t\tlawnProp = "%s"\n' %lawnProp)
f.write('\t\tlawnEvap = "%s"\n' %lawnEvap)
f.write('\t\tlawnThreshold = "%s"\n' %lawnThreshold)
f.write('\t\tlawnDeposit = "%s"\n' %lawnDeposit)
f.write('\t\tvisitUpdate = "%s"\n' %visitUpdate)
f.write('\t\tlawnCol = "%s"\n' %lawnCol)
f.write('\t\tlawnRow = "%s"\n' %lawnRow)
f.write('\t\tlawnWidth = "%s"\n' %lawnWidth)
f.write('\t\tlawnHeight = "%s"\n' %lawnHeight)
f.write('\t\tradius = "%s">\n' %Radius)
f.write('\t</ENVIRONMENT>\n\n')

for x in range(numUAV):	
	f.write('\t<ASSET assetName = "%s">\n' %('Asset' + str(x+1)) )
	# Dynamic state and Initial Conditions ----------------------
	f.write('\t\t<DynamicState\n')
	f.write('\t\t\tDynamicStateType = "%s"\n' %DyamicStateType)
	f.write('\t\t\tICs = "%s">\n' %ICs)
	# Equations of Motion
	f.write('\t\t\t<EOMS\n')
	f.write('\t\t\t\tEOMSType = "%s"\n' %EOMSType)
	f.write('\t\t\t\tsrc = "%s"\n' %eomSrc)
	f.write('\t\t\t\tclassName = "eom"\n')
	f.write('\t\t\t\tName = "%s">\n' %('Asset' + str(x+1)) )
	
	f.write('\t\t\t</EOMS>\n')
	
	f.write('\t\t</DynamicState>\n')
	# End of Dynamic state -----------------------------------
	
	# Subsystems ---------------------------------------------
	# Aircraft subsystem
	f.write('\t\t<SUBSYSTEM\n')
	f.write('\t\t\tType = "scripted"\n' )
	f.write('\t\t\tsubsystemName= "Aircraft"\n' )
	f.write('\t\t\t\tsrc = "..\..\..\\aircraft.py"\n' )
	f.write('\t\t\t\tclassName = "aircraft"\n' )
	f.write('\t\t\tUAVstartCol = "%s"\n' %UAVstartCol)
	f.write('\t\t\tUAVstartRow = "%s"\n' %UAVstartRow)
	f.write('\t\t\tUAVattitude = "%s"\n' %UAVattitude)
	f.write('\t\t\tvisitUpdate = "%s"\n' %visitUpdate)
	f.write('\t\t\tvisitProp = "%s"\n' %visitProp) # no propagation for visited space
	f.write('\t\t\tvisitEvap = "%s"\n' %visitEvap)
	f.write('\t\t\tvisitThreshold = "%s"\n' %visitThreshold)
	f.write('\t\t\tvisitDeposit = "%s"\n' %visitDeposit)
	f.write('\t\t\tradius = "%s">\n' %Radius)
	f.write('\t\t</SUBSYSTEM>\n')
	# Controller subsystem
	f.write('\t\t<SUBSYSTEM\n')
	f.write('\t\t\tType = "scripted"\n' )
	f.write('\t\t\tsubsystemName= "Controller"\n' )
	f.write('\t\t\t\tsrc = "..\..\..\controller.py"\n' )
	f.write('\t\t\t\tclassName = "controller"\n' )
	f.write('\t\t\tA = "%s\n' %A1)
	f.write('\t\t\t     %s\n' %A2)
	f.write('\t\t\t     %s\n' %A3)
	f.write('\t\t\t     %s\n' %A4)
	f.write('\t\t\t     %s\n' %A5)
	f.write('\t\t\t     %s\n' %A6)
	f.write('\t\t\t     %s\n' %A7)
	f.write('\t\t\t     %s\n' %A8)
	f.write('\t\t\t     %s\n' %A9)
	f.write('\t\t\t     %s\n' %A10)
	f.write('\t\t\t     %s"\n' %A11)
	f.write('\t\t\tB = "%s\n' %B1)
	f.write('\t\t\t     %s\n' %B2)
	f.write('\t\t\t     %s\n' %B3)
	f.write('\t\t\t     %s\n' %B4)
	f.write('\t\t\t     %s\n' %B5)
	f.write('\t\t\t     %s\n' %B6)
	f.write('\t\t\t     %s\n' %B7)
	f.write('\t\t\t     %s\n' %B8)
	f.write('\t\t\t     %s\n' %B9)
	f.write('\t\t\t     %s\n' %B10)
	f.write('\t\t\t     %s"\n' %B11)
	f.write('\t\t\tGainsMtx = "%s\n' %GainsMtx1)
	f.write('\t\t\t            %s\n' %GainsMtx2)
	f.write('\t\t\t            %s\n' %GainsMtx3)
	f.write('\t\t\t            %s"\n' %GainsMtx4)
	f.write('\t\t\tF = "%s\n' %F1)
	f.write('\t\t\t     %s\n' %F2)
	f.write('\t\t\t     %s\n' %F3)
	f.write('\t\t\t     %s\n' %F4)
	f.write('\t\t\t     %s\n' %F5)
	f.write('\t\t\t     %s\n' %F6)
	f.write('\t\t\t     %s\n' %F7)
	f.write('\t\t\t     %s\n' %F8)
	f.write('\t\t\t     %s\n' %F9)
	f.write('\t\t\t     %s\n' %F10)
	f.write('\t\t\t     %s"\n' %F11)
	f.write('\t\t\tN = "%s\n' %N1)
	f.write('\t\t\t     %s\n' %N2)
	f.write('\t\t\t     %s\n' %N3)
	f.write('\t\t\t     %s">\n' %N4)
	f.write('\t\t</SUBSYSTEM>\n')
	# Motor subsystem
	f.write('\t\t<SUBSYSTEM\n')
	f.write('\t\t\tType = "scripted"\n' )
	f.write('\t\t\tsubsystemName= "Motor"\n' )
	f.write('\t\t\t\tsrc = "..\..\..\motor.py"\n' )
	f.write('\t\t\t\tclassName = "motor"\n' )
	f.write('\t\t\t<IC type = "Double" key = "fuel" value = "0.0"></IC>\n' )
	f.write('\t\t</SUBSYSTEM>\n')
	# Camera subsystem
	f.write('\t\t<SUBSYSTEM\n')
	f.write('\t\t\tType = "scripted"\n' )
	f.write('\t\t\tsubsystemName= "Camera"\n' )
	f.write('\t\t\t\tsrc = "..\..\..\camera.py"\n' )
	f.write('\t\t\t\tclassName = "camera"\n' )
	f.write('\t\t\tRadius = "%s">\n' %CamRadius)
	f.write('\t\t\t<IC type = "Double" key = "targetfound" value = "0.0"></IC>\n' )
	f.write('\t\t\t<IC type = "Double" key = "targetdistance" value = "%s"></IC>\n' %arcLength)
	f.write('\t\t</SUBSYSTEM>\n')
		
	f.write('\t</ASSET>\n\n')
	
f.write('</MODEL>\n')
f.close()
