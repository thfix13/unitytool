//KMeans.cs
//modified from source at http://codeding.com/articles/k-means-algorithm

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Text;
using Path = Common.Path;
using Debug = UnityEngine.Debug;
using EditorArea;

namespace ClusteringSpace
{
    public class KMeans
    {
		public static Stopwatch distTime = new Stopwatch();
		public static Stopwatch clustTime = new Stopwatch();
		static FrechetDistance frechet;
        
		private static int distMetric = 0;
		
        public static List<PathCollection> DoKMeans(List<Path> paths, int clusterCount, int distMetric_)
        {
			if (paths.Count == 0)
			{
				Debug.Log("No paths to cluster!");
				return null;
			}
						
			distMetric = distMetric_;
			if (distMetric == 0)
			{
				frechet = new PolyhedralFrechetDistance(PolyhedralDistanceFunction.L1(2));
			}
			else if (distMetric == 1)
			{
				frechet = new PolyhedralFrechetDistance(PolyhedralDistanceFunction.epsApproximation2D(1.1));
			}
			else if (distMetric == 3)
			{
				frechet = new PolyhedralFrechetDistance(PolyhedralDistanceFunction.L1(3));
			//	frechet = new PolyhedralFrechetDistance(PolyhedralDistanceFunction.LInfinity(3));
			}
			clustTime.Start();

            //divide paths into equal clusters
            List<PathCollection> allClusters = new List<PathCollection>();
            List<List<Path>> allGroups = ListUtility.SplitList<Path>(paths, clusterCount);
            foreach (List<Path> pathGroup in allGroups)
            {
                PathCollection cluster = new PathCollection(pathGroup);
                allClusters.Add(cluster);
            }
			
            //start k-means clustering
            int movements = 1;
			int count = 0;
            while (movements > 0)
            {
				count ++;
				MapperWindowEditor.updatePaths(allClusters);
				
				if (count > 200) { Debug.Log("Over 200 iterations"); clustTime.Stop(); return allClusters; }
                movements = 0;

//                foreach (PathCollection cluster in allClusters) //for all clusters
                for (int clusterIndex = 0; clusterIndex < allClusters.Count; clusterIndex ++)
                {
					for (int pathIndex = 0; pathIndex < allClusters[clusterIndex].Count; pathIndex++) //for all paths in each cluster
                    {
                        Path path = allClusters[clusterIndex][pathIndex];

                        int nearestCluster = FindNearestCluster(allClusters, path);
                        if (nearestCluster != clusterIndex) //if path has moved
                        {
							if (allClusters[clusterIndex].Count > 1) //cluster shall have minimum one path
                            {
								Path removedPath = allClusters[clusterIndex].removePath(path);
                                allClusters[nearestCluster].AddPath(removedPath);
                                movements += 1;
                            }
                        }
                    }
                }
            }
			
			clustTime.Stop();

            return (allClusters);
        }

        public static int FindNearestCluster(List<PathCollection> allClusters, Path path)
        {
            double minimumDistance = 0.0;
            int nearestClusterIndex = -1;

            for (int k = 0; k < allClusters.Count; k++) //find nearest cluster
            {
	//			Debug.Log("FNC");
                double distance = FindDistance(path, allClusters[k].Centroid);
                if (k == 0)
                {
                    minimumDistance = distance;
                    nearestClusterIndex = 0;
                }
                else if (minimumDistance > distance)
                {
                    minimumDistance = distance;
                    nearestClusterIndex = k;
                }
            }
			
//			Debug.Log("Path p(" + path.points[6].x +", " + path.points[6].y + ") is closest to cluster " + nearestClusterIndex);

            return (nearestClusterIndex);
        }
		
		// source : http://www.win.tue.nl/~wmeulema/implementations.html
		// Implementation by Wouter Meulemans
		// based on paper by Buchin et al
		// http://arxiv.org/abs/1306.5527
		
        public static double FindDistance(Path path1, Path path2)
        {
			if (path1.points == null) { Debug.Log("P1NULL"); return -1; }
			else if (path2.points == null) { Debug.Log("P2NULL"); return -1; }
			
			clustTime.Stop();
			distTime.Start();

			double result = 0.0;
			
			//public String[] distMetrics = new String[] { "Fréchet (L1)", "Fréchet (Euclidean)", "Hausdorff (Euclidean)", "Frechet L1 3D" };
			if (distMetric == 0 || distMetric == 1 || distMetric == 3)
			{
				double[][] curveA = new double[path1.points.Count][];
				double[][] curveB = new double[path2.points.Count][];
				
				if (distMetric <= 1)
				{
					for (int i = 0; i < path1.points.Count; i ++)
					{
						curveA[i] = new double[] { path1.points[i].x, path1.points[i].y };
					}
					for (int i = 0; i < path2.points.Count; i ++)
					{
						curveB[i] = new double[] { path2.points[i].x, path2.points[i].y };
					}
				}
				else if (distMetric == 3)
				{
					for (int i = 0; i < path1.points.Count; i ++)
					{
						curveA[i] = new double[] { path1.points[i].x, path1.points[i].y, path1.points[i].t };
					}
					for (int i = 0; i < path2.points.Count; i ++)
					{
						curveB[i] = new double[] { path2.points[i].x, path2.points[i].y, path2.points[i].t };
					}
				}
				
				result = frechet.computeDistance(curveA,curveB);
			}
			else if (distMetric == 2)
			{
				result = HausdorffDist.computeDistance(path1, path2);
			}
			else
			{
				Debug.Log("Invalid distance metric ("+distMetric+")!");
				return -1;
			}
		//	double result = AreaDist.computeDistance(path1, path2);
			
			distTime.Stop();
			clustTime.Start();
			
			return result;
        }
    }
}
