using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using KDTreeDLL;
using Common;
using Objects;
using Extra;

namespace Exploration {
	public class RRTKDTreeGEO : NodeProvider {

		//private Cell[][][] nodeMatrix;
		private float angle;
		public KDTree tree;
		public List<NodeGeo> explored;
		// Only do noisy calculations if enemies is different from null
		public List<EnemyGeo> enemies;

		private int interval = 3;
		//Not used anymore
		private int depth = 5;
		//public Vector3 min;
		//public float tileSizeX, tileSizeZ;
		private int maxDist = 1;



		//Geo version of GetNode
		public NodeGeo GetNodeGeo (int t, float x, float y) {
			object o = tree.search (new double[] {x, t, y});
			if (o == null) {
				NodeGeo n = new NodeGeo ();
				n.x = x;
				n.y = y;
				n.t = t;
				o = n;
			}
			return (NodeGeo)o;
		}



		public List<NodeGeo> ComputeGeo (float startX, float startY, float endX, float endY, float minX, float maxX, float minY, float maxY, int maxT, int attemps, float speed, Vector2 distractPos, Vector2 distractPos2,  bool smooth = false) {
			//Debug.Log ("COMPUTEGEO");

			// Initialization
			tree = new KDTree (3);
			explored = new List<NodeGeo> ();
			//nodeMatrix = matrix;
			
			//Start and ending node
			NodeGeo start = GetNodeGeo (0, startX, startY);
			start.visited = true; 
			start.parent = null;
			
			// Prepare start and end node
			NodeGeo end = GetNodeGeo (0, endX, endY);
			tree.insert (start.GetArray (), start);
			explored.Add (start);
			
			// Prepare the variables		
			NodeGeo nodeVisiting = null;
			NodeGeo nodeTheClosestTo = null;
			
			float tan = speed / 1.0f;
			angle = 90 - Mathf.Atan (tan) * Mathf.Rad2Deg;


			
			// WHAT IS THIS??
			/*
			List<Distribution.Pair> pairs = new List<Distribution.Pair> ();
			
			for (int x = 0; x < matrix[0].Length; x++) 
				for (int y = 0; y < matrix[0].Length; y++) 
					if (((Cell)matrix [0] [x] [y]).waypoint)
						pairs.Add (new Distribution.Pair (x, y));
			
			pairs.Add (new Distribution.Pair (end.x, end.y));
			
			//Distribution rd = new Distribution(matrix[0].Length, pairs.ToArray());
			*/
			
			//RRT algo
			for (int i = 0; i <= attemps; i++) {
				
				//Pick a random time
				int rt = Random.Range (1,maxT);

				//Then pick random x and y values
				float rx;
				float ry;

				bool distractPick = false;
			

				if(Random.Range (0, 100) > 50) {
					if(Random.Range (0, 100) > 50){
						rx = distractPos.x;
						ry = distractPos.y;
						distractPick = true;
					}
					else{
						rx = distractPos2.x;
						ry = distractPos2.y;
						distractPick = true;
					}
				}
				else{
					rx = Random.Range (minX, maxX);
					ry = Random.Range (minY, maxY);
				}







				//int rx = p.x, ry = p.y;
				nodeVisiting = GetNodeGeo (rt, rx, ry);
				//if this node has already been visited continue
				if (nodeVisiting.visited) {
					i--;
					//Consider checking if point is valid earlier, for i--.
					continue;
				}
				
				explored.Add (nodeVisiting);
				
				nodeTheClosestTo = (NodeGeo)tree.nearest (new double[] {rx, rt, ry});
				
				// cannot go back in time, so skip if t is decreasing
				if (nodeTheClosestTo.t > nodeVisiting.t){
					continue;
				}


				
				// Only add if we are going in ANGLE degrees or higher.As there is a fixed max speed
				Vector3 p1 = nodeVisiting.GetVector3 ();
				Vector3 p2 = nodeTheClosestTo.GetVector3 ();
				Vector3 pd = p1 - p2;
				if (Vector3.Angle (pd, new Vector3 (pd.x, 0f, pd.z)) < angle) {
					continue;
				}				

				//Experimental New distract
				if(nodeTheClosestTo.distractTimes.Count == maxDist){
					nodeVisiting.distractTimes = nodeTheClosestTo.distractTimes;
				}
				else if(nodeTheClosestTo.distractTimes.Count > 0){
					if(distractPick){
						nodeVisiting.distractTimes = nodeTheClosestTo.distractTimes;
						nodeVisiting.distractTimes.Add (nodeVisiting.t);
					}
					else{
						nodeVisiting.distractTimes = nodeTheClosestTo.distractTimes;
					}
				}

				//Old Backup Distract
				//if(nodeTheClosestTo.distractTime > 0){
				//	nodeVisiting.distractTime = nodeTheClosestTo.distractTime;
				//}
				//else if(distractPick){
				//	nodeVisiting.distractTime = nodeVisiting.t;
				//}


				//Check for collision with obstacles
				if(checkCollObs(p2.x, p2.z, p1.x, p1.z)){

					continue;
				}
				
				//Check for collision with guard line of sight -- OLD WAY
				//if(checkCollEs(p2.x, p2.z, (int)p2.y, p1.x, p1.z, (int)p1.y, enemies, 1, depth, nodeVisiting.distractTime)){
				//		continue;
				//}

				if(checkCollEs(p2.x, p2.z, (int)p2.y, p1.x, p1.z, (int)p1.y, enemies, 1, depth, nodeVisiting.distractTimes)){
							continue;
				}



				
				try {
					tree.insert (nodeVisiting.GetArray (), nodeVisiting);
				} catch (KeyDuplicateException) {
				}
				
				nodeVisiting.parent = nodeTheClosestTo;
				nodeVisiting.visited = true;




				// Attempt to connect to the end node
				if (Random.Range (0, 1000) > 0) {
					p1 = nodeVisiting.GetVector3 ();
					p2 = end.GetVector3 ();
					p2.y = p1.y;
					float dist = Vector3.Distance (p1, p2);
					float t = dist * Mathf.Tan (angle * Mathf.Deg2Rad);
					pd = p2;
					pd.y += t;



					NodeGeo endNode = GetNodeGeo ((int)pd.y, pd.x, pd.z);


					if (!checkCollObs(p1.x, p1.z, p2.x, p2.z) && !checkCollEs(p1.x, p1.z, (int)p1.y, pd.x, pd.z, (int)pd.y, enemies, 1, depth, nodeVisiting.distractTimes)) {
						//Debug.Log ("Done3");
						endNode.parent = nodeVisiting;
						return ReturnPathGeo (endNode, smooth);
					}
				}
				
				//Might be adding the neighboor as a the goal
				if (Mathf.Approximately(nodeVisiting.x, end.x) & Mathf.Approximately(nodeVisiting.y,end.y)) {
					//Debug.Log ("Done2");
					return ReturnPathGeo (nodeVisiting, smooth);
					
				}
			}

			//End RRT algo

			return new List<NodeGeo> ();
		}



	

