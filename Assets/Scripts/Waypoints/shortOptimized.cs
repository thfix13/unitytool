using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
public partial class Visibility1 : MonoBehaviour 
{
	Hashtable h_discreteShadows6 = new Hashtable();
	Hashtable HT1 = new Hashtable();
	Hashtable HT2 = new Hashtable();
	Hashtable HT3;// = new Hashtable();
	Hashtable HT4 = new Hashtable();
	private Hashtable DeepCopy(Hashtable ht)//Only for HT2/HT3
	{
		Hashtable HT2Copy = new Hashtable ();
		foreach(Vector2 vect in ht.Keys)
		{
			List<int> listInt = (List<int>)ht[vect];
			List<int> listInt2 = new List<int>();
			listInt2.AddRange(listInt);

			HT2Copy.Add(vect,listInt2);
		}
		return HT2Copy;
	}
	private void createDiscreteMap6()
	{
		int numID = 1;
		int Indx = 0;
		while(Indx<pathPoints.Count)
		{
			sbyte[,] shadowArray = new sbyte[discretePtsX,discretePtsZ];
			
			float radius_hiddenSphere = radius_enemy;
			int j1=0;	
			for(float j=m_minX;j<m_maxX && j1<discretePtsX;j+=m_step)
			{
				int k1=0;
				for(float k=m_minZ;k<m_maxZ && k1<discretePtsZ;k+=m_step)
				{
					Vector3 pt = new Vector3(j,1,k);
					if(Indx==0)
					{
						HT2.Add(new Vector2(j1,k1),new List<int>());
					}
					if(pointInShadow(pt,Indx) && !Physics.CheckSphere(pt,radius_hiddenSphere))
					{
						shadowArray[j1,k1]=1;
						if(Indx==0)
						{
							Vector2 tmpVect2 = new Vector2(j1,k1);
							//HT1.Add(tmpVect2,numID);
							HT1.Add(numID,tmpVect2);
							((List<int>)HT2[tmpVect2]).Add(numID);
							numID++;
						}
					}
					else
					{
						shadowArray[j1,k1]=-1;
					}

					k1++;
				}
				j1++;
			}
			h_discreteShadows6.Add(Indx,shadowArray);
			Indx++;
		}
		HT3 = DeepCopy (HT2);
	}
	private void setNeighborValues6(sbyte[,] shadowArrayPrev,sbyte[,] shadowArrayNext,int i,int j,int k,Hashtable relationMap)
	{
		int rowJ = j;
		int colK = k;
		Vector3 currPos = ((Vector3)h_mapIndxToPt[new Vector2(j,k)]);
		//List<Vector2> listOfAvailablePos = new List<Vector2> ();
		
		
		bool runAgain = true;
		
		if(shadowArrayNext[j,k]>0 && shadowArrayPrev[j,k]<shadowArrayNext[j,k])
		{
			shadowArrayPrev[j,k] = shadowArrayNext[j,k];
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
				if(shadowArrayPrev[i1,colK]>=0 && shadowArrayPrev[i1,colK]<shadowArrayNext[j,k] && CheckIfNeighbor(currPos,vectPos,relationMap))
				{
					runAgain = true;
					shadowArrayPrev[i1,colK] = shadowArrayNext[j,k];
					//listOfAvailablePos.Add(new Vector2(i1,colK));
				}
				vectPos = (Vector3)h_mapIndxToPt[new Vector2(i1,colK+rowLen-1)];
				if(shadowArrayPrev[i1,colK+rowLen-1]>=0 && shadowArrayPrev[i1,colK+rowLen-1]<shadowArrayNext[j,k] && CheckIfNeighbor(currPos,vectPos,relationMap))
				{
					runAgain = true;
					shadowArrayPrev[i1,colK+rowLen-1] = shadowArrayNext[j,k];
					//listOfAvailablePos.Add(new Vector2(i1,colK+rowLen-1));
				}
			}
			for(int i2=colK+1;i2<colK+rowLen-1;i2++)
			{
				Vector3 vectPos = (Vector3)h_mapIndxToPt[new Vector2(rowJ,i2)];
				if(shadowArrayPrev[rowJ,i2]>=0 && shadowArrayPrev[rowJ,i2]<shadowArrayNext[j,k] && CheckIfNeighbor(currPos,vectPos,relationMap))
				{
					runAgain = true;
					shadowArrayPrev[rowJ,i2] = shadowArrayNext[j,k];
					//listOfAvailablePos.Add(new Vector2(rowJ,i2));
				}
				vectPos = (Vector3)h_mapIndxToPt[new Vector2(rowJ+rowLen-1,i2)];
				if(shadowArrayPrev[rowJ+rowLen-1,i2]>=0 && shadowArrayPrev[rowJ+rowLen-1,i2]<shadowArrayNext[j,k] && CheckIfNeighbor(currPos,vectPos,relationMap))
				{
					runAgain = true;
					shadowArrayPrev[rowJ+rowLen-1,i2] = shadowArrayNext[j,k];
					//listOfAvailablePos.Add(new Vector2(rowJ+rowLen-1,i2));
				}
			}
		}
		/*List<AgentNode> nodelist = new List<AgentNode> ();
		foreach(Vector2 vect in listOfAvailablePos)
		{
			if(!nodeTable.ContainsKey(vect))
			{
				nodeTable.Add(vect,new AgentNode(vect));
			}
			nodelist.Add ((AgentNode)nodeTable[vect]);
		}
		parentTable.Add (new Vector2 (j, k), nodelist);*/
	}
	private bool pointInShadowDiscrete(Vector2 pt, int Indx)
	{
		int j = (int)pt.x;
		int k = (int)pt.y;
		sbyte[,] shadowArray = (sbyte[,])h_discreteShadows6[Indx];
		if(shadowArray[j,k]>0)
		{
			return true;
		}
		return false;
	}
	private void AddToHT4(int Indx)
	{
		List<int> listHT4 = new List<int> ();
		foreach(Vector2 vect in HT2.Keys)
		{
			List<int> listHT2 = (List<int>)HT2[vect];
			foreach(int i in listHT2)
			{
				if(!listHT4.Contains(i))
				{
					listHT4.Add(i);
				}
			}
		}
		HT4.Add (Indx, listHT4);
	}
	//Updates HT3
	private void spreadEssence(Vector2 pt,int Indx,List<int> listHT2,Hashtable relationMap)
	{
		int j = (int)pt.x;
		int k = (int)pt.y;
		int rowJ = j;
		int colK = k;
		sbyte[,] shadowArrayHT3 = (sbyte[,])h_discreteShadows6[Indx];

		Vector3 currPos = ((Vector3)h_mapIndxToPt[new Vector2(j,k)]);
		
		
		bool runAgain = true;
		
		if(shadowArrayHT3[j,k]>0)
		{
			List<int> listHT3 = (List<int>)HT3[pt];
			foreach(int numID_HT2 in listHT2)
			{
				if(!listHT3.Contains(numID_HT2))
				{
					listHT3.Add(numID_HT2);
				}
			}
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
				if(shadowArrayHT3[i1,colK]>0 && CheckIfNeighbor(currPos,vectPos,relationMap))
				{
					runAgain = true;
					List<int> listHT3 = (List<int>)HT3[new Vector2(i1,colK)];
					foreach(int numID_HT2 in listHT2)
					{
						if(!listHT3.Contains(numID_HT2))
						{
							listHT3.Add(numID_HT2);
						}
					}
				}
				vectPos = (Vector3)h_mapIndxToPt[new Vector2(i1,colK+rowLen-1)];
				if(shadowArrayHT3[i1,colK+rowLen-1]>0 && CheckIfNeighbor(currPos,vectPos,relationMap))
				{
					runAgain = true;
					List<int> listHT3 = (List<int>)HT3[new Vector2(i1,colK+rowLen-1)];
					foreach(int numID_HT2 in listHT2)
					{
						if(!listHT3.Contains(numID_HT2))
						{
							listHT3.Add(numID_HT2);
						}
					}
				}
			}
			for(int i2=colK+1;i2<colK+rowLen-1;i2++)
			{
				Vector3 vectPos = (Vector3)h_mapIndxToPt[new Vector2(rowJ,i2)];
				if(shadowArrayHT3[rowJ,i2]>0 && CheckIfNeighbor(currPos,vectPos,relationMap))
				{
					runAgain = true;
					List<int> listHT3 = (List<int>)HT3[new Vector2(rowJ,i2)];
					foreach(int numID_HT2 in listHT2)
					{
						if(!listHT3.Contains(numID_HT2))
						{
							listHT3.Add(numID_HT2);
						}
					}
				}
				vectPos = (Vector3)h_mapIndxToPt[new Vector2(rowJ+rowLen-1,i2)];
				if(shadowArrayHT3[rowJ+rowLen-1,i2]>0 && CheckIfNeighbor(currPos,vectPos,relationMap))
				{
					runAgain = true;
					List<int> listHT3 = (List<int>)HT3[new Vector2(rowJ+rowLen-1,i2)];
					foreach(int numID_HT2 in listHT2)
					{
						if(!listHT3.Contains(numID_HT2))
						{
							listHT3.Add(numID_HT2);
						}
					}
				}
			}
		}
	}
	private void executeTrueCase6()
	{
		float startTimeCalc = Time.realtimeSinceStartup;
		setGlobalVars1();
		createDiscreteMap6 ();
		
		standardMaxMovement = speedEnemy*(m_stepDistance/speedPlayer);
		Hashtable relationMap = createRelationships ();
		int j1=0;
		int k1=0;
		int lastIndx = pathPoints.Count;
		for(int Indx=1;Indx<lastIndx;Indx++)
		{
			foreach(Vector2 pt in HT2.Keys)
			{
				if(pointInShadowDiscrete(pt,Indx))
				{
					spreadEssence(pt,Indx,(List<int>)HT2[pt],relationMap);
				}
				else
				{
					List<int> listHT3 = (List<int>)HT3[pt];
					listHT3.RemoveRange(0,listHT3.Count);
				}
			}
			HT2 = DeepCopy(HT3);
			AddToHT4(Indx);
		}

		//TODO
		int IndxPtr = lastIndx-1;
		Hashtable HT5 = new Hashtable ();
		while(IndxPtr>=0)
		{
			List<int> listHT4 = (List<int>)HT4[IndxPtr];
			foreach(int numID in listHT4)
			{
				Vector2 valVect2 = (Vector2)HT1[numID];
				Vector3 currPos = ((Vector3)h_mapIndxToPt[valVect2]);
				if(!HT5.ContainsKey(currPos))
				{
					HT5.Add(currPos,IndxPtr);
				}
			}
			IndxPtr--;
		}
		
		float totalTimeCalc = (Time.realtimeSinceStartup - startTimeCalc)/60;
		
		//Dumping file
		string dirName = createSaveDataDir(Application.dataPath);
		string resultFileName = dirName+"\\Result.txt";
		StreamWriter sw = new StreamWriter (resultFileName);
		
		foreach(Vector3 pt in HT5.Keys)
		{
			sw.Write("("+pt.x+","+pt.y+","+pt.z+")"+";"+(int)HT5[pt]);
			sw.WriteLine("");
		}

		sw.Close ();
		
		DumpInfoFile (dirName,totalTimeCalc);
		
	}
}