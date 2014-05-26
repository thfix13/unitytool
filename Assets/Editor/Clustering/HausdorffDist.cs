using System;
using System.Collections.Generic;
using UnityEngine;
using Path = Common.Path;
using Node = Common.Node;
using ClusteringSpace;

public class HausdorffDist
{	
	public static double computeDistance(Path path1, Path path2, int distMetric)
	{
		double maxMinDist = 0.0;
		foreach(Node n1 in path1.points)
		{
			double minDist = double.PositiveInfinity;
			foreach(Node n2 in path2.points)
			{
				double dist = computeDistance(n1, n2, distMetric);
				if (dist < minDist)
				{
					minDist = dist;
				}
				if (dist == 0)
				{
					break;
				}
			}
			if (minDist > maxMinDist)
			{
				maxMinDist = minDist;
			}
		}
		return maxMinDist;
	}
	
	public static double computeDistance(Node node1, Node node2, int distMetric)
	{
		if (distMetric == (int)KMeans.Metrics.HausdorffEuclidean)
			return Mathf.Sqrt(Mathf.Pow(node1.x - node2.x, 2) + Mathf.Pow(node1.y - node2.y, 2));
		else if (distMetric == (int)KMeans.Metrics.HausdorffEuclidean3D)
			return Mathf.Sqrt(Mathf.Pow(node1.x - node2.x, 2) + Mathf.Pow(node1.y - node2.y, 2) + Mathf.Pow(node1.t - node2.t, 2));
		else
		{
			Debug.Log("Incorrect Hausdorff dist metric.");
			return -1.0;
		}
	}
}