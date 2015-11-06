using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 
using Vectrosity;

[ExecuteInEditMode]
public class Triangulation : MonoBehaviour 
{
	//Data holder to display and save
	[HideInInspector]
	public List<Vector3> points = new List<Vector3>();
	[HideInInspector]
	public List<Color> colours = new List<Color>();
	// Use this for initialization
	[HideInInspector]

	public List<Triangle> triangles = new List<Triangle>(); 
	[HideInInspector]
	public List<Line> lines = new List<Line>(); 
	[HideInInspector]

	public List<Line> linesMinSpanTree = new List<Line>(); 
	[HideInInspector]

	public List<Geometry> obsGeos = new List<Geometry> (); 
	//Contains Map
	[HideInInspector]
	public Geometry mapBG = new Geometry ();

	public bool drawTriangles = false; 
	public bool drawRoadMap = false; 
	public bool drawMinSpanTree = false;
	public bool stopAll = false;
	[HideInInspector]

	public List<int>[] G = new List<int>[110];
	[HideInInspector]

	public int[] colorG = new int[110];
	[HideInInspector]

	public bool[] visitedG = new bool[110];
	public const int red = 1;
	public const int green = 2;
	public const int blue = 3;
	[HideInInspector]

	public List<Line> roadMap = new List<Line>();

	//Stuff for alex
	[HideInInspector]
	List<Geometry> toReturn = new List<Geometry> ();


    private float threshold = 0.00001f;

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
		roadMap.Clear(); 
		toReturn.Clear(); 

		GameObject temp = GameObject.Find("temp"); 
		DestroyImmediate(temp); 


