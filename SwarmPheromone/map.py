import System
from System import Random

import imp
grid = imp.load_source('grid.py', '..\\..\\..\\SwarmPheromone\\grid.py')
gridpoint = imp.load_source('gridpoint.py', '..\\..\\..\\SwarmPheromone\\gridpoint.py')
vehicle = imp.load_source('vehicle.py', '..\\..\\..\\SwarmPheromone\\vehicle.py')
spaceagent = imp.load_source('vehicle.py', '..\\..\\..\\SwarmPheromone\\spaceagent.py')

class Map:
	def __init__(self, num_rows, num_cols):
		self.num_rows = num_rows
		self.num_cols = num_cols
		self.track_grid = grid.Grid(num_cols, num_rows, 0.0)
		self.visit_grid = grid.Grid(num_cols, num_rows, 0.0)
		self.searched_grid = grid.Grid(num_cols, num_rows, None)
		#self.entities = []
		self.target_pt = [] # location pt of target (pheromone pump)
	

def within_bounds(world, pt):
	"""Check if pt is within platform bounds"""
	return (pt.col >= 0 and pt.col < world.num_cols and
		pt.row >= 0 and pt.row < world.num_rows)
		
def check_location(world, uav, Si):
	space1 = grid.get_cell(world.searched_grid, uav.prev_pt)
	space2 = grid.get_cell(world.searched_grid, uav.pos_pt)
	
	# previous position moving to new position
	if (Si[0] > space1.bounds_start_pt.col and Si[0] < space1.bounds_end_pt.col):
		if (Si[1] > space1.bounds_start_pt.row and Si[1] < space1.bounds_end_pt.row):
			return True
	# arriving at new position
	if (Si[0] > space2.bounds_start_pt.col and Si[0] < space2.bounds_end_pt.col):
		if (Si[1] > space2.bounds_start_pt.row and Si[1] < space2.bounds_end_pt.row):
			return True
	
	return False
		
def get_attitude_range(world, attOptions, uav):
	"""Returns spaces not occupied from the attOptions vector"""
	row = uav.pos_pt.row
	col = uav.pos_pt.col
	
	new_pos = gridpoint.GridPoint(row, col)
	nextAttitude = []
	for val in attOptions:			
		if val == 0:
			new_pos = gridpoint.GridPoint(row+1, col)
			if (within_bounds(world, new_pos) and grid.get_cell(world.searched_grid, new_pos).occupied == False):
				nextAttitude.append(val)
		if val == 1:
			new_pos = gridpoint.GridPoint(row+1, col+1)
			if (within_bounds(world, new_pos) and grid.get_cell(world.searched_grid, new_pos).occupied == False):
				nextAttitude.append(val)
		if val == 2:
			new_pos = gridpoint.GridPoint(row, col+1)
			if (within_bounds(world, new_pos) and grid.get_cell(world.searched_grid, new_pos).occupied == False):
				nextAttitude.append(val)
		if val == 3:
			new_pos = gridpoint.GridPoint(row-1, col+1)
			if (within_bounds(world, new_pos) and grid.get_cell(world.searched_grid, new_pos).occupied == False):
				nextAttitude.append(val)
		if val == 4:
			new_pos = gridpoint.GridPoint(row-1, col)
			if (within_bounds(world, new_pos) and grid.get_cell(world.searched_grid, new_pos).occupied == False):
				nextAttitude.append(val)
		if val == 5:
			new_pos = gridpoint.GridPoint(row-1, col-1)
			if (within_bounds(world, new_pos) and grid.get_cell(world.searched_grid, new_pos).occupied == False):
				nextAttitude.append(val)
		if val == 6:
			new_pos = gridpoint.GridPoint(row, col-1)
			if (within_bounds(world, new_pos) and grid.get_cell(world.searched_grid, new_pos).occupied == False):
				nextAttitude.append(val)
		if val == 7:
			new_pos = gridpoint.GridPoint(row+1, col-1)
			if (within_bounds(world, new_pos) and grid.get_cell(world.searched_grid, new_pos).occupied == False):
				nextAttitude.append(val)
		
	return nextAttitude

