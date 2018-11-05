#  create a pheromone object with all corresponding attributes

class Pheromone:
	def __init__(self, name='lawn', deposit=20, update=12, prop=0.75, evap=0.03, threshold=1e-300):
		self.name = name
		self.deposit = deposit
		self.update = update
		self.count = 0 # counter to increment until update value is reached
		self.Gf = prop
		self.Ef = evap
		self.Tf = threshold
		