		stopAll = true;
	}
	void OnDrawGizmosSelected( ) 
	{
		//return; 
		//Debug.Log(colours.Count);
		//Debug.Log(points.Count);
		//var i = 0;
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
		//TODO: move to vectrocity for the drawing. 
		return;

        /*
		//return; 
		//points.Clear(); 
		//colours.Clear();  
		if ( stopAll )
			return;


		if(drawMinSpanTree)
		{
			GameObject temp = GameObject.Find("temp"); 
			foreach(Line l in linesMinSpanTree)
			{
				//l.DrawLine(Color.blue); 
				//l.DrawVector( temp );
				Debug.DrawLine(l.vertex[0], l.vertex[1],Color.red);
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
		}*/

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

    private int infiniLoopStop = 1000;


	public List<Geometry> TriangulationSpace ()
	{

		if (toReturn.Count>0)
			return toReturn; 

		//Compute one step of the discritzation
		//Find this is the view
		GameObject floor = (GameObject)GameObject.Find ("Floor");
			
		Vector3 [] vertex = new Vector3[4]; 
		
		//First geometry is the outer one
		List<Geometry> geos = new List<Geometry> ();

		
		//Drawing lines
		//VectorLine.SetCamera3D(Camera.current); 

		//Floor
		//Vector3[] f = new Vector3[4];
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
			return null; 
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

       /* GameObject parent1 = new GameObject("DebugParent1");
        foreach (Geometry aki in obsGeos)
        { 
            foreach (Line l in aki.edges)
            {
                //Debug.Log(l.vertex[0] + "," + l.vertex[1]);

                GameObject lin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lin.GetComponent<Renderer>().sharedMaterial.color = Color.red;
                lin.transform.parent = parent1.transform;
                lin.transform.position = (l.vertex[0] + l.vertex[1]) / 2.0f;
                lin.transform.position = new Vector3(lin.transform.position.x, 0.05f * lin.transform.position.y, lin.transform.position.z);
                Vector3 dists = (l.vertex[1] - l.vertex[0]);
                dists.y = 0.05f * dists.y;

                Vector3 from = Vector3.right;
                Vector3 to = dists / dists.magnitude;

                Vector3 axis = Vector3.Cross(from, to);
                float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(from, to));
                lin.transform.RotateAround(lin.transform.position, axis, angle);


                Vector3 scale = Vector3.one;
                scale.x = Vector3.Magnitude(dists);
                scale.z = 0.2f;
                scale.y = 0.2f;

                lin.transform.localScale = scale;
            }
        }
        */



        //CODESPACE
        //Merging Polygons
      /*  for (int i = 0; i < obsGeos.Count; i++) {
			for (int j = i + 1; j < obsGeos.Count; j++) {
				//check all line intersections
				if( obsGeos[i].GeometryIntersect( obsGeos[j] ) ){
                    Debug.Log(obsGeos.Count);
					//Debug.Log("Geometries Intersect: " + i + " " + j);
					Geometry tmpG = obsGeos[i].GeometryMerge( obsGeos[j] ); 
					//remove item at position i, decrement i since it will be increment in the next step, break
					obsGeos.RemoveAt(j);
					obsGeos.RemoveAt(i);
					obsGeos.Add(tmpG);
					i--;
                    Debug.Log(obsGeos.Count);
					break;
				}
			}
		}*/


        bool done = false;
        bool difFound = false;
        string preS = "1";
        int preI = 1;
        //GameObject mergeys = new GameObject("Mergeys");
        int whileIndex = 1;
        int numMerge = 0;
        while (!done)
        {
           for(int i = 0; i < obsGeos.Count; i++) {
                if (difFound)
                {
                    break;
                }
                for(int j = i+1; j < obsGeos.Count; j++) {
                    if (obsGeos[i].GeometryIntersect(obsGeos[j]))
                    {
                        //if(preI == 33) { 
                        //if (preI == 28) {
                        //    Debug.Log("DEBUGGERATION ACTIVATED");
                        //    obsGeos[i].debuggery = true;
                       //     obsGeos[j].debuggery = true;
                            //Debug.Log(obsGeos[i].debuggery);
                            //Debug.Log(obsGeos[j].debuggery);
                        //}
                        numMerge++;
                        //Debug.Log("NEW MERGE NUMBER:" + numMerge);
                        Geometry tmpG = obsGeos[i].GeometryMerge(obsGeos[j]);
                        //Debug.Log("MERGE COMPLETE OF NUMBER:" + numMerge);
                        obsGeos[i].debuggery = false;
                        obsGeos[j].debuggery = false;
                        preS = preI.ToString();
                        /*GameObject merger = new GameObject(preS);
                        GameObject geo1 = new GameObject(preS + "geo1");
                        GameObject geo2 = new GameObject(preS + "geo2");
                        GameObject geoM = new GameObject(preS + "geoM");
                        merger.transform.parent = mergeys.transform;
                        geo1.transform.parent = merger.transform;
                        geo2.transform.parent = merger.transform;
                        geoM.transform.parent = merger.transform;*/
                        preI++;
                        
                        /*
                       foreach (Line l in obsGeos[i].edges)
                        {
                            GameObject lin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            lin.GetComponent<Renderer>().sharedMaterial.color = Color.red;
                            lin.transform.parent = geo1.transform;
                            lin.transform.position = (l.vertex[0] + l.vertex[1]) / 2.0f;
                            lin.transform.position = new Vector3(lin.transform.position.x, 0.05f * lin.transform.position.y, lin.transform.position.z);
                            Vector3 dists = (l.vertex[1] - l.vertex[0]);
                            dists.y = 0.05f * dists.y;

                            Vector3 from = Vector3.right;
                            Vector3 to = dists / dists.magnitude;

                            Vector3 axis = Vector3.Cross(from, to);
                            float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(from, to));
                            lin.transform.RotateAround(lin.transform.position, axis, angle);


                            Vector3 scale = Vector3.one;
                            scale.x = Vector3.Magnitude(dists);
                            scale.z = 0.2f;
                            scale.y = 0.2f;

                            lin.transform.localScale = scale;
                        }
                        foreach (Line l in obsGeos[j].edges)
                        {
                            GameObject lin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            lin.GetComponent<Renderer>().sharedMaterial.color = Color.red;
                            lin.transform.parent = geo2.transform;
                            lin.transform.position = (l.vertex[0] + l.vertex[1]) / 2.0f;
                            lin.transform.position = new Vector3(lin.transform.position.x, 0.05f * lin.transform.position.y, lin.transform.position.z);
                            Vector3 dists = (l.vertex[1] - l.vertex[0]);
                            dists.y = 0.05f * dists.y;

                            Vector3 from = Vector3.right;
                            Vector3 to = dists / dists.magnitude;

                            Vector3 axis = Vector3.Cross(from, to);
                            float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(from, to));
                            lin.transform.RotateAround(lin.transform.position, axis, angle);


                            Vector3 scale = Vector3.one;
                            scale.x = Vector3.Magnitude(dists);
                            scale.z = 0.2f;
                            scale.y = 0.2f;

                            lin.transform.localScale = scale;
                        }
                        foreach (Line l in tmpG.edges)
                        {
                            GameObject lin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            lin.GetComponent<Renderer>().sharedMaterial.color = Color.red;
                            lin.transform.parent = geoM.transform;
                            lin.transform.position = (l.vertex[0] + l.vertex[1]) / 2.0f;
                            lin.transform.position = new Vector3(lin.transform.position.x, 0.05f * lin.transform.position.y, lin.transform.position.z);
                            Vector3 dists = (l.vertex[1] - l.vertex[0]);
                            dists.y = 0.05f * dists.y;

                            Vector3 from = Vector3.right;
                            Vector3 to = dists / dists.magnitude;

                            Vector3 axis = Vector3.Cross(from, to);
                            float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(from, to));
                            lin.transform.RotateAround(lin.transform.position, axis, angle);


                            Vector3 scale = Vector3.one;
                            scale.x = Vector3.Magnitude(dists);
                            scale.z = 0.2f;
                            scale.y = 0.2f;

                            lin.transform.localScale = scale;
                        }*/
                        

                        obsGeos.RemoveAt(j);
                        obsGeos.RemoveAt(i);
                        obsGeos.Add(tmpG);
                        difFound = true;
                        break;
                    }
                }
            }
            if (difFound)
            {
                difFound = false;
            }
            else
            {
                done = true;
            }
            whileIndex++;
            if (whileIndex > infiniLoopStop)
            {
                Debug.Log("INFINILOOP1");
                break;
            }

       }



















        //mapBG.DrawGeometry (temp);
        //Debug.Log(obsGeos);
        //Debug.Log(obsGeos.Count);
        //TODO REMOVE DEBUG
       /* GameObject parent = new GameObject("DebugParent");
        foreach (Line l in obsGeos[0].edges) {
            //Debug.Log(l.vertex[0] + "," + l.vertex[1]);

            GameObject lin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lin.GetComponent<Renderer>().sharedMaterial.color = Color.red;
            lin.transform.parent = parent.transform;
            lin.transform.position = (l.vertex[0] + l.vertex[1]) / 2.0f;
            lin.transform.position = new Vector3(lin.transform.position.x, 0.05f * lin.transform.position.y, lin.transform.position.z);
            Vector3 dists = (l.vertex[1] - l.vertex[0]);
            dists.y = 0.05f * dists.y;

            Vector3 from = Vector3.right;
            Vector3 to = dists / dists.magnitude;

            Vector3 axis = Vector3.Cross(from, to);
            float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(from, to));
            lin.transform.RotateAround(lin.transform.position, axis, angle);


            Vector3 scale = Vector3.one;
            scale.x = Vector3.Magnitude(dists);
            scale.z = 0.2f;
            scale.y = 0.2f;

            lin.transform.localScale = scale;










        }*/
        //TODO END OF DEBUG TO REMOVE

		List<Geometry> finalPoly = new List<Geometry> ();//Contains all polygons that are fully insde the map
		foreach ( Geometry g in obsGeos ) {
			if( mapBG.GeometryIntersect( g ) && !mapBG.GeometryInside( g ) ){
                mapBG.debuggery = true;
                g.debuggery = true;
				mapBG = mapBG.GeometryMergeInner( g );
                mapBG.debuggery = false;
                g.debuggery = false;
				mapBG.BoundGeometry( mapBoundary );
			}
			else
				finalPoly.Add(g);
		}
       // Debug.Log(finalPoly.Count);

		foreach(Geometry g in finalPoly){
            //DRAWING GEOMETRY NO LONGER WORKS CUZ VECTROSITY
			//g.DrawGeometry( temp);
			toReturn.Add (g);
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
		lines.Clear ();


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
		foreach (Geometry g in toCheck) {
			g.SetVoisins( toCheck );		
		}
		//keep a list of the edges (graph where obstaceles are the nodes) in a list of lines called "linesLinking"
		List<Vector3> mapVertices = mapBG.GetVertex();

		//Possible redundancy here
		Geometry start = mapBG.findClosestQuad (mapVertices[0], toCheck, new List<Geometry> ());


		List<Line> linesLinking = new List<Line> ();
        if (start != null)
        {
            linesLinking.Add(mapBG.GetClosestLine(start, toCheck));
        	start.visited = true;
        
            List<Geometry> toCheckNode = new List<Geometry> (); 
		    toCheckNode.Add (start);
            //Debug.Log(start);
            //Debug.Log(start.voisinsLine);
            //Debug.Log(start.voisinsLine.Count);
            Line LinetoAdd = null;
            if (start.voisinsLine.Count > 0) { 
		        LinetoAdd= start.voisinsLine [0];

            }
        
            //DRAW GEOMETRY NEEDS VECTROSITY WHICH DOES NOT WORK RIGHT NOW
            //mapBG.DrawGeometry (temp);
		    toReturn.Add (mapBG);

            //Straight Porting//
            int whileIndex2 = 1;
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

                whileIndex2++;
                if(whileIndex2 > infiniLoopStop)
                {
                    Debug.Log("INFINILOOP2");
                    break;
                }
		    }

        }

        foreach (Line l in linesLinking) {
			triangulation.linesMinSpanTree.Add (l); 
		}
        //END porting

        //-----------END MST CODE--------------------//
        
        //DEBUG SECTION -- OBSTACLES
        GameObject parentO = new GameObject("DebugParentObstacles");
        foreach (Geometry G in obsGeos){
            foreach (Line l in G.edges){
           //foreach( Line l in mapBG.edges){
                GameObject lin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lin.GetComponent<Renderer>().sharedMaterial.color = Color.red;
                lin.transform.parent = parentO.transform;
                lin.transform.position = (l.vertex[0] + l.vertex[1]) / 2.0f;
                lin.transform.position = new Vector3(lin.transform.position.x, 0.05f * lin.transform.position.y, lin.transform.position.z);
                Vector3 dists = (l.vertex[1] - l.vertex[0]);
                dists.y = 0.05f * dists.y;

                Vector3 from = Vector3.right;
                Vector3 to = dists / dists.magnitude;

                Vector3 axis = Vector3.Cross(from, to);
                float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(from, to));
                lin.transform.RotateAround(lin.transform.position, axis, angle);


                Vector3 scale = Vector3.one;
                scale.x = Vector3.Magnitude(dists);
                scale.z = 0.2f;
                scale.y = 0.2f;

                lin.transform.localScale = scale;
            }
        }
        GameObject parentMap = new GameObject("ParentMap");
        foreach (Line l in mapBG.edges) {
            GameObject lin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lin.GetComponent<Renderer>().sharedMaterial.color = Color.red;
            lin.transform.parent = parentMap.transform;
            lin.transform.position = (l.vertex[0] + l.vertex[1]) / 2.0f;
            lin.transform.position = new Vector3(lin.transform.position.x, 0.05f * lin.transform.position.y, lin.transform.position.z);
            Vector3 dists = (l.vertex[1] - l.vertex[0]);
            dists.y = 0.05f * dists.y;

            Vector3 from = Vector3.right;
            Vector3 to = dists / dists.magnitude;

            Vector3 axis = Vector3.Cross(from, to);
            float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(from, to));
            lin.transform.RotateAround(lin.transform.position, axis, angle);


            Vector3 scale = Vector3.one;
            scale.x = Vector3.Magnitude(dists);
            scale.z = 0.2f;
            scale.y = 0.2f;

            lin.transform.localScale = scale;
        }


        /*

        //DEBUG SECTION -- MST
        GameObject parent = new GameObject("DebugParentMinSpanTree");
        foreach (Line l in linesMinSpanTree)
        {
            GameObject lin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lin.GetComponent<Renderer>().sharedMaterial.color = Color.red;
            lin.transform.parent = parent.transform;
            lin.transform.position = (l.vertex[0] + l.vertex[1]) / 2.0f;
            lin.transform.position = new Vector3(lin.transform.position.x, 0.05f * lin.transform.position.y, lin.transform.position.z);
            Vector3 dists = (l.vertex[1] - l.vertex[0]);
            dists.y = 0.05f * dists.y;

            Vector3 from = Vector3.right;
            Vector3 to = dists / dists.magnitude;

            Vector3 axis = Vector3.Cross(from, to);
            float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(from, to));
            lin.transform.RotateAround(lin.transform.position, axis, angle);


            Vector3 scale = Vector3.one;
            scale.x = Vector3.Magnitude(dists);
            scale.z = 0.2f;
            scale.y = 0.2f;

            lin.transform.localScale = scale;
        }*/

        //




        //int vlcnt = 0;
        lines.Clear ();
        //Constructing "lines" for triangulation
        //First add lines that are in MST


        foreach (Line l in linesMinSpanTree){
            lines.Add(l);
        }

        //		Debug.Log (allVertex.Count);
        //		int iv = -1, jv;
        //		vlcnt = 0;


        //finalPoly -> contained polygons
        //MapBG -> outer edge including polys
        //totalPoly -> all of the above edges


        foreach(Line l in mapBG.edges) {
            lines.Add(l);
        }




        //GameObject vertRent = new GameObject("vertRent");
        //GameObject vert;
        for(int i = 0; i < allVertex.Count; i++) {

            /*vert = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            vert.transform.position = allVertex[i];
            vert.transform.parent = vertRent.transform;*/

            for (int j = i+1; j < allVertex.Count; j++) {
                Line l = new Line(allVertex[i], allVertex[j]);
                Vector3 mid = l.MidPoint();
                bool collision = false;
                foreach( Geometry g in obsGeos) {
                    if (g.PointInside(mid)) {
                        collision = true;
                        break;
                    }
                }
                if (!collision) {
                    foreach (Line l2 in mapBG.edges) {
                        int intersect = l.LineIntersectMuntac(l2);
                        if (intersect == 1) {
                            collision = true;
                            break;
                        }
                    }
                }
                if (!collision) {
                    foreach(Geometry g in finalPoly) {
                        if (collision) {
                            break;
                        }
                        foreach(Line l2 in g.edges) {
                            int intersect = l.LineIntersectMuntac(l2);
                            if (intersect == 1) {
                                collision = true;
                                break;
                            }
                        }
                    }
                }
                if (!collision) {
                    foreach(Line l2 in lines) {
                        int intersect = l.LineIntersectMuntac(l2);
                        if(intersect > 0) {
                            collision = true;
                            break;
                        }
                    }
                }
                if (!collision) {
                    lines.Add(l);
                }
                
            }
        }


        /*
		foreach (Vector3 Va in allVertex) {
            vert = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            vert.transform.position = Va;
            vert.transform.parent = vertRent.transform;
//			++iv;
//			jv = -1;
			foreach(Vector3 Vb in allVertex){
//				++jv;
				if( Va != Vb ){
                    bool collides = false;
                    //bool essential = false;
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
        */
