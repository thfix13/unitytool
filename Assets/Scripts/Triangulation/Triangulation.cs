using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Vectrosity;
//using System.

[ExecuteInEditMode]
public class Triangulation : MonoBehaviour 
{
	public float eps = 1e-5f;//the margin of accuracy for all floating point equivalence checks
	private float dijkstra_init_value;
	private const int sp = 0;
	private const int tri = 1;
	private const int widc = 2;
	//Data holder to display and save
	public List<Vector3> points = new List<Vector3>();
	public List<Color> colours = new List<Color>();
	public List<Vector3> cameras = new List<Vector3>();
	//	int xsdf;
	private string path_start;
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
	List<Geometry> mstNodes = new List<Geometry> ();
	//Contains Map
	public Geometry mapBG;
	private float mapBGPerimeter = 0f;
	//Camera
	public List<KeyValuePair<Vector3,Geometry>> cameraVPS = new List<KeyValuePair<Vector3, Geometry>>();
	public List<KeyValuePair<Vector3,Geometry>> cameraVPS2 = new List<KeyValuePair<Vector3, Geometry>>();
	public List<KeyValuePair<Vector3,Geometry>> cameraUnion = new List<KeyValuePair<Vector3, Geometry>>();
	//Contains all the visibility polygons for cameras
	public int vpnum = -1;
	
	public bool inkscape = false;	
	public bool drawTriangles = false;
	public bool drawTriRoadMap = false;
	public bool drawTriRoadMapExtended = false;
	public bool drawSPRoadMap = false;
	public bool drawSPRoadMapExtended = false;
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
	List<Vector3> triNonCameraNodes = new List<Vector3>();
	
	//Tour
	private List<Line> spRoadMap = new List<Line> ();
	private List<Line> spRoadMapExtended = new List<Line> ();
	private List<Line> triRoadMap = new List<Line> ();
	private List<Line> triRoadMapExtended = new List<Line> ();
	private List<Vector3> explorationTour = new List<Vector3> ();
	private List<Line> explorationTourPath = new List<Line> ();
	private Geometry tourEdges = new Geometry ();
	private const int tourgraphsize = 1000000;
	//Dijkstra
	List<edges>[] EL = new List<edges>[tourgraphsize];
	//Stores shortest path calculations with node i as source on all nodes j
	//float [,] d = new float [800, 800];
	List<List<float>> d = new List<List<float>>();
	//Stores the path for above calculation
	//int [,] parents = new int [800, 800];
	List<List<int>> parents = new List<List<int>>();
	
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
		mstNodes.Clear ();
		lines.Clear(); 
		geos.Clear();
		linesMinSpanTree.Clear();
		obsGeos.Clear ();
		masterReflex.Clear ();
		spRoadMap.Clear ();
		spRoadMapExtended.Clear ();
		triNonCameraNodes.Clear ();
		triRoadMap.Clear ();
		triRoadMapExtended.Clear ();
		GameObject temp = GameObject.Find("temp");
		DestroyImmediate(temp);
		GameObject vptmp = GameObject.Find("vptmp");
		DestroyImmediate(vptmp);
		temp = GameObject.Find("Walls");
		if( temp != null )
			DestroyImmediate(temp);
		temp = GameObject.Find("Obstacles");
		if( temp != null )
			DestroyImmediate(temp);
		temp = GameObject.Find("Borders");
		if( temp != null )
			DestroyImmediate(temp);
		geometryDrawn = false;
		obstacleDrawn = false;
		sprDrawn = false;
		stopAll = true;
		explorationTour.Clear ();
		explorationTourPath.Clear ();
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

		if(drawTriRoadMap)
		{
			foreach( Line l in triRoadMap )
				Debug.DrawLine( l.vertex[0], l.vertex[1], Color.green );

//			foreach(Triangle tt in triangles){
//				Line[] ll = tt.GetSharedLines(); 				
//				
//				if(ll.Length == 1)
//					Debug.DrawLine(ll[0].MidPoint(), tt.GetCenterTriangle(),Color.red);
//				else if(ll.Length > 2){
//					for(int i = 0; i<ll.Length; i++)
//						Debug.DrawLine(ll[i].MidPoint(), tt.GetCenterTriangle(),Color.red);
//				}				
//				else{
//					for(int i = 0; i<ll.Length; i++)
//						Debug.DrawLine(ll[i].MidPoint(), ll[(i+1) % ll.Length].MidPoint(),Color.red);
//				}
//			}
		}

		if (drawTriRoadMapExtended){
			foreach(Line l in triRoadMapExtended)
				l.DrawLine( Color.yellow );	
		}


		if(drawMinSpanTree){
			GameObject temp = GameObject.Find("temp"); 
			foreach(Line l in linesMinSpanTree)
				Debug.DrawLine(l.vertex[0], l.vertex[1],Color.red);
		}
		
