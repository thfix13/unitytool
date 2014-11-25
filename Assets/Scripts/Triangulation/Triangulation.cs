xusing UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Vectrosity;

[ExecuteInEditMode]
public class Triangulation : MonoBehaviour 
{
	public float eps = 1e-5f;//the margin of accuracy for all floating point equivalence checks
	//Data holder to display and save
	public List<Vector3> points = new List<Vector3>();
	public List<Color> colours = new List<Color>();
	public List<Vector3> cameras = new List<Vector3>();
	// Use this for initialization
	
	public List<Triangle> triangles = new List<Triangle>();
	public List<Line> lines = new List<Line>(); 
	
	public List<Line> linesMinSpanTree = new List<Line>(); 
	public List<Geometry> obsGeos = new List<Geometry> (); 
	public List<Geometry> finalPoly = new List<Geometry> ();
	public Geometry totalGeo = new Geometry ();
	//Contains Map
	public Geometry mapBG = new Geometry ();
	public Geometry tour = new Geometry ();
	
	public bool drawTriangles = false; 
	public bool drawRoadMap = false; 
	public bool drawMinSpanTree = false;
	public bool drawGeometry = false;
	public bool stopAll = false;
	private bool drawn = false;

	//Contains roadmap traversing triangles
	public List<Line> roadMap = new List<Line> ();

	public void Start(){
		
	}
	
	public void Clear()
	{
		linesMinSpanTree.Clear(); 
		triangles.Clear(); 
		lines.Clear(); 
		points.Clear(); 
		colours.Clear();
		obsGeos.Clear ();
		GameObject temp = GameObject.Find("temp");
		DestroyImmediate(temp);
		GameObject vptmp = GameObject.Find("vptmp");
		DestroyImmediate(vptmp);
		cameras.Clear ();
		drawn = false;
		stopAll = true;
	}
	void OnDrawGizmosSelected( ) 
	{
		//return; 
		//Debug.Log(colours.Count);
		//Debug.Log(points.Count);
		var i = 0;
		foreach(Vector3 v in points)
		{
			
			//Gizmos.color = colours[i];
			//Gizmos.color = Color.red;
			Gizmos.DrawSphere (v, 0.25f);
			//i++; 
		}
		
		//Gizmos.color = Color.red;
		//Gizmos.DrawSphere (new Vector3(0,2,0), 1);
	}
	public void Update()
	{
		if ( stopAll )
			return;
		
		if(drawMinSpanTree){
			GameObject temp = GameObject.Find("temp"); 
			foreach(Line l in linesMinSpanTree)
				Debug.DrawLine(l.vertex[0], l.vertex[1],Color.red);
		}
		
		if (drawGeometry) {
			//			foreach(Geometry g in finalPoly){
			//
			//			}
			if( !drawn ){
				totalGeo.DrawGeometry(GameObject.Find("temp"));
				drawn = true;
			}
		}

		if (drawRoadMap) {
			foreach(Line l in roadMap){
				Debug.DrawLine(l.vertex[0],l.vertex[1],Color.red);
			}
		}


		foreach(Triangle tt in triangles){
			if(drawTriangles){	
				tt.DrawDebug();
				//foreach(Vector3 v in tt.getVertexMiddle())
				//	points.Add(v);
				//foreach(Color v in tt.colourVertex)
				//	colours.Add(v);
			}
			
//			if(drawRoadMap)	{
//					Line[] ll = tt.GetSharedLines(); 
//				
//	
//					if(ll.Length == 1)
//					{
//						Debug.DrawLine(ll[0].MidPoint(), tt.GetCenterTriangle(),Color.red);
//						//Debug.Log("Drawing Red Line at: " + ll[0].MidPoint() + " " + tt.GetCenterTriangle());
//					}
//					else if(ll.Length > 2)
//					{
//						for(int i = 0; i<ll.Length; i++)
//						{
//							Debug.DrawLine(ll[i].MidPoint(), tt.GetCenterTriangle(),Color.red);
//							//Debug.Log("Drawing Red Line at: " + ll[i].MidPoint() + " " + tt.GetCenterTriangle());
//						}
//	
//					}
//					
//					else
//					{
//						for(int i = 0; i<ll.Length; i++)
//						{
//							Debug.DrawLine(ll[i].MidPoint(), ll[(i+1) % ll.Length].MidPoint(),Color.red);
//						}
//					}
//				//tour.DrawGeometry( GameObject.Find("temp") );
//			}
		}
		
	}
	
