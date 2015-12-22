using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
public partial class Visibility1 : MonoBehaviour 
{

	private void createDiscreteMapForAgentBased()
	{
		int Indx = 0;
		while(Indx<pathPoints.Count)
		{
			//if(h_discreteShadows.ContainsKey(pathPoints[Indx]))
			if(h_discreteShadows.ContainsKey(Indx))
			{
				Indx++;
				Debug.Log ("##############$$$$$$$$$$ Repetition of a path point at = "+Indx);
				continue;
			}
			sbyte[,] shadowArray = new sbyte[discretePtsX,discretePtsZ];
			float radius_hiddenSphere = radius_enemy;


			/*int j1=0;	
			for(float j=m_minX;j<m_maxX && j1<discretePtsX;j+=m_step)
			{
				int k1=0;
				for(float k=m_minZ;k<m_maxZ && k1<discretePtsZ;k+=m_step)
				{
					Vector3 pt = new Vector3(j,1,k);
					
					if(pointInShadow(pt,Indx))// && !Physics.CheckSphere(pt,radius_hiddenSphere))
					{
						shadowArray[j1,k1]=0;
					}
					//else if(CheckIfInsidePolygon(pt))
					//{
					//	shadowArray[j1,k1]=2;
					//}
					else
					{
						shadowArray[j1,k1]=1;
					}
					k1++;
				}
				j1++;
			}*/

			foreach(Vector2 indx in h_mapIndxToPt.Keys)
			{
				Vector3 pt = (Vector3)h_mapIndxToPt[indx];
				if(pointInShadow(pt,Indx))
				{
					shadowArray[(int)indx.x,(int)indx.y]=0;
				}
				else
				{
					shadowArray[(int)indx.x,(int)indx.y]=1;
				}
			}
			



			//h_discreteShadows.Add(pathPoints[Indx],shadowArray);
			h_discreteShadows.Add(Indx,shadowArray);
			Indx++;
		}
	}











