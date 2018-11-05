class SpaceAgent:
	def __init__(self, update, start_pt, end_pt, target_pt):
		self.visited = False
		self.update = update
		self.count = 0
		self.occupied = False
		self.occ_update = 1
		self.occ_count = 0
		
		self.bounds_start_pt = start_pt
		self.bounds_end_pt = end_pt
		self.bounds_target_pt = target_pt