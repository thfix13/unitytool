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
	public class LineReduction
	{
		public static List<Node> shortestPathAroundObstacles(LevelRepresentation rep, List<Node> points, int prevPointCount = -1)
		{
			if (points == null || points.Count < 3 || (prevPointCount == points.Count)) return points;
			
			int numPoints = points.Count;
			points = DouglasPeuckerReduction(rep, points, 100000, true);
			
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
			
			for (int count = 0; count < rep.obstacles.Count; count ++)
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
							Debug.Log("Clipping on point (" + rep.obstacles[oldObs].start.x*1000 + ", " + rep.obstacles[oldObs].start.y*1000 + ")" );
							clip[0].Add(new IntPoint(rep.obstacles[oldObs].start.x*1000, rep.obstacles[oldObs].start.y*1000));
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
							Debug.Log("Checking against start pos of " + rep.startPos.x + ", " + rep.startPos.y);
							if (returnPoints[pointsCount].x == rep.startPos.x && returnPoints[pointsCount].y == rep.startPos.y)
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
						return shortestPathAroundObstacles(rep, sortedPoints, numPoints);
					}
					
					numIntersect = obsCount = 0;
				}
				
				Vector2 p1 = new Vector2(points[firstPoint].x, points[firstPoint].y);
				Vector2 p2 = new Vector2(points[indexFarthest].x, points[indexFarthest].y);
				Vector2 p3 = new Vector2(points[lastPoint].x, points[lastPoint].y);
				int state = Intersecting(rep.obstacles[count].start, rep.obstacles[count].end, p1, p2, p3);
				if (state == 2) numIntersect ++;
				
				obsCount ++;
			}
			
			return points;
		}
		
		// Uses the Douglas Peucker algorithm to reduce the number of points.
		public static List<Node> DouglasPeuckerReduction(LevelRepresentation rep, List<Node> Points, Double Tolerance, bool aroundObstacles)
		{
		    if (Points == null || Points.Count < 3)
		    return Points;

		    Int32 firstPoint = 0;
		    Int32 lastPoint = Points.Count - 1;
		    List<Int32> pointIndexsToKeep = new List<Int32>();

		    //Add the first and last index to the keepers
		    pointIndexsToKeep.Add(firstPoint);
		    pointIndexsToKeep.Add(lastPoint);

		    //The first and the last point cannot be the same
		    while (Points[firstPoint].Equals(Points[lastPoint]))
		    {
		        lastPoint--;
		    }

		    DouglasPeuckerReduction(rep, Points, firstPoint, lastPoint, Tolerance, ref pointIndexsToKeep, aroundObstacles);

		    List<Node> returnPoints = new List<Node>();
		    pointIndexsToKeep.Sort();
		    foreach (Int32 index in pointIndexsToKeep)
		    {
//				Debug.Log("Adding index " + index + ", numpts: " + Points.Count);
		        returnPoints.Add(Points[index]);
		    }

		    return returnPoints;
		}
    
		private static void DouglasPeuckerReduction(LevelRepresentation rep, List<Node> points, Int32 firstPoint, Int32 lastPoint, Double tolerance, ref List<Int32> pointIndexsToKeep, bool aroundObstacles)
		{			
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
    
		        DouglasPeuckerReduction(rep, points, firstPoint, indexFarthest, tolerance, ref pointIndexsToKeep, aroundObstacles);
		        DouglasPeuckerReduction(rep, points, indexFarthest, lastPoint, tolerance, ref pointIndexsToKeep, aroundObstacles);
				
				return;
		    }
			else if (maxDistance <= tolerance && indexFarthest != 0)
			{
				// check if an obstacle is contained within the triangle
				bool obstacleContained = false;
				
				if (aroundObstacles)
				{
					for (int count = 0; count < rep.smallerObstacles.Count; count ++)
					{					
						Vector2 p1 = new Vector2(points[firstPoint].x, points[firstPoint].y);
						Vector2 p2 = new Vector2(points[indexFarthest].x, points[indexFarthest].y);
						Vector2 p3 = new Vector2(points[lastPoint].x, points[lastPoint].y);
						int state = Intersecting(rep.smallerObstacles[count].start, rep.smallerObstacles[count].end, p1, p2, p3);
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
    
			        DouglasPeuckerReduction(rep, points, firstPoint, indexFarthest, tolerance, ref pointIndexsToKeep, aroundObstacles);
			        DouglasPeuckerReduction(rep, points, indexFarthest, lastPoint, tolerance, ref pointIndexsToKeep, aroundObstacles);
				}
//				else Debug.Log("Does not contain");
				
				return;
			}
		}

		/// The distance of a point from a line made from point1 and point2.
		private static Double PerpendicularDistance(Node Point1, Node Point2, Node Point)
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
		private static float Side(Vector2 p, Vector2 q, Vector2 a, Vector2 b)
		{
		    float z1 = (b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y);
		    float z2 = (b.x - a.x) * (q.y - a.y) - (q.x - a.x) * (b.y - a.y);
		    return z1 * z2;
		}

		/* Check whether segment P0P1 intersects with triangle t0t1t2 */
		private static int Intersecting(Vector2 p0, Vector2 p1, Vector2 t0, Vector2 t1, Vector2 t2)
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