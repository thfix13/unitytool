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
		bool debug = false;

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
 			
 			if(debug)
			{	
				VectorLine l = new VectorLine("Line",new Vector3[2]{from.GetVector3 (), to.GetVector3 ()},null,2.0f);
				l.vectorObject.transform.parent = lines.transform;
				l.Draw3D();
			}
			List<Vector3> lineList =  new List<Vector3>(); 

			foreach( Vector3 point in line )
			{
				lineList.Add(point);
				// Visualise the line created by the segment. 
			    // GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			    // cube.transform.parent = lines.transform; 
			    // cube.transform.position = point; 
			}
			//Instead of doing a line, check in binary fashion. 
			
			List<int> posToCheck = new List<int>(); 
			List<int> posChecked = new List<int>(); 

			posToCheck.Add(0);
			posToCheck.Add(lineList.Count/2); 

			posToCheck.Add(lineList.Count-1);

			int depth = 0; 

			while(depth<3)
			{
				depth++; 
				//Check the position

				for(int i = 0; i<posToCheck.Count; i++)
				{
					Vector3 vec3 = lineList[posToCheck[i]];

					foreach(Enemy e in context.enemies)
					{
						Vector3 ePos = new Vector3((int)((e.cells[(int)vec3.y][0].x)),// - context.min.x) / context.tileSizeX),
			                           vec3.y,
			                           (int)((e.cells[(int)vec3.y][0].y))); //- context.min.y) / context.tileSizeZ));
						if(debug)
						{
							// Debug
							VectorLine ll = new VectorLine("Line",new Vector3[2]{ePos,vec3},null,2.0f);
							ll.SetColor(Color.red);
							ll.vectorObject.transform.parent = lines.transform;
							ll.Draw3D(); 
						}

						//Need something a little bit more fancy
						//check if line of sight exists first. 
						if( Vector3.Distance(vec3, ePos) <dist)
						{
							//Check if LOS exists
							//get the cells 
							Bresenham3D line2 = new Bresenham3D( vec3, ePos );

							bool losExists = true; 

							foreach(Vector3 v in line2)
							{
								if(context.nodeMatrix.Length<v.x &&
								   context.nodeMatrix[(int)v.x].Length<v.z &&
								   context.nodeMatrix[(int)v.x][(int)v.z].Length<v.y)
								{
									if(context.nodeMatrix[(int)v.x][(int)v.z][(int)v.y].blocked)
									{
										losExists = false; 
									}
								}
		
							}

							if(losExists)
								return false; 
						}
						posChecked.Add(posToCheck[i]);
					}

				}

				//update the list of things to check. 
				posToCheck.Clear(); 

				for(int i = 0; i<posChecked.Count -1; i++)
				{
					posToCheck.Add( (posChecked[i] + posChecked[i+1])/2 );
				}

			}

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