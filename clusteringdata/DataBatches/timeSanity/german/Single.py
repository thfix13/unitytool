from copy import deepcopy
import glob
import sys
from matplotlib.colors import LogNorm
from pylab import *
import numpy as np
import glob
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
	
	idClusters = []

	maxX = -10; maxY = -10
	xs = []
	ys = [] 
	idCluster = 0

	for i,l in enumerate(data):
		if "</Path>" in l:
			if len(pathTemp)>2:
				idClusters.append(idCluster)
				paths.append((pathTemp))
			pathTemp = []

		# <r>0</r>
  #       <g>0</g>
  #       <b>1</b>
  		if "<color>" in l:
			c1 = int(data[i+1].replace("<r>","").replace("</r>",""))
			c2 = int(data[i+2].replace("<g>","").replace("</g>",""))
			c3 = int(data[i+3].replace("<b>","").replace("</b>",""))
			# print c1,c2,c3
			if c1 == 1 and c2 == 0 and c3 == 0:
				# print "hello"
				idCluster = 0
			elif c1 == 0 and c2 == 1 and c3 == 0:
				idCluster = 1	
			elif c1 == 0 and c2 == 0 and c3 == 1:
				idCluster = 2
			else:
				idCluster = 3


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

	# mapData = [[0]*60 for _ in range(60)]
	mapData = [[[0]*60 for _ in range(60)],[[0]*60 for _ in range(60)],[[0]*60 for _ in range(60)],[[0]*60 for _ in range(60)]]
	

	xs = []
	ys = []

	
	posPath = 0
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

				mapData[idClusters[posPath]][int(xs[i])][int(ys[i])]+=1

			xs = []
			ys = []
		posPath+=1


	# print len(mapData[0])


	r = [[1]*60 for _ in range(60)]



	for m in mapData:
		for n in mapData:
			if m is n:
				continue
			for j in range(len(m)):
				for i in range(len(m)):
					if m[i][j] >0 and n[i][j]>0:
						r[i][j]+=1


	mapData = r
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

f = "SanityGeometry2_KM-4c-FRE-XY-1500p-376v-000136t@2015-02-12T17-46-35_8_of_31.xml"
f = "SanityGeometry2_KM-4c-FRE-XY-1500p-379v-000135t@2015-02-12T18-14-28_24_of_31.xml"
f = "SanityTime_KM-4c-H-XYT-1418p-1142v-000008t@2015-02-13T22-27-00_30_of_31.xml"
a = []
a = readDataToPaths(f)				 

aFinal = np.array(a[0])
aFinal = np.rot90(aFinal)

clf()
imshow(aFinal,cmap="Blues",interpolation="nearest",norm=LogNorm())
# imshow(a[0],cmap="Reds",interpolation="nearest",norm=LogNorm())
# title("Nb paths:"+ str(a[1]))
axis("off")
f = f.replace(".xml",".pdf")
savefig(f,bbox_inches='tight')
show()








