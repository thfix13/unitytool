using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System;
public class Visibility1 : MonoBehaviour {
	//public List<Vector3> points = new List<Vector3>();
	//public List<Color> colours = new List<Color>();
	// Use this for initialization
	
	//public List<Triangle> triangles = new List<Triangle>(); 
	//public List<Line> lines = new List<Line>(); 
	
	//public List<Line> linesMinSpanTree = new List<Line>(); 
	public List<Geometry> obsGeos = new List<Geometry> (); 
	//Contains Map
	public Geometry mapBG = new Geometry ();
	public bool bShowLogs=false;
	public int pathIndexToShowShadow=0;
	//public bool drawTriangles = false; 
	//public bool drawRoadMap = false; 
	//public bool drawMinSpanTree = false;
	//public bool stopAll = false;
	//public List<int>[] G = new List<int>[110];
	//public int[] colorG = new int[110];
	//public bool[] visitedG = new bool[110];
	//public const int red = 1;
	//public const int green = 2;
	//public const int blue = 3;
	List<Geometry> globalPolygon;
	List<Vector3> pathPoints;
	float mapDiagonalLength = 0;
	GameObject floor ;
	public Camera camObj;
	//List<Vector3> globalTempArrangedPoints = new List<Vector3>();
	// Use this for initialization
	GameObject spTemp ;
	List<GameObject> shadowMeshes;
	void Start () 
	{
		spTemp = (GameObject)GameObject.Find ("StartPoint");
		allLineParent = GameObject.Find ("allLineParent") as GameObject;
		globalPolygon = getObstacleEdges ();

		pathPoints = definePath ();
		foreach(Vector3 vect in pathPoints)
		{
			GameObject pathObj;
			pathObj = Instantiate(pathSphere, 
			                    vect, 
			                    pathSphere.transform.rotation) as GameObject;
		}
		CalculateVisibilityForPath ();
		shadowMeshes = new List<GameObject>();
		//Uncomment
		//displayShadowMeshes(pathIndexToShowShadow);
		displayStrategicPoints (pathIndexToShowShadow);
	}


