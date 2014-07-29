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
			Frechet = 0,
			AreaDistTriangulation,
			AreaDistInterpolation3D
		}
		
		public enum FrechetDimensions
		{
			X = 0,
			Y,
			Time,
			Danger,
			LOS,
			NearMiss
		}
		public static int numSelectedDimensions = -1;
		
		public static Stopwatch distTime = new Stopwatch();
		public static Stopwatch clustTime = new Stopwatch();
		static FrechetDistance frechet;
        
		private static int distMetric = 0;
		
		public static double clustVal = 0.0;
				
		private static double[][] distances;
		public static double[] weights;
		private static bool init = false;
		public static int numPaths = -1;
		
		public static Path[] normalizedPaths;
		
		public static void reset()
		{
			distMetric = -1;
			clustVal = 0.0;
			init = false;
			numPaths = -1;
			numSelectedDimensions = -1;
			if (distances != null)
				Array.Clear(distances, 0, distances.Count());
			if (normalizedPaths != null)
				Array.Clear(normalizedPaths, 0, normalizedPaths.Count());
		}
		
        private static List<PathCollection> initializeCentroids(List<Path> paths, int numClusters, double[] weights_)
        { // based on https://www.planet-source-code.com/vb/scripts/ShowCode.asp?txtCodeId=7944&lngWId=10
			weights = weights_;
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
						{
							minD = Math.Min(minD, FindDistance(paths[curPoint], clusters[testK].Centroid));
						}

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
				clusters.Last().Add(paths[bestCentroid]);
            }
			
			for (int pathIndex = 0; pathIndex < paths.Count; pathIndex++)
            {
                int nearestCluster = FindNearestCluster(clusters, paths[pathIndex]);
				if (!clusters[nearestCluster].Contains(paths[pathIndex]))
				{
					clusters[nearestCluster].AddPath(paths[pathIndex]);
				}
            }
			
			foreach(PathCollection cluster in clusters)
			{
				cluster.UpdateCentroid();
				cluster.changed = false;
			}
			
			return clusters;
        }
		
		private static List<PathCollection> initializeCentroidsScalable(List<Path> paths, int numClusters)
		{ // based on Scalable K-Means++, Bahmani et al. http://theory.stanford.edu/~sergei/papers/vldb12-kmpar.pdf
			numClusters --;
			
			List<Path> centroids = new List<Path>();
			
            bool[] usedPoint = new bool[paths.Count()];

            //Pick 1st centroid at random
            int c1 = new System.Random().Next(0, paths.Count() - 1);
            usedPoint[c1] = true;
			centroids.Add(paths[c1]);
				
			double oversamplingFactor = numClusters*2 + 5;
		
			double initialClustCost = getClustCost(paths, centroids);
			double currentClustCost = initialClustCost;
			
            int numIterations = (int)Math.Log(initialClustCost);
            
			for (int count = 0; count < numIterations; count ++)
			{
				for (int pathCount = 0; pathCount < paths.Count(); pathCount ++)
				{ // sample each point with probability...
					if (usedPoint[pathCount]) continue;
					
					double minDist = Double.PositiveInfinity;
					foreach (Path c in centroids)
					{
						double dist = Math.Pow(FindDistance(paths[pathCount], c), 2);
						if (dist < minDist)
						{
							minDist = dist;
						}
					}
					
					double pathProbability = oversamplingFactor * minDist / currentClustCost;
					double chance = new System.Random().NextDouble();
					
					if (chance <= pathProbability)
					{ // if sampled, add to centroid list
						centroids.Add(paths[pathCount]);
						usedPoint[pathCount] = true;
					}
				}
				
				// recalculate cluster cost.
				currentClustCost = getClustCost(paths, centroids);
			}
			
			double[] centroidWeights = new double[numPaths];
			Debug.Log("Weights array has size" + centroidWeights.Count());
			foreach (Path p in paths)
			{
				double minDist = Double.PositiveInfinity;
				int minIndex = -1;
				for (int c = 0; c < centroids.Count(); c ++)
				{
					double dist = FindDistance(p, centroids[c]);
					if (dist < minDist)
					{
						minDist = dist;
						minIndex = c;
					}
				}
				
				centroidWeights[minIndex] ++;
			}
			
			for (int i = 0; i < numPaths; i ++)
			{
				if (centroidWeights[i] == 0) centroidWeights[i] = 1;
				else centroidWeights[i] = 1.0 / centroidWeights[i];
			}
			
			Debug.Log("Weights array now has size" + centroidWeights.Count());
			List<PathCollection> clusters = cluster(initializeCentroids(centroids, numClusters+1, centroidWeights), centroidWeights);
			for (int pathIndex = 0; pathIndex < paths.Count; pathIndex++)
            {
                int nearestCluster = FindNearestCluster(clusters, paths[pathIndex]);
				if (!clusters[nearestCluster].Contains(paths[pathIndex]))
					clusters[nearestCluster].Add(paths[pathIndex]);
            }
			
			foreach(PathCollection cl in clusters)
			{
				cl.UpdateCentroid();
				cl.changed = false;
			}

			return clusters;
		}
		
		public static double getClustCost(List<Path> paths, List<Path> centroids)
		{
			double sumMinDists = 0.0;
			
			foreach (Path p in paths)
			{
				double minDist = Double.PositiveInfinity;
				foreach (Path c in centroids)
				{
					double dist = Math.Pow(FindDistance(p, c), 2);
					if (dist < minDist)
					{
						minDist = dist;
					}
				}
				sumMinDists += minDist;
			}
			
			return sumMinDists;
		}

        public static List<PathCollection> DoKMeans(List<Path> paths, int clusterCount, int distMetric_, int numPasses)
		{
			double[] weights = new double[paths.Count()];
			for (int i = 0; i < paths.Count(); i ++)
			{
				weights[i] = 1.0;
			}
			numPaths = paths.Count();
			return DoKMeans(paths, clusterCount, distMetric_, numPasses, weights);
		}
		
        public static List<PathCollection> DoKMeans(List<Path> paths, int clusterCount, int distMetric_, int numPasses, double[] weights)
        {
			if (paths.Count == 0)
			{
				Debug.Log("No paths to cluster!");
				return null;
			}
			
			setDistMetric(distMetric_);
			
			if (!init)
			{
				distances = new double[paths.Count()][];
				for (int count = 0; count < paths.Count(); count ++)
				{
					distances[count] = new double[paths.Count()];
					for (int count2 = 0; count2 < paths.Count(); count2 ++)
					{
						distances[count][count2] = -1;
					}
				}
				
				normalizedPaths = new Path[paths.Count()];
				// get max vals for x,y,t,d3norm,los3norm,crazy.
				float maxX = -1f, maxY = -1f, maxT = -1f, maxD3 = -1f, maxLOS3 = -1f, maxCrazy = -1f;
				float minX = Single.PositiveInfinity, minY = Single.PositiveInfinity, minT = Single.PositiveInfinity, minD3 = Single.PositiveInfinity, minLOS3 = Single.PositiveInfinity, minCrazy = Single.PositiveInfinity;
				foreach (Path p in paths)
				{
					if (p.danger3 > maxD3) maxD3 = p.danger3;
					if (p.los3 > maxLOS3) maxLOS3 = p.los3;
					if (p.crazy > maxCrazy) maxCrazy = p.crazy;
					
					if (p.danger3 < minD3) minD3 = p.danger3;
					if (p.los3 < minLOS3) minLOS3 = p.los3;
					if (p.crazy < minCrazy) minCrazy = p.crazy;
					
					foreach (Node n in p.points)
					{
						if (n.x > maxX) maxX = n.x;
						if (n.y > maxY) maxY = n.y;
						if (n.t > maxT) maxT = n.t;
						
						if (n.x < minX) minX = n.x;
						if (n.y < minY) minY = n.y;
						if (n.t < minT) minT = n.t;
					}					
				}
				
				float maxVal = 1000f;
				
				for (int count = 0; count < paths.Count; count ++)
				{
					int index = Convert.ToInt32(paths[count].name);
					normalizedPaths[index] = new Path(paths[count]);

					if (numSelectedDimensions > 1)
					{ // normalize data.
						normalizedPaths[index].danger3 = ((normalizedPaths[index].danger3 - minD3) * maxVal) / (maxD3 - minD3);
	//					Debug.Log("Path " + index + ": " + normalizedPaths[index].danger3Norm);
						normalizedPaths[index].los3 = ((normalizedPaths[index].los3 - minLOS3) * maxVal) / (maxLOS3 - minLOS3);
						normalizedPaths[index].crazy = ((normalizedPaths[index].crazy - minCrazy) * maxVal) / (maxCrazy - minCrazy);
					
						foreach (Node n in normalizedPaths[index].points)
						{
							n.x = (int)(((n.x - minX) * maxVal) / (maxX - minX));
							n.y = (int)(((n.y - minY) * maxVal) / (maxY - minY));
							n.t = (int)(((n.t - minT) * maxVal) / (maxT - minT));
						}						
					}
				}
				
				init = true;
			}
						
			for (int count = 0; count < paths.Count(); count ++)
			{
				if (MapperWindowEditor.scaleTime)
				{
					foreach (Node n in paths[count].points)
					{
						n.t = (int)Math.Pow(n.t, 3);
					}
				}
			}

			clustTime.Start();
			
			double bestE = double.MaxValue;
			List<PathCollection> bestClustering = new List<PathCollection>();
						
			for (int curPass = 0; curPass < numPasses; curPass ++)
			{
				Debug.Log("Pass " + curPass);

				List<PathCollection> allClusters = new List<PathCollection>();
				if (MapperWindowEditor.useScalable)
					allClusters = cluster(initializeCentroidsScalable(paths, clusterCount), KMeans.weights);
				else
					allClusters = cluster(initializeCentroids(paths, clusterCount, weights), weights);
				
				// Davies-Bouldin index calculation
				// ref : http://en.wikipedia.org/wiki/Cluster_analysis#Evaluation_and_assessment
				// ref : http://en.wikipedia.org/wiki/Davies–Bouldin_index
				
				// get, for each cluster, the average distance of all elements in that cluster to the cluster centroid
				
				Path[] averageCentroids = new Path[allClusters.Count()];
				for (int i = 0; i < allClusters.Count(); i ++)
				{
					averageCentroids[i] = allClusters[i].getAveragedCentroid();
					int newIndex = paths.Count() + 1000 + i;
                    averageCentroids[i].name = newIndex.ToString();
//					averageCentroids[i] = allClusters[i].Centroid;
				}
				
				double[] avgDists = new double[allClusters.Count()];
				for (int i = 0; i < allClusters.Count(); i ++)
				{
					avgDists[i] = 0.0;
					foreach (Path p in allClusters[i])
					{
						avgDists[i] += FindDistance(p, averageCentroids[i]);
					}
				}
				
				double sumMaxVals = 0.0;
				for (int i = 0; i < allClusters.Count(); i ++)
				{ // sum over all clusters
					double maxVal = 0.0;					

					for (int j = 0; j < allClusters.Count(); j ++)
					{
						if (i == j) continue;
						
                        double distBetweenCentroids = FindDistance(averageCentroids[i], averageCentroids[j]);
                        double clustVal = 0.0;
                        if (avgDists[i] + avgDists[j] != 0.0) clustVal = (avgDists[i] + avgDists[j]) / distBetweenCentroids;
						
                        if (Double.IsInfinity(clustVal))
                        {
                            clustVal = 10000.0;
                        }
                        
						if (clustVal > maxVal)
						{
							maxVal = clustVal;
						}
					}
					
					sumMaxVals += maxVal;
				}
				double clusteringQuality = sumMaxVals / allClusters.Count(); // sum * 1/n
				
           /*   // E is the sum of the distances between each centroid and that centroids assigned points.
                // The smaller the E value, the better the clustering . . .
                double clusteringQuality = 0.0;
				foreach (PathCollection c in allClusters)
				{
					foreach (Path path in c)
					{
						clusteringQuality += FindDistance(path, c.Centroid);
					}
				} */
				
				Debug.Log("Pass " + curPass + ", DB val: " + clusteringQuality);
				if (clusteringQuality <= 0)
				{
					Debug.Log("Something has gone horribly wrong.");
				}
                else if (clusteringQuality < bestE || curPass == 0)
                {
                    //If we found a better E, update the return variables with the current ones
                    bestE = clusteringQuality;
					clustVal = bestE;
					
					bestClustering.Clear();
					foreach (PathCollection c in allClusters)
					{
						bestClustering.Add(c);
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
		
		public static List<PathCollection> cluster(List<PathCollection> allClusters, double[] weights_)
		{ // based on http://codeding.com/articles/k-means-algorithm
			for (int i = 0; i < allClusters.Count(); i ++)
			{
				if (allClusters[i].Count() == 0) Debug.Log("Empty cluster, #" + i);
			}
			weights = weights_;

            int movements = 1;
			int count = 0;
			int[] previousMovements = new int[100];
            while (movements > 0)
            {
				Debug.Log(count);
				previousMovements[count] = movements;
				if (count > 25)
				{
					int avgLastThree = (previousMovements[count-2] + previousMovements[count-1] + previousMovements[count]) / 3;
					if (Math.Abs(avgLastThree - previousMovements[count]) <= 10)
					{
						Debug.Log("Not converging.");
						break;
					}
				}
			
				count ++;
		//		MapperWindowEditor.updatePaths(allClusters);
			
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
								Path removedPath = allClusters[clusterIndex].RemovePath(path);
                                allClusters[nearestCluster].AddPath(removedPath);
                                movements += 1;
                            }
                        }
                    }
                }
				foreach(PathCollection cluster in allClusters)
				{
					if (cluster.changed)
					{
						cluster.UpdateCentroid();
						movements += 1;
						cluster.changed = false;
					}
				}
            }
			
			return allClusters;
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
			
			if (distMetric == (int)Metrics.Frechet)
			{
				numSelectedDimensions = 0;
				for (int dim = 0; dim < MapperWindowEditor.dimensionEnabled.Count(); dim ++)
				{
					if (MapperWindowEditor.dimensionEnabled[dim])
					{
						numSelectedDimensions ++;
					}
				}
				
				frechet = new PolyhedralFrechetDistance(PolyhedralDistanceFunction.L1(numSelectedDimensions));
			}
			
//			else if (distMetric == (int)Metrics.FrechetEuclidean)
//			{
//				frechet = new PolyhedralFrechetDistance(PolyhedralDistanceFunction.epsApproximation2D(1.1));
//			}
		}
				
		public static double FindDistance(Path path1, Path path2, int distMetric_)
		{
			setDistMetric(distMetric_);
			return FindDistance(path1, path2);
		}
		
        public static double FindDistance(Path path1_, Path path2_)
        {
			if (path1_.points == null) { Debug.Log("P1NULL"); return -1; }
			else if (path2_.points == null) { Debug.Log("P2NULL"); return -1; }
			
			if (path1_.name == path2_.name)
			{ // same path
				return 0.0;
			}
			
			int p1num = Convert.ToInt32(path1_.name);
			int p2num = Convert.ToInt32(path2_.name);
			
			Path path1, path2;
			
			bool saveDistances = false;
			if (p1num <= distances.Count() && p2num <= distances.Count())
			{
				if (distances[p1num][p2num] != -1) return distances[p1num][p2num];
				if (distances[p2num][p1num] != -1) return distances[p2num][p1num];
				
				path1 = normalizedPaths[p1num];
				path2 = normalizedPaths[p2num];
				
				saveDistances = true;
			}
			else
			{
				Debug.Log("Note - not storing this distance.");
				
				path1 = path1_;
				path2 = path2_;
			}
			
			clustTime.Stop();
			distTime.Start();

			double result = 0.0;
			
			if (distMetric == (int)Metrics.Frechet)
			{
				// source : http://www.win.tue.nl/~wmeulema/implementations.html
				// Implementation by Wouter Meulemans
				// based on paper by Buchin et al
				// http://arxiv.org/abs/1306.5527
				
				double[][] curveA = new double[path1.points.Count][];
				double[][] curveB = new double[path2.points.Count][];
				
				for (int i = 0; i < path1.points.Count; i ++)
				{
					double[] curve = new double[numSelectedDimensions];
					int curvePos = 0;
					for (int j = 0; j < MapperWindowEditor.dimensionEnabled.Count(); j ++)
					{
						if (MapperWindowEditor.dimensionEnabled[j])
						{
							if (j == (int)FrechetDimensions.X) curve[curvePos] = path1.points[i].x;
							else if (j == (int)FrechetDimensions.Y) curve[curvePos] = path1.points[i].y;
							else if (j == (int)FrechetDimensions.Time) curve[curvePos] = path1.points[i].t;
							else if (j == (int)FrechetDimensions.Danger) curve[curvePos] = path1.danger3;//+(path1.danger3/(10+i));
							else if (j == (int)FrechetDimensions.LOS) curve[curvePos] = path1.los3;//+(path1.los3/(10+i));
							else if (j == (int)FrechetDimensions.NearMiss) curve[curvePos] = path1.crazy;//+(path1.crazy/(10+i));
							curvePos ++;
						}
					}
					curveA[i] = curve;
				}
				
				for (int i = 0; i < path2.points.Count; i ++)
				{
					double[] curve = new double[numSelectedDimensions];
					int curvePos = 0;
					for (int j = 0; j < MapperWindowEditor.dimensionEnabled.Count(); j ++)
					{
						if (MapperWindowEditor.dimensionEnabled[j])
						{
							if (j == (int)FrechetDimensions.X) curve[curvePos] = path2.points[i].x;
							else if (j == (int)FrechetDimensions.Y) curve[curvePos] = path2.points[i].y;
							else if (j == (int)FrechetDimensions.Time) curve[curvePos] = path2.points[i].t;
							else if (j == (int)FrechetDimensions.Danger) curve[curvePos] = path2.danger3;//+(path2.danger3/(10+i));
							else if (j == (int)FrechetDimensions.LOS) curve[curvePos] = path2.los3;//+(path2.los3/(10+i));
							else if (j == (int)FrechetDimensions.NearMiss) curve[curvePos] = path2.crazy;//+(path2.crazy/(10+i));
							curvePos ++;
						}
					}
					curveB[i] = curve;
				}
				
				result = frechet.computeDistance(curveA, curveB);
			}
			else if (distMetric == (int)Metrics.AreaDistTriangulation || distMetric == (int)Metrics.AreaDistInterpolation3D)
			{
				result = AreaDist.computeDistance(path1, path2, distMetric);
			}
			else
			{
				Debug.Log("Invalid distance metric ("+distMetric+")!");
				return -1;
			}
			
			distTime.Stop();
			clustTime.Start();
			
			if (saveDistances)
			{
				distances[p1num][p2num] = result;
				distances[p2num][p1num] = result;
			}
			
//            Debug.Log("Dist between " + path1.danger3 + " and " + path2.danger3 + " is " + result);
            
			return result;
        }
    }
}