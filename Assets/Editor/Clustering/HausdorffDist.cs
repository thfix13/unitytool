using System;
using System.Collections.Generic;
using UnityEngine;
using Path = Common.Path;
using Node = Common.Node;

public class HausdorffDist
{	
	public static double computeDistance(Path path1, Path path2)
	{
		double maxMinDist = 0.0;
		foreach(Node n1 in path1.points)
		{
			double minDist = double.PositiveInfinity;
			foreach(Node n2 in path2.points)
			{
				double dist = computeDistance(n1, n2);
				if (dist < minDist)
				{
					minDist = dist;
				}
			}
			if (minDist > maxMinDist)
			{
				maxMinDist = minDist;
			}
		}
		return maxMinDist;
	}
	
	public static double computeDistance(Node node1, Node node2)
	{
		return Mathf.Sqrt(Mathf.Pow(node1.x - node2.x, 2) + Mathf.Pow(node1.y - node2.y, 2));
	}
}