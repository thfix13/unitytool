using System;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using Objects;

namespace Common {
	[Serializable]
	// Structure that holds the information used in the AStar cells
		public class Node : Priority_Queue.PriorityQueueNode {
		public int x, y, t;
		public float playerhp;
		[XmlIgnore]
		public Dictionary<Enemy, float> enemyhp;
		[XmlIgnore]
		public List<Enemy> fighting;
		[XmlIgnore]
		public Enemy died;
		[XmlIgnore]
		public Node parent;
		[XmlIgnore]
		public Cell cell;
		[XmlIgnore]
		public bool visited = false;
		[XmlIgnore]
		public double accSpeed = 0.0d;
		[XmlIgnore]
		public List<HealthPack> picked;

		public float DistanceFrom (Node n) {
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

		public double[] GetArray () {
			return new double[] {x, t, y};
		}

		public Boolean equalTo (Node b) {
			if (this.x == b.x & this.y == b.y & this.t == b.t)
				return true;
			return false; 
		}

		public override string ToString () {
			return t + "-" + x + "-" + y;
		}

		public int Axis (int axis) {
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

		/// <summary>
		/// Warning: this is a light clone, so objects references are still the same from the source object.
		/// </summary>
		public Node Copy () {
			Node copy = new Node ();
			copy.cell = this.cell;
			copy.x = this.x;
			copy.y = this.y;
			copy.t = this.t;
			copy.playerhp = this.playerhp;
			copy.enemyhp = this.enemyhp;
			copy.fighting = this.fighting;
			copy.died = this.died;
			copy.parent = this.parent;
			copy.cell = this.cell;
			copy.visited = this.visited;
			copy.accSpeed = this.accSpeed;
			copy.picked = this.picked;
			return copy;
		}
	}
}