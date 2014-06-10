using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Text;
using Path = Common.Path;
using Node = Common.Node;
using Debug = UnityEngine.Debug;
using EditorArea;

namespace ClusteringSpace
{
    public class KMeans
    {
		public enum Metrics
		{
			FrechetL1 = 0,
			FrechetL13D,
			FrechetEuclidean,
			AreaDistInterpolation3D,
			AreaDistTriangulation,
			Time
		}
		
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
			
			setDistMetric(distMetric_);
			if (MapperWindowEditor.scaleTime)
			{
				foreach (Path p in paths)
				{
					foreach (Node n in p.points)
					{
						n.t = (int)Math.Pow(n.t, 3);
					}
				}
			}
			/*			if (distMetric == (int)Metrics.FrechetL1 || distMetric == (int)Metrics.FrechetEuclidean)
			{ // make sure paths have enough points
				foreach(Path p in paths)
				{
					bool newPoint = false;
					do
					{
						newPoint = false;
						
						// get total distance
						int totalLength = 0;
						int[] distances = new int[p.points.Count()-1];
						for(int nodeCount = 0; nodeCount < p.points.Count()-1; nodeCount ++)
						{
							distances[nodeCount] = Math.Abs(p.points[nodeCount].x - p.points[nodeCount+1].x) + Math.Abs(p.points[nodeCount].y - p.points[nodeCount+1].y) + Math.Abs(p.points[nodeCount].t - p.points[nodeCount+1].t);
							totalLength += distances[nodeCount];
						}
				
						for(int nodeCount = 0; nodeCount < p.points.Count()-1; nodeCount ++)
						{
							if ((double)distances[nodeCount] / (double)totalLength > 0.10)
							{ // segment is larger than 20% of total length, so must be split						
								Node n = new Node();
								n.x = (int)(p.points[nodeCount].x + (p.points[nodeCount+1].x - p.points[nodeCount].x)*0.50);
								n.y = (int)(p.points[nodeCount].y + (p.points[nodeCount+1].y - p.points[nodeCount].y)*0.50);
								n.t = (int)(p.points[nodeCount].t + (p.points[nodeCount+1].t - p.points[nodeCount].t)*0.50);
								
								p.points.Insert(nodeCount+1, n);
						
								newPoint = true;
								break;
							}
						}
					} while(newPoint);
				}
			}*/
			
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
			// src : http://codeding.com/articles/k-means-algorithm
            int movements = 1;
			int count = 0;
			int[] previousMovements = new int[100];
            while (movements > 0)
            {
				previousMovements[count] = movements;
				if (count > 10)
				{
					int avgLastThree = (previousMovements[count-2] + previousMovements[count-1] + previousMovements[count]) / 3;
					if (Math.Abs(avgLastThree - previousMovements[count]) <= 10)
					{
						Debug.Log("Not converging.");
						break;
					}
				}
				
				count ++;
				MapperWindowEditor.updatePaths(allClusters);
				
                movements = 0;

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

			if (MapperWindowEditor.scaleTime)
			{
				foreach (Path p in paths)
				{
					foreach (Node n in p.points)
					{
						n.t = (int)(Math.Pow(n.t, (double)1.0 / 3.0));
					}
				}
			}

            return (allClusters);
        }

        public static int FindNearestCluster(List<PathCollection> allClusters, Path path)
        { // src : http://codeding.com/articles/k-means-algorithm
            double minimumDistance = 0.0;
            int nearestClusterIndex = -1;

            for (int k = 0; k < allClusters.Count; k++) //find nearest cluster
            {
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
			
            return (nearestClusterIndex);
        }
		
		private static void setDistMetric(int distMetric_)
		{
			distMetric = distMetric_;
			
			if (distMetric == (int)Metrics.FrechetL1)
			{
				frechet = new PolyhedralFrechetDistance(PolyhedralDistanceFunction.L1(2));
			}
			else if (distMetric == (int)Metrics.FrechetEuclidean)
			{
				frechet = new PolyhedralFrechetDistance(PolyhedralDistanceFunction.epsApproximation2D(1.1));
			}
			else if (distMetric == (int)Metrics.FrechetL13D)
			{
				frechet = new PolyhedralFrechetDistance(PolyhedralDistanceFunction.L1(3));
			}
		}
		
		// source : http://www.win.tue.nl/~wmeulema/implementations.html
		// Implementation by Wouter Meulemans
		// based on paper by Buchin et al
		// http://arxiv.org/abs/1306.5527
		
		public static double FindDistance(Path path1, Path path2, int distMetric_)
		{
			setDistMetric(distMetric_);
			return FindDistance(path1, path2);
		}
		
        public static double FindDistance(Path path1, Path path2)
        {
			if (path1.points == null) { Debug.Log("P1NULL"); return -1; }
			else if (path2.points == null) { Debug.Log("P2NULL"); return -1; }
			
			clustTime.Stop();
			distTime.Start();

			double result = 0.0;
			
			if (distMetric == (int)Metrics.FrechetL1 || distMetric == (int)Metrics.FrechetEuclidean || distMetric == (int)Metrics.FrechetL13D)
			{
				double[][] curveA = new double[path1.points.Count][];
				double[][] curveB = new double[path2.points.Count][];
				
				if (distMetric == (int)Metrics.FrechetL1 || distMetric == (int)Metrics.FrechetEuclidean)
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
				else if (distMetric == (int)Metrics.FrechetL13D)
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
/*			else if (distMetric == (int)Metrics.HausdorffEuclidean || distMetric == (int)Metrics.HausdorffEuclidean3D)
			{
				result = HausdorffDist.computeDistance(path1, path2, distMetric);
			}*/
			else if (distMetric == (int)Metrics.AreaDistTriangulation || distMetric == (int)Metrics.AreaDistInterpolation3D)
			{
				result = AreaDist.computeDistance(path1, path2, distMetric);
			}
			else if (distMetric == (int)Metrics.Time)
			{
				frechet = new PolyhedralFrechetDistance(PolyhedralDistanceFunction.L1(1));
				double[][] curveA = new double[path1.points.Count][];
				double[][] curveB = new double[path2.points.Count][];
				
				for (int i = 0; i < path1.points.Count; i ++)
				{
					curveA[i] = new double[] { path1.points[i].t };
				}
				for (int i = 0; i < path2.points.Count; i ++)
				{
					curveB[i] = new double[] { path2.points[i].t };
				}
				result = frechet.computeDistance(curveA,curveB);
			}
			else
			{
				Debug.Log("Invalid distance metric ("+distMetric+")!");
				return -1;
			}
			
			distTime.Stop();
			clustTime.Start();
			
			return result;
        }
    }
}