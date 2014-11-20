import copy
from matplotlib.colors import LogNorm
from pylab import *

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

def get_line(x1, y1, x2, y2):
	points = []
	issteep = abs(y2-y1) > abs(x2-x1)
	if issteep:
		x1, y1 = y1, x1
		x2, y2 = y2, x2
	rev = False
	if x1 > x2:
		x1, x2 = x2, x1
		y1, y2 = y2, y1
		rev = True
	deltax = x2 - x1
	deltay = abs(y2-y1)
	error = int(deltax / 2)
	y = y1
	ystep = None
	if y1 < y2:
		ystep = 1
	else:
		ystep = -1
	for x in range(x1, x2 + 1):
		if issteep:
			points.append((y, x))
		else:
			points.append((x, y))
		error -= deltay
		if error < 0:
			y += ystep
			error += deltax
	# Reverse the list if the coordinates were reversed
	if rev:
		points.reverse()
	return points

#This code will read and plot the heatmaps
f ="Sanity"
f1 = "Distance_8_60000nodesRRT_787_paths.xml"
f2 = "465_paths_seed_2363_1000_Crazy.xml"

d = open(f1)
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
	if "<\Path>" in l:
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

for path in paths:
	
	for i,v in enumerate(path):
		print v
		if i+1 == len(path):
			break 
		output = line(int(v[0]),int(v[1]),int(path[i+1][0]),int(path[i+1][1]))
		print"point"
		for point in output:
			print point
			xs.append(point[0])
			ys.append(point[1])

#print xs
#normal distribution center at x=0 and y=5

h = hist2d(xs, ys, bins=20, norm=LogNorm())
print h
show()








