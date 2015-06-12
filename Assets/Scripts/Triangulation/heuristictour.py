import csv
import math
from shapely.ops import cascaded_union
from shapely.geometry import Polygon,Point

def nearest():
    "Calculates a tour based on nearest non-visible nodes"
    visited = [False for x in range(N)]

    current = startingcam
    tourdistance = float(0)
    tour = []
    tour.append(current)

    #Visibility polygon of starting camera
    polycurrent = polylist[(numtovect[current][0], numtovect[current][1])]
    currarea = polycurrent.area

    #Get area of map
    fullunion = cascaded_union(allpoly)
    maparea = fullunion.area
    #print maparea

    cvp_file = open('unionNearest.csv','wb')
    writer = csv.writer(cvp_file)
    writer.writerow(['Points','Area'])

    while True:
	visited[current] = True
	mindist = float(10000)
	maxarea = 0
	nearestneighbor = int(-1)
	#Union or visibility polygons may not be perfect (allow for small difference in final area)
	if abs(polycurrent.area - maparea) < 0.1:
	    break
	for i in range(N):
	    if i == current: continue
            if cameraonly:
            	if (numtovect[i] in cameras) != True: continue
	    if visited[i]: continue
	    if d[current][i] < mindist:
		nearestneighbor = i
		mindist = d[current][i]

	if nearestneighbor == -1:
	    break
	src = current
	dest = nearestneighbor
	stk = []
	prevv = numtovect[dest]
	#Gather all the polygons in the path
	pathpolylist = []
	while src != dest:
	    pathpolylist.append(polylist[(numtovect[dest][0],numtovect[dest][1])])
	    stk.append(dest)
	    dest = parents[current][dest]
	    visited[dest] = True
	
	#Add current set of polygons
	pathpolylist.append(polycurrent)

	while len(stk) != 0:
	    tour.append(stk.pop())

	tourdistance += mindist
	current = nearestneighbor
	polycurrent = cascaded_union(pathpolylist)
	
    prevpt = Point(numtovect[tour[0]])
    polyprev = polylist[numtovect[tour[0]]]
    totaldist = 0
    writer.writerow( [totaldist, polyprev.area] )
    for x in tour[1:]:
	currpt = Point(numtovect[x])
	polycurrent = cascaded_union([polyprev,polylist[numtovect[x]]])
	totaldist += currpt.distance(prevpt)
	writer.writerow([totaldist, polycurrent.area])
	prevpt = currpt
	polyprev = polycurrent
    return

def farthest():
    "Calculates a tour based on nearest non-visible nodes"
    visited = [False for x in range(N)]

    current = startingcam
    tourdistance = float(0)
    tour = []
    tour.append(current)

    #Visibility polygon of starting camera
    polycurrent = polylist[(numtovect[current][0], numtovect[current][1])]
    currarea = polycurrent.area

    #Get area of map
    fullunion = cascaded_union(allpoly)
    maparea = fullunion.area
    #print maparea

    cvp_file = open('unionFarthest.csv','wb')
    writer = csv.writer(cvp_file)
    writer.writerow(['Points','Area'])

    while True:
	visited[current] = True
	maxdist = float(0)
	nearestneighbor = int(-1)
	#Union or visibility polygons may not be perfect (allow for small difference in final area)
	if abs(polycurrent.area - maparea) < 0.1:
	    break
	for i in range(N):
	    if i == current: continue
            if cameraonly:
            	if (numtovect[i] in cameras) != True: continue
	    if visited[i]: continue
	    if d[current][i] > maxdist:
		nearestneighbor = i
		maxdist = d[current][i]

	if nearestneighbor == -1:
	    break
	src = current
	dest = nearestneighbor
	stk = []
	prevv = numtovect[dest]
	#Gather all the polygons in the path
	pathpolylist = []
	while src != dest:
	    pathpolylist.append(polylist[(numtovect[dest][0],numtovect[dest][1])])
	    stk.append(dest)
	    dest = parents[current][dest]
	    visited[dest] = True
	
	#Add current set of polygons
	pathpolylist.append(polycurrent)

	while len(stk) != 0:
	    tour.append(stk.pop())

	tourdistance += maxdist
	current = nearestneighbor
	polycurrent = cascaded_union(pathpolylist)
	
    prevpt = Point(numtovect[tour[0]])
    polyprev = polylist[numtovect[tour[0]]]
    totaldist = 0
    writer.writerow( [totaldist, polyprev.area] )
    for x in tour[1:]:
	currpt = Point(numtovect[x])
	polycurrent = cascaded_union([polyprev,polylist[numtovect[x]]])
	totaldist += currpt.distance(prevpt)
	writer.writerow([totaldist, polycurrent.area])
	prevpt = currpt
	polyprev = polycurrent
    return