	public void AddPoint(Vector3 v){
		points.Add(v); 
		colours.Add(Color.cyan); 
	}
	
	public void AddPoint(Vector3 v,Color c){
		points.Add(v); 
		colours.Add(c); 
	}
	
	public void TriangulationSpace (){
		//Compute one step of the discritzation
		//Find this is the view
		GameObject floor = (GameObject)GameObject.Find ("Floor");
		Vector3 [] vertex = new Vector3[4]; 
		//First geometry is the outer one
		List<Geometry> geos = new List<Geometry> ();
		//Drawing lines
		//Floor
		Vector3[] f = new Vector3[4];
		MeshFilter mesh = (MeshFilter)(floor.GetComponent ("MeshFilter"));
		Vector3[] t = mesh.sharedMesh.vertices; 
		
		Geometry tempGeometry = new Geometry (); 
		
		//Get floor points manually
		vertex [0] = mesh.transform.TransformPoint (t [0]);
		vertex [2] = mesh.transform.TransformPoint (t [120]);
		vertex [1] = mesh.transform.TransformPoint (t [110]);
		vertex [3] = mesh.transform.TransformPoint (t [10]);
		
		//Working in 2D geometry using x and z. y is always 1.
		vertex [0].y = 1; 
		vertex [1].y = 1; 
		vertex [2].y = 1; 
		vertex [3].y = 1; 
		
		Vector3 [] mapBoundary = new Vector3[4]; //the map's four corners
		
		for (int i = 0; i < 4; i++) 
			mapBoundary [i] = vertex [i];
		
		mapBG = new Geometry (); //Countains the map polygon
		for (int i = 0; i < 4; i++)
			mapBG.edges.Add( new Line( mapBoundary[i], mapBoundary[(i + 1) % 4]) );
		
		GameObject[] obs = GameObject.FindGameObjectsWithTag ("Obs");
		
		if (obs == null)
			return; 
		
		//data holder
		Triangulation triangulation = GameObject.Find ("Triangulation").GetComponent<Triangulation> (); 
		triangulation.points.Clear ();
		triangulation.colours.Clear (); 
		
		//Get all polygon
		foreach (GameObject o in obs){
			mesh = (MeshFilter)(o.GetComponent ("MeshFilter"));
			t = mesh.sharedMesh.vertices;
			tempGeometry = new Geometry();
			
			vertex [0] = mesh.transform.TransformPoint (t [6]);
			vertex [1] = mesh.transform.TransformPoint (t [8]);
			vertex [3] = mesh.transform.TransformPoint (t [7]);
			vertex [2] = mesh.transform.TransformPoint (t [9]);
			
			vertex [0].y = 1;
			vertex [2].y = 1;
			vertex [1].y = 1;
			vertex [3].y = 1;
			for (int i = 0; i< vertex.Length; i+=1) {
				if (i < vertex.Length - 1)
					tempGeometry.edges.Add (new Line (vertex [i], vertex [i + 1]));
				else 	       
					tempGeometry.edges.Add (new Line (vertex [0], vertex [i]));
			}	
			geos.Add (tempGeometry);
		}
		
		//lines are defined by all the points in  obs
		lines = new List<Line> ();
		
		obsGeos.Clear ();
		foreach (Geometry g in geos)
			obsGeos.Add(g);
		
		//Create empty GameObject
		GameObject temp = GameObject.Find("temp");
		DestroyImmediate(temp);
		temp = new GameObject("temp");
		
		//CODESPACE
		//Merge obstacles that are intersecting
		for (int i = 0; i < obsGeos.Count; i++) {
			for (int j = i + 1; j < obsGeos.Count; j++) {
				//Check to see if two geometries intersect
				if( obsGeos[i].GeometryIntersect( obsGeos[j] ) ){
					Geometry tmpG = obsGeos[i].GeometryMerge( obsGeos[j] ); 
					//remove item at position i, decrement i since it will be incremented in the next step, break
					obsGeos.RemoveAt(j);
					obsGeos.RemoveAt(i);
					obsGeos.Add(tmpG);
					i--;
					break;
				}
			}
		}
		
		finalPoly = new List<Geometry> ();//Contains all polygons that are fully insde the map
		//Check for obstacles that intersect the map boundary
		//and change the map boundary to exclude them
		foreach ( Geometry g in obsGeos ) {
			if( mapBG.GeometryIntersect( g ) && !mapBG.GeometryInside( g ) ){
				mapBG = mapBG.GeometryMergeInner( g );
				mapBG.BoundGeometry( mapBoundary );
			}
			else
				finalPoly.Add(g);
		}
		
		List<Vector3> allVertex = new List<Vector3>();
		List<Vector3> tempVertex = new List<Vector3>();
		totalGeo = new Geometry ();
		
		//Getting all vertices
		foreach (Geometry g in finalPoly) {
			tempVertex = g.GetVertex();
			foreach( Vector3 v in tempVertex )
				allVertex.Add(v);
			foreach( Line l in g.edges )
				totalGeo.edges.Add(l);
		}
		
		tempVertex = mapBG.GetVertex();
		foreach( Vector3 v in tempVertex )
			allVertex.Add(v);
		foreach( Line l in mapBG.edges )
			totalGeo.edges.Add(l);
		lines.Clear ();
		//---End of obstacle merging---
		
		//-----------START MST CODE------------------//
		//We will use "mapBG" and "finalPoly"
		//finalPoly contains the "quadrilaters"
		//get all lines from quadrilaters/finalPoly and put them in "lines" || We use "obsLines"
		List<Line> obsLines = new List<Line> ();
		List<Geometry> toCheck = new List<Geometry> ();
		foreach (Geometry g in finalPoly) {
			foreach( Line l in g.edges )
				obsLines.Add( l );
			toCheck.Add(g);
		}
		//set links with neighbors for each quadrilater (send list of all obstacles as a paramter)
		foreach (Geometry g in toCheck)
			g.SetVoisins( toCheck, mapBG );
		//keep a list of the edges (graph where obstaceles are the nodes) in a list of lines called "linesLinking"
		List<Vector3> mapVertices = mapBG.GetVertex();
		
		//Possible redundancy here
		//Finds the closest geometry to the map border
		Geometry start = mapBG.findClosestQuad (mapVertices[0], toCheck, mapBG);
		//Connect border to this geometry
		List<Line> linesLinking = new List<Line> ();
		linesLinking.Add (mapBG.GetClosestLine (start, toCheck, mapBG));
		start.visited = true;
		
		List<Geometry> toCheckNode = new List<Geometry> (); 
		toCheckNode.Add (start); 
		Line LinetoAdd = start.voisinsLine [0];

		//Straight Porting//
		while (LinetoAdd != null) {
			LinetoAdd = null; 
			Geometry qToAdd = null; 
			
			//Check all 
			foreach (Geometry q in toCheckNode) {
				
				for (int i = 0; i<q.voisins.Count; i++) {
					if (! q.voisins [i].visited) {
						if (LinetoAdd != null) {
							//get the shortest line
							if ( floatCompare( LinetoAdd.Magnitude (), q.voisinsLine [i].Magnitude (), ">=" ) ){
								LinetoAdd = q.voisinsLine [i];
								qToAdd = q.voisins [i]; 
								
							}
						} else {
							qToAdd = q.voisins [i]; 
							LinetoAdd = q.voisinsLine [i];
						}
					} else {
						continue; 
					}
				}
			}
			if (LinetoAdd != null) {
				linesLinking.Add (LinetoAdd); 
				qToAdd.visited = true; 
				toCheckNode.Add (qToAdd); 
			}
		}
		
		foreach (Line l in linesLinking)
			triangulation.linesMinSpanTree.Add (l);
		//END porting
		
		//-----------END MST CODE--------------------//
		
		
		int vlcnt = 0;
		lines.Clear ();
		//Constructing "lines" for triangulation
		//First add lines that are in MST
		foreach (Line l in linesMinSpanTree)
			lines.Add (l);
		foreach (Vector3 Va in allVertex) {
			foreach(Vector3 Vb in allVertex){
				if( Va != Vb ){
					bool collides = false, essential = false;
					Line tempLine = new Line(Va, Vb);
					//A-Collision with final polygon
					foreach( Line l in totalGeo.edges ){
						if( l.LineIntersectMuntacEndPt( tempLine ) == 1 ){
							collides = true;
							break;
						}
					}
					
					if( collides ) continue;
					
					//B-Collision with existing lines
					foreach( Line l in lines ){
						if( l.LineIntersectMuntacEndPt( tempLine ) == 1 || l.Equals(tempLine) ){
							collides = true;
							break;
						}
					}
					
					if( collides ) continue;
					//C-To avoid diagonals
					foreach( Geometry g in geos ){
						if( g.PointInside( tempLine.MidPoint() ) ){
							collides = true;
							break;
						}
					}
					if( collides ) continue;
					//Add Line
					lines.Add( tempLine );
				}
			}
		}
		
		//Find the centers 
		List<Triangle> triangles = new List<Triangle> ();
		//Well why be efficient when you can be not efficient
		foreach (Line l in lines) {
			Vector3 v1 = l.vertex [0]; 
			Vector3 v2 = l.vertex [1];
			foreach (Line l2 in lines) {
				if (l == l2 || l.Equals(l2))
					continue;
				Vector3 v3 = Vector3.zero;
				
				//if (l2.vertex [0].Equals (v2))
				if( VectorApprox( l2.vertex[0], v2 ) )
					v3 = l2.vertex [1];
				//have to check if closes
				//else if (l2.vertex [1].Equals (v2))
				else if ( VectorApprox(l2.vertex [1], v2 ) )
					v3 = l2.vertex [0];
				
				
				if (v3 != Vector3.zero) {
					foreach (Line l3 in lines) {
						if (l3 == l2 || l3 == l || l3.Equals(l2) || l3.Equals(l) )
							continue; 
						if ((l3.vertex [0].Equals (v1) && l3.vertex [1].Equals (v3))
						    || (l3.vertex [1].Equals (v1) && l3.vertex [0].Equals (v3))) {
							//Debug.DrawLine(v1,v2,Color.red); 
							//Debug.DrawLine(v2,v3,Color.red); 
							//Debug.DrawLine(v3,v1,Color.red); 
							
							//Add the traingle
							Triangle toAddTriangle = new Triangle (
								v1, triangulation.points.IndexOf (v1),
								v2, triangulation.points.IndexOf (v2),
								v3, triangulation.points.IndexOf (v3));
							
							
							Boolean isAlready = false; 
							foreach (Triangle tt in triangles) {
								if (tt.Equals (toAddTriangle)) {
									//Debug.Log(toAddTriangle.refPoints[0]+", "+
									//          toAddTriangle.refPoints[1]+", "+
									//          toAddTriangle.refPoints[2]+", "); 
									isAlready = true; 
									break; 
								}
								
							}
							if (!isAlready) {
								triangles.Add (toAddTriangle);
							}
							
						}
					}
				}
			}
		}
		
		
		//Find shared edge and triangle structure
		
		foreach (Triangle tt in triangles) {
			foreach (Triangle ttt in triangles) {
				if (tt == ttt)
					continue; 
				tt.ShareEdged (ttt, linesMinSpanTree);		
			}
			
		}
		
		triangulation.triangles = triangles;
		

		////////ROADMAP//////////
		foreach(Triangle tt in triangles){
			Line[] ll = tt.GetSharedLines(); 
			
			if(ll.Length == 1)		
				roadMap.Add(new Line(ll[0].MidPoint(), tt.GetCenterTriangle()));
			else if(ll.Length > 2)
			{
				for(int i = 0; i<ll.Length; i++)
				{
					roadMap.Add(new Line(ll[i].MidPoint(), tt.GetCenterTriangle()));
				}				
			}
			else
			{
				for(int i = 0; i<ll.Length; i++)
				{
					roadMap.Add(new Line(ll[i].MidPoint(), ll[(i+1) % ll.Length].MidPoint()));
				}
			}
		}

		////////COLORING//////////
		/// ported code/////
		triangles [0].SetColour ();
		
		//Count Where to put guards 
		List<Vector3> points = new List<Vector3> (); 
		List<Color> coloursPoints = new List<Color> (); 
		
		int[] count = new int[3];
		//0 red, 1 blue, 2 green
		
		foreach (Triangle tt in triangles) 
		{
			//foreach(Vector3 v in tt.vertex)
			for (int j = 0; j<tt.vertex.Length; j++) 
			{
				bool vectorToAdd = true;
				
				for (int i = 0; i<points.Count; i++) 
				{
					if (points [i] == tt.vertex [j] && coloursPoints [i] == tt.colourVertex [j])
						vectorToAdd = false; 			
				}
				
				if (vectorToAdd) {
					points.Add (tt.vertex [j]); 
					coloursPoints.Add (tt.colourVertex [j]); 
				}
				
			}
		}
		
		foreach (Color c in coloursPoints) {
			if (c == Color.red)
				count [0]++;
			else if (c == Color.blue)
				count [1]++;
			else
				count [2]++; 
			
		}
		
		triangulation.points = points; 
		triangulation.colours = coloursPoints; 
		
		//Get points with the least colour
		Color cGuard = Color.cyan; 
		int lowest = 100000000; 
		
		for (int i = 0; i<count.Length; i++) {
			if (count [i] < lowest) {
				if (i == 0)
					cGuard = Color.red;
				else if (i == 1)
					cGuard = Color.blue;
				else
					cGuard = Color.green;
				lowest = count [i];
			}
		}
		
		vlcnt = 0;
		for( int i = 0; i < points.Count; i++ )
		{
			if( colours[i] == cGuard ){
				Vector3 v = points[i];
				cameras.Add( points[i] );
				//				GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				//				inter.transform.renderer.material.color = coloursPoints[i];
				//				inter.transform.position = v;
				//				inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f); 
				//				inter.transform.parent = temp.transform;
				//				vlcnt++;
				//				inter.gameObject.name = vlcnt.ToString();
			}
		}
		//		makeTour ( totalGeo );
		List<Vector3> masterReflex = new List<Vector3> ();
		foreach (Geometry g in finalPoly) {
			List<Vector3> lv = new List<Vector3>();
			lv = g.GetReflexVertex();
			foreach( Vector3 v in lv )
				masterReflex.Add( v );
		}
		List<Vector3> lv2 = new List<Vector3>();
		lv2 = mapBG.GetReflexVertexComplement();
		//lv2 = mapBG.GetVertex ();
		foreach( Vector3 v in lv2 )
			masterReflex.Add( v );
		int hh = 0;

	}

	void drawSphere( Vector3 v ){
		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.transform.renderer.material.color = Color.gray;
		inter.transform.position = v;
		inter.transform.localScale = new Vector3(0.1f,0.1f,0.1f); 
		inter.transform.parent = temp.transform;
		//inter.gameObject.name = vlcnt.ToString();
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
	
}//End of Class