		//Check for collision of a path with the obstacles, x, t, y
		public bool checkCollObs(float startX, float startY, float endX, float endY){
			//Debug.Log ("checkCollObs");
			Vector3 start = new Vector3(startX, 0, startY);
			Vector3 end = new Vector3(endX, 0, endY);
			int layerMask = 1 << 8;
			return Physics.Linecast (start, end, layerMask);

			/* OLD WAY
			Line path = new Line(start, end);
			foreach(Geometry g in obs){
				foreach(Line l in g.edges){
					if(l.LineIntersection(path)){
						return true;
					}
				}
			}

			return false;
			*/
		}

		//Check for collision of a path with the enemies
		public bool checkCollEs(float startX, float startY,int startT, float endX, float endY,  int endT, List<EnemyGeo> enems, int d, int depth, List<int> distractTimes){
			//Debug.Log ("CheckCollEs");
			if(enems == null){
				//Debug.Log ("no enems");
				return false;
			}




			if(d == 1){
				foreach(EnemyGeo e in enems){
					if(checkCollE(e, startX, startT, startY, distractTimes)){
							return true;
					}
					if(checkCollE(e, endX, endT, endY, distractTimes)){
							return true;
					}
				}
			}
			//if(d < depth){
				float newX = (startX + endX)/2.0f;
				float newY = (startY + endY) / 2.0f;
				int newT = (startT + endT) / 2;
				if(newT - startT <=interval){
					if( endT - newT <=interval){
						return false;
					}
					else{
						if(checkCollEs(newX, newY, newT, endX, endY, endT, enems, d+1, depth, distractTimes)){
							return true;
						}
					}
				}
				else if( endT - newT <=interval){
					if(checkCollEs(startX, startY, startT, newX, newY, newT, enems, d+1, depth, distractTimes)){
						return true;
					}
				}
				else{
					foreach(EnemyGeo e in enems){
						if(checkCollE(e, newX, newT, newY, distractTimes)){
							return true;
						}
						if(checkCollEs(startX, startY, startT, newX, newY, newT, enems, d+1, depth, distractTimes)){
							return true;
						}
						if(checkCollEs(newX, newY, newT, endX, endY, endT, enems, d+1, depth, distractTimes)){
							return true;
						}
					}
				}
					                                      
			//}

			/* OLD WAY CHECK EVERY FRAME
			int numSteps = endT - startT;
			float stepX = (endX - startX) / ((float) numSteps);
			float stepY = (endY - startY) / ((float) numSteps);

			float checkX = startX;
			float checkY = startY;

			for(int t = startT; t <= endT; t++){
				foreach(EnemyGeo e in enems){
					if(checkCollE(e, checkX, t, checkY, obs)){
						return true;
					}
				}

				checkX += stepX;
				checkY += stepY;
			}
			*/

			//Debug.Log ("All enmes checked");

			return false;
		}
		

