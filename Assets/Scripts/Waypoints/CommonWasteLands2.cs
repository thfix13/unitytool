using UnityEngine;
using System.Collections.Generic;
public static class CommonWasteLands2
{
	static float scale = 1.5f;
	static float stepPath = 0.2f*scale;
	
	//static float scale = 0.80f;
	//static float stepPath = 1.0f*scale;
	static float stepPathActual = stepPath*1.0f;
	public static List<Vector3> definePathFromIndx(int pathIndx)
	{
		//List<Vector3> pathPts = new List<Vector3> ();
		GameObject sp = (GameObject)GameObject.Find ("StartPoint");
		GameObject ep = (GameObject)GameObject.Find ("EndPoint");
		//pathPts.Add(sp.transform.position);
		
		
		List<Vector3> wayPoints = createWayPoints (sp.transform.position,ep.transform.position);
		
		List<Vector3> pathPts = new List<Vector3>();
		if(pathIndx==1)
		{
			pathPts = selectPath1(wayPoints);
		}
		else if(pathIndx==2)
		{
			//pathPts = selectPath2(wayPoints);
		}
		createPathPoints (pathPts);
		return pathPts;
	}
	public static List<Vector3> definePath()
	{
		
		GameObject sp = (GameObject)GameObject.Find ("StartPoint");
		GameObject ep = (GameObject)GameObject.Find ("EndPoint");
		
		
		
		List<Vector3> wayPoints = createWayPoints (sp.transform.position,ep.transform.position);
		
		List<Vector3>  pathPts = selectPath1(wayPoints);
		createPathPoints (pathPts);
		
		///////////////////////////////
		//pathPts.Clear ();
		//pathPts.Add(sp.transform.position);
		//pathPts.Add(ep.transform.position);
		////////////////////////////////
		return pathPts;
	}
	private static void createPathPoints (List<Vector3> pathPts)
	{
		int count = pathPts.Count;
		int i = 0;
		while(true)
		{
			if(i>=pathPts.Count-1)
				break;
			Vector3 pos = Vector3.MoveTowards(pathPts[i],pathPts[i+1],stepPathActual);
			if(pos==pathPts[i+1])
			{
				i++;
				continue;
			}
			pathPts.Insert(i+1, pos);
			i++;
		}
	}
	private static List<Vector3> createWayPoints (Vector3 startPt,Vector3 endPt)
	{
		List<Vector3> wayPoints = new List<Vector3> ();
		float step = 1 * stepPath;
		step = step * step;
		float xVar = -1.0f;
		float zVar = -1.0f;
		//Vector3 pt1 = new Vector3 (startPt.x+xVar, startPt.y, startPt.z+zVar);
		
		int n = 21;
		step = n * stepPath;
		step = step * step;
		zVar = 1.2f;
		xVar = Mathf.Sqrt(Mathf.Abs(step - zVar * zVar));
		//Vector3 pt2 = new Vector3 (startPt.x+xVar, startPt.y, startPt.z+zVar);

		
		
		step = 9 * stepPath;
		xVar = step;
		Vector3 pt3 = new Vector3 (startPt.x-xVar, startPt.y, startPt.z);
		
		step = 13 * stepPath;
		step = step * step;
		xVar = 2.7f;
		zVar = Mathf.Sqrt(Mathf.Abs (step - xVar * xVar));
		Vector3 pt4 = new Vector3 (pt3.x-xVar, pt3.y, pt3.z+zVar);

		
		step = 7 * stepPath;
		xVar = step;
		Vector3 pt5 = new Vector3 (pt4.x-xVar, pt4.y, pt4.z);

		
		step = 12 * stepPath;
		zVar = step;
		Vector3 pt6 = new Vector3 (pt5.x, pt5.y, pt5.z+zVar);

		//Vector3 pt7 = new Vector3 (pt5.x, pt5.y, pt5.z);
		step = 12 * stepPath;
		step = step * step;
		xVar = 0.5f;
		zVar = Mathf.Sqrt(Mathf.Abs (step - xVar * xVar));
		Vector3 pt7 = new Vector3 (pt6.x-xVar, pt6.y, pt6.z-zVar);
		
		
		
		step = 14 * stepPath;
		step = step * step;
		xVar = 3.5f;
		zVar = Mathf.Sqrt(Mathf.Abs(step - xVar * xVar));
		Vector3 pt8 = new Vector3 (pt7.x-xVar, pt7.y, pt7.z-zVar);

		
		step = 8 * stepPath;
		xVar = step;
		Vector3 pt9 = new Vector3 (pt8.x-xVar, pt8.y, pt8.z);
		
		step = 28 * stepPath;
		zVar = step;
		Vector3 pt10 = new Vector3 (pt9.x, pt9.y, pt9.z+zVar);
		
		step = 26 * stepPath;
		xVar = step;
		Vector3 pt11 = new Vector3 (pt10.x-xVar, pt10.y, pt10.z);
		
		step = 14 * stepPath;
		zVar = step;
		Vector3 pt12 = new Vector3 (pt11.x, pt11.y, pt11.z-zVar);
		
		step = 8 * stepPath;
		xVar = step;
		Vector3 pt13 = new Vector3 (pt12.x-xVar, pt12.y, pt12.z);

		step = 9 * stepPath;
		step = step * step;
		xVar = 2.2f;
		zVar = Mathf.Sqrt(Mathf.Abs(step - xVar * xVar));
		Vector3 pt14 = new Vector3 (pt13.x-xVar, pt13.y, pt13.z-zVar);










		
		wayPoints.Add (startPt);//0
		//wayPoints.Add (pt2);
		wayPoints.Add (pt3);
		wayPoints.Add (pt4);
		wayPoints.Add (pt5);
		wayPoints.Add (pt6);
		wayPoints.Add (pt7);
		wayPoints.Add (pt8);
		wayPoints.Add (pt9);
		wayPoints.Add (pt10);
		wayPoints.Add (pt11);
		wayPoints.Add (pt12);
		wayPoints.Add (pt13);
		wayPoints.Add (pt14);
		wayPoints.Add (endPt);//16
		return wayPoints;
	}
	private static List<Vector3> selectPath1 (List<Vector3> wayPoints)
	{
		List<Vector3> path1 = new List<Vector3> ();
		path1.Add (wayPoints [0]);
		path1.Add (wayPoints [1]);
		path1.Add (wayPoints [2]);
		path1.Add (wayPoints [3]);
		path1.Add (wayPoints [4]);
		path1.Add (wayPoints [5]);
		path1.Add (wayPoints [6]);
		path1.Add (wayPoints [7]);
		path1.Add (wayPoints [8]);
		path1.Add (wayPoints [9]);
		path1.Add (wayPoints [10]);
		path1.Add (wayPoints [11]);
		path1.Add (wayPoints [12]);
		path1.Add (wayPoints [13]);
		//path1.Add (wayPoints [14]);
		//path1.Add (wayPoints [15]);
		return path1;
	}
	//Distance b/w consecutive points
	public static float getStepDistance()
	{
		if (stepPathActual < 0.0f)
			Debug.LogError ("Distance b/w consecutive points not set.");
		return stepPathActual;
	}
	private static void showPosOfPoint(Vector3 pos,Color c)
	{
		return;
		if (float.IsNaN (pos.x) || float.IsNaN (pos.z))
			return;
		GameObject sp = (GameObject)GameObject.Find ("StartPoint");
		GameObject tempObj = (GameObject)GameObject.Instantiate (sp);
		Renderer rend = tempObj.GetComponent<Renderer>();
		rend.material.color = c;
		tempObj.transform.position=pos;
	}
	
}

