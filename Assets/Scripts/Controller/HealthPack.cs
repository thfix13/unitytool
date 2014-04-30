using Common;
using Exploration;
using System.Collections.Generic;
using Objects;
using UnityEngine;

namespace RRTController {

	public class Health : Controller {

		private float angle;

		public Health () {
		}
		
		public void onStart (Node start, RRTKDTreeCombat context) {
			start.picked = new List<HealthPack> ();

			float tan = context.playerSpeed / 1;
			angle = 90f - Mathf.Atan (tan) * Mathf.Rad2Deg;
		}
		
		public bool afterSample (Node closest, Node sampled, RRTKDTreeCombat context) {
			return true;
		}
		
		public List<Cell[][][]> beforeLineOfSight (Node from, Node to, RRTKDTreeCombat context) {
			return null;
		}
		
		public bool validateLineOfSight (Node from, Node to, Node hit, RRTKDTreeCombat context) {
			return true;
		}
		
		public Node beforeConnect (Node from, Node to, Node hit, RRTKDTreeCombat context) {
			return null;
		}
		
		public Node afterConnect (Node from, Node to, RRTKDTreeCombat context) {

			if (to.playerhp < context.playerMaxHp) {
				// Health pack solving
				foreach (HealthPack pack in context.packs) {
					if (!to.picked.Contains (pack)) {
						// Compute minimum time to reach the pack
						Vector3 p1 = to.GetVector3 ();
						Vector3 p2 = new Vector3 (pack.posX, p1.y, pack.posZ);
						float dist = Vector3.Distance (p1, p2);

						float t = dist * Mathf.Tan (angle);
						p2.y += t;

						if (p2.y <= context.nodeMatrix.GetLength (0)) {
							// TODO If the node is already on the Tree, things may break!
							// but we need to add it to the tree and retrieve from it to make it a possible path!
							Node packNode = context.GetNode ((int)p2.y, (int)p2.x, (int)p2.z);

							// Try connecting
							List<Cell[][][]> seenList = new List<Cell[][][]> ();
							foreach (Enemy e in context.enemies) {
								if (!context.simulateCombat || (context.simulateCombat && to.enemyhp [e] > 0))
									seenList.Add (e.seenCells);
							}

							Node hit = context.dda.Los3D (context.nodeMatrix, to, packNode, seenList.ToArray ());

							// To simplify things, only connect if player isn't seen or collides with an obstacle
							if (hit == null) {
								packNode.parent = to;
								context.copy (packNode.parent, packNode);
								packNode.picked.Add (pack);
								packNode.playerhp = context.playerMaxHp;
								return packNode;
							}
						}
					}
				}
			}

			return null;
		}
	}
}

