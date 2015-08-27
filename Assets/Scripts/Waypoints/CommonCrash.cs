using UnityEngine;
using System.Collections.Generic;
public static class CommonCrash
{
	static float scale = 0.6f;
	static float stepPath = 1.0f*scale;
	static float stepPathActual = stepPath*1.5f;
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
		float step = 8 * stepPath;
		step = step * step;
		float xVar = 0.1f;
		float zVar = Mathf.Sqrt(step - xVar * xVar);
		Vector3 pt1 = new Vector3 (startPt.x+xVar, startPt.y, startPt.z+zVar);

		
		step = 40 * stepPath;
		step = step * step;
		xVar = 24.1f;
		zVar = Mathf.Sqrt(step - xVar * xVar);
		zVar = 1.0f;
		Vector3 pt2 = new Vector3 (pt1.x+xVar, pt1.y, pt1.z-zVar);

		
		step = 28 * stepPath;
		step = step * step;
		xVar = 5.6f;
		zVar = Mathf.Sqrt(step - xVar * xVar);
		Vector3 pt3 = new Vector3 (pt2.x+xVar, pt2.y, pt2.z+zVar);

		step = 22 * stepPath;
		step = step * step;
		xVar = 6.7f;
		zVar = Mathf.Sqrt(step - xVar * xVar);
		Vector3 pt4 = new Vector3 (pt3.x+xVar, pt3.y, pt3.z+zVar);

		//step = 65 * stepPath;
		//Vector3 pt4 = new Vector3 (pt3.x+step, pt3.y, pt3.z);


		step = 19 * stepPath;
		step = step * step;
		xVar = -2.5f;
		zVar = Mathf.Sqrt(step - xVar * xVar);
		Vector3 pt5 = new Vector3 (pt4.x+xVar, pt4.y, pt4.z+zVar);
		//step = 19 * stepPath;
		//Vector3 pt5 = new Vector3 (pt4.x, pt4.y, pt4.z+step);


		step = 8 * stepPath;
		step = step * step;
		xVar = 5.5f;
		zVar = -1.5f;//Mathf.Sqrt(step - xVar * xVar);
		Vector3 pt6 = new Vector3 (pt5.x+xVar, pt5.y, pt5.z+zVar);

		step = 24 * stepPath;
		step = step * step;
		xVar = 5.2f;
		zVar = Mathf.Sqrt(step - xVar * xVar);
		Vector3 pt7 = new Vector3 (pt6.x+xVar, pt6.y, pt6.z-zVar);


		step = 46 * stepPath;
		Vector3 pt8 = new Vector3 (pt7.x+step, pt7.y, pt7.z);

		step = 8 * stepPath;
		Vector3 pt9 = new Vector3 (pt8.x, pt8.y, pt8.z+step);

		step = 28 * stepPath;
		Vector3 pt10 = new Vector3 (pt9.x+step, pt9.y, pt9.z);
		//Vector3 pt10 = new Vector3 (pt8.x, pt8.y, pt8.z);

		step = 14 * stepPath;
		Vector3 pt11 = new Vector3 (pt10.x, pt10.y, pt10.z-step);

		Vector3 pt12 = new Vector3 (pt10.x, pt10.y, pt10.z);

		Vector3 pt13 = new Vector3 (pt9.x, pt9.y, pt9.z);

		step = 28 * stepPath;
		Vector3 pt14 = new Vector3 (pt13.x, pt13.y, pt13.z+step);

		step = 22 * stepPath;
		Vector3 pt15 = new Vector3 (pt14.x-step, pt14.y, pt14.z);

		Vector3 pt16 = new Vector3 (pt14.x, pt14.y, pt14.z);

		step = 16 * stepPath;
		step = step * step;
		xVar = 6.0f;
		zVar = Mathf.Sqrt(step - xVar * xVar);
		Vector3 pt17 = new Vector3 (pt16.x+xVar, pt16.y, pt16.z-zVar);

		step = 34 * stepPath;
		step = step * step;
		xVar = 12.0f;
		zVar = 6.1f;//Mathf.Sqrt(step - xVar * xVar);
		Vector3 pt18 = new Vector3 (pt17.x+xVar, pt17.y, pt17.z+zVar);


		xVar = 9.0f;
		zVar = 5.0f;
		Vector3 pt19 = new Vector3 (pt18.x+xVar, pt18.y, pt18.z-zVar);

		xVar = 1.5f;
		zVar = 28.0f;
		Vector3 pt20 = new Vector3 (pt19.x+xVar, pt19.y, pt19.z-zVar);

		wayPoints.Add (startPt);//0
		wayPoints.Add (pt1);
		wayPoints.Add (pt2);
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
		wayPoints.Add (pt15);
		wayPoints.Add (pt16);
		wayPoints.Add (pt17);
		wayPoints.Add (pt18);
		wayPoints.Add (pt19);
		wayPoints.Add (pt20);
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
		path1.Add (wayPoints [14]);
		path1.Add (wayPoints [15]);
		path1.Add (wayPoints [16]);
		path1.Add (wayPoints [17]);
		path1.Add (wayPoints [18]);
		path1.Add (wayPoints [19]);
		path1.Add (wayPoints [20]);
		path1.Add (wayPoints [21]);
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

