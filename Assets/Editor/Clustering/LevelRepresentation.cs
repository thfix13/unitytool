using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

namespace ClusteringSpace
{	
	public class Line
	{
		public Vector2 start;
		public Vector2 end;
		public Line(Vector2 s_, Vector2 e_) { start = s_; end = e_; }
		public void add(double x_, double y_) {
			start.x += System.Convert.ToSingle(x_);
			start.y += System.Convert.ToSingle(y_);
			end.x += System.Convert.ToSingle(x_);
			end.y += System.Convert.ToSingle(y_);
		}
	}
	
	public class LevelRepresentation
	{
		public Vector2 startPos = new Vector2();
		public List<Line> obstacles = new List<Line>();
		public List<Line> smallerObstacles = new List<Line>();
				
		public const int OBSTACLE_LAYER = 8;
				
		public void loadPlatformerLevel(String path)
		{
			// load the level information (currently just floor pos + scale)
			XmlSerializer ser = new XmlSerializer (typeof(PlatformerLevelInfo));
			PlatformerLevelInfo loaded = null;
			using (FileStream stream = new FileStream (path + "/levelinfo.xml", FileMode.Open)) {
				loaded = (PlatformerLevelInfo)ser.Deserialize (stream);
				stream.Close ();
			}
			Vector2[] floorPositions = loaded.floorPositions;
			Vector2[] floorScales = loaded.floorScales;
			startPos = loaded.startPos;
			
			// destroy current floor
			GameObject levelObj = GameObject.Find("Level"); 
			for (int i = levelObj.transform.childCount - 1; i > -1; i--)
			{
			    GameObject.DestroyImmediate(levelObj.transform.GetChild(i).gameObject);
			}
			levelObj.transform.position = new Vector3(0, 0, 0);
			
			// add new floor!
			GameObject templateWall = GameObject.Find("TemplateWall");
			for (int count = 0; count < floorPositions.Length; count ++)
			{
				Transform wallTransform = GameObject.Instantiate(templateWall.transform) as Transform;
				GameObject wall = wallTransform.gameObject;
				wall.name = "Platform";
				wall.tag = "Platform";
				wall.transform.position = new Vector3((floorPositions[count].x), 0, floorPositions[count].y);
				wall.transform.localScale = new Vector3(floorScales[count].x, 3, floorScales[count].y);
				wall.transform.parent = levelObj.transform;
				wall.SetActive(true);
			}

			obstacles.Clear();
			smallerObstacles.Clear();
			for (int count = 0; count < floorPositions.Length; count ++)
			{
				List<Vector2> midpoints = new List<Vector2>();
				midpoints.Add(new Vector2(floorPositions[count].x, floorPositions[count].y));
				midpoints.Add(new Vector2(floorPositions[count].x + floorScales[count].x, floorPositions[count].y));
				midpoints.Add(new Vector2(floorPositions[count].x + floorScales[count].x, floorPositions[count].y - floorScales[count].y));
				midpoints.Add(new Vector2(floorPositions[count].x, floorPositions[count].y - floorScales[count].y));
				for (int count2 = 0; count2 < midpoints.Count; count2 ++)
				{
					obstacles.Add(new Line(midpoints[count2], midpoints[(count2+1)%midpoints.Count]));
				}
			}
			
			for (int count = 0; count < floorPositions.Length; count ++)
			{
				List<Vector2> midpoints = new List<Vector2>();
				midpoints.Add(new Vector2(floorPositions[count].x + (floorScales[count].x/2), floorPositions[count].y));
				midpoints.Add(new Vector2(floorPositions[count].x + floorScales[count].x, floorPositions[count].y - (floorScales[count].y/2)));
				midpoints.Add(new Vector2(floorPositions[count].x + (floorScales[count].x/2), floorPositions[count].y - floorScales[count].y));
				midpoints.Add(new Vector2(floorPositions[count].x, floorPositions[count].y - (floorScales[count].y/2)));
				for (int count2 = 0; count2 < midpoints.Count; count2 ++)
				{
			//		Debug.Log("Midpoint " + count2 +" : " + midpoints[count2]);
					smallerObstacles.Add(new Line(midpoints[count2], midpoints[(count2+1)%midpoints.Count]));
				}
			}
		}
		
