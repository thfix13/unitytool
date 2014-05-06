using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using KDTreeDLL;
using Common;
using Objects;
using Extra;
using RRTController;

namespace Exploration {

	public class RRTKDTreeCombat : NodeProvider {

		// Resulting data
		public List<List<Node>> deathPaths;

		// Exported data
		public Cell[][][] nodeMatrix;
		public Node end;
		public DDA dda;

		// Internal data
		private KDTree tree;

		// Data that must be set by the caller
		public Enemy[] enemies;
		public HealthPack[] packs;
		public Vector3 min;
		public float tileSizeX, tileSizeZ;
		public bool simulateCombat;
		public float stepSize, playerMaxHp, playerSpeed, playerDps;
		public List<Controller> controllers = new List<Controller>();
		
		// Gets the node at specified position from the NodeMap, or create the Node based on the Cell position for that Node
		public Node GetNode (int t, int x, int y) {
			object o = tree.search (new double[] {x, t, y});
			if (o == null)
				o = NewNode(t,x,y);

			return (Node)o;
		}

		// Creates a node with the specified coordinates, no matter if it already exists in the tree or not
		// This is mainly to create the connections between nodes that shouldn't be added to the tree
		// Like the nodes with combat in then
		// This avoids crashing exceptions collisions while retrieving nodes
		public Node NewNode (int t, int x, int y) {
			Node n = new Node ();
			
			n.x = x;
			n.y = y;
			n.t = t;
			n.enemyhp = new Dictionary<Enemy, float> ();
			n.fighting = new List<Enemy>();
			n.picked = new List<HealthPack>();
			n.cell = nodeMatrix [t] [x] [y];
			return n;
		}
	
		public List<Node> Compute (int startX, int startY, int endX, int endY, int attemps, bool smooth = false) {
			// Initialization
			tree = new KDTree (3);
			deathPaths = new List<List<Node>> ();

			// TODO Nodes can hold generic Map<String, Object> information insetad of having a bunch of hard coded variables to allow dynamic content to be added
			// This will need a typecast after each 'get' operation, which is an annoying behaviour.

			//Start and ending node
			Node start = GetNode (0, startX, startY);
			start.visited = true; 
			start.parent = null;

			// Prepare start and end node
			end = GetNode (0, endX, endY);
			tree.insert (start.GetArray (), start);

			foreach (Controller c in controllers)
				c.onStart(start, this);
			
			// Prepare the variables		
			Node nodeVisiting = null;
			Node nodeTheClosestTo = null;
			
			/*Distribution algorithm
			 * List<Distribution.Pair> pairs = new List<Distribution.Pair> ();
			
			for (int x = 0; x < matrix[0].Length; x++) 
				for (int y = 0; y < matrix[0].Length; y++) 
					if (((Cell)matrix [0] [x] [y]).waypoint)
						pairs.Add (new Distribution.Pair (x, y));
			
			pairs.Add (new Distribution.Pair (end.x, end.y));
			
			Distribution rd = new Distribution(matrix[0].Length, pairs.ToArray());*/
			 
			dda = new DDA (tileSizeX, tileSizeZ, nodeMatrix [0].Length, nodeMatrix [0] [0].Length);
			//RRT algo
			for (int i = 0; i <= attemps; i++) {

				//Get random point
				int rt = Random.Range (1, nodeMatrix.Length);
				int rx = Random.Range (0, nodeMatrix [rt].Length);
				int ry = Random.Range (0, nodeMatrix [rt] [rx].Length);
				//Distribution.Pair p = rd.NextRandom();
				//int rx = p.x, ry = p.y;
				nodeVisiting = GetNode (rt, rx, ry);
				if (nodeVisiting.visited) {
					i--;
					continue;
				}

				nodeTheClosestTo = (Node)tree.nearest (new double[] {rx, rt, ry});

				bool skip = false;
				foreach (Controller c in controllers)
					skip = skip || !c.afterSample(nodeTheClosestTo, nodeVisiting, this);
				if (skip) continue;

				List<Cell[][][]> seenList = new List<Cell[][][]>();

				foreach (Controller c in controllers) {
					List<Cell[][][]> returned = c.beforeLineOfSight(nodeTheClosestTo, nodeVisiting, this);
					if (returned != null)
						seenList.AddRange(returned);
				}

				// TODO Cells that are 'blocked' can be inside the Basic controller instead of being hard coded inside the DDA.Los3D

				Node hit = dda.Los3D (nodeMatrix, nodeTheClosestTo, nodeVisiting, seenList.ToArray ());

				foreach (Controller c in controllers)
					skip = skip || !c.validateLineOfSight(nodeTheClosestTo, nodeVisiting, hit, this);
				if (skip) continue;

				// Make sure everything works if no nodes are replaced below
				copy (nodeTheClosestTo, nodeVisiting);
				nodeVisiting.parent = nodeTheClosestTo;

				foreach (Controller c in controllers) {
					Node returned = c.beforeConnect(nodeTheClosestTo, nodeVisiting, hit, this);

					// TODO if multiple controllers changes the node at the same time, things may explode, since 'hit' will be invalid!

					if (returned != null)
						nodeVisiting = returned;
				}

				try {
					tree.insert (nodeVisiting.GetArray (), nodeVisiting);
				} catch (KeyDuplicateException) {
				}
				
				nodeVisiting.visited = true;

				// Add the path to the death paths list
				if (simulateCombat && nodeVisiting.playerhp <= 0) {
					Node playerDeath = nodeVisiting;
					while (playerDeath.parent.playerhp <= 0)
						playerDeath = playerDeath.parent;

					deathPaths.Add (ReturnPath (playerDeath, smooth));
				}

				//Might be adding the neighboor as a the goal
				if (nodeVisiting.x == end.x & nodeVisiting.y == end.y) {
					List<Node> done = ReturnPath (nodeVisiting, smooth);
					//UpdateNodeMatrix (done);
					return done;
					
				}

				foreach (Controller c in controllers) {
					Node returned = c.afterConnect(nodeTheClosestTo, nodeVisiting, this);
					
					if (returned != null) {
						// Check if we ended the computation
						if (returned.x == end.x && returned.y == end.y)
							return ReturnPath(returned, smooth);

						// Add the new node to the tree
						try {
							nodeTheClosestTo = nodeVisiting;
							nodeVisiting = returned;
							tree.insert(returned.GetArray(), returned);
						} catch (KeyDuplicateException) {
						}
					}
				}
			}
					
			return new List<Node> ();
		}

