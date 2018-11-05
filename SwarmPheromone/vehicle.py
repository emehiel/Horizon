class Vehicle:
	def __init__(self, entity_name, pos_pt, repel_flavor, attitude, start_pt_m, pos_pt_m):
		self.entity_name = entity_name
		self.pos_pt = pos_pt
		self.prev_pt = pos_pt
		self.repel = repel_flavor
		self.prevAttitude = attitude
		self.attitude = attitude # 0-7 (clockwise direction North = 0)
		self.turnAngle = 0 # turn angle in deg to go from cur pos to tgt pos (0,45,90)
		self.prevHead = 0 # previous heading before maneuver start. used to calculate cmd heading (no frenet-serret)
		self.path = [pos_pt]
		self.start_pt_m = start_pt_m # starts at (0,0) and updates to current position (new start pt) after every maneuver 
		self.pos_pt_m = pos_pt_m # start pos in meters determined by size of pheromone map
		self.turnIndex = 2 # index of target curve location (28 locations) uses Matrix lib (start at 1 not 0)
		self.waypoint = 0 # 0 = waypoint not reached yet, 1 = waypoint reached
		self.interval = 0 # distance interval between current pos to target space at start of maneuver
		self.alt_c = 0 # altitude control: 0 = steady state, # = altitude change in [m]
		self.needTarget = True
		
		self.t_start = 0
		self.tmanTot = 10
		self.TperMan = 0
		self.dt = 0.1
		
		self.init = 1 #used to enter loop the first time at t=0 @ aircraft.py line 114
		
		# position dependent approach conditional (change to False as uav reaches waypoint to next stage)
		self.stage1 = True
		self.stage2 = True
		self.stage3 = True