		public void updateObstaclePos(double x_, double y_)
		{
			foreach (Line l in obstacles) {
				l.add(x_, y_);
			}
			foreach (Line l in smallerObstacles) {
				l.add(x_, y_);
			}
			GameObject[] objs = GameObject.FindGameObjectsWithTag ("Platform") as GameObject[];
			for (int i = 0; i < objs.Length; i++) {
				objs[i].transform.position = new Vector3(System.Convert.ToSingle(objs[i].transform.position.x+x_), 0, System.Convert.ToSingle(objs[i].transform.position.z+y_));
			}
		}

		public void loadPuzzleLevel(String path)
		{
			// load the level information (currently just floor pos + scale)
			XmlSerializer ser = new XmlSerializer (typeof(PuzzleLevelInfo));
			PuzzleLevelInfo loaded = null;
			using (FileStream stream = new FileStream (path + "/levelinfo.xml", FileMode.Open)) {
				loaded = (PuzzleLevelInfo)ser.Deserialize (stream);
				stream.Close ();
			}
			startPos = loaded.startPos;
			
			// destroy current floor
			GameObject levelObj = GameObject.Find("Level"); 
			for (int i = levelObj.transform.childCount - 1; i > -1; i--)
			{
			    GameObject.DestroyImmediate(levelObj.transform.GetChild(i).gameObject);
			}
			levelObj.transform.position = new Vector3(0, 0, 0);
		}
		
		public void loadStealthLevel()
		{
			// get start pos
			GameObject start = GameObject.Find("Start");
			startPos.x = start.transform.localPosition.x;
			startPos.y = start.transform.localPosition.z;
			
			List<Vector2> floorPositions = new List<Vector2>();
			List<Vector2> floorScales = new List<Vector2>();
			
			// get all obstacles
			// ref : http://answers.unity3d.com/questions/329395/how-to-get-all-gameobjects-in-scene.html
			//GameObject[] objs = UnityEngine.Object.FindObjectsOfType<GameObject>();
		    Transform[] objs = GameObject.FindObjectsOfType(typeof(Transform)) as Transform[];
			
			foreach (Transform obj in objs)
			{
		        if (obj.gameObject.layer == OBSTACLE_LAYER)
		        {
					floorPositions.Add(new Vector2(obj.localPosition.x, obj.localPosition.z));
					floorScales.Add(new Vector2(obj.localScale.x, obj.localScale.z));
			//		Debug.Log("Obs found");
		        }
			}
			
			obstacles.Clear();
			smallerObstacles.Clear();
			for (int count = 0; count < floorPositions.Count; count ++)
			{
				List<Vector2> midpoints = new List<Vector2>();
				midpoints.Add(new Vector2(floorPositions[count].x, floorPositions[count].y));
				midpoints.Add(new Vector2(floorPositions[count].x + floorScales[count].x, floorPositions[count].y));
				midpoints.Add(new Vector2(floorPositions[count].x + floorScales[count].x, floorPositions[count].y - floorScales[count].y));
				midpoints.Add(new Vector2(floorPositions[count].x, floorPositions[count].y - floorScales[count].y));
				for (int count2 = 0; count2 < midpoints.Count; count2 ++)
				{
					obstacles.Add(new Line(midpoints[count2], midpoints[(count2+1)%midpoints.Count]));
				}
			}
			
			for (int count = 0; count < floorPositions.Count; count ++)
			{
				List<Vector2> midpoints = new List<Vector2>();
				midpoints.Add(new Vector2(floorPositions[count].x + (floorScales[count].x/2), floorPositions[count].y));
				midpoints.Add(new Vector2(floorPositions[count].x + floorScales[count].x, floorPositions[count].y - (floorScales[count].y/2)));
				midpoints.Add(new Vector2(floorPositions[count].x + (floorScales[count].x/2), floorPositions[count].y - floorScales[count].y));
				midpoints.Add(new Vector2(floorPositions[count].x, floorPositions[count].y - (floorScales[count].y/2)));
				for (int count2 = 0; count2 < midpoints.Count; count2 ++)
				{
			//		Debug.Log("Midpoint " + count2 +" : " + midpoints[count2]);
					smallerObstacles.Add(new Line(midpoints[count2], midpoints[(count2+1)%midpoints.Count]));
				}
			}
			
			foreach(Line l in obstacles)
			{
				Debug.Log("Obs line starting at " + l.start.x + ", " + l.start.y);
			}
		}
	}
}