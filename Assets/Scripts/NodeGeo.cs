using System;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using Objects;

namespace Common {
	[Serializable]
	// Structure that holds the information used in the AStar cells
		public class NodeGeo : Priority_Queue.PriorityQueueNode {
		public float x, y;
		public int t;
		public float playerhp;
		public int distractTime = -1;
		public List<int> distractTimes = new List<int>();
		[XmlIgnore]
		public Dictionary<Enemy, float> enemyhp;
		[XmlIgnore]
		public List<Enemy> fighting;
		[XmlIgnore]
		public Enemy died;
		[XmlIgnore]
		public NodeGeo parent;
		//[XmlIgnore]
		//public Cell cell;
		[XmlIgnore]
		public bool visited = false;
		[XmlIgnore]
		public double accSpeed = 0.0d;
		[XmlIgnore]
		public List<HealthPack> picked;

		public float DistanceFrom (NodeGeo n) {
			Vector2 v1, v2;
			v1 = new Vector2 (this.x, this.y);
			v2 = new Vector2 (n.x, n.y);

			return (v1 - v2).magnitude + Mathf.Abs (this.t - n.t);
		}

		public Vector2 GetVector2 () {
			return new Vector2 (x, y);	
		}

		public Vector3 GetVector3 () {
			return new Vector3 (x, t, y);	
		}
		public Vector3 GetVector3Draw () 
		{
			return new Vector3 (x, 1, y);	
		}
		
		public double[] GetArray () {
			return new double[] {x, t, y};
		}

		//TODO: Add distract time to this
		public Boolean equalTo (NodeGeo b) {
			if (Mathf.Approximately(this.x,b.x) & Mathf.Approximately(this.y,b.y) & this.t == b.t)
				return true;
			return false; 
		}
		//TODO: Add distract time to this
		public override string ToString () {
			if(distractTimes.Count > 0){
				return t + "-" + x + "-" + y + "-" + distractTimes[0];
			}
			return t + "-" + x + "-" + y + "-" + distractTime;
		}
		//TODO: Add distract time to this
		public float Axis (int axis) {
			switch (axis) {
			case 0:
				return x;
			case 1:
				return t;
			case 2:
				return y;
			default:
				throw new ArgumentException ();
			}
		}
	}
}