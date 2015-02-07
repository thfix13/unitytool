using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Path = Common.Path;
using Node = Common.Node;

namespace ClusteringSpace
{
	public enum Metrics
	{
		Frechet = 0,
		AreaDistTriangulation,
		AreaDistInterpolation3D,
		Hausdorff
	}
	
	public enum Dimensions
	{
		X = 0,
		Y,
		Time,
		Danger,
		LOS,
		NearMiss
	}
	
	public class Clustering
	{
		public static double[][] distances;
		public static Path[] normalizedPaths;
	
		public static void initWithPaths(List<Path> paths, bool normalize)
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

			if (normalize)
				normalizePaths(paths);
			else
			{
				for (int count = 0; count < paths.Count(); count ++)
				{
					normalizedPaths[count] = paths[count];
				}
			}
		}
	
		public static void reset()
		{
			if (distances != null)
				Array.Clear(distances, 0, distances.Count());
			if (normalizedPaths != null)
				Array.Clear(normalizedPaths, 0, normalizedPaths.Count());
		}
		
		private static void normalizePaths(List<Path> paths)
		{
			Array.Clear(normalizedPaths, 0, normalizedPaths.Count());
			// get max vals for x,y,t,d3norm,los3norm,crazy.
			float maxX = -1f, maxY = -1f, maxT = -1f, maxD3 = -1f, maxLOS3 = -1f, maxCrazy = -1f, maxNodeD3 = -1f;
			float minX = Single.PositiveInfinity, minY = Single.PositiveInfinity, minT = Single.PositiveInfinity, minD3 = Single.PositiveInfinity, minLOS3 = Single.PositiveInfinity, minCrazy = Single.PositiveInfinity, minNodeD3 = Single.PositiveInfinity;
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
					if (n.x > maxX) maxX = (float)n.xD;
					if (n.y > maxY) maxY = (float)n.yD;
					if (n.t > maxT) maxT = (float)n.tD;
					if (n.danger3 > maxNodeD3) maxNodeD3 = n.danger3;
					
					if (n.x < minX) minX = (float)n.xD;
					if (n.y < minY) minY = (float)n.yD;
					if (n.t < minT) minT = (float)n.tD;
					if (n.danger3 < maxNodeD3) minNodeD3 = n.danger3;
				}					
			}
			
			float maxVal = 1000f;
						
			for (int count = 0; count < paths.Count; count ++)
			{
				int index = Convert.ToInt32(paths[count].name);
				normalizedPaths[index] = new Path(paths[count]);

				normalizedPaths[index].danger3 = ((normalizedPaths[index].danger3 - minD3) * maxVal) / (maxD3 - minD3);
				normalizedPaths[index].los3 = ((normalizedPaths[index].los3 - minLOS3) * maxVal) / (maxLOS3 - minLOS3);
				normalizedPaths[index].crazy = ((normalizedPaths[index].crazy - minCrazy) * maxVal) / (maxCrazy - minCrazy);
			
				foreach (Node n in normalizedPaths[index].points)
				{
					n.xD = (((n.xD - minX) * maxVal) / (maxX - minX));
					n.yD = (((n.yD - minY) * maxVal) / (maxY - minY));
					n.tD = (((n.tD - minT) * maxVal) / (maxT - minT));
					n.danger3 = (((n.danger3 - minNodeD3) * maxVal) / (maxNodeD3 - minNodeD3));
					
					n.x = (int)n.xD;
					n.y = (int)n.yD;
					n.t = (int)n.tD;
				}						
			}
		}
	}
}