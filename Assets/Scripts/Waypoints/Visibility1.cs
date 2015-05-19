using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
//using System;
public partial class Visibility1 : MonoBehaviour {
	List<Geometry> obsGeos = new List<Geometry> (); 
	//Contains Map
	Geometry mapBG = new Geometry ();
	bool bShowLogs=false;
	List<Geometry> globalPolygon;
	List<Vector3> pathPoints;
	float mapDiagonalLength = 0;
	GameObject floor ;
	public Camera camObj;
	//List<Vector3> globalTempArrangedPoints = new List<Vector3>();
	// Use this for initialization
	GameObject spTemp ;
	List<GameObject> shadowMeshes;
	private float startTimePlayer;
	private float startTimeEnemy;
	private float journeyLength;
	public float speedPlayer = 1.0F;
	public float speedEnemy = 1.0F;
	GameObject playerObj;
	public GameObject playerPrefab;
	public GameObject enemyPrefab;
	//List<GameObject> m_enemySafest = new List<GameObject>();
	//public int m_enemySafestNumbers = 1;
	//public int m_enemyNearestNumbers = 0;
	public int m_nEnemyStatic = 1;
	public int m_nEnemyNearMiss = 0;
	public int nearMissAlgo = 1;
	public int setUpCase = -1;
	public bool bDisplayAreas = false;
	//List<GameObject> m_enemyNearMissList = new List<GameObject>();
	//List<bool> m_enemyNearMissCaughtList = new List<bool>();
	//List<Vector3> m_enemyNextPosNearMissList = new List<Vector3>();
	List<EnemyMovement> m_enemyNearMissList = new List<EnemyMovement>();
	class EnemyMovement
	{
		public GameObject enemyObj;
		public bool bCaught;
		public List<Vector3> vNextPos;
		public EnemyMovement()
		{
			vNextPos = new List<Vector3>();
			bCaught = false;
		}
	}

	public int m_nEnemyCentroid = 0;
	List<EnemyMovement> m_enemyCentroidList = new List<EnemyMovement>();
	List<Vector3> m_enemyNextPosCentroidList = new List<Vector3>();
	public bool m_ExecuteTrueCase = false;
	public int m_ShowTrueCase = -1;//1 for shortest path;2 for longest
	private string currSceneName;
	//Distance b/w consecutive path points
	private float m_stepDistance = -1.0f;
	void Start () 
	{

		/*float radius_hiddenSphere = (enemyPrefab.collider).radius*((SphereCollider)enemyPrefab.collider).transform.lossyScale.x;
		Debug.Log (radius_hiddenSphere);
		Debug.Break ();*/

		spTemp = (GameObject)GameObject.Find ("StartPoint");
		allLineParent = GameObject.Find ("allLineParent") as GameObject;
		globalPolygon = getObstacleEdges ();
		string[] sceneName = EditorApplication.currentScene.Split(char.Parse("/"));
		currSceneName = sceneName[sceneName.Length -1];	
		if(currSceneName=="scene1.unity")
		{
			pathPoints = CommonScene1.definePath ();
			m_stepDistance = CommonScene1.getStepDistance();
		}
		else if(currSceneName=="testCase1.unity")
		{
			pathPoints = CommonTestCase1.definePath ();
			m_stepDistance = CommonTestCase1.getStepDistance();
		}
		if(m_ShowTrueCase>0)
		{
			displayPredictedPaths2();
			return;
		}
		else if(m_ExecuteTrueCase)
		{
			CalculateVisibilityForPath ();
			//executeTrueCase();
			executeTrueCase2();
			return;
		}
		foreach(Vector3 vect in pathPoints)
		{
			GameObject pathObj;
			pathObj = Instantiate(pathSphere, 
			                    vect, 
			                    pathSphere.transform.rotation) as GameObject;
		}
		if (bDisplayAreas)
		{
			readTimings();
			displayTimingAreas();
			return;
		}
		CalculateVisibilityForPath ();
		shadowMeshes = new List<GameObject>();
		playerObj = Instantiate(playerPrefab) as GameObject;
		playerObj.transform.position = pathPoints [0];
		foreach(Line l in ((Geometry)hVisiblePolyTable[pathPoints[0]]).edges)
		{
			l.DrawVector(allLineParent);
		}
		if(setUpCase==1)
		{
			initializeForNearMissCase();
			return;
		}
		else if(setUpCase==2)
		{
			initializeForCentroidCase();
			return;
		}
		setUpEnemyInitialPos ();
	}
	private void displayPredictedPaths()
	{
		float startTime = Time.realtimeSinceStartup;
		NodeShadow headNode = readNodeStructureFor();
		NodeShadow node = headNode;
		Debug.Log("Read the txt file");
		////////////////////////////////////////////////////////////////////////////////////
		//showAllPathDetected (headNode);
		//return;
		///////////////////////////////////////////////////////////////////////////////////;
		//List<NodeShadow> firstPath = findFirstEnemyPathDetected (headNode);
		List<NodeShadow> firstPath = quickShortestPathDetected (headNode);
		//List<NodeShadow> firstPath = possibleShortestPathDetected (headNode);
		//List<NodeShadow> firstPath = findShortestEnemyPathDetected (headNode);

		showPosOfPoint (firstPath [0].getPos (),Color.cyan);
		standardMaxMovement = speedEnemy*(m_stepDistance/speedPlayer);
		for(int i=1;i<firstPath.Count;i++)
		{
			float dist = Vector3.Distance(firstPath[i].getPos(),firstPath[i-1].getPos());

			//Debug.Log(firstPath[i].getPos()+" ;;;;;; Distance from previous "+firstPath[i-1].getPos()+" is "+dist);
			if(dist>standardMaxMovement)
				Debug.LogError("Dist b/w 2 points should not be greater than standardMaxMovement.");
			//showPosOfPoint (firstPath [i].getPos (),Color.cyan);
			Line l = new Line(firstPath[i].getPos(),firstPath[i-1].getPos());
			l.DrawVector(allLineParent);
		}
		float totalTime = (Time.realtimeSinceStartup - startTime)/60;
		Debug.Log("Finished displayPredictedPaths. Time took to calculate and show shortest path = "+totalTime+" minutes");
	}
	private void showAllPathDetected (NodeShadow headNode)
	{
		List<NodeShadow> stack = new List<NodeShadow> ();
		List<NodeShadow> tempStack = new List<NodeShadow> ();
		stack.Add (headNode);
		float colorCounter = 1.0f;//from green to blue
		float colorStep = 1.0f / pathPoints.Count;
		while(true)
		{
			Color c = new Color(0.0f,colorCounter,1.0f-colorCounter);
			colorCounter-=colorStep;
			foreach(NodeShadow node in stack)
			{
				foreach(NodeShadow child in node.getChildren())
				{
					//Debug.Log(child.getPos());

					if(!tempStack.Contains(child))
						tempStack.Add(child);
					//Line l = new Line(node.getPos(),child.getPos());

					//l.DrawLine(allLineParent,c);

				}
			}
			
			if(tempStack.Count==0)
			{
				break;
			}
			stack.Clear();
			stack = tempStack;
			tempStack = new List<NodeShadow> ();
		}
		showPosOfPoint(headNode.getPos(),Color.cyan);
		foreach(NodeShadow node in stack)
		{
			showPosOfPoint(node.getPos(),Color.green);
		}
	}
	private List<NodeShadow> quickShortestPathDetected (NodeShadow headNode)
	{

		List<NodeShadow> shortestPathIndices = new List<NodeShadow> ();
		List<NodeShadow> stack = new List<NodeShadow> ();
		List<NodeShadow> tempStack = new List<NodeShadow> ();
		headNode.setMinDistFromHead (0.0f,null);
		stack.Add (headNode);
		while(true)
		{
			foreach(NodeShadow node in stack)
			{
				foreach(NodeShadow child in node.getChildren())
				{
					//Debug.Log(child.getPos());
					float dist = Vector3.Distance(node.getPos(),child.getPos());
					child.setMinDistFromHead (dist,node);
					if(!tempStack.Contains(child))
						tempStack.Add(child);
				}
			}
			//Remove duplicates
			/*for(int i=0;i<tempStack.Count;i++)
			{
				for(int j=i+1;j<tempStack.Count;j++)
				{
					if(tempStack[i]==tempStack[j])
					{
						//Debug.Log("Duplicate found.");

						tempStack.RemoveAt(j);
						j--;
					}
				}
			}
			*/
			if(tempStack.Count==0)
			{
				break;
			}
			stack.Clear();
			stack = tempStack;
			tempStack = new List<NodeShadow> ();
			//if(stack.Count==0)
				//break;
		}
		tempStack = stack;
		float minDist = tempStack [0].getMinDistFromHead ();
		NodeShadow endNodePoint = tempStack [0];
		for(int i=1;i<tempStack.Count;i++)
		{
			if(tempStack [i].getMinDistFromHead ()<minDist)
			{
				minDist = tempStack [i].getMinDistFromHead ();
				endNodePoint = tempStack [0];
			}
		}

		while(true)
		{
			shortestPathIndices.Insert (0, endNodePoint);
			endNodePoint = endNodePoint.getParentSelected();
			if(endNodePoint==null)
				break;
		}
		return shortestPathIndices;
	}
	private List<NodeShadow> possibleShortestPathDetected (NodeShadow headNode)
	{
		NodeShadow tempNode = headNode;
		List<NodeShadow> shortestPathIndices = new List<NodeShadow> ();
		while(tempNode.getChildren().Count>0)
		{
			//Debug.Log("Writing Node = "+tempNode.getPos());
			shortestPathIndices.Add(tempNode);
			//tempNode = tempNode.getChildren()[0];
			tempNode = tempNode.getChildren()[tempNode.getChildren().Count-1];
		}
		return shortestPathIndices;
	}