	private void writeEachLevel(int level,sbyte[,] shadowArray)
	{
		System.IO.File.CreateText(file_AgentBasedEachLevelFolder+"\\PathPoint_"+level+".txt");
		StreamWriter sw = new StreamWriter(file_AgentBasedEachLevelFolder+"\\PathPoint_"+level+".txt");

		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				sw.Write(", " + shadowArray[j,k]);
			}
			sw.WriteLine(""); 
		}
		sw.Close ();

	}



	private void readLevel(int pathPointReadLevel)
	{
		StreamReader sr = new StreamReader(file_AgentBasedEachLevelFolder+"\\PathPoint_"+pathPointReadLevel+".txt");

		setGlobalVars1 ();
		sbyte[,] shadowArray = new sbyte[discretePtsX,discretePtsZ];

		
		int j = 0;
		int k = 0;
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
				shadowArray[j,k] = sbyte.Parse(s);
				k++;
			}
			j++;
		}
		sr.Close ();


		for(int i=0;i<discretePtsX;i++)
		{
			for(j=0;j<discretePtsZ;j++)
			{
				//float greenNum = pointsArray[i,j]/(pathPoints.Count-1);
				//showPosOfPoint((Vector3)h_mapIndxToPt[new Vector2(i,j)],new Color(0.0f,greenNum,0.0f));
				if(shadowArray[i,j]!=9)
					continue;
				float greenNum = 1.0f;
				showPosOfPointRectangle((Vector3)h_mapIndxToPt[new Vector2(i,j)],Color.Lerp(Color.white,Color.green,greenNum));
			}
		}


	}



	bool bDisplayEachLevelAgentBased = false;

	private void agentBasedAssignmentFromEnd()
	{
		if(bDisplayEachLevelAgentBased)
		{
			int pathPointReadLevel = 10;
			readLevel(pathPointReadLevel);
			return;
		}
		FileUtil.DeleteFileOrDirectory (file_AgentBasedEachLevelFolder);
		//if (!System.IO.Directory.Exists(file_AgentBasedEachLevelFolder))
		//{
		System.IO.Directory.CreateDirectory(file_AgentBasedEachLevelFolder);
		//}
		standardMaxMovement = speedEnemy*(m_stepDistance/speedPlayer);
		createDiscreteMapForAgentBased ();
		Hashtable relationMap = createRelationships ();

		int row = -1;
		int col = -1;
		//Initialize:Placing agents
		//sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPoints [pathPoints.Count-1]];
		sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPoints.Count-1];
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
		writeEachLevel(pathPoints.Count-1,shadowArray);
		sbyte[,] shadowArrayPrev;
		sbyte[,] shadowArrayNext;
		for(int i=pathPoints.Count-1;i>0;i--)
		{
			shadowArrayPrev = (sbyte[,])h_discreteShadows [i];
			shadowArrayNext = (sbyte[,])h_discreteShadows [i-1];
			Hashtable nodeTable = new Hashtable();//contains all node objects
			Hashtable parentTable = new Hashtable();//contains parent and child nodelist
			for(int j=0;j<discretePtsX;j++)
			{
				for(int k=0;k<discretePtsZ;k++)
				{
					if(shadowArrayPrev[j,k]==9)
					{
						findChildren(shadowArrayNext,j,k,nodeTable,parentTable,relationMap);
						//placeAgent(shadowArrayNext,j,k);
					}
				}
			}
			//completed Hashtables
			fillNextLevel(shadowArrayNext,nodeTable,parentTable,i);
			writeEachLevel(i-1,shadowArrayNext);
		}
		//shadowArrayNext = (sbyte[,])h_discreteShadows [pathPoints [2]];
		//shadowArrayPrev = (sbyte[,])h_discreteShadows [pathPoints [pathPoints.Count-1]];
		for(int i=pathPoints.Count-1;i>=0;i--)
		{
			shadowArrayPrev = (sbyte[,])h_discreteShadows [i];
			Debug.Log("Agents at "+i+" = "+countAgents(shadowArrayPrev));
		}
		//Debug.Log("Agents at start = "+countAgents(shadowArrayPrev));
		//Debug.Log("Agents surviving at the end = "+countAgents(shadowArrayNext));
		displaySurvivingAgentsNew ();
	}
	private void fillNextLevel(sbyte[,] shadowArrayNext,Hashtable nodeTable,Hashtable parentTable,int level)
	{
		int fillingCurrentCount = 1;
		int numRuns = 10;
		List<AgentNode> listToRemovedAgentNodes;
		while(parentTable.Keys.Count>0)// && numRuns-->0)
		{
			bool bStartAgain = false;
			int leastFilledCount = -1;
			Vector2 keySelected = new Vector2();
			List<Vector2> listTobeRemoved = new List<Vector2>();
			/*int NotFilledCount=0;
			foreach(Vector2 key in parentTable.Keys)
			{
				NotFilledCount=0;
				foreach(AgentNode node in (List<AgentNode>)parentTable[key])
				{
					if(!node.getFillStatus())
					{
						NotFilledCount++;
						break;
					}
				}
				if(NotFilledCount>0)
				{
					keySelected = key;
					break;
				}
				else
				{
					listTobeRemoved.Add(key);
				}
			}
			if(NotFilledCount==0)
				break;*/
			foreach(Vector2 key in parentTable.Keys)
			{
				//Vector2 key = (Vector2)parentTable.Keys[i];
				int NotFilledCount=0;
				listToRemovedAgentNodes = new List<AgentNode>();
				foreach(AgentNode node in (List<AgentNode>)parentTable[key])
				{
					if(!node.getFillStatus())
					{
						NotFilledCount++;
					}
					else
					{
						listToRemovedAgentNodes.Add(node);
					}
				}
				foreach(AgentNode node in listToRemovedAgentNodes)
				{
					((List<AgentNode>)parentTable[key]).Remove(node);
				}
				if(NotFilledCount>0)
				{
					if(leastFilledCount>0)
					{
						if(NotFilledCount<leastFilledCount)
						{
							leastFilledCount = NotFilledCount;
							keySelected = key;
						}
					}
					else
					{
						leastFilledCount = NotFilledCount;
						keySelected = key;
					}
				}
				else
				{
					listTobeRemoved.Add(key);
				}
			}

			if(leastFilledCount<0)
			{
				Debug.Log("level = "+level);
				Debug.Log("leastFilledCount = "+leastFilledCount);
				break;
			}
			foreach(Vector2 vect in listTobeRemoved)
			{
				parentTable.Remove(vect);
			}
			//Debug.Log("level = "+level);
			//Debug.Log("leastFilledCount = "+leastFilledCount);
			listToRemovedAgentNodes = new List<AgentNode>();
			foreach(AgentNode node in (List<AgentNode>)parentTable[keySelected])
			{
				if(node.getFillStatus())
				{
					listToRemovedAgentNodes.Add(node);
				}
			}
			foreach(AgentNode node in listToRemovedAgentNodes)
			{
				((List<AgentNode>)parentTable[keySelected]).Remove(node);
			}
			int sel = (int)Random.Range(0,((List<AgentNode>)parentTable[keySelected]).Count-1);
			//Debug.Log("sel = "+sel);
			//int sel = 0;
			AgentNode nodeSel= ((List<AgentNode>)parentTable[keySelected])[sel];

			/*while(true)
			{
				//sel = (int)Random.Range(0,((List<AgentNode>)parentTable[keySelected]).Count-1);
				//Debug.Log("sel = "+sel);
				if(!nodeSel.getFillStatus())
					break;
				sel++;
				nodeSel= ((List<AgentNode>)parentTable[keySelected])[sel];

			}
			Debug.Log("Now selected = "+(sel));*/
			nodeSel.fillNode();
			parentTable.Remove(keySelected);

			//found leastFilledCount;
			/*foreach(Vector2 key in parentTable.Keys)
			{
				//Vector2 key = (Vector2)parentTable.Keys[i];
				int NotFilledCount=0;
				foreach(AgentNode node in (List<AgentNode>)parentTable[key])
				{
					if(!node.getFillStatus())
					{
						NotFilledCount++;
					}
				}
				if(NotFilledCount==leastFilledCount)
				{
					int sel = (int)Random.Range(0,((List<AgentNode>)parentTable[key]).Count-1);
					AgentNode nodeSel= ((List<AgentNode>)parentTable[key])[sel];
					while(nodeSel.getFillStatus())
					{
						sel = (int)Random.Range(0,((List<AgentNode>)parentTable[key]).Count-1);
						nodeSel= ((List<AgentNode>)parentTable[key])[sel];
					}
					nodeSel.fillNode();
					parentTable.Remove(key);
					bStartAgain = true;
					break;
				}
			}*/
			//Debug.Log("parentTable.Keys.Count = "+parentTable.Keys.Count);
			//if(!bStartAgain)
				//fillingCurrentCount++;
		}
		foreach(Vector2 key in nodeTable.Keys)
		{
			if(((AgentNode)nodeTable[key]).getFillStatus())
			{
				shadowArrayNext[(int)key.x,(int)key.y] = 9;
			}
		}
	}
	class AgentNode
	{
		Vector2 vectPos;
		bool bFilled = false;
		public AgentNode(Vector2 vect)
		{
			vectPos = vect;
		}
		public void fillNode()
		{
			bFilled = true;
		}
		public bool getFillStatus()
		{
			return bFilled;
		}
	}
	private void findChildren(sbyte[,] shadowArrayNext,int j,int k, Hashtable nodeTable,Hashtable parentTable,Hashtable relationMap)
	{
		int rowJ = j;
		int colK = k;
		standardMaxMovement = speedEnemy*(m_stepDistance/speedPlayer);
		Vector3 currPos = ((Vector3)h_mapIndxToPt[new Vector2(j,k)]);
		List<Vector2> listOfAvailablePos = new List<Vector2> ();
		
		
		/*bool runAgain = true;
		
		if(shadowArrayNext[j,k]==0)
		{
			listOfAvailablePos.Add(new Vector2(j,k));
		}
		while(runAgain)
		{
			runAgain = false;
			rowJ--;
			colK--;
			int rowLen = (j - rowJ)*2 +1;
			if(rowJ<0 || colK<0 || rowJ+rowLen>discretePtsX || colK+rowLen>discretePtsZ)
				break;
			for(int i1=rowJ;i1<rowJ+rowLen;i1++)
			{
				Vector3 vectPos = (Vector3)h_mapIndxToPt[new Vector2(i1,colK)];
				if(shadowArrayNext[i1,colK]==0 && Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
				{
					runAgain = true;
					listOfAvailablePos.Add(new Vector2(i1,colK));
				}
				vectPos = (Vector3)h_mapIndxToPt[new Vector2(i1,colK+rowLen-1)];
				if(shadowArrayNext[i1,colK+rowLen-1]==0 && Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
				{
					runAgain = true;
					listOfAvailablePos.Add(new Vector2(i1,colK+rowLen-1));
				}
			}
			for(int i2=colK+1;i2<colK+rowLen-1;i2++)
			{
				Vector3 vectPos = (Vector3)h_mapIndxToPt[new Vector2(rowJ,i2)];
				if(shadowArrayNext[rowJ,i2]==0 && Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
				{
					runAgain = true;
					listOfAvailablePos.Add(new Vector2(rowJ,i2));
				}
				vectPos = (Vector3)h_mapIndxToPt[new Vector2(rowJ+rowLen-1,i2)];
				if(shadowArrayNext[rowJ+rowLen-1,i2]==0 && Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos))
				{
					runAgain = true;
					listOfAvailablePos.Add(new Vector2(rowJ+rowLen-1,i2));
				}
			}
		}*/
		//New algo to fill listOfAvailablePos
		List<Vector3> listNeighbors = (List<Vector3>)relationMap[currPos];
		foreach(Vector3 neighborPosVec3 in listNeighbors)
		{
			Vector2 neighborPosVec2 = ((Vector2)h_mapPtToIndx[neighborPosVec3]);
			if(shadowArrayNext[(int)neighborPosVec2.x,(int)neighborPosVec2.y]==0)
			{
				listOfAvailablePos.Add(neighborPosVec2);
			}
		}



		List<AgentNode> nodelist = new List<AgentNode> ();
		foreach(Vector2 vect in listOfAvailablePos)
		{
			if(!nodeTable.ContainsKey(vect))
			{
				nodeTable.Add(vect,new AgentNode(vect));
			}
			nodelist.Add ((AgentNode)nodeTable[vect]);
		}
		parentTable.Add (new Vector2 (j, k), nodelist);
	}

	private void agentBasedAssignment()
	{
		createDiscreteMap ();
		int row = -1;
		int col = -1;
		int numSpots = 1;
		//Initialize:Placing agents
		sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [0];
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
		for(int i=0;i<pathPoints.Count-1;i++)
		{
			shadowArrayPrev = (sbyte[,])h_discreteShadows [i];
			shadowArrayNext = (sbyte[,])h_discreteShadows [i+1];
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
		shadowArrayPrev = (sbyte[,])h_discreteShadows [0];
		shadowArrayNext = (sbyte[,])h_discreteShadows [pathPoints.Count-1];
		Debug.Log("Agents at start = "+countAgents(shadowArrayPrev));
		Debug.Log("Agents surviving at the end = "+countAgents(shadowArrayNext));
		displaySurvivingAgents();
	}
	int countAgents(sbyte[,] shadowArray)
	{
		int counterAgents = 0;
		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				if(shadowArray[j,k]==9)
				{
					counterAgents++;
				}
			}
		}
		return counterAgents;
	}
	private void displaySurvivingAgentsNew()
	{
		sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [0];
		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				if(shadowArray[j,k]==9)
				{
					showPosOfPointRectangle((Vector3)h_mapIndxToPt[new Vector2(j,k)],Color.Lerp(Color.white,Color.green,1.0f));
					//GameObject clone1 = (GameObject)Instantiate(hiddenSphere);
					//clone1.transform.position = (Vector3)h_mapIndxToPt[new Vector2(j,k)];
				}
			}
		}
	}
	private void displaySurvivingAgents()
	{
		sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPoints.Count-1];
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
	private void placeAgent(sbyte[,] shadowArrayNext,int j,int k)
	{
		int rowJ = j;
		int colK = k;
		standardMaxMovement = speedEnemy*(m_stepDistance/speedPlayer);
		Vector3 currPos = ((Vector3)h_mapIndxToPt[new Vector2(j,k)]);
		List<Vector2> listOfAvailablePos = new List<Vector2> ();
		
		
		bool runAgain = true;
		
		if(shadowArrayNext[j,k]==0)
		{
			listOfAvailablePos.Add(new Vector2(j,k));
		}
		while(runAgain)
		{
			runAgain = false;
			rowJ--;
			colK--;
			int rowLen = (j - rowJ)*2 +1;
			if(rowJ<0 || colK<0 || rowJ+rowLen>discretePtsX || colK+rowLen>discretePtsZ)
				break;
			for(int i1=rowJ;i1<rowJ+rowLen;i1++)
			{
				if(shadowArrayNext[i1,colK]==0 && Vector3.Distance(currPos,(Vector3)h_mapIndxToPt[new Vector2(i1,colK)])<=standardMaxMovement)
				{
					runAgain = true;
					listOfAvailablePos.Add(new Vector2(i1,colK));
				}
				if(shadowArrayNext[i1,colK+rowLen-1]==0 && Vector3.Distance(currPos,(Vector3)h_mapIndxToPt[new Vector2(i1,colK+rowLen-1)])<=standardMaxMovement)
				{
					runAgain = true;
					listOfAvailablePos.Add(new Vector2(i1,colK+rowLen-1));
				}
			}
			for(int i2=colK+1;i2<colK+rowLen-1;i2++)
			{
				if(shadowArrayNext[rowJ,i2]==0 && Vector3.Distance(currPos,(Vector3)h_mapIndxToPt[new Vector2(rowJ,i2)])<=standardMaxMovement)
				{
					runAgain = true;
					listOfAvailablePos.Add(new Vector2(rowJ,i2));
				}
				if(shadowArrayNext[rowJ+rowLen-1,i2]==0 && Vector3.Distance(currPos,(Vector3)h_mapIndxToPt[new Vector2(rowJ+rowLen-1,i2)])<=standardMaxMovement)
				{
					runAgain = true;
					listOfAvailablePos.Add(new Vector2(rowJ+rowLen-1,i2));
				}
			}
		}
		//Selecting a random pos
		int selIndx = (int)Random.Range (0, listOfAvailablePos.Count - 1);
		shadowArrayNext [(int)listOfAvailablePos [selIndx].x, (int)listOfAvailablePos [selIndx].y] = 9;
	}
}