def drop_flavor(world, mgrid, pt, width, height, flavor):
	"""Drops pheromone flavor at specified location/range"""
	# nested loop to account for a target area instead of a point
	for row in range(0, height):
		for col in range(0, width):
			drop_pt = gridpoint.GridPoint(pt.row + row, pt.col + col)
			if within_bounds(world, drop_pt):
				grid.set_cell(mgrid, drop_pt, flavor.deposit)
				if flavor.name == 'lawn':
					world.target_pt.append(drop_pt)
					#if I have a target area, that's what the 2 for-loops are for.
					#line 33 sets the pheromone on the location
					#if the flavor passed in is a lawn flavor, append the point into the target point list
				

def propagate_flavor(world, mgrid, flavor):
	"""Propagate the pheromone flavor throughout platform neighbor spaces"""
	temp = []
	deposit = 0 # auto-pump at target every propagation 
	
	if flavor.name == 'lawn':
		flavor.count += 1
		
	for row in range(0, world.num_rows):
		temp.append([])
		
		for col in range(0, world.num_cols):
			
			s = grid.get_cell(mgrid, gridpoint.GridPoint(row, col))
			# check for external deposits
			if flavor.name == 'lawn':
				for pt in world.target_pt:
					# for-loop in case target pos is a target area
					# if current pos is a tgtPos (auto-pump) assign new value to cell
					if row == pt.row and col == pt.col:
						if flavor.count == flavor.update:
							deposit = flavor.deposit
							grid.set_cell(mgrid, pt, s + flavor.deposit)
							flavor.count = 0
					else:
						deposit = 0
			
			g = get_neighbor(world, mgrid, flavor, row, col)
			s_new = flavor.Ef * ((1-flavor.Gf)*(s + deposit) + g)
			if s_new < flavor.Tf:
				s_new = 0
			
			temp[row].append(float(s_new))
			
		
	mgrid.cells = temp

def searched_grid_agents(world, mgrid, update, radius):
	"""creates space agents for searched_grid"""
	interval = 2*radius
	
	print 'Creating space agents'
	for row in range(0, world.num_rows):
		for col in range(0, world.num_cols):
			currentPos = gridpoint.GridPoint(row, col)
			bound_start = gridpoint.GridPoint(row*interval, col*interval)
			bound_target = gridpoint.GridPoint(row*interval + radius, col*interval + radius)
			bound_end = gridpoint.GridPoint((row+1)*interval, (col+1)*interval)
			grid.set_cell(mgrid, currentPos, spaceagent.SpaceAgent(update, bound_start, bound_end, bound_target))
	print 'All space agents are in place'
		
def searched_grid_count_update(world, mgrid):
	"""updates the count value for visited spaces"""
	for row in range(0, world.num_rows):
		for col in range(0, world.num_cols):
			currentPos = gridpoint.GridPoint(row, col)
			space = grid.get_cell(world.searched_grid, currentPos)
			if (space.count == space.update):
				space.count = 0
				space.visited = False
			else:
				space.count += 1
				
			grid.set_cell(world.searched_grid, currentPos, space)

