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
		
		public bool changed = false;

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
		
		public PathCollection(Path centroid_) : base()
		{
			Centroid = centroid_;
		}

        #endregion

        #region Methods

        public void AddPath(Path p)
        {
            this.Add(p);
			changed = true;
        }

        public Path RemovePath(Path p)
        {
            Path removedPath = new Path(p);
            this.Remove(p);
			changed = true;

            return (removedPath);
        }

        #endregion

        #region Internal-Methods

		public Path getAveragedCentroid()
		{
			// first, make a copy of all the paths in the cluster...
			List<Path> interpolatedPaths = new List<Path>();
			foreach (Path p in this)
			{
				interpolatedPaths.Add(new Path(p)); // make a copy
			}

			double maxTime = Double.NegativeInfinity;
			foreach (Path p in this)
			{ // find the highest time value over all paths in this cluster
				foreach (Node n in p.points)
				{
					if (n.t > maxTime)
					{
						maxTime = n.t;
					}
				}
			}
			
			for (int i = 0; i < interpolatedPaths.Count; i ++)
			{ // make each path have same # of points
				Vector3[] set1 = MapperWindowEditor.GetSetPointsWithN(interpolatedPaths[i].getPoints3D(), (int)(Math.Sqrt(maxTime)), false);
	//			Debug.Log("Paths now have " + Math.Sqrt(maxTime) + " points.");
				interpolatedPaths[i].points = new List<Node>();
				foreach(Vector3 v in set1)
				{
					if (v.x == 0 && v.y == 0 && v.z == 0) continue;
					interpolatedPaths[i].points.Add(new Node((int)v.x, (int)v.z, (int)v.y));
				}
			}
			
//			return interpolatedPaths[0];
			
			Node[] averagedNodes = new Node[interpolatedPaths[0].points.Count()];
			for (int count = 0; count < interpolatedPaths[0].points.Count(); count ++)
			{
				averagedNodes[count] = new Node(0, 0, 0);
				if (count > 0) averagedNodes[count].parent = averagedNodes[count-1];
			}
            float avgDanger = 0f, avgLOS = 0f, avgNM = 0f;
			foreach (Path p in interpolatedPaths)
			{
                avgDanger += p.danger3;
                avgLOS += p.los3;
                avgNM += p.crazy;
				for (int count = 0; count < p.points.Count; count ++)
				{
					averagedNodes[count].x += Math.Abs(p.points[count].x);
					averagedNodes[count].y += Math.Abs(p.points[count].y);
					averagedNodes[count].t += Math.Abs(p.points[count].t);
				}
			}
//			for (int count = 0; count < interpolatedPaths[0].points.Count(); count ++)
//			{
//				Debug.Log("cnx:"+averagedNodes[count].x+",y:"+averagedNodes[count].y+",t:"+averagedNodes[count].t);
//			}
			foreach(Node n in averagedNodes)
			{
				n.x /= interpolatedPaths.Count;
				n.y /= interpolatedPaths.Count;
				n.t /= interpolatedPaths.Count;
			}
            avgDanger /= interpolatedPaths.Count;
            avgLOS /= interpolatedPaths.Count;
            avgNM /= interpolatedPaths.Count;
		//	Debug.Log("end centr");
        
            Path averagedPath = new Path(new List<Node>(averagedNodes));
            averagedPath.danger3 = avgDanger;
            averagedPath.los3 = avgLOS;
            averagedPath.crazy = avgNM;
		
			return averagedPath;
		}

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
			
	/*		if (MapperWindowEditor.altCentroidComp)
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
			//		Debug.Log("cnx:"+n.x+",y:"+n.y+",t:"+n.t);
				}
			//	Debug.Log("end centr");
			
				Centroid = new Path(new List<Node>(nodes));
			}*/
	//		else
	//		{
				double pathTotalMinDist = double.PositiveInfinity;
				int pIndex = -1;
				for (int i = 0; i < this.Count; i ++)
				{
					if (KMeans.weights.Count() < Convert.ToInt32(this[i].name))
					{
						Debug.Log("KMeans.weights size: " + KMeans.weights.Count() + " but need index " + Convert.ToInt32(this[i].name));
					}
					double weightOfI = KMeans.weights[Convert.ToInt32(this[i].name)];
					double currentPathTotalMinDist = 0;
					for (int j = 0; j < this.Count; j ++)
					{
						if (i == j) continue;

						currentPathTotalMinDist += (weightOfI * KMeans.FindDistance(this[i], this[j]));
//						currentPathTotalMinDist += (KMeans.FindDistance(this[i], this[j]));
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
			
				Centroid = new Path(this[pIndex]);
			}
 //       }
		
	/*	public Path getCenterDistPath()
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
		}*/

        #endregion
    }
}
