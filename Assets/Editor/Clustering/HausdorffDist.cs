using System;
using System.Collections.Generic;
using UnityEngine;
using Path = Common.Path;
using Node = Common.Node;
using ClusteringSpace;

public class HausdorffDist
{	
	public static double computeDistance(double[][] path1, double[][] path2)
	{ // double[][] is a list of curves of double[], each double being a specified dimension
		return Math.Max(computeSingleDistance(path1, path2), computeSingleDistance(path2, path1));
	}

	public static double computeSingleDistance(double[][] path1, double[][] path2)
	{
		double maxMinDist = 0.0;
		foreach (double[] curve1 in path1) // foreach curve in path1
		{
			double minDist = double.PositiveInfinity;
			foreach (double[] curve2 in path2)
			{
				double dist = computeDistance(curve1, curve2);
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
	
	public static double computeDistance(double[] curve1, double[] curve2)
	{
		double sumOfPowers = 0.0f;
		for (int count = 0; count < curve1.Length; count ++)
		{ // assuming that curve1 and curve2 have the same length
			sumOfPowers += Math.Pow(curve1[count] - curve2[count], 2);
		}
		return Math.Sqrt(sumOfPowers);
	}
}