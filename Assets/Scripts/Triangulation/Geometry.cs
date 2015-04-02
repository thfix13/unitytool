using System;
using System.Collections;
using System.Collections.Generic;
//using System.Collections.Generic.Dictionary<TKey, TValue>;
using UnityEngine;
using Common;
using Exploration;
using Objects;

public class Geometry
{
	public List<Line> edges = new List<Line> ();
	public float eps = 1e-5f;//the margin of accuracy for all floating point equivalence checks

	//Added for MST Code
	public List<Geometry> voisins = new List<Geometry>();  
	public List<Line> voisinsLine = new List<Line>();
	public bool visited = false; 	

	public void DrawGeometry(GameObject parent)//Called from outside
	{
		Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
                           UnityEngine.Random.Range(0.0f,1.0f),
                           UnityEngine.Random.Range(0.0f,1.0f)) ;

		foreach (Line l in edges) {
			l.DrawVector (parent, c);
		}
	}

	//A non-exhaustive test to make coding easier elsewhere
	//Checks collision with geometry unless the collision point is a vertex of the line or the geometry
	public bool LineCollision(Line Lparam){//Called by getClosestLine
		if( this.PointInside( Lparam.MidPoint() ) )
		   return true;
		foreach(Line l1 in edges){
			if( Lparam.LineIntersectMuntac(l1) == 1 )
				return true;
		}
		return false;
	}

	//NOTE: ONLY used after lines have been processesd by GeometryMerge i.e. throughly segmented
	//This means we deal with lines where points can't be both explicity inside and outside of the
	//geometry.
	//We only need to check if the midpoint is strictly inside
	public bool LineInside(Line l){//Called by GeometryMerge, GeomtryMergeInner and BoundGeometry
		return PointInside (l.MidPoint ());
	}

    public bool LineInsideDebug(Line l){//Called by GeometryMerge, GeomtryMergeInner and BoundGeometry
		return PointInsideDebug (l.MidPoint ());
	}

	//Finds out if a point lies strictly inside a polygon.
	//The colinear case is automatically dealt with. See manual.
	public bool PointInside( Vector3 pt ){//Called by LineInside, GeometryInside and LineCollision
		Line lray = new Line(pt, new Vector3(-100, 1f, -100)); 
		int count = 0; 
		foreach(Line myLine in edges){
			//EndPt because ray might pass through corners
			if( myLine.LineIntersectMuntacEndPt(lray) > 0 )
				count++;
			//Check if the intersection point is on the polygon edge
			if( myLine.PointOnLine( pt ) && myLine.PointOnLineB( pt ) )
				return false;
		}
		return count % 2 == 1;
	}

	public bool PointInsideDebug( Vector3 pt ){//Called by LineInside, GeometryInside and LineCollision
		Line lray = new Line(pt, new Vector3(-100, 1f, -100)); 
		int count = 0; 
		foreach(Line myLine in edges){
			if( myLine.name.Equals("Line7") ){
				Debug.Log("PIDbg : " + myLine.LineIntersectMuntac(lray));
			}
			if( myLine.LineIntersectMuntac(lray) > 0 ){
				count++;
				//myLine.DrawVector(GameObject.Find("vpA"));
				//Debug.Log("InvisibleLine: " + myLine.vertex[0] + " " + myLine.vertex[1]);
				//Debug.Log ( myLine.vertex[0].x + " " + myLine.vertex[0].z);
				//Debug.Log ( myLine.vertex[1].x + " " + myLine.vertex[1].z);
				drawSphere(pt,Color.green);
				Debug.Log("Says:"+myLine.LineIntersectMuntac(lray));
			}
			//Check if the intersection point is on the polygon edge
			if( myLine.PointOnLine( pt ) && myLine.PointOnLineB( pt ) )
				return false;
		}
		Debug.Log ("Count: " + count);
		lray.DrawVector (GameObject.Find ("temp"));
		return count % 2 == 1;
	}

	//Checks if point is explicitly outside the polygon
	public bool PointOutside( Vector3 pt ){//Called by LineInside, GeometryInside and LineCollision
		Line lray = new Line(pt, new Vector3(-100, 1f, -100)); 
		int count = 0; 
		foreach(Line myLine in edges){
			//EndPt because ray might pass through corners
			if( myLine.LineIntersectMuntacEndPt(lray) > 0 )
				count++;
			//Check if the intersection point is on the polygon edge
			if( myLine.PointOnLine( pt ) && myLine.PointOnLineB( pt ) )
				return false;
		}
		return count % 2 == 0;
	}

	public bool PointOutsideDebug( Vector3 pt ){//Called by LineInside, GeometryInside and LineCollision
		Line lray = new Line(pt, new Vector3(-100, 1f, -100)); 
		lray.DrawVector (GameObject.Find ("temp"), Color.blue);
		int count = 0; 
		foreach(Line myLine in edges){
			//EndPt because ray might pass through corners
			if( myLine.LineIntersectMuntacEndPt(lray) > 0 ){
				count++;
//				if( myLine.name.Equals("10") ){
//					myLine.LineIntersectMuntacEndPtDebug(lray);
				//if( count == 3){
				//	myLine.DrawVector(GameObject.Find("temp"), Color.magenta);
					Debug.Log( myLine.LineIntersectMuntacEndPtDebug(lray));
					Debug.Log( myLine.LineIntersectMuntacGM(lray));
					Debug.Log( VectorApprox( myLine.GetIntersectionPoint(lray), pt ) );
					//Debug.Log( pt.z - myLine.GetIntersectionPoint(lray).z);
//					Debug.Log(myLine.PointOnLine( pt ) + " " + myLine.PointOnLineB( pt ));
//					Vector3 a = myLine.vertex [0];
//					Vector3 b = myLine.vertex [1];
//					Vector3 c = pt;
//					Debug.Log( ((b.x - a.x)*(pt.z - a.z) - (b.z - a.z)*(c.x - a.x)) );
//				}
//					Debug.Log("GM" + myLine.LineIntersectMuntacGM(lray) );
//				}
			}
			//Check if the intersection point is on the polygon edge
			if( myLine.PointOnLine( pt ) && myLine.PointOnLineB( pt ) )
				return false;
		}
		Debug.Log ("DBG: " + count);
		return count % 2 == 0;
	}

	public List<Vector3> GetVertex()//Called by GetReflexVertex, GetClosestLine
	{
		//Find vertex
		List<Vector3> vertex = new List<Vector3>(); 
		foreach(Line l in edges)
		{
			foreach(Vector3 v in l.vertex)
			{
				if(!vertex.Contains(v))
					vertex.Add(v);
			}
		}
		return vertex;
	}

	public List<Vector3> GetReflexVertex(){//Called from outside
		List<Vector3> vertex = new List<Vector3>();
		vertex = this.GetVertex ();
		Vector3 minvert = new Vector3 ();
		minvert.x = 100000;
		minvert.z = 100000;
		minvert.y = 1;
		foreach (Vector3 v in vertex) {
			if( v.z < minvert.z )
				minvert = v;
			else if( floatCompare(v.z, minvert.z) && v.x < minvert.x )
				minvert = v;
		}

		List<KeyValuePair<Vector3, float>> angList = new List<KeyValuePair<Vector3, float>>();
		foreach (Vector3 v in vertex) {
			float angle;
			if( !VectorApprox( v, minvert ) )
				angle = getAngle( minvert, v );
			else
				angle = -1000;
			angList.Add(new KeyValuePair<Vector3, float>(v, angle));
		}
		//Sort list by angle
		angList.Sort (CompareAngle);

		//Graham Scan
		List<Vector3> reflexList = new List<Vector3> ();
		reflexList.Add (minvert);
		reflexList.Add (angList[1].Key);

		int sz = 2;
		for( int i = 2; i < angList.Count; i++ ){
			bool vol = isLeft( reflexList[sz - 2], reflexList[sz - 1], angList[i].Key );
			if( vol ){
				reflexList.Add ( angList[i].Key );
				sz++;
			}
			else{
				reflexList.RemoveAt( reflexList.Count - 1 );
				if( sz == 2 )
					reflexList.Add ( angList[i].Key );
				else{
					--i;
					--sz;
				}
			}
		}
		return reflexList;
	}

	public List<Vector3> GetReflexVertexComplement(){//Called from outside
		List<Vector3> reflexVertex = new List<Vector3>();
		Line currL = this.edges [0];
		Line nextL;
		List<Line> visited = new List<Line> ();
		List<Line> sortedEdges = new List<Line> ();
		sortedEdges = getSortedEdges ();
		//Sort edges//
//		while ( true ){
//			nextL = null;//take each edge
//			foreach( Line l in this.edges ){//find an adjacent edge
//				if( l.Equals( currL ) ) continue;
//				if( l.ShareVertex( currL ) && !visited.Contains(l) ){
//					Vector3 ptA, ptB;
//					Vector3 commonPoint = l.getSharedVertex( currL );
//					nextL = l;
//					if( !visited.Contains(currL) ){//if this is the first edge we're working with
//						ptA = currL.vertex[0];
//						ptB = currL.vertex[1];
//						if( !VectorApprox(ptB, commonPoint) ){
//							ptA = ptB;
//							ptB = commonPoint;
//						}
//						sortedEdges.Add( new Line( ptA, ptB ) );
//						visited.Add (currL);
//					}
//					ptA = l.vertex[0];
//					ptB = l.vertex[1];
//					if( !VectorApprox(ptA, commonPoint) ){
//						ptB = ptA;
//						ptA = commonPoint;
//					}
//					sortedEdges.Add( new Line( ptA, ptB ) );
//					visited.Add( l );
//					currL = nextL;
//					break;
//				}
//			}
//			if( nextL == null )
//				break;
//		}
		float sum = 0;
		//Check if order in sorted edges is clockwise
		//Followed theorem outlined in following link:
		//http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
		for (int i = 0; i < sortedEdges.Count - 1; i++){
			sum += (sortedEdges [i].vertex [1].x - sortedEdges [i].vertex [0].x) * (sortedEdges [i].vertex [1].z + sortedEdges [i].vertex [0].z);
			sum += (sortedEdges [i + 1].vertex[0].x - sortedEdges[i].vertex [1].x) * (sortedEdges [i + 1].vertex [0].z + sortedEdges [i].vertex [1].z);
			if( i == sortedEdges.Count - 2 ) 
				sum += (sortedEdges [i + 1].vertex[1].x - sortedEdges[i + 1].vertex [0].x) * (sortedEdges [i + 1].vertex [1].z + sortedEdges [i + 1].vertex [1].z);
		}
		bool clockwise = true;
		if ( floatCompare( sum, 0, "<=") )
			clockwise = false;
		//reflexVertex.Add (sortedEdges [1].vertex [1]);
		int size = sortedEdges.Count;
		for (int i = 0; i < size; i++) {
			if( clockwise ){
				if( sortedEdges[i].PointIsLeft(sortedEdges[(i + 1)% size])  == true )
				   reflexVertex.Add( sortedEdges[i].vertex[1] );
			}
			else{
				if( sortedEdges[i].PointIsLeft(sortedEdges[(i + 1) % size]) == false )
				   reflexVertex.Add( sortedEdges[i].vertex[1] );
			}
		}
		return reflexVertex;
	}

	public List<Line> getSortedEdges(){
		Line currL = this.edges [0];
		Line nextL = this.edges [0];
		List<Line> visited = new List<Line> ();
		List<Line> sortedEdges = new List<Line> ();
		while ( nextL != null ){
			nextL = null;//take each edge
			foreach( Line l in this.edges ){//find an adjacent edge
				if( l.Equals( currL ) ) continue;
				if( l.ShareVertex( currL ) && !visited.Contains(l) ){
					Vector3 ptA, ptB;
					Vector3 commonPoint = l.getSharedVertex( currL );
					nextL = l;
					if( !visited.Contains(currL) ){//if this is the first edge we're working with
						ptA = currL.vertex[0];
						ptB = currL.vertex[1];
						if( !VectorApprox(ptB, commonPoint) ){
							ptA = ptB;
							ptB = commonPoint;
						}
						sortedEdges.Add( new Line( ptA, ptB ) );
						visited.Add (currL);
					}
					ptA = l.vertex[0];
					ptB = l.vertex[1];
					if( !VectorApprox(ptA, commonPoint) ){
						ptB = ptA;
						ptA = commonPoint;
					}
					sortedEdges.Add( new Line( ptA, ptB ) );
					visited.Add( l );
					currL = nextL;
					break;
				}
			}
//			if( nextL == null )
//				break;
		}
		return sortedEdges;
	}

	//Returns the area of the polygon
	//whether or not it has holes
	public double getPolygonArea( int xid ){
		List<Line> tempEdge = new List<Line> ();
		Geometry tempGeo = new Geometry ();
		foreach (Line l in this.edges)
			tempGeo.edges.Add(l);	
		//float[] areas = new float[500];
		List<double> areas = new List<double> ();
		double maxArea = 0;
		while (tempGeo.edges.Count > 0) {
			tempEdge = tempGeo.getSortedEdges();
//			if( polygonClockwise(tempEdge) )
//				tempEdge.Reverse();

//			Debug.Log(tempEdge.Count);
//			Debug.Log(tempGeo.edges.Count);
			foreach( Line lA in tempEdge ){
				for( int i = 0; i < tempGeo.edges.Count; i++ ){
					if( lA.Equals( tempGeo.edges[i] ) ){
						tempGeo.edges.RemoveAt(i);
						i--;
					}
				}
			}
			double tempArea = getLocalArea( tempEdge );
			areas.Add( tempArea );
			if( maxArea < tempArea )
				maxArea = tempArea;
//			Debug.Log(tempGeo.edges.Count);
//			return tempArea;
		}
		areas.Remove (maxArea);
		double areaOfHoles = 0;
		foreach( double x in areas )
			areaOfHoles += x;
		//if (xid > 12 )
		//		Debug.Log (maxArea + " " + areaOfHoles);
		if (maxArea < areaOfHoles)
			Debug.Log ("MAX AREA less than HOLES");
		return maxArea - areaOfHoles;
	}

	//Returns the area of a simple hole-less polygon
	private double getLocalArea( List<Line> polyedges ){
		Geometry g = new Geometry ();
		foreach (Line l in polyedges)
			g.edges.Add (l);
		List<Vector3> allVertex = new List<Vector3> ();
		allVertex = g.GetVertex ();
		//Points must be in anticlockwise order
		if (polygonClockwise (polyedges))
			allVertex.Reverse ();
		double areaXY = 0;
		double areaYX = 0;
		for (int i = 0; i < allVertex.Count; i++) {
			//drawSphereSorted ( allVertex[i], Color.red, 0f, i );
			areaXY += ( (double)allVertex[i].x * (double)allVertex[(i + 1)% allVertex.Count].z );
			areaYX += ( (double)allVertex[i].z * (double)allVertex[(i + 1)% allVertex.Count].x );
		}
		return (areaXY - areaYX) / (double)2;
	}

	bool polygonClockwise( List<Line> sortedEdges ){
		float sum = 0;
		//Check if order in sorted edges is clockwise
		//Followed theorem outlined in following link:
		//http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
		for (int i = 0; i < sortedEdges.Count - 1; i++){
			sum += (sortedEdges [i].vertex [1].x - sortedEdges [i].vertex [0].x) * (sortedEdges [i].vertex [1].z + sortedEdges [i].vertex [0].z);
			sum += (sortedEdges [i + 1].vertex[0].x - sortedEdges[i].vertex [1].x) * (sortedEdges [i + 1].vertex [0].z + sortedEdges [i].vertex [1].z);
			if( i == sortedEdges.Count - 2 ) 
				sum += (sortedEdges [i + 1].vertex[1].x - sortedEdges[i + 1].vertex [0].x) * (sortedEdges [i + 1].vertex [1].z + sortedEdges [i + 1].vertex [1].z);
		}
		bool clockwise = true;
		if ( floatCompare( sum, 0, "<=") )
			clockwise = false;
		return clockwise;
	}

	private bool isLeft(Vector3 v1,Vector3 v2,Vector3 v3){//Called by getReflexVertex
		float a = v1.x, b = v1.z;  
		float c = v2.x, d = v2.z;  
		float e = v3.x, f = v3.z;  
		
		if(  (v2.x - v1.x) * (v3.z - v1.z) >= (v2.z - v1.z) * (v3.x - v1.x) ) 
			return true;
		else
			return false;
	}

	static int CompareAngle(KeyValuePair<Vector3, float> a, KeyValuePair<Vector3, float> b){//Called by getReflexVertex
		//Questionable
		return a.Value.CompareTo(b.Value);
	}

	float getAngle( Vector3 v1, Vector3 v2 ){//Called by getReflexVertex
		float delx = v1.x - v2.x;
		float delz = v1.z - v2.z;
		return (float)Math.Atan2 (delz, delx);
	}

	public void DrawVertex(GameObject parent){//Called by no one but maybe useful
		//Find vertex
		List<Vector3> vertex = GetVertex ();

		//Draw
		foreach(Vector3 v in vertex){
			GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			inter.transform.position = v;
			inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f); 
			inter.transform.parent = parent.transform;
		}
	}

	//TODO: Redo with EPS
	//Figures out the boundary of the geometry
	public void BoundGeometry(Vector3[] boundary){//Called from outside
		List<Line> removeLines = new List<Line> ();
		int i;
		foreach (Line l in edges) {
			bool rem = false;
			for( i = 0; i < 2; i++ ){
				if( l.vertex[i].x > boundary[0].x + 0.01 || l.vertex[i].x < boundary[2].x  - 0.01 || l.vertex[i].z > boundary[0].z  + 0.01 || l.vertex[i].z < boundary[2].z  - 0.01)
					rem = true;
			}
			if( rem )
				removeLines.Add(l);
		}
		foreach (Line l in removeLines)
			edges.Remove (l);
	}


	//TODO: Check near end of function
	public Geometry GeometryMerge( Geometry G2, int xid ){//Called from outside
		//1. Check if geometries are fully inside each other
		if (GeometryInside (G2, false))	return this;
		else if (G2.GeometryInside (this, false)) return G2;

		Geometry tempGeometry = new Geometry ();
		//Two Geometry objects - G1 and G2
		Geometry G1 = this;

		//2. Create new called G3 which starts as an union of G1 and G2
		Geometry G3 = new Geometry ();
		foreach (Line l in G1.edges){
			G3.edges.Add(l);
		}
		foreach (Line l in G2.edges){
			G3.edges.Add(l);
		}

		int vnt = 0;
		//3. Check for intersection points among lines in G3
		bool once = false;
		int ccnt = 0;
		for (int i = 0; i < G3.edges.Count; i++) {
			ccnt++;
			for( int j = i + 1; j < G3.edges.Count; j++ ) {

				bool dbg = false;
				Line LA = G3.edges[i];
				Line LB = G3.edges[j];
				int caseType = LA.LineIntersectMuntacGM(LB);//Endpt but not shared endpt intersection.Sorted lines.
				//3A. Regular intersections
				if( caseType == 1 ){
					Vector3 pt = LA.GetIntersectionPoint( LB );
					//TODO:
//					if( LA.isEndPoint( pt ) || LB.isEndPoint(pt) )
//						Debug.Log("SAMENESS A");
					/// 
					G3.edges.Add( new Line( pt, LA.vertex[0] ) );
					G3.edges.Add( new Line( pt, LA.vertex[1] ) );
					G3.edges.Add( new Line( pt, LB.vertex[0] ) );
					G3.edges.Add( new Line( pt, LB.vertex[1] ));
					G3.edges.RemoveAt(j);
					G3.edges.RemoveAt(i);
					i--;
					break;
				}
				//3B. Colinear and overlapping
				else if( caseType == 2 ){
					//Will additionally catch colinear points that are also overlapping only at endpoints
					//Get all unique points, sort, form line between adjacent points i.e. segment everything
					//If lines are same as LA and LB ignore, otherwise add them then remove LA, LB
					List<Vector3> uniquePts = new List<Vector3>();
					foreach( Vector3 v in LA.vertex ) uniquePts.Add(v);
					foreach( Vector3 v in LB.vertex ) uniquePts.Add(v);
					for( int k = 0; k < uniquePts.Count; k++ ){
						for( int m = k + 1; m < uniquePts.Count; m++ ){
							if( k == m ) continue;
							if( VectorApprox( uniquePts[k], uniquePts[m] ) ){
								uniquePts.RemoveAt(m);
								m--;
							}
						}
					}
					uniquePts.Sort(delegate (Vector3 a, Vector3 b) {
						if( floatCompare(a.x,b.x) ) return a.z.CompareTo(b.z);
						else return a.x.CompareTo(b.x);
					});
					List<Line> linesToAdd = new List<Line>();
					for( int k = 0; k < uniquePts.Count - 1; k++ ){
						linesToAdd.Add( new Line(uniquePts[k], uniquePts[k+1]) );
						//TODO:
//						if( uniquePts[k].Equals(uniquePts[k+1]) )
//							Debug.Log("SAMENESS B");
					}
					if( linesToAdd.Count == 2 ){
						if( (LA.Equals(linesToAdd[0]) || LA.Equals(linesToAdd[1]))
						   && (LB.Equals(linesToAdd[0]) || LB.Equals(linesToAdd[1])) )
							continue;
					}
					foreach( Line l in linesToAdd )
						G3.edges.Add(l);
					G3.edges.RemoveAt(j);
					G3.edges.RemoveAt(i);
					i--;
					break;
				}
				else{
					//TODO:
					//This is done because of precision errors
					//When two lines intersect at a non-shared endpoint we'll
					//break up the line poked. Later we'll also check if all lines
					//have shared endpoints
					//if( LA.LineIntersectMuntacEndPt( LB ) == 1 )
				}
			}
		}

		//Check: Points inside Polygon
		//Check all midpoint of each line in G3 to see if it lies in G1 or G2. If inside remove.
		Geometry toReturn = new Geometry();
		int namecnt = 0;
		int doit = 0;
		foreach(Line l in G3.edges){
//				if( xid == 11 )
//				l.DrawVector(GameObject.Find("temp"));
//			if( xid == 11 ){
//				if( namecnt == 49 ){
//					Debug.Log(!G1.LineInside(l) +" " + !G2.LineInside(l) +" "+ !toReturn.edges.Contains(l));
//					l.DrawVector(GameObject.Find("temp"));
//					G2.DrawGeometry(GameObject.Find("vpA"));
//					Debug.Log("DBG:"+!G2.LineInsideDebug(l));
//				}
//			}
			if( l.vertex[0].Equals(l.vertex[1]) ) continue;
		    if(!G1.LineInside(l) && !G2.LineInside(l) && !toReturn.edges.Contains(l)){
					l.name = namecnt++.ToString();
					toReturn.edges.Add(l);
			}
		}
		//TODO:Fix for last case stated
		//Last bit of filtering for good measure//
		//All edge vertices should appear twice in the list
		//Edges missed by precision error and edges 
		//that are shared by the polygon and appear explicitly within neither
		//but is in the middle of the merged polygon
		for (int i = 0; i < toReturn.edges.Count; i++) {
			int partner = 0;
			for (int j = 0; j < toReturn.edges.Count; j++) {
				if( i == j ) continue;
				if( toReturn.edges[i].ShareVertex( toReturn.edges[j] ) )
					partner++;
			}
			if( partner == 0 ){
				toReturn.edges.RemoveAt(i);
				i--;
			}
		}
		return toReturn;
	}

	//TODO: Make more sophisticated. Check against GMInnerCam
	//Used only for merging polygons with the map boundary
	public Geometry GeometryMergeInner( Geometry G2 ){//Called from outside
		Geometry tempGeometry = new Geometry ();
		//Two Geometry objects - G1 and G2
		Geometry G1 = this;
		//Create new called G3 which starts as an union of G1 and G2
		Geometry G3 = new Geometry ();
		foreach (Line l in G1.edges)
			G3.edges.Add(l);		
		foreach (Line l in G2.edges)
			G3.edges.Add(l);		
		
		//Check for intersection points among lines in G3
		for (int i = 0; i < G3.edges.Count; i++) {
			for( int j = i + 1; j < G3.edges.Count; j++ ) {
				Line LA = G3.edges[i];
				Line LB = G3.edges[j];
				int caseType = LA.LineIntersectMuntac( LB );
				if( caseType == 1 ){//Regular intersections
					Vector3 pt = LA.GetIntersectionPoint( LB );
					G3.edges.Add( new Line( pt, LA.vertex[0] ) );
					G3.edges.Add( new Line( pt, LA.vertex[1] ) );
					G3.edges.Add( new Line( pt, LB.vertex[0] ) );
					G3.edges.Add( new Line( pt, LB.vertex[1] ));
					G3.edges.RemoveAt(j);
					G3.edges.RemoveAt(i);
					i--;
					break;
				}
			}
		}
		//Check: Points inside Polygon
		//Check all midpoint of each line in G3 to see if it lies in G1 or G2. If inside remove.
		Geometry toReturn = new Geometry();
		
		foreach(Line l in G3.edges){
			if(!G2.LineInside(l))
				toReturn.edges.Add(l);
		}
		//Check pt inside in G2
		foreach (Line l in toReturn.edges) {
			if( G2.LineInside( l ) ){
				toReturn.edges.Remove(l);
				break;
			}
		}
		return toReturn;
	}

	//Check if two geometries intersect
	public bool GeometryIntersect( Geometry G2 ){//Called from outside
		foreach( Line La in this.edges ){
			foreach( Line Lb in G2.edges ){
				if( La.LineIntersectMuntac( Lb ) > 0 )
					return true;
			}
		}
		if (GeometryInside (G2, true)) return true;
		else if (G2.GeometryInside (this, true)) return true;
		else return false;
	}
	
