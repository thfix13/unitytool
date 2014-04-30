using Common;
using Exploration;
using System.Collections.Generic;
using Objects;
using UnityEngine;

namespace RRTController {

	public class Basic : Controller {

		private float angle;
		private Vector3 p1, p2, pd;

		public Basic () {
		}

		public void onStart (Node start, RRTKDTreeCombat context) {
			float tan = context.playerSpeed / 1;
			angle = 90f - Mathf.Atan (tan) * Mathf.Rad2Deg;
		}

		public bool afterSample (Node closest, Node sampled, RRTKDTreeCombat context) {
			if (sampled.cell.blocked) // Skip impossible cells
				return false;

			if (closest.t > sampled.t) // Skip downwards movements
				return false;

			p1 = sampled.GetVector3 ();
			p2 = closest.GetVector3 ();
			pd = p1 - p2;
			if (Vector3.Angle (pd, new Vector3 (pd.x, 0f, pd.z)) < angle) { // Respect the maximum speed
				return false; // Only if we are going in ANGLE degrees or higher
			}
			
			return true;
		}

		public List<Cell[][][]> beforeLineOfSight (Node from, Node to, RRTKDTreeCombat context) {
			if (!context.simulateCombat && context.enemies != null) {
				List<Cell[][][]> seenList = new List<Cell[][][]> ();
				foreach (Enemy e in context.enemies) {
					seenList.Add (e.seenCells);
				}

				return seenList;
			}
			return null;
		}

		public bool validateLineOfSight (Node from, Node to, Node hit, RRTKDTreeCombat context) {
			if (hit != null)
				if (hit.cell.blocked || (!context.simulateCombat && hit.cell.seen && !hit.cell.safe)) { // Collision with obstacle, ignore. If we don't simulate combat, ignore collision with enemy
					return false;
				}
			return true;
		}
		
		public Node beforeConnect (Node from, Node to, Node hit, RRTKDTreeCombat context) {
			return null;
		}
		
		public Node afterConnect (Node from, Node to, RRTKDTreeCombat context) {
			// Attemp to connect to the end node
			if (!context.simulateCombat || (context.simulateCombat && to.playerhp > 0)) {
				// Compute minimum time to reach the end node
				p1 = to.GetVector3 ();
				p2 = context.end.GetVector3 ();
				p2.y = p1.y;
				float dist = Vector3.Distance (p1, p2);
				
				float t = dist * Mathf.Tan (angle);
				pd = p2;
				pd.y += t;
				
				if (pd.y <= context.nodeMatrix.GetLength (0)) {
					Node endNode = context.GetNode ((int)pd.y, (int)pd.x, (int)pd.z);
					// Try connecting
					
					List<Cell[][][]> seenList = new List<Cell[][][]> ();
					foreach (Enemy e in context.enemies) {
						if (!context.simulateCombat || (context.simulateCombat && to.enemyhp [e] > 0))
							seenList.Add (e.seenCells);
					}
					
					Node hit = context.dda.Los3D (context.nodeMatrix, to, endNode, seenList.ToArray ());
					
					// To simplify things, only connect if player isn't seen or collides with an obstacle
					if (hit == null) {
						endNode.parent = to;
						context.copy (endNode.parent, endNode);

						return endNode;
					}
				}
			}
			return null;
		}

	}
}