	private List<NodeShadow> findShortestEnemyPathDetected (NodeShadow headNode)
	{
		int lastIndex = pathPoints.Count - 1;
		float minDistPath = -1.0f;
		List<NodeShadow> maxPathIndices = new List<NodeShadow> ();
		List<NodeShadow> shortestPathIndices = new List<NodeShadow> ();
		List<NodeShadow> currPathIndices = new List<NodeShadow> ();
		List<NodeShadow> stack = new List<NodeShadow> ();
		List<List<NodeShadow>> listOfLists = new List<List<NodeShadow>> ();
		stack.Add (headNode);
		int topIndex = -1;
		while(stack.Count>0)
		{
			//pop the top
			topIndex = stack.Count-1;
			NodeShadow nodeTop = stack[topIndex];
			stack.RemoveAt(topIndex);
			Debug.Log("Node Top = "+nodeTop.getPos());
			//before adding remove all greater aand equal levels;
			int currNodeLevel = nodeTop.getSafetyLevel();
			if(currPathIndices.Count>0)
			{
				NodeShadow lastNodeInCurrIndices = currPathIndices[currPathIndices.Count-1];
				while(true)
				{
					if(nodeTop.getParent().Contains(lastNodeInCurrIndices))
					{
						break;
					}
					else
					{
						currPathIndices.RemoveAt (currPathIndices.Count-1);
						lastNodeInCurrIndices = currPathIndices[currPathIndices.Count-1];
					}
				}
			}
			currPathIndices.Add(nodeTop);
			
			if(nodeTop.getChildren().Count==0)
			{
				if(currPathIndices.Count>=maxPathIndices.Count)
				{
					maxPathIndices = new List<NodeShadow>();
					maxPathIndices.AddRange(currPathIndices);
					listOfLists.Insert(0,maxPathIndices);
					//TODO:Remove this if
					/*if(maxPathIndices[maxPathIndices.Count-1].getSafetyLevel()==lastIndex)
					{
						listOfLists.RemoveRange(1,listOfLists.Count-1);
						break;
					}*/

					int maxCountAllowed = maxPathIndices.Count;
					while(true)
					{
						if(listOfLists[listOfLists.Count-1].Count==maxCountAllowed)
						{
							break;
						}
						else
						{
							listOfLists.RemoveAt(listOfLists.Count-1);
						}
					}
				}

			}
			else
			{
				stack.AddRange(nodeTop.getChildren());
			}
			
		}
		//return maxPathIndices;
		Debug.Log("Now Finding shortest path. Lists COunt = "+listOfLists.Count);
		shortestPathIndices.AddRange (listOfLists [0]);
		minDistPath=0.0f;
		for(int j=1;j<shortestPathIndices.Count;j++)
		{
			minDistPath+=Vector3.Distance(shortestPathIndices[j].getPos(),shortestPathIndices[j-1].getPos());

		}
		for(int i=1;i<listOfLists.Count;i++)
		{
			List<NodeShadow> tempNodeList = new List<NodeShadow>();
			tempNodeList.AddRange (listOfLists [i]);
			float minDistTempPath = 0.0f;
			for(int j=1;j<tempNodeList.Count;j++)
			{
				minDistTempPath+=Vector3.Distance(shortestPathIndices[j].getPos(),shortestPathIndices[j-1].getPos());
			}
			if(minDistPath>minDistTempPath)
			{
				shortestPathIndices.Clear();
				shortestPathIndices.AddRange(tempNodeList);
			}
		}
		return shortestPathIndices;
	}
	/*class hashNodeShadowClass
	{
		public Vector3 pt;
		public int safetyLevel;
	}*/
	private List<NodeShadow> findFirstEnemyPathDetected (NodeShadow headNode)
	{
		int lastIndex = pathPoints.Count - 1;
		int maxIndex = -1;
		List<NodeShadow> maxPathIndices = new List<NodeShadow> ();
		List<NodeShadow> currPathIndices = new List<NodeShadow> ();
		List<NodeShadow> stack = new List<NodeShadow> ();
		stack.Add (headNode);
		int topIndex = -1;
		while(stack.Count>0)
		{
			//pop the top
			topIndex = stack.Count-1;
			NodeShadow nodeTop = stack[topIndex];
			stack.RemoveAt(topIndex);

			//before adding remove all greater aand equal levels;
			int currNodeLevel = nodeTop.getSafetyLevel();
			if(currPathIndices.Count>0)
			{
				NodeShadow lastNodeInCurrIndices = currPathIndices[currPathIndices.Count-1];
				while(true)
				{
					if(nodeTop.getParent().Contains(lastNodeInCurrIndices))
					{
						break;
					}
					else
					{
						currPathIndices.RemoveAt (currPathIndices.Count-1);
						lastNodeInCurrIndices = currPathIndices[currPathIndices.Count-1];
					}
				}
			}
			currPathIndices.Add(nodeTop);

			if(nodeTop.getChildren().Count==0)
			{
				if(currPathIndices.Count>maxPathIndices.Count)
				{
					maxPathIndices.Clear();
					maxPathIndices.AddRange(currPathIndices);
					if(maxPathIndices[maxPathIndices.Count-1].getSafetyLevel()==lastIndex)
					{
						break;
					}
				}
				//currPathIndices.RemoveAt(currPathIndices.Count-1);
			}
			else
			{
				stack.AddRange(nodeTop.getChildren());
			}

		}
		return maxPathIndices;
	}
	private NodeShadow readNodeStructureFor()
	{
		setGlobalVars1 ();
		string sourceFileName = EditorUtility.OpenFilePanel("Please select data node file", Application.dataPath,""); 
		StreamReader sr = new StreamReader(sourceFileName);
		List<char> sep = new List<char>();
		sep.Add(',');
		sep.Add(' ');
		sep.Add(';');
		sep.Add('(');
		sep.Add(')');
		sep.Add('|');
		NodeShadow headNode = new NodeShadow ();
		Hashtable mapPtToNode = new Hashtable();

		//////////////////////////////////////////////
		//int jk = 100;
		/// /////////////////////////////////////////////
		string str = sr.ReadLine();

		while(!sr.EndOfStream /*&& jk>0*/)
		{
			//jk--;

			str = sr.ReadLine();

			string[] line1 = str.Split(sep.ToArray());
			//Debug.Log(str);
			List<string> line = new List<string>();
			for(int i=0;i<line1.Length;i++)
			{
				if(line1[i]=="")
					continue;
				line.Add(line1[i]);
				//Debug.Log(line1[i]);
			}
			//Debug.Log("Line list length = "+line.Count);
			//if(mapPtToNode.Keys.Count!=0)
			//	Debug.Log(line[0]+"(1) "+line[1]+"(2) "+line[2]+"(3) "+line[3]+"(4) "+line[4]+" (5) "+line[5]+"(6) "+line[6]+"(7) "+line[7]);


			//hashNodeShadowClass keyObj = new hashNodeShadowClass();
			Vector4 keyObj = new Vector4(float.Parse(line[0]),float.Parse(line[1]),float.Parse(line[2]),float.Parse(line[3]));
			//keyObj.pt = new Vector3(float.Parse(line[0]),float.Parse(line[1]),float.Parse(line[2]));
			//keyObj.safetyLevel = int.Parse(line[3]);
			//hashNodeShadowClass parentKeyObj = new hashNodeShadowClass();
			Vector4 parentKeyObj = new Vector4();
			if(mapPtToNode.Keys.Count!=0)
			{
				
				//parentKeyObj.pt = new Vector3(float.Parse(line[4]),float.Parse(line[5]),float.Parse(line[6]));
				//parentKeyObj.safetyLevel = int.Parse(line[7]);
				parentKeyObj = new Vector4(float.Parse(line[4]),float.Parse(line[5]),float.Parse(line[6]),float.Parse(line[7]));
			}
			if(mapPtToNode.Keys.Count==0)
			{
				headNode = new NodeShadow(new Vector3(keyObj.x,keyObj.y,keyObj.z));
				headNode.setSafetyLevel((int)keyObj.w);
				mapPtToNode.Add(keyObj,headNode);
			}
			else if(!mapPtToNode.ContainsKey(keyObj))
			{
				//NodeShadow node = new NodeShadow(keyObj.pt);
				NodeShadow node = new NodeShadow(new Vector3(keyObj.x,keyObj.y,keyObj.z));
				node.setSafetyLevel((int)keyObj.w);
				//Debug.Log(parentKeyObj.pt+" , "+parentKeyObj.safetyLevel);
				NodeShadow parentNode= (NodeShadow)mapPtToNode[parentKeyObj];
				/*foreach(hashNodeShadowClass h in mapPtToNode.Keys)
				{
					Debug.Log(h.pt+" , "+h.safetyLevel);
					Debug.Log("Parent Node = "+((NodeShadow)mapPtToNode[parentKeyObj]).getPos());
				}*/
				//Debug.Log("Node = "+node.getPos());
				//Debug.Log("Parent node = "+parentNode.getPos());
				parentNode.addChild(node);
				//parentKeyObj should always be present in the Hashtable mapPtToNode;
				/*if(!mapPtToNode.ContainsKey(parentKeyObj))
				{
				}
				else
				{
				}*/
				mapPtToNode.Add(keyObj,node);
			}
			else
			{
				NodeShadow node= (NodeShadow)mapPtToNode[keyObj];
				NodeShadow parentNode= (NodeShadow)mapPtToNode[parentKeyObj];
				//node.setParent(parentNode);
				parentNode.addChild(node);
			}
			
		}
		Debug.Log ("Number of nodes are  = " + mapPtToNode.Keys.Count);
		sr.Close ();

		return headNode;
	}
	/*private void writeNodeStructureNow(NodeShadow nodeCurr,string saveDataDirName)
	{

		
		
		string sourceFileName = saveDataDirName + "\\"+headNode.getPos().ToString()+".txt";
		//string tempFile = sourceFileName+"_Temp";
		StreamWriter sw = new StreamWriter(sourceFileName,true);//append to file
		//NodeSignature|ParentSignature
		sw.WriteLine("(Vector3;level)|(Vector3;level)"+"");
		
		sw.Write("("+headNode.getPos()+";"+headNode.getSafetyLevel()+")|("+null+";"+null+")");
		sw.WriteLine("");
		List<NodeShadow> nodeSafeLevelNow = new List<NodeShadow> ();
		List<NodeShadow> nodeSafeLevelNext = headNode.getChildren ();
		NodeShadow parentNode = headNode;
		while(nodeSafeLevelNext.Count>0)//while(levelOfAccess<pathPoints.Count)
		{
			//levelOfAccess++;
			nodeSafeLevelNow = nodeSafeLevelNext;
			nodeSafeLevelNext = new List<NodeShadow>();
			
			foreach(NodeShadow nodeNow in nodeSafeLevelNow)
			{
				foreach(NodeShadow nodeNowParent in nodeNow.getParent())
				{
					sw.Write("("+nodeNow.getPos()+";"+nodeNow.getSafetyLevel()+")|("+nodeNowParent.getPos()+";"+nodeNowParent.getSafetyLevel()+")");
					sw.WriteLine("");
				}
				List<NodeShadow> childrenTemp = nodeNow.getChildren();
				foreach(NodeShadow nxtLevelNode in childrenTemp)
				{
					if(!nodeSafeLevelNext.Contains(nxtLevelNode))
					{
						nodeSafeLevelNext.Add(nxtLevelNode);
					}
				}
				
			}
		}
		
		
		sw.Close ();
	}
	*/
	private void writeNodeStructure(NodeShadow headNode,string saveDataDirName)
	{
		//NodeShadow tempNode = headNode;
		/*while(tempNode.getChildren().Count>0)
		{
			Debug.Log("Writing Node = "+tempNode.getPos());
			tempNode = tempNode.getChildren()[0];
		}
		*/


		string sourceFileName = saveDataDirName + "\\"+headNode.getPos().ToString()+".txt";
		//string tempFile = sourceFileName+"_Temp";
		StreamWriter sw = new StreamWriter(sourceFileName);
		//NodeSignature|ParentSignature
		sw.WriteLine("(Vector3;level)|(Vector3;level)"+"");

		sw.Write("("+headNode.getPos()+";"+headNode.getSafetyLevel()+")|("+null+";"+null+")");
		sw.WriteLine("");
		List<NodeShadow> nodeSafeLevelNow = new List<NodeShadow> ();
		List<NodeShadow> nodeSafeLevelNext = headNode.getChildren ();
		NodeShadow parentNode = headNode;
		while(nodeSafeLevelNext.Count>0)//while(levelOfAccess<pathPoints.Count)
		{
			//levelOfAccess++;
			nodeSafeLevelNow = nodeSafeLevelNext;
			nodeSafeLevelNext = new List<NodeShadow>();
			
			foreach(NodeShadow nodeNow in nodeSafeLevelNow)
			{
				foreach(NodeShadow nodeNowParent in nodeNow.getParent())
				{
					sw.Write("("+nodeNow.getPos()+";"+nodeNow.getSafetyLevel()+")|("+nodeNowParent.getPos()+";"+nodeNowParent.getSafetyLevel()+")");
					sw.WriteLine("");
				}
				List<NodeShadow> childrenTemp = nodeNow.getChildren();
				foreach(NodeShadow nxtLevelNode in childrenTemp)
				{
					if(!nodeSafeLevelNext.Contains(nxtLevelNode))
					{
						nodeSafeLevelNext.Add(nxtLevelNode);
					}
				}
				
			}
		}


		sw.Close ();
	}
	private void executeTrueCase()
	{
		setGlobalVars1();
		standardMaxMovement = speedEnemy*(m_stepDistance/speedPlayer);
		Debug.Log ("Initialize standardMaxMovement = " + standardMaxMovement);

		string dirName = createSaveDataDir(Application.dataPath);
		int j1=0;
		for(float j=m_minX;j<m_maxX && j1<discretePtsX;j+=m_step)
		{
			int k1=0;
			for(float k=m_minZ;k<m_maxZ && k1<discretePtsZ;k+=m_step)
			{
				//Debug.Log(j1+" , "+k1);
				Vector3 pt = new Vector3(j,1,k);
				if(!pointInShadow(pt,0))
				{
					k1++;
					continue;
				}
				Vector2 keyTemp = new Vector2(j1,k1);

				Debug.Log(j1+" , "+k1);
				//if(j1==95 && k1==35)//TODO:Remove
				if(j1==44 && k1==7)//TODO:Remove
				{
					showPosOfPoint(pt,Color.cyan);//TODO:Remove
					float startTime = Time.realtimeSinceStartup;
					executeTrueCaseFor(keyTemp,dirName);
					float totalTime = (Time.realtimeSinceStartup - startTime)/60;
					Debug.Log("executeTrueCase Finished. Time taken is = "+totalTime+" mins");//TODO:Remove
					return;//TODO:Remove
				}
				k1++;
			}
			j1++;
		}
	}
	float standardMaxMovement = -1.0f;
	private void executeTrueCaseFor(Vector2 indexOfPt,string saveDataDirName)
	{
		//return;
		Vector3 pt = (Vector3)h_mapIndxToPt[indexOfPt];
		NodeShadow headNode = new NodeShadow (pt);
		headNode.setSafetyLevel (0);
		Hashtable h_mapPtToNode = new Hashtable();
		int levelOfAccess = 1;
		List<NodeShadow> nodeSafeLevelNow = new List<NodeShadow> ();
		nodeSafeLevelNow.Add (headNode);
		List<NodeShadow> nodeSafeLevelNext = reachableChildren (headNode,indexOfPt,levelOfAccess,h_mapPtToNode);
		h_mapPtToNode.Clear ();
		while(levelOfAccess<pathPoints.Count)//TODO:think other exit cases
		{
			levelOfAccess++;
			nodeSafeLevelNow = nodeSafeLevelNext;
			nodeSafeLevelNext = new List<NodeShadow>();

			foreach(NodeShadow child in nodeSafeLevelNow)
			{
				Vector2 indexOfPtTemp = (Vector2)h_mapPtToIndx[child.getPos()];
				List<NodeShadow> childrenTemp = reachableChildren (child,indexOfPtTemp,levelOfAccess,h_mapPtToNode);
				foreach(NodeShadow nxtLevelNode in childrenTemp)
				{
					if(!nodeSafeLevelNext.Contains(nxtLevelNode))
					{
						nodeSafeLevelNext.Add(nxtLevelNode);
					}
				}

			}
			h_mapPtToNode.Clear();
		}
		writeNodeStructure (headNode,saveDataDirName);
	}
	private bool addPossibleChild(Vector2 tempVect2,NodeShadow node,int pathPointIndx,Hashtable h_mapPtToNode)
	{
		//Debug.Log ("Possible Child 1 ="+(Vector3)h_mapIndxToPt [tempVect2]);
		if(h_mapIndxToPt.ContainsKey(tempVect2))
		{
			Vector3 tempVect3 = (Vector3)h_mapIndxToPt[tempVect2];
			//Debug.Log("standardMaxMovement = "+standardMaxMovement);
			//Debug.Log("Possible Child 2 = "+tempVect3);
			if(pointInShadow(tempVect3,pathPointIndx) && Vector3.Distance(node.getPos(),tempVect3)<=standardMaxMovement)
			{
				NodeShadow nodeChild;
				if(h_mapPtToNode.ContainsKey(tempVect3))
				{
					nodeChild = (NodeShadow)h_mapPtToNode[tempVect3];
				}
				else
				{
					nodeChild = new NodeShadow(tempVect3);
					nodeChild.setSafetyLevel(pathPointIndx);
					h_mapPtToNode.Add(tempVect3,nodeChild);
				}
				node.addChild(nodeChild);
				Debug.Log(tempVect3+" added as child of "+node.getPos()+" Dist b/w them is "+Vector3.Distance(node.getPos(),tempVect3));
				return true;
			}
			else
			{
				Debug.Log(tempVect3+" cannot be added as child of "+node.getPos()+" Dist b/w them is "+Vector3.Distance(node.getPos(),tempVect3));
			}
		}
		return false;
	}
	private List<NodeShadow> reachableChildren(NodeShadow node,Vector2 indexOfPt,int pathPointIndx,Hashtable h_mapPtToNode)
	{
		int rowJ = (int)indexOfPt.x;
		int colK = (int)indexOfPt.y;
		addPossibleChild(indexOfPt,node,pathPointIndx,h_mapPtToNode);
		while(true)
		{
			bool bStillReachable=false;
			bool bRunAgain=false;
			rowJ--;
			colK--;
			int rowLen = ((int)indexOfPt.x - rowJ)*2 +1;
			//////////////////////////////////////////////////////////////////////&&&&&&&&&&&&&&&&&
			Vector2 testPt2D = new Vector2(rowJ+rowLen/2,colK);
			Vector3 testPt3D = new Vector3(0,0,0);
			bool bPtAssigned = false;
			if(h_mapIndxToPt.ContainsKey(testPt2D))
			{
				testPt3D = (Vector3)h_mapIndxToPt[testPt2D];
				bPtAssigned = true;
			}
			else
			{
				testPt2D = new Vector2(rowJ+rowLen/2,colK+rowLen-1);
				if(h_mapIndxToPt.ContainsKey(testPt2D))
				{
					testPt3D = (Vector3)h_mapIndxToPt[testPt2D];
					bPtAssigned = true;
				}
				else
				{
					testPt2D = new Vector2(rowJ+rowLen-1,colK+rowLen/2);
					if(h_mapIndxToPt.ContainsKey(testPt2D))
					{
						testPt3D = (Vector3)h_mapIndxToPt[testPt2D];
						bPtAssigned = true;
					}
					else
					{
						testPt2D = new Vector2(rowJ,colK+rowLen/2);
						if(h_mapIndxToPt.ContainsKey(testPt2D))
						{
							testPt3D = (Vector3)h_mapIndxToPt[testPt2D];
							bPtAssigned = true;
						}
					}
				}
			}
			if(!bPtAssigned)
			{
				Debug.LogError("All Possible points exhausted. No outcome. Breaking from loop");
				break;
			}
			////////////////////////////////////////////////////////////////////////&&&&&&&&&&&&&&&&;
			float testDist = Vector3.Distance(node.getPos(),testPt3D);
			//Debug.Log("testDist = "+testDist);
			if(testDist > standardMaxMovement)// || rowJ<0 || colK<0 || rowJ+rowLen>discretePtsX || colK+rowLen>discretePtsZ)
				break;
			//Debug.Log("rowJ = "+rowJ);
			//Debug.Log("colK = "+colK);
			//Debug.Log("rowLen = "+rowLen);
			for(int i1=rowJ;i1<rowJ+rowLen;i1++)
			{
				Vector2 tempVect2 = new Vector2(i1,colK);
				bStillReachable = addPossibleChild(tempVect2,node,pathPointIndx,h_mapPtToNode);
				if(bStillReachable)
					bRunAgain=true;
				tempVect2 = new Vector2(i1,colK+rowLen-1);
				bStillReachable = addPossibleChild(tempVect2,node,pathPointIndx,h_mapPtToNode);
				if(bStillReachable)
					bRunAgain=true;
			}
			for(int i2=colK+1;i2<colK+rowLen-1;i2++)
			{
				Vector2 tempVect2 = new Vector2(rowJ,i2);
				bStillReachable = addPossibleChild(tempVect2,node,pathPointIndx,h_mapPtToNode);
				if(bStillReachable)
					bRunAgain=true;
				tempVect2 = new Vector2(rowJ+rowLen-1,i2);
				bStillReachable = addPossibleChild(tempVect2,node,pathPointIndx,h_mapPtToNode);
				if(bStillReachable)
					bRunAgain=true;
			}


		}
		/*Debug.Log(node.getPos()+" has following children");
		string childrEn="";
		foreach(NodeShadow ch in node.getChildren())
		{
			childrEn+=ch.getPos()+" , ";
		}
		Debug.Log(childrEn);
		*/
		return node.getChildren ();
	}
	private string createSaveDataDir (string dataPath)
	{
		string dirName = Path.Combine(dataPath ,System.DateTime.Now.Day
		                              +"th_Of_"+System.DateTime.Now.Month
		                              +"_Year_"+System.DateTime.Now.Year
		                              +"_Time_"+System.DateTime.Now.Hour
		                              +"_"+System.DateTime.Now.Minute
		                              +"_"+System.DateTime.Now.Second
		                              +"_PredictedPathData");
		System.IO.Directory.CreateDirectory(dirName);
		return dirName;
	}
	private void displayTimingAreas()
	{
		for(int i=0;i<discretePtsX;i++)
		{
			for(int j=0;j<discretePtsZ;j++)
			{
				float greenNum = timingArray[i,j]/maxTimeEvaded;
				float redNum = 1-greenNum;
				showPosOfPoint((Vector3)h_mapIndxToPt[new Vector2(i,j)],new Color(redNum,greenNum,0));
			}
		}
	}
	private void initializeForNearMissCase()
	{
		setGlobalVars1 ();
		timingArray = new float[discretePtsX,discretePtsZ];
		Vector3 tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
		//Vector3 tempVec = new Vector3 (m_nCurrDiscretePtIndxX, 1, m_nCurrDiscretePtIndxZ);
		placeEnemyNearMissAt(tempVec);
		resetCase ();
	}

