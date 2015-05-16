using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
public partial class Visibility1 : MonoBehaviour 
{
	Hashtable m_hCompleteNodeTable = new Hashtable();
	private void DumpInfoFile(string dirName,float totalTime)
	{
		string sourceFileName = dirName+"\\Info"+".txt";
		StreamWriter sw = new StreamWriter(sourceFileName);
		sw.WriteLine("Scene Name = "+currSceneName+"");
		sw.WriteLine("Discrete rows & cols = "+discretePtsX+" X "+discretePtsZ+"");
		sw.WriteLine("Speed of the Player = "+speedPlayer+"");
		sw.WriteLine("Distance covered by the player = "+m_stepDistance+"");
		sw.WriteLine("Max speed of the Enemy = "+speedEnemy+"");
		sw.WriteLine("Max distance covered by the Enemy = "+standardMaxMovement+"");
		sw.WriteLine("Time taken to calculate tree structure = "+totalTime+" mins"+"");
		sw.WriteLine("Current Date = " + System.DateTime.Now.ToLongDateString()+". Time = "+ System.DateTime.Now.ToLongTimeString() + "");
		sw.Close ();
	}
	private void executeTrueCase2()
	{
		setGlobalVars1();
		standardMaxMovement = speedEnemy*(m_stepDistance/speedPlayer);
		Debug.Log ("Initialize standardMaxMovement = " + standardMaxMovement);
		float startTime = Time.realtimeSinceStartup;
		string dirName = createSaveDataDir(Application.dataPath);
		string sourceFileName = dirName+"\\Data"+".txt";
		StreamWriter sw = new StreamWriter(sourceFileName,true);
		//NodeSignature|ParentSignature
		sw.WriteLine("(Vector3;level)|(Vector3;level)"+"");
		int j1=0;
		int numTimesToRun = 2;
		int countNumPts = 0;
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

				//if(numTimesToRun==1)	
				executeTrueCaseFor2(keyTemp,sw);

				//TODO:Remove
				if(numTimesToRun==0)
				{
					float totalTime1 = (Time.realtimeSinceStartup - startTime)/60;
					Debug.Log("executeTrueCase Finished. Time taken is = "+totalTime1+" mins");
					DumpInfoFile (dirName,totalTime1);
					writeNodeStructure3 (sw);
					sw.Close ();
					return;
				}
				numTimesToRun--;
				//TODO:Remove;

				k1++;
			}
			j1++;
		}
		writeNodeStructure3 (sw);
		float totalTime = (Time.realtimeSinceStartup - startTime)/60;
		Debug.Log("executeTrueCase Finished. Time taken is = "+totalTime+" mins");
		DumpInfoFile (dirName,totalTime);
		sw.Close ();
	}
	private void executeTrueCaseFor2(Vector2 indexOfPt,StreamWriter sw)
	{
		//return;
		Vector3 pt = (Vector3)h_mapIndxToPt[indexOfPt];
		NodeShadow headNode = new NodeShadow (pt);
		headNode.setSafetyLevel (0);
		Hashtable h_mapPtToNode = new Hashtable();
		int levelOfAccess = 1;
		List<NodeShadow> nodeSafeLevelNow = new List<NodeShadow> ();
		nodeSafeLevelNow.Add (headNode);
		List<NodeShadow> nodeSafeLevelNext = reachableChildren2 (headNode,indexOfPt,levelOfAccess,h_mapPtToNode);
		h_mapPtToNode.Clear ();
		while(levelOfAccess<pathPoints.Count)//TODO:think other exit cases
		{
			levelOfAccess++;
			nodeSafeLevelNow = nodeSafeLevelNext;
			nodeSafeLevelNext = new List<NodeShadow>();
			
			foreach(NodeShadow child in nodeSafeLevelNow)
			{
				Vector2 indexOfPtTemp = (Vector2)h_mapPtToIndx[child.getPos()];
				List<NodeShadow> childrenTemp = reachableChildren2 (child,indexOfPtTemp,levelOfAccess,h_mapPtToNode);
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
		//writeNodeStructure2(headNode,sw);
	}
	private bool addPossibleChild2(Vector2 tempVect2,NodeShadow node,int pathPointIndx,Hashtable h_mapPtToNode)
	{
		//Debug.Log ("Possible Child 1 ="+(Vector3)h_mapIndxToPt [tempVect2]);
		if(h_mapIndxToPt.ContainsKey(tempVect2))
		{
			Vector3 tempVect3 = (Vector3)h_mapIndxToPt[tempVect2];
			Vector4 tempVect4 = new Vector4(tempVect3.x,tempVect3.y,tempVect3.z,pathPointIndx);
			//Debug.Log("standardMaxMovement = "+standardMaxMovement);
			//Debug.Log("Possible Child 2 = "+tempVect3);
			if(pointInShadow(tempVect3,pathPointIndx) && Vector3.Distance(node.getPos(),tempVect3)<=standardMaxMovement)
			{
				NodeShadow nodeChild;
				if(h_mapPtToNode.ContainsKey(tempVect3))
				{
					nodeChild = (NodeShadow)h_mapPtToNode[tempVect3];
				}
				else if(m_hCompleteNodeTable.ContainsKey(tempVect4))
				{
					nodeChild = (NodeShadow)m_hCompleteNodeTable[tempVect4];
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
	private List<NodeShadow> reachableChildren2(NodeShadow node,Vector2 indexOfPt,int pathPointIndx,Hashtable h_mapPtToNode)
	{
		int rowJ = (int)indexOfPt.x;
		int colK = (int)indexOfPt.y;
		addPossibleChild2(indexOfPt,node,pathPointIndx,h_mapPtToNode);
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
				bStillReachable = addPossibleChild2(tempVect2,node,pathPointIndx,h_mapPtToNode);
				if(bStillReachable)
					bRunAgain=true;
				tempVect2 = new Vector2(i1,colK+rowLen-1);
				bStillReachable = addPossibleChild2(tempVect2,node,pathPointIndx,h_mapPtToNode);
				if(bStillReachable)
					bRunAgain=true;
			}
			for(int i2=colK+1;i2<colK+rowLen-1;i2++)
			{
				Vector2 tempVect2 = new Vector2(rowJ,i2);
				bStillReachable = addPossibleChild2(tempVect2,node,pathPointIndx,h_mapPtToNode);
				if(bStillReachable)
					bRunAgain=true;
				tempVect2 = new Vector2(rowJ+rowLen-1,i2);
				bStillReachable = addPossibleChild2(tempVect2,node,pathPointIndx,h_mapPtToNode);
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
		Vector3 pt = (Vector3)h_mapIndxToPt[indexOfPt];
		Vector4 pt4 = new Vector4(pt.x,pt.y,pt.z,pathPointIndx-1);
		if(!m_hCompleteNodeTable.ContainsKey(pt4))
		{
			m_hCompleteNodeTable.Add (pt4,node);
		}
		List<NodeShadow> newChildren = new List<NodeShadow> ();
		foreach(NodeShadow childTemp in node.getChildren())
		{
			pt = childTemp.getPos();
			pt4 = new Vector4(pt.x,pt.y,pt.z,pathPointIndx);
			if(!m_hCompleteNodeTable.ContainsKey(pt4))
			{
				newChildren.Add(childTemp);
			}
		}
		return newChildren;
	}
	private void writeNodeStructure3(StreamWriter sw)
	{
		foreach(Vector4 vect in m_hCompleteNodeTable.Keys)
		{
			NodeShadow node = (NodeShadow)m_hCompleteNodeTable[vect];
			int numParents = node.getParent().Count;
			if(numParents==0)
			{
				sw.Write("("+node.getPos()+";"+node.getSafetyLevel()+")|("+null+";"+null+")");
				sw.WriteLine("");
			}
			for(int i=0;i<numParents;i++)
			{
				sw.Write("("+node.getPos()+";"+node.getSafetyLevel()+")|("+node.getParent()[i].getPos()+";"+node.getParent()[i].getSafetyLevel()+")");
				sw.WriteLine("");
			}
		}
	}
	private void writeNodeStructure2(NodeShadow headNode,StreamWriter sw)
	{
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

	}
	private void displayPredictedPaths2()
	{
		float startTime = Time.realtimeSinceStartup;
		List<NodeShadow> headNodes = readNodeStructureFor2();
		//foreach(NodeShadow headNode in headNodes)
		NodeShadow headNode = headNodes [2];
		{
			//NodeShadow headNode = (NodeShadow)m_hCompleteNodeTable[vect];
			List<NodeShadow> firstPath = quickShortestPathDetected (headNode);
			
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
		}
		float totalTime = (Time.realtimeSinceStartup - startTime)/60;
		Debug.Log("Finished displayPredictedPaths2. Time took to calculate and show shortest path = "+totalTime+" minutes");
	}
	private List<NodeShadow> readNodeStructureFor2()
	{
		List<NodeShadow> headNodes = new List<NodeShadow> ();
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
		//NodeShadow headNode = new NodeShadow ();


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

			Vector4 parentKeyObj = new Vector4();
			Vector4 keyObj = new Vector4(float.Parse(line[0]),float.Parse(line[1]),float.Parse(line[2]),float.Parse(line[3]));
			if(keyObj.w==0.0f)//A head Node
			{
				NodeShadow headNode = new NodeShadow(new Vector3(keyObj.x,keyObj.y,keyObj.z));
				headNode.setSafetyLevel((int)keyObj.w);
				m_hCompleteNodeTable.Add(keyObj,headNode);
				headNodes.Add(headNode);
				continue;
			}
			else
			{
				parentKeyObj = new Vector4(float.Parse(line[4]),float.Parse(line[5]),float.Parse(line[6]),float.Parse(line[7]));
			}

			if(!m_hCompleteNodeTable.ContainsKey(keyObj))
			{
				//NodeShadow node = new NodeShadow(keyObj.pt);
				NodeShadow node = new NodeShadow(new Vector3(keyObj.x,keyObj.y,keyObj.z));
				node.setSafetyLevel((int)keyObj.w);
				//Debug.Log(parentKeyObj.pt+" , "+parentKeyObj.safetyLevel);
				NodeShadow parentNode= (NodeShadow)m_hCompleteNodeTable[parentKeyObj];

				parentNode.addChild(node);

				m_hCompleteNodeTable.Add(keyObj,node);
			}
			else
			{
				NodeShadow node= (NodeShadow)m_hCompleteNodeTable[keyObj];
				NodeShadow parentNode= (NodeShadow)m_hCompleteNodeTable[parentKeyObj];
				parentNode.addChild(node);
			}

			
		}
		Debug.Log ("Number of nodes are  = " + m_hCompleteNodeTable.Keys.Count);
		sr.Close ();
		return headNodes;
	}
}
