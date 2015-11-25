#define includeMultiplePaths
#if includeMultiplePaths
#define CommonMyScene1_multiplePaths
//#define CommonMGS2_multiplePaths
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
public partial class Visibility1 : MonoBehaviour 
{
	private bool pointInShadowMultiplePaths(Vector3 pt,int Indx)
	{
		if (Indx < 0)
		{
			return false;
		}
		bool bTestAllPoints = true;


		//bool bTested = false;
		List<Vector3> pointListToTest = new List<Vector3>();
		//Debug.Log (radius_enemy + " radius_enemy");
		if(bTestAllPoints)
		{
			//8 points tested
			pointListToTest.Add (pt);
			pointListToTest.Add(new Vector3(pt.x+radius_enemy,pt.y,pt.z));
			pointListToTest.Add(new Vector3(pt.x-radius_enemy,pt.y,pt.z));
			pointListToTest.Add(new Vector3(pt.x,pt.y,pt.z+radius_enemy));
			pointListToTest.Add(new Vector3(pt.x,pt.y,pt.z-radius_enemy));
			List<int> listAngleVars = new List<int>();
			listAngleVars.Add(45);
			listAngleVars.Add(-45);
			listAngleVars.Add(135);
			listAngleVars.Add(-135);
			foreach(int angleVar in listAngleVars)
			{
				Vector3 vecSel = new Vector3();
				vecSel.x = pt.x + radius_enemy*Mathf.Cos(angleVar* Mathf.Deg2Rad);
				vecSel.y = pt.y;
				vecSel.z = pt.z + radius_enemy*Mathf.Sin(angleVar* Mathf.Deg2Rad);
				pointListToTest.Add(vecSel);
			}
		}
		else
		{
			pointListToTest.Add (pt);
		}
		foreach(Vector3 pt1 in pointListToTest)
		{
			/*Vector2 pt2Temp = new Vector2(-1f,-1f);
			if(h_mapPtToIndx.ContainsKey(h_mapPtToIndx))
				pt2Temp = (Vector2)h_mapPtToIndx[pt1];*/
			
			//If outside the boundary, return false
			if (!mapBG.PointInside (pt1))
			{
				//Debug.Log("("+pt2Temp.x+" , "+pt2Temp.y+")"+"point not In Shadow. "+pt1+" is outside the boundary");
				return false;
			}
			//If inside any obstacles, return false
			foreach(Geometry geo in globalPolygon)
			{
				if(geo.PointInside(pt1))
				{
					//Debug.Log("("+pt2Temp.x+" , "+pt2Temp.y+")"+"point not In Shadow. "+pt1+" is inside an obstacle");
					return false;
				}
			}
			for(int indx = 0;indx<hTableVisibleListOfList.Count;indx++)
			{
				if(pathPointsListOfList[indx].Count>=Indx || !hTableVisibleListOfList[indx].ContainsKey(pathPointsListOfList[indx] [Indx]))
					continue;
				//bTested = true;
				Geometry visibleGeoTemp = (Geometry)hTableVisibleListOfList[indx][pathPointsListOfList[indx] [Indx]];
				if(visibleGeoTemp.PointInside(pt1))
				{
					return false;
				}
			}
		}
		/*foreach(Vector3 pt1 in pointListToTest)
			showPosOfPoint(pt1,Color.yellow);*/
		//if (bTested)
			return true;
		//else
			//return false;
	}
	public bool bMultiplePaths = false;
	List<GameObject> playerObjList = new List<GameObject>();
	List<int> nextPlayerPathList = new List<int>();
	List<List<Vector3>> pathPointsListOfList = new List<List<Vector3>> ();
	List<Hashtable> hTableVisibleListOfList = new List<Hashtable> ();
	List<Hashtable> hTableShadowListOfList = new List<Hashtable> ();
#if CommonMyScene1_multiplePaths
	Hashtable hTableShadow1 = new Hashtable ();
	Hashtable hTableVisible1 = new Hashtable ();
	Hashtable hTableShadow2 = new Hashtable ();
	Hashtable hTableVisible2 = new Hashtable ();
	Hashtable hTableShadow3 = new Hashtable ();
	Hashtable hTableVisible3 = new Hashtable ();
	List<Vector3> pathPointsList1;
	List<Vector3> pathPointsList2;
	List<Vector3> pathPointsList3;
private void setUpMultiplePaths()
	{
		pathPointsList1 = CommonMyScene1.definePathFromIndx(1);
		pathPointsList2 = CommonMyScene1.definePathFromIndx(2);
		pathPointsList3 = CommonMyScene1.definePathFromIndx(3);
		pathPointsListOfList.Add (pathPointsList1);
		pathPointsListOfList.Add (pathPointsList2);
		pathPointsListOfList.Add (pathPointsList3);
		setGlobalVars1();
		CalculateVisibilityForPath_Multiple(hTableShadow1,hTableVisible1,pathPointsList1);
		CalculateVisibilityForPath_Multiple(hTableShadow2,hTableVisible2,pathPointsList2);
		CalculateVisibilityForPath_Multiple(hTableShadow3,hTableVisible3,pathPointsList3);
		hTableVisibleListOfList.Add (hTableVisible1);
		hTableVisibleListOfList.Add (hTableVisible2);
		hTableVisibleListOfList.Add (hTableVisible3);
		hTableShadowListOfList.Add (hTableShadow1);
		hTableShadowListOfList.Add (hTableShadow2);
		hTableShadowListOfList.Add (hTableShadow3);
		if(bAgentBasedAssignment)
		{
			agentBasedAssignmentMultiplePaths();
			return;
		}
		///////////////////////////True Case//////////////////////////////
		if(m_ExecuteTrueCase)
		{
			executeTrueCase2();
			return;
		}
		if(m_CalculateTrueCase)
		{
			calculatePredictedPaths();
			return;
		}
		if(m_ShowTrueCase)
		{
			displayPredictedPaths3();
			return;
		}
		/////////////////////////////////////////////////////////
		if(!bShowJustVisibilityPoly)
		{
			foreach(List<Vector3> vectList in pathPointsListOfList)
			{
				foreach(Vector3 vect in vectList)
				{
					GameObject pathObj;
					pathObj = Instantiate(pathSphere, 
					                      vect, 
					                      pathSphere.transform.rotation) as GameObject;
				}
			}
		}
		if (bDisplayAreas)
		{
			readTimings();
			displayTimingAreas();
			return;
		}
		//shadowMeshes = new List<GameObject>();

		GameObject playerObj1 = Instantiate(playerPrefab) as GameObject;
		GameObject playerObj2 = Instantiate(playerPrefab) as GameObject;
		GameObject playerObj3 = Instantiate(playerPrefab) as GameObject;
		playerObj1.transform.position = pathPointsList1 [0];
		playerObjList.Add (playerObj1);
		nextPlayerPathList.Add (1);

		playerObj2.transform.position = pathPointsList2 [0];
		playerObjList.Add (playerObj2);
		nextPlayerPathList.Add (1);

		playerObj3.transform.position = pathPointsList3 [0];
		playerObjList.Add (playerObj3);
		nextPlayerPathList.Add (1);

		if(m_SetUpCase)
		{
			if(m_Greedy)
			{
				initializeForGreedyCaseMultiplePaths();
			}
			else if(m_NearMiss)
			{
				//initializeForNearMissCaseMultiplePaths();
			}
			else if(m_ShadowEdgeAssisted)
			{
				//initializeShadowEdgeAssistedMultiplePaths();//ForCentroidCase();
			}
			return;
		}
		////////////For Single run//////////////
		//setUpEnemyInitialPosMultiplePaths();
	}
#endif
#if CommonMGS2_multiplePaths
	Hashtable hTableShadow1 = new Hashtable ();
	Hashtable hTableVisible1 = new Hashtable ();
	Hashtable hTableShadow2 = new Hashtable ();
	Hashtable hTableVisible2 = new Hashtable ();
	Hashtable hTableShadow3 = new Hashtable ();
	Hashtable hTableVisible3 = new Hashtable ();
	Hashtable hTableShadow4 = new Hashtable ();
	Hashtable hTableVisible4 = new Hashtable ();
	Hashtable hTableShadow5 = new Hashtable ();
	Hashtable hTableVisible5 = new Hashtable ();
	Hashtable hTableShadow6 = new Hashtable ();
	Hashtable hTableVisible6 = new Hashtable ();
	List<Vector3> pathPointsList1;
	List<Vector3> pathPointsList2;
	List<Vector3> pathPointsList3;
	List<Vector3> pathPointsList4;
	List<Vector3> pathPointsList5;
	List<Vector3> pathPointsList6;
	private void setUpMultiplePaths()
	{
		pathPointsList1 = CommonMGS2.definePathFromIndx(1);
		pathPointsList2 = CommonMGS2.definePathFromIndx(2);
		pathPointsList3 = CommonMGS2.definePathFromIndx(3);
		pathPointsList4 = CommonMGS2.definePathFromIndx(4);
		pathPointsList5 = CommonMGS2.definePathFromIndx(5);
		pathPointsList6 = CommonMGS2.definePathFromIndx(6);
		CalculateVisibilityForPath_Multiple(hTableShadow1,hTableVisible1,pathPointsList1);
		CalculateVisibilityForPath_Multiple(hTableShadow2,hTableVisible2,pathPointsList2);
		CalculateVisibilityForPath_Multiple(hTableShadow3,hTableVisible3,pathPointsList3);
		CalculateVisibilityForPath_Multiple(hTableShadow4,hTableVisible4,pathPointsList4);
		CalculateVisibilityForPath_Multiple(hTableShadow5,hTableVisible5,pathPointsList5);
		CalculateVisibilityForPath_Multiple(hTableShadow6,hTableVisible6,pathPointsList6);
		pathPointsListOfList.Add (pathPointsList1);
		pathPointsListOfList.Add (pathPointsList2);
		pathPointsListOfList.Add (pathPointsList3);
		pathPointsListOfList.Add (pathPointsList4);
		pathPointsListOfList.Add (pathPointsList5);
		pathPointsListOfList.Add (pathPointsList6);
		setGlobalVars1();
		hTableVisibleListOfList.Add (hTableVisible1);
		hTableVisibleListOfList.Add (hTableVisible2);
		hTableVisibleListOfList.Add (hTableVisible3);
		hTableVisibleListOfList.Add (hTableVisible4);
		hTableVisibleListOfList.Add (hTableVisible5);
		hTableVisibleListOfList.Add (hTableVisible6);
		hTableShadowListOfList.Add (hTableShadow1);
		hTableShadowListOfList.Add (hTableShadow2);
		hTableShadowListOfList.Add (hTableShadow3);
		hTableShadowListOfList.Add (hTableShadow4);
		hTableShadowListOfList.Add (hTableShadow5);
		hTableShadowListOfList.Add (hTableShadow6);
		if(bAgentBasedAssignment)
		{
			agentBasedAssignmentMultiplePaths();
			return;
		}
		///////////////////////////True Case//////////////////////////////
		if(m_ExecuteTrueCase)
		{
			executeTrueCase2();
			return;
		}
		if(m_CalculateTrueCase)
		{
			calculatePredictedPaths();
			return;
		}
		if(m_ShowTrueCase)
		{
			displayPredictedPaths3();
			return;
		}
		/////////////////////////////////////////////////////////
		if(!bShowJustVisibilityPoly)
		{
			foreach(Vector3 vect in pathPointsList1)
			{
				GameObject pathObj;
				pathObj = Instantiate(pathSphere, 
				                      vect, 
				                      pathSphere.transform.rotation) as GameObject;
			}
			foreach(Vector3 vect in pathPointsList2)
			{
				GameObject pathObj;
				pathObj = Instantiate(pathSphere, 
				                      vect, 
				                      pathSphere.transform.rotation) as GameObject;
			}
			foreach(Vector3 vect in pathPointsList3)
			{
				GameObject pathObj;
				pathObj = Instantiate(pathSphere, 
				                      vect, 
				                      pathSphere.transform.rotation) as GameObject;
			}
		}
		if (bDisplayAreas)
		{
			readTimings();
			displayTimingAreas();
			return;
		}
		//shadowMeshes = new List<GameObject>();
		
		GameObject playerObj1 = Instantiate(playerPrefab) as GameObject;
		GameObject playerObj2 = Instantiate(playerPrefab) as GameObject;
		GameObject playerObj3 = Instantiate(playerPrefab) as GameObject;
		GameObject playerObj4 = Instantiate(playerPrefab) as GameObject;
		GameObject playerObj5 = Instantiate(playerPrefab) as GameObject;
		GameObject playerObj6 = Instantiate(playerPrefab) as GameObject;
		playerObj1.transform.position = pathPointsList1 [0];
		playerObj2.transform.position = pathPointsList2 [0];
		playerObj3.transform.position = pathPointsList3 [0];
		playerObj3.transform.position = pathPointsList4 [0];
		playerObj3.transform.position = pathPointsList5 [0];
		playerObj3.transform.position = pathPointsList6 [0];
		playerObjList.Add (playerObj1);
		playerObjList.Add (playerObj2);
		playerObjList.Add (playerObj3);
		playerObjList.Add (playerObj4);
		playerObjList.Add (playerObj5);
		playerObjList.Add (playerObj6);
		/*foreach(Line l in ((Geometry)hVisiblePolyTable[pathPoints[0]]).edges)
		{
			l.DrawVector(allLineParent);
		}*/
		if(m_SetUpCase)
		{
			if(m_Greedy)
			{
				initializeForGreedyCase();
			}
			else if(m_NearMiss)
			{
				initializeForNearMissCase();
			}
			else if(m_ShadowEdgeAssisted)
			{
				initializeShadowEdgeAssisted();//ForCentroidCase();
			}
			return;
		}
		////////////For Single run//////////////
		setUpEnemyInitialPos ();
	}
#endif
	private void CalculateVisibilityForPath_Multiple(Hashtable hTableShadow, Hashtable hTableVisible,List<Vector3> pathPointsList)
	{
		List<Vector3> endPoints = new List<Vector3> ();
		//hTable = new Hashtable ();
		//hVisiblePolyTable = new Hashtable ();
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
		Vector3 normalVect = new Vector3 (0, 1, 0);
		Vector3 xVect = new Vector3 (1, 0, 0);
		//Do for all path points
		int pathIndexTemp = -1;
		foreach(Vector3 pPoint in pathPointsList)
		{
			if(bShowJustVisibilityPoly)
			{
				pathIndexTemp++;
				if(pathIndexTemp!=bShowJustVisibilityPolyForIndex)
					continue;
			}
			if(hTableVisible.ContainsKey(pPoint))
				continue;
			Vector3 alongX = new Vector3(pPoint.x+2,pPoint.y,pPoint.z);
			List<Geometry> starPoly = new List<Geometry>();
			List<Vector3> arrangedPoints = new List<Vector3> ();
			List<float> angles = new List<float>();
			
			for(int i=0;i<endPoints.Count;i++)
			{
				Vector3 vect = endPoints[i];
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


							if(!ListContainsPoint(intersectionPoints,intsctPoint))
							{

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

						if(!ListContainsPoint(intersectionPoints,intsctPoint))
						{

							intersectionPoints.Add(intsctPoint);
						}
					}
				}
				
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
			//Remove vertex which is not visible
			foreach(List<Vector3> intersectionPts in intersectionPointsPerV)
			{
				int tmpIndex = intersectionPointsPerV.IndexOf(intersectionPts);
				
				//1st approach
				if(!VectorApprox2(intersectionPts[0],arrangedPoints[tmpIndex]))
				{
					intersectionPointsPerV[tmpIndex]=null;

				}
			}
			intersectionPointsPerV.RemoveAll(item=>item==null);

			//Remove all hidden intersection points behind visible vertices
			foreach(List<Vector3> intersectionPts in intersectionPointsPerV)
			{
				if(intersectionPts.Count<2)
					continue;
				//if second point is on same polygon, just keep the single vertex and remove all behind it

				for(int itr1=0;itr1<intersectionPts.Count-1;itr1++)
				{
					Vector3 mdPt = Vector3.Lerp(intersectionPts[itr1],intersectionPts[itr1+1],0.5f);
					if(isTheMidPtOfBoundary(mdPt) || existOnSameLineOfPolygon(intersectionPts[itr1],mdPt) 
					   || existOnSameLineOfPolygon(intersectionPts[itr1],intersectionPts[itr1+1]) || existOnSameLineOfPolygon(intersectionPts[itr1+1],mdPt))
					{
						//(new Line(pPoint,mdPt)).DrawVector(allLineParent);
						continue;
					}
					if(CheckIfInsidePolygon(mdPt) || !mapBG.PointInside(mdPt))
					{
						
						
						//showPosOfPoint(mdPt,Color.red);
						
						intersectionPts.RemoveRange(itr1+1,intersectionPts.Count-1-itr1);
						break;
						
					}
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

					geoVisible.edges.Add(new Line(intersectionPointsPerV[i][0],intersectionPointsPerV[nextIndex][0]));
				}
				//All three cases, choose points on same polygon
				else
				{
					bool bOnlyOneEdgeAdded = false;
					for(int j=0;j<intersectionPointsPerV[i].Count;j++)
					{
						if(bOnlyOneEdgeAdded)
							break;
						for(int k=0;k<intersectionPointsPerV[nextIndex].Count;k++)
						{
							if(bOnlyOneEdgeAdded)
								break;
							if(existOnSameLineOfPolygon(intersectionPointsPerV[i][j],intersectionPointsPerV[nextIndex][k]))
							{
								Line l1 = new Line(intersectionPointsPerV[i][j],intersectionPointsPerV[nextIndex][k]);
								
								geoVisible.edges.Add(l1);
								bOnlyOneEdgeAdded=true;
							}
						}
					}
				}

				starPoly.Add(geoVisible);

			}
			//Combining all visible edges
			Geometry visiblePoly = new Geometry();
			foreach(Geometry geo in starPoly)
				visiblePoly.edges.AddRange(geo.edges);
			
			hTableVisible.Add(pPoint,visiblePoly);
			List<Geometry> shadowPoly = FindShadowPolygons(visiblePoly,pathPoints.IndexOf(pPoint));

			arrangedPoints.Clear();
			hTableShadow.Add(pPoint,shadowPoly);
		}//End: Do for all path points
	}
	private void UpdateMultiplePaths()
	{
				//////////////////////////////////////////////////////////////////////////////////////

				if (bAgentBasedAssignment || bDisplayAreas || m_ExecuteTrueCase || m_ShowTrueCase || m_CalculateTrueCase) {
						Debug.Break ();
						return;
				}
	
				if (bTestingMGS2) {
						return;
				}
				if (bTestingMGS || bTestingChung || bTestingMyScene1) {
						/*Vector3 pt4 = new Vector3(-9.9f,1.0f,-6.5f);
	bool ptInShad = pointInShadow(pt4,nextPlayerPath);

	if(ptInShad)
		Debug.Log(pt4+" in shadow for "+nextPlayerPath+"rd player path");
	nextPlayerPath++;*/
						foreach (Transform child in allLineParent.transform) {
								GameObject.Destroy (child.gameObject);
						}
						//mapBG.DrawGeometry (allLineParent);
						foreach (Line l in mapBG.edges) {
								l.DrawVector (allLineParent);
						}
						for (int i=0; i<globalPolygon.Count; i++) {
								foreach (Line l in globalPolygon[i].edges) {
										l.DrawVector (allLineParent);
								}
						}
		
						return;
				}
				for (int itr=0; itr<playerObjList.Count; itr++) {
						if (playerObjList [itr].transform.position == pathPointsListOfList [itr] [pathPointsListOfList [itr].Count - 1]) {
								if (m_SetUpCase) {
										bool bNotCaught = false;
										if (m_Greedy) {
												if (!m_enemyGreedyList [0].bCaught) {
														bNotCaught = true;
												}
										} else if (m_NearMiss) {
												if (!m_enemyNearMissList [0].bCaught) {
														bNotCaught = true;
												}
										} else if (m_ShadowEdgeAssisted) {
												if (!m_enemyShadowAssistedList [0].bCaught) {
														bNotCaught = true;
												}
										}
					
										if (bNotCaught) {
												pointsArray [m_nCurrDiscretePtIndxX, m_nCurrDiscretePtIndxZ] = pathPointsListOfList [itr].Count - 1;
												timingArray [m_nCurrDiscretePtIndxX, m_nCurrDiscretePtIndxZ] = Time.time - currRunTimeEnemny;
												if (maxTimeEvaded < 0.0) {
														maxTimeEvaded = Time.time - currRunTimeEnemny;
												}
												Debug.Log ("Took " + timingArray [m_nCurrDiscretePtIndxX, m_nCurrDiscretePtIndxZ] + " Always hidden");
										}
										resetCase ();
										return;
								} else {
										Debug.Break ();
								}
						}
		
						if (playerObjList [itr].transform.position == pathPointsListOfList [itr] [nextPlayerPathList [itr]] && playerObjList [itr].transform.position != pathPointsListOfList [itr] [pathPointsListOfList [itr].Count - 1]) {
								foreach (Transform child in allLineParent.transform) {
										GameObject.Destroy (child.gameObject);
								}
								//Debug.Log("For visibility polygon for "+nextPlayerPath+" , edges.count = "+((Geometry)hVisiblePolyTable[pathPoints[nextPlayerPath]]).edges.Count);
								foreach (Line l in ((Geometry)hTableVisibleListOfList[itr][pathPointsListOfList[itr][nextPlayerPathList[itr]]]).edges) {
										l.DrawVector (allLineParent);
								}
				
								/*List<Geometry> shadowPolygonsTemp = (List<Geometry>)hTable [pathPoints [nextPlayerPath]];
			foreach(Geometry geoTemp in shadowPolygonsTemp)
			{
				foreach(Line l in geoTemp.edges)
				{
					l.DrawVector(allLineParent);
				}
			}*/
								if (bSlowShadowsDown) {
										setTimerTemp = 200;
								}
								nextPlayerPathList [itr]++;
				
				
								findNextEnemyPositionsMultiplePaths ();
				
						}
				
			Vector3 prevPlayerPos = playerObjList[itr].transform.position;
			playerObjList[itr].transform.position = Vector3.MoveTowards (playerObjList[itr].transform.position, pathPointsListOfList[itr][nextPlayerPathList[itr]], speedPlayer * Time.deltaTime);
			distBtwPlayerMovements = Vector3.Distance (prevPlayerPos, playerObjList[itr].transform.position);
			//Debug.Log ("distBtwPlayerMovements = " + distBtwPlayerMovements);
			//doPlayerMovementsMultiplePaths ();
		}
		


	}
	private int longestPathIndexInList()//last index
	{
		int count = 0;
		int Indx = -1;
		for (int i=0; i<pathPointsListOfList.Count; i++) 
		{
			if(pathPointsListOfList[i].Count>count)
			{
				count = pathPointsListOfList[i].Count;
				Indx = i;
			}
		}
		return Indx;
	}
	private int lastPathIndex()//last index
	{
		if (!bMultiplePaths)
			return pathPoints.Count-1;
		int count = 0;
		for (int i=0; i<pathPointsListOfList.Count; i++) 
		{
			if(pathPointsListOfList[i].Count>count)
				count = pathPointsListOfList[i].Count;
		}
		return count-1;
	}
	private void createDiscreteMapMultiplePaths()
	{
		int indxPathPts = longestPathIndexInList();
		int Indx = 0;
		Hashtable h_discreteShadowsTemp = new Hashtable();
		while(Indx<pathPointsListOfList[indxPathPts].Count)
		{
			List<Geometry> shadowPolyTemp = (List<Geometry>)hTableShadowListOfList[indxPathPts] [pathPointsListOfList[indxPathPts] [Indx]];
			sbyte[,] shadowArray = new sbyte[discretePtsX,discretePtsZ];
			
			float radius_hiddenSphere = ((SphereCollider)hiddenSphere.GetComponent<Collider>()).radius*((SphereCollider)hiddenSphere.GetComponent<Collider>()).transform.lossyScale.x;
			int j1=0;	
			for(float j=m_minX;j<m_maxX && j1<discretePtsX;j+=m_step)
			{
				int k1=0;
				for(float k=m_minZ;k<m_maxZ && k1<discretePtsZ;k+=m_step)
				{
					Vector3 pt = new Vector3(j,1,k);
					
					if(pointInShadowMultiplePaths(pt,Indx) && !Physics.CheckSphere(pt,radius_hiddenSphere))
					{
						shadowArray[j1,k1]=0;
					}
					else if(CheckIfInsidePolygon(pt))
					{
						shadowArray[j1,k1]=2;
					}
					else
					{
						shadowArray[j1,k1]=1;
					}
					k1++;
				}
				j1++;
			}
			h_discreteShadows.Add(pathPointsListOfList[indxPathPts] [Indx],shadowArray);
			Indx++;
		}
		
	}
	private void agentBasedAssignmentMultiplePaths()
	{
		createDiscreteMapMultiplePaths();
		int row = -1;
		int col = -1;
		int numSpots = 1;
		int indxPathPts = longestPathIndexInList();
		//Initialize:Placing agents
		sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPointsListOfList[indxPathPts]  [0]];
		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				if(shadowArray[j,k]==0)
				{
					shadowArray[j,k]=9;
				}
			}
		}
		sbyte[,] shadowArrayPrev;
		sbyte[,] shadowArrayNext;
		for(int i=0;i<pathPointsListOfList[indxPathPts].Count-1;i++)
		{
			shadowArrayPrev = (sbyte[,])h_discreteShadows [pathPointsListOfList[indxPathPts] [i]];
			shadowArrayNext = (sbyte[,])h_discreteShadows [pathPointsListOfList[indxPathPts] [i+1]];
			for(int j=0;j<discretePtsX;j++)
			{
				for(int k=0;k<discretePtsZ;k++)
				{
					if(shadowArrayPrev[j,k]==9)
					{
						placeAgent(shadowArrayNext,j,k);
					}
				}
			}
		}
		shadowArrayPrev = (sbyte[,])h_discreteShadows [pathPointsListOfList[indxPathPts] [0]];
		shadowArrayNext = (sbyte[,])h_discreteShadows [pathPointsListOfList[indxPathPts] [pathPointsListOfList[indxPathPts].Count-1]];
		Debug.Log("Agents at start = "+countAgents(shadowArrayPrev));
		Debug.Log("Agents surviving at the end = "+countAgents(shadowArrayNext));
		displaySurvivingAgentsMultiplePaths ();
	}
	private void displaySurvivingAgentsMultiplePaths()
	{
		int indxPathPts = longestPathIndexInList();
		sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPointsListOfList[indxPathPts]  [lastPathIndex()]];
		//sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPoints [pathPoints.Count-1]];
		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				if(shadowArray[j,k]==9)
				{
					GameObject clone1 = (GameObject)Instantiate(hiddenSphere);
					clone1.transform.position = (Vector3)h_mapIndxToPt[new Vector2(j,k)];
					hiddenSphereList.Add(clone1);
				}
			}
		}
	}
	private void findNextEnemyPositionsMultiplePaths()
	{
	}

	private void initializeForGreedyCaseMultiplePaths()
	{
		timingArray = new float[discretePtsX,discretePtsZ];
		pointsArray = new int[discretePtsX,discretePtsZ];
		Vector3 tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
		
		//m_nCurrDiscretePtIndxX = 27;
		//m_nCurrDiscretePtIndxZ = 15;
		//tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
		placeEnemyGreedyAt(tempVec);
		resetCase ();
	}
}
#endif