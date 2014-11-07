using System.Collections.Generic;
using UnityEngine;
using Common;
using Exploration;
using Extra;
using Objects;

namespace RRTController {

	public class Distance : Controller 
	{
		float dist; 
		public Distance(float dist)
		{
			this.dist = dist; 
			//Debug.Log("Created");
		}
		public void onStart (Node start, RRTKDTreeCombat context) 
		{
			
		}
		
		public List<Cell[][][]> beforeLineOfSight (Node from, Node to, RRTKDTreeCombat context) {
			return null;
		}

		public bool validateLineOfSight (Node from, Node to, Node hit, RRTKDTreeCombat context) 
		{
			//Check the distance on the segment to the enemies. 
			//Debug.Log();
			//Find position of the enemy on the grid
			//Debug.Log((context.enemies[0].cells[from.t][0]));
			//Get position grid
			foreach(Enemy e in context.enemies)
			{
				Vector2 ePos = new Vector2((int)((e.cells[from.t][0].x - context.min.x) / context.tileSizeX),
										   (int)((e.cells[from.t][0].y - context.min.y) / context.tileSizeZ));
				Debug.Log(ePos);

			}


			//Debug.Log(from);
			//Debug.Log(to);
			return true; 
		}

		public bool afterSample (Node closest, Node sampled, RRTKDTreeCombat context) {
			return true;
		}

		public Node beforeConnect (Node from, Node to, Node hit, RRTKDTreeCombat context) 
		{
		
			return null; 
		}
		public Node afterConnect (Node from, Node to, RRTKDTreeCombat context)
		{
			return null; 

		}
		
	}
}