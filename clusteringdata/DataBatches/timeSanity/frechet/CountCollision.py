from copy import deepcopy
import glob
import sys
from matplotlib.colors import LogNorm
from pylab import *
import numpy as np
import glob
#Need to update for 3D. 

import numpy as np


def _bresenhamline_nslope(slope):
    """
    Normalize slope for Bresenham's line algorithm.

    >>> s = np.array([[-2, -2, -2, 0]])
    >>> _bresenhamline_nslope(s)
    array([[-1., -1., -1.,  0.]])

    >>> s = np.array([[0, 0, 0, 0]])
    >>> _bresenhamline_nslope(s)
    array([[ 0.,  0.,  0.,  0.]])

    >>> s = np.array([[0, 0, 9, 0]])
    >>> _bresenhamline_nslope(s)
    array([[ 0.,  0.,  1.,  0.]])
    """
    scale = np.amax(np.abs(slope), axis=1).reshape(-1, 1)
    zeroslope = (scale == 0).all(1)
    scale[zeroslope] = np.ones(1)
    normalizedslope = np.array(slope, dtype=np.double) / scale
    normalizedslope[zeroslope] = np.zeros(slope[0].shape)
    return normalizedslope

def _bresenhamlines(start, end, max_iter):
    """
    Returns npts lines of length max_iter each. (npts x max_iter x dimension) 

    >>> s = np.array([[3, 1, 9, 0],[0, 0, 3, 0]])
    >>> _bresenhamlines(s, np.zeros(s.shape[1]), max_iter=-1)
    array([[[ 3,  1,  8,  0],
            [ 2,  1,  7,  0],
            [ 2,  1,  6,  0],
            [ 2,  1,  5,  0],
            [ 1,  0,  4,  0],
            [ 1,  0,  3,  0],
            [ 1,  0,  2,  0],
            [ 0,  0,  1,  0],
            [ 0,  0,  0,  0]],
    <BLANKLINE>
           [[ 0,  0,  2,  0],
            [ 0,  0,  1,  0],
            [ 0,  0,  0,  0],
            [ 0,  0, -1,  0],
            [ 0,  0, -2,  0],
            [ 0,  0, -3,  0],
            [ 0,  0, -4,  0],
            [ 0,  0, -5,  0],
            [ 0,  0, -6,  0]]])
    """
    if max_iter == -1:
        max_iter = np.amax(np.amax(np.abs(end - start), axis=1))
    npts, dim = start.shape
    nslope = _bresenhamline_nslope(end - start)

    # steps to iterate on
    stepseq = np.arange(1, max_iter + 1)
    stepmat = np.tile(stepseq, (dim, 1)).T

    # some hacks for broadcasting properly
    bline = start[:, np.newaxis, :] + nslope[:, np.newaxis, :] * stepmat

    # Approximate to nearest int
    return np.array(np.rint(bline), dtype=start.dtype)