def nearestnonvisible():
    "Calculates a tour based on nearest non-visible nodes"
    visited = [False for x in range(N)]

    current = startingcam
    tourdistance = float(0)
    tour = []
    tour.append(current)

    #Visibility polygon of starting camera
    polycurrent = polylist[(numtovect[current][0], numtovect[current][1])]
    currarea = polycurrent.area

    #Get area of map
    fullunion = cascaded_union(allpoly)
    maparea = fullunion.area
    #print maparea

    cvp_file = open('unionNearestNonVisible.csv','wb')
    writer = csv.writer(cvp_file)
    writer.writerow(['Points','Area'])
    nonvisible = True
    while True:
	visited[current] = True
	mindist = float(10000)
	maxarea = 0
	nearestneighbor = int(-1)
	#Union or visibility polygons may not be perfect (allow for small difference in final area)
	if abs(polycurrent.area - maparea) < 0.1:
	    break
	for i in range(N):
	    if i == current: continue
	    if nonvisible & isvisible[current][i] == 1: continue
            if cameraonly:
            	if (numtovect[i] in cameras) != True: continue
	    if visited[i]: continue
	    if d[current][i] < mindist:
		nearestneighbor = i
		mindist = d[current][i]

	if nearestneighbor == -1:
            if nonvisible:
		nonvisible = False
		continue
	    else: 
		break
	src = current
	dest = nearestneighbor
	stk = []
	prevv = numtovect[dest]
	#Gather all the polygons in the path
	pathpolylist = []
	while src != dest:
	    pathpolylist.append(polylist[(numtovect[dest][0],numtovect[dest][1])])
	    stk.append(dest)
	    dest = parents[current][dest]
	    visited[dest] = True
	
	#Add current set of polygons
	pathpolylist.append(polycurrent)

	while len(stk) != 0:
	    tour.append(stk.pop())

	tourdistance += mindist
	current = nearestneighbor
	polycurrent = cascaded_union(pathpolylist)
	
    prevpt = Point(numtovect[tour[0]])
    polyprev = polylist[numtovect[tour[0]]]
    totaldist = 0
    writer.writerow( [totaldist, polyprev.area] )
    for x in tour[1:]:
	currpt = Point(numtovect[x])
	polycurrent = cascaded_union([polyprev,polylist[numtovect[x]]])
	totaldist += currpt.distance(prevpt)
	writer.writerow([totaldist, polycurrent.area])
	prevpt = currpt
	polyprev = polycurrent
    return


def maxunion():
    "Calculates a maximum union tour"
    visited = [False for x in range(N)]

    current = startingcam
    tourdistance = float(0)
    tour = []
    tour.append(current)

    #Visibility polygon of starting camera
    polycurrent = polylist[(numtovect[current][0], numtovect[current][1])]
    currarea = polycurrent.area

    #Get area of map
    fullunion = cascaded_union(allpoly)
    maparea = fullunion.area
    #print maparea

    cvp_file = open('unionMaxUnion.csv','wb')
    writer = csv.writer(cvp_file)
    writer.writerow(['Points','Area'])

    while True:
	visited[current] = True
	mindist = float(10000)
	maxarea = 0
	nearestneighbor = int(-1)
	polymax = polycurrent
	#Union or visibility polygons may not be perfect (allow for small difference in final area)
	if abs(polycurrent.area - maparea) < 0.1:
	    break
	for i in range(N):
	    if i == current: continue
    #	if (numtovect[i] in cameras) != True: continue
    #       if visited[i]: continue
	    polytemp = polylist[(numtovect[i][0], numtovect[i][1])]
	    polytemp = cascaded_union([polycurrent,polytemp])
	    if polytemp.area > maxarea:
    #	if d[current][i] < mindist:
		nearestneighbor = i
		maxarea = polytemp.area
		polymax = polytemp
		mindist = d[current][i]

	if nearestneighbor == -1:
	    break
	src = current
	dest = nearestneighbor
	stk = []
	prevv = numtovect[dest]
	#Gather all the polygons in the path
	pathpolylist = []
	while src != dest:
	    pathpolylist.append(polylist[(numtovect[dest][0],numtovect[dest][1])])
	    stk.append(dest)
	    dest = parents[current][dest]
	    if numtovect[dest] in cameras:
		visited[dest] = True
	#Add current set of polygons
	pathpolylist.append(polycurrent)

	while len(stk) != 0:
	    tour.append(stk.pop())

	tourdistance += mindist
	current = nearestneighbor
	polycurrent = cascaded_union(pathpolylist)
	
    prevpt = Point(numtovect[tour[0]])
    polyprev = polylist[numtovect[tour[0]]]
    totaldist = 0
    writer.writerow( [totaldist, polyprev.area] )
    for x in tour[1:]:
	currpt = Point(numtovect[x])
	polycurrent = cascaded_union([polyprev,polylist[numtovect[x]]])
	totaldist += currpt.distance(prevpt)
	writer.writerow([totaldist, polycurrent.area])
	prevpt = currpt
	polyprev = polycurrent
    return

