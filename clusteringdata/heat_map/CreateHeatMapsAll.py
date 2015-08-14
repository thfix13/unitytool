from copy import deepcopy
import glob
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
	return (a,len(paths))



#This code will read and plot the heatmaps

files = glob.glob('*.xml')

for f in files:
	print f
	a = []
	a = readDataToPaths(f)				 

	aFinal = np.array(a[0])
	aFinal = np.rot90(aFinal)

	clf()
	imshow(aFinal,cmap="Blues",interpolation="nearest",norm=LogNorm())
	title("Nb paths:"+ str(a[1]))
	axis("off2")
	f = f.replace(".xml",".pdf")
	savefig(f,bbox_inches='tight')
	# show()







