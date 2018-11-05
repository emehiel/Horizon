echo off

rem create a for-loop to run through the different size swarms
for /l %%a in (1,1,4) do (
	rem target is at same location as designated pheromone pump
	rem create for-loop to change the visited pheromone update value
	for /l %%b in (2,8,18) do (
		rem specifies the visit pheromone deposit
		for /l %%c in (2,8,18) do (
			rem specifies the visit pheromone threshold before disappearing
			for /l %%d in (2,8,18) do (
				rem create XML Monte Carlo file
				cd C:\Users\Adam\OneDrive\Cal Poly Grad\Research\Horizon\UAVPheromones
				python pheromoneXmlAssetCreator.py %%a %%b %%c 1e-%%d
				
				rem change directory and run .exe
				cd C:\Users\Adam\OneDrive\Cal Poly Grad\Research\Horizon\Horizon\bin\Debug
				Horizon.exe
				
				rem rename and move HSF output data for every asset
				ren "C:\Users\Adam\OneDrive\Cal Poly Grad\Research\Horizon\*dynamicStateData.csv" "???????????????????????_up%%b_dep%%c_thr%%d.csv"
				move /y "C:\Users\Adam\OneDrive\Cal Poly Grad\Research\Horizon\*_dynamicStateData*.csv" "E:\Horizon Swarm Data\swarm_%%a\target\"
				
				rem move subsystem data
				ren "C:\HorizonLog\Scratch\*_fuel.csv" "???????????_up%%b_dep%%c_thr%%d.csv"
				ren "C:\HorizonLog\Scratch\*_targetfound.csv" "??????????????????_up%%b_dep%%c_thr%%d.csv"
				ren "C:\HorizonLog\Scratch\*_targetdistance.csv" "?????????????????????_up%%b_dep%%c_thr%%d.csv"
				move "C:\HorizonLog\Scratch\*.csv" "E:\Horizon Swarm Data\swarm_%%a\target\"
			)
		)
	)
)