def bresenhamline(start, end, max_iter=5):
    """
    Returns a list of points from (start, end] by ray tracing a line b/w the
    points.
    Parameters:
        start: An array of start points (number of points x dimension)
        end:   An end points (1 x dimension)
            or An array of end point corresponding to each start point
                (number of points x dimension)
        max_iter: Max points to traverse. if -1, maximum number of required
                  points are traversed

    Returns:
        linevox (n x dimension) A cumulative array of all points traversed by
        all the lines so far.

    >>> s = np.array([[3, 1, 9, 0],[0, 0, 3, 0]])
    >>> bresenhamline(s, np.zeros(s.shape[1]), max_iter=-1)
    array([[ 3,  1,  8,  0],
           [ 2,  1,  7,  0],
           [ 2,  1,  6,  0],
           [ 2,  1,  5,  0],
           [ 1,  0,  4,  0],
           [ 1,  0,  3,  0],
           [ 1,  0,  2,  0],
           [ 0,  0,  1,  0],
           [ 0,  0,  0,  0],
           [ 0,  0,  2,  0],
           [ 0,  0,  1,  0],
           [ 0,  0,  0,  0],
           [ 0,  0, -1,  0],
           [ 0,  0, -2,  0],
           [ 0,  0, -3,  0],
           [ 0,  0, -4,  0],
           [ 0,  0, -5,  0],
           [ 0,  0, -6,  0]])
    """
    # Return the points as a single array
    return _bresenhamlines(start, end, max_iter).reshape(-1, start.shape[-1])

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

	maxX = -10; maxY = -10; maxZ = -10
	xs = []
	ys = [] 
	zs = []

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
			zs.append(t)

			if maxX < x:
				maxX = x
			if maxY < y:
				maxY = y
			if maxZ < t: 
				maxZ = t

			pathTemp.append([x,y,t])

	#Create the data, make sure they are lines

	# print maxZ

	# mapData = [[0]*60 for _ in range(60)]
	mapData = [ [[[0]*int(maxZ) for _ in range(60)] for _ in range(60)],
	[[[0]*int(maxZ) for _ in range(60)] for _ in range(60)],
	[[[0]*int(maxZ) for _ in range(60)] for _ in range(60)],
	[[[0]*int(maxZ) for _ in range(60)] for _ in range(60)] ]
		
	# print len(mapData[0][0][0])
	
	xs = []
	ys = []
	zs = []
	
	posPath = 0
	for path in paths:
			
		for i,v in enumerate(path):
			
			if i+1 == len(path):
				break 
			# output = line(int(v[0]),int(v[1]),int(path[i+1][0]),int(path[i+1][1]))
			output = bresenhamline(np.array([[int(v[0]),int(v[1]),int(v[2])]]),
				np.array([[int(path[i+1][0]),int(path[i+1][1]),int(path[i+1][2])]]))
			# print output
			# output = output[0]
	

			#print"point"
			for point in output:
				xs.append(point[0])
				ys.append(point[1])
				zs.append(point[2])
				

			for i in range(len(xs)):

				mapData[idClusters[posPath]][int(xs[i])][int(ys[i])][int(zs[i])]+=1
			
			xs = []
			ys = []
		posPath+=1


	# print len(mapData[0])
	r = [[[1]*int(maxZ) for _ in range(60)] for _ in range(60)]

	collissions = 0; 


	for m in mapData:
		for n in mapData:
			if m is n:
				continue
			for j in range(len(m)):
				for i in range(20,len(m[0])-20,1):
					for k in range(0,int(maxZ),1):
						if m[i][j][k] >0 and n[i][j][k]>0:
							r[i][j][k]+=1
							collissions+=1

	return collissions
	


	
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



files = glob.glob('*.xml')







colls = []
for f in files:
	print f
	
	colls.append(readDataToPaths(f))				 
	# break
	

import numpy 
import pylab 
from scipy.stats import norm
from numpy import linspace
from pylab import plot,show,hist,figure,title
import matplotlib.pyplot as plt
import numpy as np

mu, std = norm.fit(colls)

a = 0.2
b = 20

# pylab.hist(colls,bins=b, normed=1,color = '#0000ff',alpha=a)

# pylab.xlim(xmax=100)

# pylab.xlabel("avg = " + str(mu) + ", std =" + str(std))
# plot(x, p, 'k', linewidth=1.4,color='#0000ff')

med = numpy.median(colls)

print colls

plt.figure(1)
plt.subplot(211)
plt.hist(colls,bins=b, normed=1,color = '#0000ff',alpha=a)
plt.title("avg = " + str(mu) + ", std =" + str(std) + ", med =" + str(med))

xmin, xmax = plt.xlim()
x = np.linspace(xmin, xmax, 100)
p = norm.pdf(x, mu, std)
plt.plot(x, p, 'k', linewidth=1.4,color='#0000ff')

plt.show()



