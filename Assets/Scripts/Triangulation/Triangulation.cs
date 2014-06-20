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
	// Use this for initialization

	public List<Triangle> triangles = new List<Triangle>(); 
	public List<Line> lines = new List<Line>(); 

	public List<Line> linesMinSpanTree = new List<Line>(); 
	public List<Geometry> obsGeos = new List<Geometry> (); 
	//Contains Map
	public Geometry mapBG = new Geometry ();

	public bool drawTriangles = false; 
	public bool drawRoadMap = false; 
	private bool drawMinSpanTree = false;
	public bool stopAll = false;

	public void Start(){
	
	}

	public void Clear()
	{
		linesMinSpanTree.Clear(); 
		triangles.Clear(); 
		lines.Clear(); 
		points.Clear(); 
		colours.Clear();
	
		GameObject temp = GameObject.Find("temp"); 
		DestroyImmediate(temp); 


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

		//return; 
		//points.Clear(); 
		//colours.Clear();  
		if ( stopAll )
			return;

		/***
		if(drawMinSpanTree)
		{
			foreach(Line l in linesMinSpanTree)
			{
				l.DrawLine(Color.blue); 
			}
		}

		foreach(Triangle tt in triangles)
		{
			//triangulation.points.Add(tt.GetCenterTriangle());
			//triangulation.colours.Add(Color.cyan); 
			if(drawTriangles)
			{	

				tt.DrawDebug();
				foreach(Vector3 v in tt.getVertexMiddle())
				{
				//	points.Add(v);
				}

				foreach(Color v in tt.colourVertex)
				{
				//	colours.Add(v);
				}
			}

			if(drawRoadMap)
			{
				Line[] ll = tt.GetSharedLines(); 
			

				if(ll.Length == 1)
				{
					Debug.DrawLine(ll[0].MidPoint(), tt.GetCenterTriangle(),Color.red);
					//Debug.Log("Drawing Red Line at: " + ll[0].MidPoint() + " " + tt.GetCenterTriangle());
				}
				else if(ll.Length > 2)
				{
					for(int i = 0; i<ll.Length; i++)
					{
						Debug.DrawLine(ll[i].MidPoint(), tt.GetCenterTriangle(),Color.red);
						//Debug.Log("Drawing Red Line at: " + ll[i].MidPoint() + " " + tt.GetCenterTriangle());
					}

				}
				
				else
				{
					for(int i = 0; i<ll.Length; i++)
					{
						Debug.DrawLine(ll[i].MidPoint(), ll[(i+1) % ll.Length].MidPoint(),Color.red);
					}
				}
			}
		}
		***/
	}

	public void AddPoint(Vector3 v)
	{
		points.Add(v); 
		colours.Add(Color.cyan); 
	}

	public void AddPoint(Vector3 v,Color c)
	{
		points.Add(v); 
		colours.Add(c); 
	}

	public void TriangulationSpace ()
	{
		//Compute one step of the discritzation
		//Find this is the view
		GameObject floor = (GameObject)GameObject.Find ("Floor");
			
		Vector3 [] vertex = new Vector3[4]; 
		
		//First geometry is the outer one
		List<Geometry> geos = new List<Geometry> ();

		
		//Drawing lines
		//VectorLine.SetCamera3D(Camera.current); 

		//Floor
		Vector3[] f = new Vector3[4];
		MeshFilter mesh = (MeshFilter)(floor.GetComponent ("MeshFilter"));
		Vector3[] t = mesh.sharedMesh.vertices; 
		
		Geometry tempGeometry = new Geometry (); 

		
		vertex [0] = mesh.transform.TransformPoint (t [0]);
		vertex [2] = mesh.transform.TransformPoint (t [120]);
		vertex [1] = mesh.transform.TransformPoint (t [110]);
		vertex [3] = mesh.transform.TransformPoint (t [10]);
		
		vertex [0].y = 1; 
		vertex [1].y = 1; 
		vertex [2].y = 1; 
		vertex [3].y = 1; 
		//these were in tempGeometry previously

		//Disabled Temporarily - Find a way to avoid floor when checking for obstacle collision
		//geos.Add (tempGeometry);

		Vector3 [] mapBoundary = new Vector3[4]; 

		for (int i = 0; i < 4; i++) {
			mapBoundary [i] = vertex [i];
		}

		Geometry mapBG = new Geometry (); 
		for (int i = 0; i < 4; i++)
			mapBG.edges.Add( new Line( mapBoundary[i], mapBoundary[(i + 1) % 4]) );
		//mapBG.DrawVertex (GameObject.Find ("temp"));
		//mapBG.DrawGeometry(GameObject.find);

		GameObject[] obs = GameObject.FindGameObjectsWithTag ("Obs");
		if(obs == null)
		{
			Debug.Log("Add tag geos to the geometries"); 
			return; 
		}
		//data holder
		Triangulation triangulation = GameObject.Find ("Triangulation").GetComponent<Triangulation> (); 
		triangulation.points.Clear ();
		triangulation.colours.Clear (); 
		
		//Only one geometry for now
		
		foreach (GameObject o in obs) {
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
			//tempGeometry.BoundGeometry(mapBoundary);
			geos.Add (tempGeometry); 
			
		}
		
		//lines are defined by all the points in  obs
		//List<Line> lines = new List<Line> (); 
		lines = new List<Line> ();

		/*foreach (Geometry g in geos) {
			for (int i = 0; i< g.vertex.Length; i+=1) {
				if (i < g.vertex.Length - 1)
					lines.Add (new Line (g.vertex [i], g.vertex [i + 1]));
				else 	       
					lines.Add (new Line (g.vertex [0], g.vertex [i]));
				}
		}*/


		obsGeos.Clear ();
		obsGeos = geos;


		//Create empty GameObject
		GameObject temp = GameObject.Find("temp");
		DestroyImmediate(temp);
		temp = new GameObject("temp");
		//CODESPACE
		//Merging Polygons
		for (int i = 0; i < obsGeos.Count; i++) {
			for (int j = i + 1; j < obsGeos.Count; j++) {
				//check all line intersections
				if( obsGeos[i].GeometryIntersect( obsGeos[j] ) ){
					//Debug.Log("Geometries Intersect: " + i + " " + j);
					tempGeometry = obsGeos[i].GeometryMerge( obsGeos[j] );
					//remove item at position i, decrement i since it will be increment in the next step, break
					obsGeos.RemoveAt(j);
					obsGeos.RemoveAt(i);
					obsGeos.Add(tempGeometry);
					i--;
					break;
				}
			}
		}

		List<Geometry> finalPoly = new List<Geometry> ();//Contains all polygons that are fully insde the map
		foreach ( Geometry g in obsGeos ) {
			if( mapBG.GeometryIntersect( g ) && !mapBG.GeometryInside( g ) ){
				mapBG = mapBG.GeometryMergeInner( g );
				mapBG.BoundGeometry( mapBoundary );
			}
			else
				finalPoly.Add(g);
		}

		//Drawing merged polygons and modified map
		foreach (Geometry g in finalPoly) {
			//g.DrawGeometry(GameObject.Find("temp"));
		}
		//mapBG.DrawGeometry (GameObject.Find ("temp"));

		List<Vector3> allVertex = new List<Vector3>();
		List<Vector3> tempVertex = new List<Vector3>();

		//Getting all vertices
		foreach (Geometry g in finalPoly) {
			tempVertex = g.GetVertex();
			foreach( Vector3 v in tempVertex )
				allVertex.Add(v);
		}

		tempVertex = mapBG.GetVertex();
		foreach( Vector3 v in tempVertex )
			allVertex.Add(v);

		/*foreach(Vector3 v in allVertex)
		{
			GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			inter.transform.position = v;
			inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f); 
			inter.transform.parent = temp.transform;
		}*/
		//Constructing "lines" for triangulation

		lines.Clear ();

		foreach (Geometry g in finalPoly) {
			//g.DrawGeometry(GameObject.Find("temp"));
			foreach( Line L in g.edges )
				lines.Add( L );
		}
		foreach (Vector3 Va in allVertex) {
			foreach(Vector3 Vb in allVertex){
				if( Va != Vb ){
					//Debug.Log("Here");
					bool collides = false;
					Line tempLine = new Line(Va, Vb);

					//A. Collision check with border
					if( mapBG.GeometryLineIntersect( tempLine ) )
						continue;

					//B. Collision check with internal polygons
					foreach( Geometry g in finalPoly ){
						if( g.GeometryLineIntersect( tempLine ) || g.LineInside( tempLine ) ){
							collides = true;
							break;
						}
					}
					if( collides ) continue;

					//C. Collision check with other triangualtion clines
					foreach( Line L in lines ){
						if( L.LineIntersectMuntac( tempLine ) > 0 ){
							collides = true;
							break;
						}
					}

					//D. Collision check inside obstacle
					if( !collides ){
						//tempGeometry.edges.Add(tempLine);
						//Debug.Log("Hello");
						lines.Add( tempLine );
					}
				}
			}
		}
		foreach ( Line l in mapBG.edges ) {
			lines.Add(l);
		}
		foreach (Line L in lines) {
			L.DrawVector(temp);
		}
//
//		foreach (Line l in lines)
//			l.DrawLine(

		///Uncomment the following later

		//Lines are also the one added. 

		//Compare each point to every point 

		/*
		for (int i = 0; i < geos.Count; i++) {
			
			for (int j = i+1; j < geos.Count; j++) {
				
				for (int w = 0; w<geos[i].vertex.Length; w++) {
					
					for (int z = 0; z<geos[j].vertex.Length; z++) {
						
						List<Line> toAdd = new List<Line> (); 
						
						Boolean foundBreak = false; 
						
						foreach (Line l in lines) {
							
							if (LineIntersection (geos [i].vertex [w], geos [j].vertex [z],
							                      l.vertex [0], l.vertex [1])) {
								
								foundBreak = true; 
								break; 
							}								
							
						}
						if (!foundBreak) {	
							//Debug.DrawLine(geos[i].vertex[w], geos[j].vertex[z], Color.blue);
							lines.Add (new Line (geos [i].vertex [w], geos [j].vertex [z])); 		
						}	
					}
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
				
				
				if (l2.vertex [0].Equals (v2)) {
					v3 = l2.vertex [1];
					//have to check if closes
				} else if (l2.vertex [1].Equals (v2)) {
					v3 = l2.vertex [0];
				}
				
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
				tt.ShareEdged (ttt);
				
			}
			
		}
		
		triangulation.triangles = triangles;

		*/
	}

	private Vector3 LineIntersectVect (Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		//Debug.Log(a); 
		//Debug.Log(b); 
		//Debug.Log(c); 
		//Debug.Log(d); 
		
		Vector2 u = new Vector2 (b.x, b.z) - new Vector2 (a.x, a.z);
		Vector2 p0 = new Vector2 (a.x, a.z);
		Vector2 p1 = new Vector2 (b.x, b.z); 
		
		Vector2 v = new Vector2 (d.x, d.z) - new Vector2 (c.x, c.z);
		Vector2 q0 = new Vector2 (c.x, c.z);
		Vector2 q1 = new Vector2 (d.x, d.z);
		
		Vector2 w = new Vector2 (a.x, a.z) - new Vector2 (d.x, d.z);
		
		
		//if (u.x * v.y - u.y*v.y == 0)
		//	return true;
		
		double s = (v.y * w.x - v.x * w.y) / (v.x * u.y - v.y * u.x);
		double t = (u.x * w.y - u.y * w.x) / (u.x * v.y - u.y * v.x); 
		//Debug.Log(s); 
		//Debug.Log(t); 
		
		//if ((s > 0 && s < 1) || (t > 0 && t < 1))
		//{
		//Interpolation
		Vector3 r = a + (b-a)*(float)s; 
		return r; 
		//}
		
		
		
		//return Vector3.zero; 
	}
	/*private Boolean LineIntersection (Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		
		
		
		// a-b
		// c-d
		//if the same lines
		
		//When share a point use the other algo
		if (a.Equals (c) || a.Equals (d) || b.Equals (c) || b.Equals (d))
			return LineIntersect (a, b, c, d); 
		
		
		
		
		return CounterClockWise (a, c, d) != CounterClockWise (b, c, d) && 
			CounterClockWise (a, b, c) != CounterClockWise (a, b, d);
		
		//if( CounterClockWise(a,c,d) == CounterClockWise(b,c,d))
		//	return false;
		//else if (CounterClockWise(a,b,c) == CounterClockWise(a,b,d))
		//	return false; 
		//else 
		//	return true; 
		
		
	}*/

	//Checks if two edges are meeting regularly at a vertex of the polygon or if they are intersecting
	//in other manners


	private Boolean CounterClockWise (Vector3 v1, Vector3 v2, Vector3 v3)
	{
		//v1 = a,b
		//v2 = c,d
		//v3 = e,f
		
		float a = v1.x, b = v1.z;  
		float c = v2.x, d = v2.z;  
		float e = v3.x, f = v3.z;  
		
		if ((f - b) * (c - a) > (d - b) * (e - a))
			return true;
		else
			return false; 
	}

	public class sortPointsX : IComparer<Vector3>
	{
		public int Compare(Vector3 a, Vector3 b)
		{
			if (a.x > b.x) return 1;
			else if (a.x < b.x) return -1;
			else return 0;
		}
	}

	public class sortPointsY : IComparer<Vector3>
	{
		public int Compare(Vector3 a, Vector3 b)
		{
			if (a.y > b.y) return 1;
			else if (a.y < b.y) return -1;
			else return 0;
		}
	}

	private static void Swap<Vector3>(ref Vector3 lhs, ref Vector3 rhs){
		Vector3 temp;
		temp = lhs;
		lhs = rhs;
		rhs = temp;
	}
}
