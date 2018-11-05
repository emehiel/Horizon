# create a grid for the pheromones

class Grid:
	def __init__(self, width, height, occupancy_value):
		self.width = width
		self.height = height
		self.cells = []
   
		# initialize grid to all specified occupancy value
		for row in range(0, self.height):
			self.cells.append([])
			for col in range(0, self.width):
				self.cells[row].append(occupancy_value)

def set_cell(mgrid, point, value):
	mgrid.cells[point.row][point.col] = value


def get_cell(mgrid, point):
	return mgrid.cells[point.row][point.col]
