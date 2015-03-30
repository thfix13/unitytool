using System;
using UnityEngine;
using System.Collections.Generic;

public class StandardPolygon
{
	private List<Vector3> points;
	public StandardPolygon ()
	{
		points = new List<Vector3>();
	}
	public bool addPoint(Vector3 pt)
	{
		/*if(!points.Contains(pt))
		{
			points.Add(pt);
			showPosOfPoint(pt,"point "+points.Count);
			return true;
		}
		return false;*/
		//int indx = points.IndexOf (pt);
		//Vector3 pt1 = points [indx];
		//Debug.Log ("Already has point " + pt.x + " , " + pt.y + " , " + pt.z);
		//Debug.Log ("The point in list is " + pt1.x + " , " + pt1.y + " , " + pt1.z);

		foreach(Vector3 pt1 in points)
		{
			if(comparePoints(pt1,pt))
			{
				return false;
			}
		}
		points.Add(pt);
		//showPosOfPoint(pt,"point "+points.Count);
		return true;
	}
	public void removePoint(int index)
	{
		points.RemoveAt(index);
	}
	public List<Vector3> getVertices()
	{
		return points;
	}
	public StandardPolygon makeSubPolygon(Vector3 duplicatePt)
	{
		int indx = points.IndexOf (duplicatePt);
		if (indx < 0 || points.Count-indx<3)
			return null;
		StandardPolygon poly = new StandardPolygon ();
		for(int i=indx;i<points.Count;i++)
		{
			poly.addPoint(points [i]);
		}
		points.RemoveRange(indx,points.Count-indx);
		addPoint(duplicatePt);
		return poly;
	}
	public void removeDuplicates()
	{
		List<Vector3> copyPts = new List<Vector3> ();
		copyPts.AddRange (getVertices ());
		for(int i=0;i<copyPts.Count;i++)
		{
			for(int j=0;j<copyPts.Count;j++)
			{
				if(i==j)
					continue;
				if(comparePoints(copyPts[i],copyPts[j]))
				{
					Debug.Log("*********** removed duplicate ******************");
					removePoint(j);
					copyPts.RemoveAt(j);
					j--;
				}
			}
		}
	}
	public int findIndexOfDuplicate(Vector3 pt)
	{
		for(int i=0;i<points.Count;i++)
		{
			if(comparePoints(points[i],pt))
			{
				return i;
			}
		}
		return -1;
	}
	public bool comparePoints(Vector3 v1,Vector3 v2)
	{
		float limit = 0.01f;
		float minVal = Vector3.Distance(v1,v2);
		if (minVal <= limit)
			return true;
		return false;
	}
	void showPosOfPoint(Vector3 pos,String name1)
	{
		GameObject sp = (GameObject)GameObject.Find ("StartPoint");
		GameObject tempObj = (GameObject)GameObject.Instantiate (sp);
		tempObj.name = name1;
		tempObj.transform.position=pos;
	}
}


