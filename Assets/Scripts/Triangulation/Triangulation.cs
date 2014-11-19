using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Vectrosity;

[ExecuteInEditMode]
public class Triangulation : MonoBehaviour 
{
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
	public bool stopAll = false;

	//Dijkstra
	List<edges>[] EL = new List<edges>[5000];
	//Stores shortest path calculations with node i as source on all nodes j
	float [,] d = new float [300, 300];
	//Stores the path for above calculation
	int [,] parents = new int [300, 300];

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

		foreach(Triangle tt in triangles){
			if(drawTriangles){	
				tt.DrawDebug();
				//foreach(Vector3 v in tt.getVertexMiddle())
					//	points.Add(v);
				//foreach(Color v in tt.colourVertex)
					//	colours.Add(v);
			}

			if(drawRoadMap)	{
//				Line[] ll = tt.GetSharedLines(); 
//			
//
//				if(ll.Length == 1)
//				{
//					Debug.DrawLine(ll[0].MidPoint(), tt.GetCenterTriangle(),Color.red);
//					//Debug.Log("Drawing Red Line at: " + ll[0].MidPoint() + " " + tt.GetCenterTriangle());
//				}
//				else if(ll.Length > 2)
//				{
//					for(int i = 0; i<ll.Length; i++)
//					{
//						Debug.DrawLine(ll[i].MidPoint(), tt.GetCenterTriangle(),Color.red);
//						//Debug.Log("Drawing Red Line at: " + ll[i].MidPoint() + " " + tt.GetCenterTriangle());
//					}
//
//				}
//				
//				else
//				{
//					for(int i = 0; i<ll.Length; i++)
//					{
//						Debug.DrawLine(ll[i].MidPoint(), ll[(i+1) % ll.Length].MidPoint(),Color.red);
//					}
//				}
				//tour.DrawGeometry( GameObject.Find("temp") );
			}
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
			g.SetVoisins( toCheck );
		//keep a list of the edges (graph where obstaceles are the nodes) in a list of lines called "linesLinking"
		List<Vector3> mapVertices = mapBG.GetVertex();

		//Possible redundancy here
		Geometry start = mapBG.findClosestQuad (mapVertices[0], toCheck, new List<Geometry> ());
		List<Line> linesLinking = new List<Line> ();
		linesLinking.Add (mapBG.GetClosestLine (start, toCheck));
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
							if (LinetoAdd.Magnitude () >= q.voisinsLine [i].Magnitude ()) {
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
				if (l == l2)
					continue;
				Vector3 v3 = Vector3.zero; 
				
				
				if (l2.vertex [0].Equals (v2))
					v3 = l2.vertex [1];
					//have to check if closes
				else if (l2.vertex [1].Equals (v2))
					v3 = l2.vertex [0];

				
				if (v3 != Vector3.zero) {
					foreach (Line l3 in lines) {
						if (l3 == l2 || l3 == l)
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
		makeSPRoadMap (masterReflex, totalGeo );
	}

	private void makeSPRoadMap( List<Vector3> masterReflex, Geometry totalGeo ) {
		List<Line> roadMap = new List<Line> ();
		//int lineCnt = 0; //DBG
		foreach (Vector3 vA in masterReflex) {
			foreach (Vector3 vB in masterReflex) {
				if( vA == vB ) continue;
				Line tmpLine = new Line( vA, vB );
				if( roadMap.Contains( tmpLine ) ) continue;
				bool added = false;
				foreach( Line l in totalGeo.edges ){
					//This function only checks mid point. Might want to use something else.
					if( l.Equals( tmpLine ) ){
//						tmpLine.name = "Vector Line" + lineCnt; //DBG
						roadMap.Add( tmpLine );
//						lineCnt++; //DBG
						added = true;
						break;
					}
				}
				if( added ) continue;
				Vector2 vA2 = new Vector2( vA.x, vA.z );
				Vector2 vB2 = new Vector2( vB.x, vB.z );
				Vector2 dirA2 = vB2 - vA2;
				Vector2 dirB2 = vA2 - vB2;
				float alp = 1.02f;
				Vector2 vB_new2 = vA2 + (alp * dirA2);
				Vector2 vA_new2 = vB2 + (alp * dirB2);
				Vector3 vA_new = new Vector3( vA_new2.x, 1, vA_new2.y );
				Vector3 vB_new = new Vector3( vB_new2.x, 1, vB_new2.y );
				bool collides = false;
				foreach( Geometry g in obsGeos ){
					//Note: There maybe a case where after extending the point is not inside a geometry
					//but the line is inside one
					if( g.PointInside(vA_new) || g.PointInside(vB_new) || g.LineCollision(tmpLine) ){ 
						collides = true;
						break;
					}
				}
				if( !collides ){
					//tmpLine.name = "Vector Line" + lineCnt; //for debugging
					roadMap.Add(tmpLine);
					//lineCnt++; //for debugging
				}
			}
		}
		//Draw Roadmap without cameras
//		foreach (Line l in roadMap) {
//			l.DrawVector( GameObject.Find("temp"));
//			l.DrawVector( GameObject.Find("temp"), Color.blue );
//		}
//		//Draw reflex points
//		foreach (Vector3 v in masterReflex) {
//			drawSphere( v, Color.green );		
//		}
		makeTourOnSPR (roadMap, masterReflex);
	}

	public struct edges{//used to represent graph for Dijkstra
		public int v;
		public float w;
		public edges( int a, float b ){
			this.v = a;
			this.w = b;
		}
	}

    void makeTourOnSPR(List<Line> roadMap, List<Vector3> masterReflex){
		Dictionary<Vector3, int> dict = new Dictionary<Vector3, int> ();
		Dictionary<int, Vector3> numToVect = new Dictionary<int, Vector3> ();
		int N = 0;
		//Construct graph in EL
		foreach (Line l in roadMap) {
			if( !dict.ContainsKey( l.vertex[0] ) ){
				EL[N] = new List<edges>();//Initialize edge list for this node
				dict.Add( l.vertex[0], N );
				numToVect.Add( N++, l.vertex[0] );
			}
			if( !dict.ContainsKey( l.vertex[1] ) ){
				EL[N] = new List<edges>();//Initialize edge list for this node
				dict.Add( l.vertex[1], N );
				numToVect.Add( N++, l.vertex[1] );
			}
			int u = dict[l.vertex[0]];
			int v = dict[l.vertex[1]];
			EL[u].Add(new edges( v, l.Magnitude() ));
			EL[v].Add(new edges( u, l.Magnitude() ));
		}
		int GSC = N; //graphSizeSansCameras
		//Add cameras to the graph
		//Connect cameras to each other
		foreach( Vector3 v1 in cameras ){
			foreach( Vector3 v2 in cameras ){
				if( v1.Equals(v2) ) continue;
				if( !dict.ContainsKey( v1 ) ){
					EL[N] = new List<edges>();
					dict.Add( v1, N );
					numToVect.Add( N++, v1 );
				}
				if( !dict.ContainsKey( v2 ) ){
					EL[N] = new List<edges>();
					dict.Add( v2, N );
					numToVect.Add( N++, v2 );
				}
				int u = dict[v1];
				int v = dict[v2];
				Line tmpLine = new Line( v1, v2 );
				bool collides = false;
				foreach( Geometry g in obsGeos ){//Check for visibility
					if( g.LineCollision( tmpLine ) ){
						collides = true;
						break;
					}
				}
				if( !collides ){
					EL[u].Add(new edges( v, tmpLine.Magnitude() ) );
					EL[v].Add(new edges( u, tmpLine.Magnitude() ) );
				}
			}
		}
		//Connect cameras to the rest of the graph
		foreach (Vector3 v1 in cameras) {
			foreach( Vector3 v2 in masterReflex ){
				if( v1.Equals(v2) ) continue;
				int u = dict[v1];
				int v = dict[v2];
				Line tmpLine = new Line( v1, v2 );
				bool collides = false;
				foreach( Geometry g in obsGeos ){//Check for visibility
					if( g.LineCollision( tmpLine ) ){
						collides = true;
						break;
					}
				}
				if( !collides ){
					EL[u].Add(new edges( v, tmpLine.Magnitude() ) );
					EL[v].Add(new edges( u, tmpLine.Magnitude() ) );
				}
			}		
		}
		//Calculate All-Pair-Shortest-Path
        for( int i = 0; i < N; i++ ){
			Dijkstra( i, N );
			for( int j = 0; j < N; j++ ){
				if( i == GSC + 4 )
				  Debug.Log( "Distance from " + i + " to " + j + ": " + d[i, j] );
				if( d[i, j] > 5000 )
					Debug.Log ("WRONG");
			}
		}
		//0 to (GSC - 1) - Draws graph
//		for (int i = 0; i < N; i++) {
//			foreach( edges l in EL[i] ){
//				Line tmpline = new Line( numToVect[i], numToVect[l.v] );
//				tmpline.DrawVector( GameObject.Find("temp") );
//			}
//		}
		//Make tour
		bool [] visited = new bool [300];
		for( int i = 0; i < N; i++ )
			visited[i] = false;
		Debug.Log ("GSC : " + GSC + " N: " + N);
		int current = GSC;
		float tourDistance = 0;
		List< int > tour = new List<int> ();
		tour.Add (GSC);
		int xcnt = 0;
		for( int i = GSC; i < N; i++ ){
			if( !visited[i] )
				xcnt++;
		}
		Debug.Log ("Not vis" + xcnt);
		xcnt = 0;
		while( true ){
			visited[current] = true;
			xcnt++;
			float mindist = 10000f;
			int nearestNeighbor = -1;
			Debug.Log ( current - GSC + 1);
			for( int i = 0; i < N; i++ ){
				if( !cameras.Contains(numToVect[i]) ) continue; //if this is not a camera
				if( i == current ) continue;
				if( visited[i] ) continue;
				if( d[current, i] < mindist ){
					nearestNeighbor = i;
					mindist = d[current, i];
				}
			}
			if( nearestNeighbor == -1 ){
				break;
			}
			//Path
			int src = current;
			int dest = nearestNeighbor;
			Stack<int> stk = new Stack<int> ();
			while( src != dest ){
				stk.Push(dest);
				dest = parents[current,dest];
			}
			while( stk.Count != 0 )
				tour.Add( stk.Pop() );			
			//Size of tour
			tourDistance += mindist;
			current = nearestNeighbor;
		}
		Debug.Log ("Cameras Visited: " + xcnt);
		Debug.Log ("Size of exploration tour: " + tourDistance);
		//Draw tour
		for( int i = 0; i < tour.Count - 1; i++ ){
			Line tmpline = new Line( numToVect[tour[i]], numToVect[tour[i + 1]] );
			//tmpline.DrawVector( GameObject.Find("temp") );
		}
		drawSphere( numToVect[tour[0]], Color.red);
		visibilityPolygonDiff(numToVect[tour[0]]);
	}

	private void Dijkstra( int id, int N ){
		SortedDictionary< float, int > SD = new SortedDictionary< float, int > ();

		for (int i = 0; i <= N; i++) {
			d [id, i] = 100000f;
			parents[id, i] = i;
		}
		d[id, id] = 0f;
		parents [id, id] = id;
		SD.Add( 0f, id );
		
		while( SD.Count > 0 ){
			int u = 0;
			float dist = 0;
			foreach( KeyValuePair<float, int> kvp in SD ){
				dist = kvp.Key;
				u = kvp.Value;
				break;
			}
			SD.Remove(dist);//will it remove the first one when removing duplicate keys
			foreach( edges E in EL[u] ){
				int v = E.v;
				float nw = E.w + dist;
				if( nw < d[id, v] ){
					d[id, v] = nw;
					SD.Add( nw, v );
					parents[ id, v ] = u;
				}
			}
		}
	}

	public void makeTour( Geometry totalGeo ){
		List<Vector3> roadMapPoints = new List<Vector3> ();
		//Get all points in road map
		tour.edges.Clear ();
		foreach (Triangle tt in triangles) {
			Line[] ll = tt.GetSharedLines(); 
			if(ll.Length == 1)
			{
				//Debug.DrawLine(ll[0].MidPoint(), tt.GetCenterTriangle(),Color.red);
				tour.edges.Add( new Line(ll[0].MidPoint(), tt.GetCenterTriangle()) );
				if( !roadMapPoints.Contains( ll[0].MidPoint() ) )
					roadMapPoints.Add( ll[0].MidPoint() );
				if( !roadMapPoints.Contains( tt.GetCenterTriangle() ) )
					roadMapPoints.Add( tt.GetCenterTriangle() );
			}
			else if(ll.Length > 2)
			{
				for(int i = 0; i<ll.Length; i++)
				{
					//Debug.DrawLine(ll[i].MidPoint(), tt.GetCenterTriangle(),Color.red);
					tour.edges.Add( new Line(ll[i].MidPoint(), tt.GetCenterTriangle()) );
					if( !roadMapPoints.Contains( ll[i].MidPoint() ) )
						roadMapPoints.Add( ll[i].MidPoint() );
					if( !roadMapPoints.Contains( tt.GetCenterTriangle() ) )
						roadMapPoints.Add( tt.GetCenterTriangle() );
					//Debug.Log("Drawing Red Line at: " + ll[i].MidPoint() + " " + tt.GetCenterTriangle());
				}
			}			
			else
			{
				for(int i = 0; i<ll.Length; i++)
				{
					//Debug.DrawLine(ll[i].MidPoint(), ll[(i+1) % ll.Length].MidPoint(),Color.red);
					tour.edges.Add( new Line(ll[i].MidPoint(), ll[(i+1) % ll.Length].MidPoint()) );
					if( !roadMapPoints.Contains( ll[i].MidPoint() ) )
						roadMapPoints.Add( ll[i].MidPoint() );
					if( !roadMapPoints.Contains( ll[(i+1) % ll.Length].MidPoint() ) )
						roadMapPoints.Add( ll[(i+1) % ll.Length].MidPoint() );
				}
			}			
		}
		//For each camera find closest connection to roadmap, check for collisions in between
		tour.DrawGeometry( GameObject.Find ( "temp" ) );
		foreach (Vector3 v in cameras) {
			float shortestLine = 100000f;
			Line finalLine = new Line ( Vector3.zero, Vector3.zero );
			foreach( Vector3 rmv in roadMapPoints ){
				Line tmpLine = new Line( v, rmv );
				//check for collision
				bool collides = false;
				foreach( Line l in totalGeo.edges ){
					if( l.LineIntersectMuntacEndPt( tmpLine ) == 1 ){
						collides = true;
						break;
					}
				}
				if( collides ) continue;

				if( tmpLine.Magnitude() < shortestLine ){
					shortestLine = tmpLine.Magnitude();
					finalLine = tmpLine;
				}
			}
			//tour.edges.Add( finalLine );
			finalLine.DrawVector( GameObject.Find("temp"), Color.blue );
		}
		//tour.DrawGeometry( GameObject.Find ( "temp" ) );
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

	void visibilityPolygonDiff( Vector3 kernel ){
		//Find closest intersections
		Stack<Vector3> extendablePoints = new Stack<Vector3> ();
		Debug.Log (totalGeo.edges.Count);
		int cnt = 0;
		List<Vector3> allpoints = totalGeo.GetVertex ();
		//Taking each edge
		foreach (Line obstacleEdge in totalGeo.edges) {
			//Taking each vertex of the edge
			for (int i = 0; i < 2; i++) {
				//Creating new line
				Line NL = new Line (kernel, obstacleEdge.vertex [i]);
				bool intersects = false;
				//Check if the line intersects any other edge
				foreach (Line earlierLine in totalGeo.edges) {
					//If same line
					if (obstacleEdge == earlierLine)
							continue;
					//if intersects
					if (NL.LineIntersectMuntac (earlierLine) != 0){
						Vector3 vect = NL.GetIntersectionPoint(earlierLine);
						//Intersects at only an endpoint
						if( earlierLine.vertex[0] == vect || earlierLine.vertex[1] == vect )
							continue;
						else{
							//drawSphere(vect);
							intersects = true;
							break;
						}
					}
				}
				if (intersects)
					continue;
				//Check if the line is inside one of the original obstacle. 
				//Simultaneously checks if line is outside the final map as it can only be so
				//if it's connected to an obstacle point outside the map
				foreach (Geometry g in obsGeos) {
					List<Vector3> gv = g.GetVertex ();
					if ( gv.Contains (kernel) && g.PointInside (NL.MidPoint ()) ){
						intersects = true;
						break;
					}
				}
				if (!intersects) {
//					Line tmpline = new Line (kernel, obstacleEdge.vertex [i]);
//					tmpline.name = cnt.ToString ();
//					tmpline.DrawVector (GameObject.Find ("temp"), Color.green);
//					cnt++;
					extendablePoints.Push (obstacleEdge.vertex [i]);
				}
			}
		}
		//Extend lines of the extendable points
		Debug.Log ("Size of ext points : " + extendablePoints.Count);
		List<Vector3> VPPoints = new List<Vector3> ();
		while( extendablePoints.Count != 0 ){
			Vector3 expoint = extendablePoints.Pop();
			Vector3 expointext = getExtendedPoint(kernel, expoint, 1.005f);//Potential error, extend less maybe
			bool cantextend = false;
			//Check if slight extension causes it to enter an obstacle
			foreach( Geometry g in finalPoly ){
				if( g.PointInside(expointext) ){
						cantextend = true;
						break;
				}
			}
			if( cantextend ){//Line can't be extended
				//Add to visibility polygon
				VPPoints.Add(expoint);
//				Line tmpline = new Line (kernel, expoint);
//				tmpline.name = cnt.ToString ();
//				tmpline.DrawVector (GameObject.Find ("temp"), Color.cyan);
//				cnt++;
			}
			else{
				//Old line
				Line origline = new Line( kernel, expoint );
				//Create ray
				Line NLX = new Line( kernel, getExtendedPoint( kernel, expoint, 10f ) );
				//Check ray intersection
				Vector3 closestPoint = new Vector3();
				float mindist = 100000f;
				bool validextension = false;
				foreach( Line L in totalGeo.edges ){
					if( L.LineIntersectMuntacEndPt(NLX) != 0 ){
						//Check if same point or earlier colinear point
						Line NLXInter = new Line( kernel, L.GetIntersectionPoint(NLX) );
						float dist = NLXInter.Magnitude();
						if( dist > origline.Magnitude() && dist < mindist ){
							//drawSphere(NLXInter.vertex[1]);
							//Check if it falls into an obstacle
							Line extSeg = new Line( expoint, NLXInter.vertex[1]);
							bool inside = false;
							foreach( Geometry gg in obsGeos ){
								if( gg.PointInside(extSeg.MidPoint()) ){
									inside = true;
									break;
								}
							}
							if( inside ) continue;
							validextension = true;
							mindist = dist;
							closestPoint = NLXInter.vertex[1];
						}
					}
				}
				VPPoints.Add(expoint);
				if( validextension ){
					extendablePoints.Push (closestPoint);
//					drawSphere(closestPoint);
//					Line tmpline = new Line (kernel, closestPoint);
//					tmpline.name = cnt.ToString ();
//					tmpline.DrawVector (GameObject.Find ("temp"), Color.cyan);
//					cnt++;
				}
				else{
					//VPPoints.Add(expoint);
				}
			}
		}
		//return;
		Geometry visiPoly = new Geometry ();
		//Sort w.r.t kernel and some arbitrary point
		List<Vector3> lsv = new List<Vector3> ();
		foreach( Vector3 vv in VPPoints ){
			if( !lsv.Contains(vv) ){
				lsv.Add(vv);
			}
		}
		VPPoints = lsv;
		List<KeyValuePair<Vector3, float>> realVPpts = new List<KeyValuePair<Vector3, float>>  ();
		realVPpts = visiPoly.GetVertexAngleSorted(kernel, lsv);
		List<List<Vector3>> raylist = new List<List<Vector3>> ();
		int rayptr = 0;
		for (int i = 0; i < realVPpts.Count; i++) {
			raylist.Add(new List<Vector3>());
			raylist[rayptr].Add(realVPpts[i].Key);
			float angle = realVPpts[i].Value;
			while( i + 1 < realVPpts.Count && Math.Abs(realVPpts[i + 1].Value - angle) < 0.0000001f ){
				i++;
				raylist[rayptr].Add(realVPpts[i].Key);
			}
			rayptr++;
		}
		GameObject vptmp = new GameObject("vptmp");
		visiPoly.edges.Add( new Line(kernel, raylist[0][0]) );
		raylist.Add(new List<Vector3>());
		raylist[raylist.Count - 1].Add(kernel);//So that kernel is automatically connected back to at the end
		//drawSphere (raylist [0] [0]);
		Debug.Log ("Size of ray list: " + raylist.Count);
		for (int i = 0; i < raylist.Count - 1; i++) {
			//Reverse list if needed
			if( OnSameLine(raylist[i][raylist[i].Count - 1], raylist[i + 1][raylist[i + 1].Count - 1]) )
				raylist[i + 1].Reverse();
			for( int j = 0; j < raylist[i].Count - 1; j++ ){
				visiPoly.edges.Add ( new Line(raylist[i][j], raylist[i][j + 1]) );
				drawSphere(raylist[i][j + 1]);
			}
			visiPoly.edges.Add ( new Line(raylist[i][raylist[i].Count - 1], raylist[i + 1][0]) );
			drawSphere(raylist[i + 1][0]);
		}
		//visiPoly.DrawGeometry (GameObject.Find ("vptemp"));
		visiPoly.DrawGeometry (GameObject.Find ("temp"));
	}

	public Vector3 getExtendedPoint( Vector3 vSrc, Vector3 vobs, float amount ){
		Vector2 vSrc2 = new Vector2( vSrc.x, vSrc.z );
		Vector2 vobs2 = new Vector2( vobs.x, vobs.z );
		Vector2 dirvSrc2 = vobs2 - vSrc2;
		float alp = amount;//100.02f
		Vector2 vobs_new2 = vSrc2 + (alp * dirvSrc2);
		Vector3 vobs_new = new Vector3( vobs_new2.x, 1, vobs_new2.y );
		return vobs_new;
	}

	public bool VectorApprox ( List<Vector3> obs_pts, Vector3 interPt ){
		foreach (Vector3 v in obs_pts) {
			if( Math.Abs (v.x - interPt.x) < 0.01 && Math.Abs (v.z - interPt.z) < 0.01 )
				return true;
		}
		return false;
	}
	public bool VectorApprox ( Vector3 a, Vector3 b ){
		if( Math.Abs (a.x - b.x) < 0.01 && Math.Abs (a.z - b.z) < 0.01 )
			return true;
		else
			return false;
	}
	public bool OnSameLine( Vector3 v1, Vector3 v2 ){
		foreach (Line l in totalGeo.edges) {
			bool la = false;
			bool lb = false;
			Line lv1a = new Line( l.vertex[0], v1 );
			Line lv1b = new Line( l.vertex[1], v1 );
			Line lv2a = new Line( l.vertex[0], v2 );
			Line lv2b = new Line( l.vertex[1], v2 );
			if( Math.Abs ( l.Magnitude() - (lv1a.Magnitude() + lv1b.Magnitude()) ) < 0.01f )
				la = true;
			if( Math.Abs ( l.Magnitude() - (lv2a.Magnitude() + lv2b.Magnitude()) ) < 0.01f )
				lb = true;
			if( la && lb )
				return true;
		}
		return false;
	}
}//End of Class

	