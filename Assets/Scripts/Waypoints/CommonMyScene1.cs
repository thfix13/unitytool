//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;
public static class CommonMyScene1
{
	static float stepPath = 0.16f;
	public static List<Vector3> definePath()
	{
		//List<Vector3> pathPts = new List<Vector3> ();
		GameObject sp = (GameObject)GameObject.Find ("StartPoint");
		GameObject ep = (GameObject)GameObject.Find ("EndPoint");
		//pathPts.Add(sp.transform.position);
		
		
		List<Vector3> wayPoints = createWayPoints (sp.transform.position,ep.transform.position);
		
		List<Vector3>  pathPts = selectPath3(wayPoints);
		createPathPoints (pathPts);
		
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
			Vector3 pos = Vector3.MoveTowards(pathPts[i],pathPts[i+1],stepPath);
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

		float step = 14 * stepPath;
		Vector3 pt1 = new Vector3 (startPt.x, startPt.y, startPt.z+step);
		//showPosOfPoint (pt1, Color.green);

		step = 15 * stepPath;
		step = step * step;
		float xVar = 2.1f;
		float zVar = Mathf.Sqrt(step - xVar * xVar);
		Vector3 pt2 = new Vector3 (pt1.x+xVar, pt1.y, pt1.z+zVar);
		//showPosOfPoint (pt2, Color.green);

		step = 20 * stepPath;
		Vector3 pt3 = new Vector3 (pt2.x+step, pt2.y, pt2.z);
		//showPosOfPoint (pt3, Color.green);

		step = 22 * stepPath;
		step = step * step;
		xVar = 1.0f;
		zVar = Mathf.Sqrt(step - xVar * xVar);
		Vector3 pt4 = new Vector3 (pt3.x+xVar, pt3.y, pt3.z-zVar);
		//showPosOfPoint (pt4, Color.green);

		step = 40 * stepPath;
		Vector3 pt5 = new Vector3 (pt4.x+step, pt4.y, pt4.z);
		//showPosOfPoint (pt5, Color.green);


		//2nd path points
		step = 19 * stepPath;
		Vector3 pt6 = new Vector3 (startPt.x+step, startPt.y, startPt.z);
		//showPosOfPoint (pt6, Color.green);

		step = 50 * stepPath;
		step = step * step;
		xVar = 7.0f;
		zVar = Mathf.Sqrt(step - xVar * xVar);
		Vector3 pt7 = new Vector3 (pt6.x+xVar, pt6.y, pt6.z+zVar);
		//showPosOfPoint (pt7, Color.green);

		step = 19 * stepPath;
		Vector3 pt8 = new Vector3 (pt7.x+step, pt7.y, pt7.z);
		//showPosOfPoint (pt8, Color.green);

		//3rd path
		step = 54 * stepPath;
		step = step * step;
		xVar = 5.6f;
		zVar = Mathf.Sqrt(step - xVar * xVar);
		Vector3 pt9 = new Vector3 (pt6.x+xVar, pt6.y, pt6.z+zVar);
		//showPosOfPoint (pt9, Color.green);
		
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
		/*wayPoints.Add (pt10);
		wayPoints.Add (pt11);
		wayPoints.Add (pt12);
		wayPoints.Add (pt13);
		wayPoints.Add (pt14);
		wayPoints.Add (pt15);
		wayPoints.Add (pt16);
		wayPoints.Add (pt17);*/
		wayPoints.Add (endPt);//18
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
		path1.Add (wayPoints [10]);
		return path1;
	}
	private static List<Vector3> selectPath2 (List<Vector3> wayPoints)
	{
		List<Vector3> path1 = new List<Vector3> ();
		path1.Add (wayPoints [0]);
		path1.Add (wayPoints [6]);
		path1.Add (wayPoints [7]);
		path1.Add (wayPoints [8]);
		path1.Add (wayPoints [10]);
		return path1;
	}
	private static List<Vector3> selectPath3 (List<Vector3> wayPoints)
	{
		List<Vector3> path1 = new List<Vector3> ();
		path1.Add (wayPoints [0]);
		path1.Add (wayPoints [6]);
		path1.Add (wayPoints [9]);
		path1.Add (wayPoints [10]);
		return path1;
	}
	
	//Distance b/w consecutive points
	public static float getStepDistance()
	{
		if (stepPath < 0.0f)
			Debug.LogError ("Distance b/w consecutive points not set.");
		return stepPath;
	}
	private static void showPosOfPoint(Vector3 pos,Color c)
	{
		if (float.IsNaN (pos.x) || float.IsNaN (pos.z))
			return;
		GameObject sp = (GameObject)GameObject.Find ("StartPoint");
		GameObject tempObj = (GameObject)GameObject.Instantiate (sp);
		Renderer rend = tempObj.GetComponent<Renderer>();
		rend.material.color = c;
		tempObj.transform.position=pos;
	}
}

