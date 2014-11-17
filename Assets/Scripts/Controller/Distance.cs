using System.Collections.Generic;
using UnityEngine;
using Common;
using Exploration;
using Extra;
using Objects;
using Vectrosity;

namespace RRTController {

	public class Distance : Controller 
	{
		float dist; 
		GameObject lines;
		
		public Distance(float dist)
		{
			this.dist = dist; 

			//Debug.Log("Created");
			lines = GameObject.Find("Lines");
			if(lines != null)
				GameObject.DestroyImmediate(lines);
			lines = new GameObject("Lines");


		}
		public void onStart (Node start, RRTKDTreeCombat context) 
		{
			VectorLine.SetCamera(); 
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


			//Find the block you need in 3d to be check
			//between from - to nodes

			Bresenham3D line = new Bresenham3D( from.GetVector3 (), to.GetVector3 () );
 	
			//VectorLine l = new VectorLine("Line",new Vector3[2]{from.GetVector3 (), to.GetVector3 ()},null,2.0f);
			//l.vectorObject.transform.parent = lines.transform;
			//l.Draw3D();

			foreach( Vector3 point in line )
			{
				//Visualise the line created by the segment. 
			    //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			    //cube.transform.parent = lines.transform; 
			    //cube.transform.position = point; 


				foreach(Enemy e in context.enemies)
				{
					Vector3 ePos = new Vector3((int)((e.cells[(int)point.y][0].x)),// - context.min.x) / context.tileSizeX),
					                           point.y,
					                           (int)((e.cells[(int)point.y][0].y))); //- context.min.y) / context.tileSizeZ));
					//Debug.Log(ePos);

					//Add debug line vectrocity
					//Foreach cube check the distance to the enemy

					//VectorLine ll = new VectorLine("Line",new Vector3[2]{ePos,point},null,2.0f);
					//ll.SetColor(Color.red);
					//ll.vectorObject.transform.parent = lines.transform;
					//ll.Draw3D(); 


					//Line from ePos to point is the line segment from the search to the enemy as a function of time. 
					//Draw the line only if the enemy is seened by it. 

					//Check the distance
					//Debug.Log(Vector3.Distance(point, ePos));

					if( Vector3.Distance(point, ePos) <dist)
					{
						return false; 

						//get the nodes inbetween
						Bresenham3D lineToEnemy = new Bresenham3D( ePos, point );
							



						foreach( Vector3 point2 in line )
						{
							//Check on the map
							//if(context.nodeMatrix[][][])
						}
					}

				}

			}





				





			//find the line between each block and the enemy
				//Check if it instersects with a wall



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