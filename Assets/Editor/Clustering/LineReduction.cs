// C# Implementation of Douglas-Peucker algorithm
// by Craig Selbert
// src : http://www.codeproject.com/Articles/18936/A-Csharp-Implementation-of-Douglas-Peucker-Line-Ap

using System;
using System.Collections;
using System.Collections.Generic;
using Node = Common.Node;

namespace ClusteringSpace
{
	public class LineReduction
	{
		// Uses the Douglas Peucker algorithm to reduce the number of points.
		public static List<Node> DouglasPeuckerReduction(List<Node> Points, Double Tolerance)
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

		    DouglasPeuckerReduction(Points, firstPoint, lastPoint, Tolerance, ref pointIndexsToKeep);

		    List<Node> returnPoints = new List<Node>();
		    pointIndexsToKeep.Sort();
		    foreach (Int32 index in pointIndexsToKeep)
		    {
		        returnPoints.Add(Points[index]);
		    }

		    return returnPoints;
		}
    
		private static void DouglasPeuckerReduction(List<Node> points, Int32 firstPoint, Int32 lastPoint, Double tolerance, ref List<Int32> pointIndexsToKeep)
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
		    {
		        //Add the largest point that exceeds the tolerance
		        pointIndexsToKeep.Add(indexFarthest);
    
		        DouglasPeuckerReduction(points, firstPoint, indexFarthest, tolerance, ref pointIndexsToKeep);
		        DouglasPeuckerReduction(points, indexFarthest, lastPoint, tolerance, ref pointIndexsToKeep);
		    }
			else if (maxDistance < tolerance)
			{
				// todo...
			}
		}

		/// The distance of a point from a line made from point1 and point2.
		public static Double PerpendicularDistance(Node Point1, Node Point2, Node Point)
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
	}
}