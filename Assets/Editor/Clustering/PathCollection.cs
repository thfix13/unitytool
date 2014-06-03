//PathCollection.cs
//modified from source at http://codeding.com/articles/k-means-algorithm

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Path = Common.Path;
using Node = Common.Node;

using UnityEngine;
namespace ClusteringSpace
{
    public class PathCollection : List<Path>
    {
        #region Properties

        public Path Centroid { get; set; }

        #endregion

        #region Constructors

        public PathCollection()
            : base()
        {
            Centroid = new Path();
        }
		
        public PathCollection(List<Path> paths)
            : base()
        {
            Centroid = new Path();
			
			foreach (Path p in paths)
			{
				this.Add(p);
			}
            UpdateCentroid();
        }

        #endregion

        #region Methods

        public void AddPath(Path p)
        {
            this.Add(p);
            UpdateCentroid();
        }

        public Path removePath(Path p)
        {
            Path removedPath = new Path(p.points);
            this.Remove(p);
            UpdateCentroid();

            return (removedPath);
        }

/*        public Path GetPathNearestToCentroid()
        {
            double minimumDistance = 0.0;
            int nearestPathIndex = -1;

            foreach (Path p in this)
            {

				Debug.Log("GPNTC");
                double distance = KMeans.FindDistance(p, Centroid);

                if (this.IndexOf(p) == 0)
                {
                    minimumDistance = distance;
                    nearestPathIndex = this.IndexOf(p);
                }
                else
                {
                    if (minimumDistance > distance)
                    {
                        minimumDistance = distance;
                        nearestPathIndex = this.IndexOf(p);
                    }
                }
            }

            return (this[nearestPathIndex]);
        }*/

        #endregion

        #region Internal-Methods

        public void UpdateCentroid()
        {
	//		System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
	//		watch.Start();
			
		/*	double xSum = 0.0;
			double ySum = 0.0;
			
			for (int i = 0; i < this.Count; i ++)
			{
				double xSump = 0.0;
				double ySump = 0.0;
				for (int j = 0; j < this[i].points.Count; j++) xSump += this[i].points[j].x;
				for (int j = 0; j < this[i].points.Count; j++) ySump += this[i].points[j].y;
				
				xSum += (xSump / this[i].points.Count);
				ySum += (ySump / this[i].points.Count);
			}
			
    //        double xSum = (from p in this select p.X).Sum();
      //      double ySum = (from p in this select p.Y).Sum();
            Centroid.X = (xSum / (double)this.Count);
            Centroid.Y = (ySum / (double)this.Count);*/
			
	/*		int maxPathLength = 0;
			foreach (Path p in this)
			{
				if (p.points.Count > maxPathLength)
				{
					maxPathLength = p.points.Count;
				}
			}
			
			Node[] nodes = new Node[maxPathLength];
			for (int count = 0; count < maxPathLength; count ++)
			{
				nodes[count] = new Node();
				nodes[count].x = nodes[count].y = 0;
			}
			foreach (Path p in this)
			{
				for (int count = 0; count < maxPathLength; count ++)
				{
					if (count < p.points.Count)
					{ // has a node at that position
						nodes[count].x += (p.points[count].x / maxPathLength);
						nodes[count].y += (p.points[count].y / maxPathLength);
					}
				}
			}
			Centroid = new Path(new List<Node>(nodes));*/
			
			// TODO-find different way of computing centroid
			
			double pathTotalMinDist = double.PositiveInfinity;
			int pIndex = -1;
			for (int i = 0; i < this.Count; i ++)
			{
				double currentPathTotalMinDist = 0;
				for (int j = 0; j < this.Count; j ++)
				{
					if (i == j) continue;

		//			Debug.Log("uc");
					currentPathTotalMinDist += KMeans.FindDistance(this[i], this[j]);
				}
				if (currentPathTotalMinDist < pathTotalMinDist)
				{
					//currentPathTotalMinDist = pathTotalMinDist;
					pathTotalMinDist = currentPathTotalMinDist;
					pIndex = i;
				}
			}
			
			if (pIndex == -1)
			{
				Debug.Log("-1");
				Centroid = null;
				return;
			}
			
			Centroid = new Path(this[pIndex].points);
			
//			watch.Stop();
//			Debug.Log("UC elapsed time: " + watch.Elapsed);
        }

        #endregion
    }
}
