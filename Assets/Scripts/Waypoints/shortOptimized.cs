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
	private Hashtable createRelationshipMap2()
	{
		Hashtable relationMap = new Hashtable ();
		foreach(Vector3 pt in h_mapPtToIndx.Keys)
		{
			Vector2 indx = (Vector2)h_mapPtToIndx[pt];
			findNeighbors2(pt,indx,(int)indx.x,(int)indx.y,relationMap);
		}
		return relationMap;		
		
	}
	private void findNeighbors2(Vector3 pt,Vector2 ptVect2,int j1,int k1,Hashtable relationMap)
	{
		int rowJ = j1;
		int colK = k1;
		Vector3 currPos = pt;//((Vector3)h_mapIndxToPt[keyTemp]);
		List<Vector2> listOfAvailablePos = new List<Vector2> ();
		
		listOfAvailablePos.Add(ptVect2);
		
		
		bool runAgain = true;
		Vector3 vectPos = new Vector3 ();
		while(runAgain)
		{
			runAgain = false;
			rowJ--;
			colK--;
			int rowLen = (j1 - rowJ)*2 +1;
			//if(rowJ<0 || colK<0 || rowJ+rowLen>discretePtsX || colK+rowLen>discretePtsZ)
			//	break;
			for(int i1=rowJ;i1<rowJ+rowLen;i1++)
			{
				Vector2 vect2Pos = new Vector2(i1,colK);
				if(h_mapIndxToPt.ContainsKey(vect2Pos))
				{
					
					vectPos = (Vector3)h_mapIndxToPt[vect2Pos];
					
					if(Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos) && !CheckIfInsidePolygon(vectPos))
					{
						runAgain = true;
						listOfAvailablePos.Add(vect2Pos);
						
					}
				}
				vect2Pos = new Vector2(i1,colK+rowLen-1);
				if(h_mapIndxToPt.ContainsKey(vect2Pos))
				{
					vectPos = (Vector3)h_mapIndxToPt[vect2Pos];
					
					if(Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos) && !CheckIfInsidePolygon(vectPos))
					{
						runAgain = true;
						listOfAvailablePos.Add(vect2Pos);
						
					}
				}
			}
			for(int i2=colK+1;i2<colK+rowLen-1;i2++)
			{
				Vector2 vect2Pos = new Vector2(rowJ,i2);
				if(h_mapIndxToPt.ContainsKey(vect2Pos))
				{
					vectPos = (Vector3)h_mapIndxToPt[vect2Pos];
					if(Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos) && !CheckIfInsidePolygon(vectPos))
					{
						runAgain = true;
						listOfAvailablePos.Add(vect2Pos);
						
					}
				}
				vect2Pos = new Vector2(rowJ+rowLen-1,i2);
				if(h_mapIndxToPt.ContainsKey(vect2Pos))
				{
					vectPos = (Vector3)h_mapIndxToPt[vect2Pos];
					if(Vector3.Distance(currPos,vectPos)<=standardMaxMovement && CheckStraightLineVisibility(currPos,vectPos) && !CheckIfInsidePolygon(vectPos))
					{
						runAgain = true;
						listOfAvailablePos.Add(vect2Pos);
						
					}
				}
			}
		}
		relationMap.Add(ptVect2,listOfAvailablePos);
		//return relationMap;
	}
	//Updates HT3
	private void spreadEssence(Vector2 pt,int Indx,List<int> listHT2,Hashtable relationMapVect2)
	{
		int j = (int)pt.x;
		int k = (int)pt.y;
		int rowJ = j;
		int colK = k;
		sbyte[,] shadowArrayHT3 = (sbyte[,])h_discreteShadows6[Indx];

		Vector3 currPos = ((Vector3)h_mapIndxToPt[new Vector2(j,k)]);
		
		
		bool runAgain = true;
		
		/*if(shadowArrayHT3[j,k]>0)
		{
			List<int> listHT3 = (List<int>)HT3[pt];
			foreach(int numID_HT2 in listHT2)
			{
				if(!listHT3.Contains(numID_HT2))
				{
					listHT3.Add(numID_HT2);
				}
			}
		}*/
		List<Vector2> listNeighbors = (List<Vector2>)relationMapVect2[new Vector2(j,k)];
		foreach(Vector2 vect2 in listNeighbors)
		{
			int jTemp = (int)vect2.x;
			int kTemp = (int)vect2.y;
			if(shadowArrayHT3[jTemp,kTemp]>0)
			{
				List<int> listHT3 = (List<int>)HT3[vect2];
				foreach(int numID_HT2 in listHT2)
				{
					if(!listHT3.Contains(numID_HT2))
					{
						listHT3.Add(numID_HT2);
					}
				}
			}
		}
		return;
		/*while(runAgain)
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
		}*/
	}
	private void executeTrueCase6()
	{
		float startTimeCalc = Time.realtimeSinceStartup;
		setGlobalVars1();
		createDiscreteMap6 ();
		
		standardMaxMovement = speedEnemy*(m_stepDistance/speedPlayer);
		//Hashtable relationMap = createRelationships ();
		Hashtable relationMapVect2 = createRelationshipMap2 ();
		int j1=0;
		int k1=0;
		int lastIndx = pathPoints.Count;
		for(int Indx=1;Indx<lastIndx;Indx++)
		{
			foreach(Vector2 pt in HT2.Keys)
			{
				if(pointInShadowDiscrete(pt,Indx))
				{
					spreadEssence(pt,Indx,(List<int>)HT2[pt],relationMapVect2);

				}
				else
				{
					List<int> listHT3 = (List<int>)HT3[pt];
					listHT3.RemoveRange(0,listHT3.Count);
				}
			}
			//float totalTimeCalcSpreadEssence = (Time.realtimeSinceStartup - startTimeCalc)/60;
			//Debug.Log("totalTimeCalcSpreadEssence = "+totalTimeCalcSpreadEssence);
			//float startTimeCalcDeepCopy = Time.realtimeSinceStartup;
			HT2 = DeepCopy(HT3);
			//float totalTimeCalcDeepCopy = (Time.realtimeSinceStartup - startTimeCalcDeepCopy)/60;
			//Debug.Log("totalTimeCalcDeepCopy = "+totalTimeCalcDeepCopy);
			//float startTimeCalcAddToHT4 = Time.realtimeSinceStartup;
			AddToHT4(Indx);
			//float totalTimeCalcAddToHT4 = (Time.realtimeSinceStartup - startTimeCalcAddToHT4)/60;
			//Debug.Log("totalTimeCalcAddToHT4 = "+totalTimeCalcAddToHT4);
			/*if(Indx==3)
			{
				break;
			}*/
		}
		float totalTimeCalcHT4 = (Time.realtimeSinceStartup - startTimeCalc)/60;
		Debug.Log ("totalTimeCalcHT4 = " + totalTimeCalcHT4);
		//return;
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
		Debug.Log ("totalTimeCalc = " + totalTimeCalc);
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