//	public bool GeometryInside( Geometry G2 ){//Called from outside
//		foreach (Line L in G2.edges) {
//			if( !PointInside( L.vertex[0] ) )
//				return false;
//			if( !PointInside( L.vertex[1] ) )
//				return false;
//		}
//		return true;
//	}

	public bool GeometryInside( Geometry G2, bool LineIntersectionsChecked ){
		List<Vector3> allvert = GetVertex ();
		List<Vector3> interpts = GetVertex ();
		if( !LineIntersectionsChecked ){
			foreach (Line L in edges) {
				foreach(Line L2 in G2.edges){
					if( L.LineIntersectMuntac( L2 ) == 1 )
						return false;
				}
			}
		}
		List<Vector3> G2AllVert = G2.GetVertex ();
		foreach (Vector3 v in G2AllVert)
			if( this.PointOutside( v ) ) return false;
		return true;
	}

	//From this point all code is ported from Jonatha's MST calculation
	//ported code. src:"Triangulation"

	//Connects geometry to all other geometries that are visible form it
	public void SetVoisins(List<Geometry> geos, Geometry mapBG)//Called from outside
	{
		foreach(Geometry g in geos)
		{
			if( g == this ) continue;
			Line voisinConnect = this.GetClosestLine( g, geos, mapBG );
			if(voisinConnect != null)
			{
				voisins.Add (g);
				voisinsLine.Add(voisinConnect);
			}
		}
	}

	//ported code. src:"Triangulation"
	//Finds the closest vertex-to-vertex line from this geometry to geometry g
	public Line GetClosestLine(Geometry g, List<Geometry> geos, Geometry mapBG )//Called by SetVoisins
	{
		Line toReturn = null;
		float dist = 1000000; 
		
		foreach(Vector3 v1 in this.GetVertex())
		{
			foreach(Vector3 v2 in g.GetVertex())
			{
				Line l = new Line(v1,v2);
				
				//Check collision
				bool collisionFree = true;
				
				foreach(Geometry gCollision in geos)
				{
					
					if(gCollision.LineCollision(l)){
						collisionFree = false; 
						break;
					}
				}

				foreach( Line borderLine in mapBG.edges ){
					if( borderLine.LineIntersectMuntac(l) != 0 ){
						collisionFree = false;
						break;
					}
				}

				if(!collisionFree)
				{
					continue; 
				}
				
				if(l.Magnitude()<dist)
				{
					toReturn = l; 
					dist = l.Magnitude(); 
				}
			}
		}
		return toReturn; 
	}

	//ported code. src:"Triangulation"
	//Finds the closest point between a vector v and a geometry g
	public Line GetClosestLine(Vector3 v,Geometry g, List<Geometry> geos, Geometry mapBG)//Called by findClosestQuad
	{
		Line toReturn = null;
		float dist = 1000000; 
		
		foreach(Vector3 v2 in g.GetVertex())
		{
			Line l = new Line(v, v2);
			
			//Check collision
			bool collisionFree = true;
			
			foreach(Geometry gCollision in geos)
			{
				if(this == gCollision)
					continue;
				if(gCollision.LineCollision(l)){
					collisionFree = false;
					break;
				}
			}

			foreach( Line borderLine in mapBG.edges ){
				if( borderLine.LineIntersectMuntacEndPt(l) != 0 ){
					collisionFree = false;
					break;
				}
			}

			if(!collisionFree)
			{
				continue; 
			}
			
			if(l.Magnitude()<dist)
			{
				toReturn = l; 
				dist = l.Magnitude(); 
			}
		}
		
		return toReturn; 
	}

	//ported code. src:"Triangulation"
	//Finds closest geometry to a vector v
	public Geometry findClosestQuad(Vector3 v,List<Geometry> geos, Geometry mapBG)//Called from outside
	{
		Geometry toReturn = null; 
		float dist = 100000000f; 

		foreach(Geometry g in geos)
		{
			if(g == this)
				continue;

			Line l = this.GetClosestLine( v, g, geos, mapBG );
			
			if(l == null)
				continue;

			if( l.Magnitude() < dist)
			{
				toReturn = g; 
				dist = l.Magnitude (); 
			}
		}

		return toReturn; 
		
	}

	public class angclass{
		public Vector3 vect;
		public float angle;
		public float distance;
		public angclass( Vector3 v, float a, float d ){
			vect = v;
			angle = a;
			distance = d;
		}
	}

	public List<KeyValuePair<Vector3, float>> GetVertexAngleSorted( Vector3 vSrc, List<Vector3> verts ){
		List< angclass > angList = new List< angclass >();
		Vector3 outpoint = new Vector3 ();
		outpoint.x = -100;
		outpoint.y = 1;
		outpoint.z = 100;
//		Line tmpline = new Line (vSrc, outpoint);
//		tmpline.name = "Line Outpoint";
//		tmpline.DrawVector (GameObject.Find ("temp"));

		foreach (Vector3 v in verts) {
			if( v == vSrc ){
				angList.Add(new angclass(v, 0, 0 ));
				continue;
			}
		    float angle;
			Line AB = new Line( vSrc, outpoint );
			Line AC = new Line( vSrc, v );
			Line BC = new Line ( v, outpoint );

			angle = (AB.Magnitude() * AB.Magnitude()) + (AC.Magnitude() * AC.Magnitude()) - (BC.Magnitude() * BC.Magnitude());
			angle /= (2.0f * AB.Magnitude() * AC.Magnitude() );
			angle = (float)Math.Acos((double)angle);
			if( !isLeft( vSrc, outpoint, v ) )
				angle = (float)Math.PI + (float)Math.PI - angle;
			if( floatCompare( AC.Magnitude() + BC.Magnitude(), AB.Magnitude() ) )
			    angle = 2 * (float)Math.PI;
			angList.Add(new angclass(v, (float)Math.Round( angle, 4 ), AC.Magnitude()));
			//angList.Add(new angclass(v, angle, AC.Magnitude()));
		}

		angList.Sort( delegate(angclass a, angclass b){
			int xdiff;
			if( floatCompare( a.angle, b.angle ) )
				xdiff = 0;
			else if( a.angle - b.angle > 0 )
				xdiff = 1;
			else
				xdiff = -1;
			if (xdiff != 0) return xdiff;
			else return a.distance.CompareTo(b.distance);
		});

//		int num = 0;
//		foreach (angclass kvp in angList) {
//			drawSphereSorted (kvp.vect, Color.red, kvp.angle, num++ );
//		}

		int cnt = 0;
		List<KeyValuePair<Vector3, float>> retlist = new List<KeyValuePair<Vector3, float>>();
		foreach (angclass ang in angList) {
			retlist.Add( new KeyValuePair<Vector3,float>( ang.vect, ang.angle ) );
		}
		return retlist;
	}

	void drawSphere( Vector3 v, Color x ){
		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.transform.renderer.material.color = x;
		inter.transform.position = v;
		//inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
		inter.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
		inter.transform.parent = temp.transform;
	}

	void drawSphere( Vector3 v, Color x, float vlcnt ){
		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.transform.renderer.material.color = x;
		inter.transform.position = v;
		inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
		inter.transform.parent = temp.transform;
		inter.gameObject.name = "Geo" + vlcnt.ToString();
	}

	void drawSphereSorted( Vector3 v, Color x, float vlcnt, int num ){
		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.transform.renderer.material.color = x;
		inter.transform.position = v;
		inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
		inter.transform.parent = temp.transform;
		inter.gameObject.name = "Sorted " + num.ToString() + " " + vlcnt.ToString();
	}

	public bool VectorApprox ( List<Vector3> obs_pts, Vector3 interPt ){
		foreach (Vector3 v in obs_pts) {
			if( Math.Abs (v.x - interPt.x) < eps && Math.Abs (v.z - interPt.z) < eps )
				return true;
		}
		return false;
	}
	public bool VectorApprox ( Vector3 a, Vector3 b ){
		if( Math.Abs (a.x - b.x) < eps && Math.Abs (a.z - b.z) < eps )
			return true;
		else
			return false;
	}
	
	public bool floatCompare ( float a, float b ){
		return Math.Abs (a - b) < eps;
	}
	
	public bool floatCompare ( float a, float b, string condition ){
		switch (condition) {
		case(">="):
			if (a > b || Math.Abs (a - b) < eps)
				return true;
			break;
		case("=="):
			if (Math.Abs (a - b) < eps)
				return true;
			break;
		case("<="):
			if (a < b || Math.Abs (a - b) < eps)
				return true;
			break;
		}
		return false;
	}

//	public class vectorSorter : IComparer{
//		int IComparer.Compare ( System.Object a, System.Object b ){
//			Vector3 v1 = Vector3 (a);
//			Vector3 v2 = Vector3 (b);
//			if( floatCompare( v1.x, v2.x) )
//				return true;
//			else
//		}
//	}
}