	private void initializeForCentroidCase()
	{
		setGlobalVars1 ();
		timingArray = new float[discretePtsX,discretePtsZ];
		Vector3 tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
		//Vector3 tempVec = new Vector3 (m_nCurrDiscretePtIndxX, 1, m_nCurrDiscretePtIndxZ);
		placeEnemyCentroidAt(tempVec);
		resetCase ();
	}
	int m_nCurrDiscretePtIndxX=0;
	int m_nCurrDiscretePtIndxZ=0;
	float[,] timingArray = null;
	float maxTimeEvaded = -1.0f;
	float currRunTimeEnemny = 0f;
	string fileTimings = "C:\\Users\\Dhaliwal\\Desktop\\timingScene1.txt";
	private void readTimings()
	{
		setGlobalVars1 ();
		timingArray = new float[discretePtsX,discretePtsZ];
		StreamReader sr = new StreamReader(fileTimings);
		int j = 0;
		int k = 0;
		maxTimeEvaded = float.Parse(sr.ReadLine());
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
	}
	private void writeTimings()
	{
		string tempFile = fileTimings + "_Temp";
		StreamWriter sw = new StreamWriter(tempFile);
		sw.WriteLine(maxTimeEvaded+"");
		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				sw.Write(", " + timingArray[j,k]);
			}
			sw.WriteLine(""); 
		}
		sw.Close ();
		File.Replace(tempFile,fileTimings,fileTimings+"Backup");
		File.Delete (tempFile);
		//FileUtil.ReplaceFile (tempFile,fileTimings);
		//FileUtil.DeleteFileOrDirectory (tempFile);
	}
	void resetCase()
	{
		if (m_nCurrDiscretePtIndxZ >= discretePtsZ)
		{
			Debug.Break();
			writeTimings();
			return;
		}
		int skipPts = 1;
		while(true)
		{
			Vector3 tempVec = (Vector3)h_mapIndxToPt[new Vector2(m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ)];
			//Vector3 tempVec = new Vector3 (m_nCurrDiscretePtIndxX, 1, m_nCurrDiscretePtIndxZ);
			if(pointInShadow(tempVec,0) && timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ]==0)
			{
				//Assumption:Only one enemy at a time
				playerObj.transform.position = pathPoints [0];
				nextPlayerPath = 1;
				if(setUpCase==1)
				{
					m_enemyNearMissList[0].enemyObj.transform.position = tempVec;
					if(nearMissAlgo==1)
						m_enemyNearMissList[0].vNextPos.Add (findNextPosEnemyNearMiss1(m_enemyNearMissList[0].enemyObj));
					else if(nearMissAlgo==2)
						m_enemyNearMissList[0].vNextPos.Add(findNextPosEnemyNearMiss2(m_enemyNearMissList[0].enemyObj));
					//currRunTimeEnemny = Time.time;
					m_enemyNearMissList[0].bCaught = false;
					break;
				}
				else if(setUpCase==2)
				{

					break;
				}

			}
			/*else
			{
				Debug.Log(tempVec+" not in shadow. At ("+m_nCurrDiscretePtIndxX+" , "+m_nCurrDiscretePtIndxZ+")");
			}*/
			m_nCurrDiscretePtIndxX+=skipPts;
			if (m_nCurrDiscretePtIndxX >= discretePtsX)
			{
				m_nCurrDiscretePtIndxX=0;
				m_nCurrDiscretePtIndxZ+=skipPts;
			}

			if (m_nCurrDiscretePtIndxZ >= discretePtsZ)
			{
				Debug.Break();
				writeTimings();
				return;
			}
		}
		Debug.Log("Selected At ("+m_nCurrDiscretePtIndxX+" , "+m_nCurrDiscretePtIndxZ+")");
		writeTimings ();
		currRunTimeEnemny = Time.time;
	}
	List<Vector3> enemyPath = null;
	//int nextEnemyPath = 1;
	int nextPlayerPath = 1;
	void Update () 
	{
		if (bDisplayAreas || m_ExecuteTrueCase || m_ShowTrueCase>0)
		{
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
		if(playerObj.transform.position == pathPoints[pathPoints.Count-1])
		{
			if(setUpCase==1)
			{
				if(!m_enemyNearMissList[0].bCaught)
				{

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
			foreach(Transform child in allLineParent.transform)
			{
				GameObject.Destroy(child.gameObject);
			}
			foreach(Line l in ((Geometry)hVisiblePolyTable[pathPoints[nextPlayerPath]]).edges)
			{
				l.DrawVector(allLineParent);
			}
			nextPlayerPath++;
			//startTimePlayer=Time.time;
			for(int j=0;j<m_enemyCentroidList.Count;j++)
			{
				EnemyMovement centroidObj = m_enemyCentroidList[j];
				if(centroidObj.bCaught)
					continue;
				Vector3 currPosEnemy = centroidObj.enemyObj.transform.position;

				centroidObj.vNextPos.Add(findNextPosEnemyCentroid(m_enemyCentroidList[j].enemyObj));
				if(currPosEnemy == centroidObj.vNextPos[0])
					centroidObj.vNextPos.RemoveAt(0);
				if(enemyCaught(centroidObj.enemyObj.transform.position))
				{
					if(setUpCase==2)
					{
						timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ] = Time.time - currRunTimeEnemny;
						Debug.Log("Caught!!! Took "+timingArray[m_nCurrDiscretePtIndxX,m_nCurrDiscretePtIndxZ]+" to catch");
						resetCase();
						return;
					}
					Renderer rend = centroidObj.enemyObj.GetComponent<Renderer>();
					rend.material.shader = Shader.Find("Specular");
					rend.material.SetColor("_SpecColor", Color.white);
					centroidObj.bCaught=true;
					Debug.Log("Centroid Enemy Caught");
					continue;
				}
				//m_enemyCentroidList[j].enemyObj.transform.position = Vector3.MoveTowards(currPosEnemy,centroidObj.vNextPos[0], speedEnemy*Time.deltaTime);
			}
			for(int j=0;j<m_enemyNearMissList.Count;j++)
			{
				EnemyMovement nearMissObj = m_enemyNearMissList[j];
				if(nearMissObj.bCaught)
					continue;
				Vector3 currPosEnemy = nearMissObj.enemyObj.transform.position;

				if(nearMissAlgo==1)
					nearMissObj.vNextPos.Add(findNextPosEnemyNearMiss1(nearMissObj.enemyObj));
				else if(nearMissAlgo==2)
					nearMissObj.vNextPos.Add (findNextPosEnemyNearMiss2(nearMissObj.enemyObj));
				if(currPosEnemy == nearMissObj.vNextPos[0])
					nearMissObj.vNextPos.RemoveAt(0);
				if(enemyCaught(nearMissObj.enemyObj.transform.position))
				{
					if(setUpCase==1)
					{
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
			/*
			List<Vector3> centrePts = (List<Vector3>)hCentroidShadows[pathPoints[nextPlayerPath]];
			foreach(Vector3 vect in centrePts)
			{
				showPosOfPoint(vect);
			}
			*/
		}
		playerObj.transform.position = Vector3.MoveTowards(playerObj.transform.position, pathPoints[nextPlayerPath], speedPlayer*Time.deltaTime);
		for(int j=0;j<m_enemyNearMissList.Count;j++)
		{
			EnemyMovement nearMissObj = m_enemyNearMissList[j];
			if(nearMissObj.bCaught)
				continue;
			nearMissObj.enemyObj.transform.position = Vector3.MoveTowards(nearMissObj.enemyObj.transform.position,nearMissObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}
		for(int j=0;j<m_enemyCentroidList.Count;j++)
		{
			EnemyMovement centroidObj = m_enemyCentroidList[j];
			if(centroidObj.bCaught)
				continue;
			centroidObj.enemyObj.transform.position = Vector3.MoveTowards(centroidObj.enemyObj.transform.position,centroidObj.vNextPos[0], speedEnemy*Time.deltaTime);
		}

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
	private bool enemyCaught(Vector3 currPos)
	{
		if (pointInShadow (currPos, nextPlayerPath - 1))
			return false;
		return true;
	}
	//Nearest safepoint. Least movement.
	private Vector3 findNextPosEnemyNearMiss1(GameObject enemyObj)
	{
		if(!pointInShadow(enemyObj.transform.position,nextPlayerPath))
		{
			float timePlayer = Vector3.Distance(pathPoints[nextPlayerPath],pathPoints[nextPlayerPath-1])/speedPlayer;
			float radiusMovement = speedEnemy*timePlayer;
			float radiiVar = radiusMovement/12;
			float radiiStep = radiiVar;
			while(radiiVar<radiusMovement)
			{
				Vector3 vecSel = enemyObj.transform.position;
				//vecSel.x+=radiiVar;
				int angleVar=0;
				while(!pointInShadow(vecSel,nextPlayerPath))
				{

					vecSel.x = enemyObj.transform.position.x + radiiVar*Mathf.Cos(angleVar* Mathf.Deg2Rad);
					vecSel.z = enemyObj.transform.position.z + radiiVar*Mathf.Sin(angleVar* Mathf.Deg2Rad);
					angleVar++;
					if(angleVar==360)
						break;

				}
				if(pointInShadow(vecSel,nextPlayerPath))
				{
					return vecSel;
				}
				radiiVar+=radiiStep;
			}
		}
		return enemyObj.transform.position;

	}
	//Reaching safest possible point:based on largest angle made in shadow region from start point.select the middle angle
	private Vector3 findNextPosEnemyNearMiss2(GameObject enemyObj)
	{
		if(!pointInShadow(enemyObj.transform.position,nextPlayerPath))
		{
			float timePlayer = Vector3.Distance(pathPoints[nextPlayerPath],pathPoints[nextPlayerPath-1])/speedPlayer;
			float radiusMovement = speedEnemy*timePlayer;
			int skipAngle=1;
			Vector3 vecSel = enemyObj.transform.position;
			//vecSel.x+=radiusMovement;
			int angleVar=0;
			List<int> listAngles = new List<int>();
			bool insideShadow=false;
			while(true)
			{
				vecSel.x = enemyObj.transform.position.x + radiusMovement*Mathf.Cos(angleVar* Mathf.Deg2Rad);
				vecSel.z = enemyObj.transform.position.z + radiusMovement*Mathf.Sin(angleVar* Mathf.Deg2Rad);
				if(pointInShadow(vecSel,nextPlayerPath))
				{
					if(!insideShadow)
						listAngles.Add(angleVar);
					insideShadow=true;
				}
				else 
				{
					if(insideShadow)
						listAngles.Add(angleVar-skipAngle);
					insideShadow=false;
				}
				angleVar+=skipAngle;
				if(angleVar>=360)
				{
					if(insideShadow)
						listAngles.Add(angleVar-skipAngle);
					break;
				}
				
			}
			int allAnglesCount = listAngles.Count;

			//////////////////////////////////////
			/*string strAngles="( ";
			foreach(int angleTemp in listAngles)
			{
				strAngles+=" , "+angleTemp;
				
			}
			Debug.Log(strAngles+" )");
			*/
			//////////////////////////////////////;

			if(allAnglesCount==0)
				return enemyObj.transform.position;
			int lastProbableInsideAngle = -1;
			if(360%skipAngle!=0)
				lastProbableInsideAngle = 360-(360%skipAngle);
			else
				lastProbableInsideAngle = 360-skipAngle;
			if(listAngles[0]==0 && listAngles[allAnglesCount-1]==lastProbableInsideAngle)
			{
				listAngles[allAnglesCount-1]=listAngles[1]+360;
				listAngles.RemoveAt(0);
				listAngles.RemoveAt(0);
				allAnglesCount = listAngles.Count;
			}
			int maxDiff=-1;
			int indxMaxDiff=-1;

			for(int j=0;j<=allAnglesCount-2;j+=2)
			{
				if(listAngles[j+1]-listAngles[j]>maxDiff)
				{
					indxMaxDiff=j;
					maxDiff = listAngles[j+1]-listAngles[j];
				}
			}
			//////////////////////////////////////
			/*strAngles="( ";
			foreach(int angleTemp in listAngles)
			{
				strAngles+=" , "+angleTemp;
				
			}
			Debug.Log(strAngles+" ))) indxMaxDiff = "+indxMaxDiff);
			*/
			//////////////////////////////////////;
			int angleSel = -1;
			if(indxMaxDiff==-1)
				angleSel = listAngles[0];
			else
			{
				angleSel = (listAngles[indxMaxDiff+1]+listAngles[indxMaxDiff])/2;
				//angleSel = getAngleWithMaxPotential(listAngles[indxMaxDiff],listAngles[indxMaxDiff+1],enemyObj.transform.position,radiusMovement);
			}
			vecSel.x = enemyObj.transform.position.x + radiusMovement*Mathf.Cos(angleSel* Mathf.Deg2Rad);
			vecSel.z = enemyObj.transform.position.z + radiusMovement*Mathf.Sin(angleSel* Mathf.Deg2Rad);
			return vecSel;
		}
		return enemyObj.transform.position;
	}
	private int getAngleWithMaxPotential(int angle1,int angle2,Vector3 currPos,float radiusMovement)
	{
		int skipAngle = 1;
		int tempAngle = angle1;
		int maxCounterShadowTemp = -1;
		int bestAngle = -1;
		while(tempAngle<=angle2)
		{
			Vector3 newCentre = new Vector3();
			newCentre.x = currPos.x + radiusMovement*Mathf.Cos(tempAngle* Mathf.Deg2Rad);
			newCentre.y = 1;
			newCentre.z = currPos.z + radiusMovement*Mathf.Cos(tempAngle* Mathf.Deg2Rad);
			Vector3 tempPoint = new Vector3();
			tempPoint.y = 1;
			int counterShadowTemp = 0;
			for(int theta=0;theta<360;theta+=skipAngle)
			{
				tempPoint.x = newCentre.x + radiusMovement*Mathf.Cos(theta* Mathf.Deg2Rad);
				tempPoint.z = newCentre.z + radiusMovement*Mathf.Cos(theta* Mathf.Deg2Rad);

				if(pointInShadow(tempPoint,nextPlayerPath) || CheckIfInsidePolygon(tempPoint))
				{
					counterShadowTemp++;
				}
			}
			if(maxCounterShadowTemp<counterShadowTemp)
			{
				maxCounterShadowTemp = counterShadowTemp;
				bestAngle = tempAngle;
			}
			tempAngle+=skipAngle;
		}
		return bestAngle;
	}
	/*private Vector3 findNextPosEnemyNearMiss3(GameObject enemyObj)
	{
		if(!pointInShadow(enemyObj.transform.position,nextPlayerPath))
		{
			float timePlayer = Vector3.Distance(pathPoints[nextPlayerPath],pathPoints[nextPlayerPath-1])/speedPlayer;
			float radiusMovement = speedEnemy*timePlayer;
			Vector3 vecSel = enemyObj.transform.position;
			//vecSel.x+=radiusMovement;
			int angleVar=0;
			bool insideShadow=false;
			//check all lines and find nearest visibility edge
			Geometry visiblePolyTemp = (Geometry)hVisiblePolyTable [pathPoints [nextPlayerPath]];
			float minDist = 0f;
			float distTemp = 0f;
			Line firstEdge = visiblePolyTemp.edges[0];
			float y2 = firstEdge.vertex[1].z;
			float y1 = firstEdge.vertex[0].z;
			float x2 = firstEdge.vertex[1].x;
			float x1 = firstEdge.vertex[0].x;
			minDist = Mathf.Abs((y2-y1)*vecSel.x - (x2-x1)*vecSel.z + x2*y1 - y2*x1);
			minDist = minDist/Mathf.Sqrt(Mathf.Pow((y2-y1),2) + Mathf.Pow((x2-x1),2));
			int selEdgeIndx = 0;
			for(int i=1;i<visiblePolyTemp.edges.Count;i++)
			{
				firstEdge = visiblePolyTemp.edges[i];
				y2 = firstEdge.vertex[1].z;
				y1 = firstEdge.vertex[0].z;
				x2 = firstEdge.vertex[1].x;
				x1 = firstEdge.vertex[0].x;
				distTemp = Mathf.Abs((y2-y1)*vecSel.x - (x2-x1)*vecSel.z + x2*y1 - y2*x1);
				distTemp = distTemp/Mathf.Sqrt(Mathf.Pow((y2-y1),2) + Mathf.Pow((x2-x1),2));
				if(distTemp<minDist)
				{
					selEdgeIndx = i;
					minDist = distTemp;
				}
			}
			vecSel.x = enemyObj.transform.position.x + radiusMovement*Mathf.Cos(angleSel* Mathf.Deg2Rad);
			vecSel.z = enemyObj.transform.position.z + radiusMovement*Mathf.Sin(angleSel* Mathf.Deg2Rad);
			return vecSel;
		}
		return enemyObj.transform.position;

	}
	*/
	private void setUpEnemyInitialPos()
	{
		if(m_nEnemyStatic>0)
		{
			createDiscreteMap ();
			sbyte[,] shadowArray = null;
			shadowArray = findSafestSpots();
			placeEnemyStatic(shadowArray);
		}
		if(m_nEnemyNearMiss>0)
		{
			setGlobalVars1();
			int numNearMiss = m_nEnemyNearMiss;
			while(numNearMiss>0)
			{
				Vector3 sel = selectInitialNearMissRandomPos();
				placeEnemyNearMissAt(sel);
				numNearMiss--;
			}
		}
		if(m_nEnemyCentroid>0)
		{
			//createCentroidPoints();
			//placeEnemyCentroid();

			setGlobalVars1();
			int numCentroid = m_nEnemyCentroid;
			while(numCentroid>0)
			{
				Vector3 sel = selectInitialNearMissRandomPos();
				placeEnemyCentroidAt(sel);
				numCentroid--;
			}
		}
	}
	private void createCentroidPoints ()
	{
		m_minX = mapBoundary[0].x;
		m_minZ = mapBoundary[0].z;
		m_maxX = mapBoundary[0].x;
		m_maxZ = mapBoundary[0].z;
		for(int i=1;i<4;i++)
		{
			if(m_minX>mapBoundary[i].x)
			{
				m_minX=mapBoundary[i].x;
			}
			if(m_minZ>mapBoundary[i].z)
			{
				m_minZ=mapBoundary[i].z;
			}
			if(m_maxX<mapBoundary[i].x)
			{
				m_maxX=mapBoundary[i].x;
			}
			if(m_maxZ<mapBoundary[i].z)
			{
				m_maxZ=mapBoundary[i].z;
			}
		}
		foreach(Vector3 pathPt in pathPoints)
		{
			List<Geometry> shadows = (List<Geometry>)hTable[pathPt];
			List<Vector3> centroidPtsList = new List<Vector3>();
			foreach(Geometry geo in shadows)
			{
				Vector3 centroidPt = findCentroid1(geo);
				//Debug.Log(centroidPt);
				centroidPtsList.Add(centroidPt);
			}
			hCentroidShadows.Add(pathPt,centroidPtsList);
		}
	}
	//TODO :Error while placing enemies. Maybe random placement works.
	private void placeEnemyCentroid()
	{
		/*int numCentroidEnemies = m_nEnemyCentroid;
		while(numCentroidEnemies>0)
		{
			GameObject enemyObj = Instantiate(enemyPrefab) as GameObject;
			Component.Destroy (enemyObj.GetComponent("Enemy"));
			List<Vector3> centrePts = (List<Vector3>)hCentroidShadows[pathPoints[0]];
			enemyObj.transform.position = centrePts[Random.Range(0,centrePts.Count-1)];//[m_nEnemyCentroid-numCentroidEnemies];
			//m_enemyNextPosCentroidList.Add(((List<Vector3>)hCentroidShadows[pathPoints[1]])[m_nEnemyCentroid-numCentroidEnemies]);

			m_enemyCentroidList.Add(enemyObj);
			m_enemyNextPosCentroidList.Add(findNextPosEnemyCentroid(enemyObj));
			numCentroidEnemies--;
		}*/
	}
	//TODO:Incomplete
	private Vector3 findNextPosEnemyCentroid(GameObject enemyObj)
	{
		Vector3 vecSel = enemyObj.transform.position;
		List<Geometry> shadowPolygonsTemp = (List<Geometry>)hTable [pathPoints [nextPlayerPath]];

		List<Line> allShadowEdges = new List<Line> ();
		foreach(Geometry shadowGeo in shadowPolygonsTemp)
		{
			allShadowEdges.AddRange(shadowGeo.edges);
		}
		float minDist = 0.0f;
		float distTemp = 0.0f;
		Line firstEdge = allShadowEdges[0];
		float y2 = firstEdge.vertex[1].z;
		float y1 = firstEdge.vertex[0].z;
		float x2 = firstEdge.vertex[1].x;
		float x1 = firstEdge.vertex[0].x;
		minDist = Mathf.Abs((y2-y1)*vecSel.x - (x2-x1)*vecSel.z + x2*y1 - y2*x1);
		minDist = minDist/Mathf.Sqrt(Mathf.Pow((y2-y1),2) + Mathf.Pow((x2-x1),2));
		int selEdgeIndx = 0;
		for(int i=1;i<allShadowEdges.Count;i++)
		{
			firstEdge = allShadowEdges[i];
			y2 = firstEdge.vertex[1].z;
			y1 = firstEdge.vertex[0].z;
			x2 = firstEdge.vertex[1].x;
			x1 = firstEdge.vertex[0].x;
			distTemp = Mathf.Abs((y2-y1)*vecSel.x - (x2-x1)*vecSel.z + x2*y1 - y2*x1);
			distTemp = distTemp/Mathf.Sqrt(Mathf.Pow((y2-y1),2) + Mathf.Pow((x2-x1),2));
			if(distTemp<minDist)
			{
				selEdgeIndx = i;
				minDist = distTemp;
			}
		}
		
		return vecSel;
	}
	private void placeEnemyCentroidAt(Vector3 sel)
	{
		GameObject enemyObj = Instantiate(enemyPrefab) as GameObject;
		Component.Destroy (enemyObj.GetComponent("Enemy"));
		enemyObj.transform.position = sel;
		EnemyMovement centroidObj = new EnemyMovement();
		centroidObj.bCaught = false;
		centroidObj.enemyObj = enemyObj;
		centroidObj.vNextPos.Add (findNextPosEnemyCentroid(enemyObj));
		m_enemyCentroidList.Add(centroidObj);
		
	}
	private void placeEnemyNearMissAt(Vector3 sel)
	{
		GameObject enemyObj = Instantiate(enemyPrefab) as GameObject;
		Component.Destroy (enemyObj.GetComponent("Enemy"));
		enemyObj.transform.position = sel;
		EnemyMovement nearMissObj = new EnemyMovement();
		nearMissObj.bCaught = false;
		nearMissObj.enemyObj = enemyObj;
		if(nearMissAlgo==1)
			nearMissObj.vNextPos.Add (findNextPosEnemyNearMiss1(enemyObj));
		else if(nearMissAlgo==2)
			nearMissObj.vNextPos.Add (findNextPosEnemyNearMiss2(enemyObj));
		m_enemyNearMissList.Add(nearMissObj);
			
	}
	private Vector3 selectInitialNearMissRandomPos()
	{
		//sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPoints [0]];
		while(true)
		{
			int selX = Random.Range(0,discretePtsX);
			int selZ = Random.Range(0,discretePtsZ);
			Vector3 sel = (Vector3)h_mapIndxToPt[new Vector2(selX,selZ)];
			while(!pointInShadow(sel,0))
			{
				selX = Random.Range(0,discretePtsX);
				selZ = Random.Range(0,discretePtsZ);
				sel = (Vector3)h_mapIndxToPt[new Vector2(selX,selZ)];
			}

			bool selAgain = false;
			foreach(EnemyMovement nearMissObjTemp in m_enemyNearMissList)
			{
				if(Vector3.Distance(nearMissObjTemp.enemyObj.transform.position,sel)<1.0f)
				{
					selAgain=true;
					break;
				}
			}
			if(selAgain)
			{
				continue;
			}
			return sel;
		}
	}
	private void placeEnemyStatic(sbyte[,] shadowArray)
	{
		List<Vector3> listHiddenSpots = new List<Vector3> ();
		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				if(shadowArray[j,k]==0)
				{
					listHiddenSpots.Add((Vector3)h_mapIndxToPt[new Vector2(j,k)]);
				}
			}
		}
		int numStatic = m_nEnemyStatic;
		while(numStatic>0 && listHiddenSpots.Count>0)
		{
			int sel = Random.Range (0, listHiddenSpots.Count);
			Vector3 selVec = listHiddenSpots[sel];
			GameObject enemyObj = Instantiate(enemyPrefab) as GameObject;
			Component.Destroy (enemyObj.GetComponent("Enemy"));
			enemyObj.transform.position = selVec;
			listHiddenSpots.RemoveAt(sel);
			numStatic--;
		}
	}
	private Vector3 findCentroid1(Geometry geo)
	{

		float radius_hiddenSphere = ((SphereCollider)hiddenSphere.collider).radius*((SphereCollider)hiddenSphere.collider).transform.lossyScale.x;
		int numPts = 0;
		float meanX = 0f;
		float meanZ = 0f;
		for(float j=m_minX;j<m_maxX;j+=m_step)
		{
			for(float k=m_minZ;k<m_maxZ;k+=m_step)
			{
				Vector3 pt = new Vector3(j,1,k);

				if(geo.PointInside(pt) && !Physics.CheckSphere(pt,radius_hiddenSphere))
				{
					numPts++;
					meanX+=j;
					meanZ+=k;
				}

			}
		}
		meanX = meanX / numPts;
		meanZ = meanZ / numPts;
		return new Vector3 (meanX, 1, meanZ);
	}
	Hashtable hCentroidShadows = new Hashtable();
	float m_minX = 0f;
	float m_minZ = 0f;
	float m_maxX = 0f;
	float m_maxZ = 0f;
	float m_step = 0.1f;
	private void setGlobalVars1()
	{
		m_minX = mapBoundary[0].x;
		m_minZ = mapBoundary[0].z;
		m_maxX = mapBoundary[0].x;
		m_maxZ = mapBoundary[0].z;
		for(int i=1;i<4;i++)
		{
			if(m_minX>mapBoundary[i].x)
			{
				m_minX=mapBoundary[i].x;
			}
			if(m_minZ>mapBoundary[i].z)
			{
				m_minZ=mapBoundary[i].z;
			}
			if(m_maxX<mapBoundary[i].x)
			{
				m_maxX=mapBoundary[i].x;
			}
			if(m_maxZ<mapBoundary[i].z)
			{
				m_maxZ=mapBoundary[i].z;
			}
		}
		int Indx = 0;

		
		discretePtsX = (int)(((m_maxX - m_minX) / m_step)+0.5);
		discretePtsZ = (int)(((m_maxZ - m_minZ) / m_step)+0.5);
		Debug.Log("discretePtsX = "+discretePtsX);
		Debug.Log("discretePtsZ = "+discretePtsZ);

		int j1=0;
		for(float j=m_minX;j<m_maxX && j1<discretePtsX;j+=m_step)
		{
			int k1=0;
			for(float k=m_minZ;k<m_maxZ && k1<discretePtsZ;k+=m_step)
			{
				Vector3 pt = new Vector3(j,1,k);
				Vector2 keyTemp = new Vector2((float)j1,(float)k1);
				if(!h_mapIndxToPt.ContainsKey(keyTemp))
				{
					h_mapIndxToPt.Add(keyTemp,pt);
				}
				if(!h_mapPtToIndx.ContainsKey(pt))
				{
					h_mapPtToIndx.Add(pt,keyTemp);

				}
				k1++;
			}
			j1++;
		}

	}
	//private List<Vector3> findEnemyPath2 ()
	private void findEnemyPath2 ()
	{
		setGlobalVars1 ();
		foreach(Vector3 pathPt in pathPoints)
		{
			List<Geometry> shadows = (List<Geometry>)hTable[pathPt];
			List<Vector3> centroidPtsList = new List<Vector3>();
 			foreach(Geometry geo in shadows)
			{
				Vector3 centroidPt = findCentroid1(geo);
				centroidPtsList.Add(centroidPt);
			}
			hCentroidShadows.Add(pathPt,centroidPtsList);
		}
	}
	private List<Vector3> findEnemyPath1 ()
	{
		enemyPath = new List<Vector3>();

		int centralityIndx = -1;
		int row = -1;
		int col = -1;
		//List<Vector2> centreList = new List<Vector2>();
		int numSpots = 1;
		//float minDist = 5;
		for(int i=0;i<pathPoints.Count;i++)
		{
			sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPoints [i]];
			for(int j=0;j<discretePtsX;j++)
			{
				for(int k=0;k<discretePtsZ;k++)
				{
					if(shadowArray[j,k]==0)
					{
						int centralityIndxTemp = centralityCalc(shadowArray,j,k);
						if(centralityIndx<=centralityIndxTemp)
						{
							centralityIndx = centralityIndxTemp;
							row = j;
							col = k;
							/*centreList.Insert(0,new Vector2(row,col));
							if(centreList.Count>numSpots)
							{
								centreList.RemoveAt(numSpots);
							}*/
						}
					}
				}
			}
			enemyPath.Add((Vector3)h_mapIndxToPt[new Vector2(row,col)]);
			//centreList.RemoveAt(0);
		}
		return enemyPath;
	}
	/*private void createEnemies()
	{
		int numSafest = m_enemySafestNumbers;
		sbyte[,] shadowArray = null;
		if (numSafest > 0)
			shadowArray = findSafestSpots ();
		//return;
		while(numSafest>0)
		{
			GameObject enemyObj = Instantiate(enemyPrefab) as GameObject;
			Component.Destroy (enemyObj.GetComponent("Enemy"));
			placeEnemy(shadowArray,enemyObj);
			m_enemySafest.Add(enemyObj);
			numSafest--;
		}
	}*/
	private int centralityCalc2(sbyte[,] shadowArray,int j,int k)
	{
		return 1;
	}
	private int centralityCalc(sbyte[,] shadowArray,int j,int k)
	{
		int rowJ = j;
		int colK = k;
		int centralityIndx = 0;
		int antiCentralityIndx = 0;
		while(true)
		{
			rowJ--;
			colK--;
			int rowLen = (j - rowJ)*2 +1;
			if(rowJ<0 || colK<0 || rowJ+rowLen>discretePtsX || colK+rowLen>discretePtsZ)
				break;
			int centralityIndxTemp = 0;
			int antiCentralityIndxTemp = 0;
			for(int i1=rowJ;i1<rowLen;i1++)
			{
				if(shadowArray[i1,colK]==0)
				{
					centralityIndxTemp++;
				}
				else
				{
					antiCentralityIndxTemp++;
				}
				if(shadowArray[i1,colK+rowLen-1]==0)
				{
					centralityIndxTemp++;
				}
				else
				{
					antiCentralityIndxTemp++;
				}
			}
			for(int i2=colK+1;i2<rowLen-1;i2++)
			{
				if(shadowArray[rowJ,i2]==0)
				{
					centralityIndxTemp++;
				}
				else
				{
					antiCentralityIndxTemp++;
				}
				if(shadowArray[rowJ+rowLen-1,i2]==0)
				{
					centralityIndxTemp++;
				}
				else
				{
					antiCentralityIndxTemp++;
				}
			}
			if(antiCentralityIndxTemp>centralityIndxTemp/4)
			//if(antiCentralityIndx>centralityIndx/10)
				break;
			centralityIndx+=centralityIndxTemp;
			antiCentralityIndx+=antiCentralityIndxTemp;
		}
		return centralityIndx- antiCentralityIndx;
	}
	private void placeEnemy(sbyte[,] shadowArray,GameObject enemyObj)
	{
		//Algo: find centre and place there
		int centralityIndx = -1;
		int row = -1;
		int col = -1;
		List<Vector2> centreList = new List<Vector2>();
		int numSpots = 1;
		//float minDist = 5;
		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				if(shadowArray[j,k]==0)
				{
					int centralityIndxTemp = centralityCalc(shadowArray,j,k);
					if(centralityIndx<=centralityIndxTemp)
					{
						centralityIndx = centralityIndxTemp;
						row = j;
						col = k;
						centreList.Insert(0,new Vector2(row,col));
						if(centreList.Count>numSpots)
						{
							centreList.RemoveAt(numSpots);
						}
					}
				}
			}
		}
		enemyObj.transform.position = (Vector3)h_mapIndxToPt[new Vector2(row,col)];
		centreList.RemoveAt (0);
		foreach(Vector2 vect in centreList)
		{
			GameObject enemyObj1 = Instantiate(enemyPrefab) as GameObject;
			Component.Destroy (enemyObj1.GetComponent("Enemy"));
			enemyObj1.transform.position = (Vector3)h_mapIndxToPt[vect];
		}
	}
	private sbyte[,] findSafestSpots()
	{
		Debug.Log("h_mapIndxToPt.Count = "+h_mapIndxToPt.Keys.Count);
		Debug.Log ("h_discreteShadows.Count = "+h_discreteShadows.Keys.Count);
		sbyte[,] shadowArrayFirst = (sbyte[,])h_discreteShadows [pathPoints [0]];
		sbyte[,] shadowArray = new sbyte[discretePtsX,discretePtsZ];
		sbyte[,] shadowArrayTemp = new sbyte[discretePtsX,discretePtsZ];
		int pathPointsCount = pathPoints.Count;
		//float from_factor = 0.0f;
		//float to_factor = 1.0f;
		while(true)
		{
			System.Array.Copy(shadowArrayFirst,shadowArray,discretePtsX*discretePtsZ);
			for(int i=1;i<pathPointsCount;i++)
			{
				System.Array.Copy(shadowArray,shadowArrayTemp,discretePtsX*discretePtsZ);
				shadowArray = findCommonInArray2D(shadowArray,(sbyte[,])h_discreteShadows [pathPoints [i]]);
				if(checkForNullShadow(shadowArray))
				{
					return shadowArrayTemp;
				}
			}
			break;
			/*if(checkForNullShadow(shadowArray))
			{
				shadowArray = new sbyte[discretePtsX,discretePtsZ];
				to_factor = factor;
				//from_factor/=2;
				pathPointsCount=pathPointsCount*factor;
			}
			else
			{
				float old_factorTemp = old_factor;
				old_factor = factor;
				factor = (old_factorTemp+factor)/2;
				pathPointsCount=pathPointsCount*factor;
				break;
			}
			*/
		}
		//visualizeSafeSpots (shadowArray);
		return shadowArray;
	}
	private bool checkForNullShadow(sbyte[,] array1)
	{
		int shadowPointCounter = 0;
		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				if(array1[j,k]==0)
				{
					shadowPointCounter++;
				}
			}
		}
		if (shadowPointCounter > 0)
		{
			return false;
		}
		else
		{
			return true;
		}
	}
	private sbyte[,] findCommonInArray2D(sbyte[,] array1,sbyte[,] array2)
	{
		sbyte[,] shadowArray = new sbyte[discretePtsX,discretePtsZ];
		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				shadowArray[j,k] = (array1[j,k] > array2[j,k])?array1[j,k]:array2[j,k];
			}
		}
		return shadowArray;
	}
	private void visualizeSafeSpots(sbyte[,] visibleArray2D)
	{
		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				if(visibleArray2D[j,k]==0)
				{
					GameObject clone1 = (GameObject)Instantiate(hiddenSphere);
					Debug.Log("visualizeSafeSpots for pt = "+j+" , "+k);
					clone1.transform.position = (Vector3)h_mapIndxToPt[new Vector2(j,k)];
					//Debug.Log("clone1.transform.position = "+clone1.transform.position);
					//hiddenSphereList.Add(clone1);
				}
			}
		}
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
	private void makeBox() 
	{
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
	void showPosOfPoint(Vector3 pos,Color c)
	{
		if (float.IsNaN (pos.x) || float.IsNaN (pos.z))
			return;
		GameObject sp = (GameObject)GameObject.Find ("StartPoint");
		GameObject tempObj = (GameObject)GameObject.Instantiate (sp);
		Renderer rend = tempObj.GetComponent<Renderer>();
		rend.material.color = c;
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
	private GameObject allLineParent;
	private bool pointInShadow(Vector3 pt,int Indx)
	{
		if (Indx >= pathPoints.Count || Indx < 0)
		{
			Debug.LogError(Indx);
			return false;
		}
		foreach(Geometry geo in globalPolygon)
		{
			if(geo.PointInside(pt))
				return false;
		}

		List<Geometry> shadowPolyTemp = (List<Geometry>)hTable [pathPoints [Indx]];
		foreach(Geometry geo in shadowPolyTemp)
		{
			if(geo.PointInside(pt))
				return true;
		}
		return false;
	}
	//Key = path point (Vector3), Value = 2D array of 1's represent visible, 0's shadows, 2's for inside obstacle ...
	Hashtable h_discreteShadows = new Hashtable();
	//Key = (i,j) , Value = Vector3. i,j are indices corresponding to 2D arrays in h_discreteShadows
	Hashtable h_mapIndxToPt = new Hashtable();
	Hashtable h_mapPtToIndx = new Hashtable();
	//Func: fills h_discreteShadows and h_mapIndxToPt according to step and pathPoints
	int discretePtsX = -1;
	int discretePtsZ = -1;
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

		discretePtsX = (int)(((maxX - minX) / step)+0.5);
		discretePtsZ = (int)(((maxZ - minZ) / step)+0.5);
		Debug.Log(""+(maxX - minX) / step+" discretePtsX = "+discretePtsX+" discretePtsZ = "+discretePtsZ);
		while(Indx<pathPoints.Count)
		{
			List<Geometry> shadowPolyTemp = (List<Geometry>)hTable [pathPoints [Indx]];
			sbyte[,] shadowArray = new sbyte[discretePtsX,discretePtsZ];

			float radius_hiddenSphere = ((SphereCollider)hiddenSphere.collider).radius*((SphereCollider)hiddenSphere.collider).transform.lossyScale.x;
			int j1=0;	
			for(float j=minX;j<maxX && j1<discretePtsX;j+=step)
			{
				int k1=0;
				for(float k=minZ;k<maxZ && k1<discretePtsZ;k+=step)
				{
					Vector3 pt = new Vector3(j,1,k);
					Vector2 keyTemp = new Vector2(j1,k1);
					if(!h_mapIndxToPt.ContainsKey(keyTemp))
					{
						//Debug.Log("Adding key value pair to h_mapIndxToPt"+j1+" , "+k1);
						h_mapIndxToPt.Add(keyTemp,pt);
					}
					//Debug.Log(j1+" , "+k1);
					//Debug.Log(j+" < "+maxX+" , "+k+" < "+maxZ);
					if(pointInShadow(pt,Indx) && !Physics.CheckSphere(pt,radius_hiddenSphere))
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
			h_discreteShadows.Add(pathPoints[Indx],shadowArray);
			Indx++;
			//Debug.Log("h_mapIndxToPt.Count = "+h_mapIndxToPt.Keys.Count);
			//Debug.Log("h_discreteShadows.Count = "+h_discreteShadows.Keys.Count);
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
	class VisibleTriangulation
	{
		List<Vector3> points;
		List<int> newTriangles;
		public void setPoints(List<Vector3> pts)
		{
			points = pts;
		}
		public void setTriangles(List<int> newT)
		{
			newTriangles = newT;
		}
		public List<Vector3> getPoints()
		{
			return points;
		}
		public List<int> getTriangles()
		{
			return newTriangles;
		}
	}
	private List<int> reverseTriangels(List<int> newTriangles)
	{
		List<int> newTrianglesReversed = new List<int> ();
		for(int i=0;i<newTriangles.Count-3;i+=3)
		{
			newTrianglesReversed.Add(newTriangles[i+2]);
			newTrianglesReversed.Add(newTriangles[i+1]);
			newTrianglesReversed.Add(newTriangles[i]);
		}
		return newTrianglesReversed;
	}
	List<int> triangulateVisible(List<Vector3> points)
	{
		List<int> newTriangles = new List<int>();
		int i = 1;
		for(i=1;i<points.Count-1;i++)
		{
			newTriangles.Add(0);
			newTriangles.Add(i);
			newTriangles.Add(i+1);
		}
		newTriangles.Add(0);
		newTriangles.Add(points.Count-1);
		newTriangles.Add(1);

		/*newTriangles.Add(0);
		newTriangles.Add(points.Count);
		newTriangles.Add(1);*/

		return newTriangles;
	}
	Hashtable h_visible_Star_Triangles = new Hashtable ();
	private void createVisibleTriangulation()
	{
		int Indx=0;
		while(Indx<pathPoints.Count)
		{
			Geometry visiblePolyTemp = (Geometry)hVisiblePolyTable [pathPoints [Indx]];

			//List<Geometry> shadowPolyTemp = (List<Geometry>)hTable [pathPoints [Indx]];
			List<int> newTriangles;
			List<StandardPolygon> sdList = arrangeCounterClockwise(visiblePolyTemp);
			List<VisibleTriangulation> llist = new List<VisibleTriangulation>();
			//for(int i=0;i<shadowPolyTemp.Count;i++)
			{
				//List<StandardPolygon> sdList = arrangeCounterClockwise(shadowPolyTemp[i]);
				
				foreach(StandardPolygon sd in sdList)
				{
					List<Vector3> points = sd.getVertices();
					//newTriangles = applyEarClipping(points);

					List<Vector3> pointsTemp = new List<Vector3>();
					pointsTemp.Add(pathPoints[Indx]);
					pointsTemp.AddRange(points);
					points = pointsTemp;

					newTriangles = triangulateVisible(points);
					List<int> newTrianglesReversed = reverseTriangels(newTriangles);
					//newTriangles.AddRange(newTrianglesReversed);
					//newTriangles = newTrianglesReversed;

					VisibleTriangulation tempVT = new VisibleTriangulation();
					tempVT.setPoints(points);
					tempVT.setTriangles(newTriangles);
					llist.Add(tempVT);
				}
			}
			h_visible_Star_Triangles.Add(Indx,llist);
			Indx++;
		}
	}
	private void displayShadowMeshes(int Indx)
	{
		/*Geometry visiblePolyTemp = (Geometry)hVisiblePolyTable [pathPoints [Indx]];

		List<StandardPolygon> sdList = arrangeCounterClockwise(visiblePolyTemp);
		Debug.Log("sdList.Count = "+sdList.Count);*/
		/*Geometry visiblePolyTemp = (Geometry)hVisiblePolyTable [pathPoints [Indx]];
		foreach(Line l in visiblePolyTemp.edges)
		{
			l.DrawVector(allLineParent);
		}*/


		List<int> newTriangles;
		foreach(GameObject tempObj in shadowMeshes)
		{
			GameObject.Destroy(tempObj);
		}
		shadowMeshes.Clear ();
		List<VisibleTriangulation> llist = (List<VisibleTriangulation>)h_visible_Star_Triangles[Indx];

		foreach(VisibleTriangulation VT in llist)
		{
			//Debug.Log("displayShadowMeshes Index = "+Indx);
			/*Debug.Log("sd.Count = "+sd.getVertices().Count);
			List<Vector3> points = sd.getVertices();
			newTriangles = applyEarClipping(points);*/
			List<Vector3> points = VT.getPoints();
			Debug.Log("VT.getPoints() = "+VT.getPoints().Count);
			Debug.Log("VT.getTriangles() = "+VT.getTriangles().Count);
			newTriangles = VT.getTriangles();
			shadowMeshes.Add(new GameObject("ShadowMesh"));
			MeshFilter filter = shadowMeshes[shadowMeshes.Count-1].AddComponent<MeshFilter>();

			MeshRenderer meshRenderer = shadowMeshes[shadowMeshes.Count-1].AddComponent<MeshRenderer>();
			Material material = mat;
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
	private void displayShadowMeshes_Old(int Indx)
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
	//float lastTimeUpdateCalled=Time.time;
	//int lastUpdateCalled=100;
	//System.DateTime lastTimeUpdateCalled = System.DateTime.Now;
	bool AnalyzeNearestPathPoint()
	{
		/*lastUpdateCalled--;
		if(lastUpdateCalled==0)
		{
			lastUpdateCalled=100;
		}
		else 
			return false;*/
		/*if(Time.time-lastTimeUpdateCalled>1.0f)
			lastTimeUpdateCalled = Time.time;
		else
			return false;*/
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
			if(hVisiblePolyTable.ContainsKey(pPoint))
				continue;
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
