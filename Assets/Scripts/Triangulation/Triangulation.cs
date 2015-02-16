using UnityEngine;
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
	public List<Line> lines; 

	List<Geometry> geos = new List<Geometry>();//Contains original polygons. Includes ones outside the map.
	public Triangulation triangulation;
	public List<Line> linesMinSpanTree = new List<Line>(); 
	public Vector3[] mapBoundary = new Vector3[4];
	public List<Geometry> obsGeos = new List<Geometry> (); //has all merged polygons. doesn't have proper border/obstacles.
	public List<Geometry> finalPoly = new List<Geometry> ();//has proper obstacles
	public Geometry totalGeo = new Geometry ();
	//Contains Map
	public Geometry mapBG;
	//Camera
	public List<KeyValuePair<Vector3,Geometry>> cameraVPS = new List<KeyValuePair<Vector3, Geometry>>();
	//Contains all the visibility polygons for cameras
	public int vpnum = -1;
	
	public bool drawTriangles = false;
	public bool drawSPRoadMap = false;
	public bool drawMinSpanTree = false;
	public bool drawGeometry = false;
	public bool drawCameras = false;
	public bool drawVP = false;
	public bool stopAll = false;
	private bool geometryDrawn = false;
	private bool sprDrawn = false;

	List<Vector3> masterReflex = new List<Vector3>();

	//Tour
	private List<Line> spRoadMap = new List<Line> ();
	private List<Vector3> explorationTour = new List<Vector3> ();
	private Geometry tourEdges = new Geometry ();
	//Dijkstra
	List<edges>[] EL = new List<edges>[5000];
	//Stores shortest path calculations with node i as source on all nodes j
	float [,] d = new float [300, 300];
	//Stores the path for above calculation
	int [,] parents = new int [300, 300];

	public void Start(){
	}

	public void Clear(){
		points.Clear(); 
		colours.Clear();
		cameras.Clear ();
		obsGeos.Clear ();
		finalPoly.Clear ();
		triangles.Clear(); 
		lines.Clear(); 
		geos.Clear();
		linesMinSpanTree.Clear();
		masterReflex.Clear ();
		spRoadMap.Clear ();
		GameObject temp = GameObject.Find("temp");
		DestroyImmediate(temp);
		GameObject vptmp = GameObject.Find("vptmp");
		DestroyImmediate(vptmp);
		geometryDrawn = false;
		sprDrawn = false;
		stopAll = true;
		explorationTour.Clear ();
		cameraVPS.Clear ();
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
			if( !geometryDrawn ){
				//totalGeo.DrawGeometry(GameObject.Find("temp"));
				mapBG.DrawGeometry(GameObject.Find("temp"));
				geometryDrawn = true;
			}
		}

		if (drawCameras) {
			drawCameras = false;
			int cntcam = 0;
			foreach( Vector3 v in cameras ){
//				if( cntcam == 15 || cntcam == 16 )
//					visibilityPolygon( v ).DrawGeometry( GameObject.Find("temp") );
				drawSphere( v, Color.blue, cntcam++ );
			}		
		}

		if (drawSPRoadMap){
				GameObject temp = GameObject.Find("temp"); 
				foreach(Line l in spRoadMap)
					l.DrawLine( Color.yellow );
		}

		if( drawVP && vpnum != -1 ){
			Line tmpline = new Line (cameraVPS[vpnum].Key, new Vector3(-100,1,100));
			tmpline.DrawLine(Color.white);
			foreach( Line l in cameraVPS[vpnum].Value.edges )
				l.DrawLine( Color.blue );
		}

		foreach(Triangle tt in triangles){
			if(drawTriangles){	
				tt.DrawDebug();
			}
		}
	}

    public void TriangulationSpace (){
		/***PHASE A: PREPROCESSING***/
		/*STEP 1 - GET POLYGON POINTS*/
		getPolygons ();
		/*STEP 2 - MERGE POLYGONS*/
		mergePolygons ();
		/*STEP 3 - DEFINE MAP BOUNDARY*/
		defineBoundary ();
		
		/***PHASE B: COLORING***/
		/*STEP 4 - MAKE MST OF POLYGONS*/
		MST ();		
		/*STEP 5 - TRIANGULATE*/
		triangulate ();		
		/*STEP 6 - COLOR AND GET CAMERAS*/
		colorCameras ();

		/***PHASE C: TOUR***/
		/*STEP 7 - CREATE "SHORTEST-PATH" ROADMAP*/
		makeSPRoadMap ();
		/*STEP 8 - RUN DIJKSTRA FOR EVERY CAMERA PAIR*/
		makeTourOnSPR ();
		/*STEP 9 - CREATE TOUR USING NEAREST NEIGHBOUR*/
		//happens in makeTourOnSPR

		/***PHASE D: VISIBILITY***/
		/*STEP 10 - GET VISIBILITY POLYGONS OF CAMERAS*/
		getCameraVPS ();
		/*STEP 11 - CHECK CAMERA VISIBILITY NESTING*/
		//cameraNesting ();
	}

	void getPolygons(){
		//Compute one step of the discritzation
		//Find this is the view
		GameObject floor = (GameObject)GameObject.Find ("Floor");
		Vector3 [] vertex = new Vector3[4]; 
		//First geometry is the outer one
		geos = new List<Geometry> ();
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
		
		mapBoundary = new Vector3[4]; //the map's four corners
		
		for (int i = 0; i < 4; i++) 
			mapBoundary [i] = vertex [i];
		
		mapBG = new Geometry (); //Countains the map polygon
		for (int i = 0; i < 4; i++)
			mapBG.edges.Add( new Line( mapBoundary[i], mapBoundary[(i + 1) % 4]) );
		
		GameObject[] obs = GameObject.FindGameObjectsWithTag ("Obs");
		
		if (obs == null)
			return; 
		
		//data holder
		triangulation = GameObject.Find ("Triangulation").GetComponent<Triangulation> (); 
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
	}

	void mergePolygons(){
		//Merge obstacles that are intersecting
		for (int i = 0; i < obsGeos.Count; i++){
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
	}

	void defineBoundary(){
		//Check for obstacles that intersect the map boundary
		//and change the map boundary to exclude them
		finalPoly = new List<Geometry> ();//Contains all polygons that are fully insde the map
		foreach ( Geometry g in obsGeos ) {
			if( mapBG.GeometryIntersect( g ) && !mapBG.GeometryInside( g ) ){
				mapBG = mapBG.GeometryMergeInner( g );
				mapBG.BoundGeometry( mapBoundary );
			}
			else
				finalPoly.Add(g);
		}
	}

	void MST(){
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
		
		//Can be made simpler
		//Find a geometry to link to any of the map vertices
		Geometry start = null;
		for (int i = 0; i < mapVertices.Count; i++) {
			start = mapBG.findClosestQuad (mapVertices [i], toCheck, mapBG);
			if( start != null )
				break;
		}
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
	}

	void triangulate(){
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
					//C-To avoid diagonals and polygons outside the map (i.e. those not in obstacle list)
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
		triangles = new List<Triangle> ();
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
	}

	void colorCameras(){
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
		
		int vlcnt = 0;
		for( int i = 0; i < points.Count; i++ )
		{
			if( colours[i] == cGuard ){
				Vector3 v = points[i];
				cameras.Add( points[i] );
				//drawSphere(v, Color.green);
			}
		}
	}

	private void makeSPRoadMap() {
		//Get all reflex vertices from obstacles
		masterReflex = new List<Vector3> ();
		foreach (Geometry g in finalPoly){
			List<Vector3> lv = new List<Vector3>();
			lv = g.GetReflexVertex();
			foreach( Vector3 v in lv )
				masterReflex.Add( v );
		}

		List<Vector3> lv2 = new List<Vector3>();
		//Get reflex vertices(interior) of map
		lv2 = mapBG.GetReflexVertexComplement();
		foreach( Vector3 v in lv2 )
			masterReflex.Add( v );

		spRoadMap = new List<Line> ();
		//Connect all reflex vertices
		foreach (Vector3 vA in masterReflex) {
			//drawSphere( vA, Color.blue, cnt++);
			foreach (Vector3 vB in masterReflex) {
				if( vA == vB || VectorApprox( vA, vB ) ) continue;
				Line tmpLine = new Line( vA, vB );
				if( spRoadMap.Contains( tmpLine ) ) continue;
				bool added = false;
				//Add link if already an obstacle edge
				foreach( Line l in totalGeo.edges ){
					//This function only checks mid point. Might want to use something else.
					if( l.Equals( tmpLine ) ){
//						tmpLine.name = "Vector Line" + lineCnt; //DBG
						spRoadMap.Add( tmpLine );
						//tmpLine.DrawVector( GameObject.Find("temp"), Color.cyan );
//						lineCnt++; //DBG
						added = true;
						break;
					}
				}
				if( added ) continue;
				//Check for regular collisions
				bool collides = false;
				foreach( Geometry g in geos ){
					//Note: There maybe a case where after extending the point is not inside a geometry
					//but the line is inside one
					if( g.PointInside(tmpLine.MidPoint()) ){
						collides = true;
						break;
					}
				}
				foreach( Line l in totalGeo.edges ){
					if( l.LineIntersectMuntacEndPt(tmpLine) == 1 ){
						collides = true;
						break;
					}
				}
				if( collides )
					continue;
				//Checking for extendability
				Vector2 vA2 = new Vector2( vA.x, vA.z );
				Vector2 vB2 = new Vector2( vB.x, vB.z );
				Vector2 dirA2 = vB2 - vA2;
				Vector2 dirB2 = vA2 - vB2;
				float alp = 1.02f;
				Vector2 vB_new2 = vA2 + (alp * dirA2);
				Vector2 vA_new2 = vB2 + (alp * dirB2);
				Vector3 vA_new = new Vector3( vA_new2.x, 1, vA_new2.y );
				Vector3 vB_new = new Vector3( vB_new2.x, 1, vB_new2.y );
				Line extLine = new Line( vA_new, vB_new );
				//TODO:Bug Point - geos or ObsGeos and MapBG
				foreach( Geometry g in geos ){
					//Note: There maybe a case where after extending the point is not inside a geometry
					//but the line is inside one
					if( g.PointInside(vA_new) || g.PointInside(vB_new) || g.PointInside(extLine.MidPoint()) ){
						collides = true;
						break;
					}
				}
				foreach( Line l in totalGeo.edges ){
					if( l.LineIntersectMuntacEndPt(tmpLine) == 1 ){
						collides = true;
						break;
					}
				}
				if( !collides ){
					//tmpLine.name = "Vector Line" + lineCnt; //for debugging
					spRoadMap.Add(tmpLine);
					//tmpLine.DrawVector( GameObject.Find("temp"), Color.cyan );
					//lineCnt++; //for debugging
				}
			}
		}
	}

	public struct edges{//used to represent graph for Dijkstra
		public int v;
		public float w;
		public edges( int a, float b ){
			this.v = a;
			this.w = b;
		}
	}

    void makeTourOnSPR(){
		Debug.Log ("Total Cameras:");
		Debug.Log (cameras.Count);
		Dictionary<Vector3, int> dict = new Dictionary<Vector3, int> ();
		Dictionary<int, Vector3> numToVect = new Dictionary<int, Vector3> ();
		int N = 0;
		//Construct graph in EL (global variable, edge list)
		foreach (Vector3 v in masterReflex){
			EL[N] = new List<edges>();
			dict.Add( v, N );
			numToVect.Add(N, v);
			N++;
			//drawSphere( v, Color.blue);
		}

		foreach (Line l in spRoadMap) {
			int u = dict[l.vertex[0]];
			int v = dict[l.vertex[1]];
			EL[u].Add(new edges( v, l.Magnitude() ));
			EL[v].Add(new edges( u, l.Magnitude() ));
		}
		int GSC = N; //graphSizeSansCameras

		/*CAMERA WORK*/
		//1. Add cameras to the graph
		foreach (Vector3 v in cameras) {
			if( !dict.ContainsKey( v ) ){
				EL[N] = new List<edges>();
				dict.Add( v, N );
				numToVect.Add(N, v);
				N++;
			}
		}
		//2. Connect cameras to each other and the rest of the graph (i.e. reflex points)
		List<Vector3> masterReflexAndCamera = new List<Vector3> ();
		foreach (Vector3 v1 in cameras)
				masterReflexAndCamera.Add (v1);
		foreach (Vector3 v1 in masterReflex)
			masterReflexAndCamera.Add (v1);
		foreach( Vector3 v1 in cameras ){
			//drawSphere( v1, Color.red);
			foreach( Vector3 v2 in masterReflexAndCamera){
				if( VectorApprox( v1, v2 ) ) continue;
				int u = dict[v1];
				int v = dict[v2];
				Line tmpLine = new Line( v1, v2 );
				bool collides = false;
				if( spRoadMap.Contains( tmpLine ) ) continue;
				bool added = false;
				//Add link if already an obstacle or map edge
				foreach( Line l in totalGeo.edges ){
					if( l.Equals( tmpLine ) ){
						EL[u].Add(new edges( v, tmpLine.Magnitude() ) );
						EL[v].Add(new edges( u, tmpLine.Magnitude() ) );
						added = true;
						break;
					}
				}
				if( added ) continue;
				//Check for visibility. 
				//Line is diagonal inside a geometry.
//				foreach( Geometry g in geos ){
//					if( g.PointInside( tmpLine.MidPoint() ) ){
//						collides = true;
//						break;
//					}
//				}
//				if( collides ) continue;
//				//Cameras only at vertices so only endpoint to endpoint intersection need to be ignored
//				//Unless the intersection point is a corner
//				foreach( Line l in totalGeo.edges ){
//					if( l.LineIntersectMuntacEndPt(tmpLine) == 1 ){
//						//Colinearity check
//						Vector3 intpoint = l.GetIntersectionPoint(tmpLine);
//						if( l.isEndPoint( intpoint ) || tmpLine.isEndPoint( intpoint ) )
//							continue;
//						collides = true;
//						break;
//					}
//				}
//				if( collides ) continue;
//				//Check if outside map
//				if( !mapBG.PointInside(tmpLine.MidPoint()) )
//					collides = true;
				collides = collisionGeneral( tmpLine, 1, -1 );
				if( !collides ){
					EL[u].Add(new edges( v, tmpLine.Magnitude() ) );
					EL[v].Add(new edges( u, tmpLine.Magnitude() ) );
					//Line tmptmp = new Line( v1, v2 );
					//tmptmp.DrawVector( GameObject.Find("temp"), Color.red );
				}
			}
		}

		//3. Connect cameras to the rest of the graph
		/*foreach (Vector3 v1 in cameras) {
			foreach( Vector3 v2 in masterReflex ){
				if( VectorApprox( v1, v2 ) ) continue;
				int u = dict[v1];
				int v = dict[v2];
				Line tmpLine = new Line( v1, v2 );
				bool collides = false;
				//Check for visibility. 
				//Line is diagonal inside a geometry.
				foreach( Geometry g in geos ){
					if( g.PointInside( tmpLine.MidPoint() ) ){
						collides = true;
						break;
					}
				}
				if( collides ) continue;
				//Cameras only at vertices so only endpoint to endpoint intersection need to be ignored
				//Unless the intersection point is a corner
				foreach( Line l in totalGeo.edges ){
					if( l.LineIntersectMuntacEndPt(tmpLine) == 1 ){
						//Colinearity check
						Vector3 intpoint = l.GetIntersectionPoint(tmpLine);
						if( l.isEndPoint( intpoint ) || tmpLine.isEndPoint( intpoint ) )
							continue;
						collides = true;
						break;
					}
				}
				if( collides ) continue;
				//Check if outside map
				if( !mapBG.PointInside(tmpLine.MidPoint()) )
					collides = true;
				if( !collides ){
					EL[u].Add(new edges( v, tmpLine.Magnitude() ) );
					EL[v].Add(new edges( u, tmpLine.Magnitude() ) );
					//Line tmptmp = new Line( v1, v2 );
					//tmptmp.DrawVector( GameObject.Find("temp"), Color.yellow );
				}
			}		
		}*/
		/*DIJKSTRA*/
		//Calculate All-Pair-Shortest-Path
        for( int i = 0; i < N; i++ ){
			Dijkstra( i, N );
			/*for( int j = 0; j < N; j++ ){
				if( i == GSC + 4 )
				  Debug.Log( "Distance from " + i + " to " + j + ": " + d[i, j] );
				if( d[i, j] > 5000 )
					Debug.Log ("WRONG");
			}*/
		}
//		0 to (GSC - 1) - Draws graph
		for (int i = 0; i < N; i++) {
			foreach( edges l in EL[i] ){
				Line tmpline = new Line( numToVect[i], numToVect[l.v] );
				//tmpline.DrawVector( GameObject.Find("temp") );
			}
		}
		/*MAKE TOUR*/
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
		xcnt = 0;
		List<Vector3> drawnCameras = new List<Vector3> ();
		for( int i = 0; i < tour.Count - 1; i++ ){
			Line tmpline = new Line( numToVect[tour[i]], numToVect[tour[i + 1]] );
			//tmpline.DrawVector( GameObject.Find("temp"), Color.yellow );
			tourEdges.edges.Add(tmpline);
			Vector3 v = numToVect[tour[i]];
			if( cameras.Contains( v ) && !drawnCameras.Contains(v)){
				//drawSphere( v, Color.green, xcnt++ );
				drawnCameras.Add(v);
			}
		}

		Vector3 vv = numToVect [tour [tour.Count - 1]];
		if( cameras.Contains( vv ) && !drawnCameras.Contains(vv)){
			//drawSphere( vv, Color.green, xcnt++ );
			drawnCameras.Add(vv);
		}
		xcnt = 0;
		Geometry vpdraw = new Geometry ();
		List<Geometry> vplist = new List<Geometry> ();
		Debug.Log ("Exp tour" + explorationTour.Count);
		for (int i = 0; i < tour.Count - 1; i++) {
			Vector3 v = numToVect[tour[i]];
			explorationTour.Add(v);
			if( cameras.Contains( v ) ){
				if( xcnt == 6 ){
					//vpdraw = visibilityPolygon (numToVect [tour [i]]);
					//Line tmpline = new Line( v, new Vector3(-100,1,100) );
					//vpdraw.DrawGeometry(GameObject.Find ("temp"));
					//tmpline.DrawVector(GameObject.Find("temp"));
					//vplist.Add(vpdraw);
				}
				xcnt++;
			}
		}
		explorationTour.Add (numToVect[tour[tour.Count - 1]]);
		Debug.Log ("Exp tour after" + explorationTour.Count);
		Debug.Log ("Tour size" + tour.Count);
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

	void drawSphere( Vector3 v, Color x, int vlcnt ){
		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.transform.renderer.material.color = x;
		inter.transform.position = v;
		inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
		inter.transform.parent = temp.transform;
		inter.gameObject.name = vlcnt.ToString();
	}

	Geometry visibilityPolygon( Vector3 kernel ){
		List<Vector3> allpoints = totalGeo.GetVertex ();
		List<Vector3> vp = new List<Vector3> ();
		int cnt = 0;
		//Step 1 - Find all points that are initially visible
		foreach (Vector3 v in allpoints){
//			cnt++;
//			drawSphere( v, Color.green, cnt);
			Line tmpLine = new Line( kernel, v );
			bool collides = false;
			//TODO: same check, also in more points
			//Check for collision with any obstacles (no endpoint collision)
			foreach( Geometry g in finalPoly ){
				if( g.LineCollision(tmpLine) ){ 
					collides = true;
//					if( cnt == 35 ){
//						Debug.Log ("35 in g.linecoll");
//						foreach(Line l1 in g.edges){
//							if( tmpLine.LineIntersectMuntac(l1) == 1 ){
//								l1.DrawVector(GameObject.Find("temp"), Color.yellow);
//								tmpLine.DrawVector(GameObject.Find("temp"), Color.red);
//								drawSphere( l1.GetIntersectionPoint(tmpLine), Color.white);
//								Debug.Log("fuck");
//								Vector3 vtmp = l1.GetIntersectionPoint(tmpLine);
//								Debug.Log ( l1.vertex[0] + "" + l1.vertex[1] + "" + vtmp );
//								if( l1.isEndPoint( vtmp ) || VectorApprox( vtmp, l1.vertex[0], 0 ) )
//									Debug.Log("Is an endpoint for 35 inter");
//							}
//						}
//					}
				}
				else{
					//This is for cases where the midpoint is not located inside the geometry
					//but the line crosses the geometry anyway and intersects only at endpoints
					//of the geometry's edge (midpoint scenario happens only in this case)
					foreach( Line l in g.edges ){
						if( l.LineIntersectMuntacEndPt( tmpLine ) == 1 ){
							Vector3 interv = l.GetIntersectionPoint( tmpLine );
							if( l.isEndPoint( interv ) ){
								Line TLA = new Line( tmpLine.vertex[0], interv );
								Line TLB = new Line( tmpLine.vertex[1], interv );
								if( g.PointInside( TLA.MidPoint() ) || g.PointInside( TLB.MidPoint() ) ){
									collides = true;
									if( cnt == 35 )	Debug.Log ("35 in else of g.linecoll");
									break;
								}
							}
						}
					}
				}
				if( collides )
					break;
			}
			//Check for collision with map boundary
			//Note: Was buggy before LineIntersectMuntac's final check started using eps
			int printvar = -1;
			foreach( Line l in mapBG.edges ){
				if( l.LineIntersectMuntac( tmpLine ) == 1 ){
					if( cnt == 35 )	Debug.Log ("35 in mapbg");
					collides = true;
					break;
				}
				//TODO: I think this is still needed. Uncomment shortly.
//				if( l.LineIntersectMuntacEndPt( tmpLine ) == 1 ){
//					if( l.isEndPoint( l.GetIntersectionPoint( tmpLine ) ) ){
//						collides = true;
//						break;
//					}
//				}
			}
			//The point inside function doesn't (always?) work
			//when the poin is on the polygon edge.
			if( !collides && !mapBG.PointInside( tmpLine.MidPoint() ) ){
				collides = true;
				foreach( Line lb in mapBG.edges ){
					if( VectorApprox( lb.MidPoint(), tmpLine.MidPoint() ) ){
						if( cnt == 35 )	Debug.Log ("35 in pointinside");
						collides = false;
						break;
					}
				}

			}
			if(!collides){
				vp.Add ( v );
//				Line tmpline = new Line( kernel, v );
//				tmpline.name = "Line" + cnt;
//				tmpline.DrawVector(GameObject.Find("temp"), Color.cyan);
 			}
		}
		//Step 2- Sort the points by angle
		Geometry x = new Geometry ();
		List<KeyValuePair<Vector3, float>> anglist = x.GetVertexAngleSorted (kernel, vp);
		cnt = 0;
//		Line temp2 = new Line (kernel, anglist[0].Key);
//		foreach(KeyValuePair<Vector3, float> kvp in anglist) {
//			Line tmpline = new Line( kernel, kvp.Key );
//			drawSphere( kvp.Key );
//			tmpline.name = "Line" + cnt++ + " " + kvp.Value;
//			tmpline.DrawVector(GameObject.Find("temp"), Color.cyan);
//		}
//		return new Geometry();

		//Step 3 - Extend where applicable and find rest of the points
		//foreach (KeyValuePair<Vector3, float> kvp in anglist) {
		List<KeyValuePair<Vector3, float>> anglistExt = new List<KeyValuePair<Vector3, float>> ();
		for( int i = 0; i < anglist.Count; i++ ){
			Vector3 v = anglist[i].Key;
			float angle = anglist[i].Value;
			if( v == kernel )
				continue;
//			Line dbgline = new Line( kernel, v );
//			dbgline.name = "Line" + i;
//			dbgline.DrawVector(GameObject.Find("temp"), Color.yellow);
			Vector3 nv = v;
			Vector3 prevv = kernel;

			KeyValuePair<bool, Vector3> mrpts = new KeyValuePair<bool, Vector3>(true, nv);
			int depth = 0;
			while( mrpts.Key ){
				++depth;
				anglistExt.Add( new KeyValuePair<Vector3, float>( nv, angle ));
				mrpts = morePoints(prevv, nv, i);
				prevv = nv;
				nv = mrpts.Value;
			}
		}
		//Step 4 - Order the points
		List<Vector3> vispol = new List<Vector3>();
		string startfrom = "none";
		for( int i = 0; i < anglistExt.Count; i++ ){
			Vector3 vx = anglistExt[i].Key;
			if( vx == kernel )
				continue;
			bool printsome = false;
//			if( i <= 10 ){
//				new Line( kernel, vx ).DrawVector( GameObject.Find ("temp"), Color.red );
//				printsome = true;
//			}
			List<Vector3> templistA = new List<Vector3>();
			List<Vector3> templistB = new List<Vector3>();
			int indA, indB;
			float angleA = anglistExt[i].Value;
			for( indA = i; indA < anglistExt.Count && anglistExt[indA].Value == angleA; indA++, i++ ){
				templistA.Add( anglistExt[indA].Key );
			}
			if( indA == anglistExt.Count ){
				//TODO:double check this scenario
				if( startfrom == "last" )
					templistA.Reverse();
				foreach( Vector3 v in templistA )
					vispol.Add( v );
				break;
			}
			float angleB = anglistExt[i].Value;
			for( indB = i; indB < anglistExt.Count && anglistExt[indB].Value == angleB; indB++ ){
				templistB.Add( anglistExt[indB].Key );
			}
			i = indA - 1;//since it'll be incremented after the end of the loop
			indA = templistA.Count - 1;
			indB = templistB.Count - 1;
			bool connected = false;
			bool addkernel = false;
//			if( templistA.Count != indA + 1 )
//				Debug.Log ( "counter error A");
//			if( templistB.Count != indB + 1 )
//				Debug.Log ( "counter error B");
			if( angleB - angleA > Math.PI ){
				//FP to kernel;
				startfrom = "none";
				templistA.Reverse();
				addkernel = true;
			}
			else{
				bool LL = connectable( templistA[indA], templistB[indB] );
				bool LF = connectable( templistA[indA], templistB[0] );
				bool FL = connectable( templistA[0], templistB[indB] );
				bool FF = connectable ( templistA[0], templistB[0] );
				if( startfrom != "last" ){ 
					if( LL && LF && FL && FF  ){
						//LP to LP
						startfrom = "last";
						connected = true;
					}
					else if( LF ){
						//LP to FP
						startfrom = "first";
						connected = true;
					}
				}
				if( !connected && startfrom != "first" ){
					if( FL ){
						//FP to LP
						templistA.Reverse();
						startfrom = "last";
						connected = true;
					}	
					else if( FF ){
						//FP to FP
						templistA.Reverse();
						startfrom = "first";
						connected = true;
					}
				}
				if( !connected ){
					//FP to Kernel
					templistA.Reverse();
					startfrom = "none";
					connected = true;
					addkernel = true;
				}
			}
			foreach( Vector3 v in templistA )
				vispol.Add( v );
			if( addkernel )
				vispol.Add( kernel );
			if( printsome )
				Debug.Log ( startfrom );

		}

		//If kernel hasn't been added yet
        if (!vispol.Contains (kernel)) {
			vispol.Add (kernel);
			//drawSphere( kernel, Color.red, -1 );
		}
		cnt = 0;
		foreach (Vector3 v in vispol) {
			//drawSphere( v, Color.cyan, cnt++ );
			//Line tmpline = new Line( kernel, v );			
			//tmpline.name = "Line" + cnt++;
			//tmpline.DrawVector(GameObject.Find("temp"), Color.yellow);
		}
		cnt = 0;
		//return new Geometry();
		Geometry VPret = new Geometry ();
		for( int i = 0; i < vispol.Count; i++ ){
			Line tmpline = new Line( kernel, vispol[i] );
			//drawSphere( vispol[i], Color.red );
			if( i == vispol.Count - 1 ){
				tmpline = new Line( vispol[i], vispol[0] );
				//tmpline = null;
			}
			else{
				tmpline = new Line( vispol[i], vispol[i + 1] );
			}
			tmpline.name = "Line" + cnt++;
			VPret.edges.Add(tmpline);
			//tmpline.DrawVector(GameObject.Find("temp"), Color.cyan);
		}
		return VPret;
	}

	bool connectable( Vector3 A, Vector3 B ){
		Line tmpline = new Line (A, B);
		return !collisionGeneral( tmpline, 0, 0 );
	}

	KeyValuePair<bool, Vector3> morePoints( Vector3 vA, Vector3 vB, int i ){
		//Extend Line
		Vector2 vA2 = new Vector2( vA.x, vA.z );
		Vector2 vB2 = new Vector2( vB.x, vB.z );
		Vector2 dirA2 = vB2 - vA2;//Direction from A towards B
		float alp = 1.08f;
		//float alp = 1.01f;
		Vector2 vB_new2 = vA2 + (alp * dirA2);
		Vector3 vB_new = new Vector3( vB_new2.x, 1, vB_new2.y );
		bool collides = false;
		//collides = collisionGeneral (new Line (vA, vB_new));
		//Check if new endpoint is inside a geometry
		foreach( Geometry g in geos ){
			//Note: There maybe a case where after extending the point is not inside a geometry
			//but the line is inside one
			if( g.PointInside(vB_new) ){ 
				collides = true;
				break;
			}
		}
		//For the case where line is coliner to a map edge and then point goes out of map but midpoint of line stays in
		if (!mapBG.PointInside (vB_new)) {
			collides = true;
		}
		if (collides)
			return new KeyValuePair<bool, Vector3> (false, new Vector3());
		//Extend ray widely and find closest intersection point
		//vA = vA;
		Vector3 origvB = vB;
		vB = vB_new;
		//vA2 = new Vector2( vA.x, vA.z );
		vB2 = new Vector2( vB.x, vB.z );
		dirA2 = vB2 - vA2;//Direction from A towards B
		alp = 100f / new Line( vA, vB ).Magnitude();//In case original line is too small
		vB_new2 = vA2 + (alp * dirA2);
		vB_new = new Vector3( vB_new2.x, 1, vB_new2.y );
		collides = false;
		//New line is from vB to vB_new as involving vA would bring back noted intersection points (i.e. vB)
		Line ray = new Line (vB, vB_new);
		float mindist = 1000f;
		Vector3 retvect = new Vector3 ();
		//if (i == 5)
		//	ray.DrawVector (GameObject.Find ("temp"), Color.grey);
		foreach (Geometry g in obsGeos) {
			foreach(Line l1 in g.edges){
				if( l1.LineIntersectMuntac(ray) == 1 ){
					Vector3 v = l1.GetIntersectionPoint(ray);
					if( VectorApprox( v, origvB ) )
						continue;
					Line tmpline = new Line( vB, v );
					if( tmpline.Magnitude() < mindist ){
						mindist = tmpline.Magnitude();
						retvect = v;
					}
				}
			}
		}
		foreach (Line l1 in mapBG.edges) {
			if( l1.LineIntersectMuntac(ray) == 1 ){
				Vector3 v = l1.GetIntersectionPoint(ray);
				if( VectorApprox( v, origvB ) )
					continue;
				Line tmpline = new Line( vB, v );
				if( tmpline.Magnitude() < mindist ){
					mindist = tmpline.Magnitude();
					retvect = v;
				}
			}
		}
		return new KeyValuePair< bool, Vector3 >(true, retvect);
	}

	void getCameraVPS(){
		List<Vector3> tempcam = new List<Vector3> ();
		foreach (Vector3 v in explorationTour) {
			if( cameras.Contains(v) && !tempcam.Contains(v) ){
				tempcam.Add(v);
				cameraVPS.Add ( new KeyValuePair<Vector3, Geometry>( v, visibilityPolygon( v ) ) );
			}
		}
	}

	void cameraNesting(){
		Debug.Log ("Cameras in cameraVPS");
		Debug.Log (cameraVPS.Count);
		Debug.Log (explorationTour.Count);
		Debug.Log (cameras.Count);
		int cnt = 0;
		List<Vector3> allpoints = totalGeo.GetVertex ();
		List<Geometry> allvps = new List<Geometry> ();
		for (int i = 0; i < cameraVPS.Count; i++) {
			Geometry vpA = cameraVPS[i].Value;
//			if( i == 1 )
//				vpA.DrawGeometry(GameObject.Find("temp"));
//			for( int j = 0; j < cameraVPS.Count; j++ ){
//				if( i == j ) continue;
//				Geometry vpB = cameraVPS[j].Value;
//				if( vpA.GeometryInside( vpB ) )
//					cnt++;
//			}
		}
		Debug.Log (cnt);
		//foreach( Vector3 v in 
	}

	public bool OnSameLine( Vector3 v1, Vector3 v2 ){
		foreach (Line l in totalGeo.edges) {
			bool la = false;
			bool lb = false;
			Line lv1a = new Line( l.vertex[0], v1 );
			Line lv1b = new Line( l.vertex[1], v1 );
			Line lv2a = new Line( l.vertex[0], v2 );
			Line lv2b = new Line( l.vertex[1], v2 );
			//if( Math.Abs ( l.Magnitude() - (lv1a.Magnitude() + lv1b.Magnitude()) ) < 0.01f )
			if( floatCompare( l.Magnitude(), (lv1a.Magnitude() + lv1b.Magnitude()) ) )
				la = true;
			//if( Math.Abs ( l.Magnitude() - (lv2a.Magnitude() + lv2b.Magnitude()) ) < 0.01f )
			if( floatCompare( l.Magnitude(), (lv2a.Magnitude() + lv2b.Magnitude()) ) )
				lb = true;
			if( la && lb )
				return true;
		}
		return false;
	}

	//General collision check for line formed with verties of obstacles or map
	//Cameras only at vertices so only endpoint to endpoint intersection need to be ignored
	//Unless the intersection point is a corner
	public bool collisionGeneral( Line tmpLine, int endpt, int fid ){
		bool collides = false;
		//1.Diagonal Check
		foreach( Geometry g in finalPoly ){
			if( g.PointInside( tmpLine.MidPoint() ) ){
				collides = true;
				return collides;
			}
		}
		//2.Edge intersection check
		foreach( Line l in totalGeo.edges ){
			if( endpt == 0 ){
				if( l.LineIntersectMuntac(tmpLine) == 1 ){
					//3.Ignore colinear intersecting points i.e. corners
					Vector3 intpoint = l.GetIntersectionPoint(tmpLine);
					if( l.isEndPoint( intpoint ) || tmpLine.isEndPoint( intpoint ) )
						continue;
					if( colinear( l, tmpLine ) )
						continue;
					collides = true;
					return collides;
				}
			}
			else{
				if( l.LineIntersectMuntacEndPt(tmpLine) == 1 ){
					//3.Ignore colinear intersecting points
					Vector3 intpoint = l.GetIntersectionPoint(tmpLine);
					if( colinear( l, tmpLine ) )
						continue;
					collides = true;
					return collides;
				}
			}
		}
		//Check if outside map
		if (!mapBG.PointInside (tmpLine.MidPoint ())) {
			foreach( Line l in mapBG.edges ){
				if( floatCompare( new Line( l.vertex[0], tmpLine.MidPoint() ).Magnitude()
				   + new Line( l.vertex[1], tmpLine.MidPoint() ).Magnitude(), l.Magnitude() ) )
					return false;
			}
			collides = true;
//			if( fid == 5 ){
//				Debug.Log("3Foreign id is" + fid.ToString() );
//				//mapBG.DrawGeometry(GameObject.Find("temp"));
//			}
		}
		return collides;
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

	public bool VectorApprox ( Vector3 a, Vector3 b, int debug ){
		if (Math.Abs (a.x - b.x) < eps && Math.Abs (a.z - b.z) < eps)
			return true;
		else {
			//Debug.Log ( Math.Abs (a.x - b.x) + " " + Math.Abs (a.z - b.z) + " " + eps );
			//Debug.Log ( Math.Abs (a.x - b.x) );
			Debug.Log (Math.Abs (a.z));
			Debug.Log (Math.Abs (b.z));
			Debug.Log (eps );
			return false;
		}
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

	private bool colinear( Line A, Line B ){
		Line a1 = new Line (A.vertex [0], B.vertex [0]);
		Line a2 = new Line (A.vertex [1], B.vertex [1]);
		if (floatCompare (a1.Magnitude () + B.Magnitude () + a2.Magnitude(), A.Magnitude ()))
			return true;
		a1 = new Line (A.vertex [0], B.vertex [1]);
		a2 = new Line (A.vertex [1], B.vertex [0]);
		if (floatCompare (a1.Magnitude () + B.Magnitude () + a2.Magnitude(), A.Magnitude ()))
			return true;
		return false;
	}
}//End of Class