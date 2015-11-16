#define includeMultipleEnemies

using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
//using System;
public partial class Visibility1 : MonoBehaviour {

	
	#if includeMultipleEnemies
	
	private void displayTimingAreas()
	{
		readTimings ();
		for(int i=0;i<discretePtsX;i++)
		{
			for(int j=0;j<discretePtsZ;j++)
			{
				//float greenNum = pointsArray[i,j]/(pathPoints.Count-1);
				//showPosOfPoint((Vector3)h_mapIndxToPt[new Vector2(i,j)],new Color(0.0f,greenNum,0.0f));
				if(pointsArray[i,j]<=0)
					continue;
				float greenNum = (float)pointsArray[i,j]/(float)maxPathPoints;
				float G = (255 * (float)pointsArray[i,j]) / maxPathPoints;
				float R = (255 * (maxPathPoints - (float)pointsArray[i,j])) / maxPathPoints ;
				float B = 0;
				//showPosOfPoint((Vector3)h_mapIndxToPt[new Vector2(i,j)],new Color(0.0f,greenNum,0.0f));
				//showPosOfPoint((Vector3)h_mapIndxToPt[new Vector2(i,j)],new Color(R,G,B));
				//showPosOfPointRectangle((Vector3)h_mapIndxToPt[new Vector2(i,j)],Color.Lerp(Color.white,Color.grey,greenNum));
				showPosOfPointRectangle((Vector3)h_mapIndxToPt[new Vector2(i,j)],Color.Lerp(Color.white,Color.green,greenNum));
			}
		}
	}
	
	private void initializeForGreedyCase()
	{
		setGlobalVars1 ();
		timingArray = new float[discretePtsX,discretePtsZ];
		pointsArray = new int[discretePtsX,discretePtsZ];
		//Vector3 tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
		foreach(Vector3 tempVec in h_mapIndxToPt.Values)
		{
			if(pointInShadow(tempVec,0))
				placeEnemyGreedyAt(tempVec);
		}
		//resetCase ();
	}
	private void initializeForNearMissCase()
	{
		setGlobalVars1 ();
		timingArray = new float[discretePtsX,discretePtsZ];
		pointsArray = new int[discretePtsX,discretePtsZ];
		//Vector3 tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
		foreach(Vector3 tempVec in h_mapIndxToPt.Values)
		{
			if(pointInShadow(tempVec,0))
				placeEnemyNearMissAt(tempVec);
		}
		
		//resetCase ();
	}
	private void initializeShadowEdgeAssisted()
	{
		setGlobalVars1 ();
		timingArray = new float[discretePtsX,discretePtsZ];
		pointsArray = new int[discretePtsX,discretePtsZ];
		//Vector3 tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
		foreach(Vector3 tempVec in h_mapIndxToPt.Values)
		{
			if(pointInShadow(tempVec,0))
				placeEnemyShadowAssistedAt(tempVec);
		}
		
		//resetCase ();
	}
	private void initializeForCentroidCase()
	{
		setGlobalVars1 ();
		timingArray = new float[discretePtsX,discretePtsZ];
		pointsArray = new int[discretePtsX,discretePtsZ];
		Vector3 tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
		//Vector3 tempVec = new Vector3 (m_nCurrDiscretePtIndxX, 1, m_nCurrDiscretePtIndxZ);
		placeEnemyCentroidAt(tempVec);
		resetCase ();
	}
	int maxPathPoints=-1;
	private void readTimings()
	{
		setGlobalVars1 ();
		timingArray = new float[discretePtsX,discretePtsZ];
		pointsArray = new int[discretePtsX,discretePtsZ];
		StreamReader sr = new StreamReader(fileTimings);
		StreamReader sr1 = new StreamReader(filePoints);
		
		int j = 0;
		int k = 0;
		maxTimeEvaded = float.Parse(sr.ReadLine());
		maxPathPoints = int.Parse(sr1.ReadLine());
		Debug.Log ("maxPathPoints read = " + maxPathPoints);
		List<char> sep = new List<char>();
		sep.Add(',');
		while(!sr.EndOfStream)
		{
			string str = sr.ReadLine();
			k=0;
			foreach(string s in str.Split(sep.ToArray()))
			{
				//Debug.Log(s);
				if(s.Length==0)
					continue;
				timingArray[j,k] = float.Parse(s);
				k++;
			}
			j++;
		}
		sr.Close ();
		
		j = 0;
		k = 0;
		while(!sr1.EndOfStream)
		{
			string str = sr1.ReadLine();
			k=0;
			foreach(string s in str.Split(sep.ToArray()))
			{
				//Debug.Log(s);
				if(s.Length==0)
					continue;
				pointsArray[j,k] = int.Parse(s);
				k++;
			}
			j++;
		}
		sr1.Close ();
	}
	
