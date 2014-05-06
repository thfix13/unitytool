from __future__ import division
from matplotlib.patches import Patch
from pylab import *
import numpy 
import pylab 
from scipy.stats import norm
from numpy import linspace
from pylab import plot,show,hist,figure,title
import matplotlib.pyplot as plt
import numpy as np


     


o = open("triangulation2.xml")
fovAngle = -1
fovDist = -1

fovAngles = []
fovDists = []
Ratios = []


for i, l in enumerate(o.readlines()):
	if "fovAngle" in l:
		s = float(l.replace("<fovAngle>","").replace("</fovAngle>",""))
		if s != fovAngle:
			fovAngle = s
	if "fovDistance" in l:
		s = float(l.replace("<fovDistance>","").replace("</fovDistance>",""))
		if s != fovDist:
			fovDist = s
	if "ratio" in l:
		ratio = float(l.replace("<ratio>","").replace("</ratio>",""))
		#print str(fovAngle) + ", " + str(fovDist) + " = " + str(ratio)
		fovAngles.append(fovAngle)
		fovDists.append(fovDist)
		Ratios.append(ratio)
		#Save the data here
print(fovAngles)
print(fovDists)
print(Ratios)




y = []
for i in fovAngles:
	if i not in y:
		y.append(i)

x = []
for i in fovDists:
	if i not in x:
		x.append(i)


X,Y = meshgrid(x,y)
prev = 0
posy = 0
data = []
print "    "+str(x) 
for i,v in enumerate(Ratios):
	if i == 0:
		continue
	if i % len(x) == 0:
		print str(y[posy])+" "+str(Ratios[prev:i])
		data.append(Ratios[prev:i])
		prev = i
		posy +=1

print data
Z = np.array(data)
#exit()

fig = plt.figure()
ax = fig.add_subplot(111)

cax = ax.pcolor(X, Y, Z, cmap = gray())

#c = colorbar()
cbar = colorbar(cax)
cbar.ax.set_yticklabels(["Unsolvable","","","","","","","","","Solvable"])

#cbar = fig.colorbar(cax, ticks=[-1, 0, 1])
#cbar.ax.set_yticklabels(['< -1', '0', '> 1'])# vertically oriented colorbar


#c.set_clim(0, 10)

xlabel("FoV distance")
ylabel("FoV angle")
grid(False)


#savefig("SelectingFunction.svg", format = "svg")
show()


