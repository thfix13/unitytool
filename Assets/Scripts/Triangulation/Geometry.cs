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
	public bool LineInside(Line l){//Called by GeometryMerge, GeomtryMergeInner and BoundGeometry
		//Test if one of my line
		//This is not the best version should check is colinear instead!
		foreach(Line myLine in edges){
			if( myLine.Equals( l ) )
				return false;
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
	//Note don't know if this should be treated as a bug as a lot of other logic now
	//depends on assuming PointInside will return false when a point is on the edge
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

	public bool PointInsideCam( Vector3 pt ){//Called by LineInside, GeometryInside and LineCollision
		Line lray = new Line(pt, new Vector3(-100, 1f, -100)); 
		int count = 0; 
		foreach(Line myLine in edges){
			if( myLine.LineIntersectMuntacEndPt(lray) > 0 ){
				count++;
				//Check if the intersection point is on the polygon edge
				//Note: other checks tried but precision error kept coming up in cases
				Vector3 vtemp = myLine.GetIntersectionPoint(lray);
				drawSphere( vtemp, Color.blue );
				if( VectorApprox(vtemp, pt) )
					return false;
			}
		}
		Debug.Log(count);
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

	//Note: Polygons must not be touching edge to edge. i.e. the two touching edges must not be colinear
	//Extension: In the merging for loop can fix this by adding a new case type and dealing accordingly
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
				//TODO:Copy geometry merge camera function here
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

	public Geometry GeometryMergeCamera( Geometry G2, int xid ){//Called from outside
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
		int vnt = 0;
		//Check for intersection points among lines in G3
		bool once = false;
		for (int i = 0; i < G3.edges.Count; i++) {
			//for( int j = i + 1; j < G3.edges.Count; j++ ) {
			for( int j = i + 1; j < G3.edges.Count; j++ ) {
				Line LA = G3.edges[i];
				Line LB = G3.edges[j];
				if( xid == 7 && i == 54 && !once ){
//					Debug.Log("wasdf");
//					LA.DrawVector(GameObject.Find("temp"));
					//LB.DrawVector(GameObject.Find("temp"));
					//if( LB.name == "Line4" )
						//Debug.Log ( "this is j:" + j );
				}
				int caseType = LA.LineIntersectMuntacDebug(LB);
				if( caseType == 1 ){//Regular intersections
					Vector3 pt = LA.GetIntersectionPoint( LB );
					G3.edges.Add( new Line( pt, LA.vertex[0] ) );
					G3.edges.Add( new Line( pt, LA.vertex[1] ) );
					G3.edges.Add( new Line( pt, LB.vertex[0] ) );
					G3.edges.Add( new Line( pt, LB.vertex[1] ));
					if( xid == 7 && i == 54 ){
//						new Line( pt, LA.vertex[0] ).DrawVector(GameObject.Find("temp"));
//						new Line( pt, LA.vertex[1] ).DrawVector(GameObject.Find("temp"));
//						new Line( pt, LB.vertex[0] ).DrawVector(GameObject.Find("temp"));
//						new Line( pt, LB.vertex[1] ).DrawVector(GameObject.Find("temp"));
					}
					G3.edges.RemoveAt(j);
					G3.edges.RemoveAt(i);
					i--;
					if( i == 53 )
						once = true;
					break;
				}
				//When colinear, also need to break everything.
				//Take one extreme point. Find distance with other points. Sort to get ordering.
				else if( caseType == 2 ){
					//This condition is to prevent otherwise disjoint lines that share a vertex
					//from infintely being added and removed from G3

					if( LA.ShareVertex( LB ) ){
						Vector3 sharedVert = LA.getSharedVertex(LB);
						Line LAShared = new Line( sharedVert, LA.GetOther(sharedVert) );
						Line LBShared = new Line( sharedVert, LB.GetOther(sharedVert) );
						Line LCShared = new Line( LA.GetOther(sharedVert), LB.GetOther(sharedVert) );
						if( floatCompare( LAShared.Magnitude() + LBShared.Magnitude(),  LCShared.Magnitude() ) ){
							if( xid == 7 && i == 54){
//								if( vnt == 42 ){
//									//drawSphere( sharedVert, Color.blue);
//									//drawSphere( LA.GetOther(sharedVert), Color.red);	
//									//drawSphere( LB.GetOther(sharedVert), Color.green);
//									Debug.Log ("MEtrics");
//									Debug.Log ( LAShared.Magnitude() + LBShared.Magnitude() + " " + LCShared.Magnitude() );
//								}
//								LA.name = vnt.ToString();
//								LA.DrawVector( GameObject.Find ("temp") );
//								LB.name = vnt.ToString();
//								LB.DrawVector( GameObject.Find ("temp") );
//								vnt++;
							}

							continue;
						}
					}
//					if( xid == 7 && i == 54 ){
//						LA.name = vnt.ToString();
//						LA.DrawVector( GameObject.Find ("temp") );
//						LB.name = vnt.ToString();
//						LB.DrawVector( GameObject.Find ("temp") );
//						vnt++;
//					}
					float maxlen = 0;
					Vector3 lowPoint = new Vector3 ( 1000, 1, 1000 );
					List<Vector3> vlst = new List<Vector3>();
					if( !vlst.Contains(LA.vertex[0] ) )
						vlst.Add( LA.vertex[0] );
					if( !vlst.Contains(LA.vertex[1] ) )
						vlst.Add( LA.vertex[1] );
					if( !vlst.Contains(LB.vertex[0] ) )
						vlst.Add( LB.vertex[0] );
					if( !vlst.Contains(LB.vertex[1] ) )
						vlst.Add( LB.vertex[1] );

					foreach( Vector3 v1 in vlst ){
						if( v1.x < lowPoint.x ){
							lowPoint = new Vector3( v1.x, 1, v1.z );
						}
						else if( floatCompare( v1.x, lowPoint.x ) ){
							if( v1.z < lowPoint.z )
								lowPoint = v1;
						}
					}
					List<KeyValuePair<Vector3, float>> distlist = new List<KeyValuePair<Vector3, float>>();
					foreach( Vector3 v1 in vlst ){
						if( v1 == lowPoint )
							distlist.Add( new KeyValuePair<Vector3, float>( v1, 0f ) );
						else
							distlist.Add( new KeyValuePair<Vector3, float>( v1, new Line( lowPoint, v1 ).Magnitude() ) );
					}
					distlist.Sort(CompareAngle);
					//Debug.Log("Distlist" + distlist.Count);
					for( int k = 0; k < distlist.Count - 1; k++ ){
//						if( xid == 1 && i == 3 )
//							drawSphere( distlist[k].Key , Color.blue );
						//Debug.Log ( distlist[k].Key + " " + distlist[k + 1].Key);
						//Debug.Log ( k + " " + distlist[k].Value + " " + xid + " " + i );
						//continue;
//						if( xid == 3 )
//							new Line( distlist[k].Key, distlist[k + 1].Key ).DrawVector(GameObject.Find("temp"));
						G3.edges.Add ( new Line( distlist[k].Key, distlist[k + 1].Key ) );
//						if( xid == 7 )
//							new Line( distlist[k].Key, distlist[k + 1].Key ).DrawVector(GameObject.Find("temp"));
					}

					G3.edges.RemoveAt(j);
					G3.edges.RemoveAt(i);
					i--;
					break;
//					if( xid == 7 && i == 53 )
//						once = true;
				}
				else{
//					if( xid == 7 && i == 54 && j == 58 ){
//						Debug.Log("In here");
//						LA.DrawVector(GameObject.Find("temp"));
//						LB.DrawVector(GameObject.Find("temp"));
//						Debug.Log( LA.vertex[0].x + " " + LA.vertex[0].z + "," + LA.vertex[1].x + " " + LA.vertex[1].z);
//						Debug.Log( LB.vertex[0].x + " " + LB.vertex[0].z + "," + LB.vertex[1].x + " " + LB.vertex[1].z);
//						Debug.Log( LA.LineIntersectMuntacDebug( LB ) );
//					}
				}
			}
		}
		//TODO:Remove TEMPORARY FIX and debug real issue//
		/****************REDUNDANT*******************/
		//Check: Points inside Polygon
		//Check all midpoint of each line in G3 to see if it lies in G1 or G2. If inside remove.
		Geometry toReturn = new Geometry();
		int namecnt = 0;
		if (xid == 7) {
			//Line a1 = G3.edges [54];
			//Line b1 = G3.edges [100];
			//a1.DrawVector(GameObject.Find("temp"));
			//b1.DrawVector(GameObject.Find("temp"));
//			Debug.Log(a1.LineIntersectMuntac(b1) + " " + b1.LineIntersectMuntac(a1) );
//			Debug.Log(a1.LineIntersectMuntacDebug(b1) + " " + b1.LineIntersectMuntac(a1) );
//			Debug.Log( a1.vertex[0].x + " " + a1.vertex[1].x );
//			Debug.Log( b1.vertex[0].x + " " + b1.vertex[1].x );
//			//Debug.Log(a1.LineIntersectRegular(b1) + " " + b1.LineIntersectRegular(a1) );
//			//drawSphere( a1.GetIntersectionPoint(b1), Color.grey );
//			//Debug.Log(a1.GetIntersectionPoint(b1) );
		}
		foreach(Line l in G3.edges){
			if( xid == 7 ){
				l.name = namecnt.ToString(); 
//				l.DrawVector( GameObject.Find("temp") );
			}
			if(!G1.LineInside(l) && !G2.LineInside(l) && !toReturn.edges.Contains(l)){
//				if( toReturn.edges.Contains(l) )
//					Debug.Log("Duplicate found");
//				if( xid == 3 && namecnt == 13 ){
//					Debug.Log (!G1.LineInside(l));
//					Debug.Log (!G2.LineInside(l));
//				}
//				if( xid == 3 )
//					l.DrawVector( GameObject.Find("temp"));
				toReturn.edges.Add(l);
			}
			if( xid == 7 && (namecnt == 54 || namecnt == 100) ){
//				Debug.Log (!G1.LineInside(l));
//				Debug.Log (!G2.LineInside(l));
////				l.DrawVector( GameObject.Find("temp") );
////				drawSphere( l.MidPoint(), Color.red );
//				Line lray = new Line(l.MidPoint(), new Vector3(-100, 1f, -100)); 
//				lray.DrawVector( GameObject.Find("temp"));
//				G1.DrawGeometry(GameObject.Find("vpB"));
////				G1.PointInsideCam( l.MidPoint() );
			}
			namecnt++;
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

	public bool GeometryInsideExt( Geometry G2 ){
		List<Vector3> allvert = GetVertex ();
		List<Vector3> interpts = GetVertex ();
		foreach (Line L in edges) {
			foreach(Line L2 in G2.edges){
				if( L.LineIntersectMuntac( L2 ) == 1 )
					return false;
				if( L.LineIntersectMuntacEndPt( L2 ) == 1 ){
					//more checks here to avoid instance
					//where intersection point is corner of
					//potentially enclosing polygon
				}
			}
		}
		foreach (Line L in G2.edges) {
			if( !( PointInside(L.vertex[0]) || allvert.Contains(L.vertex[0]) ) ){
				//TODO: more sophisticated check here
				bool found = false;
				foreach( Line LB in edges ){
					if( floatCompare( new Line( LB.vertex[0], L.vertex[0] ).Magnitude() + new Line(LB.vertex[1], L.vertex[0]).Magnitude(),
					                 LB.Magnitude() ) ){
						found = true;
						break;
					}
				}
				if( !found )
					return false;
			}
			if( !( PointInside(L.vertex[1]) || allvert.Contains(L.vertex[1]) ) ){
				bool found = false;
				foreach( Line LB in edges ){
					if( floatCompare( new Line( LB.vertex[0], L.vertex[1] ).Magnitude() + new Line(LB.vertex[1], L.vertex[1]).Magnitude(),
					                 LB.Magnitude() ) ){
						found = true;
						break;
					}
				}
				if( !found )
					return false;
			}
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
		inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
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
}
