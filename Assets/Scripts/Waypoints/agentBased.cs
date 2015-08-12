using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
public partial class Visibility1 : MonoBehaviour 
{
	private void agentBasedAssignmentFromEnd()
	{
		createDiscreteMap ();
		int row = -1;
		int col = -1;
		int numSpots = 1;
		//Initialize:Placing agents
		sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPoints [pathPoints.Count-1]];
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
		for(int i=pathPoints.Count-1;i>0;i--)
		{
			shadowArrayPrev = (sbyte[,])h_discreteShadows [pathPoints [i]];
			shadowArrayNext = (sbyte[,])h_discreteShadows [pathPoints [i-1]];
			Hashtable nodeTable = new Hashtable();//contains all node objects
			Hashtable parentTable = new Hashtable();//contains parent and child nodelist
			for(int j=0;j<discretePtsX;j++)
			{
				for(int k=0;k<discretePtsZ;k++)
				{
					if(shadowArrayPrev[j,k]==9)
					{
						findChildren(shadowArrayNext,j,k,nodeTable,parentTable);
						//placeAgent(shadowArrayNext,j,k);
					}
				}
			}
			//completed Hashtables
			fillNextLevel(shadowArrayNext,nodeTable,parentTable,i);
		}
		//shadowArrayNext = (sbyte[,])h_discreteShadows [pathPoints [2]];
		//shadowArrayPrev = (sbyte[,])h_discreteShadows [pathPoints [pathPoints.Count-1]];
		for(int i=pathPoints.Count-1;i>=0;i--)
		{
			shadowArrayPrev = (sbyte[,])h_discreteShadows [pathPoints [i]];
			Debug.Log("Agents at "+i+" = "+countAgents(shadowArrayPrev));
		}
		//Debug.Log("Agents at start = "+countAgents(shadowArrayPrev));
		//Debug.Log("Agents surviving at the end = "+countAgents(shadowArrayNext));
		displaySurvivingAgentsNew ();
	}
	private void fillNextLevel(sbyte[,] shadowArrayNext,Hashtable nodeTable,Hashtable parentTable,int level)
	{
		int fillingCurrentCount = 1;
		//int numRuns = 1000;
		while(parentTable.Keys.Count>0)// && numRuns-->0)
		{
			bool bStartAgain = false;
			int leastFilledCount = -1;
			foreach(Vector2 key in parentTable.Keys)
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
				if(NotFilledCount>0)
				{
					if(leastFilledCount>0)
					{
						if(NotFilledCount<leastFilledCount)
						{
							leastFilledCount = NotFilledCount;
						}
					}
					else
					{
						leastFilledCount = NotFilledCount;
					}
				}
			}
			if(leastFilledCount<0)
			{
				Debug.Log("level = "+level);
				Debug.Log("leastFilledCount = "+leastFilledCount);
				break;
			}
			//Debug.Log("level = "+level);
			//Debug.Log("leastFilledCount = "+leastFilledCount);
			//found leastFilledCount;
			foreach(Vector2 key in parentTable.Keys)
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
			}
			Debug.Log("parentTable.Keys.Count = "+parentTable.Keys.Count);
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
	private void findChildren(sbyte[,] shadowArrayNext,int j,int k, Hashtable nodeTable,Hashtable parentTable)
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
		sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPoints [0]];
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
			shadowArrayPrev = (sbyte[,])h_discreteShadows [pathPoints [i]];
			shadowArrayNext = (sbyte[,])h_discreteShadows [pathPoints [i+1]];
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
		shadowArrayPrev = (sbyte[,])h_discreteShadows [pathPoints [0]];
		shadowArrayNext = (sbyte[,])h_discreteShadows [pathPoints [pathPoints.Count-1]];
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
		sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPoints [0]];
		for(int j=0;j<discretePtsX;j++)
		{
			for(int k=0;k<discretePtsZ;k++)
			{
				if(shadowArray[j,k]==9)
				{
					GameObject clone1 = (GameObject)Instantiate(hiddenSphere);
					clone1.transform.position = (Vector3)h_mapIndxToPt[new Vector2(j,k)];
					//hiddenSphereList.Add(clone1);
				}
			}
		}
	}
	private void displaySurvivingAgents()
	{
		sbyte[,] shadowArray = (sbyte[,])h_discreteShadows [pathPoints [pathPoints.Count-1]];
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