def make_move(world, uav, m, m_idx, V, V_occ):
	row = uav.pos_pt.row
	col = uav.pos_pt.col
	new_pos = 0
	uav.alt_c = 0 # control altitude at steady state
	
	#=========================================================
	# Possible Final Vehicle Attitude Options
	#=========================================================
	# attOptions is being used as direction of next target space.
	# get possible directions based on current uav attitude (-90, -45, 0, 45, 90)
	attOptionsRaw = [uav.attitude - 2, uav.attitude - 1, uav.attitude,
		uav.attitude + 1, uav.attitude + 2]
	attOptions = []
	# make sure the range is between 0-7
	for att in attOptionsRaw:
		if (att < 0):
			attOptions.append(8 + att)
		elif (att > 7):
			attOptions.append(att - 8)
		else:
			attOptions.append(att)
		
		
	nextAttConcent = []
	nextAttitude = []
	
	# move aircraft to open space, otherwise move to an unoccupied space, otherwise stay until next round
	#=========================================================
	# Select Favorable Target Space Directions
	#=========================================================
	# When vehicle is at starting space and there's no 
	# pheromone concentration, look for unoccupied target 
	# spaces. If the vehicle is surrounded then stay until
	# the other vehicles have moved.
	# When everything is normal, append all probable target
	# space with a positive pheromone concentration into list.
	#=========================================================
	if uav.pos_pt == uav.path[0]:
		# Find open space with gradient
		for att in attOptions:
			if (V_occ[att] > 0):
				nextAttConcent.append(V_occ[att])
				nextAttitude.append(att)
		# Find an unoccupied space
		if not nextAttitude:
			nextAttitude = get_attitude_range(world, attOptions, uav)
			
			# Stay in the starting location until next iteration
			if not nextAttitude:
				uav.path.append(uav.pos_pt)
				return
		
	else:		
		# check for possible new position. Get value to find gradient
		for att in attOptions:
			if (V_occ[att] > 0):
				nextAttConcent.append(V_occ[att])
				nextAttitude.append(att)
				
	
	# if list is empty, find a space that is not occupied
	#=========================================================
	# Select Target Space Directions with No Gradient
	#=========================================================
	# When the vehicle is not at the starting space and there
	# is no positive pheromone gradient within the all the
	# possible direction list (nextAttitude list is empty),
	# check for spaces not occupied within the attOptions list.
	#
	# When there are no spaces available, check if the
	# options are within bounds and append that as a possible
	# target position. Climb to higher altitude in order to
	# avoid collision. Direction is then chosen at random
	# from the list of possible occupied target spaces.
	#=========================================================
	if not nextAttitude:
		nextAttitude = get_attitude_range(world, attOptions, uav)
		
		# if surrounded, check if spots are within map bounds
		if not nextAttitude:
			for att in attOptions:
				if att == 0:
					new_pos = gridpoint.GridPoint(row+1, col)
					if (within_bounds(world, new_pos)):
						nextAttitude.append(att)
				if att == 1:
					new_pos = gridpoint.GridPoint(row+1, col+1)
					if (within_bounds(world, new_pos)):
						nextAttitude.append(att)
				if att == 2:
					new_pos = gridpoint.GridPoint(row, col+1)
					if (within_bounds(world, new_pos)):
						nextAttitude.append(att)
				if att == 3:
					new_pos = gridpoint.GridPoint(row-1, col+1)
					if (within_bounds(world, new_pos)):
						nextAttitude.append(att)
				if att == 4:
					new_pos = gridpoint.GridPoint(row-1, col)
					if (within_bounds(world, new_pos)):
						nextAttitude.append(att)
				if att == 5:
					new_pos = gridpoint.GridPoint(row-1, col-1)
					if (within_bounds(world, new_pos)):
						nextAttitude.append(att)
				if att == 6:
					new_pos = gridpoint.GridPoint(row, col-1)
					if (within_bounds(world, new_pos)):
						nextAttitude.append(att)
				if att == 7:
					new_pos = gridpoint.GridPoint(row+1, col-1)
					if (within_bounds(world, new_pos)):
						nextAttitude.append(att)
						
				uav.alt_c = 50 # increase alt by 50m to avoid collisions
	
	# When gradient approach was not available				
	if not nextAttConcent:
		# choose a random direction from recently visited or possible occupied spaces
		rand = Random()
		idx = rand.Next(0, len(nextAttitude))
		m_idx = nextAttitude[idx]
		
	else:
		# find gradient to follow and/or choose a random direction of multiple equal gradients
		m = max(nextAttConcent)
		m_idx = [i for i, j in enumerate(V_occ) if j == m]
		rand = Random()
		idx = rand.Next(0, len(m_idx))
		
		m_idx = m_idx[idx]
	
	attAngle = m_idx 
	
	uav.prevAttitude = uav.attitude
	# Get direction of space location
	if (attAngle == attOptions[0]):
		uav.turnAngle = -90
		uav.attitude = uav.attitude - 2
		if (uav.attitude < 0):
			uav.attitude = 8 + uav.attitude
		if (uav.attitude > 7):
			uav.attitude = uav.attitude - 8
			
	elif (attAngle == attOptions[1]):
		uav.turnAngle = -45
		uav.attitude = uav.attitude - 0
		if (uav.attitude < 0):
			uav.attitude = 8 + uav.attitude
			
	elif (attAngle == attOptions[2]):
		uav.turnAngle = 0
		
	elif (attAngle == attOptions[3]):
		uav.turnAngle = 45
		uav.attitude = uav.attitude + 0
		if (uav.attitude > 7):
			uav.attitude = uav.attitude - 8
			
	elif (attAngle == attOptions[4]):
		uav.turnAngle = 90
		uav.attitude = uav.attitude + 2
		if (uav.attitude > 7):
			uav.attitude = uav.attitude - 8
		if (uav.attitude < 0):
			uav.attitude = 8 + uav.attitude
			
	else:
		print ('out of turning bounds', m_idx, attOptions[0], attOptions[1], attOptions[2], 
			attOptions[3], attOptions[4], m_idx == attOptionsRaw[0], m_idx == attOptionsRaw[1],
			m_idx == attOptionsRaw[2], m_idx == attOptionsRaw[3], m_idx == attOptionsRaw[4],
			type(m_idx), type(attOptions[0]))
			
			
	# Create point for new position
	if m_idx == 0: # UP
		new_pos = gridpoint.GridPoint(row+1, col)
	elif m_idx == 1: # TOP-RIGHT
		new_pos = gridpoint.GridPoint(row+1, col+1)
	elif m_idx == 2: # RIGHT
		new_pos = gridpoint.GridPoint(row, col+1)
	elif m_idx == 3: # BOTTOM-RIGHT
		new_pos = gridpoint.GridPoint(row-1, col+1)
	elif m_idx == 4: # DOWN
		new_pos = gridpoint.GridPoint(row-1, col)
	elif m_idx == 5: # BOTTOM-LEFT
		new_pos = gridpoint.GridPoint(row-1, col-1)
	elif m_idx == 6: # LEFT
		new_pos = gridpoint.GridPoint(row, col-1)
	elif m_idx == 7: # TOP-LEFT
		new_pos = gridpoint.GridPoint(row+1, col-1)
		
			
	drop_flavor(world, world.visit_grid, new_pos, 1, 1, uav.repel)
	
	# make current space available for other units to move into
	space = grid.get_cell(world.searched_grid, uav.pos_pt)
	space.occupied = False
	grid.set_cell(world.searched_grid, uav.pos_pt, space)
	
	# change values of next space
	space = grid.get_cell(world.searched_grid, new_pos)
	space.visited = True
	space.occupied = True
	grid.set_cell(world.searched_grid, new_pos, space)
	
	uav.prev_pt = uav.pos_pt
	uav.pos_pt = new_pos
	uav.path.append(new_pos)


