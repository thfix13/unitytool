using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class Visibility1 : MonoBehaviour {
	public List<Vector3> points = new List<Vector3>();
	public List<Color> colours = new List<Color>();
	// Use this for initialization
	
	public List<Triangle> triangles = new List<Triangle>(); 
	public List<Line> lines = new List<Line>(); 
	
	public List<Line> linesMinSpanTree = new List<Line>(); 
	public List<Geometry> obsGeos = new List<Geometry> (); 
	//Contains Map
	public Geometry mapBG = new Geometry ();
	
	//public bool drawTriangles = false; 
	//public bool drawRoadMap = false; 
	//public bool drawMinSpanTree = false;
	//public bool stopAll = false;
	public List<int>[] G = new List<int>[110];
	public int[] colorG = new int[110];
	public bool[] visitedG = new bool[110];
	public const int red = 1;
	public const int green = 2;
	public const int blue = 3;
	List<Geometry> globalPolygon;
	List<Vector3> pathPoints;
	float mapDiagonalLength = 0;
	//List<Vector3> globalTempArrangedPoints = new List<Vector3>();
	// Use this for initialization
	void Start () 
	{
		globalPolygon = getObstacleEdges ();
		//Debug.Log (globalPolygon.Count);
		pathPoints = definePath ();
		CalculateVisibilityForPath ();
		/*foreach (Vector3 vect in pathPoints) 
		{

			Debug.Log(vect.x+" , "+vect.y+" , "+vect.z);
		}*/
	}
	/*void OnGUI() 
	{
		foreach (Vector3 vect in globalTempArrangedPoints) 
		{
			GUI.Label (new Rect (10, 10, 10, 10), "Hello World!");
		}
	}*/
	//float timer = 5;
	//float timer2=0;
	//int arranged_index = 0;

	Vector3 first_point;
	//bool bArranged=false;
	List<Geometry> globalTempStarPoly = new List<Geometry>();
	List<List<Vector3>> globalTempintersectionPointsPerV = new List<List<Vector3>>();
	// Update is called once per frame
	void Update () 
	{
		foreach (Geometry geo in globalTempStarPoly) 
		{
			foreach(Line l in geo.edges)
			{
				Debug.DrawLine (l.vertex [0], l.vertex [1], Color.blue);
			}
		}
		/*
		for (int i=0; i<globalTempintersectionPointsPerV.Count-1; i++) 
		{
			for(int j=0;j<globalTempintersectionPointsPerV[i].Count;j++)
			{
				Debug.DrawLine(first_point,globalTempintersectionPointsPerV[i][j],Color.blue);
			}
		}*/
		/*if (!bArranged)
			return;
		timer-=Time.deltaTime;
		if (timer <= 0)
		{
			Debug.DrawLine (first_point, globalTempArrangedPoints[arranged_index], Color.blue);
			arranged_index++;
			timer=5;
			//timer2=1;
		}
		if (timer > 0) 
		{
			Debug.DrawLine(first_point, globalTempArrangedPoints[arranged_index], Color.blue);
			//timer2-=Time.deltaTime;
		}*/

				/*foreach (Geometry g in globalPolygon) {
						foreach (Line line in g.edges) {
								//Debug.Log("From"+line.vertex[0]);
								//Debug.Log("To"+line.vertex[1]);
								Debug.DrawLine (line.vertex [0], line.vertex [1], Color.blue);
						}
				}*/
	}
	public void CalculateVisibilityForPath()
	{
		globalPolygon = getObstacleEdges ();

		List<Vector3> endPoints = new List<Vector3> ();
		Hashtable hTable = new Hashtable ();
		//Extract all end points


		foreach(Line l in mapBG.edges)
		{
			foreach(Vector3 vect in l.vertex)
			{
				if(!endPoints.Contains(vect))
				{
					//finding
					for(int j=0;j<endPoints.Count;j++)
					{
						float dist = (Vector3.Distance(vect,endPoints[j]));
						if(mapDiagonalLength<dist)
						{
							mapDiagonalLength=dist;
						}
					}
					endPoints.Add(vect);
				}
			}
		}
		foreach (Geometry g in globalPolygon) 
		{
			foreach(Line l in g.edges)
			{
				foreach(Vector3 vect in l.vertex)
				{
					if(!endPoints.Contains(vect))
					{
						endPoints.Add(vect);
					}
				}
			}
		}
		//
		Vector3 normalVect = new Vector3 (0, 1, 0);
		Vector3 xVect = new Vector3 (1, 0, 0);
		//Do for all path points
		foreach(Vector3 pPoint in pathPoints)
		{
			Vector3 alongX = new Vector3(pPoint.x+2,pPoint.y,pPoint.z);
			List<Geometry> starPoly = new List<Geometry>();
			List<Vector3> arrangedPoints = new List<Vector3> ();
			List<float> angles = new List<float>();

			foreach(Vector3 vect in endPoints)
			{
				float sAngle = SignedAngleBetween(pPoint-vect,alongX-pPoint,normalVect);
				//Debug.Log(pPoint+" , "+vect+" , "+sAngle);
				angles.Add(sAngle);
			}
			int numTemp = angles.Count;
			while(numTemp>0)
			{
				float minAngle = 370;
				int indexAngle = -1;
				for (int i=0;i<angles.Count;i++)
				{
					if(minAngle>angles[i])
					{
						minAngle = angles[i];
						indexAngle = i;
					}
				}
				arrangedPoints.Add(endPoints[indexAngle]);
				angles[indexAngle]=370;
				numTemp--;
			}
			//find all intersection points
			List<List<Vector3>> intersectionPointsPerV = new List<List<Vector3>>();
			foreach(Vector3 vect in arrangedPoints)
			{
				Ray rayTemp = new Ray();
				rayTemp.direction = vect - pPoint;
				rayTemp.origin = pPoint;
				Vector3 extendedPoint = rayTemp.GetPoint(mapDiagonalLength);
				//Debug.Log(pPoint+" , "+vect+" , "+extendedPoint);
				Line longRayLine = new Line(pPoint,extendedPoint);
				//Find intersection points for longRayLine
				List<Vector3> intersectionPoints = new List<Vector3>();
				//Intersection with holes
				foreach (Geometry g in globalPolygon) 
				{
					foreach(Line l in g.edges)
					{
						if(l.LineIntersectMuntacEndPt(longRayLine)!=0)
						{
							Vector3 intsctPoint = l.GetIntersectionPoint(longRayLine);//LineIntersectionVect(longRayLine);
							//intsctPoint.x
							//if(!intersectionPoints.Contains(intsctPoint))
							if(!ListContainsPoint(intersectionPoints,intsctPoint))
							{
								//Debug.Log("Adding from intersection with holes "+intsctPoint.z);
								intersectionPoints.Add(intsctPoint);
							}
						}
					}
				}
				//Intersection with boundary points
				foreach(Line l in mapBG.edges)
				{
					if(l.LineIntersectMuntacEndPt(longRayLine)!=0)
					{
						Vector3 intsctPoint = l.GetIntersectionPoint(longRayLine);
						//if(!intersectionPoints.Contains(intsctPoint))
						if(!ListContainsPoint(intersectionPoints,intsctPoint))
						{
							//Debug.Log("Adding from intersection with boundary "+intsctPoint.z);
							intersectionPoints.Add(intsctPoint);
						}
					}
				}
				//Debug.Log(ListContainsPoint(intersectionPoints,vect));
				//Debug.Log(intersectionPoints.Count+"-----------------------------------");

				intersectionPointsPerV.Add(intersectionPoints);
				//Sort Intersection Points
				foreach(List<Vector3> intersectionPts in intersectionPointsPerV)
				{
					List<float> distancesFromV = new List<float>();
					foreach(Vector3 intsctPoint in intersectionPts)
					{
						distancesFromV.Add(Vector3.Distance(pPoint,intsctPoint));
					}
					for(int j=0;j<distancesFromV.Count;j++)
					{
						float leastVal = distancesFromV[j];
						for(int i=j+1;i<distancesFromV.Count;i++)
						{
							if(leastVal>distancesFromV[i])
							{
								leastVal=distancesFromV[i];
							}
						}
						int indexToReplace = distancesFromV.IndexOf(leastVal);
						float tmpA = distancesFromV[indexToReplace];
						distancesFromV[indexToReplace] = distancesFromV[j];
						distancesFromV[j] = tmpA;
						//Interchange values for intersection points
						Vector3 tmpB = intersectionPts[indexToReplace];
						intersectionPts[indexToReplace] = intersectionPts[j];
						intersectionPts[j] = tmpB;
					}
				}
			}
			//Debug.Log(intersectionPointsPerV[0].Count);
			//Remove vertex which is not visible
			//List<int> toRemoveListIndex = new List<int>();
			foreach(List<Vector3> intersectionPts in intersectionPointsPerV)
			{
				int tmpIndex = intersectionPointsPerV.IndexOf(intersectionPts);
				if(intersectionPts[0]!=arrangedPoints[tmpIndex])
				{
					//toRemoveListIndex.Add(tmpIndex);
					intersectionPointsPerV[tmpIndex]=null;//TODO: check if will be garbage collected
				}
			}
			intersectionPointsPerV.RemoveAll(item=>item==null);
			/*foreach(int toRemoveIndex in toRemoveListIndex)
			{
				intersectionPointsPerV.RemoveAt(toRemoveIndex);
			}*/
			//Remove all hidden intersection points behind visible vertices
			//TODO Have handle special case of two vertices on same ray from V,
			//then we might have more intersection points to consider other than 2
			foreach(List<Vector3> intersectionPts in intersectionPointsPerV)
			{
				if(intersectionPts.Count<2)
					continue;
				//if second point is on same polygon, just keep the single vertex and remove all behind it
				//if(existOnSamePolygon(intersectionPts[0],intersectionPts[1]))
				if(CheckIfInsidePolygon((intersectionPts[0]+intersectionPts[1])/2))
				{
					intersectionPts.RemoveRange(1,intersectionPts.Count-1);
				}
				//else keep the first two points
				else
				{
					intersectionPts.RemoveRange(2,intersectionPts.Count-2);
				}
			}

			//Build geometries
			for(int i=0;i<intersectionPointsPerV.Count;i++)
			{
				int nextIndex = (i+1)%intersectionPointsPerV.Count;
				Geometry geoVisible = new Geometry();
				for(int j=0;j<intersectionPointsPerV[i].Count-1;j++)
				{
					geoVisible.edges.Add(new Line(intersectionPointsPerV[i][j],intersectionPointsPerV[i][j+1]));
				}
				if(intersectionPointsPerV[i].Count==1 && intersectionPointsPerV[nextIndex].Count==1)
				{
					//geoVisible.edges.Add(new Line(pPoint,intersectionPointsPerV[i][0]));
					//geoVisible.edges.Add(new Line(pPoint,intersectionPointsPerV[i+1][0]));
					geoVisible.edges.Add(new Line(intersectionPointsPerV[i][0],intersectionPointsPerV[nextIndex][0]));
				}
				//All three cases, choose points on same polygon
				else
				{
					for(int j=0;j<intersectionPointsPerV[i].Count;j++)
					{
						for(int k=0;k<intersectionPointsPerV[nextIndex].Count;k++)
						{
							//if(existOnSamePolygon(intersectionPointsPerV[i][j],intersectionPointsPerV[i+1][k]))
							if(existOnSameLineOfPolygon(intersectionPointsPerV[i][j],intersectionPointsPerV[nextIndex][k]))
							{
								//geoVisible.edges.Add(new Line(pPoint,intersectionPointsPerV[i][j]));
								//geoVisible.edges.Add(new Line(pPoint,intersectionPointsPerV[i+1][k]));
								geoVisible.edges.Add(new Line(intersectionPointsPerV[i][j],intersectionPointsPerV[nextIndex][k]));
							}
						}
					}
				}
				/*else if(intersectionPointsPerV[i].Count==1 && intersectionPointsPerV[i+1].Count==2)
				{
				}
				else if(intersectionPointsPerV[i].Count==2 && intersectionPointsPerV[i+1].Count==1)
				{
				}
				else if(intersectionPointsPerV[i].Count==2 && intersectionPointsPerV[i+1].Count==2)
				{
				}*/
				starPoly.Add(geoVisible);
			}
			FindShadowPolygons(starPoly);
			//globalTempArrangedPoints.AddRange(arrangedPoints);
			globalTempStarPoly.AddRange(starPoly);
			globalTempintersectionPointsPerV.AddRange(intersectionPointsPerV);
			//bArranged = true;
			arrangedPoints.Clear();
			hTable.Add(pPoint,starPoly);
			break;//TODO Remove this
		}//End: Do for all path points
	}
	private List<Geometry> FindShadowPolygons(List<Geometry> starPoly)
	{
		List<Geometry> modObstacles = CreateModifiedObstacles(starPoly);
		Geometry mapModBoundary = CreateModifiedBoundary(starPoly);
		List<Geometry> shadowPoly = new List<Geometry> ();
		return shadowPoly;
	}

	List<Geometry> CreateModifiedObstacles (List<Geometry> starPoly)
	{
		List<Geometry> modObstacles = new List<Geometry> ();
		List<Vector3> verticesStar = new List<Vector3> ();
		foreach(Geometry gStar in starPoly)
		{
			foreach(Line l in gStar.edges)
			{
				if(!ListContainsPoint(verticesStar,l.vertex[0]))
				{
					verticesStar.Add(l.vertex[0]);
				}
				if(!ListContainsPoint(verticesStar,l.vertex[1]))
				{
					verticesStar.Add(l.vertex[1]);
				}
			}
		}
		/*foreach(Vector3 vect in verticesStar)
		{
			Debug.Log(vect);
		}*/
		foreach(Geometry g in globalPolygon)
		{
			Geometry obstacle = new Geometry();
			foreach(Line l in g.edges)
			{
				List<Vector3> pointsOnSameline = new List<Vector3>();
				pointsOnSameline.Add(l.vertex[0]);
				foreach(Vector3 vect in verticesStar)
				{
					if(l.PointOnLine(vect))
					{
						if(!ListContainsPoint(pointsOnSameline,vect))
						{
							pointsOnSameline.Add(vect);
						}
					}
				}
				//Sort points in a line
				for(int i=1;i<pointsOnSameline.Count-1;i++)
				{
					float dist = Vector3.Distance(pointsOnSameline[0],pointsOnSameline[i+1]);
					for(int j=i+1;j<pointsOnSameline.Count;j++)
					{
						float dist2 = Vector3.Distance(pointsOnSameline[0],pointsOnSameline[j]);
						if(dist>dist2)
						{
							dist=dist2;
						}
					}
				}
				pointsOnSameline.Add(l.vertex[1]);
			}
			modObstacles.Add(obstacle);
		}
		return modObstacles;
	}

	Geometry CreateModifiedBoundary (List<Geometry> starPoly)
	{
		Geometry mapModBoundary = new Geometry ();
		return mapModBoundary;
	}

	private bool ListContainsPoint(List<Vector3> intersectionPoints,Vector3 intsctPoint)
	{
		float limit = 0.0001f;
		foreach (Vector3 vect in intersectionPoints) 
		{
			if(Vector3.SqrMagnitude(vect-intsctPoint)<limit)
			//if(Mathf.Approximately(vect.magnitude,intsctPoint.magnitude))
				return true;
		}
		return false;
	}
	private bool CheckIfInsidePolygon(Vector3 pt)
	{
		bool result = false;
		foreach (Geometry g in globalPolygon)
		{
			result = g.PointInside(pt);
			if(result)
				break;
		}
		return result;
	}
	private bool existOnSameLineOfPolygon(Vector3 pt1,Vector3 pt2)
	{
		List<Geometry> allGeometries = new List<Geometry>();
		allGeometries.Add (mapBG);
		allGeometries.AddRange(globalPolygon);
		//TODO test this shit
		bool pt1Found=false;
		bool pt2Found=false;
		foreach (Geometry g in allGeometries)
		{
			foreach (Line l in g.edges) 
			{
				pt1Found = l.PointOnLine(pt1);
				pt2Found = l.PointOnLine(pt2);
				
				if(pt1Found && pt2Found)
				{
					return true;
				}
				/*if(!pt1Found && !pt2Found)
				{
					continue;
				}
				else
				{
					Debug.Log ("Line is " + l.vertex [1] + " to " + l.vertex [0]);
					//Debug.Log("Not on same line"+pt1+" , "+pt2);
					if(pt1Found)
					{
						Debug.Log(pt1+" found");
						Debug.Log(pt2+" NOT found");
					}
					if(pt2Found)
					{
						Debug.Log(pt2+" found");
						Debug.Log(pt1+" NOT found");
					}
					//return false;
				}*/
			}
		}
		return false;
	}
	private bool existOnSamePolygon(Vector3 pt1,Vector3 pt2)
	{
		List<Geometry> allGeometries = new List<Geometry>();
		allGeometries.Add (mapBG);
		allGeometries.AddRange (globalPolygon);
		//TODO test this shit
		//Debug.Log ("In existOnSamePolygon->" + allGeometries.Count + "=mapBG+"+globalPolygon.Count);
		foreach (Geometry g in allGeometries)
		{
			Boolean pt1Found=false;
			Boolean pt2Found=false;
			foreach (Line l in g.edges) 
			{
				if(l.PointOnLine(pt1))
				{
					pt1Found = true;
				}
				if(l.PointOnLine(pt2))
				{
					pt2Found = true;
				}
			}
			if(pt1Found && pt2Found)
			{
				return true;
			}
			if(!pt1Found && !pt2Found)
			{
				continue;
			}
			else
			{
				break;
			}
		}
		return false;
	}
	//copied from stackoverflow
	float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n){
		// angle in [0,180]
		float angle = Vector3.Angle(a,b);
		float sign = Mathf.Sign(Vector3.Dot(n,Vector3.Cross(a,b)));
		
		// angle in [-179,180]
		float signed_angle = angle * sign;
		
		// angle in [0,360] (not used but included here for completeness)
		float angle360 =  (signed_angle + 180) % 360;
		
		//return signed_angle;
		return angle360;
	}

	List<Vector3> definePath()
	{
		List<Vector3> pathPts = new List<Vector3> ();
		GameObject sp = (GameObject)GameObject.Find ("StartPoint");
		GameObject ep = (GameObject)GameObject.Find ("EndPoint");
		pathPts.Add(sp.transform.position);
		first_point = pathPts [0];
		pathPts.Add(ep.transform.position);
		findPath (pathPts);//straight Line points
		return pathPts;
	}
	private void findPath (List<Vector3> pathPts)
	{
		int iterations = 10;//increase to increase number of points on path
		for (int i=0; i<iterations; i++)
		{
			int k=0;
			int numPoints = pathPts.Count;
			for(int j=0;j<numPoints-1;j++)
			{
				Vector3 newVect = (pathPts[k]+pathPts[k+1])/2;
				//Debug.Log(newVect);
				pathPts.Insert(k+1,newVect);
				k+=2;
			}
		}
	}
	public List<Geometry> getObstacleEdges()
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
		
		vertex [0] = mesh.transform.TransformPoint (t [1]);
		vertex [1] = mesh.transform.TransformPoint (t [0]);
		vertex [2] = mesh.transform.TransformPoint (t [23]);
		vertex [3] = mesh.transform.TransformPoint (t [11]);

		
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
		
		//Geometry mapBG = new Geometry (); 
		for (int i = 0; i < 4; i++)
			mapBG.edges.Add( new Line( mapBoundary[i], mapBoundary[(i + 1) % 4]) );
		//mapBG.DrawVertex (GameObject.Find ("temp"));
		//mapBG.DrawGeometry(GameObject.find);
		
		GameObject[] obs = GameObject.FindGameObjectsWithTag ("Obs");
		if(obs == null)
		{
			//Debug.Log("Add tag geos to the geometries"); 
			return null; 
		}
		//data holder
		//Triangulation triangulation = GameObject.Find ("Triangulation").GetComponent<Triangulation> (); 
		//triangulation.points.Clear ();
		//triangulation.colours.Clear (); 
		
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
		return finalPoly;
	}
}