def maxuniontodistanceratio():
    "Calculates a maximum union tour"
    visited = [False for x in range(N)]

    current = startingcam
    tourdistance = float(0)
    tour = []
    tour.append(current)

    #Visibility polygon of starting camera
    polycurrent = polylist[(numtovect[current][0], numtovect[current][1])]
    currarea = polycurrent.area

    #Get area of map
    fullunion = cascaded_union(allpoly)
    maparea = fullunion.area
    #print maparea

    cvp_file = open('unionMaxUnionToDistanceRatio.csv','wb')
    writer = csv.writer(cvp_file)
    writer.writerow(['Points','Area'])

    while True:
	visited[current] = True
	mindist = float(10000)
	nearestneighbor = int(-1)
        metricratio = -1
	#Union or visibility polygons may not be perfect (allow for small difference in final area)
	if abs(polycurrent.area - maparea) < 0.1:
	    break
	for i in range(N):
	    if i == current: continue
    	    #if (numtovect[i] in cameras) != True: continue
            if visited[i]: continue
	    polytemp = polylist[(numtovect[i][0], numtovect[i][1])]
	    polytemp = cascaded_union([polycurrent,polytemp])
	    #Calculate percentage increase for area and distance
            areaincrease = (polytemp.area - polycurrent.area)/polycurrent.area
            if tourdistance != 0:
            	distincrease = d[current][i]/tourdistance
            else:
                distincrease = 1
	    if areaincrease/distincrease > metricratio:
		nearestneighbor = i
                metricratio = areaincrease/distincrease

	if nearestneighbor == -1:
	    break
	src = current
	dest = nearestneighbor
	stk = []
	prevv = numtovect[dest]
	#Gather all the polygons in the path
	pathpolylist = []
	while src != dest:
	    pathpolylist.append(polylist[(numtovect[dest][0],numtovect[dest][1])])
	    stk.append(dest)
	    dest = parents[current][dest]
 	    visited[dest] = True
	#Add current set of polygons
	pathpolylist.append(polycurrent)

	while len(stk) != 0:
	    tour.append(stk.pop())

	tourdistance += d[current][nearestneighbor]
	current = nearestneighbor
	polycurrent = cascaded_union(pathpolylist)
	
    prevpt = Point(numtovect[tour[0]])
    polyprev = polylist[numtovect[tour[0]]]
    totaldist = 0
    writer.writerow( [totaldist, polyprev.area] )
    for x in tour[1:]:
	currpt = Point(numtovect[x])
	polycurrent = cascaded_union([polyprev,polylist[numtovect[x]]])
	totaldist += currpt.distance(prevpt)
	writer.writerow([totaldist, polycurrent.area])
	prevpt = currpt
	polyprev = polycurrent
    return


#PROGRAM EXECUTION STARTS FROM HERE

instance = []
pointlist = []
polylist = {}
allpoly = []
x = 0

#1. Get all the visibility polygons
with open('vpsall.csv', 'rb') as csvfile:
    reader = csv.reader(csvfile, delimiter=',')
    reader = list(reader)
    first = True
    prevx = 0
    prevy = 0
    currx = 0
    curry = 0
    distance = 0
    for instance in reader:
	if instance[0] == "Start":
            pointlist = []
	    if( first ) : 
	        first = False
		currx = float(instance[1])
		curry = float(instance[2])
	    else :
	        currx = float(instance[1])
		curry = float(instance[2])
		distance = distance + Point(prevx,prevy).distance(Point(currx,curry))
        elif instance[0] == "L":
            pointlist.append([float(instance[1]),float(instance[2])])
	else:
            polygon = Polygon(pointlist)
            polylist[(currx,curry)] = polygon
	    allpoly.append(polygon)
	    prevx = currx
	    prevy = curry
print len(polylist)
#2. Get all the dijkstra values
vecttonum = {}
numtovect = {}
cameras = []
d = [[0 for x in range(len(polylist))] for x in range(len(polylist))]
parents = [[0 for x in range(len(polylist))] for x in range(len(polylist))]
isvisible = [[0 for x in range(len(polylist))] for x in range(len(polylist))]

index = 0

with open('dijkstra.csv', 'rb') as csvfile:
    reader = csv.reader(csvfile, delimiter=',')
    reader = list(reader)
    for instance in reader:
	if instance[0] == "Index":
	    index = int(instance[1])
	    iscam = int(instance[2])
	    x = float(instance[3])
	    y = float(instance[4])
	    if iscam == 1:
		 cameras.append((x,y))
	    numtovect[index] = (x,y)
	    vecttonum[(x,y)] = index	    
	elif instance[0] == "d":
	    for i in range(len(instance) - 1):
		if i == 0: continue
 	        d[index][i - 1] = float(instance[i])
	elif instance[0] == "parents":
	    for i in range(len(instance) - 1):
		if i == 0: continue
		parents[index][i - 1] = int(instance[i])
	else:
	    for i in range(len(instance) - 1):
		if i == 0: continue
		isvisible[index][i - 1] = int(instance[i])

#3. Compute the heuristic tour
N = len(polylist)
GSC = N - len(cameras)
cameraonly = False
startingcam = GSC #Randomize this assignment
nearest()
farthest()
nearestnonvisible()
maxunion()
maxuniontodistanceratio()
