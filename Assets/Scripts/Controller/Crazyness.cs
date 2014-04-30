using System.Collections.Generic;
using UnityEngine;
using Common;
using Exploration;
using Extra;

namespace RRTController {

	public class Crazyness : Controller {

		private float limit;
		private int stepsBehind;

		private Path path = new Path();
		private List<Path> paths = new List<Path>();
		private Node p1, p2;

		public Crazyness (int stepsBehind, float limit) {
			path = new Path();
			paths = new List<Path>();
			path.points = new List<Node>();
			paths.Add(path);
			this.stepsBehind = stepsBehind;
			this.limit = limit;
		}

		public void onStart (Node start, RRTKDTreeCombat context) {
		}
		
		public bool afterSample (Node closest, Node sampled, RRTKDTreeCombat context) {
			return true;
		}
		
		public List<Cell[][][]> beforeLineOfSight (Node from, Node to, RRTKDTreeCombat context) {
			return null;
		}
		
		public bool validateLineOfSight (Node from, Node to, Node hit, RRTKDTreeCombat context) {
			path.points.Clear();
			path.points.Add(from);
			path.points.Add(to);

			p1 = from.parent;
			p2 = to.parent;
			from.parent = null;
			to.parent = from;

			Analyzer.PreparePaths(paths);
			Analyzer.ComputeCrazyness(paths, context.nodeMatrix, stepsBehind);

			from.parent = p1;
			to.parent = p2;

			if (path.crazy > limit) {
				Debug.Log("Limit reached!");
				return false;
			}

			return true;
		}
		
		public Node beforeConnect (Node from, Node to, Node hit, RRTKDTreeCombat context) {
			return null;
		}
		
		public Node afterConnect (Node from, Node to, RRTKDTreeCombat context) {
			return null;
		}
	}
}

