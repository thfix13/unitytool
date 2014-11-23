from copy import deepcopy
from matplotlib.colors import LogNorm
from pylab import *
import numpy as np

def line( x0, y0, x1, y1):
    #"Bresenham's line algorithm"
    toReturn = []
    dx = abs(x1 - x0)
    dy = abs(y1 - y0)
    x, y = x0, y0
    sx = -1 if x0 > x1 else 1
    sy = -1 if y0 > y1 else 1
    if dx > dy:
        err = dx / 2.0
        while x != x1:
            toReturn.append([x, y])
            err -= dy
            if err < 0:
                y += sy
                err += dx
            x += sx
    else:
        err = dy / 2.0
        while y != y1:
            toReturn.append([x, y])
            err -= dx
            if err < 0:
                x += sx
                err += dy
            y += sy        
    toReturn.append([x, y])
    return toReturn

def readDataToPaths(name):
	d = open(name)
	data = d.readlines()
	d.close()

	result = []

	numPaths = 0
	paths = [] 
	pathTemp = []

	maxX = -10; maxY = -10
	xs = []
	ys = [] 


	for i,l in enumerate(data):
		if "</Path>" in l:
			if len(pathTemp)>2:
				
				paths.append((pathTemp))
			pathTemp = []
		if "<Node>" in l: 
			#Cleaning
			x = float(data[i+1].replace("<x>","").replace("</x>",""))
			y = float(data[i+2].replace("<y>","").replace("</y>",""))
			t = float(data[i+3].replace("<t>","").replace("</t>",""))

			xs.append(x)
			ys.append(y)

			if maxX < x:
				maxX = x
			if maxY < y:
				maxY = y

			pathTemp.append([x,y,t])

	#Create the data, make sure they are lines

	mapData = [[0]*60 for _ in range(60)]

	xs = []
	ys = []

	for path in paths:
		
		for i,v in enumerate(path):
			
			if i+1 == len(path):
				break 
			output = line(int(v[0]),int(v[1]),int(path[i+1][0]),int(path[i+1][1]))
			#print"point"
			for point in output:
				
				
				xs.append(point[0])
				ys.append(point[1])


	for i in range(len(xs)):
		mapData[int(xs[i])][int(ys[i])]+=1

	#create grayScale map
	#Find max value
	maxValue = 0
	for i in mapData:
		for j in i:
			if maxValue<j:
				maxValue=j


	#normalyse data and putthem in the right color format 
	a = [[1]*60 for _ in range(60)]
	# for i in a:
	# 	print a
	for j,v1 in enumerate(a):
		for i, v2 in enumerate(v1):
			v = float(mapData[j][i])/float(maxValue)
			a[j][i] = v
	return a



#This code will read and plot the heatmaps
f = "Sanity"
f1 = "Distance_8_60000nodesRRT_787_paths.xml"
f2 = "465_paths_seed_2363_1000_Crazy.xml"


a1 = np.array(readDataToPaths(f1))				 
a2 = np.array(readDataToPaths(f2))				 


# a1 = (readDataToPaths(f1))				 
# a2 = (readDataToPaths(f2))				 


#Calculate the difference


#print a1
a = deepcopy(a1)
#print a

for j in range(len(a1)):
	for i in range(len(a1[j])):
		if(a[j][i] is 0):
			continue
		if(a1[j][i] > a2[j][i]):
			a[j][i] = a1[j][i]
		else:
			a[j][i] = 0.5	
		

aFinal = np.array(a)
aFinal = np.array(a2) - np.array(a1)
aFinal = np.rot90(aFinal)

imshow(a1,cmap="Blues",norm=LogNorm(),interpolation="none",alpha=0.5)
imshow(a2,cmap="Reds",norm=LogNorm(),interpolation="none",alpha=0.5)
#savefig("Test1.pdf")
show()