def pick_move(world, uav):
	"""Get gradient at neighbor pos depending on current pos"""
	# if space has been visited then pheromone value is zero until available
	# get pheromone values and get pheromone combines with visited grid
	row = uav.pos_pt.row
	col = uav.pos_pt.col
	
	# middle platform grid
	if row > 0 and row+1 < world.num_rows and col > 0 and col+1 < world.num_cols:
		rgt = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row, col+1)).visited):
			rgt_occ = rgt
		else:
			rgt_occ = rgt
			
		lft = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row, col-1)).visited):
			lft_occ = lft
		else:
			lft_occ = lft
		
		dwn = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col)).visited):
			dwn_occ = dwn
		else:
			dwn_occ = dwn
			
		up = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col)).visited):
			up_occ = up
		else: 
			up_occ = up
			
		br = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col+1)).visited):
			br_occ = br
		else:
			br_occ = br
			
		tr = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col+1)).visited):
			tr_occ = tr
		else:
			tr_occ = tr
			
		bl = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col-1)).visited):
			bl_occ = bl
		else:
			bl_occ = bl
			
		tl = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col-1)).visited):
			tl_occ = tl
		else:
			tl_occ = tl
		
		V = [up, tr, rgt, br, dwn, bl, lft, tl]
		V_occ = [up_occ, tr_occ, rgt_occ, br_occ, dwn_occ, bl_occ, lft_occ, tl_occ]
		
	# middle-left grid
	elif row > 0 and row+1 < world.num_rows and col == 0:
		rgt = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row, col+1)).visited):
			rgt_occ = rgt
		else:
			rgt_occ = rgt
			
		dwn = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col)).visited):
			dwn_occ = dwn
		else:
			dwn_occ = dwn
			
		up = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col)).visited):
			up_occ = up
		else: 
			up_occ = up
			
		br = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col+1)).visited):
			br_occ = br
		else:
			br_occ = br
			
		tr = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col+1)).visited):
			tr_occ = tr
		else:
			tr_occ = tr
		
		V = [up, tr, rgt, br, dwn, 0, 0, 0]
		V_occ = [up_occ, tr_occ, rgt_occ, br_occ, dwn_occ, 0, 0, 0]
			
	# bottom-middle grid
	elif row == 0 and col > 0 and col+1 < world.num_cols:
		rgt = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row, col+1)).visited):
			rgt_occ = rgt
		else:
			rgt_occ = rgt
			
		lft = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row, col-1)).visited):
			lft_occ = lft
		else:
			lft_occ = lft
		
		up = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col)).visited):
			up_occ = up
		else: 
			up_occ = up
		
		tr = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col+1)).visited):
			tr_occ = tr
		else:
			tr_occ = tr
			
		tl = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col-1)).visited):
			tl_occ = tl
		else:
			tl_occ = tl
		
		V = [up, tr, rgt, 0, 0, 0, lft, tl]
		V_occ = [up_occ, tr_occ, rgt_occ, 0, 0, 0, lft_occ, tl_occ]
			
	# top-middle grid
	elif row+1 == world.num_rows and col > 0 and col+1 < world.num_cols:
		rgt = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row, col+1)).visited):
			rgt_occ = rgt
		else:
			rgt_occ = rgt
			
		lft = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row, col-1)).visited):
			lft_occ = lft
		else:
			lft_occ = lft
			
		dwn = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col)).visited):
			dwn_occ = dwn
		else:
			dwn_occ = dwn
			
		br = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col+1)).visited):
			br_occ = br
		else:
			br_occ = br
			
		bl = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col-1)).visited):
			bl_occ = bl
		else:
			bl_occ = bl
		
		V = [0, 0, rgt, br, dwn, bl, lft, 0]
		V_occ = [0, 0, rgt_occ, br_occ, dwn_occ, bl_occ, lft_occ, 0]
		
	# middle-right grid
	elif row > 0 and row+1 < world.num_rows and col+1 == world.num_cols:
		lft = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row, col-1)).visited):
			lft_occ = lft
		else:
			lft_occ = lft
			
		dwn = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col)).visited):
			dwn_occ = dwn
		else:
			dwn_occ = dwn
			
		up = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col)).visited):
			up_occ = up
		else: 
			up_occ = up
			
		bl = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col-1)).visited):
			bl_occ = bl
		else:
			bl_occ = bl
			
		tl = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col-1)).visited):
			tl_occ = tl
		else:
			tl_occ = tl
		
		V = [up, 0, 0, 0, dwn, bl, lft, tl]
		V_occ = [up_occ, 0, 0, 0, dwn_occ, bl_occ, lft_occ, tl_occ]
			
	# top-left corner grid
	elif row+1 == world.num_rows and col == 0:
		rgt = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row, col+1)).visited):
			rgt_occ = rgt
		else:
			rgt_occ = rgt
			
		dwn = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col)).visited):
			dwn_occ = dwn
		else:
			dwn_occ = dwn
			
		br = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col+1)).visited):
			br_occ = br
		else:
			br_occ = br
		
		V = [0, 0, rgt, br, dwn, 0, 0, 0]
		V_occ = [0, 0, rgt_occ, br_occ, dwn_occ, 0, 0, 0]
		
	# bottom-left corner grid
	elif row == 0 and col == 0:
		rgt = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row, col+1)).visited):
			rgt_occ = rgt
		else:
			rgt_occ = rgt
			
		up = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col)).visited):
			up_occ = up
		else: 
			up_occ = up
			
		tr = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col+1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col+1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col+1)).visited):
			tr_occ = tr
		else:
			tr_occ = tr
		
		V = [up, tr, rgt, 0, 0, 0, 0, 0]
		V_occ = [up_occ, tr_occ, rgt_occ, 0, 0, 0, 0, 0]
		
	# top-right corner grid
	elif row+1 == world.num_rows and col+1 == world.num_cols:
		lft = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row, col-1)).visited):
			lft_occ = lft
		else:
			lft_occ = lft
			
		dwn = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col)).visited):
			dwn_occ = dwn
		else:
			dwn_occ = dwn
			
		bl = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row-1, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row-1, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row-1, col-1)).visited):
			bl_occ = bl
		else:
			bl_occ = bl
		
		V = [0, 0, 0, 0, dwn, bl, lft, 0]
		V_occ = [0, 0, 0, 0, dwn_occ, bl_occ, lft_occ, 0]
			
	# bottom-right corner grid
	elif row == 0 and col+1 == world.num_cols:
		lft = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row, col-1)).visited):
			lft_occ = lft
		else:
			lft_occ = lft
			
		up = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col)).visited):
			up_occ = up
		else: 
			up_occ = up
			
		tl = (grid.get_cell(world.track_grid, gridpoint.GridPoint(row+1, col-1))
			- grid.get_cell(world.visit_grid, gridpoint.GridPoint(row+1, col-1)))
		if (not grid.get_cell(world.searched_grid, gridpoint.GridPoint(row+1, col-1)).visited):
			tl_occ = tl
		else:
			tl_occ = tl
		
		V = [up, 0, 0, 0, 0, 0, lft, tl]
		V_occ = [up_occ, 0, 0, 0, 0, 0, lft_occ, tl_occ]

	
	m = max(V) # max value used to match on list
	
	m_occ = max(V_occ)
	# return values i for variables i,j in enumerate (which returns i, j (0,'a')) if max values repeat (j == m)
	m_idx = [i for i, j in enumerate(V) if j == m]

	make_move(world, uav, m, m_idx, V, V_occ)
	
		
	