//		foreach (Line L in lines)
//			L.DrawVector(temp);
//		Debug.Log ("Total Lines" + lines.Count);
				
		//Find the centers 
		List<Triangle> triangles = new List<Triangle> ();
        //Well why be efficient when you can be not efficient
        GameObject linesPar = new GameObject("linesPar");


        Vector3 v1;
        Vector3 v2;
        Vector3 v3;
        bool foundLine2 = false;

        foreach (Line l in lines) {
            //DEBUG PRINTING
                GameObject lin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lin.GetComponent<Renderer>().sharedMaterial.color = Color.red;
                lin.transform.parent = linesPar.transform;
                lin.transform.position = (l.vertex[0] + l.vertex[1]) / 2.0f;
                lin.transform.position = new Vector3(lin.transform.position.x, 0.05f * lin.transform.position.y, lin.transform.position.z);
                Vector3 dists = (l.vertex[1] - l.vertex[0]);
                dists.y = 0.05f * dists.y;

                Vector3 from = Vector3.right;
                Vector3 to = dists / dists.magnitude;

                Vector3 axis = Vector3.Cross(from, to);
                float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(from, to));
                lin.transform.RotateAround(lin.transform.position, axis, angle);


                Vector3 scale = Vector3.one;
                scale.x = Vector3.Magnitude(dists);
                scale.z = 0.2f;
                scale.y = 0.2f;

                lin.transform.localScale = scale;
                //END DEBUG PRINTING
            v1 = l.vertex [0]; 
			v2 = l.vertex [1];
			foreach (Line l2 in lines) {
				if (l == l2)
					continue;
								
				if ((l2.vertex[0] - v2).magnitude < threshold) {
					v3 = l2.vertex [1];
                    foundLine2 = true;

				} else if ((l2.vertex[1] - v2).magnitude < threshold) {
					v3 = l2.vertex [0];
                    foundLine2 = true;
				}
                else {
                    v3 = Vector3.zero;
                    foundLine2 = false;
                }
				
				if (foundLine2) {
					foreach (Line l3 in lines) {
                        if (l3 == l2 || l3 == l) {
                            continue;
                        }
						if (  (((l3.vertex [0] -v1).magnitude < threshold)   && ((l3.vertex[1] - v3).magnitude < threshold))
						    || (((l3.vertex[1] - v1).magnitude < threshold) && ((l3.vertex[0] - v3).magnitude < threshold))) {
							//Debug.DrawLine(v1,v2,Color.red); 
							//Debug.DrawLine(v2,v3,Color.red); 
							//Debug.DrawLine(v3,v1,Color.red); 
							
							//Add the traingle
							Triangle toAddTriangle = new Triangle (
								v1, triangulation.points.IndexOf (v1),
								v2, triangulation.points.IndexOf (v2),
								v3, triangulation.points.IndexOf (v3));
							
							
							bool isAlready = false; 
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

		//Create the road map
		roadMap.Clear();


        makeRoadMap(triangles);  

        /*
		foreach(Triangle tt in triangles)
		{
			Line[] ll = tt.GetSharedLines(); 
			if(ll.Length == 1)
			{
				
				roadMap.Add(new Line(ll[0].MidPoint(), tt.GetCenterTriangle()));
			}
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
		}*/

        /*GameObject roadPar = new GameObject("roadPar");
        foreach (Line l in roadMap) {
            GameObject lin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lin.GetComponent<Renderer>().sharedMaterial.color = Color.red;
            lin.transform.parent = roadPar.transform;
            lin.transform.position = (l.vertex[0] + l.vertex[1]) / 2.0f;
            lin.transform.position = new Vector3(lin.transform.position.x, 0.05f * lin.transform.position.y, lin.transform.position.z);
            Vector3 dists = (l.vertex[1] - l.vertex[0]);
            dists.y = 0.05f * dists.y;

            Vector3 from = Vector3.right;
            Vector3 to = dists / dists.magnitude;

            Vector3 axis = Vector3.Cross(from, to);
            float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(from, to));
            lin.transform.RotateAround(lin.transform.position, axis, angle);


            Vector3 scale = Vector3.one;
            scale.x = Vector3.Magnitude(dists);
            scale.z = 0.2f;
            scale.y = 0.2f;

            lin.transform.localScale = scale;
        }
        */
	
		return toReturn;
	}

    private void makeRoadMap(List<Triangle> tris) {
        Triangle t = tris[0];
        List<Triangle> visited = new List<Triangle>();
        addLines(t, visited);
    }

    private List<Triangle> addLines(Triangle t, List<Triangle> visited) {
        visited.Add(t);
        foreach(Triangle tt in t.voisins) {
            if (!visited.Contains(tt)) {
                Vector3 midPoint = t.ShareEdged(tt).MidPoint();
                Line l1 = new Line(t.GetCenterTriangle(), midPoint);
                Line l2 = new Line(midPoint, tt.GetCenterTriangle());
                roadMap.Add(l1);
                roadMap.Add(l2);
            }
        }
        foreach(Triangle tt in t.voisins) {
            if (!visited.Contains(tt)) {
                visited = addLines(tt, visited);
            }
        }
        return visited;
    }

}



