import copy
import math
import pylab
import numpy
from mpl_toolkits.axes_grid1 import host_subplot
import mpl_toolkits.axisartist as AA
import matplotlib.pyplot as plt

o = open("clusterresults.xml")

c = 0

crazyAV = []
crazySD = []
crazyMedian = []

crazyTemp = []

LOSAV = []
LOSSD = []
LOSMedian = []

LOSTemp = []


DistAV = []
DistSD = []
DistMedian = []

DistTemp = []

hists = [[0 for j in range(6)] for i in range(6)]
colors = []
clustNumber = 0;

for i, l in enumerate(o.readlines()):
	
	if "cluster number=" in l:
		if(len(crazyTemp)>0):
			clustNumber +=1
			#Get av
			crazyAV.append(numpy.mean(crazyTemp))
			crazySD.append(numpy.std(crazyTemp))
			crazyMedian.append(numpy.median(crazyTemp))
			
			LOSAV.append(numpy.mean(LOSTemp))
			LOSSD.append(numpy.std(LOSTemp))
			LOSMedian.append(numpy.median(LOSTemp))
			
			DistAV.append(numpy.mean(DistTemp))
			DistSD.append(numpy.std(DistTemp))
			DistMedian.append(numpy.median(DistTemp))
		
		crazyTemp = []
		LOSTemp = []
		DistTemp = []
	
	if("Crazyness" in l):
		#clear
		l=l.replace("<total-results name=\"Crazyness\">","")
		l=l.replace("</total-results>","")
		l=l.replace("\n","")
		
		crazyTemp.append(float(l))
	
	if("Los3\">" in l):
		#clear
		l=l.replace("<total-results name=\"Los3\">","")
		l=l.replace("</total-results>","")
		l=l.replace("\n","")
		
		LOSTemp.append(float(l))
	
	if("Danger3\">" in l):
		#clear
		l=l.replace("<total-results name=\"Danger3\">","")
		l=l.replace("</total-results>","")
		l=l.replace("\n","")
		
		DistTemp.append(float(l))
		
		v = 0.0016
        
		if(float(l) < v):
			hists[clustNumber].append(float(l))
	
	if("HexColor" in l):
		#clear
		l=l.replace("<path-info name=\"HexColor\">","")
		l=l.replace("</path-info>","")
		l=l.replace(" ","")
		l=l.replace("\n","")
		
		color = "#" + str(l)
		
		if (color not in colors):
			colors.append("#" + str(l))

#Last time
crazyAV.append(numpy.mean(crazyTemp))
crazySD.append(numpy.std(crazyTemp))
crazyMedian.append(numpy.median(crazyTemp))

LOSAV.append(numpy.mean(LOSTemp))
LOSSD.append(numpy.std(LOSTemp))
LOSMedian.append(numpy.median(LOSTemp))

DistAV.append(numpy.mean(DistTemp))
DistSD.append(numpy.std(DistTemp))
DistMedian.append(numpy.median(DistTemp))


print('[%s]' % ', '.join(map(str, colors)));
print ("crazy")
print (crazyAV)
print (crazySD)
print (crazyMedian)

print ("LOS")
print (LOSAV)
print (LOSSD)
print (LOSMedian)

print ("Dist")
print (DistAV)
print (DistSD)
print (DistMedian)

import json

f = open('graphlog.txt', 'w');
f.write("NM Avg: ");
json.dump(DistAV, f);
f.write("\nNM SD: ");
json.dump(DistSD, f);
f.write("\nNM Median: ");
json.dump(DistMedian, f);
f.close();

import numpy as np
import pylab as P
import matplotlib
from matplotlib.ticker import FuncFormatter

#print hist1
a = 0.5
b = 100

for i in range(6):
    n, bins, patches = P.hist(hists[i], color = colors[i], bins = b,cumulative=False, normed=False,alpha=a)

#n, bins, patches = P.hist(hists[0], color = '#ff0000', bins = b,cumulative=False, normed=False,alpha=a)
#n, bins, patches = P.hist(hists[1], color = '#0000ff', bins = b,normed=False,alpha=a)
#n, bins, patches = P.hist(hists[2], color = '#00ff00',bins = b,normed=False,alpha=a)
#n, bins, patches = P.hist(hists[3], color = '#ff00ff',bins = b,normed=False,alpha=a)
#n, bins, patches = P.hist(hists[4], color = '#000000',bins = b,normed=False,alpha=a)
#n, bins, patches = P.hist(hists[5], color = '#ffffff',bins = b,normed=False,alpha=a)

P.xlabel('Metric values')
P.ylabel('Number of paths in that interval')

P.savefig("Hist.pdf")
P.show()