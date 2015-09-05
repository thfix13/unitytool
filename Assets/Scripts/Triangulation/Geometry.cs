using System;
using System.Collections.Generic;
using UnityEngine;
//using Common;
//using Exploration;
//using Objects;

public class Geometry
{
	public List<Line> edges = new List<Line> ();

	//Added for MST Code
	public List<Geometry> voisins = new List<Geometry>();  
	public List<Line> voisinsLine = new List<Line>();
	public bool visited = false; 	

	public void DrawGeometry(GameObject parent)
	{
		Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
                           UnityEngine.Random.Range(0.0f,1.0f),
                           UnityEngine.Random.Range(0.0f,1.0f)) ;

		foreach (Line l in edges) {
			l.DrawVector (parent, c);
			//l.DrawLine(c);
		}
		//DrawVertex (parent);
	}



	public bool Collision(Geometry g)
	{
		foreach(Line l1 in edges)
		{
			foreach(Line l2 in g.edges)
			{
				if(l1 == l2)
					continue;
				if(l1.LineIntersection(l2))
					return true; 
			}
		}
		return false; 
	}

	public bool LineCollision(Line Lparam){
		if( this.PointInside( Lparam.MidPoint() ) )
		   return true;
		foreach(Line l1 in edges){
			if( Lparam.LineIntersectMuntac(l1) == 1 )
				return true;
		}
		return false; 
	}

	//TODO: Switch to lineintmuntac
	public bool LineInside(Line l)
	{
		//Test if one of my line
		//This is not the best version should check is colinear inastead!
		foreach(Line myLine in edges)
		{
			foreach(Vector3 v1 in myLine.vertex)
			{
				foreach(Vector3 v2 in l.vertex)
				{
					if(v1 == v2)
						return false; 
				}
			}
		}

		//Now we check count the intersection
		Vector3 mid = l.MidPoint(); 
		return PointInside (l.MidPoint ());
	}

	//TODO: Fix for lines colinear
	public bool PointInside( Vector3 pt )
	{
		int extreme = 500;
		List<Line> lRayList = new List<Line> ();
		List<Vector3> pointListToTest = new List<Vector3>();
		List<int> listAngleVars = new List<int>();
		/*for(int itr=10;itr<360;itr+=30)
		{
			listAngleVars.Add(itr);
		}*/
		listAngleVars.Add(45);
		listAngleVars.Add(-45);
		listAngleVars.Add(90);
		listAngleVars.Add(-90);
		listAngleVars.Add(135);
		listAngleVars.Add(-135);
		foreach(int angleVar in listAngleVars)
		{
			Vector3 vecSel = new Vector3();
			vecSel.x = pt.x + extreme*Mathf.Cos(angleVar* Mathf.Deg2Rad);
			vecSel.y = pt.y;
			vecSel.z = pt.z + extreme*Mathf.Sin(angleVar* Mathf.Deg2Rad);
			pointListToTest.Add(vecSel);
		}
		//Line lray = new Line(pt, new Vector3(-100,1,-100)); 
		int count = 0;
		foreach(Vector3 vectVar in pointListToTest)
		{
			lRayList.Add (new Line (pt, vectVar));
		}

		//lRayList.Add (new Line (pt, new Vector3 (-extreme, 1,-extreme)));
		//lRayList.Add (new Line (pt, new Vector3 (extreme, 1, -extreme)));
		//lRayList.Add (new Line (pt, new Vector3 (extreme, 1, extreme)));
		//lRayList.Add (new Line (pt, new Vector3 (-extreme, 1, extreme)));
		int count1 = 0;
		foreach(Line lray in lRayList)
		{
			count=0;
			foreach(Line myLine in edges)
			{
				if( myLine.LineIntersectMuntacEndPt(lray) > 0 )
				{
					count++;
					//Check if the intersection point is on the polygon edge
					//Note: other checks tried but precision error kept coming up in cases
					Vector3 vtemp = myLine.GetIntersectionPoint(lray);
					if( Math.Abs( vtemp.x - pt.x ) < 0.01 && Math.Abs(vtemp.z - pt.z) < 0.01 )
						return false;
				}
			}
			if(count%2 != 1)
			{
				count1++;
				//return false;
			}
		}
		if (count1 >= lRayList.Count/2)
			return false;
		//return count%2 == 1; 
		return true;
	}

	public List<Vector3> GetVertex()
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

	public void DrawVertex(GameObject parent)
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

		//Draw
		foreach(Vector3 v in vertex)
		{
			GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			inter.transform.position = v;
			inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f); 
			inter.transform.parent = parent.transform;
		}

	}

	public void CollisionDraw(Geometry g, GameObject parent)
	{
		foreach(Line l1 in edges)
		{
			foreach(Line l2 in g.edges)
			{
				if(l1 == l2)
					continue;
				if(l1.LineIntersection(l2))
				{

					Vector3 pos = l1.LineIntersectionVect(l2);
					GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					inter.transform.position = pos;
					inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f); 
					inter.transform.parent = parent.transform; 
				} 
			}
		}
		 
	}
	//TODO: Redo with EPS
	//Figures out the boundary of the geometry
	public void BoundGeometryCrude(Vector3[] boundary){//Called from outside
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
	public void BoundGeometry(Geometry refMap){//Called from outside
		//Checks for edges that do not intersect with and those who have at least one vertex outside of refMap
		//We have to check those who do not intersect as a line could have endpoints inside but some other point outside the polygon
		for (int i = 0; i < edges.Count; i++) {
			if( !refMap.LineCollision( edges[i] ) && (refMap.PointOutside(edges[i].vertex[0]) || refMap.PointOutside(edges[i].vertex[1])) ){
				edges.RemoveAt(i);
				i--;
			}
		}
		for (int i = 0; i < edges.Count; i++) {
			int partnerA = 0;
			int partnerB = 0;
			for (int j = 0; j < edges.Count; j++) {
				if( i == j ) continue;
				if( edges[i].ShareVertex( edges[j] ) ){
					Vector3 shv = edges[i].getSharedVertex( edges[j] );
					if( VectorApprox( shv, edges[i].vertex[0] ) )
						partnerA++;
					else
						partnerB++;
				}
			}
			//TODO: fix these bandaid fixes. this is mainly for inkscape.
			//Shared edges that fall inside merged geometry but not
			//inside either individual geometry
			if( partnerA == 0 || partnerB == 0 ){//|| partnerB == 0 ){
				//				edges.RemoveAt(i);
				//				i--;
			}
			else if( partnerA == 2 && partnerB == 2 ){
				//				edges.RemoveAt(i);
				//				i--; 
			}			
		}
	}
	//Figures out the boundary of the geometry
	public void BoundGeometry_Old(Vector3[] boundary){
		List<Line> removeLines = new List<Line> ();
		int i;
		//Debug.Log (boundary [0]);
		//Debug.Log (boundary [2]);
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
			//			if( xid == 9 && ccnt > 10000 ){
			//				Debug.Log("Over 10000");
			//				break;
			//			}
			for( int j = i + 1; j < G3.edges.Count; j++ ) {
				
				bool dbg = false;
				Line LA = G3.edges[i];
				Line LB = G3.edges[j];
				if( LA.Equals(LB) ){
					G3.edges.RemoveAt(j);
					j--;
					continue;
				}
				int caseType = LA.LineIntersectMuntacGM(LB);//Endpt but not shared endpt intersection.Sorted lines.
				//3A. Regular intersections
				if( caseType == 1 ){
					Vector3 pt = LA.GetIntersectionPoint( LB );
					List<Line> linesToAdd = new List<Line>();
					if( !VectorApprox(pt, LA.vertex[0] ) )
						linesToAdd.Add( new Line( pt, LA.vertex[0] ) );
					if( !VectorApprox(pt, LA.vertex[1] ) )
						linesToAdd.Add( new Line( pt, LA.vertex[1] ) );
					if( !VectorApprox(pt, LB.vertex[0] ) )
						linesToAdd.Add( new Line( pt, LB.vertex[0] ) );
					if( !VectorApprox(pt, LB.vertex[1] ) )
						linesToAdd.Add( new Line( pt, LB.vertex[1] ));
					//					if( linesToAdd.Contains( LA ) && linesToAdd.Contains( LB ) )
					//						continue;
					bool LARepeat = false;
					bool LBRepeat = false;
					foreach( Line lad in linesToAdd ){
						if( lad.Equals( LA ) )
							LARepeat = true;
						if( lad.Equals( LB ) )
							LBRepeat = true;
					}
					if( LARepeat && LBRepeat ){
						Debug.Log("Anomalous Repetition");
						continue;
					}
					foreach( Line lad in linesToAdd )
						G3.edges.Add( lad );
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
					}
					bool LARepeat = false;
					bool LBRepeat = false;
					foreach( Line lad in linesToAdd ){
						if( lad.Equals( LA ) )
							LARepeat = true;
						if( lad.Equals( LB ) )
							LBRepeat = true;
					}
					if( LARepeat && LBRepeat )
						continue;
					foreach( Line l in linesToAdd ){
						if( !VectorApprox(l.vertex[0], l.vertex[1] ) )
							G3.edges.Add(l);
					}
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
		//		if (xid == 9)
		//			return new Geometry ();
		//Check: Points inside Polygon
		//Check all midpoint of each line in G3 to see if it lies in G1 or G2. If inside remove.
		Geometry toReturn = new Geometry();
		int namecnt = 0;
		int doit = 0;
		foreach(Line l in G3.edges){
			l.name = namecnt++.ToString();
			if( xid == 3 ){
				//l.DrawVector(GameObject.Find("temp"));
				if( l.name.Equals("59") ){
					//l.DrawVector(GameObject.Find("temp"));
					//Debug.Log( G2.LineInsideDebug(l) );
					foreach( Line ld in G2.edges ){
						//						if( VectorApprox(ld.vertex[0],ld.vertex[1] ) )
						//						   Debug.Log("Equality");
					}
					//G2.DrawGeometry(GameObject.Find("temp"));
				}
				
			}
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
				//l.name = namecnt++.ToString();
				if( VectorApprox( l.vertex[0], l.vertex[1] ) )
					Debug.Log("Equality in GM final");
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
			int partnerA = 0;
			int partnerB = 0;
			for (int j = 0; j < toReturn.edges.Count; j++) {
				if( i == j ) continue;
				if( toReturn.edges[i].ShareVertex( toReturn.edges[j] ) ){
					Vector3 shv = toReturn.edges[i].getSharedVertex( toReturn.edges[j] );
					if( VectorApprox( shv, toReturn.edges[i].vertex[0] ) )
						partnerA++;
					else
						partnerB++;
				}
			}
			//TODO: fix these bandaid fixes. this is mainly for inkscape.
			//Shared edges that fall inside merged geometry but not
			//inside either individual geometry
			if( partnerA == 0 || partnerB == 0 ){//|| partnerB == 0 ){
				toReturn.edges.RemoveAt(i);
				i--;
			}
			else if( partnerA == 2 && partnerB == 2 ){
				toReturn.edges.RemoveAt(i);
				i--; 
			}
			
		}
		return toReturn;
	}
	public bool floatCompare ( float a, float b ){
		return Math.Abs (a - b) < eps;
	}
	
	public bool doubleCompare ( double a, double b ){
		return System.Math.Abs (a - b) < (double)eps;
	}
	public float eps = 1e-5f;//the margin of accuracy for all floating point equivalence checks
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
	public Geometry GeometryMerge( Geometry G2 ){
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
					if( LA.Equals(LB) ){
						G3.edges.RemoveAt(j);
						j--;
						continue;
					}
					int caseType = LA.LineIntersectMuntac( LB );
					if( caseType == 1 ){//Regular intersections
						Vector3 pt = LA.GetIntersectionPoint( LB );
						if( !VectorApprox( pt, LA.vertex[0] ) )
							G3.edges.Add( new Line( pt, LA.vertex[0] ) );
						if( !VectorApprox( pt, LA.vertex[1] ) )
							G3.edges.Add( new Line( pt, LA.vertex[1] ) );
						if( !VectorApprox( pt, LB.vertex[0] ) )
							G3.edges.Add( new Line( pt, LB.vertex[0] ) );
						if( !VectorApprox( pt, LB.vertex[1] ) )
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
	public bool GeometryInsideMap( Geometry G2 ){
		if (this.edges.Count == 1 || G2.edges.Count == 1) return false;
		List<Vector3> allvert = GetVertex ();
		List<Vector3> interpts = GetVertex ();
		foreach (Line L in edges) {
			foreach(Line L2 in G2.edges){
				if( L.LineIntersectMuntac( L2 ) > 0 )
					return false;
			}
		}
		
		List<Vector3> G2AllVert = G2.GetVertex ();
		foreach (Vector3 v in G2AllVert)
			if( this.PointOutside( v ) ) return false;
		foreach( Line l in G2.edges )
			if( this.PointOutside( l.MidPoint() ) ) return false;
		return true;
	}
	public Geometry GeometryMergeInner( Geometry G2, int xid ){//Called from outside
		//1. Check if geometries are fully inside each other
		
		//		if (GeometryInsideMap (G2))	return this;
		//		else if (G2.GeometryInsideMap (this)) return G2;
		
		Geometry tempGeometry = new Geometry ();
		//Two Geometry objects - G1 and G2
		Geometry G1 = this;
		
		//2. Create new called G3 which starts as an union of G1 and G2
		Geometry G3 = new Geometry ();
		foreach (Line l in G1.edges)
			G3.edges.Add(l);
		foreach (Line l in G2.edges)
			G3.edges.Add(l);
		
		int vnt = 0;
		//3. Check for intersection points among lines in G3
		bool once = false;
		int ccnt = 0;
		//		if( xid == 22 )
		//			G3.DrawGeometry(GameObject.Find("temp"));
		for (int i = 0; i < G3.edges.Count; i++) {
			ccnt++;
			for( int j = i + 1; j < G3.edges.Count; j++ ) {
				bool dbg = false;
				Line LA = G3.edges[i];
				Line LB = G3.edges[j];
				if( LA.Equals(LB) ){
					G3.edges.RemoveAt(j);
					j--;
					continue;
				}
				int caseType = LA.LineIntersectMuntacGM(LB);//Endpt but not shared endpt intersection.Sorted lines.
				//int caseType = LA.LineIntersectMuntac(LB);//Endpt but not shared endpt intersection.Sorted lines.
				//				if( xid == 22 ){
				//					if( LA.name.Equals("Border 13") )
				//						Debug.Log("Found");
				//				}
				//3A. Regular intersections
				if( caseType == 1 ){
					Vector3 pt = LA.GetIntersectionPoint( LB );
					List<Line> linesToAdd = new List<Line>();
					if( !VectorApprox(pt, LA.vertex[0] ) )
						linesToAdd.Add( new Line( pt, LA.vertex[0] ) );
					if( !VectorApprox(pt, LA.vertex[1] ) )
						linesToAdd.Add( new Line( pt, LA.vertex[1] ) );
					if( !VectorApprox(pt, LB.vertex[0] ) )
						linesToAdd.Add( new Line( pt, LB.vertex[0] ) );
					if( !VectorApprox(pt, LB.vertex[1] ) )
						linesToAdd.Add( new Line( pt, LB.vertex[1] ));
					bool LARepeat = false;
					bool LBRepeat = false;
					foreach( Line lad in linesToAdd ){
						if( lad.Equals( LA ) )
							LARepeat = true;
						if( lad.Equals( LB ) )
							LBRepeat = true;
					}
					if( LARepeat && LBRepeat ){
						Debug.Log("Anomalous Repetition");
						continue;
					}
					foreach( Line lad in linesToAdd )
						G3.edges.Add( lad );
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
					}
					bool LARepeat = false;
					bool LBRepeat = false;
					foreach( Line lad in linesToAdd ){
						if( lad.Equals( LA ) )
							LARepeat = true;
						if( lad.Equals( LB ) )
							LBRepeat = true;
					}
					if( LARepeat && LBRepeat )
						continue;
					foreach( Line l in linesToAdd ){
						if( !VectorApprox(l.vertex[0], l.vertex[1] ) )
							G3.edges.Add(l);
					}
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
			//			else{
			//				foreach( Line l2 in G2.edges ){
			//					if( 
			//				}
			//			}
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
	//Used only for merging polygons with the map boundary
	public Geometry GeometryMergeInner_Old( Geometry G2 ){
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
		if (this.edges.Count == 1 || G2.edges.Count == 1) return false;
		if( this.Equals( G2 ) )
			return true;
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
	
	private double CrossProduct( Vector2 a, Vector2 b ){
		return (a.x * b.y) - (a.y * b.x);
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
	public bool GeometryInside( Geometry G2, bool LineIntersectionsChecked ){
		if (this.edges.Count == 1 || G2.edges.Count == 1) return false;
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
		foreach( Line l in G2.edges )
			if( this.PointOutside( l.MidPoint() ) ) return false;
		return true;
	}

	//ported code. src:"Triangulation"
	public void SetVoisins(List<Geometry> geos)
	{
		foreach(Geometry g in geos)
		{
			if( g == this ) continue;
			Line voisinConnect = this.GetClosestLine(g,geos);
			if(voisinConnect != null)
			{
				voisins.Add (g);
				voisinsLine.Add(voisinConnect);
			}
		}
	}

	//ported code. src:"Triangulation"
	public Line GetClosestLine(Geometry g, List<Geometry> geos)
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
					
					if(gCollision.LineCollision(l))
						collisionFree = false; 
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
	public Line GetClosestLine(Vector3 v,Geometry g, List<Geometry> geos)
	{
		Line toReturn = null;
		float dist = 1000000; 
		
		
		foreach(Vector3 v2 in g.GetVertex())
		{
			Line l = new Line(v,v2);
			
			//Check collision
			bool collisionFree = true;
			
			foreach(Geometry gCollision in geos)
			{
				if(this == gCollision)
					continue; 
				if(gCollision.LineCollision(l))
					collisionFree = false; 
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
	public Geometry findClosestQuad(Vector3 v,List<Geometry> geos,List<Geometry> already)
	{
		Geometry toReturn = null; 
		float dist = 100000000; 
		
		foreach(Geometry g in geos)
		{
			if(g == this || already.Contains(g))
				continue;
			
			Line l = this.GetClosestLine(v,g,geos); 
			
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
}