		public void copy(Node from, Node to) {
			// TODO create an onCopy so that controllers can pass along their information

			to.playerhp = from.playerhp;

			foreach (Enemy e in enemies)
				to.enemyhp.Add (e, from.enemyhp[e]);

			to.fighting.AddRange(from.fighting);

			to.picked.AddRange(from.picked);

		}
		
		// Returns the computed path by the RRT, and smooth it if that's the case
		private List<Node> ReturnPath (Node endNode, bool smooth) {
			Node n = endNode;
			List<Node> points = new List<Node> ();
			
			while (n != null) {
				points.Add (n);
				n = n.parent;
			}
			points.Reverse ();
			
			// If we didn't find a path
			if (points.Count == 1)
				points.Clear ();
			else if (smooth) {
				// Smooth out the path
				Node final = null;
				foreach (Node each in points) {
					final = each;
					while (Extra.Collision.SmoothNode(final, this, SpaceState.Editor, true)) {
					}
				}
				
				points.Clear ();
				
				while (final != null) {
					points.Add (final);
					final = final.parent;
				}
				points.Reverse ();
			}
						
			return points;
		}

		private void UpdateNodeMatrix (List<Node> points) {
			// Updating the stuff after the player/enemies have fought each other
			Node lastNode = null;
			foreach (Node each in points) {
				if (each.died != null) {
					// After the enemy is dead
					Vector3 outside = new Vector3 (100f, 0f, 100f);
					for (int t = each.t; t < this.nodeMatrix.Length; t++) {
						// Move the guy to a place far away
						each.died.positions [t] = outside;
						each.died.rotations [t] = each.died.rotations [each.t];
						each.died.forwards [t] = each.died.forwards [each.t];
						
						// And remove any seen cells by him
						for (int x = 0; x < each.died.seenCells[0].Length; x++)
							for (int y = 0; y < each.died.seenCells[0][0].Length; y++) 
								if (each.died.seenCells [t] [x] [y] != null) {
									bool cellSeen = false;
									foreach (Enemy e in enemies)
										if (e != each.died) {
											Node correct = points [points.Count - 1];
											while (correct.t > t)
												correct = correct.parent;
									
											if (correct.enemyhp [e] > 0 && e.seenCells [t] [x] [y] != null)
												cellSeen = true;
										}
									if (!cellSeen)
										each.died.seenCells [t] [x] [y].seen = false;
									each.died.seenCells [t] [x] [y] = null;
								}
					}
				}
				
				if (lastNode != null && lastNode.fighting != null) {
					foreach (Enemy e in lastNode.fighting) {
						
						Node fightStarted = lastNode;
						while (fightStarted.parent.fighting != null && fightStarted.parent.fighting.Contains(e))
							fightStarted = fightStarted.parent;
						
						Cell[][] seen = e.seenCells [fightStarted.t];
						
						for (int t = lastNode.t; t < each.t; t++) {
							e.positions [t] = e.positions [fightStarted.t];
							e.rotations [t] = e.rotations [fightStarted.t];
							e.forwards [t] = e.forwards [fightStarted.t];
							
							for (int x = 0; x < seen.Length; x++) {
								for (int y = 0; y < seen[0].Length; y++) {
									if (seen [x] [y] != null) {
										e.seenCells [t] [x] [y] = this.nodeMatrix [t] [x] [y];
										e.seenCells [t] [x] [y].seen = true;
										//e.seenCells [t] [x] [y].safe = true;
									} else if (e.seenCells [t] [x] [y] != null) {
										e.seenCells [t] [x] [y].seen = false;
										e.seenCells [t] [x] [y] = null;
									}
								}
							}
						}
					}
				}
				
				lastNode = each;
			}
		}
	}
}