	//Vector3 first_point;
	List<Geometry> globalTempShadowPoly = new List<Geometry>();
	Geometry globalTempStarPoly;
	List<List<Vector3>> globalTempintersectionPointsPerV = new List<List<Vector3>>();
	List<Line> globalTempAllShadowLines = new List<Line>();
	public Material mat;
	public GameObject pathSphere;
	public GameObject hiddenSphere;
	public GameObject selectedBoxPrefab;
	GameObject selectedBox;
	List<GameObject> hiddenSphereList;
	//GameObject shadowObject = new GameObject();

	
	Hashtable hTable;
	Hashtable hVisiblePolyTable;
	Vector3 start_box,end_box;
	Rect boundbox;
	bool b_ShowBoundbox=false;
	private void makeBox() {
		Debug.Log("In makeBox");
		//Ensures the bottom left and top right values are correct
		//regardless of how the user boxes units
		float xmin = Mathf.Min(start_box.x, end_box.x);
		float zmin = Mathf.Min(start_box.z, end_box.z);
		float width = Mathf.Max(start_box.x, end_box.x) - xmin;
		float height = Mathf.Max(start_box.z, end_box.z) - zmin;
		boundbox = new Rect(xmin, zmin, width, height);
		if(width*height>0.01)
		{
			selectedBox = Instantiate(selectedBoxPrefab) as GameObject;
			b_ShowBoundbox = true;
			float centreX=(start_box.x+end_box.x)/2;
			float centreZ=(start_box.z+end_box.z)/2;
			selectedBox.renderer.enabled=true;
			Vector3 tempVect = new Vector3(centreX,1,centreZ);
			selectedBox.transform.position=tempVect;
			tempVect.x=width;
			tempVect.z=height;
			selectedBox.transform.localScale=tempVect;

			//////////////////////////////////////////
			/// //Identify path points in box
			Geometry boundboxGeo = new Geometry ();
			boundboxGeo.edges.Add (new Line (new Vector3(boundbox.x,1,boundbox.y),new Vector3(boundbox.x+boundbox.width,1,boundbox.y)));
			boundboxGeo.edges.Add (new Line (new Vector3(boundbox.x+boundbox.width,1,boundbox.y),new Vector3(boundbox.x+boundbox.width,1,boundbox.y+boundbox.height)));
			boundboxGeo.edges.Add (new Line (new Vector3(boundbox.x+boundbox.width,1,boundbox.y+boundbox.height),new Vector3(boundbox.x,1,boundbox.y+boundbox.height)));
			boundboxGeo.edges.Add (new Line (new Vector3(boundbox.x,1,boundbox.y+boundbox.height),new Vector3(boundbox.x,1,boundbox.y)));
			startIndex = -1;
			endIndex = -1;
			int currIndex = 0;
			foreach(Vector3 vect in pathPoints)
			{
				if(boundboxGeo.PointInside(vect))
				{
					if(startIndex==-1)
					{
						startIndex=currIndex;
					}
				}
				else
				{
					if(startIndex!=-1)
					{
						endIndex=currIndex-1;
						break;
					}
				}
				currIndex++;
			}

			//if all points selected
			if(startIndex!=-1 && endIndex==-1)
			{
				endIndex=currIndex-1;
			}

			Debug.Log ("startIndex = " + startIndex);
			Debug.Log ("endIndex = " + endIndex);
			/// /////////////////////////////////////////
			//Only show shadow polygon if only one path point is selected

		}
		else
		{
			GameObject.Destroy(selectedBox);
			b_ShowBoundbox=false;
		}
		if(hiddenSphereList!=null)
		{
			foreach(GameObject g in hiddenSphereList)
			{
				GameObject.Destroy(g);
			}
		}
		hiddenSphereList=null;
		Debug.Log ("Destroying shadowMeshes = " + shadowMeshes.Count);
		foreach(GameObject tempObj in shadowMeshes)
		{
			GameObject.Destroy(tempObj);
		}
		shadowMeshes.Clear ();
	}
	public bool checkIfLineExists(Line lineTemp,List<Line> listEdges)
	{
		foreach(Line l in listEdges)
		{
			if(l.Equals(lineTemp))
				return true;
		}
		return false;
	}
	/// <summary>
	/// for use in consolidate polygons
	////// </summary>
	/// <returns><c>true</c>, if edges was compared, <c>false</c> otherwise.</returns>
	/// <param name="l1">L1.</param>
	/// <param name="l2">L2.</param>
	private bool compareEdges(Line l1,Line l2)
	{
		float limit = 0.1f;
		float minVal = Vector3.Distance(l1.MidPoint(),l2.MidPoint());
		if (minVal <= limit)
		{
			if((comparePoints(l1.vertex[0],l2.vertex[1]) && comparePoints(l1.vertex[1],l2.vertex[0])) || (comparePoints(l1.vertex[0],l2.vertex[0]) && comparePoints(l1.vertex[1],l2.vertex[1])))
				return true;
		}
		return false;
	}
	private bool comparePoints(Vector3 v1,Vector3 v2)
	{
		float limit = 0.1f;
		float minVal = Vector3.SqrMagnitude (v1 - v2);
		if (minVal <= limit)
			return true;
		return false;
	}
	public Geometry consolidateShadowPolygon(Geometry geo)
	{
		//Removing duplicate points
		StandardPolygon sdTemp = new StandardPolygon();
		bool addAnother = false;
		Geometry newGeo = new Geometry ();
		foreach(Line l1 in geo.edges)
		{
			Line l = l1;
			addAnother = sdTemp.addPoint(l.vertex[0]);
			if(!addAnother)
			{
				int indx = sdTemp.findIndexOfDuplicate(l.vertex[0]);
				Vector3 dupPt = sdTemp.getVertices()[indx];
				l.vertex[0]=dupPt;
			}
			addAnother = sdTemp.addPoint(l.vertex[1]);
			if(!addAnother)
			{
				int indx = sdTemp.findIndexOfDuplicate(l.vertex[1]);
				Vector3 dupPt = sdTemp.getVertices()[indx];
				l.vertex[1]=dupPt;
			}
			newGeo.edges.Add(l);
		}
		//Removing very close edges
		for(int i=0;i<newGeo.edges.Count;i++)
		{
			for(int j=i+1;j<newGeo.edges.Count;j++)
			{
				//if(compareEdges(newGeo.edges[i],newGeo.edges[j]))
				if(newGeo.edges[i].Equals(newGeo.edges[j]))
				{
					newGeo.edges.RemoveAt(j);
					j--;
				}
			}
		}
		return newGeo;
		//Removing very small lines
		List<int> tobeRemoved = new List<int> ();
		for(int i=0;i<newGeo.edges.Count;i++)
		{
			if(newGeo.edges[i].LengthOfLine()<0.1)
			{
				Vector3 v1 = newGeo.edges[i].vertex[0];
				Vector3 v2 = newGeo.edges[i].vertex[1];
				Vector3 midPt = newGeo.edges[i].vertex[0];//newGeo.edges[i].MidPoint();
				bool bV1Done = false;
				bool bV2Done = false;
				int j1=-1;
				int j2=-1;
				for(int j=0;i<newGeo.edges.Count;j++)
				{
					if(i==j)
						continue;
					if(!bV1Done && newGeo.edges[j].vertex[0].Equals(v1))
					{
						newGeo.edges[j].vertex[0] = midPt;

						bV1Done=true;
					}
					if(!bV1Done && newGeo.edges[j].vertex[1].Equals(v1))
					{
						newGeo.edges[j].vertex[1] = midPt;

						bV1Done=true;
					}
					if(!bV2Done && newGeo.edges[j].vertex[0].Equals(v2))
					{
						newGeo.edges[j].vertex[0] = midPt;

						bV2Done=true;
					}
					if(!bV2Done && newGeo.edges[j].vertex[1].Equals(v2))
					{
						newGeo.edges[j].vertex[1] = midPt;

						bV2Done=true;
					}
				}
				//newGeo.edges.RemoveAt(i);
				//i--;

				if(bV1Done && bV2Done)
				{
					tobeRemoved.Add(i);
				}
				else
				{
					Debug.Log ("&&&&&&&&&&&& Error while removing small edges &&&&&&&&&&&&");
				}
			}
		}
		foreach(int i in tobeRemoved)
		{
			newGeo.edges.RemoveAt(i);
		}
		return newGeo;
		/*List<Line> hiddenLines = new List<Line> ();
		//ForEach first path point:
		//Identify lines behind which to hide
		//List<Geometry> shadowPolyTemp = (List<Geometry>)hTable [pathPoints [startIndex]];
		//foreach(Geometry geo in shadowPolyTemp)
		{
			foreach(Line l in geo.edges)
			{
				List<Vector3> pair = l.PointsOnEitherSide(0.02f);
				
				int ct_insideObstacle=0;
				foreach(Vector3 pt in pair)
				{
					foreach(Geometry g in globalPolygon)
					{
						if(g.PointInside(pt))
						{
							ct_insideObstacle++;
						}
					}
				}
				if(ct_insideObstacle==1)
				{
					hiddenLines.Add(l);
				}
			}
		}
		hiddenLines.RemoveAll(item=>item==null);*/

		//StandardPolygon sdTemp = new StandardPolygon();
		foreach(Line l in geo.edges)
		{
			sdTemp.addPoint(l.vertex[0]);
			sdTemp.addPoint(l.vertex[1]);
		}
		sdTemp.removeDuplicates();
		//Debug.Log("sdTemp.getVertices().Count = "+sdTemp.getVertices().Count);
		Debug.Log("geo.edges.Count = "+geo.edges.Count);
		List<Vector3> copyPts = new List<Vector3> ();
		copyPts.AddRange (sdTemp.getVertices ());
		Geometry geo2 = new Geometry ();
		for(int i=0;i<copyPts.Count;i++)
		{
			for(int j=0;j<copyPts.Count;j++)
			{
				if(i==j)
					continue;
				Line lineTemp = new Line(copyPts[i],copyPts[j]);
				foreach(Line l in geo.edges)
				{
					if(l.Equals(lineTemp))
					{
						if(!checkIfLineExists(lineTemp,geo2.edges))
							geo2.edges.Add(lineTemp);
					}
				}
			}
		}
		//geo2.edges.AddRange (hiddenLines);
		return geo2;
	}
	private List<Line> removeLineFromList(List<Line> allEdges1,Line edgeToRemove)
	{
		int counter = 0;
		foreach(Line l in allEdges1)
		{
			if(l.Equals(edgeToRemove))
			{
				allEdges1.RemoveAt(counter);
				//Debug.Log("Removed edge");
				break;
			}
			counter++;
		}
		List<Line> allEdges = new List<Line> ();
		allEdges.AddRange (allEdges1);
		return allEdges;
	}
	private List<Vector3> arrangePointsFromPoint(Vector3 pPoint, List<Vector3> endPts)
	{
		Vector3 normalVect = new Vector3 (0, 1, 0);
		Vector3 xVect = new Vector3 (1, 0, 0);
		Vector3 alongX = new Vector3(pPoint.x+2,pPoint.y,pPoint.z);

		List<Vector3> arrangedPoints = new List<Vector3> ();
		List<float> angles = new List<float>();
		
		foreach(Vector3 vect in endPts)
		{
			float sAngle = SignedAngleBetween(pPoint-vect,alongX-pPoint,normalVect);
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
			arrangedPoints.Add(endPts[indexAngle]);
			angles[indexAngle]=370;
			numTemp--;
		}
		return arrangedPoints;
	}
	private List<StandardPolygon> arrangeCounterClockwise(Geometry geo)
	{
		if(bShowLogs)
			Debug.Log("Entering arrangeCounterClockwise ");
		Line firstLine = geo.edges [0];
		Line secondLine = geo.edges [1];
		Vector3? cEndPt = firstLine.getCommonEndPoint (secondLine);
		if(cEndPt==null)
		{
			Debug.Log ("@@@@@@@@@@@@@@@@@@@ Should not be here");
			return null;
		}
		Vector3 commonEndPt = cEndPt.Value;
		Vector3 endPointThird = secondLine.GetOther(commonEndPt);
		bool counterClk = firstLine.CounterClockWise (firstLine.vertex[0],firstLine.vertex[1],endPointThird);
		Line begLine;
		if(counterClk)
		{
			begLine = firstLine;
		}
		else
		{
			begLine = new Line(firstLine.vertex[1],firstLine.vertex[0]);
		}
		List<StandardPolygon> sdList = new List<StandardPolygon>();
		StandardPolygon sd = new StandardPolygon ();
		sd.addPoint (begLine.vertex [0]);
		sd.addPoint (begLine.vertex [1]);
		bool addedAnother = true;
		List<Line> allEdges = new List<Line> ();
		allEdges.AddRange (geo.edges);
		//begLine.name = "edge 1";
		//%%%%%%%%%%%%%%%%%%%%%%
		//begLine.DrawVector (allLineParent);
		int ff = 1;
		/*foreach(Line l2 in allEdges)
		{
			l2.name = "edge "+ff; 
			l2.DrawVector (allLineParent);
			ff++;
		}*/
		StandardPolygon sdTemp = new StandardPolygon();
		/*foreach(Line l2 in allEdges)
		{
			sdTemp.addPoint(l2.vertex[0]);
			sdTemp.addPoint(l2.vertex[1]);
		}
		Debug.Break ();*/
		//allEdges = removeLineFromList (allEdges, begLine);
		/////////////////////////////
		Line lToRemove = begLine;
		int numRuns = 0;
		while(allEdges.Count>1)
		{
			numRuns++;
			if(numRuns>500)
			{
				Debug.Log("##################### Broke due to more infinity loop #############################");
				break;
			}
			bool addedAnotherPt = false;
			//int beforeInt = allEdges.Count;
			if(lToRemove!=null)
				allEdges = removeLineFromList (allEdges,lToRemove);
			//int afterInt = allEdges.Count;
			/*if(beforeInt==afterInt)
			{
				Debug.Log("%%%%%%%%%%%%%%%%%%%% Line not deleted");
			}*/

			////////////////////////////////////Start Here///////////////////////////////////////////////
			List<Vector3> endPts = new List<Vector3>();
			foreach(Line l in allEdges)
			{
				cEndPt = l.getCommonEndPoint (begLine);
				if(cEndPt==null)
				{
					continue;
				}
				commonEndPt = cEndPt.Value;
				if(commonEndPt.Equals(begLine.vertex[0]))
				{
					continue;
				}
				//Now common end point is begline.vertex[1]
				endPointThird = l.GetOther(begLine.vertex[1]);
				endPts.Add(endPointThird);
			}
			endPts = arrangePointsFromPoint(begLine.vertex[1],endPts);
			if(endPts.Count<=0)
				Debug.Log("%%%%%%%%%%%%%%%%%%%% arrangePointsFromPoint gives 0 end points");
			lToRemove = new Line(begLine.vertex[1],endPts[0]);
			begLine = new Line(begLine.vertex[1],endPts[0]);
			addedAnotherPt = sd.addPoint(endPts[0]);
			//ff++;
			//lToRemove.name = "edge "+ff; 
			//lToRemove.DrawVector (allLineParent);
			/*if(ff==10)
			{
				Debug.Log("edge 10 length = "+Vector3.Distance(lToRemove.vertex[0],lToRemove.vertex[1]));
			}*/
			if(lToRemove!=null)
				allEdges = removeLineFromList (allEdges,lToRemove);
			lToRemove=null;
			
			if(!addedAnotherPt)
			{
				Debug.Log("%%%%%%%%%%%%%%%%%%%% Pt not added. Must have completed a polygon.");
				StandardPolygon sdPolyNew = sd.makeSubPolygon(endPts[0]);
				if(sdPolyNew!=null)
					sdList.Add(sdPolyNew);
				//sdList.Add(sd);
				//sd = new StandardPolygon ();
			}
			continue;
			/// ///////////////////////////////////End Here////////////////////////////////////////////////////

			lToRemove = null;

			foreach(Line l in allEdges)
			{
				cEndPt = l.getCommonEndPoint (begLine);
				if(cEndPt==null)
				{
					continue;
				}
				commonEndPt = cEndPt.Value;
				if(commonEndPt.Equals(begLine.vertex[0]))
				{
					continue;
				}
				//Now common end point is begline.vertex[1]
				endPointThird = l.GetOther(begLine.vertex[1]);
				lToRemove = l;
				begLine = new Line(begLine.vertex[1],endPointThird);
				addedAnotherPt = sd.addPoint(endPointThird);
				//lToRemove.DrawVector(allLineParent);
				if(lToRemove!=null)
					allEdges = removeLineFromList (allEdges,lToRemove);
				lToRemove=null;

				if(!addedAnotherPt && allEdges.Count>=2)
				{
					Debug.Log("%%%%%%%%%%%%%%%%%%%% Pt not added. Should have been added.");
					StandardPolygon sdPolyNew = sd.makeSubPolygon(endPointThird);
					sdList.Add(sdPolyNew);
				}
				break;
			}
			/*if(!addedAnotherPt)
			{
				sdList.Add(sd);
				sd = new StandardPolygon ();
				//break;
			}*/
		}
		sdList.Add(sd);
		/*foreach(Line l2 in allEdges)
		{
			l2.name = "edge "+ff;
			ff++;
			l2.DrawVector(allLineParent);
		}*/
		return sdList;
		/// //////////////////////////
		/// 
		/// 
		/// 
		/// 
		/// 
		/// 
		/// 
		//while(geo.edges.Count>sd.getVertices ().Count)
		while(addedAnother)
		{
			addedAnother = false;

			foreach(Line l in allEdges)
			{
				if(!l.Equals(begLine))
				{
					cEndPt = l.getCommonEndPoint (begLine);
					if(cEndPt==null)
					{
						continue;
					}
					commonEndPt = cEndPt.Value;
					if(commonEndPt==begLine.vertex[0])
					{
						continue;
					}
					endPointThird = l.GetOther(commonEndPt);
					begLine = new Line(commonEndPt,endPointThird);
					addedAnother = sd.addPoint(endPointThird);

					if(addedAnother)
					{
						if(bShowLogs)
						{
							Debug.Log("In arrangeCounterClockwise : added point. Count = "+sd.getVertices().Count);
						}
						int indx = sd.getVertices().Count;
						Line lTemp = new Line(sd.getVertices()[indx-2],sd.getVertices()[indx-1]);
						indx--;
						lTemp.name = "edge "+indx;
						//lTemp.DrawVector (allLineParent);
						if(indx==4 && allEdges.Count>2)
						{
							foreach(Line l2 in geo.edges)
							{
								if(l2.Equals(lTemp))
								{
									
									Debug.Log("%%%%%%%%%%%%%%%%%%%% The edge is in geometry");
									break;
								}
							}
						}
						allEdges = removeLineFromList (allEdges,l );
						break;
					}
					else
					{
						Debug.Log("In arrangeCounterClockwise : DID NOT add point.");
					}
				}
			}
		}
		Debug.Log ("allEdges.Count = " + allEdges.Count);
		//Till here added all points in standard polygon  in counterclockwise fashion.
		if (geo.edges.Count != sd.getVertices ().Count)
		{
			Debug.Log("geo.edges.Count = "+geo.edges.Count);
			Debug.Log ("@@@@@@@@@@@@@@@@@@@ Counter clockwise polygon is not complete");
		}
		/*foreach(Line l in geo.edges)
		{
			showPosOfPoint(l.vertex[0]);
			showPosOfPoint(l.vertex[1]);
		}*/
		//sd.removeDuplicates();
		/*foreach(Vector3 vect in sd.getVertices())
		{
			showPosOfPoint(vect);
		}*/
		if(bShowLogs)
		{
			Debug.Log("arranged CounterClockwise ");
		}

		return sdList;
	}
	void showPosOfPoint(Vector3 pos)
	{
		GameObject sp = (GameObject)GameObject.Find ("StartPoint");
		GameObject tempObj = (GameObject)GameObject.Instantiate (sp);
		tempObj.transform.position=pos;
	}
	List<int> applyEarClipping(/*Geometry shadowGeo,*/ List<Vector3> points)
	{
		Geometry shadowGeo = new Geometry ();
		for(int i=0;i<points.Count;i++)
		{
			shadowGeo.edges.Add(new Line(points[i],points[(i+1)%points.Count]));
		}
		List<int> newTriangles = new List<int>();
		if (points.Count <= 2)
			return newTriangles;
		if(points.Count==3)
		{
			newTriangles.Add(2);
			newTriangles.Add(1);
			newTriangles.Add(0);

			return newTriangles;
		}
		List<Vector3> copyPoints = new List<Vector3>();
		copyPoints.AddRange (points);
		if(bShowLogs)
			Debug.Log ("applyEarClipping " + copyPoints.Count);
		int numPts = copyPoints.Count;
		int numRuns = 0;
		while(true)
		{
			int itr=0;
			while(itr<numPts)
			{
				numRuns++;
				if(numPts<=3)
					break;
				if(numRuns>50)
				{

					Debug.Log("numRuns>50 ******* SHOULD NEVER BE HERE ******* ");
					Debug.Log("numRuns "+numRuns+") "+"numPts "+numPts+" itr "+itr);
					/*foreach(Vector3 pt in copyPoints)
					{
						showPosOfPoint(pt);
					}*/
					return newTriangles;
				}
				Line tmpLine = new Line(copyPoints[itr],copyPoints[(itr+2)%numPts]);
				//first check:mid point inside
				if(shadowGeo.PointInside(tmpLine.MidPoint()))
				{
					if(bShowLogs)
					{
						Debug.Log("MidPoint Inside");
						Debug.Log("numRuns "+numRuns+") "+"numPts "+numPts+" itr "+itr);
					}
					//second check
					bool bIntersects=false;
					foreach(Line edge in shadowGeo.edges)
					{
						if(edge.CommonEndPoint(tmpLine))
							continue;
						if(edge.LineIntersectMuntacEndPt(tmpLine)!=0)
						{
							bIntersects=true;
							break;
						}
					}
					if(!bIntersects)
					{
						newTriangles.Add(points.IndexOf(copyPoints[itr]));
						newTriangles.Add(points.IndexOf(copyPoints[(itr+1)%numPts]));
						newTriangles.Add(points.IndexOf(copyPoints[(itr+2)%numPts]));
						copyPoints.RemoveAt((itr+1)%numPts);
						if((itr+1)%numPts==0)
						{
							itr--;
						}
						numPts--;
						if(bShowLogs)
						{
							Debug.Log("Does not intersect");
							Debug.Log("numRuns "+numRuns+") "+"numPts "+numPts+" itr "+itr);
						}


					}
					else
					{
						itr++;
					}
				}
				else
				{
					itr++;
				}
			}
			if(numPts<=3)
				break;
		}
		newTriangles.Add(points.IndexOf(copyPoints[0]));
		newTriangles.Add(points.IndexOf(copyPoints[1]));
		newTriangles.Add(points.IndexOf(copyPoints[2]));

		if(bShowLogs)
			Debug.Log("applyEarClipping returning");
		return newTriangles;
	}
	//Predefined Tuple class
	public class Tuple<T1, T2>
	{
		public T1 First { get; private set; }
		public T2 Second { get; private set; }
		internal Tuple(T1 first, T2 second)
		{
			First = first;
			Second = second;
		}
	}
	public static class Tuple
	{
		public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second)
		{
			var tuple = new Tuple<T1, T2>(first, second);
			return tuple;
		}
	}
	private GameObject allLineParent;
	private bool pointInShadow(Vector3 pt,int Indx)
	{
		List<Geometry> shadowPolyTemp = (List<Geometry>)hTable [pathPoints [Indx]];
		foreach(Geometry geo in shadowPolyTemp)
		{
			if(geo.PointInside(pt))
				return true;
		}
		return false;
	}
	//Key = path point (Vector3), Value = 2D array of 1's represent visible, 0's shadows, 2's boundary of obstacle ...
	Hashtable h_discreteShadows = new Hashtable();
	//Key = (i,j) , Value = Vector3. i,j are indices corresponding to 2D arrays in h_discreteShadows
	Hashtable h_mapIndxToPt = new Hashtable();
	//Func: fills h_discreteShadows and h_mapIndxToPt according to step and pathPoints
	private void createDiscreteMap()
	{
		float minX = mapBoundary[0].x;
		float minZ = mapBoundary[0].z;
		float maxX = mapBoundary[0].x;
		float maxZ = mapBoundary[0].z;
		for(int i=1;i<4;i++)
		{
			if(minX>mapBoundary[i].x)
			{
				minX=mapBoundary[i].x;
			}
			if(minZ>mapBoundary[i].z)
			{
				minZ=mapBoundary[i].z;
			}
			if(maxX<mapBoundary[i].x)
			{
				maxX=mapBoundary[i].x;
			}
			if(maxZ<mapBoundary[i].z)
			{
				maxZ=mapBoundary[i].z;
			}
		}
		int Indx = 0;
		float step = 0.1f;
		Debug.Log((maxX - minX) / step);
		return;
		int discretePts = (int)((maxX - minX) / step);
		while(Indx<pathPoints.Count)
		{
			List<Geometry> shadowPolyTemp = (List<Geometry>)hTable [pathPoints [Indx]];

			sbyte[,] shadowArray = new sbyte[discretePts,discretePts];


			float radius_hiddenSphere = ((SphereCollider)hiddenSphere.collider).radius*((SphereCollider)hiddenSphere.collider).transform.lossyScale.x;
			int j1=0;	
			for(float j=minX;j<maxX;j+=step)
			{
				int k1=0;
				for(float k=minZ;k<maxZ;k+=step)
				{
					Vector3 pt = new Vector3(j,1,k);

					/*Tuple a1(j1,k1);
					if(!h_mapIndxToPt.ContainsKey(a1))
					{
						h_mapIndxToPt.Add(new Tuple(j1,k1),pt);
					}
					*/
					if(pointInShadow(pt,Indx) && !Physics.CheckSphere(pt,radius_hiddenSphere))
					{
						shadowArray[j1,k1]=0;
					}
					else
					{
						shadowArray[j1,k1]=1;
					}
					k1++;
				}
				j1++;
			}
			Indx++;
			h_discreteShadows.Add(pathPoints[Indx],shadowArray);

		}
	}
	private void displayStrategicPoints (int Indx)
	{
		Geometry visiblePolyTemp = (Geometry)hVisiblePolyTable [pathPoints [Indx]];
		if(visiblePolyTemp!=null)
		{
			List<Line> allEdges = new List<Line> ();
			allEdges.AddRange (visiblePolyTemp.edges);
			int ff = 1;
			foreach(Line l2 in allEdges)
			{
				l2.name = "edge "+ff; 
				l2.DrawVector (allLineParent);
				ff++;
			}
		}
		float minX = mapBoundary[0].x;
		float minZ = mapBoundary[0].z;
		float maxX = mapBoundary[0].x;
		float maxZ = mapBoundary[0].z;
		for(int i=1;i<4;i++)
		{
			if(minX>mapBoundary[i].x)
			{
				minX=mapBoundary[i].x;
			}
			if(minZ>mapBoundary[i].z)
			{
				minZ=mapBoundary[i].z;
			}
			if(maxX<mapBoundary[i].x)
			{
				maxX=mapBoundary[i].x;
			}
			if(maxZ<mapBoundary[i].z)
			{
				maxZ=mapBoundary[i].z;
			}
		}
		List<Geometry> shadowPolyTemp = (List<Geometry>)hTable [pathPoints [Indx]];
		float step = 0.1f;
		for(int i=0;i<shadowPolyTemp.Count;i++)
		{
			////////////////////////////////////////////
			//List<StandardPolygon> sdList = arrangeCounterClockwise(shadowPolyTemp[i]);
			/*List<Line> allEdges = new List<Line> ();
			allEdges.AddRange (shadowPolyTemp[i].edges);
			int ff = 1;
			foreach(Line l2 in allEdges)
			{
				l2.name = "edge "+ff; 
				l2.DrawVector (allLineParent);
				ff++;
			}*/

			////////////////////////////////////////////;
			float radius_hiddenSphere = ((SphereCollider)hiddenSphere.collider).radius*((SphereCollider)hiddenSphere.collider).transform.lossyScale.x;
			//Debug.Log ("radius" + radius_hiddenSphere);
			hiddenSphereList = new List<GameObject> ();
			//Foreach point:
			for(float j=minX;j<maxX;j+=step)
			{
				for(float k=minZ;k<maxZ;k+=step)
				{
					Vector3 pt = new Vector3(j,1,k);
					if(pointInShadow(pt,Indx) && !Physics.CheckSphere(pt,radius_hiddenSphere))
					{
						GameObject clone1 = (GameObject)Instantiate(hiddenSphere);
						clone1.transform.position = pt;
						hiddenSphereList.Add(clone1);
					}
				}
			}
		}
	}
	private void displayShadowMeshes(int Indx)
	{
		//get the shadow polygons
		if(bShowLogs)
			Debug.Log("displayShadowMeshes = " + Indx);

		List<Geometry> shadowPolyTemp = (List<Geometry>)hTable [pathPoints [Indx]];
		//List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles;// = new List<int>();
		//For each shadow polygon


		for(int i=0;i<shadowPolyTemp.Count;i++)
		{
			Geometry shadowGeo = shadowPolyTemp[i];
			if(bShowLogs)
				Debug.Log("shadowGeo = "+shadowGeo.edges);
			Geometry geo = consolidateShadowPolygon (shadowGeo);

			List<StandardPolygon> sdList = arrangeCounterClockwise(geo);
			Debug.Log("sdList.Count = "+sdList.Count);

			foreach(StandardPolygon sd in sdList)
			{
				Debug.Log("sd.Count = "+sd.getVertices().Count);
			List<Vector3> points = sd.getVertices();
			
			newTriangles = applyEarClipping(/*shadowGeo*/points);
			/////////////////Just for debugging////////////////////
			for(int counter=0;counter<points.Count;counter++)
			{
				Line l = new Line(points[counter],points[(counter+1)%points.Count]);
				l.name = "edge "+counter;
				l.DrawVector (allLineParent);
			}
			/*for(int counter=0;counter<geo.edges.Count;counter++)
			{
				Line l = geo.edges[counter];
				l.name = "edge "+counter;
				l.DrawVector (allLineParent);
			}*/
			/////////////////END: Just for debugging////////////////////
			if(bShowLogs)
			{
				Debug.Log("newTriangles = "+newTriangles.Count);
				Debug.Log("points = "+points.Count);
			}

			shadowMeshes.Add(new GameObject("ShadowMesh"));
			MeshFilter filter = shadowMeshes[shadowMeshes.Count-1].AddComponent<MeshFilter>();
			//shadowMeshes[shadowMeshes.Count-1].GetComponent<MeshFilter>();
			//filter.sharedMesh = mesh;
			MeshRenderer meshRenderer = shadowMeshes[shadowMeshes.Count-1].AddComponent<MeshRenderer>();
			
			Material material = mat;//Resources.Load("Shadow", typeof(Material)) as Material;
			if(material==null)
			{
				Debug.Log("material not found");
			}
			meshRenderer.material = material;
			
			Mesh mesh = filter.sharedMesh;
			if(mesh==null)
			{
				mesh = new Mesh();
				filter.sharedMesh = mesh;
			}
			Vector2[] uvs = new Vector2[points.Count];

			//shadowMeshes[shadowMeshes.Count-1].transform.position = new Vector3(0,1,0);

			int i2 = 0;
			while (i2 < uvs.Length) 
			{
				uvs[i2] = new Vector2(points[i2].x, points[i2].z);
				i2++;
			}
			
			Vector3[] normals = new Vector3[points.Count];
			for (i2 = 0; i2 < normals.Length; i2++) 
			{
				normals[i2] = Vector3.up;
			}
			
			
			mesh.vertices = points.ToArray();
			mesh.uv = uvs;
			mesh.triangles = newTriangles.ToArray();
			mesh.normals=normals;
			
			filter.mesh = mesh;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			//newTriangles.Clear();
			}
		}
		//bCallComplete=true;
	}

	int shadowCounter=1;
	private void showShadowMesh ()
	{
		Debug.Log("Creating shadows "+shadowCounter++);
		List<Vector3> newVertices = new List<Vector3>();
		Vector2[] newUV;
		List<Line> newLines = new List<Line> ();
		List<Line> allLines = new List<Line> ();
		List<int> newTriangles = new List<int>();

		List<Geometry> shadowPolyTemp = (List<Geometry>)hTable [pathPoints [startIndex]];
		for(int i=0;i<shadowPolyTemp.Count;i++)
		{
			shadowMeshes.Add(new GameObject("ShadowMesh"));
			MeshFilter filter = shadowMeshes[shadowMeshes.Count-1].AddComponent<MeshFilter>();
			//shadowMeshes[shadowMeshes.Count-1].GetComponent<MeshFilter>();
			//filter.sharedMesh = mesh;
			MeshRenderer meshRenderer = shadowMeshes[shadowMeshes.Count-1].AddComponent<MeshRenderer>();

			Material material = mat;//Resources.Load("Shadow", typeof(Material)) as Material;
			if(material==null)
			{
				Debug.Log("material not found");
			}
			meshRenderer.material = material;

			Mesh mesh = filter.sharedMesh;
			if(mesh==null)
			{
				mesh = new Mesh();
				filter.sharedMesh = mesh;
			}
			//mesh.Clear();
			//foreach(Geometry geo in shadowPolyTemp)
			//{
			foreach(Line edge in shadowPolyTemp[i].edges)
			{
				//May contain error as same point can be represented by two points which are quite near.
				if(!ListContainsPoint(newVertices,edge.vertex[0]))
				{
					newVertices.Add(edge.vertex[0]);
				}
				if(!ListContainsPoint(newVertices,edge.vertex[1]))
				{
					newVertices.Add(edge.vertex[1]);
				}
			}
			//}
			// Creating diagonals as newLines
			for(int j=0;j<newVertices.Count;j++)
			{
				for(int k=0;k<newVertices.Count;k++)
				{
					int diff = Mathf.Abs(j-k);
					//ignoring neighbours
					if(diff==0 || diff==1 || newVertices.Count%diff==1)
						continue;
					Line tmpLine = new Line(newVertices[j],newVertices[k]);
					//first check:mid point inside
					if(shadowPolyTemp[i].PointInside(tmpLine.MidPoint()))
					{
						//second check
						bool bIntersects=false;
						foreach(Line edge in shadowPolyTemp[i].edges)
						{
							if(edge.CommonEndPoint(tmpLine))
								continue;
							if(edge.LineIntersectMuntacEndPt(tmpLine)!=0)
							{
								bIntersects=true;
								break;
							}
						}
						if(!bIntersects)
						{
							newLines.Add(new Line(newVertices[j],newVertices[k]));
						}
					}
				}
			}
			allLines.AddRange(shadowPolyTemp[i].edges);
			allLines.AddRange(newLines);

			foreach(Line l in allLines)
			{
				//Debug.Log("From "+l.vertex[0]+" to "+l.vertex[1]);
				//Debug.DrawLine(l.vertex[0],l.vertex[1]);
				l.DrawLine(Color.red);

					GameObject pathObj;
					pathObj = Instantiate(pathSphere, 
				                      l.vertex[0], 
					                      pathSphere.transform.rotation) as GameObject;
				pathObj = Instantiate(pathSphere, 
				                      l.vertex[1], 
				                      pathSphere.transform.rotation) as GameObject;
				
			}



			//Creating triangles using diagonals
			foreach(Line diagonal in newLines)
			{
				foreach(Line anyLine in allLines)
				{
					if(diagonal.Equals(anyLine))
						continue;
					if(diagonal.CommonEndPoint(anyLine))
					{
						Vector3? commonEndPtTemp = diagonal.getCommonEndPoint(anyLine);
						if(commonEndPtTemp==null)
							Debug.Log ("Should not be here");
						Vector3 commonEndPt = commonEndPtTemp.Value;
						Vector3 endPoint1 = diagonal.GetOther(commonEndPt);
						Vector3 endPoint2 = anyLine.GetOther(commonEndPt);
						foreach(Line anyline2 in allLines)
						{
							if(anyline2.PointOnLine(endPoint1) && anyline2.PointOnLine(endPoint2))
							{
								/*if(ListContainsPoint(newVertices,commonEndPt))
								{
									if(newVertices.IndexOf(commonEndPt)<0)
									{
										Debug.Log(commonEndPt+" is inside list but index is not identified");
									}
								}
								if(ListContainsPoint(newVertices,endPoint1))
								{
									if(newVertices.IndexOf(endPoint1)<0)
									{
										Debug.Log(endPoint1+" is inside list but index is not identified");
									}
								}
								if(ListContainsPoint(newVertices,endPoint2))
								{
									if(newVertices.IndexOf(endPoint2)<0)
									{
										Debug.Log(endPoint2+" is inside list but index is not identified");
									}
								}
								*/
								//Debug.Log(newVertices.IndexOf(commonEndPt));
								//Debug.Log(newVertices.IndexOf(endPoint1));
								//Debug.Log(newVertices.IndexOf(endPoint2));
								newTriangles.Add(IndexInList(newVertices,commonEndPt));
								newTriangles.Add(IndexInList(newVertices,endPoint1));
								newTriangles.Add(IndexInList(newVertices,endPoint2));
								//newTriangles.Add(newVertices.IndexOf(commonEndPt));
								//newTriangles.Add(newVertices.IndexOf(endPoint1));
								//newTriangles.Add(newVertices.IndexOf(endPoint2));
							}
						}
					}
				}
			}
			/*for(int itr = 0;itr<newTriangles.Count-2;itr+=3)
			{
				Debug.DrawLine(newVertices[newTriangles[itr]],newVertices[newTriangles[itr+1]]);
				Debug.DrawLine(newVertices[newTriangles[itr+1]],newVertices[newTriangles[itr+2]]);
				Debug.DrawLine(newVertices[newTriangles[itr+2]],newVertices[newTriangles[itr]]);
			}*/

			Vector2[] uvs = new Vector2[newVertices.Count];
			int i2 = 0;

			shadowMeshes[shadowMeshes.Count-1].transform.position = new Vector3(0,1,0);//newVertices[0];
			/*for(i2=0;i2<newVertices.Count;i2++)
			{
				newVertices[i2]-=shadowMeshes[shadowMeshes.Count-1].transform.position;
			}*/
			i2 = 0;
			while (i2 < uvs.Length) 
			{
				uvs[i2] = new Vector2(newVertices[i2].x, newVertices[i2].z);
				i2++;
			}

			Vector3[] normals = new Vector3[newVertices.Count];
			for (i2 = 0; i2 < normals.Length; i2++) 
			{
				normals[i2] = Vector3.up;
			}


			mesh.vertices = newVertices.ToArray();
			mesh.uv = uvs;
			mesh.triangles = newTriangles.ToArray();
			mesh.normals=normals;

			filter.mesh = mesh;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			newTriangles.Clear();
			allLines.Clear();
			newLines.Clear();
			newVertices.Clear();
		}
		Debug.Break();

	}
	bool AnalyzeNearestPathPoint()
	{
		float dist=1000;
		int index = -1;
		Vector3 mousePos = camObj.ScreenToWorldPoint(Input.mousePosition);
		mousePos.y=1;
		for(int i=0;i<pathPoints.Count;i++)
		{
			float distTemp = Vector3.Distance(pathPoints[i],mousePos);
			if(distTemp<dist)
			{
				dist=distTemp;
				index=i;
			}
		}
		//Debug.Log ("Found dist " + dist);
		if (dist > 0.05)
			return false;
		Debug.Log ("Found index " + index);

			
		displayShadowMeshes(index);

		return true;
	}
	bool bNearBy = false;
	bool bCallComplete=true;
	void Update () 
	{
				if (bCallComplete) {
						bCallComplete = false;
						//bNearBy = AnalyzeNearestPathPoint ();
						bCallComplete = true;
				} else {
						return;
				}
				if (Input.GetMouseButtonDown (0)) {
						GameObject.Destroy (selectedBox);
						start_box = Input.mousePosition;
				}
		
				if (Input.GetMouseButtonUp (0)) {
						end_box = Input.mousePosition;
						start_box = camObj.ScreenToWorldPoint (start_box);
						start_box.y = 1;
						end_box = camObj.ScreenToWorldPoint (end_box);
						end_box.y = 1;
						Debug.Log (start_box + "," + end_box);
						makeBox ();
						//TODO:Uncomment
						//IdentifyGoodHidingSpots();

						/*if(startIndex!=-1 && startIndex==endIndex)
			{
				Debug.Log("Calling displayShadowMeshes "+ startIndex);
				Debug.Break();
				displayShadowMeshes(startIndex);
				//showShadowMesh();
			}*/

				}
		if(false)
		{
				List<Geometry> shadowPolyTemp = (List<Geometry>)hTable [pathPoints [pathIndexToShowShadow]];
				foreach (Geometry geo in shadowPolyTemp) {
						List<Line> hiddenLines = new List<Line> ();
						//ForEach first path point:
						//Identify lines behind which to hide
						//List<Geometry> shadowPolyTemp = (List<Geometry>)hTable [pathPoints [startIndex]];
						//foreach(Geometry geo in shadowPolyTemp)
						{
								foreach (Line l in geo.edges) {
										List<Vector3> pair = l.PointsOnEitherSide (0.02f);
					
										int ct_insideObstacle = 0;
										foreach (Vector3 pt in pair) {
												foreach (Geometry g in globalPolygon) {
														if (g.PointInside (pt)) {
																ct_insideObstacle++;
														}
												}
										}
										if (ct_insideObstacle == 1) {
												hiddenLines.Add (l);
										}
								}
						}
						hiddenLines.RemoveAll (item => item == null);


						/*foreach (Line l in hiddenLines) {
								l.DrawLine ();
						}*/
						foreach (Line l in geo.edges) {
								l.DrawLine ();
						}

				}
				Debug.Break ();
		}
		
	}
	int startIndex = -1;
	int endIndex = -1;
	void IdentifyGoodHidingSpots ()
	{
		if (!b_ShowBoundbox)
			return;
		if (startIndex == -1)
			return;
		List<Line> hiddenLines = new List<Line> ();
		//ForEach first path point:
		//Identify lines behind which to hide
		List<Geometry> shadowPolyTemp = (List<Geometry>)hTable [pathPoints [startIndex]];
		foreach(Geometry geo in shadowPolyTemp)
		{
			foreach(Line l in geo.edges)
			{
				List<Vector3> pair = l.PointsOnEitherSide(0.02f);

				int ct_insideObstacle=0;
				//int ct_insideBoundary=0;
				foreach(Vector3 pt in pair)
				{
					foreach(Geometry g in globalPolygon)
					{
						if(g.PointInside(pt))
						{
							ct_insideObstacle++;
						}
					}
				}
				if(ct_insideObstacle==1)
				{
					hiddenLines.Add(l);
				}
			}
		}
		/* Used for removing lines which are open towards path point
		currIndex = 0;
		foreach(Line l in hiddenLines)
		{
			Vector3 midPt = l.MidPoint();
			Vector3 tempPt = Vector3.MoveTowards(midPt,pathPoints[startIndex],0.1f);
			bool b_insideObs=false;
			foreach(Geometry g in globalPolygon)
			{
				if(g.PointInside(tempPt))
				{
					b_insideObs=true;
				}
			}
			if(!b_insideObs)
			{
				hiddenLines[currIndex]=null;
			}
			
			currIndex++;
		}
		*/
		hiddenLines.RemoveAll(item=>item==null);
		float radius_hiddenSphere = ((SphereCollider)hiddenSphere.collider).radius*((SphereCollider)hiddenSphere.collider).transform.lossyScale.x;
		//Debug.Log ("radius" + radius_hiddenSphere);
		hiddenSphereList = new List<GameObject> ();
		//Foreach line:
		foreach(Line l in hiddenLines)
		{
			Vector3 towardsVect=l.vertex[0];
			while(towardsVect!=l.vertex[1])
			{
				Vector3 previous = towardsVect;
				towardsVect = Vector3.MoveTowards(previous,l.vertex[1],radius_hiddenSphere+0.01f);
				Line tempLine = new Line(previous,towardsVect);
				List<Vector3> pair = tempLine.PointsOnEitherSide(radius_hiddenSphere+0.01f);
				if(!Physics.CheckSphere(pair[0],radius_hiddenSphere))
				{
					GameObject clone1 = (GameObject)Instantiate(hiddenSphere);
					clone1.transform.position = pair[0];
					hiddenSphereList.Add(clone1);
				}
				if(!Physics.CheckSphere(pair[1],radius_hiddenSphere))
				{
					GameObject clone1 = (GameObject)Instantiate(hiddenSphere);
					clone1.transform.position = pair[1];
					hiddenSphereList.Add(clone1);
				}
			}
			////// move over line, identify point beside line inside shadow polygon
			////// , make abstract area and check if all points on hidden sphere fits in all shadow
		}
		List<Vector3> circumPoints = new List<Vector3>();
		for(int k=0;k<hiddenSphereList.Count;k++)
		{
			bool sphereFound=false;
			circumPoints.Clear();
			circumPoints.Add(new Vector3(hiddenSphereList[k].transform.position.x,1,hiddenSphereList[k].transform.position.z));
			//circumPoints.Add(new Vector3(hiddenSphereList[k].transform.position.x-radius_hiddenSphere,1,hiddenSphereList[k].transform.position.z));
			//circumPoints.Add(new Vector3(hiddenSphereList[k].transform.position.x+radius_hiddenSphere,1,hiddenSphereList[k].transform.position.z));
			//circumPoints.Add(new Vector3(hiddenSphereList[k].transform.position.x,1,hiddenSphereList[k].transform.position.z-radius_hiddenSphere));
			//circumPoints.Add(new Vector3(hiddenSphereList[k].transform.position.x,1,hiddenSphereList[k].transform.position.z+radius_hiddenSphere));
			for(int i=startIndex;i<=endIndex;i++)
			{
				shadowPolyTemp = (List<Geometry>)hTable [pathPoints [i]];
				int insideCounterTemp=0;
				sphereFound=false;
				foreach(Geometry geo in shadowPolyTemp)
				{
					insideCounterTemp=0;
					foreach(Vector3 vect in circumPoints)
					{
						if(geo.PointInside(vect))
						{
							insideCounterTemp++;
						}
					}
					//if(insideCounterTemp>0 && insideCounterTemp<4)
						//Debug.Log (insideCounterTemp);
					if(insideCounterTemp==circumPoints.Count)
					{
						sphereFound=true;
						break;
					}
				}
				if(!sphereFound)
				{
					GameObject.Destroy(hiddenSphereList[k]);
					hiddenSphereList[k]=null;
				}
			}
		}
		hiddenSphereList.RemoveAll(item=>item==null);
	}

	public void CalculateVisibilityForPath()
	{
		//globalPolygon = getObstacleEdges ();

		List<Vector3> endPoints = new List<Vector3> ();
		hTable = new Hashtable ();
		hVisiblePolyTable = new Hashtable ();
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
			/*if(pPoint==pathPoints[pathIndexToShowShadow])
			{
				foreach(List<Vector3> intersectionPts in intersectionPointsPerV)
				{
					Line l1 = new Line(intersectionPts[0],intersectionPts[intersectionPts.Count-1]);
					l1.DrawVector(allLineParent);
				}
			}*/
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
								Line l1 = new Line(intersectionPointsPerV[i][j],intersectionPointsPerV[nextIndex][k]);

								geoVisible.edges.Add(l1);
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
			//Combining all visible edges
			Geometry visiblePoly = new Geometry();
			foreach(Geometry geo in starPoly)
				visiblePoly.edges.AddRange(geo.edges);
			visiblePoly = verifyVisibilityPolygon(pPoint,visiblePoly);
			hVisiblePolyTable.Add(pPoint,visiblePoly);
			List<Geometry> shadowPoly = FindShadowPolygons(visiblePoly);
			//ValidatePolygons(shadowPoly);
			//globalTempArrangedPoints.AddRange(arrangedPoints);
			//globalTempStarPoly = visiblePoly;
			//globalTempShadowPoly = shadowPoly;
			//globalTempintersectionPointsPerV.AddRange(intersectionPointsPerV);
			//bArranged = true;
			arrangedPoints.Clear();
			hTable.Add(pPoint,shadowPoly);
		}//End: Do for all path points
	}
	Geometry verifyVisibilityPolygon(Vector3 pPoint,Geometry visiblePoly)
	{
		for(int i=0;i<visiblePoly.edges.Count;i++)
		{
			Vector3 midPt = visiblePoly.edges[i].MidPoint();
			Line l = new Line(pPoint,midPt);
			for(int j=0;j<visiblePoly.edges.Count;j++)
			{
				if(i==j)
					continue;
				if(visiblePoly.edges[j].LineIntersection(l))
				{
					/*float gradx1 = (l.vertex [1].x - l.vertex [0].x);
					float m1=0;
					if(gradx1!=0)
					{
						m1=(l.vertex[1].z - l.vertex[0].z)/gradx1;
					}
					float gradx2 = (visiblePoly.edges[i].vertex [1].x - visiblePoly.edges[i].vertex [0].x);
					float m2=0;
					if(gradx2!=0)
					{
						m2=(visiblePoly.edges[i].vertex[1].z - visiblePoly.edges[i].vertex[0].z)/gradx2;
					}
					//if(gradx1==0 && gradx2==0)
					if(m1==m2)
						continue;*/
					/*if(pPoint==pathPoints[pathIndexToShowShadow])
					{
						l.DrawVector(allLineParent);
						visiblePoly.edges[i].DrawVector(allLineParent);
					}*/
					Vector3 intsctPoint = visiblePoly.edges[j].GetIntersectionPoint(l);
					if(visiblePoly.edges[i].PointOnLine(intsctPoint))
						continue;
					visiblePoly.edges.RemoveAt(i);
					i--;
					break;
				}
			}
		}
		return visiblePoly;
	}
	void ValidatePolygons (List<Geometry> shadowPoly)
	{
		foreach(Geometry g in shadowPoly)
		{
		}
	}

	private List<Geometry> FindShadowPolygons(Geometry starPoly)
	{
		List<Vector3> verticesStar = new List<Vector3> ();
		//foreach(Geometry gStar in starPoly)
		{
			foreach(Line l in starPoly.edges)
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

		List<Geometry> modObstacles = CreateModifiedObstacles(verticesStar);

		Geometry mapModBoundary = CreateModifiedBoundary(verticesStar);
		List<Geometry> allGeometries = new List<Geometry> ();
		allGeometries.AddRange (modObstacles);
		allGeometries.Add (mapModBoundary);
		allGeometries.Add (starPoly);
		List<Geometry> shadowPoly = new List<Geometry> ();
		List<Line> listEdges = new List<Line> ();
		foreach(Geometry geo in allGeometries)
		{
			foreach(Line l in geo.edges)
			{
				List<Vector3> pair = l.PointsOnEitherSide(0.02f);
				int ct_visible=0;
				int ct_insideObstacle=0;
				int ct_insideBoundary=0;
				foreach(Vector3 pt in pair)
				{
					if(starPoly.PointInside(pt))
					{
						ct_visible++;
					}
					foreach(Geometry g in globalPolygon)
					{
						if(g.PointInside(pt))
						{
							ct_insideObstacle++;
						}
					}
					if(mapBG.PointInside(pt))
					{
						ct_insideBoundary++;
					}
				}
				//if(ct_visible>1)
				{
					//GameObject clone1 = (GameObject)Instantiate(spTemp);
					//clone1.transform.position = pair[0];
					//GameObject clone2 = (GameObject)Instantiate(spTemp);
					//clone2.transform.position = pair[1];
					//Debug.Log("ct_visible="+ct_visible+" &&&&& ct_insideObstacle = "+ct_insideObstacle+"ct_insideBoundary="+ct_insideBoundary);
					//Debug.Log(pair[0].x+","+pair[0].z+" )"+pair[1].x+","+pair[1].z);
				}
				if((ct_visible==0) || (ct_visible==1 && ct_insideObstacle==0 && ct_insideBoundary==2))
				{
					//GameObject clone1 = (GameObject)Instantiate(spTemp);
					//clone1.transform.position = pair[0];
					//GameObject clone2 = (GameObject)Instantiate(spTemp);
					//clone2.transform.position = pair[1];
					listEdges.Add(l);
				}
			}
		}
		globalTempAllShadowLines.AddRange(listEdges);
		//Concatinating all lines into geometries
		//foreach(Line l in listEdges)
		/*for(int i=0;i<listEdges.Count;i++)
		{
			if(listEdges[i]==null)
				continue;
			Geometry shadow = new Geometry();
			shadow.edges.Add(listEdges[i]);
			for(int j=i;j<listEdges.Count;j++)
			{
				if(listEdges[j]==null)
					continue;
				for(int k=0;k<shadow.edges.Count;k++)
				{
					int intsct = listEdges[j].LineIntersectMuntacEndPt(shadow.edges[k]);
					if(intsct!=0)
					{
						shadow.edges.Add(listEdges[j]);
						listEdges[j]=null;
						break;
					}
				}
			}
			shadowPoly.Add(shadow);
		}*/
		///////////////////////////////////
		for(int i=0;i<listEdges.Count;i++)
		{
			if(listEdges[i]==null)
				continue;
			Geometry shadow = new Geometry();
			shadow.edges.Add(listEdges[i]);
			listEdges[i]=null;
			for(int k=0;k<shadow.edges.Count;k++)
			{
				for(int j=0;j<listEdges.Count;j++)
				{
					if(listEdges[j]==null)
						continue;
					//int intsct = listEdges[j].LineIntersectMuntacEndPt(shadow.edges[k]);
					bool intsct = listEdges[j].CommonEndPoint(shadow.edges[k]);
					if(intsct)
					{
						shadow.edges.Add(listEdges[j]);
						listEdges[j]=null;
					}
				}
			}
			shadowPoly.Add(shadow);
		}
		///////////////////////////////////
		/*foreach(Line l in listEdges)
		{
			Debug.Log(l);
		}*/
		return shadowPoly;
	}

	List<Geometry> CreateModifiedObstacles (List<Vector3> verticesStar)
	{
		List<Geometry> modObstacles = new List<Geometry> ();
		foreach(Geometry g in globalPolygon)
		{
			Geometry obstacle = CreateModifiedPolygon(g,verticesStar);
			modObstacles.Add(obstacle);
		}
		return modObstacles;
	}

	Geometry CreateModifiedBoundary (List<Vector3> verticesStar)
	{
		Geometry mapModBoundary = CreateModifiedPolygon(mapBG,verticesStar);
		return mapModBoundary;
	}
	private Geometry CreateModifiedPolygon(Geometry g,List<Vector3> verticesStar)
	{
		Geometry obstacle = new Geometry();
		//Debug.Log("************Obstacle****************");
		foreach(Line l in g.edges)
		{
			//Debug.Log("************SameLine****************");
			List<Vector3> pointsOnSameline = new List<Vector3>();
			pointsOnSameline.Add(l.vertex[0]);
			foreach(Vector3 vect in verticesStar)
			{
				if(l.PointOnLine(vect))
				{
					if(!ListContainsPoint(pointsOnSameline,vect))
					{
						//Debug.Log(vect.x+","+vect.z);
						pointsOnSameline.Add(vect);
					}
				}
			}
			//Sort points in a line
			for(int i=1;i<pointsOnSameline.Count-1;i++)
			{
				float dist = Vector3.Distance(pointsOnSameline[0],pointsOnSameline[i]);
				int indexToReplace=-1;
				for(int j=i+1;j<pointsOnSameline.Count;j++)
				{
					float dist2 = Vector3.Distance(pointsOnSameline[0],pointsOnSameline[j]);
					if(dist>dist2)
					{
						dist=dist2;
						indexToReplace=j;
					}
				}
				if(indexToReplace>0)
				{
					Vector3 tempVar = pointsOnSameline[i];
					pointsOnSameline[i] = pointsOnSameline[indexToReplace];
					pointsOnSameline[indexToReplace] = tempVar;
				}
			}
			if(!ListContainsPoint(pointsOnSameline,l.vertex[1]))
			{
				pointsOnSameline.Add(l.vertex[1]);
			}
			for(int i=0;i<pointsOnSameline.Count-1;i++)
			{
				//Debug.Log(pointsOnSameline[i].x+","+pointsOnSameline[i].z+" to "+pointsOnSameline[i+1].x+","+pointsOnSameline[i+1].z);
				obstacle.edges.Add(new Line(pointsOnSameline[i],pointsOnSameline[i+1]));
			}
		}
		return obstacle;
	}
	private bool ListContainsPoint(List<Vector3> intersectionPoints,Vector3 intsctPoint)
	{
		float limit = 0.0001f;
		foreach (Vector3 vect in intersectionPoints) 
		{
			if(Vector3.SqrMagnitude(vect-intsctPoint)<limit)
			//if(Mathf.Approximately(vect.magnitude,intsctPoint.magnitude))
				return true;
			//Debug.Log("Points not equal"+vect+" , "+intsctPoint);
		}
		return false;
	}
	private int IndexInList(List<Vector3> myList,Vector3 intsctPoint)
	{
		float limit = 0.0001f;
		int counter = 0;
		foreach (Vector3 vect in myList) 
		{
			if(Vector3.SqrMagnitude(vect-intsctPoint)<limit)
				return counter;
			counter++;
		}
		return -1;
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
			bool pt1Found=false;
			bool pt2Found=false;
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
		//first_point = pathPts [0];
		pathPts.Add(ep.transform.position);
		findPath (pathPts);//straight Line points
		return pathPts;
	}
	private void findPath (List<Vector3> pathPts)
	{
		int iterations = 6;//increase to increase number of points on path
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
	Vector3 [] mapBoundary;
	public List<Geometry> getObstacleEdges()
	{
		//Compute one step of the discritzation
		//Find this is the view
		floor = (GameObject)GameObject.Find ("Floor");
		
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
		
		mapBoundary = new Vector3[4]; //the map's four corners
		
		for (int i = 0; i < 4; i++) {
			mapBoundary [i] = vertex [i];
		}
		
		//Geometry mapBG = new Geometry (); 
		for (int i = 0; i < 4; i++)
			mapBG.edges.Add( new Line( mapBoundary[i], mapBoundary[(i + 1) % 4]) );
		//Debug.Log ("mapBg" + mapBG.edges.Count);
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
		//lines = new List<Line> ();
		
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
