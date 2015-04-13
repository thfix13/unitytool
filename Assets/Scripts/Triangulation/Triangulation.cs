using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
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
	public Geometry totalGeo = new Geometry (); //has both map and merged polygons
	private List<Vector3> totalGeoVerts = new List<Vector3>();
	//Contains Map
	public Geometry mapBG;
	private float mapBGPerimeter = 0f;
	//Camera
	public List<KeyValuePair<Vector3,Geometry>> cameraVPS = new List<KeyValuePair<Vector3, Geometry>>();
	public List<KeyValuePair<Vector3,Geometry>> cameraVPS2 = new List<KeyValuePair<Vector3, Geometry>>();
	public List<KeyValuePair<Vector3,Geometry>> cameraUnion = new List<KeyValuePair<Vector3, Geometry>>();
	//Contains all the visibility polygons for cameras
	public int vpnum = -1;
	
	public bool drawTriangles = false;
	public bool drawSPRoadMap = false;
	public bool drawMinSpanTree = false;
	public bool drawMapBoundary = false;
	public bool drawObstacles = false;
	public bool drawCameras = false;
	public bool drawVP = false;
	public bool drawVPUnion = false;
	public bool drawTour = false;
	public bool stopAll = false;
	private bool geometryDrawn = false;
	private bool obstacleDrawn = false;
	private bool sprDrawn = false;
	
	List<Vector3> masterReflex = new List<Vector3>();
	
	//Tour
	private List<Line> spRoadMap = new List<Line> ();
	private List<Vector3> explorationTour = new List<Vector3> ();
	private Geometry tourEdges = new Geometry ();
	//Dijkstra
	List<edges>[] EL = new List<edges>[10000];
	//Stores shortest path calculations with node i as source on all nodes j
	float [,] d = new float [500, 500];
	//Stores the path for above calculation
	int [,] parents = new int [500, 500];
	
	public void Start(){
		this.Clear ();
	}
	
	public void Clear(){
		points.Clear();
		colours.Clear();
		cameras.Clear ();
		triangles.Clear ();
		lines.Clear ();
		obsGeos.Clear ();
		mapBoundary = new Vector3[4];
		totalGeo = new Geometry ();
		mapBG = new Geometry ();
		finalPoly.Clear ();
		triangles.Clear(); 
		lines.Clear(); 
		geos.Clear();
		linesMinSpanTree.Clear();
		obsGeos.Clear ();
		masterReflex.Clear ();
		spRoadMap.Clear ();
		GameObject temp = GameObject.Find("temp");
		DestroyImmediate(temp);
		GameObject vptmp = GameObject.Find("vptmp");
		DestroyImmediate(vptmp);
		geometryDrawn = false;
		obstacleDrawn = false;
		sprDrawn = false;
		stopAll = true;
		explorationTour.Clear ();
		cameraVPS.Clear ();
		cameraVPS2.Clear ();
		cameraUnion.Clear ();
		GameObject vpa = GameObject.Find("vpA");
		if( vpa != null )
			DestroyImmediate(vpa);
		vpa = GameObject.Find("vpB");
		if( vpa != null )
			DestroyImmediate(vpa);
		vpa = GameObject.Find("vpMerged");
		if( vpa != null )
			DestroyImmediate(vpa);
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
		
		if (drawMapBoundary && !geometryDrawn) {
			//totalGeo.DrawGeometry(GameObject.Find("temp"));
			mapBG.DrawGeometry(GameObject.Find("temp"));
			geometryDrawn = true;
		}
		
		if (drawObstacles && !obstacleDrawn ) {
			foreach( Geometry g in finalPoly )
				g.DrawGeometry( GameObject.Find("temp") );
			obstacleDrawn = true;
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
		
		if( drawVPUnion && vpnum != -1 && cameraUnion.Count > 0 ){
			Line tmpline = new Line (cameraUnion[vpnum].Key, new Vector3(-100,1,100));
			tmpline.DrawLine(Color.white);
			foreach( Line l in cameraUnion[vpnum].Value.edges )
				l.DrawLine( Color.blue );
		}
		
		if (drawTour) {
			drawTour = false;
			for( int i = 0; i < explorationTour.Count - 1; i++ ){
				if( i == 0 )
					drawSphere( explorationTour[i], Color.red );
				else if ( i == explorationTour.Count - 2 )
					drawSphere( explorationTour[i + 1], Color.green );
				Line tmpline = new Line( explorationTour[i], explorationTour[i + 1] );
				tmpline.DrawVector( GameObject.Find("temp"), Color.blue );
				//Debug.DrawLine( explorationTour[i], explorationTour[i + 1] );
			}
		}
		if( drawTriangles ){	
			foreach(Triangle tt in triangles)
				tt.DrawDebug();
		}
	}

	public void TriangulationSpace (){
		this.Clear ();
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
		if (!makeTourOnSPR ()){
			Debug.Log("makeTourOnSPR () failed");
			return;
		}

		/*STEP 9 - CREATE TOUR USING NEAREST NEIGHBOUR*/
		//happens in makeTourOnSPR

		/***PHASE D: VISIBILITY***/
		/*STEP 10 - GET VISIBILITY POLYGONS OF CAMERAS*/
		getCameraVPS ();
		VPValidation ();
		/*STEP 11 - CHECK CAMERA VISIBILITY NESTING*/
		mergeVPS ();
		return;
		//return;
		/***PHASE E: MORE METRICS***/
		/*STEP 12 - CHECK INCREMENTAL CAMERA AREA COVERAGE*/
		areaCoverage ();
		/*STEP 13 - CHECK SUBTRACTIVE COVERAGE*/
		//subtractiveCoverage ();
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

		//If not obstacles return
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
					Geometry tmpG = obsGeos[i].GeometryMerge( obsGeos[j], 0 );
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
			if( mapBG.GeometryIntersect( g ) && !mapBG.GeometryInside( g, false ) ){
				mapBG = mapBG.GeometryMergeInner( g );
				mapBG.BoundGeometry( mapBoundary );
			}
			else
				finalPoly.Add(g);
		}
		//TODO:Check if mapBG has any disjoint parts
		//mapBG.getSortedEdges ();
		foreach( Line l in mapBG.edges )
			mapBGPerimeter += l.Magnitude();
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
		totalGeoVerts = totalGeo.GetVertex ();
		int vlcnt = 0;
		lines.Clear ();
		//Constructing "lines" for triangulation
		//First add lines that are in MST
		foreach (Line l in linesMinSpanTree)
			lines.Add (l);
		foreach (Vector3 Va in allVertex){
			foreach(Vector3 Vb in allVertex){
				if( Va != Vb ){
					bool collides = false, essential = false;
					Line tempLine = new Line(Va, Vb);
					
					//A-Collision with existing triangulation lines
					foreach( Line l in lines ){
						if( l.LineIntersectMuntacEndPt( tempLine ) == 1 || l.Equals(tempLine) ){
							collides = true;
							break;
						}
					}
					
					if( collides ) continue;
					
					//B-Collision with obstacles and maps
					collides = comprehensiveCollision( tempLine, 0 );
					
					//Add Line
					if( !collides ){
						lines.Add( tempLine );
						if( vlcnt == 92 ){
							//Debug.Log( comprehensiveCollision( tempLine, 92 ) );
							int xid = 0;
//							foreach( Geometry g in finalPoly )
//								g.DrawGeometry(new GameObject( xid++.ToString() ) );
						}
						tempLine.name = "Line"+vlcnt++.ToString();
						//tempLine.DrawVector(GameObject.Find("temp"));
					}
				}
			}
		}
		vlcnt = 0;
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
		//1. Get all reflex vertices from obstacles
		masterReflex = new List<Vector3> ();
		foreach (Geometry g in finalPoly){
			List<Vector3> lv = new List<Vector3>();
			lv = g.GetReflexVertex();
			foreach( Vector3 v in lv )
				masterReflex.Add( v );
		}
		
		List<Vector3> lv2 = new List<Vector3>();
		//2. Get reflex vertices(interior) of map
		lv2 = mapBG.GetReflexVertexComplement();
		foreach( Vector3 v in lv2 )
			masterReflex.Add( v );
		
		spRoadMap = new List<Line> ();
		//3. Connect all reflex vertices
		foreach (Vector3 vA in masterReflex) {
			//drawSphere( vA, Color.blue, cnt++);
			foreach (Vector3 vB in masterReflex) {
				if( vA == vB || VectorApprox( vA, vB ) ) continue;
				Line tmpLine = new Line( vA, vB );
				if( spRoadMap.Contains( tmpLine ) ) continue;
				bool added = false;
				//3A. Add link if already an obstacle edge
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
				//3B. Check for regular collisions
				bool collides = comprehensiveCollision(tmpLine, 0);
				if( collides )	continue;
				//3C. Checking for extendability
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
				//foreach( Geometry g in geos ){
				foreach( Geometry g in finalPoly ){
					//Note: There maybe a case where after extending the point is not inside a geometry
					//but the line is inside one
					if( g.PointInside(vA_new) || g.PointInside(vB_new) || g.PointInside(extLine.MidPoint()) ){
						collides = true;
						break;
					}
				}
				if( mapBG.PointOutside(vA_new) || mapBG.PointOutside(vB_new) || mapBG.PointOutside(extLine.MidPoint()) ){
					collides = true;
					break;
				}
				foreach( Line l in totalGeo.edges ){
					if( l.LineIntersectMuntacEndPt(tmpLine) == 1 ){
						collides = true;
						break;
					}
				}
				if( !collides ){
					spRoadMap.Add(tmpLine);
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
	
	bool makeTourOnSPR(){
		Debug.Log ("Total Cameras: " + cameras.Count);
		Debug.Log ("SPRoadMap: " + spRoadMap.Count);
		if (cameras.Count < 2)	return true;
		Dictionary<Vector3, int> dict = new Dictionary<Vector3, int> ();
		Dictionary<int, Vector3> numToVect = new Dictionary<int, Vector3> ();
		int N = 0;
		//1. Construct graph in EL (global variable, edge list)
		//1A. give IDs to vertices (masterReflex (without cameras) and cameras independently)
		foreach (Vector3 v in masterReflex){
			if( cameras.Contains(v) ) continue;
			EL[N] = new List<edges>();
			dict.Add( v, N );
			numToVect.Add(N, v);
			N++;
		}
		int GSC = N; //graphSizeSansCameras
		foreach (Vector3 v in cameras) {
			if( !dict.ContainsKey( v ) ){
				EL[N] = new List<edges>();
				dict.Add( v, N );
				numToVect.Add(N, v);
				N++;
			}
		}
		if (N > 490) {
			Debug.Log("Too many nodes in SPRoadMap");
			return false;
		}
		//1B. Construct edges with these new ids
		foreach (Line l in spRoadMap) {
			int u = dict[l.vertex[0]];
			int v = dict[l.vertex[1]];
			EL[u].Add(new edges( v, l.Magnitude() ));
			EL[v].Add(new edges( u, l.Magnitude() ));
		}
		
		/*2. CAMERA WORK*/
		//2A. Connect cameras to each other and the rest of the graph (i.e. reflex points)
		List<Vector3> masterReflexAndCamera = new List<Vector3> ();
		foreach (Vector3 v1 in cameras)
			masterReflexAndCamera.Add (v1);
		foreach (Vector3 v1 in masterReflex){
			if( !cameras.Contains(v1) ) masterReflexAndCamera.Add (v1);
		}
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
				collides = comprehensiveCollision( tmpLine, 0 );
				if( !collides ){
					EL[u].Add(new edges( v, tmpLine.Magnitude() ) );
					EL[v].Add(new edges( u, tmpLine.Magnitude() ) );
					//Line tmptmp = new Line( v1, v2 );
					//tmptmp.DrawVector( GameObject.Find("temp"), Color.red );
				}
			}
		}
		
		/*3. DIJKSTRA*/
		//3A. Calculate All-Pair-Shortest-Path
		for( int i = 0; i < N; i++ )
			Dijkstra( i, N );
		for( int i = 0; i < N; i++ ){
			for( int j = 0; j < N; j++ ){
				//Debug.Log(i + "," + j + ":" + parents[i,j]);
				if( i!=j && j == parents[i,j] )
					Debug.Log("Discrepancy");
			}
		}
		//0 to (GSC - 1) - Draws graph
//		for (int i = 0; i < N; i++) {
//			foreach( edges l in EL[i] ){
//				Line tmpline = new Line( numToVect[i], numToVect[l.v] );
//				tmpline.DrawVector( GameObject.Find("temp") );
//			}
//		}

		/*4. MAKE TOUR*/
		bool [] visited = new bool [300];
		for( int i = 0; i < N; i++ )
			visited[i] = false;
		Debug.Log ("GSC : " + GSC + " N: " + N);
		int current = GSC;
		float tourDistance = 0;
		List< int > tour = new List<int> ();
		tour.Add (GSC);
		int xcnt = 0;
//		for( int i = GSC; i < N; i++ ){
//			if( !visited[i] )
//				xcnt++;
//		}
//		Debug.Log ("Not visited yet: " + xcnt);
//		xcnt = 0;
		//4A. Choose any camera as starting point
		while( true ){
			visited[current] = true;
			xcnt++;
			float mindist = 10000f;
			int nearestNeighbor = -1;
			//4B. Find the camera closest to it in the EL graph
			for( int i = 0; i < N; i++ ){
				if( i == current ) continue;
				if( !cameras.Contains(numToVect[i]) ) continue; //if this is not a camera
				if( visited[i] ) continue;
				if( d[current, i] < mindist ){
					nearestNeighbor = i;
					mindist = d[current, i];
				}
			}
			if( nearestNeighbor == -1 ){
				break;
			}
			//4C. Determine the path to the chosen camera
			int src = current;
			int dest = nearestNeighbor;
			Stack<int> stk = new Stack<int> ();
			//Debug.Log( src + " " + GSC + " " + N);
			while( src != dest ){
				stk.Push(dest);
				dest = parents[current,dest];
				xcnt++;
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
		//Debug.Log ("Exp tour" + explorationTour.Count);
		for (int i = 0; i < tour.Count - 1; i++) {
			Vector3 v = numToVect[tour[i]];
			explorationTour.Add(v);
		}
		explorationTour.Add (numToVect[tour[tour.Count - 1]]);
//		Debug.Log ("Exp tour after" + explorationTour.Count);
//		Debug.Log ("Tour size: " + tour.Count);
		return true;
	}
	
	private void Dijkstra( int id, int N ){
		SortedDictionary< float, int > SD = new SortedDictionary< float, int > ();
		//1. Initialization
		for (int i = 0; i < N; i++) {
			d [id, i] = 100000f;
			parents[id, i] = -1;
		}
		d[id, id] = 0f;
		parents [id, id] = id;
		SD.Add( 0f, id );
		//2. Take first node (least cost) from SD and update connected nodes
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
					//TODO:Implement a more permanent solution. Multiple keys for a sorted dictionary.
					while( SD.ContainsKey(nw) )
						nw += eps;
					SD.Add( nw, v );
					parents[ id, v ] = u;
				}
			}
		}
		for (int i = 0; i <= N; i++) {
			if( parents[id, i] == -1 )
				Debug.Log("All nodes not reached:" + i + " for " + id + " tour");
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
		//inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
		inter.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
		inter.transform.parent = temp.transform;
		//inter.gameObject.name = vlcnt.ToString();
	}
	
	void drawSphere( Vector3 v, Color x, int vlcnt ){
		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.transform.renderer.material.color = x;
		inter.transform.position = v;
		//inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
		//inter.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
		inter.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
		inter.transform.parent = temp.transform;
		inter.gameObject.name = vlcnt.ToString();
	}
	
	void drawSphere( Vector3 v, Color x, float vlcnt ){
		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.transform.renderer.material.color = x;
		inter.transform.position = v;
		//inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
		//inter.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
		inter.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
		inter.transform.parent = temp.transform;
		inter.gameObject.name = vlcnt.ToString();
	}
	
	Geometry visibilityPolygon( Vector3 kernel, int xid ){
		List<Vector3> allpoints = totalGeo.GetVertex ();
		List<Vector3> vp = new List<Vector3> ();
		int cnt = 0;
		//Step 1 - Find all points that are initially visible
		foreach (Vector3 v in allpoints){
			cnt++;
			Line tmpLine = new Line( kernel, v );
			if( !comprehensiveCollision( tmpLine, 0 ) ){
				vp.Add ( v );
			}
		}
		if (vp.Count == 0)
			return new Geometry ();
		//Step 2- Sort the points by angle
		Geometry x = new Geometry ();
		List<KeyValuePair<Vector3, float>> anglist = x.GetVertexAngleSorted (kernel, vp);
		if (xid == 16) {
//			drawSphere( kernel, Color.blue);
//			Line kernelline = new Line (kernel, new Vector3 (-100, 1, 100));
//			kernelline.name = "Line Outpoint";
//			kernelline.DrawVector (GameObject.Find ("temp"));
		}
		cnt = 0;
		foreach(KeyValuePair<Vector3, float> kvp in anglist) {
			if( xid == 0 ){
//				drawSphere( kvp.Key, Color.red, kvp.Value );
//				Line tmpline = new Line( kernel, kvp.Key );
//				tmpline.name = "Line" + cnt + " " + kvp.Value;
//				tmpline.DrawVector(GameObject.Find("temp"), Color.cyan);
			}
		}

		//Step 3 - Extend where applicable and find rest of the points
		List<KeyValuePair<Vector3, float>> anglistExt = new List<KeyValuePair<Vector3, float>> ();
		List<Vector3> uniqueList = new List<Vector3> ();
		cnt = -1;
		for( int i = 0; i < anglist.Count; i++ ){
			cnt++;
			Vector3 v = anglist[i].Key;
			float angle = anglist[i].Value;
			List<Vector3> tmplist = new List<Vector3>();
			if( v == kernel )
				continue;
			Vector3 nv = v;
			Vector3 prevv = kernel;
			
			KeyValuePair<bool, Vector3> mrpts = new KeyValuePair<bool, Vector3>(true, nv);
			int depth = 0;
			while( i != anglist.Count - 1 && floatCompare( anglist[i].Value, anglist[i + 1].Value ) ){
			//while( i != anglist.Count - 1 && Math.Abs( anglist[i].Value - anglist[i + 1].Value ) < 0.001 ){
				tmplist.Add( anglist[i].Key );
				anglistExt.Add( new KeyValuePair<Vector3, float>( anglist[i].Key, anglist[i].Value ));
				prevv = nv;
				nv = anglist[i + 1].Key;
				i++;
			}
			while( mrpts.Key ){
				++depth;
				anglistExt.Add( new KeyValuePair<Vector3, float>( nv, angle ));
				tmplist.Add( nv );
				mrpts = morePointsDebug(prevv, nv, xid, cnt);
				prevv = nv;
				nv = mrpts.Value;
			}
//			if( xid == 28 && cnt == 3 ){
//				int xx = 0;
//				prevv = kernel;
//				foreach( Vector3 vvv in tmplist ){
//					//drawSphere(vvv);
//					//if( xx == 1 )
//					morePointsDebug(prevv, vvv, -100, 0);
//					prevv = vvv;
//					xx++;
//				}
//			}
		}
		cnt = 0;
//		foreach (KeyValuePair<Vector3, float> kvp in anglistExt) {
//			if( xid == 36 )
//				drawSphere( kvp.Key, Color.blue, cnt++ );
//		}
		cnt = 0;
		//Step 4 - Order the points
		List<Vector3> vispol = new List<Vector3>();
		string nextstartingpoint = "none";
		cnt = 0;
		for( int i = 0; i < anglistExt.Count; i++ ){
			Vector3 vx = anglistExt[i].Key;
			if( vx == kernel )
				continue;
			cnt++;
			bool printsome = false;
			List<Vector3> templistA = new List<Vector3>();
			List<Vector3> templistB = new List<Vector3>();
			int indA, indB;
			float angleA = anglistExt[i].Value;
			for( indA = i; indA < anglistExt.Count && floatCompare(anglistExt[indA].Value, angleA); indA++, i++ ){
				templistA.Add( anglistExt[indA].Key );
			}

			if( indA == anglistExt.Count ){
				if( nextstartingpoint == "last" )
					templistA.Reverse();
				foreach( Vector3 v in templistA )
					vispol.Add( v );
				break;
			}
			float angleB = anglistExt[i].Value;
			for( indB = i; indB < anglistExt.Count && floatCompare(anglistExt[indB].Value, angleB); indB++ ){
				templistB.Add( anglistExt[indB].Key );
			}
			if( xid == 13 ){
				if( cnt == 14 ){
//					foreach( Vector3 vvv in templistA )
//						drawSphere(vvv, Color.blue);
//					foreach( Vector3 vvv in templistB )
//						drawSphere(vvv, Color.red);
//					Debug.Log(angleA + "," + angleB + "," + (angleB - angleA) );
				}		
			}
			i = indA - 1;//since it'll be incremented after the end of the loop
			indA = templistA.Count - 1;
			indB = templistB.Count - 1;
			bool connected = false;
			bool addkernel = false;
			if( angleB - angleA > Math.PI ){
				//Case 0: Reflex angle i.e. FP to kernel
				nextstartingpoint = "first";
				templistA.Reverse();
				addkernel = true;
			}
			else{
				bool LL = connectable( templistA[indA], templistB[indB] );
				bool LF = connectable( templistA[indA], templistB[0] );
				bool FL = connectable( templistA[0], templistB[indB] );
				bool FF = connectable( templistA[0], templistB[0] );
				if( xid == 13 && cnt == 14 ){
//					Debug.Log( LL + " " + LF + " " + FL + " " + FF );
//					drawSphere( templistA[indA] );
//					drawSphere( templistB[indB] );
//					comprehensiveCollision( new Line( templistA[indA], templistB[indB] ), 100 );
				}
				if( LL && LF && FL && FF  ){
					//Case 1: LP to LP
					nextstartingpoint = "last";
				}
				else if( FF ){
					//Case 2: Farthest connectable points
					int connectorA = -1;
					int connectorB = -1;

					if( nextstartingpoint != "none" ){
						if( nextstartingpoint == "first" ) connectorA = indA;
						else{
							connectorA = 0;
							//templistA.Reverse();
						}
					}
					else{
						//If A[last] can reach B[first] then all A can reach B[first]
						if( LF ) connectorA = indA;
						else{
							for( int j = indA; j >= 0; j-- ){
								if( connectable( templistA[j], templistB[0] ) ){
									connectorA = j;
									break;
								}
							}
						}
					}
					if( xid == 13 && cnt == 14 ){
//						drawSphere(templistA[2], Color.blue );
//						drawSphere(templistB[0], Color.red );
//						Line lkj = new Line( templistA[2], templistB[0] );
//						lkj.DrawVector(GameObject.Find("temp"));
//						Debug.Log("Conn " + connectorA);
//						Debug.Log(LF);
						//Debug.Log(connectable(templistA[
					}
					//If A[last] can reach B[j] then all above A[last] can reach B[j] and all above B[j]
					for( int j = indB; j >= 0; j-- ){
						if( connectable( templistA[connectorA], templistB[j] ) ){
							connectorB = j;
							break;
						}
					}
					//If connector not the first or last point then delete all after it
					if( connectorA != 0 && connectorA < indA )
						templistA.RemoveRange( connectorA + 1, indA - (connectorA + 1) + 1);
					if( connectorB != 0 && connectorB < indB )
						templistB.RemoveRange( connectorB + 1, indB - (connectorB + 1) + 1);
					//If connector not the first point then reverse
					if( connectorA == 0 )
						templistA.Reverse();
					//Set marker for next one
					if( connectorB == 0 ) nextstartingpoint = "first";
					else nextstartingpoint = "last";
				}
				else{
					//if unconnectable, connect to kernel
					templistA.Reverse();
					addkernel = true;
					nextstartingpoint = "first";
				}
			}
			foreach( Vector3 v in templistA )
				vispol.Add( v );
			if( addkernel )
				vispol.Add( kernel );
		}
		
		//If kernel hasn't been added yet
		if (!vispol.Contains (kernel)) {
			vispol.Add (kernel);
			//drawSphere( kernel, Color.red, -1 );
		}
		//Special case where first ray has anomalous extensions
		for (int i = 0; i < vispol.Count - 1; i++) {
			if( connectable( vispol[vispol.Count - 1], vispol[i] ) ){
				if( i != 0 ){
					vispol.RemoveRange( 0, i );
				}
				break;
			}
		}
		cnt = 0;
		foreach (Vector3 v in vispol) {
			if( xid == 0 ){
//				drawSphere( v, Color.cyan, cnt++ );
//				Line tmpline = new Line( kernel, v );			
//				tmpline.name = "Line" + cnt.ToString();
//				tmpline.DrawVector(GameObject.Find("temp"), Color.yellow);
			}
			//drawSphere( v );
		}
		//return new Geometry();
		cnt = 0;
		Geometry VPret = new Geometry ();
		for( int i = 0; i < vispol.Count; i++ ){
			Line tmpline = new Line( kernel, vispol[i] );
			//if( xid == 0 ) drawSphere( vispol[i], Color.red, i ); 
			if( i == vispol.Count - 1 ){
				tmpline = new Line( vispol[i], vispol[0] );
				//tmpline = null;
			}
			else{
				tmpline = new Line( vispol[i], vispol[i + 1] );
			}
			tmpline.name = "Line" + cnt++;
			VPret.edges.Add(tmpline);
			//			if( xid == 0 )
			//				tmpline.DrawVector(GameObject.Find("temp"), Color.cyan);
		}
		foreach (Line l in VPret.edges) {
			if( VectorApprox( l.vertex[0], l.vertex[1] ) )
				Debug.Log("GETTING THE SAME");
		}
		return VPret;
	}

	bool connectable( Vector3 A, Vector3 B ){
		Line tmpline = new Line (A, B);
		return !comprehensiveCollision( tmpline, 0 );
	}
	
	KeyValuePair<bool, Vector3> morePointsDebug( Vector3 vA, Vector3 vB, int xid, int yid ){
		if( !totalGeoVerts.Contains (vB) )
			return new KeyValuePair<bool, Vector3> (false, new Vector3());
		//1. Extend Line
		Vector2 vA2d = new Vector2( vA.x, vA.z );
		Vector2 vB2d = new Vector2( vB.x, vB.z );
		Line tmpline = new Line (vA, vB);
		Vector2 dirA2d = vB2d - vA2d;//Direction from A towards B
		float alp = 1.01f;
		Vector2 vB_new2d = vA2d + (alp * dirA2d);
		//if (xid == -200) {
		float extensionfactor = 0.1f;
		if (tmpline.Magnitude () < extensionfactor)
			extensionfactor = tmpline.Magnitude();

		vB_new2d.x = vB2d.x + dirA2d.x / tmpline.Magnitude() * extensionfactor;
		vB_new2d.y = vB2d.y + dirA2d.y / tmpline.Magnitude() * extensionfactor;
		//}
		Vector3 vB_new = new Vector3( vB_new2d.x, 1, vB_new2d.y );
		bool collides = false;
		//2. Check if new endpoint is inside a geometry
		if( xid == -100 ){
			//tmpline.DrawVector(GameObject.Find("temp"));
			drawSphere (vB, Color.magenta, 1);
			drawSphere (vB_new, Color.green, 2);
			Debug.Log( mapBG.PointOutsideDebug(vB_new) );
			Debug.Log("Mag Orig: " + tmpline.Magnitude());
			tmpline = new Line (vB, vB_new);
			Debug.Log("Mag: " + tmpline.Magnitude());
		}
		tmpline = new Line (vB, vB_new);

		int cnt = 0;
		foreach( Geometry g in finalPoly ){
//			if( i == -100 )
//				g.DrawGeometry(new GameObject(cnt++.ToString()));
			if( g.PointInside(vB_new) || g.PointInside(tmpline.MidPoint())){ 
				collides = true;
				break;
			}
		}
		
		//3. Check if new endpoint is inside the map	
		if (!collides){
			if( xid == -99 ){
				mapBG.PointOutsideDebug(vB_new);
				collides = true;
			}
			else if( mapBG.PointOutside(vB_new) || mapBG.PointOutside(tmpline.MidPoint()) )
				collides = true;
		}
		if (collides) return new KeyValuePair<bool, Vector3> (false, new Vector3());
		
		//3. Consider ray from vB to vB_new
		//3A. Extend ray to the length of the map's perimeter and find closest intersection point
		Vector2 dirB2d = vB_new2d - vB2d;//Direction from B towards B_new
		vB_new2d.x = vB2d.x + dirB2d.x / tmpline.Magnitude() * mapBGPerimeter;
		vB_new2d.y = vB2d.y + dirB2d.y / tmpline.Magnitude() * mapBGPerimeter;

		vB_new = new Vector3( vB_new2d.x, 1, vB_new2d.y );
		Line ray = new Line (vB, vB_new);
		collides = false;		
		//4. Find the closest occuring intersection with the ray
		//New line is from vB to vB_new as involving vA would bring back noted intersection points (i.e. vB)
		float mindist = mapBGPerimeter;
		if (xid == -100){
//			drawSphere (vB_new, Color.red, 3);
//			ray.DrawVector (GameObject.Find ("temp"));
//			cnt = 0;
//			foreach(Line l1 in totalGeo.edges){
//				l1.name = "Line"+cnt++.ToString();
//				l1.DrawVector(GameObject.Find("temp"),Color.green);
//			}
		}
		Vector3 retvect = new Vector3 ();
		cnt = 0;
		foreach(Line l1 in totalGeo.edges){
			//TODO:FIGURE OUT
			//if( l1.LineIntersectMuntac(ray) == 1 ){
			if( xid == -100 && cnt == 36 ){
//				l1.DrawVector(GameObject.Find("temp"));
//				Debug.Log("WT:" + l1.LineIntersectMuntacEndPtDebug(ray));
//				Debug.Log("WT:" + l1.LineIntersectRegular(ray));
			}
			if( l1.LineIntersectMuntacEndPt(ray) == 1 ){
				Vector3 v = l1.GetIntersectionPoint(ray);
				if( VectorApprox( v, vB ) )	continue;
				Line vamp = new Line( vB, v );
				if( vamp.Magnitude() < mindist ){
					mindist = vamp.Magnitude();
					retvect = v;
				}
				collides = true;
			}
			cnt++;
		}

		if (!collides ){
			Debug.Log ("No collision in morePoints " + xid.ToString() + " " + yid.ToString());
			//ray.DrawVector(GameObject.Find("temp"));
			//drawSphere( vB_new, Color.green, 2 );
			return new KeyValuePair<bool, Vector3> (false, new Vector3());
		}
		//Final collision check as precision issues sometimes allows
		//lines inside an obstacle
		if( comprehensiveCollision( new Line( vB, retvect ), 0 ) )
			return new KeyValuePair<bool, Vector3> (false, new Vector3());
		else
			return new KeyValuePair< bool, Vector3 >(true, retvect);
	}

	void getCameraVPS(){
		List<Vector3> tempcam = new List<Vector3> ();
		//Debug.Log ("Camera count is " + cameras.Count);
		int xid = 0;
		foreach (Vector3 v in explorationTour) {
			if( cameras.Contains(v) && !tempcam.Contains(v) ){
				//if( !tempcam.Contains(v) ){
				tempcam.Add(v);
				//if( xid == 4 ) break;
				cameraVPS.Add ( new KeyValuePair<Vector3, Geometry>( v, visibilityPolygon( v, xid ) ) );
				xid++;
				//return;
			}
		}
		//Debug.Log ("Temp cam count is " + tempcam.Count);
	}

	void VPValidation(){
		int cnt = 0;
		foreach (KeyValuePair<Vector3, Geometry> kvp in cameraVPS) {
			foreach( Line l in kvp.Value.edges ){
				if( comprehensiveCollision( l , 0 ) ){
					//if( cnt == 0 ) l.DrawVector(GameObject.Find("temp"));
					Debug.Log("Invalid VP at: " + cnt);
					break;
				}
			}
			cnt++;
		}
	}

	void mergeVPS(){
//		new GameObject ("vpA");
//		new GameObject ("vpB");
//		new GameObject ("vpMerged");
	
		int x = cameraVPS.Count;
		Geometry incrementalCover = new Geometry ();
		incrementalCover = cameraVPS [0].Value;
		cameraVPS2.Add( new KeyValuePair<Vector3, Geometry>( cameraVPS[0].Key, cameraVPS[0].Value ) );

		int cnt = 0;
		Debug.Log ("Total Cameras:" + x);
		for (int i = 1; i < x; i++) {
			//if( i == 4) return;
			if( incrementalCover.GeometryInside(cameraVPS[i].Value, false) )
				cnt++;
			if( i == 9 ){
//				incrementalCover.DrawGeometry(GameObject.Find("vpA"));
//				cameraVPS[i].Value.DrawGeometry(GameObject.Find("vpB"));
//				for( int j = 0; j < incrementalCover.edges.Count; j++ ){
//					for( int k = j + 1; k < incrementalCover.edges.Count; k++ ){
//						if( incrementalCover.edges[j].Equals(incrementalCover.edges[k]) )
//							Debug.Log("SAMENESSLINE");
//					}
//				}
//				for( int j = 0; j < cameraVPS[i].Value.edges.Count; j++ ){
//					for( int k = j + 1; k < cameraVPS[i].Value.edges.Count; k++ ){
//						if( cameraVPS[i].Value.edges[j].Equals(cameraVPS[i].Value.edges[k]) )
//							Debug.Log("SAMENESSLINEB");
//					}
//				}
//				foreach( Line ll in cameraVPS[i].Value.edges ){
//					if( VectorApprox( ll.vertex[0], ll.vertex[1] ) )
//						Debug.Log("SAMENESSB");
//				}
			}
			incrementalCover = incrementalCover.GeometryMerge(cameraVPS[i].Value, i);

			//if( i == 1 ) return;
			//if( i == x - 1 ) return;

			cameraVPS2.Add( new KeyValuePair<Vector3, Geometry>( cameraVPS[i].Key, incrementalCover ) );
//			if( i == 3 )
//				incrementalCover.DrawGeometry(GameObject.Find("vpMerged"));
			//if( i == 15 ) break;

		}
		//incrementalCover.DrawGeometry (GameObject.Find ("temp"));
		//cameraVPS.Clear ();

		foreach( KeyValuePair<Vector3, Geometry> kvp in cameraVPS2 ){
			cameraUnion.Add(kvp);
			//cameraVPS.Add(kvp);
		}
		Debug.Log("Number of Overlaps: " + cnt);
		return;
	}
	
	void areaCoverage(){
		List<double> coverageAreas = new List<double> ();
		int xid = 0;
		Geometry gA = new Geometry ();
		Geometry gB = new Geometry ();
		foreach( KeyValuePair<Vector3, Geometry> kvp in cameraUnion ){
			Geometry g = kvp.Value;
			//g.DrawGeometry(GameObject.Find("temp"));
			double area = g.getPolygonArea(xid++);
			coverageAreas.Add( area );
			//if( xid == 8 ) return;
//			if( xid == 5 )
//				gA = g;			
//			if( xid == 6 )
//				gB = g;
//			Debug.Log (area + " " + xid++ + " " + g.edges.Count);
//			if( xid == 15 ){
//				//g.DrawGeometry(GameObject.Find("vpA"));
//			}
			//xid++;
			//break;
		}
		xid = 0;
		string createText = "";
		string path = @"C:\Users\Asus\Desktop\McGill\Thesis\Week 15\area.csv";
		string delimeter = ",";
		//var properties = new object[];
		createText += "Point,Area,\n";
		for( int i = 0; i < coverageAreas.Count; i++ ){
			//Debug.Log (f + " " + xid++);
			//createText += f.ToString() + Environment.NewLine;
//			if( i == coverageAreas.Count - 1 )
//				createText += coverageAreas[i].ToString();
//			else
				//createText += "\""+i.ToString()+","+coverageAreas[i].ToString() + "\",\n";
			createText += (i+1).ToString()+","+coverageAreas[i].ToString() + ",\n";
			//File.WriteAllText(path, createText);
		}
		File.WriteAllText(path, createText);
	}
	
	public void subtractiveCoverage(){
		//public List<KeyValuePair<Vector3,Geometry>> tempUnion = new List<KeyValuePair<Vector3, Geometry>>();
		int x = cameraVPS.Count;
		double mapArea = cameraUnion [x - 1].Value.getPolygonArea (0);
		int cnt = 0;
		for( int j = 1; j < x; j++ ) {
			Geometry tempUnion = new Geometry ();
			foreach( Line l in cameraVPS[0].Value.edges )
				tempUnion.edges.Add( l );
			for (int i = 1; i < x; i++) {
				if( i == j ) continue;
				tempUnion.GeometryMerge(cameraVPS[i].Value, i);
			}
			double areaCovered = tempUnion.getPolygonArea( 0 );
			if( mapArea != 0 && areaCovered / mapArea > 0.95 )
				cnt++;
		}
		Debug.Log ("Number of individual cameras without which 95% coverage is possible: " + cnt);
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
	
	bool comprehensiveCollision( Line tmpLine, int xid ){
		//1.Geometry Check
		foreach( Geometry g in finalPoly ){
			//1A. Crude collision check (diagonal and line equality)
			if( g.LineCollision(tmpLine) ){
				return true;
			}
			else{
				//1B. Endpoint collection
				//This is for cases where the midpoint is not located inside the geometry
				//but the line crosses the geometry anyway and intersects only at endpoints
				//of the geometry's edge (midpoint scenario happens only in this case)
				List<Vector3> endptIntersection = new List<Vector3>();
				foreach( Line l in g.edges ){
					if( l.LineIntersectRegular( tmpLine ) == 1 ){
						Vector3 interv = l.GetIntersectionPoint( tmpLine );
						endptIntersection.Add( interv );
					}
				}
				//1C. Collected endpoint check
				//Now check with the collecetd endpoints
				foreach( Vector3 vA in endptIntersection ){
					Line TLA = new Line( tmpLine.vertex[0], vA );
					Line TLB = new Line( tmpLine.vertex[1], vA );
					if( g.PointInside( TLA.MidPoint() ) || g.PointInside( TLB.MidPoint() ) ){
						//						if (xid == 86){
						//							Debug.Log ("Got 86 tlatlb");
						//							Debug.Log ( g.PointInsideDebug( TLA.MidPoint() ) + " " + g.PointInside( TLB.MidPoint() ) );
						//							drawSphere( TLA.MidPoint() );
						//							g.DrawGeometry( GameObject.Find("temp") );
						//						}
						return true;
					}
					foreach( Vector3 vB in endptIntersection ){
						if( vA.Equals(vB) ) continue;
						Line testline = new Line( vA, vB );
						if( g.PointInside( testline.MidPoint() ) ){
//							if (xid == 86)
//								Debug.Log ("Got 86 testline");
							return true;						
						}
					}
				}
			}
		}
//		if( xid == 100 )
//			Debug.Log("Past the geometries");
		//2: MapBoundary
		//Note: Was buggy before LineIntersectMuntac's final check started using eps
		List<Vector3> endpointinterMap = new List<Vector3>();
		foreach( Line l in mapBG.edges ){
			//2A. Crude intersection check
			if( l.LineIntersectMuntac( tmpLine ) == 1 )
				return true;
			//2B. Endpoint collection
			else if( l.LineIntersectRegular( tmpLine ) == 1 ){
				//else if( l.LineIntersectMuntacEndPt( tmpLine ) == 1 ){
				Vector3 interv = l.GetIntersectionPoint( tmpLine );
				//if( !endpointinterMap.Contains( interv ) ) 
				endpointinterMap.Add( interv );
			}
		}
		//2C. Collected endpoint check
		//Midpoint case scenario for maps
		foreach( Vector3 vA in endpointinterMap ){
			Line TLA = new Line( tmpLine.vertex[0], vA );
			Line TLB = new Line( tmpLine.vertex[1], vA );
			if( xid == 2 ){
				//Debug.Log ( "Passed itar " + mapBG.PointOutside( TLA.MidPoint() ) + " " + mapBG.PointOutsideDebug( TLA.MidPoint(), 2 ) );
				//drawSphere( TLA.MidPoint() );
				//drawSphere( TLB.MidPoint() );
			}
			if( mapBG.PointOutside( TLA.MidPoint() ) || mapBG.PointOutside( TLB.MidPoint() ) )
				return true;
			foreach( Vector3 vB in endpointinterMap ){
				if( vA.Equals(vB) ) continue;
				Line testline = new Line( vA, vB );
				if( mapBG.PointOutside( testline.MidPoint() ) )
					return true;
			}
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