	private void writeTimings()
	{
		if (!System.IO.File.Exists(fileLastCaseExecutedFor))
		{
			//System.IO.File.WriteAllText(fileLastCaseExecutedFor, "This is text that goes into the text file fileLastCaseExecutedFor");
			System.IO.File.CreateText(fileLastCaseExecutedFor);
		}
		if (!System.IO.File.Exists(filePoints))
		{
			//System.IO.File.WriteAllText(filePoints, "This is text that goes into the text file filePoints");
			System.IO.File.CreateText(filePoints);
		}
		if (!System.IO.File.Exists(fileTimings))
		{
			//System.IO.File.WriteAllText(fileTimings, "This is text that goes into the text file fileTimings");
			System.IO.File.CreateText(fileTimings);
		}
		
		//string tempFile2 = fileLastCaseExecutedFor + "_Temp";
		string tempFile2 = fileLastCaseExecutedFor;
		StreamWriter sw2 = new StreamWriter(tempFile2);
		sw2.WriteLine(m_nCurrDiscretePtIndxX+","+m_nCurrDiscretePtIndxZ+"");
		sw2.Close ();
		//File.Replace(tempFile2,fileLastCaseExecutedFor,fileLastCaseExecutedFor+"Backup");
		//File.Delete (tempFile2);
		
		string tempFile1 = filePoints + "_Temp";
		StreamWriter sw1 = new StreamWriter(tempFile1);
		sw1.WriteLine(pathPoints.Count-1+"");
		
		string tempFile = fileTimings + "_Temp";
		StreamWriter sw = new StreamWriter(tempFile);
		sw.WriteLine(maxTimeEvaded+"");
		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				sw.Write(", " + timingArray[j,k]);
				sw1.Write(", " + pointsArray[j,k]);
			}
			sw.WriteLine(""); 
			sw1.WriteLine(""); 
		}
		sw.Close ();
		sw1.Close ();
		File.Replace(tempFile1,filePoints,filePoints+"Backup");
		File.Delete (tempFile1);
		File.Replace(tempFile,fileTimings,fileTimings+"Backup");
		File.Delete (tempFile);
		//FileUtil.ReplaceFile (tempFile,fileTimings);
		//FileUtil.DeleteFileOrDirectory (tempFile);
	}
	void Update () 
	{
		if(bDisplayAreas)
		{
			return;
		}
		
		if(playerObj.transform.position == pathPoints[pathPoints.Count-1])
		{
			if(m_SetUpCase)
			{
				//foreach(Vector2 ptVect2 in h_mapIndxToPt.Keys)
				{
					//bool bNotCaught = false;
					if(m_Greedy)
					{
						for(int k9=0;k9<m_enemyGreedyList.Count;k9++)
						{
							int itrI = (int)m_enemyGreedyList[k9].startPosIndx.x;
							int itrJ = (int)m_enemyGreedyList[k9].startPosIndx.y;
							if(!m_enemyGreedyList[k9].bCaught)
							{
								//bNotCaught = true;
								pointsArray[itrI,itrJ] = pathPoints.Count-1;
							}
						}
					}
					else if(m_NearMiss)
					{
						for(int k9=0;k9<m_enemyNearMissList.Count;k9++)
						{
							if(!m_enemyNearMissList[k9].bCaught)
							{
								//bNotCaught = true;
								pointsArray[(int)m_enemyNearMissList[k9].startPosIndx.x,(int)m_enemyNearMissList[k9].startPosIndx.y] = pathPoints.Count-1;
							}
						}
					}
					else if(m_ShadowEdgeAssisted)
					{
						for(int k9=0;k9<m_enemyShadowAssistedList.Count;k9++)
						{
							if(!m_enemyShadowAssistedList[k9].bCaught)
							{
								//bNotCaught = true;
								pointsArray[(int)m_enemyShadowAssistedList[k9].startPosIndx.x,(int)m_enemyShadowAssistedList[k9].startPosIndx.y] = pathPoints.Count-1;
							}
						}
					}
					
					/*if(bNotCaught)
				{
					pointsArray[ptVect2.x,ptVect2.y] = pathPoints.Count-1;
					//timingArray[ptVect2.x,ptVect2.y] = Time.time - currRunTimeEnemny;
					
				}*/
				}
				writeTimings();
				Debug.Break();
				return;
				
			}
		}
		if(playerObj.transform.position == pathPoints[nextPlayerPath] && playerObj.transform.position != pathPoints[pathPoints.Count-1])
		{
			foreach(Transform child in allLineParent.transform)
			{
				GameObject.Destroy(child.gameObject);
			}
			//Debug.Log("For visibility polygon for "+nextPlayerPath+" , edges.count = "+((Geometry)hVisiblePolyTable[pathPoints[nextPlayerPath]]).edges.Count);
			foreach(Line l in ((Geometry)hVisiblePolyTable[pathPoints[nextPlayerPath]]).edges)
			{
				GameObject allLineParentChild = new GameObject();
				LineRenderer lineR = allLineParentChild.AddComponent<LineRenderer>();
				lineR.material = matGreen;
				lineR.SetWidth(0.25f,0.25f);
				lineR.SetVertexCount(2);
				lineR.SetPosition(0,l.vertex[0]);
				lineR.SetPosition(1,l.vertex[1]);
				allLineParentChild.transform.parent = allLineParent.transform;
				
				//l.DrawVector(allLineParent);
			}
			
			/*List<Geometry> shadowPolygonsTemp = (List<Geometry>)hTable [pathPoints [nextPlayerPath]];
			foreach(Geometry geoTemp in shadowPolygonsTemp)
			{
				foreach(Line l in geoTemp.edges)
				{
					l.DrawVector(allLineParent);
				}
			}*/
			nextPlayerPath++;
			
			
			findNextEnemyPositions();
			
		}
		Vector3 prevPlayerPos = playerObj.transform.position;
		playerObj.transform.position = Vector3.MoveTowards(playerObj.transform.position, pathPoints[nextPlayerPath], speedPlayer*Time.deltaTime);
		distBtwPlayerMovements = Vector3.Distance (prevPlayerPos, playerObj.transform.position);
		//Debug.Log ("distBtwPlayerMovements = " + distBtwPlayerMovements);
		doPlayerMovements ();
		
		
		/*if (Input.GetMouseButtonDown (0)) {
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
			makeBox();
		}*/
	}
	private void doPlayerMovements()
	{
		for(int j=0;j<m_enemyGreedyList.Count;j++)
		{
			EnemyMovement greedyObj = m_enemyGreedyList[j];
			if(greedyObj.bCaught)
				continue;
			greedyObj.enemyObj.transform.position = Vector3.MoveTowards(greedyObj.enemyObj.transform.position,greedyObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}
		for(int j=0;j<m_enemyNearMissList.Count;j++)
		{
			EnemyMovement nearMissObj = m_enemyNearMissList[j];
			if(nearMissObj.bCaught)
				continue;
			nearMissObj.enemyObj.transform.position = Vector3.MoveTowards(nearMissObj.enemyObj.transform.position,nearMissObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}
		for(int j=0;j<m_enemyShadowAssistedList.Count;j++)
		{
			EnemyMovement shadowAssistedObj = m_enemyShadowAssistedList[j];
			if(shadowAssistedObj.bCaught)
				continue;
			////////////////////////////////
			
			
			Vector3 currPosEnemy = shadowAssistedObj.enemyObj.transform.position;
			
			shadowAssistedObj.vNextPos.Add(findNextPosEnemyShadowAssisted(shadowAssistedObj.enemyObj));
			//if(currPosEnemy == shadowAssistedObj.vNextPos[0])
			shadowAssistedObj.vNextPos.RemoveAt(0);
			if(enemyCaught(shadowAssistedObj.enemyObj.transform.position))
			{
				//GameObject.Destroy(shadowAssistedObj.enemyObj);
				shadowAssistedObj.bCaught=true;
				if(m_SetUpCase)
				{
					pointsArray[(int)shadowAssistedObj.startPosIndx.x,(int)shadowAssistedObj.startPosIndx.y] = nextPlayerPath-1;
					continue;
					//return;
				}
				Renderer rend = shadowAssistedObj.enemyObj.GetComponent<Renderer>();
				rend.material.shader = Shader.Find("Specular");
				rend.material.SetColor("_SpecColor", Color.white);
				shadowAssistedObj.bCaught=true;
				Debug.Log("Shadow Assisted Caught");
			}
			
			/// //////////////////////////////
			shadowAssistedObj.enemyObj.transform.position = Vector3.MoveTowards(shadowAssistedObj.enemyObj.transform.position,shadowAssistedObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}
		for(int j=0;j<m_enemyCentroidList.Count;j++)
		{
			EnemyMovement centroidObj = m_enemyCentroidList[j];
			if(centroidObj.bCaught)
				continue;
			centroidObj.enemyObj.transform.position = Vector3.MoveTowards(centroidObj.enemyObj.transform.position,centroidObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}
	}
	private void findNextEnemyPositions()
	{
		for(int j=0;j<m_enemyNearMissList.Count;j++)
		{
			EnemyMovement nearMissObj = m_enemyNearMissList[j];
			if(nearMissObj.bCaught)
				continue;
			Vector3 currPosEnemy = nearMissObj.enemyObj.transform.position;
			
			
			nearMissObj.vNextPos.Add (findNextPosEnemyNearMiss2(nearMissObj.enemyObj));
			if(currPosEnemy == nearMissObj.vNextPos[0])
				nearMissObj.vNextPos.RemoveAt(0);
			if(enemyCaught(nearMissObj.enemyObj.transform.position))
			{
				//GameObject.Destroy(nearMissObj.enemyObj);
				nearMissObj.bCaught=true;
				if(m_SetUpCase)
				{
					pointsArray[(int)nearMissObj.startPosIndx.x,(int)nearMissObj.startPosIndx.y] = nextPlayerPath-1;
					//timingArray[nearMissObj.startPosIndx.x,nearMissObj.startPosIndx.y] = Time.time - currRunTimeEnemny;
					//Debug.Log("Caught!!! Took "+timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ]+" to catch");
					//resetCase();
					
					//return;
					continue;
				}
				Renderer rend = nearMissObj.enemyObj.GetComponent<Renderer>();
				rend.material.shader = Shader.Find("Specular");
				rend.material.SetColor("_SpecColor", Color.white);
				nearMissObj.bCaught=true;
				Debug.Log("Near Miss Enemy Caught");
			}
			//nearMissObj.enemyObj.transform.position = Vector3.MoveTowards(currPosEnemy,nearMissObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}
		
		for(int j=0;j<m_enemyGreedyList.Count;j++)
		{
			EnemyMovement greedyObj = m_enemyGreedyList[j];
			if(greedyObj.bCaught)
				continue;
			Vector3 currPosEnemy = greedyObj.enemyObj.transform.position;
			
			greedyObj.vNextPos.Add(findNextPosEnemyGreedy(greedyObj.enemyObj));
			if(currPosEnemy == greedyObj.vNextPos[0])
				greedyObj.vNextPos.RemoveAt(0);
			if(enemyCaught(greedyObj.enemyObj.transform.position))
			{
				//GameObject.Destroy(greedyObj.enemyObj);
				greedyObj.bCaught=true;
				if(m_SetUpCase)
				{
					pointsArray[(int)greedyObj.startPosIndx.x,(int)greedyObj.startPosIndx.y] = nextPlayerPath-1;
					continue;
					//return;
				}
				Renderer rend = greedyObj.enemyObj.GetComponent<Renderer>();
				rend.material.shader = Shader.Find("Specular");
				rend.material.SetColor("_SpecColor", Color.white);
				greedyObj.bCaught=true;
				Debug.Log("Greedy Enemy Caught");
			}
			//nearMissObj.enemyObj.transform.position = Vector3.MoveTowards(currPosEnemy,nearMissObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}
		
		/*for(int j=0;j<m_enemyShadowAssistedList.Count;j++)
		{
			EnemyMovement shadowAssistedObj = m_enemyShadowAssistedList[j];
			if(shadowAssistedObj.bCaught)
				continue;
			Vector3 currPosEnemy = shadowAssistedObj.enemyObj.transform.position;
			
			shadowAssistedObj.vNextPos.Add(findNextPosEnemyShadowAssisted(shadowAssistedObj.enemyObj));
			if(currPosEnemy == shadowAssistedObj.vNextPos[0])
				shadowAssistedObj.vNextPos.RemoveAt(0);
			if(enemyCaught(shadowAssistedObj.enemyObj.transform.position))
			{
				if(m_SetUpCase)
				{
					pointsArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ] = nextPlayerPath-1;
					timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ] = Time.time - currRunTimeEnemny;
					Debug.Log("Caught!!! Took "+timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ]+" to catch");
					resetCase();
					return;
				}
				Renderer rend = shadowAssistedObj.enemyObj.GetComponent<Renderer>();
				rend.material.shader = Shader.Find("Specular");
				rend.material.SetColor("_SpecColor", Color.white);
				shadowAssistedObj.bCaught=true;
				Debug.Log("Greedy Enemy Caught");
			}
			//nearMissObj.enemyObj.transform.position = Vector3.MoveTowards(currPosEnemy,nearMissObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}*/
	}
#else
	void Update () 
	{
		if(bMultiplePaths)
		{
			UpdateMultiplePaths();
			return;
		}
		/*mapBG.DrawGeometry(allLineParent,mat);

		foreach(Geometry geo in globalPolygon)
		{
			geo.DrawGeometry(allLineParent,matGreen);
		}*/
		if(bDebugNow)
		{
			mapBG.DrawGeometry(allLineParent,mat);
			foreach(Geometry geo in globalPolygon)
			{
				geo.DrawGeometry(allLineParent,matGreen);
			}
			/*int howManySafe=0;
			foreach(Vector3 key in h_mapPtToIndx.Keys)
			{
				if(pointInShadow(key,PointToDebug))
				{
					howManySafe++;
					//showPosOfPointRectangle(key,Color.green);
					//showPosOfPoint(key,Color.red);
				}
			}
			Debug.Log("howManySafe="+howManySafe);
			Debug.Break();*/
			return;
		}
		if(bShowJustVisibilityPoly)
		{
			//mapBG.DrawGeometry(allLineParent);
			/*foreach(Geometry geo in globalPolygon)
			{
				geo.DrawGeometry(allLineParent);
			}*/
			showPosOfPoint(pathPoints[bShowJustVisibilityPolyForIndex],Color.cyan);
			Debug.Log("For visibility polygon for "+bShowJustVisibilityPolyForIndex+" , edges.count = "+((Geometry)hVisiblePolyTable[pathPoints[bShowJustVisibilityPolyForIndex]]).edges.Count);
			foreach(Line l in ((Geometry)hVisiblePolyTable[pathPoints[bShowJustVisibilityPolyForIndex]]).edges)
			{
				l.DrawVector(allLineParent);
			}
			Debug.Break();
			return;
		}
		if (bAgentBasedAssignment|| bDisplayAreas || m_ExecuteTrueCase || m_ShowTrueCase || m_CalculateTrueCase)
		{
			//Debug.Break();
			return;
		}
		if(m_DisplayOptimizedPaths)
		{
			if (Input.GetMouseButtonDown (0)) 
			{
				foreach(GameObject gb in displayPathList)
				{
					GameObject.Destroy (gb);
				}
				//start_box = Input.mousePosition;
			}
			
			if (Input.GetMouseButtonUp (0)) 
			{
				displayOptimizedPt = Input.mousePosition;
				//start_box = camObj.ScreenToWorldPoint (start_box);
				//start_box.y = 1;
				displayOptimizedPt = camObj.ScreenToWorldPoint (displayOptimizedPt);
				displayOptimizedPt.y = 1.0f;
				Debug.Log("displayOptimizedPt = "+displayOptimizedPt);
				displayOptimalPathNow();
				
			}
			return;
		}
		/*if (bCallComplete) 
		{
			bCallComplete = false;
			bNearBy = AnalyzeNearestPathPoint ();
			bCallComplete = true;
		}
		else 
		{
			return;
		}*/
		if(bTestingMGS2)
		{
			return;
		}
		if(bTestingMGS || bTestingChung || bTestingMyScene1 || bTestingMyCrash)
		{
			/*Vector3 pt4 = new Vector3(-9.9f,1.0f,-6.5f);
			bool ptInShad = pointInShadow(pt4,nextPlayerPath);

			if(ptInShad)
				Debug.Log(pt4+" in shadow for "+nextPlayerPath+"rd player path");
			nextPlayerPath++;*/
			foreach(Transform child in allLineParent.transform)
			{
				GameObject.Destroy(child.gameObject);
			}
			//mapBG.DrawGeometry (allLineParent);
			foreach(Line l in mapBG.edges)
			{
				l.DrawVector(allLineParent);
			}
			for(int i=0;i<globalPolygon.Count;i++)
			{
				foreach(Line l in globalPolygon[i].edges)
				{
					l.DrawVector(allLineParent);
				}
			}
			
			return;
		}
		if(playerObj.transform.position == pathPoints[pathPoints.Count-1])
		{
			if(m_SetUpCase)
			{
				bool bNotCaught = false;
				if(m_Greedy)
				{
					if(!m_enemyGreedyList[0].bCaught)
					{
						bNotCaught = true;
					}
				}
				else if(m_NearMiss)
				{
					if(!m_enemyNearMissList[0].bCaught)
					{
						bNotCaught = true;
					}
				}
				else if(m_ShadowEdgeAssisted)
				{
					if(!m_enemyShadowAssistedList[0].bCaught)
					{
						bNotCaught = true;
					}
				}
				
				if(bNotCaught)
				{
					pointsArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ] = pathPoints.Count-1;
					timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ] = Time.time - currRunTimeEnemny;
					if(maxTimeEvaded<0.0)
					{
						maxTimeEvaded = Time.time - currRunTimeEnemny;
					}
					Debug.Log("Took "+timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ]+" Always hidden");
				}
				resetCase();
				return;
			}
			else
			{
				Debug.Break();
			}
		}
		if(playerObj.transform.position == pathPoints[nextPlayerPath] && playerObj.transform.position != pathPoints[pathPoints.Count-1])
		{
			//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			Debug.Log("Current player path point = "+nextPlayerPath);
			if(bJustTestCrashNow && currSceneName=="Crash.unity")
			{
				foreach(GameObject go in listShows)
				{
					GameObject.DestroyImmediate(go);
				}
				/*foreach(Vector3 vect in h_mapPtToIndx.Keys)
			{
				if(pointInShadow(vect,nextPosIndx9))
					showPosOfPointRectangle(vect,Color.green);
			}*/
				/*if(nextPosIndx9==pathPoints.Count)
				{
					Debug.Break();
				}
				foreach(Line l in ((Geometry)hVisiblePolyTable[pathPoints[nextPosIndx9]]).edges)
				{
					//l.DrawVector(allLineParent);
					GameObject allLineParentTemp = new GameObject();
					LineRenderer line1 = allLineParentTemp.AddComponent<LineRenderer> ();
					line1.material = mat;
					line1.SetWidth (0.4f, 0.4f);
					line1.SetColors(Color.magenta,Color.magenta);
					line1.SetVertexCount (2);
					line1.SetPosition (0, l.vertex[0]);
					line1.SetPosition (1, l.vertex[1]);
					listShows.Add(allLineParentTemp);
				}
				nextPosIndx9++;*/
				//Debug.Break();
				//return;
			}
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			foreach(Transform child in allLineParent.transform)
			{
				GameObject.Destroy(child.gameObject);
			}
			//Debug.Log("For visibility polygon for "+nextPlayerPath+" , edges.count = "+((Geometry)hVisiblePolyTable[pathPoints[nextPlayerPath]]).edges.Count);
			//((Geometry)hVisiblePolyTable[pathPoints[nextPlayerPath]]).DrawGeometry(allLineParent,matGreen);
			
			List<VisibleTriangles> listTriangles = (List<VisibleTriangles>)hVisibleTrianglesTable[pathPoints[nextPlayerPath]];
			foreach(VisibleTriangles vt in listTriangles)
			{
				vt.DrawTriangle();
			}
			
			{
				/*foreach(Line lineBG in mapBG.edges)
					{
						lineBG.DrawVector(allLineParent);
					}*/
				/*Geometry visibleGeoTemp = (Geometry)hVisiblePolyTable[pathPoints [nextPlayerPath]];
				foreach(Vector3 vectSafe in h_mapPtToIndx.Keys)
				{
					//Debug.Log(vectSafe);
					if(visibleGeoTemp.PointInside(vectSafe))
						//if(pointInShadow(vectSafe,nextPlayerPath))
					{
						
						showPosOfPoint(vectSafe,Color.green);
						
					}
				}
				Debug.Break();*/
			}
			/*List<Geometry> shadowPolygonsTemp = (List<Geometry>)hTable [pathPoints [nextPlayerPath]];
			foreach(Geometry geoTemp in shadowPolygonsTemp)
			{
				foreach(Line l in geoTemp.edges)
				{
					l.DrawVector(allLineParent);
				}
			}*/
			if(bSlowShadowsDown)
			{
				setTimerTemp = 200;
			}
			nextPlayerPath++;
			
			
			findNextEnemyPositions();
			
		}
		Vector3 prevPlayerPos = playerObj.transform.position;
		playerObj.transform.position = Vector3.MoveTowards(playerObj.transform.position, pathPoints[nextPlayerPath], speedPlayer*Time.deltaTime);
		distBtwPlayerMovements = Vector3.Distance (prevPlayerPos, playerObj.transform.position);
		//Debug.Log ("distBtwPlayerMovements = " + distBtwPlayerMovements);
		doPlayerMovements ();
		
	}
	
	private void doPlayerMovements()
	{
		for(int j=0;j<m_enemyGreedyList.Count;j++)
		{
			EnemyMovement greedyObj = m_enemyGreedyList[j];
			if(greedyObj.bCaught)
				continue;
			greedyObj.enemyObj.transform.position = Vector3.MoveTowards(greedyObj.enemyObj.transform.position,greedyObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}
		for(int j=0;j<m_enemyNearMissList.Count;j++)
		{
			EnemyMovement nearMissObj = m_enemyNearMissList[j];
			if(nearMissObj.bCaught)
				continue;
			nearMissObj.enemyObj.transform.position = Vector3.MoveTowards(nearMissObj.enemyObj.transform.position,nearMissObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}
		for(int j=0;j<m_enemyShadowAssistedList.Count;j++)
		{
			EnemyMovement shadowAssistedObj = m_enemyShadowAssistedList[j];
			if(shadowAssistedObj.bCaught)
				continue;
			////////////////////////////////
			
			
			Vector3 currPosEnemy = shadowAssistedObj.enemyObj.transform.position;
			
			shadowAssistedObj.vNextPos.Add(findNextPosEnemyShadowAssisted(shadowAssistedObj.enemyObj));
			//if(currPosEnemy == shadowAssistedObj.vNextPos[0])
			shadowAssistedObj.vNextPos.RemoveAt(0);
			if(enemyCaught(shadowAssistedObj.enemyObj.transform.position))
			{
				if(m_SetUpCase)
				{
					pointsArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ] = nextPlayerPath-1;
					timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ] = Time.time - currRunTimeEnemny;
					Debug.Log("Caught!!! Took "+timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ]+" to catch");
					resetCase();
					return;
				}
				Renderer rend = shadowAssistedObj.enemyObj.GetComponent<Renderer>();
				rend.material.shader = Shader.Find("Specular");
				rend.material.SetColor("_SpecColor", Color.white);
				shadowAssistedObj.bCaught=true;
				Debug.Log("Shadow Assisted Caught");
			}
			
			/// //////////////////////////////
			shadowAssistedObj.enemyObj.transform.position = Vector3.MoveTowards(shadowAssistedObj.enemyObj.transform.position,shadowAssistedObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}
		for(int j=0;j<m_enemyCentroidList.Count;j++)
		{
			EnemyMovement centroidObj = m_enemyCentroidList[j];
			if(centroidObj.bCaught)
				continue;
			centroidObj.enemyObj.transform.position = Vector3.MoveTowards(centroidObj.enemyObj.transform.position,centroidObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}
	}
	
	private void findNextEnemyPositions()
	{
		for(int j=0;j<m_enemyNearMissList.Count;j++)
		{
			EnemyMovement nearMissObj = m_enemyNearMissList[j];
			if(nearMissObj.bCaught)
				continue;
			Vector3 currPosEnemy = nearMissObj.enemyObj.transform.position;
			
			
			nearMissObj.vNextPos.Add (findNextPosEnemyNearMiss2(nearMissObj.enemyObj));
			if(currPosEnemy == nearMissObj.vNextPos[0])
				nearMissObj.vNextPos.RemoveAt(0);
			if(enemyCaught(nearMissObj.enemyObj.transform.position))
			{
				if(m_SetUpCase)
				{
					pointsArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ] = nextPlayerPath-1;
					timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ] = Time.time - currRunTimeEnemny;
					Debug.Log("Caught!!! Took "+timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ]+" to catch");
					resetCase();
					return;
				}
				Renderer rend = nearMissObj.enemyObj.GetComponent<Renderer>();
				rend.material.shader = Shader.Find("Specular");
				rend.material.SetColor("_SpecColor", Color.white);
				nearMissObj.bCaught=true;
				Debug.Log("Near Miss Enemy Caught");
			}
			//nearMissObj.enemyObj.transform.position = Vector3.MoveTowards(currPosEnemy,nearMissObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}
		
		for(int j=0;j<m_enemyGreedyList.Count;j++)
		{
			EnemyMovement greedyObj = m_enemyGreedyList[j];
			if(greedyObj.bCaught)
				continue;
			Vector3 currPosEnemy = greedyObj.enemyObj.transform.position;
			
			greedyObj.vNextPos.Add(findNextPosEnemyGreedy(greedyObj.enemyObj));
			if(currPosEnemy == greedyObj.vNextPos[0])
				greedyObj.vNextPos.RemoveAt(0);
			if(enemyCaught(greedyObj.enemyObj.transform.position))
			{
				if(m_SetUpCase)
				{
					pointsArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ] = nextPlayerPath-1;
					timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ] = Time.time - currRunTimeEnemny;
					Debug.Log("Caught!!! Took "+pointsArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ]+" path points to catch");
					resetCase();
					return;
				}
				Renderer rend = greedyObj.enemyObj.GetComponent<Renderer>();
				rend.material.shader = Shader.Find("Specular");
				rend.material.SetColor("_SpecColor", Color.white);
				greedyObj.bCaught=true;
				Debug.Log("Greedy Enemy Caught");
			}
			//nearMissObj.enemyObj.transform.position = Vector3.MoveTowards(currPosEnemy,nearMissObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}
		
		/*for(int j=0;j<m_enemyShadowAssistedList.Count;j++)
		{
			EnemyMovement shadowAssistedObj = m_enemyShadowAssistedList[j];
			if(shadowAssistedObj.bCaught)
				continue;
			Vector3 currPosEnemy = shadowAssistedObj.enemyObj.transform.position;
			
			shadowAssistedObj.vNextPos.Add(findNextPosEnemyShadowAssisted(shadowAssistedObj.enemyObj));
			if(currPosEnemy == shadowAssistedObj.vNextPos[0])
				shadowAssistedObj.vNextPos.RemoveAt(0);
			if(enemyCaught(shadowAssistedObj.enemyObj.transform.position))
			{
				if(m_SetUpCase)
				{
					pointsArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ] = nextPlayerPath-1;
					timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ] = Time.time - currRunTimeEnemny;
					Debug.Log("Caught!!! Took "+timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ]+" to catch");
					resetCase();
					return;
				}
				Renderer rend = shadowAssistedObj.enemyObj.GetComponent<Renderer>();
				rend.material.shader = Shader.Find("Specular");
				rend.material.SetColor("_SpecColor", Color.white);
				shadowAssistedObj.bCaught=true;
				Debug.Log("Greedy Enemy Caught");
			}
			//nearMissObj.enemyObj.transform.position = Vector3.MoveTowards(currPosEnemy,nearMissObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}*/
	}

	private void displayTimingAreas()
	{
		for(int i=0;i<discretePtsX;i++)
		{
			for(int j=0;j<discretePtsZ;j++)
			{
				//float greenNum = timingArray[i,j]/maxTimeEvaded;
				float greenNum = pointsArray[i,j]/(pathPoints.Count-1);
				//greenNum = greenNum*255;
				//float redNum = 1-greenNum;
				//showPosOfPoint((Vector3)h_mapIndxToPt[new Vector2(i,j)],new Color(redNum,greenNum,0));
				showPosOfPoint((Vector3)h_mapIndxToPt[new Vector2(i,j)],new Color(0.0f,greenNum,0.0f));
			}
		}
	}
	
	private void initializeForGreedyCase()
	{
		setGlobalVars1 ();
		timingArray = new float[discretePtsX,discretePtsZ];
		pointsArray = new int[discretePtsX,discretePtsZ];
		Vector3 tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
		
		//m_nCurrDiscretePtIndxX = 27;
		//m_nCurrDiscretePtIndxZ = 15;
		//tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
		placeEnemyGreedyAt(tempVec);
		resetCase ();
	}
	private void initializeForNearMissCase()
	{
		setGlobalVars1 ();
		timingArray = new float[discretePtsX,discretePtsZ];
		pointsArray = new int[discretePtsX,discretePtsZ];
		Vector3 tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
		//Vector3 tempVec = new Vector3 (m_nCurrDiscretePtIndxX, 1, m_nCurrDiscretePtIndxZ);
		placeEnemyNearMissAt(tempVec);
		resetCase ();
	}
	private void initializeShadowEdgeAssisted()
	{
		setGlobalVars1 ();
		timingArray = new float[discretePtsX,discretePtsZ];
		pointsArray = new int[discretePtsX,discretePtsZ];
		Vector3 tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
		//Vector3 tempVec = new Vector3 (m_nCurrDiscretePtIndxX, 1, m_nCurrDiscretePtIndxZ);
		placeEnemyShadowAssistedAt(tempVec);
		resetCase ();
	}
	private void initializeForCentroidCase()
	{
		setGlobalVars1 ();
		timingArray = new float[discretePtsX,discretePtsZ];
		pointsArray = new int[discretePtsX,discretePtsZ];
		Vector3 tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
		//Vector3 tempVec = new Vector3 (m_nCurrDiscretePtIndxX, 1, m_nCurrDiscretePtIndxZ);
		placeEnemyCentroidAt(tempVec);
		resetCase ();
	}
	
	private void readTimings()
	{
		setGlobalVars1 ();
		timingArray = new float[discretePtsX,discretePtsZ];
		pointsArray = new int[discretePtsX,discretePtsZ];
		StreamReader sr = new StreamReader(fileTimings);
		StreamReader sr1 = new StreamReader(filePoints);
		
		int j = 0;
		int k = 0;
		maxTimeEvaded = float.Parse(sr.ReadLine());
		int maxPathPoints = int.Parse(sr1.ReadLine());
		Debug.Log ("maxTimeEvaded read = " + maxTimeEvaded);
		List<char> sep = new List<char>();
		sep.Add(',');
		while(!sr.EndOfStream)
		{
			string str = sr.ReadLine();
			k=0;
			foreach(string s in str.Split(sep.ToArray()))
			{
				//Debug.Log(s);
				if(s.Length==0)
					continue;
				timingArray[j,k] = float.Parse(s);
				k++;
			}
			j++;
		}
		sr.Close ();
		
		j = 0;
		k = 0;
		while(!sr1.EndOfStream)
		{
			string str = sr1.ReadLine();
			k=0;
			foreach(string s in str.Split(sep.ToArray()))
			{
				//Debug.Log(s);
				if(s.Length==0)
					continue;
				pointsArray[j,k] = int.Parse(s);
				k++;
			}
			j++;
		}
		sr1.Close ();
	}
	
	private void writeTimings()
	{
		if (!System.IO.File.Exists(fileLastCaseExecutedFor))
		{
			//System.IO.File.WriteAllText(fileLastCaseExecutedFor, "This is text that goes into the text file fileLastCaseExecutedFor");
			System.IO.File.CreateText(fileLastCaseExecutedFor);
		}
		if (!System.IO.File.Exists(filePoints))
		{
			//System.IO.File.WriteAllText(filePoints, "This is text that goes into the text file filePoints");
			System.IO.File.CreateText(filePoints);
		}
		if (!System.IO.File.Exists(fileTimings))
		{
			//System.IO.File.WriteAllText(fileTimings, "This is text that goes into the text file fileTimings");
			System.IO.File.CreateText(fileTimings);
		}
		
		//string tempFile2 = fileLastCaseExecutedFor + "_Temp";
		string tempFile2 = fileLastCaseExecutedFor;
		StreamWriter sw2 = new StreamWriter(tempFile2);
		sw2.WriteLine(m_nCurrDiscretePtIndxX+","+m_nCurrDiscretePtIndxZ+"");
		sw2.Close ();
		//File.Replace(tempFile2,fileLastCaseExecutedFor,fileLastCaseExecutedFor+"Backup");
		//File.Delete (tempFile2);
		
		string tempFile1 = filePoints + "_Temp";
		StreamWriter sw1 = new StreamWriter(tempFile1);
		sw1.WriteLine(pathPoints.Count-1+"");
		
		string tempFile = fileTimings + "_Temp";
		StreamWriter sw = new StreamWriter(tempFile);
		sw.WriteLine(maxTimeEvaded+"");
		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				sw.Write(", " + timingArray[j,k]);
				sw1.Write(", " + pointsArray[j,k]);
			}
			sw.WriteLine(""); 
			sw1.WriteLine(""); 
		}
		sw.Close ();
		sw1.Close ();
		File.Replace(tempFile1,filePoints,filePoints+"Backup");
		File.Delete (tempFile1);
		File.Replace(tempFile,fileTimings,fileTimings+"Backup");
		File.Delete (tempFile);
		//FileUtil.ReplaceFile (tempFile,fileTimings);
		//FileUtil.DeleteFileOrDirectory (tempFile);
	}

#endif
}