		public bool checkCollE(EnemyGeo e, float x, int t, float y, List<int> distractTimes){
			//Debug.Log ("CheckCollE");
			Vector3 posE3;
			Vector3 forw;
			if(distractTimes.Count > 0){
				//Debug.Log ("distracted movement");
				posE3 = e.getPositionDists(t, distractTimes);
				forw = e.getForwardDists(t, distractTimes);
			}
			else{
				posE3 = e.getPosition (t);
				forw = e.getForward(t);
			}
			Vector2 posE = new Vector2(posE3.x, posE3.z);
			Vector2 posP = new Vector2(x,y);
			if(Vector2.Distance(posE, posP) > e.fovDistance){
				//Debug.Log ("Too Far Away: " + Vector2.Distance(posE, posP) + " t: " + t );
				return false;
			}
			Vector2 toPlay = posP-posE;
			Vector2 look = new Vector2(forw.x, forw.z);

			if(Vector2.Angle(toPlay, look) > e.fovAngle*0.5){
				//Debug.Log ("Angle Too Big: " + Vector2.Angle((posP-posE), e.getForward(t))+ " t: " + t);
				//Debug.Log ("poP " + posP + " posE " + posE + " vec " + (posP-posE) + " forw " + e.getForward (t) + " angle "  + Vector2.Angle ((posP-posE), e.getForward (t)));
				return false;
			}
			bool toReturn = checkCollObs(x, y, posE.x, posE.y);
			if(toReturn){
				//Debug.Log ("Obstacle to enemy detected");
			}
			else{
				//Debug.Log ("Collision with enemy");
			}
			return !toReturn;
		}


		//Returns the computed geo path by the RRT
		private List<NodeGeo> ReturnPathGeo(NodeGeo endNode, bool smooth) {
			NodeGeo n = endNode;
			List<NodeGeo> points = new List<NodeGeo> ();
			
			while (n != null) {
				points.Add (n);
				n = n.parent;
			}
			points.Reverse ();
			
			// If we didn't find a path
			if (points.Count == 1){
				points.Clear ();
			}
			else if(smooth){
				Debug.Log ("NO SMOOTHING IMPLEMENTED CURRENTLY");
			}

			return points;
		}

		#region oldcode
		//To prevent errors temporarily

		public Node GetNode (int t, int x, int y) {
			return null;
		}


		//OLD CODE FOLLOWS



