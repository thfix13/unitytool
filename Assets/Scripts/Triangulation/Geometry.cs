using System;
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

	//Checks collision with geometry unless the collision point is an vertex of the line or the geometry
	public bool LineCollision(Line Lparam){//Called by getClosestLine
		if( this.PointInside( Lparam.MidPoint() ) )
		   return true;
		foreach(Line l1 in edges){
			if( Lparam.LineIntersectMuntac(l1) == 1 )
				return true;
		}
		return false;
	}

	//Checks collision with geometry unless the collision point is a shared vertex of line and the geometry
	public bool LineCollisionEndPoint(Line Lparam){//Called from outside
		if( this.PointInside( Lparam.MidPoint() ) )
			return true;
		foreach(Line l1 in edges){
			if( Lparam.LineIntersectMuntacEndPt(l1) == 1 )
				return true;
		}
		return false; 
	}

	//TODO: Switch to lineintmuntac
	public bool LineInside(Line l){//Called by GeomtryMergeInner and BoundGeometry
		//Test if one of my line
		//This is not the best version should check is colinear instead!
		foreach(Line myLine in edges){
			foreach(Vector3 v1 in myLine.vertex){
				foreach(Vector3 v2 in l.vertex){
					if( VectorApprox(v1, v2) )
						return false; 
				}
			}
		}
		//Now we check count the intersection
		Vector3 mid = l.MidPoint(); 
		return PointInside (l.MidPoint ());
	}

	//TODO: Fix for lines colinear
	//Finds out if a point lies inside a polygon.
	//BUG: Returns false even if it's on the edge.
	//SOLN: Change LineIntersectMuntacEndPt to return -1 when there's an intersection at shared endpoints
	//instead of 0. And change condition here to only accept 0
	public bool PointInside( Vector3 pt ){//Called by LineInside, GeometryInside and LineCollision
		Line lray = new Line(pt, new Vector3(-100, 1f, -100)); 
		int count = 0; 
		foreach(Line myLine in edges){
			if( myLine.LineIntersectMuntacEndPt(lray) > 0 ){
				count++;
				//Check if the intersection point is on the polygon edge
				//Note: other checks tried but precision error kept coming up in cases
				Vector3 vtemp = myLine.GetIntersectionPoint(lray);
				if( VectorApprox(vtemp, pt) )
					return false;
			}
		}
		return count % 2 == 1;
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
		while ( true ){
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
			if( nextL == null )
				break;
		}
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

	public Geometry GeometryMerge( Geometry G2 ){//Called from outside
		if (GeometryInside (G2))
			return this;
		else if (G2.GeometryInside (this))
			return G2;

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
			if(!G1.LineInside(l) && !G2.LineInside(l))
				toReturn.edges.Add(l);
		}
		return toReturn;
	}

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
		if (GeometryInside (G2))
			return true;
		else if (G2.GeometryInside (this))
			return true;

		foreach( Line La in this.edges ){
			foreach( Line Lb in G2.edges ){
				if( La.LineIntersectMuntac( Lb ) > 0 )
					return true;
			}
		}
		return false;
	}
	
	public bool GeometryInside( Geometry G2 ){//Called from outside
		foreach (Line L in G2.edges) {
			if( !PointInside( L.vertex[0] ) )
				return false;
			if( !PointInside( L.vertex[1] ) )
				return false;
		}
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
		Line tmpline = new Line (vSrc, outpoint);
		tmpline.name = "Line Outpoint";
		tmpline.DrawVector (GameObject.Find ("temp"));
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
			angList.Add(new angclass(v, angle, AC.Magnitude()));
		}
		angList.Sort( delegate(angclass a, angclass b){
			int xdiff = a.angle.CompareTo(b.angle);
			if (xdiff != 0) return xdiff;
			else return a.distance.CompareTo(b.distance);
		});

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
		inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
		inter.transform.parent = temp.transform;
		//inter.gameObject.name = vlcnt.ToString();
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
}
