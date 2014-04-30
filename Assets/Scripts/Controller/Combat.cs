using Objects;
using Common;
using System.Collections.Generic;
using Exploration;
using UnityEngine;
using Extra;

namespace RRTController {

	public class Combat : Controller {
		public Combat () {
		}

		public void onStart (Node start, RRTKDTreeCombat context) {
			start.playerhp = context.playerMaxHp;
			foreach (Enemy e in context.enemies) {
				start.enemyhp.Add (e, e.maxHealth);
			}
		}

		public bool afterSample (Node closest, Node sampled, RRTKDTreeCombat context) {
			return !context.simulateCombat || context.simulateCombat && closest.playerhp > 0;
		}

		public List<Cell[][][]> beforeLineOfSight (Node from, Node to, RRTKDTreeCombat context) {
			if (context.simulateCombat) {
				// Check for all alive enemies
				List<Cell[][][]> seenList = new List<Cell[][][]> ();
				foreach (Enemy e in context.enemies) {
					if (from.enemyhp [e] > 0)
						seenList.Add (e.seenCells);
				}
				
				if (seenList.Count > 0)
					return seenList;
			}

			return null;
		}
	
		public bool validateLineOfSight (Node from, Node to, Node hit, RRTKDTreeCombat context) {
			return true;
		}
		
		public Node beforeConnect (Node from, Node to, Node hit, RRTKDTreeCombat context) {

			if (!context.simulateCombat || hit == null)
				return null;
			
			// Which enemy has seen me?
			Enemy toFight = null;
			foreach (Enemy e in context.enemies) {
				if (e.seenCells [hit.t] [hit.x] [hit.y] != null && from.enemyhp [e] > 0)
					toFight = e;
			}
			
			// Solve the time
			float timef = from.enemyhp [toFight] / (context.playerDps * context.stepSize);
			int timeT = Mathf.CeilToInt (timef);
			
			// Search for more enemies
			List<object> more = new List<object> ();
			foreach (Enemy e2 in context.enemies) {
				if (toFight != e2)
					for (int t = hit.t; t < hit.t + timeT; t++)
						if (e2.seenCells [t] [hit.x] [hit.y] != null && from.enemyhp [e2] > 0) {
							Tuple<Enemy, int> whenSeen = new Tuple<Enemy, int> (e2, t);
							more.Add (whenSeen);
							break; // Skip this enemy
						}
			}
			
			// Did another enemy saw the player while he was fighting?
			if (more.Count > 0) {
				
				// Who dies when
				List<object> dyingAt = new List<object> ();
				
				// First, save when the first fight starts
				Node firstFight = context.NewNode (hit.t, hit.x, hit.y);
				firstFight.parent = from;
				
				// Then when the first guy dies
				Tuple<Enemy, int> death = new Tuple<Enemy, int> (toFight, firstFight.t + timeT);
				dyingAt.Add (death);
				
				// And proccess the other stuff
				context.copy (from, firstFight);
				firstFight.fighting.Add (toFight);
				
				// Navigation node
				Node lastNode = firstFight;
				
				// Solve for all enemies joining the fight
				foreach (object o in more) {
					Tuple<Enemy, int> joined = (Tuple<Enemy, int>)o;						
					
					// Calculate dying time
					timef = timef + lastNode.enemyhp [joined.First] / (context.playerDps * context.stepSize);
					timeT = Mathf.CeilToInt (timef);
					death = new Tuple<Enemy, int> (joined.First, timeT + hit.t);
					dyingAt.Add (death);
					
					// Create the node structure
					Node startingFight = context.NewNode (joined.Second, hit.x, hit.y);
					
					// Add to fighting list
					context.copy (lastNode, startingFight);
					startingFight.fighting.Add (joined.First);
					
					// Correct parenting
					startingFight.parent = lastNode;
					lastNode = startingFight;
				}
				
				// Solve for all deaths
				foreach (object o in dyingAt) {
					
					Tuple<Enemy, int> dead = (Tuple<Enemy, int>)o;
					Node travel = lastNode;
					bool didDie = false;
					while (!didDie && travel.parent != null) {
						
						// Does this guy dies between two nodes?
						if (dead.Second > travel.parent.t && dead.Second < travel.t) {
							
							// Add the node
							Node adding = context.NewNode (dead.Second + hit.t, hit.x, hit.y);
							adding.fighting = new List<Enemy> ();
							adding.fighting.AddRange (travel.parent.fighting);
							
							// And remove the dead people
							adding.fighting.Remove (dead.First);
							adding.died = dead.First;
							
							Node remove = lastNode;
							
							// Including from nodes deeper in the tree
							while (remove != travel.parent) {
								remove.fighting.Remove (dead.First);
								remove = remove.parent;
							}
							
							// Reparent the nodes
							adding.parent = travel.parent;
							travel.parent = adding;
							didDie = true;
						}
						
						travel = travel.parent;
					}
					if (!didDie) {
						// The guy didn't die between, that means he's farthest away than lastNode
						Node adding = context.NewNode (dead.Second, hit.x, hit.y);
						context.copy (lastNode, adding);
						adding.fighting.Remove (dead.First);
						adding.enemyhp [dead.First] = 0;
						adding.died = dead.First;
						adding.parent = lastNode;
						
						// This is the new lastNode
						lastNode = adding;
					}
				}
				
				// Grab the first node with fighting
				Node first = lastNode;
				while (first.parent != from)
					first = first.parent;
				
				while (first != lastNode) {
					
					Node navigate = lastNode;
					// And grab the node just after the first
					while (navigate.parent != first)
						navigate = navigate.parent;
					
					// Copy the damage already applied
					navigate.playerhp = first.playerhp;
					
					// And deal more damage
					foreach (Enemy dmgDealer in first.fighting)
						navigate.playerhp -= (navigate.t - first.t) * dmgDealer.dps * context.stepSize;
					
					// Goto next node
					first = navigate;
					
				}
				// Make the tree structure
				return lastNode;
			} else {
				// Only one enemy has seen me
				Node toAdd = context.NewNode (hit.t, hit.x, hit.y);
				Node lastNode = context.NewNode (hit.t + timeT, hit.x, hit.y);
				
				lastNode.parent = toAdd;
				toAdd.parent = from;
				
				context.copy (from, toAdd);
				toAdd.fighting.Add (toFight);
				
				context.copy (from, lastNode);
				lastNode.playerhp = toAdd.playerhp - timef * toFight.dps * context.stepSize;
				lastNode.enemyhp [toFight] = 0;
				lastNode.died = toFight;

				return lastNode;
			}
		}

		public Node afterConnect (Node from, Node to, RRTKDTreeCombat context) {
			return null;
		}


	}
}

