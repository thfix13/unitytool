using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Common;
using Extra;
using System.Linq;

namespace EditorArea {
	
	[ExecuteInEditMode]
	public class ClusteringEditorDrawer : MonoBehaviour {
		
		// Options
		public bool drawMap = true, drawHeatMap = true, drawPath = false, editGrid = false;
		
		// Caller must set these up
		public Cell[][][] fullMap;
		public int timeSlice;
		public Vector2 zero = new Vector2 (), tileSize = new Vector2 ();
		
		// Optional things
		public Dictionary<Path, bool> paths = new Dictionary<Path, bool> ();
		public Cell[][] editingGrid;
		public List<Tuple<Vector3, string>> textDraw;
		
		// We are just maintaning a bunch of maps so we can color them accordinlly
		public int[,] heatMap;
		public int[][,] heatMapColored;
		
		// Auxiliary values for the maps above
		public float heatMapMax;
		
		// Fixed values
		private Color orange = new Color (1.0f, 0.64f, 0f, 1f);
				
		public void Start () {
			hideFlags = HideFlags.HideInInspector;
		}
		
		public void OnDrawGizmos () {
			// We need 2 if blocks since we are using 2 different variables to poke the data from
			if (drawMap && fullMap != null)
			{
				// check if one of the heat maps is requested to be in color
				int numColors = 0;
				foreach (bool b in ClusteringEditorWindow.drawHeatMapColors) if (b) numColors ++;
				
				if (numColors > 0 && heatMapColored != null)
				{
					for (int color = 0; color < ClusteringEditorWindow.colors.Count(); color ++)
					{
						if (!ClusteringEditorWindow.drawHeatMapColors[color]) continue;
						
						for (int x = 0; x < fullMap[timeSlice].Length; x++)
						{
							for (int y = 0; y < fullMap[timeSlice][x].Length; y++)
							{
//								Cell c = fullMap [timeSlice] [x] [y];
					
								if (heatMapColored[color][x, y] > 0)
								{
									Color regColor = ClusteringEditorWindow.colors[color];
									Gizmos.color = Color.Lerp (new Color(regColor.r, regColor.g, regColor.b, 0.0f), Color.black, (float)heatMapColored[color][x, y] / (heatMapMax * 6f / 8f));
									Gizmos.DrawCube (new Vector3(x * tileSize.x + zero.x + tileSize.x / 2f, 0.1f, y * tileSize.y + zero.y + tileSize.y / 2f), new Vector3(tileSize.x - tileSize.x * 0.05f, 0.0f, tileSize.y - tileSize.y * 0.05f));
								}
							}
						}
					}
				}
				else
				{
					for (int x = 0; x < fullMap[timeSlice].Length; x++)
					{
						for (int y = 0; y < fullMap[timeSlice][x].Length; y++)
						{
							Cell c = fullMap [timeSlice] [x] [y];
					
							if (drawHeatMap) {
								if (heatMap != null)
									Gizmos.color = Color.Lerp (Color.white, Color.black, (float)heatMap [x, y] / (heatMapMax * 6f / 8f));
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
								else if (c.cluster > 0)
									Gizmos.color = Color.white;
								else
									Gizmos.color = Color.green;
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
			
			// All Paths drawning
			if (drawPath && paths.Count > 0) {
				Gizmos.color = Color.blue;
				foreach (KeyValuePair<Path, bool> kv in paths)
				if (kv.Value) {
					foreach (Node n in kv.Key.points) {
						Gizmos.color = kv.Key.color;
						if (n.parent != null) 
						{
							float danger3 = n.t/20;
							float danger3Parent = n.parent.t/20; 
							if (ClusteringEditorWindow.clustEnvironment == ClustEnv.ENV_PLATFORM)
							{
								zero = new Vector2(0, 0);
								tileSize = new Vector2(1, 1);
							}
							else if (ClusteringEditorWindow.clustEnvironment == ClustEnv.ENV_PUZZLE)
							{
								zero = new Vector2 (0, 0);
								tileSize = new Vector2 (32, 32);	
							}

							if (n.danger3 > 0)
							{
								danger3 = n.danger3*100+0.1f ;
								danger3Parent = n.parent.danger3*100+0.1f;
							}
							if (n.yD == 0.0 && n.xD == 0.0)
							{
								Gizmos.DrawLine (
									new Vector3(n.x * tileSize.x + zero.x, (danger3), (n.y * tileSize.x + zero.y)), 
									new Vector3(n.parent.x * tileSize.y + zero.x, (danger3Parent), (n.parent.y * tileSize.y + zero.y))
									);
							//	Gizmos.DrawLine (new Vector3(n.x * tileSize.x + zero.x, 0.1f + (n.danger3*100), (n.y * tileSize.x + zero.y)), new Vector3(n.parent.x * tileSize.y + zero.x, 0.1f + (n.parent.danger3*100), (n.parent.y * tileSize.y + zero.y)));
							}
							else
							{
//								Debug.Log("Tilesize: " + tileSize + ", zero: " + zero + ", n: " + n);
								Gizmos.DrawLine (new Vector3((System.Convert.ToSingle(n.xD) * tileSize.x + zero.x),(danger3), 
													(System.Convert.ToSingle(n.yD) * tileSize.x + zero.y)), 
												new Vector3((float)(n.parent.xD * tileSize.y + zero.x), (danger3Parent),
												 	(float)(n.parent.yD * tileSize.y + zero.y)));
							//	Gizmos.DrawLine (new Vector3((System.Convert.ToSingle(n.xD) * tileSize.x + zero.x), 0.1f + (n.danger3*100), (System.Convert.ToSingle(n.yD) * tileSize.x + zero.y)), new Vector3((float)(n.parent.xD * tileSize.y + zero.x), 0.1f + (n.parent.danger3*100), (float)(n.parent.yD * tileSize.y + zero.y)));
							}
						}
					}
				}
			}
			
			GUIStyle style = new GUIStyle();
			style.normal.textColor = Color.red;
			style.fontSize = 16;
			style.normal.background = new Texture2D(100,20);
			
			if (textDraw != null)
				foreach (Tuple<Vector3, string> t in textDraw)
					Handles.Label(t.First, t.Second, style);
		}
	}
	
}