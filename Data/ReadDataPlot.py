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

def func3(x,y):
    for i in  y:
    	print i
     


o = open("Triangulation.xml")
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
# print(fovAngles)
# print(fovDists)
# print(Ratios)

x = []
for i in fovAngles:
	if len(x) == 0 or x[len(x)-1] != i:
		x.append(i)

y = []
for i in fovDists:
	if i not in y:
		y.append(i)


X,Y = meshgrid(x,y)

func3(X,Y)




