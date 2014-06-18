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
		
		private static List<PathCollection> usePredeterminedCentroids(List<Path> paths)
		{
			List<PathCollection> allClusters = new List<PathCollection>();
			
			allClusters.Add(new PathCollection(new Path(new List<Node>() {
				new Node(2, 43, 0),
				new Node(19, 59, 1504),
				new Node(31, 57, 1546),
				new Node(53, 49, 1572),
				new Node(55, 3, 1646)
			}))); // top
			allClusters.Add(new PathCollection(new Path(new List<Node>() {
				new Node(2, 43, 0),
				new Node(13, 35, 717),
				new Node(50, 38, 798),
				new Node(55, 3, 855)
			}))); // 2nd
			allClusters.Add(new PathCollection(new Path(new List<Node>() {
				new Node(2, 43, 0),
				new Node(8, 26, 1433),
				new Node(55, 23, 1627),
				new Node(55, 3, 1659)
			}))); // 3rd
			allClusters.Add(new PathCollection(new Path(new List<Node>() {
				new Node(2, 43, 0),
				new Node(13, 9, 68),
				new Node(31, 14, 87),
				new Node(53, 7, 112),
				new Node(55, 3, 1648)
			}))); // bottom
			
			for (int pathIndex = 0; pathIndex < paths.Count; pathIndex++)
            {
                int nearestCluster = FindNearestCluster(allClusters, paths[pathIndex]);
                allClusters[nearestCluster].Add(paths[pathIndex]);
            }
			
			return allClusters;
		}
		
		// based on https://www.planet-source-code.com/vb/scripts/ShowCode.asp?txtCodeId=7944&lngWId=10
        private static List<PathCollection> initializeCentroids(List<Path> paths, int numClusters)
        {
			numClusters --;
			
			List<PathCollection> clusters = new List<PathCollection>();
			
            bool[] usedPoint = new bool[paths.Count()];

            //Pick 1st centroid at random
            int c1 = new System.Random().Next(0, paths.Count() - 1);
            usedPoint[c1] = true;
			clusters.Add(new PathCollection(paths[c1]));

            //Pick the rest of the centroids
            for (int curK = 0; curK < numClusters; curK++)
            {
                double maxD = 0;
                int bestCentroid = 0;

                //Find a point that is the furthest distance from all current centroids
                for (int curPoint = 0; curPoint < paths.Count(); curPoint++)
                {
                    //Skip if this point is a centroid
                    if (!usedPoint[curPoint])
                    {
                        //Find distance to the closest centroid
                        double minD = double.MaxValue;
                        for (int testK = 0; testK < clusters.Count(); testK++)
                            minD = Math.Min(minD, FindDistance(paths[curPoint], clusters[testK].Centroid));

                        //See if this distance is farther than current farthest point
                        if (maxD < minD)
                        {
                            maxD = minD;
                            bestCentroid = curPoint;
                        }
                    }
                }

                //Set the centroid
                usedPoint[bestCentroid] = true;
				clusters.Add(new PathCollection(paths[bestCentroid]));
            }
			
			for (int pathIndex = 0; pathIndex < paths.Count; pathIndex++)
            {
                int nearestCluster = FindNearestCluster(clusters, paths[pathIndex]);
                clusters[nearestCluster].Add(paths[pathIndex]);
            }
			
			return clusters;
        }
		
        public static List<PathCollection> DoKMeans(List<Path> paths, int clusterCount, int distMetric_, int numPasses)
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
			
			clustTime.Start();

            //divide paths into equal clusters
       //   List<PathCollection> allClusters = usePredeterminedCentroids(paths);
			
       /*   List<PathCollection> allClusters = new List<PathCollection>();

            List<List<Path>> allGroups = ListUtility.SplitList<Path>(paths, clusterCount);
            foreach (List<Path> pathGroup in allGroups)
            {
                PathCollection cluster = new PathCollection(pathGroup);
                allClusters.Add(cluster);
            } */
			
			double bestE = double.MaxValue;
			List<PathCollection> bestClustering = new List<PathCollection>();
						
			for (int curPass = 0; curPass < numPasses; curPass ++)
			{
				List<PathCollection> allClusters = initializeCentroids(paths, clusterCount);
				
	            // loop src : http://codeding.com/articles/k-means-algorithm
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
				
                // E is the sum of the distances between each centroid and that centroids assigned points.
                // The smaller the E value, the better the clustering . . .
                double E = 0;
				foreach (PathCollection cluster in allClusters)
				{
					foreach (Path path in cluster)
					{
						E += FindDistance(path, cluster.Centroid);
					}
				}
				Debug.Log("Pass " + curPass + ", val: " + E);
//                for (int curPoint = 0; curPoint < paths.Count(); curPoint++)
//                    E += EuclideanDistance(data, curPoint, centroids, clusters[curPoint], nDimensions);
                if (E < bestE || curPass == 0)
                {
                    //If we found a better E, update the return variables with the current ones
                    bestE = E;
					
					bestClustering.Clear();
					foreach (PathCollection cluster in allClusters)
					{
						bestClustering.Add(cluster);
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

            return bestClustering;
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