		if (drawMapBoundary && !geometryDrawn) {
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
				drawSphere( v, Color.blue, cntcam++ );
			}		
		}
		
		if (drawSPRoadMap){
			GameObject temp = GameObject.Find("temp"); 
			foreach(Line l in spRoadMap)
				l.DrawLine( Color.yellow );
		}
		
		if (drawSPRoadMapExtended){
			GameObject temp = GameObject.Find("temp"); 
			foreach(Line l in spRoadMapExtended)
				l.DrawLine( Color.yellow );	
		}
		
		if( drawVP && vpnum != -1 ){
			Line tmpline = new Line (cameraVPS[vpnum].Key, new Vector3(0,1,50));
			tmpline.DrawLine(Color.white);
			foreach( Line l in cameraVPS[vpnum].Value.edges )
				l.DrawLine( Color.blue );
		}
		
		if( drawVPUnion && vpnum != -1 && cameraUnion.Count > 0 ){
			Line tmpline = new Line (cameraUnion[vpnum].Key, new Vector3(0,1,50));
			tmpline.DrawLine(Color.white);
			foreach( Line l in cameraUnion[vpnum].Value.edges )
				l.DrawLine( Color.blue );
		}
		
		if (drawTour) {
//			drawTour = false;
			for( int i = 0; i < explorationTour.Count - 1; i++ ){
//				if( i == 0 )
//					drawSphere( explorationTour[i], Color.red );
//				else if ( i == explorationTour.Count - 2 )
//					drawSphere( explorationTour[i + 1], Color.green );
				Line tmpline = new Line( explorationTour[i], explorationTour[i + 1] );
				//tmpline.DrawVector( GameObject.Find("temp"), Color.blue );
				tmpline.DrawLine(Color.blue);
				//Debug.DrawLine( explorationTour[i], explorationTour[i + 1] );
			}
		}
		if( drawTriangles ){	
			foreach(Triangle tt in triangles)
				tt.DrawDebug();
		}
	}
	
	public void drawingFromFile (){
		new GameObject ("temp");
		new GameObject ("Walls");
		new GameObject ("Obstacles");
		new GameObject ("Borders");
		float scalingFactor = 20f;
		float displacementFactorX = -40f;
		float displacementFactorZ = 40f;
		//		float scalingFactor = 1f;
		//		float displacementFactorX = 0f;
		//		float displacementFactorZ = 0f;
		//		Line a = new Line (new Vector3 (0, 1, 0), new Vector3 (10, 1, 0));
		//		a.DrawVector (GameObject.Find ("temp"));
		
		//scalingFactor and dispFact need to be changed for this -> 
		var reader = new StreamReader(File.OpenRead(path_start + @"\map.csv"));
		List<string> coord = new List<string> ();
		Geometry walls = new Geometry ();
		int wallcnt = 0;
		int mapcnt = 0;
		
		while ( !reader.EndOfStream )
		{
			var line = reader.ReadLine();
			var values = line.Split(';');
			//Check the symbol in first column of line
			if( values[0].Equals("M") ){
				//Coordinates in 2nd and 3rd column. Take them in as separate lists of floats.
				var prev = values[1].Split(',');
				var current = values[2].Split(',');
				Vector3 a = new Vector3();
				Vector3 b = new Vector3();					
				a.x = float.Parse( prev[0] )/scalingFactor;
				a.z = (-float.Parse( prev[1] )/scalingFactor) + displacementFactorZ;
				//					a.z = (float.Parse( prev[1] )/scalingFactor) + displacementFactorZ;
				a.y = b.y = 1;
				b.x = float.Parse( current[0] )/scalingFactor;
				b.z = (-float.Parse( current[1] )/scalingFactor) + displacementFactorZ;
				//					b.z = (float.Parse( current[1] )/scalingFactor) + displacementFactorZ;
				Line ln = new Line( a, b );
				
				ln.name = "Border "+mapcnt++.ToString();
				//ln.DrawVector(GameObject.Find ("Borders"));
				mapBG.edges.Add(ln);
			}
			else if( values[0].Equals("OBS") ){
				float x = float.Parse(values[1])/scalingFactor;
				float z = (-float.Parse(values[2])/scalingFactor) + displacementFactorZ;
				float width  = float.Parse(values[3])/scalingFactor;
				float height = -float.Parse(values[4])/scalingFactor;
				Vector3 a = new Vector3();
				Vector3 b = new Vector3();
				Vector3 c = new Vector3();
				Vector3 d = new Vector3();
				a.y = b.y = c.y = d.y = 1;
				a.x = x; a.z = z;
				b.x = x + width; b.z = z;
				c.x = x + width; c.z = z + height;
				d.x = x; d.z = z + height;
				Geometry g = new Geometry();
				g.edges.Add( new Line( a, b ) );
				g.edges.Add( new Line( b, c ) );
				g.edges.Add( new Line( c, d ) );
				g.edges.Add( new Line( d, a ) );
				obsGeos.Add(g);
				//g.DrawGeometry(GameObject.Find ("Obstacles"));
			}
			else{
				line = reader.ReadLine();
				values = line.Split(';');
				Geometry g = new Geometry();
				while( values[0].Equals("OB") ){
					var prev = values[1].Split(',');
					var current = values[2].Split(',');
					Vector3 a = new Vector3();
					Vector3 b = new Vector3();					
					a.x = float.Parse( prev[0] )/scalingFactor;
					a.z = (-float.Parse( prev[1] )/scalingFactor) + displacementFactorZ;
					//					a.z = (float.Parse( prev[1] )/scalingFactor) + displacementFactorZ;
					a.y = b.y = 1;
					b.x = float.Parse( current[0] )/scalingFactor;
					b.z = (-float.Parse( current[1] )/scalingFactor) + displacementFactorZ;
					//					b.z = (float.Parse( current[1] )/scalingFactor) + displacementFactorZ;
					Line ln = new Line( a, b );
					g.edges.Add( ln );
					ln.name = wallcnt.ToString();
					//ln.DrawVector(GameObject.Find("temp"));
					line = reader.ReadLine();
					values = line.Split(';');
				}
				wallcnt++;
				obsGeos.Add(g);
			}
		}
	}
	
	public void TriangulationSpace (){
		//Testing point
//		Vector2 a = new Vector2 (2, 17);
//		Vector2 b = new Vector2 (9, 12);
//		Debug.Log ((a - b).SqrMagnitude ());
//		float unitdistance = 5;
//		float div = (a - b).SqrMagnitude () / unitdistance;
////		Debug.Log (div);
//		Vector2 c = new Vector2 ((a.x + b.x) / div, (a.y + b.y) / div);
//		Vector2 d = new Vector2 ((a.x + b.x) / 2, (a.y + b.y) / 2);
////		Debug.Log (a);
////		Debug.Log (c);
//		Debug.Log (d);
////		Debug.Log ((a - c).SqrMagnitude ());
//		Debug.Log ((a - d).SqrMagnitude ());
		
		return;
		path_start = @"C:\Users\Asus\Desktop\McGill\Thesis\MapData\CoDCrash";
		this.Clear ();
		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch ();
		stopwatch.Start ();
		inkscape = true;
		bool computetilltriangulation = false;
		bool computetilltour = false;
		bool computetour = false;
		bool computeexternaltour = true;
		int roadmap = tri;
		/***PHASE A: PREPROCESSING***/
		/*STEP 1 - GET POLYGON POINTS*/
		if( inkscape )
			drawingFromFile ();
		else
			getPolygons ();
		
		/*STEP 2 - MERGE POLYGONS*/
		mergePolygons ();
		
		/*STEP 3 - DEFINE MAP BOUNDARY*/
		defineBoundary ();
		stopwatch.Stop();
		Debug.Log("PREPROCESSING: " + stopwatch.ElapsedMilliseconds * 0.001);
		
		//Debug.Log (getMapArea ());
		/***PHASE B: COLORING***/
		/*STEP 4 - MAKE MST OF POLYGONS*/
		if( !computetilltriangulation )
			scanMST ();
		else{
			stopwatch.Reset ();
			stopwatch.Start ();
			MST ();
			stopwatch.Stop ();
			Debug.Log("MST: " + stopwatch.ElapsedMilliseconds * 0.001);
			printMST ();
		}
		
		/*STEP 5 - TRIANGULATE*/
		if( !computetilltriangulation )
			scanTriangulation ();//Done externally by Triangle library
		else{
			//External Method
			printMapToPolyFile ();
			//Internal Method
			//		stopwatch.Reset ();
			//		stopwatch.Start ();
			//		triangulate ();
			//		stopwatch.Stop ();
			//		Debug.Log("Triangulation: " + stopwatch.ElapsedMilliseconds * 0.001);
			return;
		}
		
		/*STEP 6 - COLOR AND GET CAMERAS*/
		if( !computetilltour )
			scanCameras ();
		else{
			stopwatch.Reset ();
			stopwatch.Start ();
			colorCameras ();
			stopwatch.Stop ();
			Debug.Log("Cameras and Coloring: " + stopwatch.ElapsedMilliseconds * 0.001);
			printCameras ();
		}
		
		/***PHASE C: TOUR***/
		/*STEP 7 - CREATE "SHORTEST-PATH" ROADMAP*/
		if( !computetilltour ){
			if( roadmap == sp )
				scanSPRoadMap ();
			else if( roadmap == tri )
				scanTriRoadMap ();
		}
		else{
			stopwatch.Reset ();
			stopwatch.Start ();
			if( roadmap == sp ){
				makeSPRoadMap ();
				printSPRoadMap ();
			}
			else if( roadmap == tri ){
				makeTriRoadMap ();
				printTriRoadMap ();
			}
			stopwatch.Stop ();
			Debug.Log("Make SP Roadmap: " + stopwatch.ElapsedMilliseconds * 0.001);
		}

		/*External Tour*/
		if( computeexternaltour ){
			externalTour ( roadmap );
			return;
		}

		/*STEP 8 - RUN DIJKSTRA FOR EVERY CAMERA PAIR*/
		//Executed in makeTourOnSPR()
		/*----x-----*/
		
		/*STEP 9 - CREATE TOUR USING NEAREST NEIGHBOUR*/
		if( !computetour )
			scanTour ();
		else{
			stopwatch.Reset ();
			stopwatch.Start ();
			if( roadmap == sp ){
				if (!makeTourOnSPR (false, 1)){
					Debug.Log("makeTourOnSPR () failed");
					return;
				}
				stopwatch.Stop ();
				Debug.Log("Make tour: " + stopwatch.ElapsedMilliseconds * 0.001 + "s");
				printTour ();
			}
			else if( roadmap == tri ){
				if (!makeTourOnTri (false, 1)){
					Debug.Log("makeTourOnTri () failed");
					return;
				}
				stopwatch.Stop ();
				Debug.Log("Make tour: " + stopwatch.ElapsedMilliseconds * 0.001 + "s");
				printTour ();
			}
		}

		return;
		/***PHASE D: VISIBILITY***/
		/*STEP 10 - GET VISIBILITY POLYGONS OF TOUR POINTS*/
		if( !computetour )
			scanVPS ();
		else{
			//External method
			externalVP ();
			Debug.Log ("Printed mapWithTour");
//			Internal method
//			getCameraVPS ();
//			if (VPValidation () > 0){
//				Debug.Log("Invalid VPs found.");
//				return;
//			}
//			Debug.Log ("Vispol done");
//			printVPS ();		
		}
//		if (VPValidation () > 0){
//			Debug.Log("Invalid VPs found.");
//			return;
//		}
		Debug.Log ("Vispol done");
		return;
		
		/*STEP 11 - CHECK CAMERA VISIBILITY NESTING*/
		//		mergeVPS ();
		//		if (UnionValidation () > 0)
		//			return;
		Debug.Log ("ALL UNIONS valid");
		
		//return;
		/***PHASE E: MORE METRICS***/
		/*STEP 12 - CHECK INCREMENTAL CAMERA AREA COVERAGE*/
		areaCoverage ();
		return;
		/*STEP 13 - CHECK SUBTRACTIVE COVERAGE*/
		//subtractiveCoverage ();
		double mapArea = mapBG.getPolygonArea (0);
		foreach( Geometry g in finalPoly )
			mapArea -= g.getPolygonArea(0);
		Debug.Log (mapArea);
		return;
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
		int xid = 0;
		foreach (Geometry g in obsGeos) {
			//	g.DrawGeometry(GameObject.Find(	"temp" ), xid++ );		
		}
		xid = -1;
		foreach ( Geometry g in obsGeos ) {
			if( mapBG.GeometryIntersect( g ) && !mapBG.GeometryInsideMap( g ) ){
				Geometry tempBG = new Geometry();
				tempBG = mapBG.GeometryMergeInner( g, xid );
				if( inkscape )
					tempBG.BoundGeometry( mapBG );
				else
					tempBG.BoundGeometryCrude( mapBoundary );
				mapBG = tempBG;
			}
			else
				finalPoly.Add(g);
		}
		//TODO:Check if mapBG has any disjoint parts
		//mapBG.getSortedEdges ();
		int cnt = 0;
		foreach (Geometry g in finalPoly) {
			foreach( Line l in g.edges ){
				l.name = "Wall " + cnt++.ToString();
				totalGeo.edges.Add( l );
			}
		}
		foreach( Line l in mapBG.edges ){
			mapBGPerimeter += l.Magnitude();
			totalGeo.edges.Add( l );
		}
		totalGeoVerts = totalGeo.GetVertex ();
	}
	
	private float getMapArea(){
		double mapArea = mapBG.getPolygonArea (0);
		foreach( Geometry g in finalPoly )
			mapArea -= g.getPolygonArea(0);
		return (float)mapArea;
	}
	
	//	void MST(){
	//		//-----------START MST CODE------------------//
	//		//We will use "mapBG" and "finalPoly"
	//		//finalPoly contains the "quadrilaters"
	//		//get all lines from quadrilaters/finalPoly and put them in "lines" || We use "obsLines"
	//		List<Line> obsLines = new List<Line> ();
	//		List<Geometry> toCheck = new List<Geometry> ();
	//		foreach (Geometry g in finalPoly) {
	//			foreach( Line l in g.edges )
	//				obsLines.Add( l );
	//			toCheck.Add(g);
	//		}
	//		//set links with neighbors for each quadrilater (send list of all obstacles as a paramter)
	//
	//		foreach (Geometry g in toCheck)
	//			g.SetVoisins( toCheck, mapBG );
	//		Debug.Log ("Hello");
	//
	//
	//		//keep a list of the edges (graph where obstaceles are the nodes) in a list of lines called "linesLinking"
	//		List<Vector3> mapVertices = mapBG.GetVertex();
	//		
	//		//Can be made simpler
	//		//Find a geometry to link to any of the map vertices
	//		Geometry start = null;
	//		for (int i = 0; i < mapVertices.Count; i++) {
	//			start = mapBG.findClosestQuad (mapVertices [i], toCheck, mapBG);
	//			if( start != null )
	//				break;
	//		}
	//		//Connect border to this geometry
	//		List<Line> linesLinking = new List<Line> ();
	//		linesLinking.Add (mapBG.GetClosestLine (start, toCheck, mapBG));
	//		start.visited = true;
	//		
	//		List<Geometry> toCheckNode = new List<Geometry> (); 
	//		toCheckNode.Add (start); 
	//		Line LinetoAdd = start.voisinsLine [0];
	//		
	//		//Straight Porting//
	//		while (LinetoAdd != null) {
	//			LinetoAdd = null; 
	//			Geometry qToAdd = null; 
	//			
	//			//Check all 
	//			foreach (Geometry q in toCheckNode) {
	//				
	//				for (int i = 0; i<q.voisins.Count; i++) {
	//					if (! q.voisins [i].visited) {
	//						if (LinetoAdd != null) {
	//							//get the shortest line
	//							if ( floatCompare( LinetoAdd.Magnitude (), q.voisinsLine [i].Magnitude (), ">=" ) ){
	//								LinetoAdd = q.voisinsLine [i];
	//								qToAdd = q.voisins [i]; 
	//								
	//							}
	//						} else {
	//							qToAdd = q.voisins [i]; 
	//							LinetoAdd = q.voisinsLine [i];
	//						}
	//					} else {
	//						continue; 
	//					}
	//				}
	//			}
	//			if (LinetoAdd != null) {
	//				linesLinking.Add (LinetoAdd); 
	//				qToAdd.visited = true; 
	//				toCheckNode.Add (qToAdd); 
	//			}
	//		}
	//		
	//		foreach (Line l in linesLinking)
	//			triangulation.linesMinSpanTree.Add (l);
	//		//END porting
	//		
	//		//-----------END MST CODE--------------------//
	//	}
	
	KeyValuePair<bool, List<Line>> makeMST( List<int> visited, List<Line> connectors ){
		for( int i = 0; i < mstNodes.Count; i++ ){
			if( visited[i] == 1 ) continue;
			
			for( int j = 0; j < mstNodes.Count; j++ ){
				if( visited[j] != 1 ) continue;
				
				KeyValuePair<bool, List<Line>> kvp = MSTConnectable( mstNodes[i], mstNodes[j], connectors );
				if( kvp.Key == true ){
					visited[i] = 1;
					kvp = makeMST( visited, kvp.Value );
					if( kvp.Key == true ) return kvp;
					visited[i] = 0;
				}
			}
		}
		bool success = true;
		//		foreach( int i in visited ){
		//			if( visited[i] == 0 ){
		//				success = false;
		//				break;
		//			}
		//		}
		if( connectors.Count == mstNodes.Count - 1 ) success = true;
		return new KeyValuePair<bool, List<Line>> (success, connectors);
	}
	
	KeyValuePair<bool, List<Line>> MSTConnectable( Geometry a, Geometry b, List<Line> connectors ){
		float mindist = 1000f;
		Line minLine = null;
		foreach (Vector3 v1 in a.GetVertex()){
			foreach (Vector3 v2 in b.GetVertex()){
				Line l1 = new Line( v1, v2 );
				if( comprehensiveCollision(l1, 0) ) continue;
				bool collides = false;
				foreach( Line l2 in connectors ){
					if( l1.LineIntersectMuntac(l2) > 0 ){
						collides = true;
						break;
					}
				}
				if( !collides ){
					if( l1.Magnitude() < mindist ){
						//connectors.Add( l1 );
						mindist = l1.Magnitude();
						minLine = l1;
					}
					//return new KeyValuePair<bool, List<Line>>( true, connectors );
				}								
			}
		}
		if( minLine != null ){
			connectors.Add( minLine );
			//minLine.DrawVector(GameObject.Find("temp"));
			return new KeyValuePair<bool, List<Line>>( true, connectors );
		}
		return new KeyValuePair<bool, List<Line>>( false, connectors );
	}
	
	void MST(){
		List<int> visited = new List<int> ();
		mstNodes.Clear ();
		mstNodes.Add(mapBG);
		visited.Add (0);
		foreach (Geometry g in finalPoly){
			mstNodes.Add (g);
			visited.Add (0);
		}
		
		for( int i = 0; i < mstNodes.Count; i++ ){
			visited[i] = 1;
			//toSend.Remove( g );
			KeyValuePair<bool, List<Line>> kvp = makeMST( visited, new List<Line>() );
			if( kvp.Key == true ){
				foreach( Line l in kvp.Value )
					linesMinSpanTree.Add( l );
				break;
			}
		}
		if( linesMinSpanTree.Count == 0 )
			Debug.Log("MST couldn't be created");
	}
	
	void triangulate(){
		List<Vector3> tempVertex = new List<Vector3>();
		totalGeo = new Geometry ();
		int vlcnt = 0;
		int vertcnt = 0;
		lines.Clear ();
		//Constructing "lines" for triangulation
		//First add lines that are in MST
		foreach (Line l in totalGeo.edges)
			lines.Add (l);
		foreach (Line l in linesMinSpanTree)
			lines.Add (l);
		foreach (Vector3 Va in totalGeoVerts){
			vertcnt++;
			//drawSphere( Va, Color.blue, vertcnt );
			int vcnt2 = 0;
			foreach(Vector3 Vb in totalGeoVerts){
				vcnt2++;
				//				if( vertcnt == 45 && vcnt2 == 146 )
				//					new Line( Va, Vb ).DrawVector(GameObject.Find("temp"));
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
					//Note: comprehensiveCollision uses Geometry.LineCollision
					//which only uses LineIntersectMuntac and not one with EndPt detection
					//Same for checks against MapBG.
					//But since we've added all the totalGeo edges beforehand those collision will be
					//checked in above loop.
					collides = comprehensiveCollision( tempLine, 0 );
					//					if( vertcnt == 45 && vcnt2 == 146 ){
					//						Debug.Log(collides);
					//						Debug.Log( tempLine.LineIntersectMuntacEndPt( new Line( allVertex[141], allVertex[142] ) ) );
					//						comprehensiveCollision( tempLine, -100 );
					//					}
					//Add Line
					if( !collides ){
						lines.Add( tempLine );
						//tempLine.DrawVector(GameObject.Find("temp"));
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
		//		foreach (Line ls in lines)
		//			ls.DrawVector (GameObject.Find ("temp"));
		//Find the centers 
		triangles = new List<Triangle> ();
		//Well why be efficient when you can be not efficient
		foreach (Line l in lines) {
			//l.DrawVector(GameObject.Find("temp"));
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
		//List<Vector3> points = new List<Vector3> (); 
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
		
		//Debug.Log (count [0] + "," + count [1] + "," + count [2]);
		triangulation.points = points; 
		triangulation.colours = coloursPoints; 
		
		//Get points with the least colour
		Color cGuard = Color.cyan; 
		int lowest = 100000; 
		
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
			}
			//drawSphere( points[i], colours[i] );
		}
		//Debug.Log (cameras.Count);
	}

	//Creates a "shortest path roadmap" also known as "reduced visibility graph"
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

	private void makeTriRoadMap() {
		foreach(Triangle tt in triangles){
			Line[] ll = tt.GetSharedLines();
			if(ll.Length == 1)
				triRoadMap.Add(new Line(ll[0].MidPoint(), tt.GetCenterTriangle()));
			else if(ll.Length > 2){
				for(int i = 0; i<ll.Length; i++)
					triRoadMap.Add( new Line(ll[i].MidPoint(), tt.GetCenterTriangle()) );
			}				
			else{
				for(int i = 0; i<ll.Length; i++)
					triRoadMap.Add(new Line(ll[i].MidPoint(), ll[(i+1) % ll.Length].MidPoint()));
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
	
	bool makeTourOnSPR( bool external, int granularity ){
		Debug.Log ("Total Cameras: " + cameras.Count);
		Debug.Log ("SPRoadMap: " + spRoadMap.Count);
		if (cameras.Count < 2)	return true;
		Dictionary<Vector3, int> dict = new Dictionary<Vector3, int> ();
		Dictionary<int, Vector3> numToVect = new Dictionary<int, Vector3> ();
		int N = 0;
		//0. Deal with granularity
		foreach( Vector3 vA in masterReflex ){
			foreach( Vector3 vB in masterReflex ){
				if( vA == vB ) continue;

			}
		}
		//1. Construct graph in EL (global variable, edge list)
		//1A. give IDs to vertices (masterReflex (without cameras) and cameras independently)
		foreach (Vector3 v in masterReflex){
			if( cameras.Contains(v) || dict.ContainsKey(v) ) continue;
			EL[N] = new List<edges>();
			dict.Add( v, N );
			numToVect.Add(N, v);
			N++;
		}
		int GSC = N; //graphSizeSansCameras alternatively the id of the first camera
		foreach (Vector3 v in cameras) {
			if( !dict.ContainsKey( v ) ){
				EL[N] = new List<edges>();
				dict.Add( v, N );
				numToVect.Add(N, v);
				N++;
			}
		}
		//		if (N > 490) {
		//			Debug.Log("Too many nodes in SPRoadMap");
		//			return false;
		//		}
		//1B. Construct edges with these new ids from existing RoadMap
		foreach (Line l in spRoadMap) {
			int u = dict[l.vertex[0]];
			int v = dict[l.vertex[1]];
			EL[u].Add(new edges( v, l.Magnitude() ));
			EL[v].Add(new edges( u, l.Magnitude() ));
			spRoadMapExtended.Add( l );
		}
		
		/*2. CAMERA WORK*/
		//2A. Connect cameras to each other and the rest of the graph (i.e. reflex points)
		List<Vector3> masterReflexAndCamera = new List<Vector3> ();
		foreach (Vector3 v1 in cameras){
			if( !masterReflexAndCamera.Contains(v1) )
				masterReflexAndCamera.Add (v1);
		}
		foreach (Vector3 v1 in masterReflex){
			if( !cameras.Contains(v1) && !masterReflexAndCamera.Contains(v1) ) 
				masterReflexAndCamera.Add (v1);
		}
		//Make edges between cameras and points in spRoadMap and cameras
		int cntx = 0;
		int cnty = 0;
		//		foreach (Vector3 v1 in masterReflexAndCamera) {
		//			drawSphere( v1, Color.green, cntx++ );
		//		}
		//		Debug.Log(cameras.Count + " " + masterReflexAndCamera.Count);
		foreach(Vector3 v1 in cameras){
			//drawSphere( v1, Color.red);
			cntx++;
			bool connected = false;
			cnty = 0;
			foreach(Vector3 v2 in masterReflexAndCamera){
				cnty++;
				if( VectorApprox( v1, v2 ) ) continue;
				int u = dict[v1];
				int v = dict[v2];
				Line tmpLine = new Line( v1, v2 );
				bool collides = false;
				if( spRoadMap.Contains( tmpLine ) ){
					connected = true;
					continue;
				}
				bool added = false;
				//Add link if already an obstacle or map edge
				foreach( Line l in totalGeo.edges ){
					if( l.Equals( tmpLine ) ){
						EL[u].Add(new edges( v, tmpLine.Magnitude() ) );
						EL[v].Add(new edges( u, tmpLine.Magnitude() ) );
						connected = true;
						added = true;
						spRoadMapExtended.Add( l );
						break;
					}
				}
				if( added ) continue;
				
				collides = comprehensiveCollision( tmpLine, 0 );
				if( !collides ){
					connected = true;
					EL[u].Add(new edges( v, tmpLine.Magnitude() ) );
					EL[v].Add(new edges( u, tmpLine.Magnitude() ) );
					spRoadMapExtended.Add( new Line( v1, v2 ) );
				}
			}
			
			
			if( !connected ){
				Debug.Log("Camera not connected");
				//				drawSphere( v1 );
				//				Debug.Log(cntx);
				//return false;
			}
		}
		
		/*3. DIJKSTRA*/
		//3A. Calculate All-Pair-Shortest-Path
		dijkstra_init_value = 100000f;
		for( int i = 0; i < N; i++ )
			Dijkstra( i, N, numToVect );
		for( int i = 0; i < N; i++ ){
			for( int j = 0; j < N; j++ ){
				if( i!=j && j == parents[i][j] )
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

		if( !external ){
			makeMinimumTour( GSC, N, numToVect );
//			makeMaximumTour( GSC, N, numToVect );
//			makeClosestNonVisibleTour (GSC, N, numToVect);
		}
		else
			printDijkstra( GSC, N, numToVect );
		return true;
	}

	bool makeTourOnTri( bool external, int granularity ){
		Debug.Log ("Total Cameras: " + cameras.Count);
		Debug.Log ("TriRoadMap: " + triRoadMap.Count);
		if (cameras.Count < 2)	return true;
		Dictionary<Vector3, int> dict = new Dictionary<Vector3, int> ();
		Dictionary<int, Vector3> numToVect = new Dictionary<int, Vector3> ();
		int N = 0;

		//1. Construct graph in EL (global variable, edge list)
		//1A. give IDs to vertices (masterReflex (without cameras) and cameras independently)
		triNonCameraNodes = new List<Vector3> ();
		foreach (Line l in triRoadMap) {
			if( !triNonCameraNodes.Contains(l.vertex[0]) && !cameras.Contains(l.vertex[0]) )
				triNonCameraNodes.Add( l.vertex[0] );
			if( !triNonCameraNodes.Contains(l.vertex[1]) && !cameras.Contains(l.vertex[1]) )
				triNonCameraNodes.Add( l.vertex[1] );
		}
		foreach (Vector3 v in triNonCameraNodes){
			if( cameras.Contains(v) || dict.ContainsKey(v) ) continue;
			EL[N] = new List<edges>();
			dict.Add( v, N );
			numToVect.Add(N, v);
			N++;
		}
		int GSC = N; //graphSizeSansCameras alternatively the id of the first camera
		foreach (Vector3 v in cameras) {
			if( !dict.ContainsKey( v ) ){
				EL[N] = new List<edges>();
				dict.Add( v, N );
				numToVect.Add(N, v);
				N++;
			}
		}

		//1B. Construct edges with these new ids from existing RoadMap
		foreach (Line l in triRoadMap) {
			int u = dict[l.vertex[0]];
			int v = dict[l.vertex[1]];
			EL[u].Add(new edges( v, l.Magnitude() ));
			EL[v].Add(new edges( u, l.Magnitude() ));
			triRoadMapExtended.Add( l );
		}
		
		/*2. CAMERA WORK*/
		//2A. Connect cameras to each other and the rest of the graph (i.e. reflex points)
		List<Vector3> triAllNodes = new List<Vector3> ();
		foreach (Vector3 v1 in cameras){
			if( !triAllNodes.Contains(v1) )
				triAllNodes.Add (v1);
		}
		foreach (Vector3 v1 in triNonCameraNodes){
			if( !cameras.Contains(v1) && !triAllNodes.Contains(v1) ) 
				triAllNodes.Add (v1);
		}
		//Connect each camera to the point its closest to on the roadmap
		int cntx = 0;
		int cnty = 0;
		foreach(Vector3 v1 in cameras){
			//drawSphere( v1, Color.red);
			cntx++;
			bool connected = false;
			cnty = 0;
			float mindist = 10000f;
			Line minconnector = null;
			foreach(Vector3 v2 in triNonCameraNodes){
				cnty++;
				if( VectorApprox( v1, v2 ) ) continue;
//				int u = dict[v1];
//				int v = dict[v2];
				Line tmpLine = new Line( v1, v2 );
				bool collides = false;
				if( triRoadMap.Contains( tmpLine ) ){
					connected = true;
					continue;
				}
				bool added = false;
				//Add link if already an obstacle or map edge
				foreach( Line l in totalGeo.edges ){
					if( l.Equals( tmpLine ) ){
//						EL[u].Add(new edges( v, tmpLine.Magnitude() ) );
//						EL[v].Add(new edges( u, tmpLine.Magnitude() ) );
						connected = true;
						added = true;
//						triRoadMapExtended.Add( l );
						break;
					}
				}
				if( added ) break;
				
				collides = comprehensiveCollision( tmpLine, 0 );
				if( !collides && tmpLine.Magnitude() < mindist ){
					connected = true;
//					EL[u].Add(new edges( v, tmpLine.Magnitude() ) );
//					EL[v].Add(new edges( u, tmpLine.Magnitude() ) );
//					triRoadMapExtended.Add( new Line( v1, v2 ) );
					minconnector = tmpLine;
					mindist = minconnector.Magnitude();
				}
			}

			if( !connected ){
				Debug.Log("Camera not connected");
				return false;
			}
			else if( mindist != 10000f ){
				triRoadMapExtended.Add( minconnector );
				int u = dict[minconnector.vertex[0]];
				int v = dict[minconnector.vertex[1]];
				EL[u].Add(new edges( v, mindist ) );
				EL[v].Add(new edges( u, mindist ) );
			}
		}
		
		/*3. DIJKSTRA*/
		//3A. Calculate All-Pair-Shortest-Path
		dijkstra_init_value = 100000f;
		for( int i = 0; i < N; i++ )
			Dijkstra( i, N, numToVect );
		for( int i = 0; i < N; i++ ){
			for( int j = 0; j < N; j++ ){
				//Debug.Log(i + "," + j + ":" + parents[i,j]);
				if( i!=j && j == parents[i][j] )
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
		if( !external ){
			makeMinimumTour( GSC, N, numToVect );
//			makeMaximumTour( GSC, N, numToVect );
//			makeClosestNonVisibleTour (GSC, N, numToVect);
		}
		else
			printDijkstra( GSC, N, numToVect );
		return true;
	}
	
	private void Dijkstra( int id, int N, Dictionary<int, Vector3> numToVect ){
		SortedDictionary< float, int > SD = new SortedDictionary< float, int > ();
		//1. Initialization
		int cnt = 0;
		for (int i = 0; i < N; i++) {
			d [id][i] = dijkstra_init_value;
			parents[id][i] = -1;
		}
		d[id][id] = 0f;
		parents [id][id] = id;
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
				float nw = dist + E.w;
				if( nw < d[id][v] ){
					d[id][v] = nw;
					//TODO:Implement a more permanent solution. Multiple keys for a sorted dictionary.
					while( SD.ContainsKey(nw) )
						nw += eps;
					SD.Add( nw, v );
					parents[id][v] = u;
				}
			}
		}
		for (int i = 0; i < N; i++) {
			if( parents[id][i] == -1 )
				Debug.Log("All nodes not reached:" + i + " for " + id + " tour");
		}
	}
	
	public void makeMinimumTour( int GSC, int N, Dictionary<int, Vector3> numToVect ){
		/*4. MAKE TOUR*/
		bool [] visited = new bool [N + 100];
		for( int i = 0; i < N; i++ )
			visited[i] = false;
		//		Debug.Log ("GSC : " + GSC + " N: " + N);
		int current = GSC;
		float tourDistance = 0;
		List< int > tour = new List<int> ();
		//Add first camera to the tour
		tour.Add (current);
		int xcnt = 0;
		Debug.Log (N + "," + GSC + "," + cameras.Count);
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
				if( d[current][i] < mindist ){
					nearestNeighbor = i;
					mindist = d[current][i];
				}
			}
			if( nearestNeighbor == -1 )
				break;
			Debug.Log(nearestNeighbor + "," + current + ", " + d[current][nearestNeighbor]);
			//4C. Determine the path to the chosen camera
			int src = current;
			int dest = nearestNeighbor;
			Stack<int> stk = new Stack<int> ();
			//Debug.Log( src + " " + GSC + " " + N);
			Vector3 prevv = numToVect[dest];
			while( src != dest ){
				stk.Push(dest);
				dest = parents[current][dest];
				if( cameras.Contains(numToVect[dest]) )
					visited[dest] = true;
				xcnt++;
			}
			while( stk.Count != 0 ){
				Vector3 v = numToVect[stk.Peek()];
				tour.Add( stk.Pop() );
			}
			//Size of tour
			tourDistance += mindist;
			current = nearestNeighbor;
		}
		//Debug.Log ("Cameras Visited: " + xcnt);
		Debug.Log ("Size of exploration tour: " + tourDistance);
		float totalexp = 0;
		for (int i = 0; i < tour.Count; i++) {
			Vector3 v = numToVect[tour[i]];
			explorationTour.Add(v);
		}
		Debug.Log ("Nodes traversed on tour: " + explorationTour.Count);
	}
	
	public void makeMaximumTour( int GSC, int N, Dictionary<int, Vector3> numToVect ){
		/*4. MAKE TOUR*/
		bool [] visited = new bool [N + 100];
		for( int i = 0; i < N; i++ )
			visited[i] = false;
		//		Debug.Log ("GSC : " + GSC + " N: " + N);
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
			float maxdist = 0f;
			int farthestNeighbor = -1;
			//4B. Find the camera closest to it in the EL graph
			for( int i = 0; i < N; i++ ){
				if( i == current ) continue;
				if( !cameras.Contains(numToVect[i]) ) continue; //if this is not a camera
				if( visited[i] ) continue;
				if( d[current][i] > maxdist ){
					farthestNeighbor = i;
					maxdist = d[current][i];
				}
			}
			if( farthestNeighbor == -1 )
				break;
			//4C. Determine the path to the chosen camera
			int src = current;
			int dest = farthestNeighbor;
			Stack<int> stk = new Stack<int> ();
			while( src != dest ){
				stk.Push(dest);
				dest = parents[current][dest];
				if( cameras.Contains(numToVect[dest]) )
					visited[dest] = true;
				xcnt++;
			}
			while( stk.Count != 0 )
				tour.Add( stk.Pop() );
			//Size of tour
			tourDistance += maxdist;
			current = farthestNeighbor;
		}
		//Debug.Log ("Cameras Visited: " + xcnt);
		Debug.Log ("Size of exploration tour: " + tourDistance);
		
		for (int i = 0; i < tour.Count; i++) {
			Vector3 v = numToVect[tour[i]];
			explorationTour.Add(v);
		}
	}
	
	public void makeClosestNonVisibleTour( int GSC, int N, Dictionary<int, Vector3> numToVect ){
		/*4. MAKE TOUR*/
		bool [] visited = new bool [N + 100];
		for( int i = 0; i < N; i++ )
			visited[i] = false;
		//		Debug.Log ("GSC : " + GSC + " N: " + N);
		int [,] isVisible = new int [N + 10, N + 10];
		for( int i = 0; i < N + 10; i++ )
			for( int j = 0; j < N + 10; j++ )
				isVisible[i,j] = 0;
		
		for (int i = 0; i < N; i++) {
			for (int j = i + 1; j < N; j++) {
				Vector3 v1 = numToVect[i];
				Vector3 v2 = numToVect[j];
				Line ln = new Line( v1, v2 );
				if( !comprehensiveCollision( ln, 0 ) ){
					isVisible[i,j] = 1;
					isVisible[j,i] = 1;
				}
			}
		}
		float tourDistance = 0;
		List< int > tour = new List<int> ();
		int current = GSC;
		tour.Add (current);
		int xcnt = 0;
		int totalvisited = 0;
		List<Vector3> visitedcams = new List<Vector3> ();
		//4A. Choose any camera as starting point
		while( true ){
			visited[current] = true;
			if( cameras.Contains( numToVect[current] ) && !visitedcams.Contains( numToVect[current] ) )
				visitedcams.Add( numToVect[current] );
			if( visitedcams.Count == cameras.Count ) break;
			xcnt++;
			float mindist = 10000f;
			int nearestNeighbor = -1;
			//4B. Find the camera closest to it in the EL graph
			for( int i = 0; i < N; i++ ){
				if( i == current ) continue;
				if( visited[i] ) continue;
				if( isVisible[current,i] == 1 ) continue;
				if( d[current][i] < mindist ){
					nearestNeighbor = i;
					mindist = d[current][i];
				}
			}
			//If no nodes found
			if( nearestNeighbor == -1 ){
				//Stop if all cameras have been visited
				if( visitedcams.Count == cameras.Count ) break;
				//Otherwise visit any camera regardless of whether it's visible
				else{
					for( int i = 0; i < N; i++ ){
						if( i == current ) continue;
						if( !cameras.Contains(numToVect[i]) ) continue; //if this is not a camera
						if( visited[i] ) continue;
						if( d[current][i] < mindist ){
							nearestNeighbor = i;
							mindist = d[current][i];
						}
					}
				}
			}
			//if( nearestNeighbor == -1 ) break;
			//4C. Determine the path to the chosen camera
			int src = current;
			int dest = nearestNeighbor;
			Stack<int> stk = new Stack<int> ();
			//Debug.Log( src + " " + GSC + " " + N);
			while( src != dest ){
				stk.Push(dest);
				dest = parents[current][dest];
				if( current == -1 || dest == -1 ){
					Debug.Log("-1 found");
					return;
				}
				if( cameras.Contains(numToVect[dest]) ){
					visited[dest] = true;
					if( !visitedcams.Contains(numToVect[dest]) )
						visitedcams.Add(numToVect[dest]);
				}
				xcnt++;
			}
			while( stk.Count != 0 )
				tour.Add( stk.Pop() );
			//Size of tour
			tourDistance += mindist;
			current = nearestNeighbor;
		}
		//Debug.Log ("Cameras Visited: " + xcnt);
		Debug.Log ("Size of exploration tour: " + tourDistance);
		
		for (int i = 0; i < tour.Count; i++) {
			Vector3 v = numToVect[tour[i]];
			explorationTour.Add(v);
		}
		Debug.Log ("Nodes traversed on tour: " + explorationTour.Count);
	}
	
	void drawSphere( Vector3 v ){
		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.transform.renderer.material.color = Color.gray;
		inter.transform.position = v;
		inter.transform.localScale = new Vector3(0.1f,0.1f,0.1f); 
		//inter.transform.localScale = new Vector3(0.01f,0.01f,0.01f); 
		inter.transform.parent = temp.transform;
		//inter.gameObject.name = vlcnt.ToString();
	}
	void drawSphere( Vector3 v, Color x ){
		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.transform.renderer.material.color = x;
		inter.transform.position = v;
		inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
		//inter.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
		inter.transform.parent = temp.transform;
		//inter.gameObject.name = vlcnt.ToString();
	}
	
	void drawSphere( Vector3 v, Color x, int vlcnt ){
		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.transform.renderer.material.color = x;
		inter.transform.position = v;
		inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
		//inter.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
		//inter.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
		inter.transform.parent = temp.transform;
		inter.gameObject.name = vlcnt.ToString();
	}
	
	void drawSphere( Vector3 v, Color x, double vlcnt ){
		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.transform.renderer.material.color = x;
		inter.transform.position = v;
		inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
		//inter.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
		//inter.transform.localScale = new Vector3(0.01f,0.01f,0.01f);
		inter.transform.parent = temp.transform;
		inter.gameObject.name = vlcnt.ToString ();
	}
	
	Geometry visibilityPolygon( Vector3 kernel, int xid ){
		List<Vector3> allpoints = totalGeo.GetVertex ();
		List<Vector3> vp = new List<Vector3> ();
		int cnt = 0;
		int tempcnt = 0;
		//Step 1 - Find all points that are initially visible
		foreach (Vector3 v in allpoints){
			//drawSphere( v, Color.cyan, tempcnt++ );
			//tempcnt++;
			Line tmpLine = new Line( kernel, v );
			if( !comprehensiveCollision( tmpLine, 0 ) ){
				//if( !comprehensiveCollisionEndPt( tmpLine, 0 ) ){
				vp.Add ( v );
				//				if( xid == 5 && cnt == 21 )
				//					comprehensiveCollision( tmpLine, -100 );
				//				if( xid == 5 )
				//					drawSphere( v, Color.blue, cnt++);
			}
		}
		if (vp.Count == 0)
			return new Geometry ();
		//Step 2- Sort the points by angle
		Geometry x = new Geometry ();
		List<KeyValuePair<Vector3, double>> anglist = x.GetVertexAngleSorted (kernel, vp);
		if (xid == 16) {
			//			drawSphere( kernel, Color.blue);
			//			Line kernelline = new Line (kernel, new Vector3 (-100, 1, 100));
			//			kernelline.name = "Line Outpoint";
			//			kernelline.DrawVector (GameObject.Find ("temp"));
		}
		cnt = 0;
		foreach(KeyValuePair<Vector3, double> kvp in anglist) {
			if( xid == 81 ){
				//				drawSphere( kvp.Key, Color.red, kvp.Value );
				//				Line tmpline = new Line( kernel, kvp.Key );
				//				tmpline.name = "Line" + cnt + " " + kvp.Value;
				//				tmpline.DrawVector(GameObject.Find("temp"), Color.cyan);
			}
		}
		
		//Step 3 - Extend where applicable and find rest of the points
		List<KeyValuePair<Vector3, double>> anglistExt = new List<KeyValuePair<Vector3, double>> ();
		List<Vector3> uniqueList = new List<Vector3> ();
		cnt = -1;
		for( int i = 0; i < anglist.Count; i++ ){
			cnt++;
			Vector3 v = anglist[i].Key;
			double angle = anglist[i].Value;
			List<Vector3> tmplist = new List<Vector3>();
			if( v == kernel )
				continue;
			Vector3 nv = v;
			Vector3 prevv = kernel;
			
			KeyValuePair<bool, Vector3> mrpts = new KeyValuePair<bool, Vector3>(true, nv);
			int depth = 0;
			while( i != anglist.Count - 1 && doubleCompare( anglist[i].Value, anglist[i + 1].Value ) ){
				//while( i != anglist.Count - 1 && Math.Abs( anglist[i].Value - anglist[i + 1].Value ) < 0.01 ){
				tmplist.Add( anglist[i].Key );
				anglistExt.Add( new KeyValuePair<Vector3, double>( anglist[i].Key, anglist[i].Value ));
				prevv = nv;
				nv = anglist[i + 1].Key;
				i++;
			}
			while( mrpts.Key ){
				++depth;
				anglistExt.Add( new KeyValuePair<Vector3, double>( nv, angle ));
				tmplist.Add( nv );
				mrpts = morePointsDebug(prevv, nv, xid, cnt);
				prevv = nv;
				nv = mrpts.Value;
			}
			if( xid == 9 && cnt == 2 ){
				int xx = 0;
				prevv = kernel;
				foreach( Vector3 vvv in tmplist ){
					//drawSphere(vvv, Color.green, xx++ );
					//break;
					//if( xx == 1 )
					//morePointsDebug(prevv, vvv, -100, 0);
					//morePointsDebug(prevv, vvv, -100, 0);
					break;
					//prevv = vvv;
					//xx++;
				}
			}
		}
		cnt = 0;
		List<Vector3> penultimateList = new List<Vector3> ();
		foreach (KeyValuePair<Vector3, double> kvp in anglistExt) {
			//			penultimateList.Add( kvp.Key );
			//			if( xid == 7 )
			//			drawSphere( kvp.Key, Color.blue, kvp.Value );
			//			drawSphere( kvp.Key );
		}
		
		if( xid == 9 ){
			//			foreach (Vector3 v in allpoints){
			//				Line tmpLine = new Line( kernel, v );
			//				//if( !comprehensiveCollision( tmpLine, 0 ) ){
			//				if( !penultimateList.Contains( v ) && !comprehensiveCollision( tmpLine, 0 ) ){
			//					drawSphere(v);
			//				}
			//			}
		}
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
			double angleA = anglistExt[i].Value;
			for( indA = i; indA < anglistExt.Count && doubleCompare(anglistExt[indA].Value, angleA); indA++, i++ ){
				templistA.Add( anglistExt[indA].Key );
			}
			
			if( indA == anglistExt.Count ){
				if( nextstartingpoint == "last" )
					templistA.Reverse();
				foreach( Vector3 v in templistA )
					vispol.Add( v );
				break;
			}
			double angleB = anglistExt[i].Value;
			int anglistExtIndBStart = i;
			for( indB = i; indB < anglistExt.Count && doubleCompare(anglistExt[indB].Value, angleB); indB++ ){
				templistB.Add( anglistExt[indB].Key );
			}
			i = indA - 1;//since it'll be incremented after the end of the loop
			int anglistExtIndB = indB - 1;
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
				if( xid == 9 && cnt == 2 ){
					//Debug.Log( LL + " " + LF + " " + FL + " " + FF );
					//					foreach( Vector3 vyv in templistA )
					//						drawSphere( vyv, Color.grey, 1 );									
					//					foreach( Vector3 vyv in templistB )
					//						drawSphere( vyv, Color.grey, 2 );
					//					Debug.Log(nextstartingpoint);
					//					Debug.Log( !comprehensiveCollision( new Line( templistA[indA], templistB[indB] ), -100 ) );
					//					drawSphere( templistA[indA] );
					//					drawSphere( templistB[indB] );
					//					comprehensiveCollision( new Line( templistA[indA], templistB[indB] ), 100 );
				}
				//Case 1: LP to LP
				if( LL && LF && FL && FF  ){
					nextstartingpoint = "last";
					//Case 1b: Visibility points from different rays are colinear
					if( indB > 0 ){
						if( new Line( templistA[indA], templistB[indB] ).POL(templistB[0]) )
							nextstartingpoint = "first";
					}
				}
				//Case 2: Farthest connectable points
				else if( FF ){
					int connectorA = -1;
					int connectorB = -1;
					
					//Case 2A:
					//This is the most common case
					//We use this because visibility polygons are simple star polygons
					//and therefore will "zig-zag". Ordering of points (first-to-last,last-to-first)
					//in a ray will alternate each time. We only need to make sure anomalous single line extensions
					//are ignored.
					if( xid == 9 && cnt == 1 ){
						Debug.Log(nextstartingpoint);
					}
					if( nextstartingpoint != "none" ){
						if( nextstartingpoint == "first" ){
							//Find farthest possible point that can connect to the beginning of B
							for( int j = indA; j >= 0; j-- ){
								if( connectable( templistA[j], templistB[0] ) ){
									connectorA = j;
									break;
								}
							}
						}
						else{
							connectorA = 0;
						}
					}
					//Case 2B: Only happens on the first iteration
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
					
					//Find connector for ray B
					//If A[last] can reach B[j] then all above A[last] can reach B[j] and all above B[j]
					for( int j = indB; j >= 0; j-- ){
						if( connectable( templistA[connectorA], templistB[j] ) ){
							connectorB = j;
							break;
						}
					}
					//					if( xid == 9 && cnt == 1 ){
					//						Debug.Log(connectorA);
					//						Debug.Log(indA);
					//						Debug.Log(connectorB);
					//					}
					//If connector not the first or last point then delete all after it
					if( connectorA != 0 && connectorA < indA )
						templistA.RemoveRange( connectorA + 1, indA - (connectorA + 1) + 1);
					
					int anglistExtConnectorB = anglistExtIndBStart + connectorB;
					if( connectorB != 0 && connectorB < indB )
						anglistExt.RemoveRange( anglistExtConnectorB + 1, anglistExtIndB - (anglistExtConnectorB + 1) + 1);
					//Fault point 
					//templistB.RemoveRange( connectorB + 1, indB - (connectorB + 1) + 1);
					//If connector not the first point then reverse
					if( connectorA == 0 )
						templistA.Reverse();
					//Set marker for next one
					if( connectorB == 0 ) nextstartingpoint = "first";
					else nextstartingpoint = "last";
					//					if( xid == 9 && cnt == 1 )
					//						Debug.Log("connB " +  connectorB + "," + nextstartingpoint );
					
				}
				//Case 3: Two rays joined by kernel
				else{
					//if unconnectable, connect to kernel
					templistA.Reverse();
					addkernel = true;
					nextstartingpoint = "first";
				}
			}
			//Add the points of list A
			foreach( Vector3 v in templistA )
				vispol.Add( v );
			//If case 3 then add the kernel before moving on to next ray
			if( addkernel )
				vispol.Add( kernel );
		}
		
		//If kernel hasn't been added yet
		if (!vispol.Contains (kernel))
			vispol.Add (kernel);
		
		//Special case where first ray of iteration has anomalous extensions
		//TODO: Fix for case where anomalous extension may extend to more than one point
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
			if( xid == 9 ){
				//				drawSphere( v, Color.cyan, cnt++ );
				//				Line tmpline = new Line( kernel, v );			
				//				tmpline.name = "Line" + cnt.ToString();
				//				tmpline.DrawVector(GameObject.Find("temp"), Color.yellow);
			}
			
		}
		cnt = 0;
		Geometry VPret = new Geometry ();
		for( int i = 0; i < vispol.Count; i++ ){
			Line tmpline = new Line( kernel, vispol[i] );
			//if( xid == 0 ) drawSphere( vispol[i], Color.red, i ); 
			if( i == vispol.Count - 1 ){
				tmpline = new Line( vispol[i], vispol[0] );
			}
			else{
				tmpline = new Line( vispol[i], vispol[i + 1] );
			}
			tmpline.name = "Line" + cnt++;
			VPret.edges.Add(tmpline);
		}
		foreach (Line l in VPret.edges) {
			if( VectorApprox( l.vertex[0], l.vertex[1] ) )
				Debug.Log("GETTING equivalent points on VP edge on VP " + xid.ToString());
		}
		return VPret;
	}
	
	Geometry visibilityPolygonAlternate( Vector3 kernel, int xid ){
		List<Vector3> allpoints = totalGeo.GetVertex ();
		List<Vector3> vp = new List<Vector3> ();
		int cnt = 0;
		int tempcnt = 0;
		//Step 1 - Find all points that are initially visible
		foreach (Vector3 v in allpoints){
			Line tmpLine = new Line( kernel, v );
			//if( !comprehensiveCollision( tmpLine, 0 ) ){
			if( !comprehensiveCollisionEndPt( tmpLine, 0 ) )
				vp.Add ( v );			
		}
		if (vp.Count == 0)
			return new Geometry ();
		//Step 2- Sort the points by angle
		Geometry x = new Geometry ();
		List<KeyValuePair<Vector3, double>> anglist = x.GetVertexAngleSorted (kernel, vp);
		cnt = 0;
		foreach(KeyValuePair<Vector3, double> kvp in anglist) {
			if( xid == 81 ){
				//				drawSphere( kvp.Key, Color.red, kvp.Value );
				//				Line tmpline = new Line( kernel, kvp.Key );
				//				tmpline.name = "Line" + cnt + " " + kvp.Value;
				//				tmpline.DrawVector(GameObject.Find("temp"), Color.cyan);
			}
		}
		
		//Step 3 - Extend where applicable and find rest of the points
		List<KeyValuePair<Vector3, double>> anglistExt = new List<KeyValuePair<Vector3, double>> ();
		List<Vector3> uniqueList = new List<Vector3> ();
		cnt = -1;
		for( int i = 0; i < anglist.Count; i++ ){
			cnt++;
			Vector3 v = anglist[i].Key;
			double angle = anglist[i].Value;
			List<Vector3> tmplist = new List<Vector3>();
			if( v == kernel )
				continue;
			//Find how to shoot ray at angle x
			//Get new point
		}
		return new Geometry();
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
		float extensionfactor = 0.1f;
		if (tmpline.Magnitude () < extensionfactor)
			extensionfactor = tmpline.Magnitude();
		
		vB_new2d.x = vB2d.x + dirA2d.x / tmpline.Magnitude() * extensionfactor;
		vB_new2d.y = vB2d.y + dirA2d.y / tmpline.Magnitude() * extensionfactor;
		
		Vector3 vB_new = new Vector3( vB_new2d.x, 1, vB_new2d.y );
		bool collides = false;
		//2. Check if new endpoint is inside a geometry
		if( xid == -100 ){
			drawSphere (vA, Color.red, 0);
			drawSphere (vB, Color.magenta, 1);
			drawSphere (vB_new, Color.green, 2);
			//Debug.Log( mapBG.PointOutsideDebug(vB_new) );
			Debug.Log("Mag Orig: " + tmpline.Magnitude());
			tmpline = new Line (vB, vB_new);
			Debug.Log("Mag: " + tmpline.Magnitude());
		}
		tmpline = new Line (vB, vB_new);
		
		int cnt = 0;
		foreach( Geometry g in finalPoly ){
			if( g.PointInside(vB_new) || g.PointInside(tmpline.MidPoint())){ 
				collides = true;
				break;
			}
		}
		
		//3. Check if new endpoint is inside the map	
		if (!collides){
			if( xid == -99 ){
				//mapBG.PointOutsideDebug(vB_new);
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
			drawSphere (vB_new, Color.red, 3);
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
				//				if( xid == -100 )
				//					l1.DrawVector(GameObject.Find("temp"));
				Vector3 v = l1.GetIntersectionPoint(ray);
				if( VectorApprox( v, vB ) )	continue;
				//if( !ray.POL(v) ) continue;
				Line vamp = new Line( vB, v );
				if( vamp.Magnitude() < mindist ){
					mindist = vamp.Magnitude();
					retvect = v;
					//					if( xid == -100 ){
					//						drawSphere( retvect, Color.blue, 10);
					//						l1.DrawVector(GameObject.Find("temp"));
					//						ray.DrawVector(GameObject.Find("temp"));
					//						Debug.Log(l1.LineIntersectMuntacEndPtDebug(ray));
					//					}
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
		else{
			//			if( xid == -100 )
			//				drawSphere( retvect, Color.blue, 4);
			return new KeyValuePair< bool, Vector3 >(true, retvect);
		}
	}
	
	void getCameraVPS(){
		List<Vector3> tempcam = new List<Vector3> ();
		//Debug.Log ("Camera count is " + cameras.Count);
		int xid = 0;
		foreach (Vector3 v in explorationTour) {
			//if( cameras.Contains(v) && !tempcam.Contains(v) ){
			//			if( !tempcam.Contains(v) ){
			//				tempcam.Add(v);
			//if( xid == 81 )
			cameraVPS.Add ( new KeyValuePair<Vector3, Geometry>( v, visibilityPolygon( v, xid ) ) );
			xid++;
			//				//if( xid > 9 ) return;
			//				//return;
			//			}
		}
	}
	
	int VPValidation(){
		int cnt = 0;
		int invalid = 0;
		foreach (KeyValuePair<Vector3, Geometry> kvp in cameraVPS){
			cnt++;
			foreach( Line l in kvp.Value.edges ){
				if( comprehensiveCollision( l , 0 ) ){
					//					if( cnt == 92 )
					//					l.DrawVector(GameObject.Find("temp"));
					Debug.Log("Invalid VP at: " + cnt);
					//					kvp.Value.DrawGeometry(GameObject.Find("temp"));
					return 1;
					invalid++;
					break;
				}
			}
			//			for( int i = 0; i < kvp.Value.edges.Count; i++ ){
			//				bool invflag = false;
			//				for( int j = i + 1; j < kvp.Value.edges.Count; j++ ){
			//					int casetype = kvp.Value.edges[i].LineIntersectMuntacGM( kvp.Value.edges[j] );
			//					if( casetype != 0 ){
			//						Debug.Log("B Invalid VP at: " + cnt);
			//						//						kvp.Value.edges[i].DrawVector(GameObject.Find("temp"));
			//						//						kvp.Value.edges[j].DrawVector(GameObject.Find("temp"));
			//						//						Debug.Log(casetype);
			//						//						return 1;
			//						invflag = true;
			//						invalid++;
			//						break;
			//					}
			//				}
			//				if( invflag ) break;
			//			}
			//			cnt++;
		}
		if( invalid > 0 )
			Debug.Log (invalid + " invalid VPs found. Debug vispol function.");
		return invalid;
	}
	
	int UnionValidation(){
		int cnt = 0;
		int invalid = 0;
		foreach(KeyValuePair<Vector3, Geometry> kvp in cameraUnion) {
			foreach( Line l in kvp.Value.edges ){
				if( comprehensiveCollision( l , 0 ) ){
					//if( cnt == 0 ) l.DrawVector(GameObject.Find("temp"));
					Debug.Log("Invalid VP Union at: " + cnt);
					l.DrawVector(GameObject.Find("temp"));
					return 1;
					invalid++;
					break;
				}
			}
			for( int i = 0; i < kvp.Value.edges.Count; i++ ){
				bool invflag = false;
				for( int j = i + 1; j < kvp.Value.edges.Count; j++ ){
					int casetype = kvp.Value.edges[i].LineIntersectMuntacGM( kvp.Value.edges[j] );
					if( casetype != 0 ){
						Debug.Log("B Invalid VP Union at: " + cnt);
						kvp.Value.edges[i].DrawVector(GameObject.Find("temp"));
						kvp.Value.edges[j].DrawVector(GameObject.Find("temp"));
						if( kvp.Value.edges[i].Equals(kvp.Value.edges[j]) )
							Debug.Log("Equality");
						Debug.Log(casetype);
						return 1;
						invflag = true;
						invalid++;
						break;
					}
				}
				if( invflag ) break;
			}
			cnt++;
		}
		if( invalid > 0 )
			Debug.Log (invalid + " invalid VP Unions found. Debug vispol function.");
		return invalid;
	}
	
	void mergeVPS(){
		new GameObject ("vpA");
		new GameObject ("vpB");
		new GameObject ("vpMerged");
		
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
			if( i == 32 ){
				incrementalCover.DrawGeometry(GameObject.Find("vpA"));
				cameraVPS[i].Value.DrawGeometry(GameObject.Find("vpB"));
				for( int j = 0; j < incrementalCover.edges.Count; j++ ){
					for( int k = j + 1; k < incrementalCover.edges.Count; k++ ){
						if( incrementalCover.edges[j].Equals(incrementalCover.edges[k]) )
							Debug.Log("SAMENESSLINE");
					}
				}
				for( int j = 0; j < cameraVPS[i].Value.edges.Count; j++ ){
					for( int k = j + 1; k < cameraVPS[i].Value.edges.Count; k++ ){
						if( cameraVPS[i].Value.edges[j].Equals(cameraVPS[i].Value.edges[k]) )
							Debug.Log("SAMENESSLINEB");
					}
				}
				foreach( Line ll in cameraVPS[i].Value.edges ){
					if( VectorApprox( ll.vertex[0], ll.vertex[1] ) )
						Debug.Log("SAMENESSB");
				}
			}
			incrementalCover = incrementalCover.GeometryMerge(cameraVPS[i].Value, i);
			
			//if( i == 1 ) return;
			//if( i == x - 1 ) return;
			
			cameraVPS2.Add( new KeyValuePair<Vector3, Geometry>( cameraVPS[i].Key, incrementalCover ) );
			if( i == 40 ) break;
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
		double mapArea = mapBG.getPolygonArea (0);
		foreach( Geometry g in finalPoly )
			mapArea -= g.getPolygonArea(0);
		double finalCoverage = coverageAreas [coverageAreas.Count - 1];
		double percentCovered = (finalCoverage / mapArea) * 100;
		Debug.Log ("Area Calculated. " + Math.Round( percentCovered, 5 ) + "% of map explored.");
	}
	
	void printMapToPolyFile(){
		string createText = "";
		string path = path_start + @"\map.poly";
		string delimeter = " ";
		Dictionary<Vector3, int> dict = new Dictionary<Vector3, int> ();
		Dictionary<int, Vector3> numToVect = new Dictionary<int, Vector3> ();
		createText += totalGeoVerts.Count + " 2 0 0\n";
		int xid = 1;
		foreach(Vector3 v in totalGeoVerts) {
			dict.Add( v, xid );
			numToVect.Add( xid, v );
			createText += xid.ToString() + " " + v.x.ToString() + " " + v.z.ToString() + "\n";
			xid++;
		}
		int cnt = totalGeo.edges.Count + linesMinSpanTree.Count;
		createText +=  cnt.ToString() + " 0\n";
		xid = 1;
		foreach (Line l in totalGeo.edges){
			int x = dict[l.vertex[0]];
			int y = dict[l.vertex[1]];
			createText += xid++.ToString() + " " + x.ToString() + " " + y.ToString() + " 0\n";
		}
		foreach (Line l in linesMinSpanTree){
			int x = dict[l.vertex[0]];
			int y = dict[l.vertex[1]];
			createText += xid++.ToString() + " " + x.ToString() + " " + y.ToString() + " 0\n";
		}
		createText += finalPoly.Count.ToString () + "\n";
		xid = 1;
		foreach( Geometry g in finalPoly ){
			Vector3 v = g.PointInsidePolygon();
			if( v.x == 0 && v.y == 0 && v.z == 0 ) Debug.Log( "Failed to get points inside holes" );
			createText += xid++.ToString() + " " + v.x.ToString() + " " + v.z.ToString() + "\n";
		}
		createText += "0\n";
		File.WriteAllText(path, createText);
	}
	
	void printMST(){
		string createText = "";
		string path = path_start + @"\mst.csv";
		string delimeter = ",";
		foreach( Line l in linesMinSpanTree ){
			createText += l.vertex[0].x.ToString()+"," + l.vertex[0].z.ToString()
				+ "," + l.vertex[1].x.ToString() + "," + l.vertex[1].z.ToString() + ",\n";
		}
		File.WriteAllText(path, createText);
	}
	
	void printTour(){
		string createText = "";
		string path = path_start + @"\tour.csv";
		string delimeter = ",";
		for( int i = 0; i < explorationTour.Count; i++ ){
			int isCam = 0;
			if( cameras.Contains( explorationTour[i] ) ) isCam = 1;
			createText += (explorationTour[i].x).ToString()+","+(explorationTour[i].z).ToString()
				+ "," + isCam.ToString() + ",\n";
		}
		File.WriteAllText(path, createText);
	}
	
	void printVPS(){
		string createText = "";
		string path = path_start + @"\vps.csv";
		string delimeter = ",";
		for( int i = 0; i < cameraVPS.Count; i++ ){
			createText += "Start,"+ cameraVPS[i].Key.x.ToString() + "," + cameraVPS[i].Key.z.ToString() + ",\n";
			foreach( Line l in cameraVPS[i].Value.edges ){
				createText += "L," + l.vertex[0].x.ToString()+","+l.vertex[0].z.ToString()
					+ "," + l.vertex[1].x.ToString() + "," + l.vertex[1].z.ToString() + ",\n";
			}
			createText += "End,\n";
		}
		File.WriteAllText(path, createText);
	}
	
	void printCameras(){
		string createText = "";
		string path = path_start + @"\cameras.csv";
		string delimeter = ",";
		foreach( Vector3 v in cameras )
			createText += v.x.ToString() + "," + v.z.ToString() + ",\n";
		File.WriteAllText(path, createText);
	}
	
	void printSPRoadMap(){
		string createText = "";
		string path = path_start + @"\SPRoadMap.csv";
		string delimeter = ",";
		foreach( Line l in spRoadMap ){
			createText += "L," + l.vertex[0].x.ToString()+","+l.vertex[0].z.ToString()
				+ "," + l.vertex[1].x.ToString() + "," + l.vertex[1].z.ToString() + ",\n";
		}
		foreach( Vector3 v in masterReflex ){
			createText += "M," + v.x.ToString() + "," + v.z.ToString() + ",\n";
		}
		File.WriteAllText(path, createText);
	}

	void printTriRoadMap(){
		string createText = "";
		string path = path_start + @"\TriRoadMap.csv";
		string delimeter = ",";
		foreach( Line l in triRoadMap ){
			createText += "L," + l.vertex[0].x.ToString()+","+l.vertex[0].z.ToString()
				+ "," + l.vertex[1].x.ToString() + "," + l.vertex[1].z.ToString() + ",\n";
		}
		foreach( Vector3 v in triNonCameraNodes ){
			createText += "M," + v.x.ToString() + "," + v.z.ToString() + ",\n";
		}
		File.WriteAllText(path, createText);
	}
	
	void scanMST(){
		var reader = new StreamReader(File.OpenRead(path_start + @"\mst.csv"));
		List<string> coord = new List<string> ();
		Geometry walls = new Geometry ();
		
		while ( !reader.EndOfStream )
		{
			var line = reader.ReadLine();
			var values = line.Split(',');
			//Check the symbol in first column of line
			Vector3 expPointA = new Vector3();
			Vector3 expPointB = new Vector3();
			expPointA.x = float.Parse(values[0]);
			expPointA.y = 1;
			expPointA.z = float.Parse(values[1]);
			expPointB.x = float.Parse(values[2]);
			expPointB.y = 1;
			expPointB.z = float.Parse(values[3]);
			linesMinSpanTree.Add( new Line( expPointA, expPointB ) );
		}
	}
	
	void scanTriangulation(){
		triangles = new List<Triangle> ();
		
		Dictionary<Vector3, int> dict = new Dictionary<Vector3, int> ();
		Dictionary<int, Vector3> numToVect = new Dictionary<int, Vector3> ();
		int xid = 1;
		foreach(Vector3 v in totalGeoVerts) {
			dict.Add( v, xid );
			numToVect.Add( xid, v );
			xid++;
		}
		
		var reader = new StreamReader(File.OpenRead(path_start + @"\map.1.ele"));
		List<string> coord = new List<string> ();
		Geometry walls = new Geometry ();
		bool firstline = true;
		int cnt = 0;
		
		while ( !reader.EndOfStream )
		{
			var line = reader.ReadLine();
			if( firstline ){
				firstline = false;
				continue;
			}
			var values = line.Split(' ');
			
			int turn = 0;
			int x = -1, y = -1, z = -1;
			for( int i = 0; i < values.Length; i++ ){
				values[i].Trim();
				if( values[i] == "#" ) break;
				if( values[i] == "" ) continue;
				else{
					//if( turn != 0 ) Debug.Log(values[i]);
					if( turn == 1 ){
						x = int.Parse( values[i] );
					}
					else if( turn == 2 ){
						y = int.Parse( values[i] );
					}
					else if( turn == 3 ){
						z = int.Parse( values[i] );
						triangles.Add( new Triangle( numToVect[x], x, numToVect[y], y, numToVect[z], z ) );
						//						linesToAdd.Add( new Line( numToVect[x], numToVect[y] ) );
						//						linesToAdd.Add( new Line( numToVect[z], numToVect[y] ) );
						//						linesToAdd.Add( new Line( numToVect[x], numToVect[z] ) );
						break;
					}
					turn++;
				}
			}
		}
		//		foreach (Line l in linesToAdd) {
		//			l.DrawVector( GameObject.Find("temp"));
		//		}
		foreach (Triangle tt in triangles) {
			foreach (Triangle ttt in triangles) {
				if (tt == ttt)
					continue; 
				tt.ShareEdged (ttt, linesMinSpanTree);		
			}			
		}		
		triangulation.triangles = triangles;
	}
	
	void scanTour(){
		var reader = new StreamReader(File.OpenRead(path_start + @"\tour.csv"));
		List<string> coord = new List<string> ();
		Geometry walls = new Geometry ();
		
		while ( !reader.EndOfStream )
		{
			var line = reader.ReadLine();
			var values = line.Split(',');
			//Check the symbol in first column of line
			Vector3 expPoint = new Vector3();
			expPoint.x = float.Parse(values[0]);
			expPoint.y = 1;
			expPoint.z = float.Parse(values[1]);
			if( values[2].Equals("1") ){
				if( !cameras.Contains(expPoint) ){
					cameras.Add(expPoint);
					Debug.Log("New Camera Added");
				}
			}
			explorationTour.Add(expPoint);
		}
	}
	
	void scanVPS(){
		var reader = new StreamReader(File.OpenRead(path_start + @"\vps.csv"));
		List<string> coord = new List<string> ();
		Geometry vptemp = new Geometry ();
		Vector3 kernel = new Vector3 ();
		while ( !reader.EndOfStream ){
			var line = reader.ReadLine();
			var values = line.Split(',');
			//Check the symbol in first column of line
			if( values[0].Equals("End") )
				cameraVPS.Add( new KeyValuePair<Vector3, Geometry>( kernel, vptemp ) );
			else if( values[0].Equals("Start") ){
				vptemp = new Geometry();
				kernel.x = float.Parse(values[1]);
				kernel.y = 1;
				kernel.z = float.Parse(values[2]);
			}
			else{
				Vector3 a = new Vector3(); 
				a.x = float.Parse( values[1] ); a.y = 1f; a.z = float.Parse( values[2] );
				Vector3 b = new Vector3(); 
				b.x = float.Parse( values[3] ); b.y = 1f; b.z = float.Parse( values[4] );
				vptemp.edges.Add( new Line( a, b ) );
			}
		}
	}
	
	void scanSPRoadMap(){
		var reader = new StreamReader(File.OpenRead(path_start + @"\SPRoadMap.csv"));
		List<string> coord = new List<string> ();
		Geometry walls = new Geometry ();
		
		while ( !reader.EndOfStream )
		{
			var line = reader.ReadLine();
			var values = line.Split(',');
			//Check the symbol in first column of line
			if( values[0].Equals ("L") ){
				Vector3 expPointA = new Vector3();
				Vector3 expPointB = new Vector3();
				expPointA.x = float.Parse(values[1]);
				expPointA.y = 1;
				expPointA.z = float.Parse(values[2]);
				expPointB.x = float.Parse(values[3]);
				expPointB.y = 1;
				expPointB.z = float.Parse(values[4]);
				spRoadMap.Add(new Line(expPointA, expPointB));
			}
			else{
				Vector3 mrpoint = new Vector3();
				mrpoint.x = float.Parse(values[1]);
				mrpoint.y = 1;
				mrpoint.z = float.Parse(values[2]);
				masterReflex.Add( mrpoint );
			}
		}
	}

	void scanTriRoadMap(){
		var reader = new StreamReader(File.OpenRead(path_start + @"\TriRoadMap.csv"));
		List<string> coord = new List<string> ();
		Geometry walls = new Geometry ();
		
		while ( !reader.EndOfStream )
		{
			var line = reader.ReadLine();
			var values = line.Split(',');
			//Check the symbol in first column of line
			if( values[0].Equals ("L") ){
				Vector3 expPointA = new Vector3();
				Vector3 expPointB = new Vector3();
				expPointA.x = float.Parse(values[1]);
				expPointA.y = 1;
				expPointA.z = float.Parse(values[2]);
				expPointB.x = float.Parse(values[3]);
				expPointB.y = 1;
				expPointB.z = float.Parse(values[4]);
				triRoadMap.Add(new Line(expPointA, expPointB));
			}
			else{
				Vector3 mrpoint = new Vector3();
				mrpoint.x = float.Parse(values[1]);
				mrpoint.y = 1;
				mrpoint.z = float.Parse(values[2]);
				triNonCameraNodes.Add(mrpoint);
			}
		}	
	}
	
	void scanCameras(){
		var reader = new StreamReader(File.OpenRead(path_start + @"\cameras.csv"));
		List<string> coord = new List<string> ();
		Geometry walls = new Geometry ();
		
		while ( !reader.EndOfStream )
		{
			var line = reader.ReadLine();
			var values = line.Split(',');
			//Check the symbol in first column of line
			Vector3 expPointA = new Vector3();
			Vector3 expPointB = new Vector3();
			expPointA.x = float.Parse(values[0]);
			expPointA.y = 1;
			expPointA.z = float.Parse(values[1]);
			cameras.Add(expPointA);
		}
	}

	private void externalTour( int roadmap ){
		if( roadmap == sp )
			makeTourOnSPR (true, 1);
		else if( roadmap == tri )
			makeTourOnTri (true, 1);
		string createText = "";
		string path = path_start + @"\mapwithtourall.csv";
		string delimeter = ",";
		List<Line> lsorted = new List<Line> ();
		//Print Map
		lsorted = mapBG.getSortedEdges();
		for( int i = 0; i < lsorted.Count; i++ ){
			createText += lsorted[i].vertex[1].x.ToString() + "," + lsorted[i].vertex[1].z.ToString()
				+ ",\n";
		}
		createText += "M,\n";
		//Print Obstacles
		foreach( Geometry g in finalPoly ){
			lsorted = g.getSortedEdges();
			for( int i = 0; i < lsorted.Count; i++ ){
				createText += lsorted[i].vertex[1].x.ToString() + "," + lsorted[i].vertex[1].z.ToString()
					+ ",\n";
			}
			createText += "H,\n";
		}

		List<Vector3> tourall = new List<Vector3> ();

		if( roadmap == sp ){
			foreach ( Vector3 v in masterReflex )
				if( !tourall.Contains( v ) ) tourall.Add( v );
		}
		else if( roadmap == tri ){
			foreach ( Vector3 v in triNonCameraNodes )
				if( !tourall.Contains( v ) ) tourall.Add( v );
		}
		foreach (Vector3 v in cameras )
			if( !tourall.Contains( v ) ) tourall.Add( v );
		//Add points all points that can be used for building a tour
		foreach (Vector3 v in tourall)
			createText += "T," + v.x.ToString() + "," + v.z.ToString() + ",\n";
		File.WriteAllText(path, createText);
	}

	public void printDijkstra( int GSC, int N, Dictionary<int,Vector3> numToVect ){
		string createText = "";
		string path = path_start + @"\dijkstra.csv";
		string delimeter = ",";
		File.WriteAllText(path, createText);
		int [,] isVisible = new int [N + 10, N + 10];
		for( int i = 0; i < N + 10; i++ )
			for( int j = 0; j < N + 10; j++ )
				isVisible[i,j] = 0;
		
		for (int i = 0; i < N; i++) {
			for (int j = i + 1; j < N; j++) {
				Vector3 v1 = numToVect[i];
				Vector3 v2 = numToVect[j];
				Line ln = new Line( v1, v2 );
				if( !comprehensiveCollision( ln, 0 ) ){
					isVisible[i,j] = 1;
					isVisible[j,i] = 1;
				}
			}
		}

		//List<Line> lsorted = new List<Line> ();
		for( int i = 0; i < N; i++ ){
			int iscam = 0;
			if( cameras.Contains(numToVect[i]) ) iscam = 1;
			createText += "Index," + i.ToString() + "," + iscam.ToString() + "," + numToVect[i].x.ToString()
				+ "," + numToVect[i].z.ToString() + ",\n";
			createText += "d";
			for( int j = 0; j < N; j++ )
				createText += "," + d[i][j].ToString();			
			File.AppendAllText( path, createText );
			createText = "";
			createText += ",\n";
			createText += "parents";
			for( int j = 0; j < N; j++ )
				createText += "," + parents[i][j].ToString();
			createText += ",\n";
			File.AppendAllText( path, createText );
			createText = "";
			createText += "isvisible";
			for( int j = 0; j < N; j++ )
				createText += "," + isVisible[i,j].ToString();
			createText += ",\n";
			File.AppendAllText( path, createText );
			createText = "";
		}
	}


	public void externalVP(){
		string createText = "";
		string path = path_start + @"\mapwithtour.csv";
		string delimeter = ",";
		List<Line> lsorted = new List<Line> ();
		//Print Map
		lsorted = mapBG.getSortedEdges();
		for( int i = 0; i < lsorted.Count; i++ ){
			createText += lsorted[i].vertex[1].x.ToString() + "," + lsorted[i].vertex[1].z.ToString()
				+ ",\n";
		}
		createText += "M,\n";
		//Print Obstacles
		foreach( Geometry g in finalPoly ){
			lsorted = g.getSortedEdges();
			for( int i = 0; i < lsorted.Count; i++ ){
				createText += lsorted[i].vertex[1].x.ToString() + "," + lsorted[i].vertex[1].z.ToString()
					+ ",\n";
			}
			createText += "H,\n";
		}
		//Add tour points
		foreach (Vector3 v in explorationTour)
			createText += "T," + v.x.ToString() + "," + v.z.ToString() + ",\n";
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
		if( xid == -100 ){
			//return true;
			Debug.Log("CompColl Debug");
		}
		//1.Geometry Check
		int dummy = 0;
		foreach( Geometry g in finalPoly ){
			//1A. Crude collision check (diagonal and line equality)
			if( g.LineCollision(tmpLine) ){
				if( xid == -100 ){
					tmpLine.DrawVector(GameObject.Find("temp"));
					Debug.Log("Mark 1");
					Debug.Log(g.PointInside( tmpLine.MidPoint() ));
					foreach( Line l in g.edges ){
						if( tmpLine.LineIntersectMuntac(l) == 1 ){
							//tmpLine.LineIntersectMuntacDebug(l);
							drawSphere(tmpLine.GetIntersectionPoint(l));
							//l.DrawVector(GameObject.Find("temp"));
							break;
						}
					}
				}
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
						if( xid == -100 )
							Debug.Log("Mark 2");
						return true;
					}
					foreach( Vector3 vB in endptIntersection ){
						if( vA.Equals(vB) ) continue;
						Line testline = new Line( vA, vB );
						if( g.PointInside( testline.MidPoint() ) ){
							//							if (xid == 86)
							//								Debug.Log ("Got 86 testline");
							if( xid == -100 )
								Debug.Log("Mark 3");
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
		if( xid == -100 )
			Debug.Log("Mark Boundary");
		List<Vector3> endpointinterMap = new List<Vector3>();
		foreach( Line l in mapBG.edges ){
			//2A. Crude intersection check
			if( l.LineIntersectMuntac( tmpLine ) == 1 ){
				if( xid == -100 ){
					Debug.Log("Mark 4");
					tmpLine.DrawVector(GameObject.Find("temp"));
					l.DrawVector(GameObject.Find("temp"));
					//l.LineIntersectMuntacDebug( tmpLine );
					Debug.Log( "POL:" + l.POL( tmpLine.vertex[0] ) + "," + l.POL(tmpLine.vertex[1]) );
				}
				return true;
			}
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
			if( mapBG.PointOutside( TLA.MidPoint() ) || mapBG.PointOutside( TLB.MidPoint() ) ){
				if( xid == -100 )
					Debug.Log("Mark 5");
				return true;
			}
			foreach( Vector3 vB in endpointinterMap ){
				if( vA.Equals(vB) ) continue;
				Line testline = new Line( vA, vB );
				if( mapBG.PointOutside( testline.MidPoint() ) ){
					if( xid == -100 )
						Debug.Log("Mark 6");
					return true;
				}
			}
		}
		if( xid == -100 )
			Debug.Log("Mark False");
		return false;
	}
	
	bool comprehensiveCollisionEndPt( Line tmpLine, int xid ){
		if( xid == -100 ){
			//return true;
			Debug.Log("CompColl Debug");
		}
		//1.Geometry Check
		int dummy = 0;
		foreach( Geometry g in finalPoly ){
			//1A. Crude collision check (diagonal and line equality)
			if( g.LineCollisionEndPt(tmpLine) ){
				if( xid == -100 )
					Debug.Log("Mark 1");
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
						if( xid == -100 )
							Debug.Log("Mark 2");
						return true;
					}
					foreach( Vector3 vB in endptIntersection ){
						if( vA.Equals(vB) ) continue;
						Line testline = new Line( vA, vB );
						if( g.PointInside( testline.MidPoint() ) ){
							//							if (xid == 86)
							//								Debug.Log ("Got 86 testline");
							if( xid == -100 )
								Debug.Log("Mark 3");
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
		if( xid == -100 )
			Debug.Log("Mark Boundary");
		List<Vector3> endpointinterMap = new List<Vector3>();
		foreach( Line l in mapBG.edges ){
			//2A. Crude intersection check
			if( l.LineIntersectMuntacEndPt( tmpLine ) == 1 ){
				if( xid == -100 ){
					Debug.Log("Mark 4");
					tmpLine.DrawVector(GameObject.Find("temp"));
					l.DrawVector(GameObject.Find("temp"));
					//l.LineIntersectMuntacDebug( tmpLine );
					Debug.Log( "POL:" + l.POL( tmpLine.vertex[0] ) + "," + l.POL(tmpLine.vertex[1]) );
				}
				return true;
			}
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
			if( mapBG.PointOutside( TLA.MidPoint() ) || mapBG.PointOutside( TLB.MidPoint() ) ){
				if( xid == -100 )
					Debug.Log("Mark 5");
				return true;
			}
			foreach( Vector3 vB in endpointinterMap ){
				if( vA.Equals(vB) ) continue;
				Line testline = new Line( vA, vB );
				if( mapBG.PointOutside( testline.MidPoint() ) ){
					if( xid == -100 )
						Debug.Log("Mark 6");
					return true;
				}
			}
		}
		if( xid == -100 )
			Debug.Log("Mark False");
		return false;
	}
	
	public bool doubleCompare ( double a, double b ){
		return System.Math.Abs (a - b) < (double)eps;
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