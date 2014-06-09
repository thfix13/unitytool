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

	public bool drawTriangles = false; 
	public bool drawRoadMap = false; 
	private bool drawMinSpanTree = false;


	public void Clear()
	{
		linesMinSpanTree.Clear(); 
		triangles.Clear(); 
		lines.Clear(); 
		points.Clear(); 
		colours.Clear();
	}
	void OnDrawGizmosSelected() 
	{
		return; 
		//Debug.Log(colours.Count);
		//Debug.Log(points.Count);
		var i = 0;
		foreach(Vector3 v in points)
		{

			Gizmos.color = colours[i];
			//Gizmos.color = Color.red;
			Gizmos.DrawSphere (v, 0.25f);
			i++; 
		}

		//Gizmos.color = Color.red;
		//Gizmos.DrawSphere (new Vector3(0,2,0), 1);
	}
	public void Update()
	{

		//return; 
		//points.Clear(); 
		//colours.Clear();  

		

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
					Debug.DrawLine(ll[0].MidPoint(), tt.GetCenterTriangle(),Color.blue);

				}
				else if(ll.Length == 3)
				{
					for(int i = 0; i<ll.Length; i++)
					{
						Debug.DrawLine(ll[i].MidPoint(), tt.GetCenterTriangle(),Color.blue);
					}
				}
				
				else
				{
					for(int i = 0; i<ll.Length; i++)
					{
						Debug.DrawLine(ll[i].MidPoint(), ll[(i+1) % ll.Length].MidPoint(),
							Color.red);
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
	
	public void ShowTriangulation()
	{
		GameObject dataCurve = GameObject.Find("DataPath");
		GameObject temp = GameObject.Find("temp");
		

		
			
		if (dataCurve == null || 
			dataCurve.GetComponent<PathsHolder>().paths.Count==0)
		{
			Debug.Log("No curves");
			return; 
		}

		Vector3[] path1 = dataCurve.GetComponent<PathsHolder>().paths[0].getPoints3DFlat();
		Vector3[] path2 = dataCurve.GetComponent<PathsHolder>().paths[1].getPoints3DFlat();


		
		float area = TriangulationCurves(path1, path2, true);
		Debug.Log("Area: " + area);
	}
	
	public float TriangulationCurves(Vector3[] path1, Vector3[] path2, bool display = false)
	{
		GameObject temp = GameObject.Find("temp");
		if(temp != null)
			GameObject.DestroyImmediate(temp);
		
		temp = new GameObject("temp");
				
		//Draw the two paths:

		VectorLine line1 = new VectorLine("1",path1,Color.red,null,10.0f);

		line1.Draw3D();

		line1.vectorObject.transform.parent = temp.transform;

		//Second line

		VectorLine line2 = new VectorLine("2",path2,Color.blue,null,10.0f);
		
		line2.Draw3D();
		
		line2.vectorObject.transform.parent = temp.transform;
		
		//Constructing the geometry
		//Find all the vectors that are colliding and there position. 
		lines.Clear(); 

		//From the persceptive p1 to p2
		foreach(Line l in GetLines(path1,path2))
			lines.Add(l);
		foreach(Line l in GetLines(path2,path1))
			lines.Add(l);

		//Keep polygon in memory
		List<Line> poly = new List<Line>(); 

		for(int i =0; i<path1.Length;i+=2)
			poly.Add(new Line(path1[i],path1[i+1]));
		for(int i =0; i<path2.Length;i+=2)
			poly.Add(new Line(path2[i],path2[i+1]));


		//Draw polygon
		foreach(Line l in poly)
		{
			l.DrawVector(temp,Color.gray);
		}

		//Get the vertex
		List<Vector3> vertex = new List<Vector3>(); 

		foreach(Line l in lines)
		{
			foreach(Vector3 v in l.vertex)
			{
				if(!vertex.Contains(v))
					vertex.Add(v);
			}
		}	

		//Debug.Log(vertex.Count);

		foreach(Vector3 v in vertex)
		{
			//Testing

	        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
	        sphere.transform.parent = temp.transform;
       
	        //Find the position of collision
	        sphere.transform.position = v; 
		}

		//Triangulate

		foreach(Vector3 v1 in vertex)
		{
			foreach(Vector3 v2 in vertex)
			{
				if(v1 == v2)
					continue;

				bool collisionFree = true; 

				Line t = new Line(v1,v2);
				
				//Check if the line is inside or outside. 
				//odd indside, even outside. 

				int counter = 0; 
				Vector3 center = t.MidPoint(); 

				//Path will never be negatif, so safe to test
				Vector3 rayEnd = new Vector3(-10, center.y,-1 );

				Line rayTest = new Line(center,rayEnd);

				//rayTest.DrawVector(temp); 

				foreach(Line l in poly)
				{
					if(l.LineIntersection(rayTest))
						counter++; 
					if(l.LineIntersection(t))
					{	
						collisionFree = false;
						break;
					}
				}

				if(counter %2==0 || !collisionFree)
					continue; 
				

				//Check if collision exists
				foreach (Line l in lines)
				{
					if(l == t)
						continue;


					if(l.LineIntersection(t))
					{
						//Debug.DrawLine(t.vertex[0],t.vertex[1],Color.red);	
						//lines.Add(t);
						collisionFree = false; 
						break; 
					}

				}


				if(collisionFree && !lines.Contains(t))
				{
					//Add the line	
					lines.Add(t);
				}
			}
		}

		//Find the triangles
		//Find the centers 
		triangles = new List<Triangle> (); 
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
								v1,v2,v3);
							
							
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
		
		//Get the area of the triangles
		float area = 0.0f; 
		foreach(Triangle tt in triangles)
			area += tt.GetArea(); 

		return area;

		//foreach(Line l in lines)
			//l.DrawVector(temp);
	}
	private List<Line> GetLines(Vector3[] path1, Vector3[] path2)
	{
		List<Line>ToReturnLine = new List<Line>(); 
		for(int i = 0; i<path1.Length-1; i+=2)
		{
			//Colliding with your path
			
			List<Vector3> vs = new List<Vector3>(); 
			//list of points where collision occured

			//Use to add to the collection of lines for triangulation
			
			for(int j = 0; j<path1.Length-1; j+=2)
			{
				if (j == i)
				{
					continue; 
				}
				if(LineIntersection(path1[i],path1[i+1],path1[j],path1[j+1]))
				{
					vs.Add(	LineIntersectVect(path1[i],path1[i+1],path1[j],path1[j+1]) );	
				}
			}

			//Collide with the other path
			for(int j = 0; j<path2.Length-1; j+=2)
			{
				Line t1 = new Line(path1[i],path1[i+1]);
				Line t2 = new Line(path2[j],path2[j+1]);

				if (t1 == t2)
				{
					continue; 
				}
				GameObject temp = GameObject.Find("temp");

				if(LineIntersection(path1[i],path1[i+1],path2[j],path2[j+1]))
				{
					//Debug.Log("Collide p1 with p2");
					vs.Add(	LineIntersectVect(path1[i],path1[i+1],path2[j],path2[j+1]) );
					//Testing
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.parent = temp.transform;
                   
                    //Find the position of collision
                    sphere.transform.position = LineIntersectVect(path1[i],path1[i+1],
                        path2[j],path2[j+1]);

                    //Debug.Log(sphere.transform.position);   

                    //Draw the two lines
                    /*
                    if(i == 2)
                    {
	                    VectorLine line = new VectorLine("4",new Vector3[]{path1[i],path1[i+1],
	                    	path2[j],path2[j+1]},Color.red,null,2.0f);

	                    line.vectorObject.transform.parent = temp.transform;
	                    line.Draw3D();
						
					}
					*/
				}


			}

			//Construct the lines based on the collisions
			if(vs.Count == 0)
			{	
				//No collision add the line normally
				ToReturnLine.Add(new Line(path1[i],path1[i+1]));
				continue;
			}
			else 
			{
				Vector3 start = path1[i];
				//Debug.Log(vs[0]);	
				while(vs.Count>0)
				{	

					Vector3 end = vs[0]; 
					float dist = (start - end).magnitude;
					
					//Find the closest vector to the start
					//Could sort them as a distance function with start;
					//Lazyness shall be it
					for(int t=1; t<vs.Count; t++)
					{
						if( (start - vs[t]).magnitude<dist)
						{
							dist =(start - vs[t]).magnitude; 
							end = vs[t];
						}
					}

					vs.Remove(end);
					ToReturnLine.Add(new Line(start,end));


					start = end; 


				}
				//Add the last part. 
				ToReturnLine.Add(new Line(start,path1[i+1]));

			}
		}
		return ToReturnLine; 
	}
	public void TriangulationSpace ()
	{
		//Compute one step of the discritzation
		//Find this is the view
		GameObject floor = (GameObject)GameObject.Find ("Floor");
			

		
		//First geometry is the outer one
		List<Geometry> geos = new List<Geometry> ();
		
		
		//Floor
		Vector3[] f = new Vector3[4];
		MeshFilter mesh = (MeshFilter)(floor.GetComponent ("MeshFilter"));
		Vector3[] t = mesh.sharedMesh.vertices; 
		
		Geometry tempGeometry = new Geometry (); 
		
		
		tempGeometry.vertex [0] = mesh.transform.TransformPoint (t [0]);
		tempGeometry.vertex [2] = mesh.transform.TransformPoint (t [120]);
		tempGeometry.vertex [1] = mesh.transform.TransformPoint (t [110]);
		tempGeometry.vertex [3] = mesh.transform.TransformPoint (t [10]);
		
		tempGeometry.vertex [0].y = 1; 
		tempGeometry.vertex [1].y = 1; 
		tempGeometry.vertex [2].y = 1; 
		tempGeometry.vertex [3].y = 1; 
		
		
		geos.Add (tempGeometry);
		
		
		
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
			
			tempGeometry = new Geometry (); 
			
			tempGeometry.vertex [0] = mesh.transform.TransformPoint (t [6]);
			tempGeometry.vertex [1] = mesh.transform.TransformPoint (t [8]);
			tempGeometry.vertex [3] = mesh.transform.TransformPoint (t [7]);
			tempGeometry.vertex [2] = mesh.transform.TransformPoint (t [9]);
			
			tempGeometry.vertex [0].y = 1; 
			tempGeometry.vertex [2].y = 1; 
			tempGeometry.vertex [1].y = 1; 
			tempGeometry.vertex [3].y = 1; 
			
			
			geos.Add (tempGeometry); 
			
		}
		
		//lines are defined by all the points in  obs
		List<Line> lines = new List<Line> (); 
		foreach (Geometry g in geos) {
			for (int i = 0; i< g.vertex.Length; i+=1) {
				if (i < g.vertex.Length - 1)
					lines.Add (new Line (g.vertex [i], g.vertex [i + 1]));
				else 	       
					lines.Add (new Line (g.vertex [0], g.vertex [i]));
				//triangulation.points.Add(g.vertex[i]);	      
				//triangulation.colours.Add(Color.cyan);	      
			}
			
		}
		
		foreach (Line l in lines) {
			//Debug.DrawLine(l.vertex[0],l.vertex[1],Color.blue);
			
		}
		//Lines are also the one added. 
		
		//Compare each point to every point 
		
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
		
		
		
	}
	
	private Boolean LineIntersect (Vector3 a, Vector3 b, Vector3 c, Vector3 d)
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
		
		if ((s > 0 && s < 1) && (t > 0 && t < 1))
			return true;
		
		return false; 
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

	private Boolean LineIntersection (Vector3 a, Vector3 b, Vector3 c, Vector3 d)
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
		
		
	}
	
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

}