def get_neighbor(world, mgrid, flavor, row, col):
	# external deposit has been included into cell before calling onto get_neighbor()
	# Quadrant I: (0,0) is at the Origin. (end_row,end_col) is at the top right
	rgt = lft = dwn = up = br = tr = bl = tl = 0
	Gf = flavor.Gf
	N = 8 # number of neighboring spaces
	
	# center
	if row > 0 and row+1 < world.num_rows and col > 0 and col+1 < world.num_cols:
		# get pheromone value at neighboring cells
		rgt = grid.get_cell(mgrid, gridpoint.GridPoint(row, col+1))
		lft = grid.get_cell(mgrid, gridpoint.GridPoint(row, col-1))
		dwn = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col))
		up = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col))
		br = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col+1))
		tr = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col+1))
		bl = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col-1))
		tl = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col-1)) 

		g = Gf/N * (rgt + lft + up + dwn + tr + br + tl + bl)
	
	# middle-left grid
	elif row > 0 and row+1 < world.num_rows and col == 0:
		
		rgt = grid.get_cell(mgrid, gridpoint.GridPoint(row, col+1))
		dwn = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col))
		up = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col))
		br = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col+1))
		tr = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col+1))
		
		g = Gf/N * (rgt + up + dwn + tr + br)
	
	# bottom-middle grid
	elif row == 0 and col > 0 and col+1 < world.num_cols:
		
		rgt = grid.get_cell(mgrid, gridpoint.GridPoint(row, col+1))
		lft = grid.get_cell(mgrid, gridpoint.GridPoint(row, col-1))
		up = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col))
		tr = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col+1))
		tl = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col-1))
		
		g = Gf/N * (rgt + lft + up + tr + tl)
	
	# top-middle grid
	elif row == world.num_rows-1 and col > 0 and col+1 < world.num_cols:
		
		rgt = grid.get_cell(mgrid, gridpoint.GridPoint(row, col+1))
		lft = grid.get_cell(mgrid, gridpoint.GridPoint(row, col-1))
		dwn = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col))
		br = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col+1))
		bl = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col-1))
		
		g = Gf/N * (rgt + lft + dwn + br + bl)
	
	# middle-right grid
	elif row > 0 and row+1 < world.num_rows and col == world.num_cols-1:
		
		lft = grid.get_cell(mgrid, gridpoint.GridPoint(row, col-1))
		dwn = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col))
		up = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col))
		bl = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col-1))
		tl = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col-1))
		
		g = Gf/N * (lft + dwn + up + bl + tl)
	
	# bottom-left corner grid
	elif row == 0 and col == 0:
		
		rgt = grid.get_cell(mgrid, gridpoint.GridPoint(row, col+1))
		up = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col))
		tr = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col+1))
		
		g = Gf/N * (rgt + up + tr)
	
	# top-left corner grid
	elif row == world.num_rows-1 and col == 0:
		
		rgt = grid.get_cell(mgrid, gridpoint.GridPoint(row, col+1))
		dwn = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col))
		br = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col+1))
		
		g = Gf/N * (rgt + dwn + br)
	
	# bottom-right corner grid
	elif row == 0 and col == world.num_cols-1:
		
		lft = grid.get_cell(mgrid, gridpoint.GridPoint(row, col-1))
		up = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col))
		tl = grid.get_cell(mgrid, gridpoint.GridPoint(row+1, col-1))
		
		g = Gf/N * (lft + up + tl)
	
	# top-right corner grid
	elif row == world.num_rows-1 and col == world.num_cols-1:
		
		lft = grid.get_cell(mgrid, gridpoint.GridPoint(row, col-1))
		dwn = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col))
		bl = grid.get_cell(mgrid, gridpoint.GridPoint(row-1, col-1))
		
		g = Gf/N * (lft + dwn + bl)
		
	return g
