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
	public List<int>[] G = new List<int>[110];
	public int[] colorG = new int[110];
	public bool[] visitedG = new bool[110];
	public const int red = 1;
	public const int green = 2;
	public const int blue = 3;

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

		Vector3 [] mapBoundary = new Vector3[4]; //the map's four corners

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
			geos.Add (tempGeometry); 
		}
		
		//lines are defined by all the points in  obs
		lines = new List<Line> ();

		obsGeos.Clear ();
		foreach (Geometry g in geos) {
			obsGeos.Add(g);
		}


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
					Geometry tmpG = obsGeos[i].GeometryMerge( obsGeos[j] ); 
					//remove item at position i, decrement i since it will be increment in the next step, break
					obsGeos.RemoveAt(j);
					obsGeos.RemoveAt(i);
					obsGeos.Add(tmpG);
					i--;
					break;
				}
			}
		}
//		mapBG.DrawGeometry (temp);
		List<Geometry> finalPoly = new List<Geometry> ();//Contains all polygons that are fully insde the map
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
		Geometry totalGeo = new Geometry ();

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
//		foreach(Vector3 v in allVertex)
//		{
//			GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//			inter.transform.position = v;
//			inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f); 
//			inter.transform.parent = temp.transform;
//			inter.gameObject.name = vlcnt.ToString();
//			++vlcnt;
//		}

		lines.Clear ();

		//Constructing "lines" for triangulation

		Debug.Log (allVertex.Count);
		int iv = -1, jv;
		vlcnt = 0;
		foreach (Vector3 Va in allVertex) {
			++iv;
			jv = -1;
			foreach(Vector3 Vb in allVertex){
				++jv;
				if( Va != Vb ){
					bool collides = false, essential = false;
					Line tempLine = new Line(Va, Vb);
					//A-Collision with final polygon
					foreach( Line l in totalGeo.edges ){
						if( l.LineIntersectMuntacEndPt( tempLine ) == 1 ){
//							if( iv == 16 && jv == 18 ){	
//								++vlcnt;
//								l.name = vlcnt.ToString();
//								l.DrawVector(temp);
//								tempLine.name = l.name + "DUP";
//								tempLine.DrawVector(temp);
//								Debug.Log("Here 1");
//							}
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

//		foreach (Line L in lines)
//			L.DrawVector(temp);
//		Debug.Log ("Total Lines" + lines.Count);
				
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


		List<Vector3> verts = new List<Vector3> ();
		foreach (Line L in lines) {
			if( !verts.Contains(L.vertex[0]) )
				verts.Add(L.vertex[0]);
			if( !verts.Contains(L.vertex[1]) )
				verts.Add(L.vertex[1]);
		}
		for (int i = 0; i < 100; i++)
			G [i] = new List<int> ();
		foreach (Line L in lines) {
			//int indU = FindIndexManual( verts, L.vertex[0] );
			int indU = verts.IndexOf(L.vertex[0]);
			int indV = FindIndexManual( verts, L.vertex[1] );
			G[indU].Add(indV);
			G[indV].Add(indU);
		}
		int total = 0;
		for (int i = 0; i < 100; i++) {
			if( G[i].Count == 0 ) break;
			total += G[i].Count;
		}
		Debug.Log ("Total: " + total);
		Debug.Log ("Lines: " + lines.Count);
		colorG = new int[110];
		visitedG = new bool[110];
		for (int i = 0; i < 100; i++) {
			colorG [i] = -1;
			visitedG[i] = false;
		}
		TriColor ( 0 );
		//DrawVertices ( temp );
//		Debug.Log (verts.Count);
//		for (int i = 0; i < verts.Count; i++) {
//			GameObject inter = GameObject.CreatePrimitive (PrimitiveType.Sphere);
//			inter.transform.position = verts[i];
//			if( colorG[i] == red )
//				inter.transform.renderer.material.color = Color.red;
//			else if( colorG[i] == green )
//				inter.transform.renderer.material.color = Color.green;
//			else
//				inter.transform.renderer.material.color = Color.blue;
//			inter.transform.localScale = new Vector3 (0.3f, 0.3f, 0.3f); 
//			inter.transform.parent = temp.transform;
//		}
		int numRed = 0, numGreen = 0, numBlue = 0;
		for (int i = 0; i < verts.Count; i++) {
			if( colorG[i] == red ) numRed++;
			else if( colorG[i] == green ) numGreen++;
			else numBlue++;
		}
		int minColor;
		if (numRed <= numGreen && numRed <= numBlue)
			minColor = red;
		else if (numGreen <= numRed && numGreen <= numBlue)
			minColor = green;
		else
			minColor = blue;
		for (int i = 0; i < verts.Count; i++) {
			//if( colorG[i] != minColor )
			//	continue;
			GameObject inter = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			inter.transform.position = verts[i];
			inter.transform.localScale = new Vector3 (0.3f, 0.3f, 0.3f); 
			if( colorG[i] == minColor ){
				inter.transform.renderer.material.color = Color.red;
				inter.transform.localScale = new Vector3 (0.7f, 0.7f, 0.7f); 
			}
			else
				inter.transform.renderer.material.color = Color.green;

			inter.transform.parent = temp.transform;
		}
	}

	void TriColor( int source ){
		if( colorG[source] == -1 )
			colorG[source] = red;
		int u = source;
		visitedG [u] = true;
		foreach (int v in G[source]) {
			if( colorG[v] == -1 )
				colorVertex( v );
		}
		foreach (int v in G[source]) {
			if( visitedG[v] == false )
				TriColor( v );
		}
	}

	void colorVertex( int node ){
		for (int currColor = 1; currColor <= 3; currColor++) {
			bool available = true;
			foreach( int v in G[node] ){
				if( colorG[v] == currColor ){
					available = false;
					break;
				}
			}
			if( available ){
				colorG[node] = currColor;
				break;
			}
		}
	}

	int FindIndexManual( List<Vector3> L, Vector3 V ){
		int ind = -1;
		foreach( Vector3 X in L ){
			++ind;
			if( X == V )
				break;
		}
		return ind;
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
