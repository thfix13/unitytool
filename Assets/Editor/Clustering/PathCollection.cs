//PathCollection.cs
//modified from source at http://codeding.com/articles/k-means-algorithm

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Path = Common.Path;
using Node = Common.Node;
using EditorArea;

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
			
            double xSum = (from p in this select p.X).Sum();
            double ySum = (from p in this select p.Y).Sum();
            Centroid.X = (xSum / (double)this.Count);
            Centroid.Y = (ySum / (double)this.Count); */
			
			if (MapperWindowEditor.altCentroidComp)
			{
				Node[] nodes = new Node[this[0].points.Count()];
				for (int count = 0; count < this[0].points.Count(); count ++)
				{
					nodes[count] = new Node(0, 0, 0);
				}
				foreach (Path p in this)
				{
					for (int count = 0; count < p.points.Count; count ++)
					{
						nodes[count].x += p.points[count].x;
						nodes[count].y += p.points[count].y;
						nodes[count].t += p.points[count].t;
					}
				}
				foreach(Node n in nodes)
				{
					n.x /= this[0].points.Count;
					n.y /= this[0].points.Count;
					n.t /= this[0].points.Count;
				}
			
				Centroid = new Path(new List<Node>(nodes));
			}
			else
			{
				double pathTotalMinDist = double.PositiveInfinity;
				int pIndex = -1;
				for (int i = 0; i < this.Count; i ++)
				{
					double currentPathTotalMinDist = 0;
					for (int j = 0; j < this.Count; j ++)
					{
						if (i == j) continue;

						currentPathTotalMinDist += KMeans.FindDistance(this[i], this[j]);
					}
					if (currentPathTotalMinDist < pathTotalMinDist)
					{
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
			}
        }
		
		public Path getCenterDistPath()
		{
			double pathTotalMinDist = double.PositiveInfinity;
			int pIndex = -1;
			for (int i = 0; i < this.Count; i ++)
			{
				double currentPathTotalMinDist = 0;
				for (int j = 0; j < this.Count; j ++)
				{
					if (i == j) continue;

					currentPathTotalMinDist += KMeans.FindDistance(this[i], this[j]);
				}
				if (currentPathTotalMinDist < pathTotalMinDist)
				{
					pathTotalMinDist = currentPathTotalMinDist;
					pIndex = i;
				}
			}

			if (pIndex == -1)
			{
				Debug.Log("-1");
				return null;
			}

			return new Path(this[pIndex].points);
		}

        #endregion
    }
}
