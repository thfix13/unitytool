//PathCollection.cs
//modified from source at http://codeding.com/articles/k-means-algorithm

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Path = Common.Path;

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

        public Path RemovePath(int index)
        {
            Path removedPath = new Path(this[index].points);
            this.RemoveAt(index);
            UpdateCentroid();

            return (removedPath);
        }

        public Path removePath(Path p)
        {
            Path removedPath = new Path(p.points);
            this.Remove(p);
            UpdateCentroid();

            return (removedPath);
        }

        public Path GetPathNearestToCentroid()
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
        }

        #endregion

        #region Internal-Methods

        public void UpdateCentroid()
        {
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
			
			// TODO-find different way of computing centroid
			
			double pathTotalMinDist = 1000000;
			int pIndex = -1;
			for (int i = 0; i < this.Count; i ++)
			{
				double currentPathTotalMinDist = 0;
				for (int j = 0; j < this.Count; j ++)
				{
					if (i == j) continue;

					Debug.Log("uc");
					currentPathTotalMinDist += KMeans.FindDistance(this[i], this[j]);
				}
				if (currentPathTotalMinDist < pathTotalMinDist)
				{
					currentPathTotalMinDist = pathTotalMinDist;
					pIndex = i;
				}
			}
			
			if (pIndex == -1) Debug.Log("-1");
			else Debug.Log("NOT-1");
//			foreach(Path p in this[pIndex])
//			{
				Centroid = new Path(this[pIndex].points);
//			}
			Debug.Log("c new path count: " + Centroid.points.Count);
        }

        #endregion
    }
}
