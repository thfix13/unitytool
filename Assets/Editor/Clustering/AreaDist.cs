using System;
using System.Collections.Generic;
using UnityEngine;
using Path = Common.Path;
using Node = Common.Node;
using System.Linq;
using Vectrosity; 
using EditorArea;
using ClusteringSpace;

public class AreaDist
{
	public class Point : IEquatable<Point>
	{
		public int x, y;
		public Point(int x_, int y_)
		{
			x = x_;
			y = y_;
		}
/*		public Point(Vector3 vec_)
		{
			x = (int)vec_.x;
			y = (int)vec_.y;
		}*/
		public bool Equals(Point other)
		{
			return (x == other.x && y == other.y);
		}
	}
	
	public static double computeDistance(Path path1, Path path2, int distMetric)
	{
		if (distMetric == (int)Metrics.AreaDistTriangulation)
		{
			return areaFromTriangulation(path1, path2);
		}
		else if (distMetric == (int)Metrics.AreaDistInterpolation3D)
		{
			return areaFromInterpolation3D(path1, path2);
		}
		else return -1;
	}
	
	public static double areaFromInterpolation3D(Path path1, Path path2)
	{
		int numberLines = 20;
		Vector3[] set1 = ClusteringEditorWindow.GetSetPointsWithN(path1.getPoints3D(), numberLines); 
		Vector3[] set2 = ClusteringEditorWindow.GetSetPointsWithN(path2.getPoints3D(), numberLines); 
		
		// shape formed from set1[i], set1[i+1] to set2[i], set2[i+1]
		double area = 0.0;
		for (int i = 0; i < set1.Length-1; i ++)
		{			
			float[][] vertices = new float[4][];
			for (int j = 0; j < 4; j ++) vertices[j] = new float[2];
			
			vertices[0][0] = set1[i].x;
			vertices[0][1] = set1[i].y;
			vertices[1][0] = set1[i+1].x;
			vertices[1][1] = set1[i+1].y;
			vertices[2][0] = set2[i+1].x;
			vertices[2][1] = set2[i+1].y;
			vertices[3][0] = set2[i].x;
			vertices[3][1] = set2[i].y;
			
			area += getArea(vertices);
		}
		
		return area;
	}
	
	public static double areaFromTriangulation(Path path1, Path path2)
	{ // vertices of each path
		
		GameObject g = GameObject.Find("Triangulation"); 
				
		if(g != null)
		{
			Triangulation tObject = g.GetComponent<Triangulation>(); 
			float area = tObject.TriangulationCurves(path1.getPoints3D(), path2.getPoints3D(),ClusteringEditorWindow.only2DTriangulation);
//			Debug.Log("A: "+area);
			return area;
		}
		else
		{
			Debug.Log("You must have a Triangulation object.");
			return -1;
		}
		
		/*List<Point> polygonVertices = new List<Point>();
		
		double area = 0.0;
		for (int count = 0; count < path1.points.Count-1; count ++)
		{
			polygonVertices.Add(new Point(path1.points[count].x, path1.points[count].y)); // s1
			for (int count2 = 0; count2 < path2.points.Count-1; count2 ++)
			{
				Point intersection = getIntersectionOf(path1.points[count], path1.points[count+1], path2.points[count2], path2.points[count2+1]);
				if (intersection != null)
				{
					polygonVertices.Add(intersection); // i
					polygonVertices.Add(new Point(path2.points[count2].x, path2.points[count2].y)); // s2
					
					area += getArea(polygonVertices);
					
					polygonVertices.Clear();
					polygonVertices.Add(intersection); // i
					polygonVertices.Add(new Point(path2.points[count2+1].x, path2.points[count2+1].y)); // e2
				}
			}
			polygonVertices.Add(new Point(path1.points[count+1].x, path1.points[count+1].y)); // e1
		} */
		
/*		List<Point> intersections = new List<Point>();
		for (int count = 0; count < path1.points.Count-1; count ++)
		{
			for (int count2 = 0; count2 < path2.points.Count-1; count2 ++)
			{
				Point intersection = getIntersectionOf(path1.points[count], path1.points[count+1], path2.points[count2], path2.points[count2+1]);
				if (intersection != null)
				{
					intersections.Add(intersection);
				}
			}
		}
				
		return area;*/
	}
	
	public static Point getIntersectionOf(Node s1, Node e1, Node s2, Node e2)
	{ // src : http://stackoverflow.com/questions/15648607/finding-the-intersection-of-a-line
		Vector2 thisPoint1 = new Vector3(s1.x, s1.y);
		Vector2 thisPoint2 = new Vector3(e1.x, e1.y);

		Vector2 otherPoint1 = new Vector3(s2.x, s2.y);
		Vector2 otherPoint2 = new Vector3(e2.x, e2.y);

		float A1 = thisPoint2.y - thisPoint1.y;
		float B1 = thisPoint1.x - thisPoint2.x;
		float C1 = A1 * thisPoint1.x + B1 * thisPoint1.y;

		float A2 = otherPoint2.y - otherPoint1.y;
		float B2 = otherPoint1.x - otherPoint2.x;
		float C2 = A2 * otherPoint1.x + B2 * otherPoint1.y;

		float det = A1 * B2 - A2 * B1;
		if (det == 0)
		{
			Debug.Log("no intersection");
			return null;
		}

		float x = (B2 * C1 - B1 * C2) / det;
		float y = (A1 * C2 - A2 * C1) / det;

		Debug.Log("intersection at "+x+", "+y);
		return new Point((int)x, (int)y);
	}
	
	public static double getArea(List<Point> vertexList)
	{
		float[][] vertices = new float[vertexList.Count()][];
		for (int count = 0; count < vertexList.Count(); count ++)
		{ // alg takes ordered pairs of (x,y)
			vertices[count] = new float[2];
			vertices[count][0] = vertexList[count].x;
			vertices[count][1] = vertexList[count].y;
		}
		
		return getArea(vertices);
	}
	
	public static double getArea(float[][] arr)
	{ // src : http://www.sanfoundry.com/java-program-shoelace-algorithm/
		int n = arr.Length;
		/** copy initial point to last row **/
		arr[n - 1][0] = arr[0][0];
		arr[n - 1][1] = arr[0][1];
 
        double det = 0.0;
        /** add product of x coordinate of ith point with y coordinate of (i + 1)th point **/
        for (int i = 0; i < n - 1; i++)
            det += (double)(arr[i][0] * arr[i + 1][1]);
        /** subtract product of y coordinate of ith point with x coordinate of (i + 1)th point **/
        for (int i = 0; i < n - 1; i++)
            det -= (double)(arr[i][1] * arr[i + 1][0]);

        /** find absolute value and divide by 2 **/
        det = Math.Abs(det);    
        det /= 2;
        return det;
	}
}