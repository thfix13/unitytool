using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class MapperEditorDrawer : MonoBehaviour
{
	
	public Cell[][][] fullMap;
	public float[][] seenNeverSeen;
	public List<Node> rrtMap;
	public Dictionary<Path, bool> paths = new Dictionary<Path, bool> ();
	public int[,] heatMap;
	public int[][,] heatMap3d;
	public float heatMapMax = 0, seenNeverSeenMax;
	public int[] heatMapMax3d;
	public int timeSlice;
	public Vector2 zero = new Vector2 ();
	public Vector2 tileSize = new Vector2 ();
	public bool drawMap = true, drawMoveMap = false, drawNeverSeen = false, draw3dExploration = false, drawHeatMap = true, drawPath = false, editGrid = false, drawFoVOnly = false, drawVoronoi = false;
	public Cell[][] editingGrid;
	public Cell[][] voronoiGrid;
	// Fixed values
	private Color orange = new Color (1.0f, 0.64f, 0f, 1f), transparent = new Color (1f, 1f, 1f, 0f);
	
	public void Start ()
	{
		Debug.Log ("Started");
		hideFlags = HideFlags.HideInInspector;
	}
	
	public void OnDrawGizmos ()
	{
		// We need 2 if blocks since we are using 2 different variables to poke the data from
		if (editGrid && editingGrid != null) {
			for (int x = 0; x < editingGrid.Length; x++)
				for (int y = 0; y < editingGrid[x].Length; y++) {
					Cell c = editingGrid [x] [y];
				
					if (drawFoVOnly) {
						if (c != null && c.seen)
							Gizmos.color = orange;
						else
							Gizmos.color = transparent;
					} else {
						if (c == null)
							Gizmos.color = Color.gray;
						else if (c.safe)
							Gizmos.color = Color.blue;
						else if (c.blocked)
							Gizmos.color = Color.red;
						else if (c.seen)
							Gizmos.color = orange;
						else if (c.noisy)
							Gizmos.color = Color.yellow;
						else if (c.waypoint)
							Gizmos.color = Color.cyan;
						else
							Gizmos.color = Color.gray;
					}
				
					Gizmos.DrawCube (new Vector3
							(x * tileSize.x + zero.x + tileSize.x / 2f,
							0.1f,
							y * tileSize.y + zero.y + tileSize.y / 2f),
							new Vector3
								(tileSize.x - tileSize.x * 0.05f,
								0.0f,
								tileSize.y - tileSize.y * 0.05f));
				}
		} else if (drawMap && fullMap != null) {
			for (int x = 0; x < fullMap[timeSlice].Length; x++)
				for (int y = 0; y < fullMap[timeSlice][x].Length; y++) {
					Cell c = fullMap [timeSlice] [x] [y];
						
					if (drawHeatMap) {
						if (heatMap != null)
							Gizmos.color = Color.Lerp (Color.white, Color.black, (float)heatMap [x, y] / (heatMapMax * 6f / 8f));
						else if (heatMap3d != null)
							Gizmos.color = Color.Lerp (Color.white, Color.black, heatMapMax3d [timeSlice] != 0 ? (float)heatMap3d [timeSlice] [x, y] / (float)heatMapMax3d [timeSlice] : 0f);
					
					} else {
						if (drawFoVOnly) {
							if (c.seen)
								Gizmos.color = orange;
							else
								Gizmos.color = transparent;
						} else {
							if (c.safe)
								Gizmos.color = Color.blue;
							else if (c.blocked)
								Gizmos.color = Color.red;
							else if (c.seen)
								Gizmos.color = orange;
							else if (c.noisy)
								Gizmos.color = Color.yellow;
							else if (c.waypoint)
								Gizmos.color = Color.cyan;
							else if (drawNeverSeen)
								Gizmos.color = Color.Lerp (Color.green, Color.magenta, seenNeverSeen [x] [y] / (seenNeverSeenMax * 3f / 8f));
							else
								Gizmos.color = Color.green;
						}
					}
				
					Gizmos.DrawCube (new Vector3
							(x * tileSize.x + zero.x + tileSize.x / 2f,
							drawMoveMap ? timeSlice : 0.1f,
							y * tileSize.y + zero.y + tileSize.y / 2f),
							new Vector3
								(tileSize.x - tileSize.x * 0.05f,
								0.0f,
								tileSize.y - tileSize.y * 0.05f));
				}
		}
			
		// RRT exploration tree drawning
		if (draw3dExploration && rrtMap != null) {
			foreach (Node n in rrtMap) {
				if (n.parent != null)
					Gizmos.DrawLine (new Vector3
							(n.x * tileSize.x + zero.x + tileSize.x / 2f,
							n.t * (tileSize.x + tileSize.y) / 2,
							n.y * tileSize.y + zero.y + tileSize.y / 2f), 
							new Vector3
							(n.parent.x * tileSize.x + zero.x + tileSize.x / 2f,
							n.parent.t * (tileSize.x + tileSize.y) / 2,
							n.parent.y * tileSize.y + zero.y + tileSize.y / 2f));
			}
		}
			
		// All Paths drawning
		if (drawPath) {
			Gizmos.color = Color.blue;
			foreach (KeyValuePair<Path, bool> kv in paths)
				if (kv.Value) {
					Gizmos.color = kv.Key.color;
					foreach (Node n in kv.Key.points)
						if (n.parent != null)
							Gizmos.DrawLine (new Vector3
								((n.x * tileSize.x + zero.x),
								0.1f,
								(n.y * tileSize.x + zero.y)),
							
							new Vector3
								((n.parent.x * tileSize.y + zero.x),
								0.1f,
								(n.parent.y * tileSize.y + zero.y)));
				}
		}
		
		if (drawVoronoi && voronoiGrid != null) {
			for (int x = 0; x < voronoiGrid.Length; x++) {
				for (int y = 0; y < voronoiGrid[x].Length; y++) {
					Cell c = voronoiGrid[x][y];
					if (!c.blocked) {
						switch (c.nearestVoronoiCentre) {
							case 0:
								Gizmos.color = Color.red;
								break;
							case 1:
								Gizmos.color = Color.yellow;
								break; 
							case 2:
								Gizmos.color = Color.green;
								break;
							case 3:
								Gizmos.color = Color.cyan;
								break;
							case 4:
								Gizmos.color = Color.magenta;
								break;
							case 5:
								Gizmos.color = Color.blue;
								break;
							case 6:
								Gizmos.color = Color.Lerp (Color.red, Color.yellow, 0.4f);
								break;
							case 7:
								Gizmos.color = Color.Lerp (Color.yellow, Color.blue, 0.2f);
								break;
							case 8:
								Gizmos.color = Color.Lerp (Color.red, Color.blue, 0.2f);
								break;
							case 9:
								Gizmos.color = Color.Lerp (Color.green, Color.red, 0.2f);
								break;
							case 10:
								Gizmos.color = Color.Lerp (Color.green, Color.yellow, 0.7f);
								break;
							case 11:
								Gizmos.color = Color.Lerp (Color.blue, Color.green, 0.3f);
								break;
							case 12:
								Gizmos.color = Color.Lerp (Color.magenta, Color.gray, 0.6f);
								break;
							case 13:
								Gizmos.color = Color.Lerp (Color.cyan, Color.yellow, 0.6f);
								break;
							default:
								break;
						}
						
						Gizmos.DrawCube (new Vector3
								(x * tileSize.x + zero.x + tileSize.x / 2f,
								0.1f,
								y * tileSize.y + zero.y + tileSize.y / 2f),
								new Vector3
									(tileSize.x - tileSize.x * 0.05f,
									0.0f,
									tileSize.y - tileSize.y * 0.05f));
					}
				}
			}
		}
		
	}
}