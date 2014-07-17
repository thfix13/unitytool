using System.Collections.Generic;
using UnityEngine;
using Common;
using Exploration;
using Extra;

namespace RRTController {

	public class Danger : Controller {

		private float limit;

		private Path path = new Path();
		private List<Path> paths = new List<Path>();
		private Node p1, p2;

		public Danger (float limit) {
			path = new Path();
			paths = new List<Path>();
			path.points = new List<Node>();
			paths.Add(path);
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

			path.ZeroValues();
			Analyzer.ComputePathsDangerValues(paths, context.enemies, context.min, context.tileSizeX, context.tileSizeZ, context.nodeMatrix);

			from.parent = p1;
			to.parent = p2;

			if (path.danger3 > limit) {
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

