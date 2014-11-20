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
	}
	
	public class LevelRepresentation
	{
		public Vector2[] floorPositions;
        public Vector2[] floorScales;
		public Vector2 startPos;
		public List<Line> obstacles = new List<Line>();
		public List<Line> smallerObstacles = new List<Line>();
		
		public static Vector2 zero = new Vector2 ();
		public static Vector2 tileSize = new Vector2 ();
				
		public void loadPlatformerLevel()
		{
			// load the level information (currently just floor pos + scale)
			XmlSerializer ser = new XmlSerializer (typeof(PlatformerLevelInfo));
			PlatformerLevelInfo loaded = null;
			using (FileStream stream = new FileStream ("batchpaths/levelinfo.xml", FileMode.Open)) {
				loaded = (PlatformerLevelInfo)ser.Deserialize (stream);
				stream.Close ();
			}
			floorPositions = loaded.floorPositions;
			floorScales = loaded.floorScales;
			startPos = loaded.startPos;
			
			// destroy current floor
			GameObject levelObj = GameObject.Find("Level"); 
			for (int i = levelObj.transform.childCount - 1; i > -1; i--)
			{
			    GameObject.DestroyImmediate(levelObj.transform.GetChild(i).gameObject);
			}
			
			// add new floor!
			GameObject templateWall = GameObject.Find("TemplateWall");
			for (int count = 0; count < floorPositions.Length; count ++)
			{
				Transform wallTransform = GameObject.Instantiate(templateWall.transform) as Transform;
				GameObject wall = wallTransform.gameObject;
				wall.name = "Platform";
				wall.tag = "Platform";
			//	floorScales[count].x *= tileSize.x;
			//	floorScales[count].y *= tileSize.y;
				wall.transform.position = new Vector3((floorPositions[count].x) * tileSize.x + zero.x, 0, floorPositions[count].y * tileSize.y + zero.y);
				wall.transform.localScale = new Vector3(floorScales[count].x*tileSize.x, 3, floorScales[count].y*tileSize.y);
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
	}
}