import csv
import math
from shapely.ops import cascaded_union
from shapely.geometry import Polygon,Point


allaccdata = [] #contains bike accident likelihood with geolocation
instance = []
cvp_file = open('union.csv', 'wb')
writer = csv.writer(cvp_file)
writer.writerow(['Points', 'Area'])
pointlist = []
polylist = []
x = 0
with open('vps.csv', 'rb') as csvfile:
    reader = csv.reader(csvfile, delimiter=',')
    reader = list(reader)
    first = True
    prevx = 0
    prevy = 0
    distance = 0
#    distancelist = []
    for instance in reader:
	if instance[0] == "Start":
            pointlist = []
	    if( first ) : 
	        first = False
		prevx = float(instance[1])
		prevy = float(instance[2])
#		distancelist.append(float(0))
	    else :
	        currx = float(instance[1])
		curry = float(instance[2])
		distance = distance + Point(prevx,prevy).distance(Point(currx,curry))
	        prevx = currx
		prevy = curry
#		distancelist.append(distance)		
        elif instance[0] == "L":
            pointlist.append([float(instance[1]),float(instance[2])])
	else:
#            if x > 40 : break
            polygon = Polygon(pointlist)
#	    if x != 72:
            polylist.append(polygon)
            newpoly = cascaded_union(polylist)
            polylist = []
            polylist.append(newpoly)
#    	    print newpoly.area
            x+=1
	    #print x
            writer.writerow([distance, newpoly.area])
cvp_file.close()