		/*



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


		// Gets the node at specified position from the NodeMap, or create the Node based on the Cell position for that Node

		
		public List<Node> Compute (int startX, int startY, int endX, int endY, int attemps, float speed, Cell[][][] matrix, bool smooth = false) {
			// Initialization
			tree = new KDTree (3);
			explored = new List<Node> ();
			nodeMatrix = matrix;
			
			//Start and ending node
			Node start = GetNode (0, startX, startY);
			start.visited = true; 
			start.parent = null;
			
			// Prepare start and end node
			Node end = GetNode (0, endX, endY);
			tree.insert (start.GetArray (), start);
			explored.Add (start);
			
			// Prepare the variables		
			Node nodeVisiting = null;
			Node nodeTheClosestTo = null;
			
			float tan = speed / 1;
			angle = 90f - Mathf.Atan (tan) * Mathf.Rad2Deg;
			
			List<Distribution.Pair> pairs = new List<Distribution.Pair> ();
			
			for (int x = 0; x < matrix[0].Length; x++) 
				for (int y = 0; y < matrix[0].Length; y++) 
					if (((Cell)matrix [0] [x] [y]).waypoint)
						pairs.Add (new Distribution.Pair (x, y));
			
			pairs.Add (new Distribution.Pair (end.x, end.y));
			
			//Distribution rd = new Distribution(matrix[0].Length, pairs.ToArray());
			
			//RRT algo
			for (int i = 0; i <= attemps; i++) {
				
				//Get random point
				int rt = Random.Range (1, nodeMatrix.Length);
				//Distribution.Pair p = rd.NextRandom();
				int rx = Random.Range (0, nodeMatrix [rt].Length);
				int ry = Random.Range (0, nodeMatrix [rt] [rx].Length);
				//int rx = p.x, ry = p.y;
				nodeVisiting = GetNode (rt, rx, ry);
				if (nodeVisiting.visited || nodeVisiting.cell.blocked) {
					i--;
					continue;
				}
				
				explored.Add (nodeVisiting);
				
				nodeTheClosestTo = (Node)tree.nearest (new double[] {rx, rt, ry});
				
				// Skip downwards movement
				if (nodeTheClosestTo.t > nodeVisiting.t)
					continue;
				
				// Only add if we are going in ANGLE degrees or higher
				Vector3 p1 = nodeVisiting.GetVector3 ();
				Vector3 p2 = nodeTheClosestTo.GetVector3 ();
				Vector3 pd = p1 - p2;
				if (Vector3.Angle (pd, new Vector3 (pd.x, 0f, pd.z)) < angle) {
					continue;
				}
				
				// And we have line of sight
				if ((nodeVisiting.cell.seen && !nodeVisiting.cell.safe) || Extra.Collision.CheckCollision (nodeVisiting, nodeTheClosestTo, this, SpaceState.Editor, true))
					continue;
				
				try {
					tree.insert (nodeVisiting.GetArray (), nodeVisiting);
				} catch (KeyDuplicateException) {
				}
				
				nodeVisiting.parent = nodeTheClosestTo;
				nodeVisiting.visited = true;
				
				// Attemp to connect to the end node
				if (Random.Range (0, 1000) > 0) {
					p1 = nodeVisiting.GetVector3 ();
					p2 = end.GetVector3 ();
					p2.y = p1.y;
					float dist = Vector3.Distance (p1, p2);
					
					float t = dist * Mathf.Tan (angle);
					pd = p2;
					pd.y += t;
					
					if (pd.y <= nodeMatrix.GetLength (0)) {
						Node endNode = GetNode ((int)pd.y, (int)pd.x, (int)pd.z);
						if (!Extra.Collision.CheckCollision (nodeVisiting, endNode, this, SpaceState.Editor, true)) {
							//Debug.Log ("Done3");
							endNode.parent = nodeVisiting;
							return ReturnPath (endNode, smooth);
						}
					}
				}
				
				//Might be adding the neighboor as a the goal
				if (nodeVisiting.x == end.x & nodeVisiting.y == end.y) {
					//Debug.Log ("Done2");
					return ReturnPath (nodeVisiting, smooth);
					
				}
			}
			
			return new List<Node> ();
		}

		*/

		#endregion oldcode
	}
}
