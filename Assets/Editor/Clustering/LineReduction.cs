// C# Implementation of Douglas-Peucker algorithm
// by Craig Selbert
// src : http://www.codeproject.com/Articles/18936/A-Csharp-Implementation-of-Douglas-Peucker-Line-Ap
// modified to not smooth over obstacles

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using Node = Common.Node;
using ClipperLib;

namespace ClusteringSpace
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.17020")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
	[Serializable, XmlRoot("LevelInfo")]
	public partial class PlatformerLevelInfo
	{
		public Vector2[] floorPositions;
        public Vector2[] floorScales;
		public Vector2 startPos;
    }
	
	public class Line
	{
		public Vector2 start;
		public Vector2 end;
		public Line(Vector2 s_, Vector2 e_) { start = s_; end = e_; }
	}
	
	public class LineReduction
	{
		public Vector2[] floorPositions;
        public Vector2[] floorScales;
		public Vector2 startPos;
		public List<Line> obstacles = new List<Line>();
		public List<Line> smallerObstacles = new List<Line>();
		
		public static Vector2 zero = new Vector2 ();
		public static Vector2 tileSize = new Vector2 ();
		
		private static bool restart = false;
		
		public LineReduction()
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
				floorScales[count].x *= tileSize.x;
				floorScales[count].y *= tileSize.y;
				wall.transform.position = new Vector3((floorPositions[count].x) * tileSize.x + zero.x, 0, floorPositions[count].y * tileSize.y + zero.y);
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
				midpoints.Add(new Vector2(floorPositions[count].x + (floorScales[count].x/2), floorPositions[count].y)); // [0]
				midpoints.Add(new Vector2(floorPositions[count].x + floorScales[count].x, floorPositions[count].y - (floorScales[count].y/2))); // [1]
				midpoints.Add(new Vector2(floorPositions[count].x + (floorScales[count].x/2), floorPositions[count].y - floorScales[count].y)); // [2]
				midpoints.Add(new Vector2(floorPositions[count].x, floorPositions[count].y - (floorScales[count].y/2))); // [3]
				for (int count2 = 0; count2 < midpoints.Count; count2 ++)
				{
			//		Debug.Log("Midpoint " + count2 +" : " + midpoints[count2]);
					smallerObstacles.Add(new Line(midpoints[count2], midpoints[(count2+1)%midpoints.Count]));
				}
			}
		}
		
		public List<Node> shortestPathAroundObstacles(List<Node> points, int prevPointCount = -1)
		{
			if (points == null || points.Count < 3 || (prevPointCount == points.Count)) return points;
			
			int numPoints = points.Count;
			points = DouglasPeuckerReduction(points, 100000, true);
			
			// get point that is farthest from the line from first point to last point
			int firstPoint = 0, lastPoint = points.Count - 1;
		    double maxDistance = 0;
		    int indexFarthest = 0;
    
		    for (int index = firstPoint; index < lastPoint; index++)
		    {
		        double distance = PerpendicularDistance(points[firstPoint], points[lastPoint], points[index]);
		        if (distance > maxDistance)
		        {
					maxDistance = distance;
					indexFarthest = index;
		        }
		    }
			
			if (indexFarthest == 0) return points;
			
			// check if an obstacle is contained within the triangle
			int obsCount = 0, numIntersect = 0;
			
			for (int count = 0; count < obstacles.Count; count ++)
			{
				if (obsCount == 4) // assuming square obstacle
				{ // gone through 4 lines of one obstacle.
					if (numIntersect > 0)
					{ // an obstacle intersects but is not fully contained within the triangle
						// so try to construct a line around that obstacle.
												
						// polygon subtraction
						List<List<IntPoint>> subj = new List<List<IntPoint>>(1);
						subj.Add(new List<IntPoint>(3));
						subj[0].Add(new IntPoint(points[firstPoint].xD*1000, points[firstPoint].yD*1000));
						subj[0].Add(new IntPoint(points[indexFarthest].xD*1000, points[indexFarthest].yD*1000));
						subj[0].Add(new IntPoint(points[lastPoint].xD*1000, points[lastPoint].yD*1000));
						
						List<List<IntPoint>> clip = new List<List<IntPoint>>(1);
						clip.Add(new List<IntPoint>(4)); // assuming square obstacle
						for (int oldObs = count - 4; oldObs < count; oldObs ++)
						{
							Debug.Log("Clipping on point (" + obstacles[oldObs].start.x*1000 + ", " + obstacles[oldObs].start.y*1000 + ")" );
							clip[0].Add(new IntPoint(obstacles[oldObs].start.x*1000, obstacles[oldObs].start.y*1000));
						}
						
						List<List<IntPoint>> solution = new List<List<IntPoint>>();
						Clipper c = new Clipper();
						c.AddPaths(subj, PolyType.ptSubject, true);
						c.AddPaths(clip, PolyType.ptClip, true);
						c.Execute(ClipType.ctUnion, solution, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
						
						List<Node> returnPoints = new List<Node>();
						foreach(List<IntPoint> list in solution) foreach (IntPoint ip in list)
						{
							double xpos = ip.X / 1000.0, ypos = ip.Y / 1000.0;
							Debug.Log("RP y: " + ypos + " ( " + ip.Y + ")");
							returnPoints.Add(new Node(xpos, ypos, 0.0));
						}
						
						List<Node> sortedPoints = new List<Node>();
						for (int pointsCount = 0; pointsCount < returnPoints.Count; pointsCount ++)
						{ // sort points according to start position
							Debug.Log("Checking against start pos of " + startPos.x + ", " + startPos.y);
							if (returnPoints[pointsCount].x == startPos.x && returnPoints[pointsCount].y == startPos.y)
							{
								for (int startIndex = pointsCount; ; startIndex = (startIndex + 1) % returnPoints.Count)
								{
									if (startIndex == pointsCount && sortedPoints.Count > 0) break;
									sortedPoints.Add(returnPoints[startIndex]);
									Debug.Log(" P (" + returnPoints[startIndex].x + ", " + returnPoints[startIndex].y + ")");
								}
								break;
							}
						}
						
						// found a line around this one
						return shortestPathAroundObstacles(sortedPoints, numPoints);
					}
					
					numIntersect = obsCount = 0;
				}
				
				Vector2 p1 = new Vector2(points[firstPoint].x, points[firstPoint].y);
				Vector2 p2 = new Vector2(points[indexFarthest].x, points[indexFarthest].y);
				Vector2 p3 = new Vector2(points[lastPoint].x, points[lastPoint].y);
				int state = Intersecting(obstacles[count].start, obstacles[count].end, p1, p2, p3);
				if (state == 2) numIntersect ++;
				
				obsCount ++;
			}
			
			return points;
		}
		
		// Uses the Douglas Peucker algorithm to reduce the number of points.
		public List<Node> DouglasPeuckerReduction(List<Node> Points, Double Tolerance, bool aroundObstacles)
		{
		    if (Points == null || Points.Count < 3)
		    return Points;

		    Int32 firstPoint = 0;
		    Int32 lastPoint = Points.Count - 1;
		    List<Int32> pointIndexsToKeep = new List<Int32>();
			restart = false;

		    //Add the first and last index to the keepers
		    pointIndexsToKeep.Add(firstPoint);
		    pointIndexsToKeep.Add(lastPoint);

		    //The first and the last point cannot be the same
		    while (Points[firstPoint].Equals(Points[lastPoint]))
		    {
		        lastPoint--;
		    }

		    DouglasPeuckerReduction(Points, firstPoint, lastPoint, Tolerance, ref pointIndexsToKeep, aroundObstacles);

		    List<Node> returnPoints = new List<Node>();
		    pointIndexsToKeep.Sort();
		    foreach (Int32 index in pointIndexsToKeep)
		    {
//				Debug.Log("Adding index " + index + ", numpts: " + Points.Count);
		        returnPoints.Add(Points[index]);
		    }

		    return returnPoints;
		}
    
		private void DouglasPeuckerReduction(List<Node> points, Int32 firstPoint, Int32 lastPoint, Double tolerance, ref List<Int32> pointIndexsToKeep, bool aroundObstacles)
		{
			if (restart) return;
			
		    Double maxDistance = 0;
		    Int32 indexFarthest = 0;
    
		    for (Int32 index = firstPoint; index < lastPoint; index++)
		    {
		        Double distance = PerpendicularDistance(points[firstPoint], points[lastPoint], points[index]);
		        if (distance > maxDistance)
		        {
					maxDistance = distance;
					indexFarthest = index;
		        }
		    }

		    if (maxDistance > tolerance && indexFarthest != 0)
		    { // Add the largest point that exceeds the tolerance
		        pointIndexsToKeep.Add(indexFarthest);
//				Debug.Log("Exceeds tolerance!");
    
		        DouglasPeuckerReduction(points, firstPoint, indexFarthest, tolerance, ref pointIndexsToKeep, aroundObstacles);
		        DouglasPeuckerReduction(points, indexFarthest, lastPoint, tolerance, ref pointIndexsToKeep, aroundObstacles);
				
				return;
		    }
			else if (maxDistance <= tolerance && indexFarthest != 0)
			{
				// check if an obstacle is contained within the triangle
				bool obstacleContained = false;
				
				if (aroundObstacles)
				{
					for (int count = 0; count < smallerObstacles.Count; count ++)
					{					
						Vector2 p1 = new Vector2(points[firstPoint].x, points[firstPoint].y);
						Vector2 p2 = new Vector2(points[indexFarthest].x, points[indexFarthest].y);
						Vector2 p3 = new Vector2(points[lastPoint].x, points[lastPoint].y);
						int state = Intersecting(smallerObstacles[count].start, smallerObstacles[count].end, p1, p2, p3);
						if (state > 0)
						{
							obstacleContained = true;
							break;
						}
					}
				}
				
				if (obstacleContained)
				{ // must keep that point!
			        pointIndexsToKeep.Add(indexFarthest);
//					Debug.Log("Contains Obstacle!");
    
			        DouglasPeuckerReduction(points, firstPoint, indexFarthest, tolerance, ref pointIndexsToKeep, aroundObstacles);
			        DouglasPeuckerReduction(points, indexFarthest, lastPoint, tolerance, ref pointIndexsToKeep, aroundObstacles);
				}
//				else Debug.Log("Does not contain");
				
				return;
			}
		}

		/// The distance of a point from a line made from point1 and point2.
		public Double PerpendicularDistance(Node Point1, Node Point2, Node Point)
		{
		    //Area = |(1/2)(x1y2 + x2y3 + x3y1 - x2y1 - x3y2 - x1y3)|   *Area of triangle
		    //Base = v((x1-x2)²+(x1-x2)²)                               *Base of Triangle*
		    //Area = .5*Base*H                                          *Solve for height
		    //Height = Area/.5/Base

		    Double area = Math.Abs(.5 * (Point1.x * Point2.y+ Point2.x * 
		    Point.y + Point.x * Point1.y - Point2.x * Point1.y - Point.x * 
		    Point2.y - Point1.x * Point.y));
		    Double bottom = Math.Sqrt(Math.Pow(Point1.x - Point2.x, 2) + 
		    Math.Pow(Point1.y - Point2.y, 2));
		    Double height = area / bottom * 2;

		    return height;
		}
		
		// src : http://gamedev.stackexchange.com/questions/21096/what-is-an-efficient-2d-line-segment-versus-triangle-intersection-test
		/* Check whether P and Q lie on the same side of line AB */
		float Side(Vector2 p, Vector2 q, Vector2 a, Vector2 b)
		{
		    float z1 = (b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y);
		    float z2 = (b.x - a.x) * (q.y - a.y) - (q.x - a.x) * (b.y - a.y);
		    return z1 * z2;
		}

		/* Check whether segment P0P1 intersects with triangle t0t1t2 */
		int Intersecting(Vector2 p0, Vector2 p1, Vector2 t0, Vector2 t1, Vector2 t2)
		{
		    /* Check whether segment is outside one of the three half-planes
		     * delimited by the triangle. */
		    float f1 = Side(p0, t2, t0, t1), f2 = Side(p1, t2, t0, t1);
		    float f3 = Side(p0, t0, t1, t2), f4 = Side(p1, t0, t1, t2);
		    float f5 = Side(p0, t1, t2, t0), f6 = Side(p1, t1, t2, t0);
		    /* Check whether triangle is totally inside one of the two half-planes
		     * delimited by the segment. */
		    float f7 = Side(t0, t1, p0, p1);
		    float f8 = Side(t1, t2, p0, p1);

		    /* If segment is strictly outside triangle, or triangle is strictly
		     * apart from the line, we're not intersecting */
		    if ((f1 < 0 && f2 < 0) || (f3 < 0 && f4 < 0) || (f5 < 0 && f6 < 0)
		          || (f7 > 0 && f8 > 0))
		        return 0; // separate

		    /* If segment is aligned with one of the edges, we're overlapping */
		    if ((f1 == 0 && f2 == 0) || (f3 == 0 && f4 == 0) || (f5 == 0 && f6 == 0))
		        return 0; // segment on edge

		    /* If segment is outside but not strictly, or triangle is apart but
		     * not strictly, we're touching */
		    if ((f1 <= 0 && f2 <= 0) || (f3 <= 0 && f4 <= 0) || (f5 <= 0 && f6 <= 0)
		          || (f7 >= 0 && f8 >= 0))
		        return 0; // point on edge

		    /* If both segment points are strictly inside the triangle, we
		     * are not intersecting either */
		    if (f1 > 0 && f2 > 0 && f3 > 0 && f4 > 0 && f5 > 0 && f6 > 0)
		        return 1; // contained

		    /* Otherwise we're intersecting with at least one edge */
		    return 2;
		}
	}
}