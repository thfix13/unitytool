using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Common;
using Exploration;
using Path = Common.Path;
using Extra;
using Objects;

namespace EditorArea {
	public class MapperWindowEditor : EditorWindow {

		// Data holders
		public static Cell[][][] fullMap;
		public static List<Path> paths = new List<Path> ();

		// Parameters with default values
		public static int timeSamples = 800, attemps = 25000, iterations = 5, gridSize = 60, ticksBehind = 0;
		private static bool drawMap = true, drawNeverSeen = false, drawHeatMap = false, drawHeatMap3d = false, drawPath = true, smoothPath = true, drawFoVOnly = false;
		private static float stepSize = 1 / 10f, crazySeconds = 5f;

		// Computed parameters
		private static int[,] heatMap;
		private static GameObject start = null, end = null, floor = null, playerPrefab = null;
		private static Dictionary<Path, bool> toggleStatus = new Dictionary<Path, bool> ();
		private static Dictionary<Path, GameObject> players = new Dictionary<Path, GameObject> ();
		private static int startX, startY, endX, endY, maxHeatMap, timeSlice, imported = 0;
		private static bool seeByTime, seeByLength, seeByDanger, seeByLoS, seeByDanger3, seeByLoS3, seeByDanger3Norm, seeByLoS3Norm, seeByCrazy, seeByVelocity;
		private static List<Path> arrangedByTime, arrangedByLength, arrangedByDanger, arrangedByLoS, arrangedByDanger3, arrangedByLoS3, arrangedByDanger3Norm, arrangedByLoS3Norm, arrangedByCrazy, arrangedByVelocity;

		// Helping stuff
		private static Vector2 scrollPos = new Vector2 ();
		private static GameObject playerNode;
		private int lastTime = timeSlice;
		private long stepInTicks = 0L, playTime = 0L;
		private static bool simulated = false, playing = false;
		private Mapper mapper;
		private RRTKDTree rrt = new RRTKDTree ();
		private MapperEditorDrawer drawer;
		private DateTime previous = DateTime.Now;
		private long accL = 0L;
		
		[MenuItem("Window/Mapper")]
		static void Init () {
			MapperWindowEditor window = (MapperWindowEditor)EditorWindow.GetWindow (typeof(MapperWindowEditor));
			window.title = "Mapper";
			window.ShowTab ();
		}
		
		void OnGUI () {
			#region Pre-Init
			
			// Wait for the floor to be set and initialize the drawer and the mapper
			if (floor != null) {
				if (floor.collider == null) {
					Debug.LogWarning ("Floor has no valid collider, game object ignored.");
					floor = null;
				} else {
					drawer = floor.gameObject.GetComponent<MapperEditorDrawer> ();
					if (drawer == null) {
						drawer = floor.gameObject.AddComponent<MapperEditorDrawer> ();
						drawer.hideFlags = HideFlags.HideInInspector;
					}
				}
				if (mapper == null) {
					mapper = floor.GetComponent<Mapper> ();

					if (mapper == null) 
						mapper = floor.AddComponent<Mapper> ();
					
					mapper.hideFlags = HideFlags.None;
				}
			} 
			
			#endregion
			
			// ----------------------------------
			
			scrollPos = EditorGUILayout.BeginScrollView (scrollPos);
			
			#region 1. Map
			
			EditorGUILayout.LabelField ("1. Map");
			playerPrefab = (GameObject)EditorGUILayout.ObjectField ("Player Prefab", playerPrefab, typeof(GameObject), false);
			
			floor = (GameObject)EditorGUILayout.ObjectField ("Floor", floor, typeof(GameObject), true);
			gridSize = EditorGUILayout.IntSlider ("Grid size", gridSize, 10, 300);
	
			EditorGUILayout.LabelField ("");
			
			#endregion
			
			// ----------------------------------
			
			#region 2. Units
			
			EditorGUILayout.LabelField ("2. Units");
			
			if (GUILayout.Button ("Store Positions")) {
				StorePositions ();
			}
			EditorGUILayout.LabelField ("");
			
			#endregion
			
			// ----------------------------------
			
			#region 3. Map Computation
			
			EditorGUILayout.LabelField ("3. Map Computation");
			timeSamples = EditorGUILayout.IntSlider ("Time samples", timeSamples, 1, 10000);
			stepSize = EditorGUILayout.Slider ("Step size", stepSize, 0.01f, 1f);
			stepInTicks = ((long)(stepSize * 10000000L));
			ticksBehind = EditorGUILayout.IntSlider (new GUIContent ("Ticks behind", "Number of ticks that the FoV will remain seen after the enemy has no visibility on that cell (prevents noise/jitter like behaviours)"), ticksBehind, 0, 100);
			
			if (GUILayout.Button ("Precompute Maps")) {
				
				//Find this is the view
				if (playerPrefab == null) {
					playerPrefab = GameObject.Find ("Player"); 
				}
				
				if (floor == null) {
					floor = (GameObject)GameObject.Find ("Floor");
					
					if (mapper == null) {
						mapper = floor.GetComponent<Mapper> ();
						
						if (mapper == null)
							mapper = floor.AddComponent<Mapper> ();

						mapper.hideFlags = HideFlags.None;
					
					}
					drawer = floor.gameObject.GetComponent<MapperEditorDrawer> ();
					if (drawer == null) {
						drawer = floor.gameObject.AddComponent<MapperEditorDrawer> ();
						drawer.hideFlags = HideFlags.HideInInspector;
					}
				}
				
				if (!simulated) {
					StorePositions ();
					simulated = true;
				}
				
				Cell[][] baseMap = null;
				if (MapperEditor.grid != null) {
					Cell[][] obstacles = mapper.ComputeObstacles ();
					baseMap = new Cell[gridSize][];
					for (int x = 0; x < gridSize; x++) {
						baseMap [x] = new Cell[gridSize];
						for (int y = 0; y < gridSize; y++) {
							baseMap [x] [y] = MapperEditor.grid [x] [y] == null ? obstacles [x] [y] : MapperEditor.grid [x] [y];
						}
					}
				}
				
				fullMap = mapper.PrecomputeMaps (SpaceState.Editor, floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timeSamples, stepSize, ticksBehind, baseMap);
				
				drawer.fullMap = fullMap;
				float maxSeenGrid;
				drawer.seenNeverSeen = Analyzer.ComputeSeenValuesGrid (fullMap, out maxSeenGrid);
				drawer.seenNeverSeenMax = maxSeenGrid;
				drawer.tileSize = SpaceState.Editor.tileSize;
				drawer.zero.Set (floor.collider.bounds.min.x, floor.collider.bounds.min.z);
				
				ResetAI ();
				previous = DateTime.Now;
			} 
			EditorGUILayout.LabelField ("");
			
			#endregion
			
			// ----------------------------------
			
			#region 4. Path
			
			EditorGUILayout.LabelField ("4. Path");
			
			start = (GameObject)EditorGUILayout.ObjectField ("Start", start, typeof(GameObject), true);
			end = (GameObject)EditorGUILayout.ObjectField ("End", end, typeof(GameObject), true);
			attemps = EditorGUILayout.IntSlider ("Attempts", attemps, 1000, 100000);
			iterations = EditorGUILayout.IntSlider ("Iterations", iterations, 1, 1500);
			smoothPath = EditorGUILayout.Toggle ("Smooth path", smoothPath);
			
			if (GUILayout.Button ("Compute Path")) {
				float speed = GameObject.FindGameObjectWithTag ("AI").GetComponent<Player> ().speed;
				
				
				//Check the start and the end and get them from the editor. 
				if (start == null) {
					start = GameObject.Find ("Start");
				}
				if (end == null) {
					end = GameObject.Find ("End");	
				}
				
				paths.Clear ();
				ClearPathsRepresentation ();
				arrangedByCrazy = arrangedByDanger = arrangedByDanger3 = arrangedByDanger3Norm = arrangedByLength = arrangedByLoS = arrangedByLoS3 = arrangedByLoS3Norm = arrangedByTime = arrangedByVelocity = null;
				
				startX = (int)((start.transform.position.x - floor.collider.bounds.min.x) / SpaceState.Editor.tileSize.x);
				startY = (int)((start.transform.position.z - floor.collider.bounds.min.z) / SpaceState.Editor.tileSize.y);
				endX = (int)((end.transform.position.x - floor.collider.bounds.min.x) / SpaceState.Editor.tileSize.x);
				endY = (int)((end.transform.position.z - floor.collider.bounds.min.z) / SpaceState.Editor.tileSize.y);
				
				rrt.min = floor.collider.bounds.min;
				rrt.tileSizeX = SpaceState.Editor.tileSize.x;
				rrt.tileSizeZ = SpaceState.Editor.tileSize.y;
				rrt.enemies = SpaceState.Editor.enemies;
				
				List<Node> nodes = null;
				for (int it = 0; it < iterations; it++) {
					nodes = rrt.Compute (startX, startY, endX, endY, attemps, speed, fullMap, smoothPath);
					if (nodes.Count > 0) {
						paths.Add (new Path (nodes));
						toggleStatus.Add (paths.Last (), true);
						paths.Last ().color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
					}
				}
				
				Debug.Log ("Paths found: " + paths.Count);
				
				ComputeHeatMap (paths);
			}
			
			if (GUILayout.Button ("(DEBUG) Export Paths")) {
				PathBulk.SavePathsToFile ("pathtest.xml", paths);
			}
			
			if (GUILayout.Button ("(DEBUG) Import Paths")) {
				paths.Clear ();
				ClearPathsRepresentation ();
				paths.AddRange (PathBulk.LoadPathsFromFile ("pathtest.xml"));
				foreach (Path p in paths) {
					p.name = "Imported " + (++imported);
					p.color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
					toggleStatus.Add (p, true);
				}
				ComputeHeatMap (paths);
				SetupArrangedPaths (paths);
			}
			
			EditorGUILayout.LabelField ("");
			
			#endregion
			
			// ----------------------------------
			
			#region 5. Visualization
			
			EditorGUILayout.LabelField ("5. Visualization");
			
			timeSlice = EditorGUILayout.IntSlider ("Time", timeSlice, 0, timeSamples - 1);
			drawMap = EditorGUILayout.Toggle ("Draw map", drawMap);
			drawNeverSeen = EditorGUILayout.Toggle ("- Draw safe places", drawNeverSeen);
			drawFoVOnly = EditorGUILayout.Toggle ("- Draw only fields of view", drawFoVOnly);
			drawHeatMap = EditorGUILayout.Toggle ("- Draw heat map", drawHeatMap);
			drawHeatMap3d = EditorGUILayout.Toggle ("-> Draw heat map 3d", drawHeatMap3d);
			drawPath = EditorGUILayout.Toggle ("Draw path", drawPath);
			
			if (drawer != null) {
				if (drawHeatMap3d)
					drawer.heatMap = null;
				else
					drawer.heatMap = heatMap;
			}
			
			EditorGUILayout.LabelField ("");
			
			if (GUILayout.Button (playing ? "Stop" : "Play")) {
				playing = !playing;
			}
			
			EditorGUILayout.LabelField ("");
			
			/*if (GUILayout.Button ("Batch computation")) {
				BatchComputing ();
			}*/
			
			#endregion
			
			// ----------------------------------

			#region Triangle



			EditorGUILayout.LabelField ("Triangle");
			
			EditorGUILayout.LabelField (""); 

			if (GUILayout.Button ("Triangulate Space")) 
			{
				TriangulationSpace(); 
			}

			if (GUILayout.Button ("Quadrilateralization Space")) 
			{
				QuadrilateralizationSpace(); 
			}

			if (GUILayout.Button ("Reduce the space")) 
			{
				ReduceSpace(); 
			}
			if(GUILayout.Button("Clear Enemies"))
			{

				GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
				GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Waypoint");

				//Clear the enemies

				if(enemies != null)
				{	
				    
				    foreach (GameObject g in enemies) {
						DestroyImmediate(g);
					}
					
				}
				foreach (GameObject g in waypoints) 
		    	{
					DestroyImmediate(g);
				}	
			}
			#endregion

			#region 6. Paths
			
			EditorGUILayout.LabelField ("6. Paths");
			
			EditorGUILayout.LabelField ("");
			
			crazySeconds = EditorGUILayout.Slider ("Crazy seconds window", crazySeconds, 0f, 10f);
			
			if (GUILayout.Button ("Analyze paths")) {		
				ClearPathsRepresentation ();
				
				// Setup paths names
				int i = 1;
				foreach (Path path in paths) {
					path.name = "Path " + (i++);
					path.color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
					toggleStatus.Add (path, true);
					path.ZeroValues ();
				}
				
				// Analyze paths
				Analyzer.PreparePaths (paths);
				Analyzer.ComputePathsTimeValues (paths);
				Analyzer.ComputePathsLengthValues (paths);
				Analyzer.ComputePathsVelocityValues (paths);
				Analyzer.ComputePathsLoSValues (paths, SpaceState.Editor.enemies, floor.collider.bounds.min, SpaceState.Editor.tileSize.x, SpaceState.Editor.tileSize.y, fullMap, drawer.seenNeverSeen, drawer.seenNeverSeenMax);
				Analyzer.ComputePathsDangerValues (paths, SpaceState.Editor.enemies, floor.collider.bounds.min, SpaceState.Editor.tileSize.x, SpaceState.Editor.tileSize.y, fullMap, drawer.seenNeverSeen, drawer.seenNeverSeenMax);
				Analyzer.ComputeCrazyness (paths, fullMap, Mathf.FloorToInt (crazySeconds / stepSize));
				Analyzer.ComputePathsVelocityValues (paths);
				
				SetupArrangedPaths (paths);
			}
			
			if (GUILayout.Button ("(DEBUG) Export Analysis")) {
				XmlSerializer ser = new XmlSerializer (typeof(MetricsRoot), new Type[] {
					typeof(PathResults),
					typeof(PathValue),
					typeof(Value)
				});
				
				MetricsRoot root = new MetricsRoot ();
				
				foreach (Path path in paths) {
					root.everything.Add (new PathResults (path, Analyzer.pathMap [path]));
				}
				using (FileStream stream = new FileStream ("pathresults.xml", FileMode.Create)) {
					ser.Serialize (stream, root);
					stream.Flush ();
					stream.Close ();
				}
			}
			
			if (GUILayout.Button ("(DEBUG) Compute clusters")) {
				ComputeClusters ();
			}
			
			#region Paths values display
			
			seeByTime = EditorGUILayout.Foldout (seeByTime, "Paths by Time");
			if (seeByTime && arrangedByTime != null) {
				for (int i = 0; i < arrangedByTime.Count; i++) {
					EditorGUILayout.BeginHorizontal ();
	
					EditorGUILayout.FloatField (arrangedByTime [i].name, arrangedByTime [i].time);
					toggleStatus [arrangedByTime [i]] = EditorGUILayout.Toggle ("", toggleStatus [arrangedByTime [i]], GUILayout.MaxWidth (20f));
					EditorGUILayout.ColorField (arrangedByTime [i].color, GUILayout.MaxWidth (40f));
					
					EditorGUILayout.EndHorizontal ();
				}
			}
			
			seeByLength = EditorGUILayout.Foldout (seeByLength, "Paths by Length");
			if (seeByLength && arrangedByLength != null) {
				for (int i = 0; i < arrangedByLength.Count; i++) {
					EditorGUILayout.BeginHorizontal ();
	
					EditorGUILayout.FloatField (arrangedByLength [i].name, arrangedByLength [i].length2d);
					toggleStatus [arrangedByLength [i]] = EditorGUILayout.Toggle ("", toggleStatus [arrangedByLength [i]], GUILayout.MaxWidth (20f));
					EditorGUILayout.ColorField (arrangedByLength [i].color, GUILayout.MaxWidth (40f));
					
					EditorGUILayout.EndHorizontal ();
				}
			}
			
			seeByDanger = EditorGUILayout.Foldout (seeByDanger, "Paths by Danger (A*)");
			if (seeByDanger && arrangedByDanger != null) {
				for (int i = 0; i < arrangedByDanger.Count; i++) {
					EditorGUILayout.BeginHorizontal ();
	
					EditorGUILayout.FloatField (arrangedByDanger [i].name, arrangedByDanger [i].danger);
					toggleStatus [arrangedByDanger [i]] = EditorGUILayout.Toggle ("", toggleStatus [arrangedByDanger [i]], GUILayout.MaxWidth (20f));
					EditorGUILayout.ColorField (arrangedByDanger [i].color, GUILayout.MaxWidth (40f));
					
					EditorGUILayout.EndHorizontal ();
				}
			}
			
			seeByDanger3 = EditorGUILayout.Foldout (seeByDanger3, "Paths by Danger 3 (A*)");
			if (seeByDanger3 && arrangedByDanger3 != null) {
				for (int i = 0; i < arrangedByDanger3.Count; i++) {
					EditorGUILayout.BeginHorizontal ();
	
					EditorGUILayout.FloatField (arrangedByDanger3 [i].name, arrangedByDanger3 [i].danger3);
					toggleStatus [arrangedByDanger3 [i]] = EditorGUILayout.Toggle ("", toggleStatus [arrangedByDanger3 [i]], GUILayout.MaxWidth (20f));
					EditorGUILayout.ColorField (arrangedByDanger3 [i].color, GUILayout.MaxWidth (40f));
					
					EditorGUILayout.EndHorizontal ();
				}
			}
			
			seeByDanger3Norm = EditorGUILayout.Foldout (seeByDanger3Norm, "Paths by Danger 3 (A*) (normalized)");
			if (seeByDanger3Norm && arrangedByDanger3Norm != null) {
				for (int i = 0; i < arrangedByDanger3Norm.Count; i++) {
					EditorGUILayout.BeginHorizontal ();
	
					EditorGUILayout.FloatField (arrangedByDanger3Norm [i].name, arrangedByDanger3Norm [i].danger3Norm);
					toggleStatus [arrangedByDanger3Norm [i]] = EditorGUILayout.Toggle ("", toggleStatus [arrangedByDanger3Norm [i]], GUILayout.MaxWidth (20f));
					EditorGUILayout.ColorField (arrangedByDanger3Norm [i].color, GUILayout.MaxWidth (40f));
					
					EditorGUILayout.EndHorizontal ();
				}
			}
			
			seeByLoS = EditorGUILayout.Foldout (seeByLoS, "Paths by Line of Sight (FoV)");
			if (seeByLoS && arrangedByLoS != null) {
				for (int i = 0; i < arrangedByLoS.Count; i++) {
					EditorGUILayout.BeginHorizontal ();
	
					EditorGUILayout.FloatField (arrangedByLoS [i].name, arrangedByLoS [i].los);
					toggleStatus [arrangedByLoS [i]] = EditorGUILayout.Toggle ("", toggleStatus [arrangedByLoS [i]], GUILayout.MaxWidth (20f));
					EditorGUILayout.ColorField (arrangedByLoS [i].color, GUILayout.MaxWidth (40f));
					
					EditorGUILayout.EndHorizontal ();
				}
			}
			
			seeByLoS3 = EditorGUILayout.Foldout (seeByLoS3, "Paths by Line of Sight 3 (FoV)");
			if (seeByLoS3 && arrangedByLoS3 != null) {
				for (int i = 0; i < arrangedByLoS3.Count; i++) {
					EditorGUILayout.BeginHorizontal ();
	
					EditorGUILayout.FloatField (arrangedByLoS3 [i].name, arrangedByLoS3 [i].los3);
					toggleStatus [arrangedByLoS3 [i]] = EditorGUILayout.Toggle ("", toggleStatus [arrangedByLoS3 [i]], GUILayout.MaxWidth (20f));
					EditorGUILayout.ColorField (arrangedByLoS3 [i].color, GUILayout.MaxWidth (40f));
					
					EditorGUILayout.EndHorizontal ();
				}
			}
			
			seeByLoS3Norm = EditorGUILayout.Foldout (seeByLoS3Norm, "Paths by Line of Sight 3 (FoV) (normalized)");
			if (seeByLoS3Norm && arrangedByLoS3Norm != null) {
				for (int i = 0; i < arrangedByLoS3Norm.Count; i++) {
					EditorGUILayout.BeginHorizontal ();
	
					EditorGUILayout.FloatField (arrangedByLoS3Norm [i].name, arrangedByLoS3Norm [i].los3Norm);
					toggleStatus [arrangedByLoS3Norm [i]] = EditorGUILayout.Toggle ("", toggleStatus [arrangedByLoS3Norm [i]], GUILayout.MaxWidth (20f));
					EditorGUILayout.ColorField (arrangedByLoS3Norm [i].color, GUILayout.MaxWidth (40f));
					
					EditorGUILayout.EndHorizontal ();
				}
			}
			
			seeByCrazy = EditorGUILayout.Foldout (seeByCrazy, "Paths by Crazyness");
			if (seeByCrazy && arrangedByCrazy != null) {
				for (int i = 0; i < arrangedByCrazy.Count; i++) {
					EditorGUILayout.BeginHorizontal ();
	
					EditorGUILayout.FloatField (arrangedByCrazy [i].name, arrangedByCrazy [i].crazy);
					toggleStatus [arrangedByCrazy [i]] = EditorGUILayout.Toggle ("", toggleStatus [arrangedByCrazy [i]], GUILayout.MaxWidth (20f));
					EditorGUILayout.ColorField (arrangedByCrazy [i].color, GUILayout.MaxWidth (40f));
					
					EditorGUILayout.EndHorizontal ();
				}
			}
			
			seeByVelocity = EditorGUILayout.Foldout (seeByVelocity, "Paths by Velocity Changes");
			if (seeByVelocity && arrangedByVelocity != null) {
				for (int i = 0; i < arrangedByVelocity.Count; i++) {
					EditorGUILayout.BeginHorizontal ();
	
					EditorGUILayout.FloatField (arrangedByVelocity [i].name, arrangedByVelocity [i].velocity);
					toggleStatus [arrangedByVelocity [i]] = EditorGUILayout.Toggle ("", toggleStatus [arrangedByVelocity [i]], GUILayout.MaxWidth (20f));
					EditorGUILayout.ColorField (arrangedByVelocity [i].color, GUILayout.MaxWidth (40f));
					
					EditorGUILayout.EndHorizontal ();
				}
			}
			
			#endregion
			
			
			#endregion
			
			// ----------------------------------
			
			if (GUILayout.Button ("(WIP) " + (MapperEditor.editGrid ? "Finish Editing" : "Edit Grid"))) {
				if (floor != null) {
					MapperEditor.editGrid = !MapperEditor.editGrid;
					Selection.activeGameObject = mapper.gameObject;
				}
			}
			
			#region Temp Player setup
			
			if (playerNode == null) {
				playerNode = GameObject.Find ("TempPlayerNode");
				if (playerNode == null) {
					playerNode = new GameObject ("TempPlayerNode");
					playerNode.hideFlags = HideFlags.HideAndDontSave;
				}
			}
			if (playerPrefab != null) {
				foreach (KeyValuePair<Path, bool> p in toggleStatus) {
					if (p.Value) {
						if (!players.ContainsKey (p.Key)) {
							GameObject player = GameObject.Instantiate (playerPrefab) as GameObject;
							player.transform.position.Set (p.Key.points [0].x, 0f, p.Key.points [0].y);
							player.transform.parent = playerNode.transform;
							players.Add (p.Key, player);
							Material m = new Material (player.renderer.sharedMaterial);
							m.color = p.Key.color;
							player.renderer.material = m;
							player.hideFlags = HideFlags.HideAndDontSave;
						} else {
							players [p.Key].SetActive (true);
						}
					} else {
						if (players.ContainsKey (p.Key)) {
							players [p.Key].SetActive (false);
						}
					}
				}
			}
			
			#endregion
			
			EditorGUILayout.EndScrollView ();
			
			// ----------------------------------
			
			if (drawer != null) {
				drawer.timeSlice = timeSlice;
				drawer.drawHeatMap = drawHeatMap;
				drawer.drawMap = drawMap;
				drawer.drawFoVOnly = drawFoVOnly;
				drawer.drawNeverSeen = drawNeverSeen;
				drawer.drawPath = drawPath;
				drawer.paths = toggleStatus;
				
			}
			
			if (fullMap != null && lastTime != timeSlice) {
				lastTime = timeSlice;
				UpdatePositions (timeSlice, mapper);
			}
			
			SceneView.RepaintAll ();
		}
			
		public void Update () {
			if (playing) {
				long l = DateTime.Now.Ticks - previous.Ticks;
				playTime += l;
				accL += l;
				if (playTime > stepInTicks) {
					playTime -= stepInTicks;
					timeSlice++;
					if (timeSlice >= timeSamples) {
						timeSlice = 0;
					}
					drawer.timeSlice = timeSlice;
					SpaceState.Editor.timeSlice = timeSlice;
					UpdatePositions (timeSlice, mapper, 0f);
					accL += playTime;
				} else {
					UpdatePositions (timeSlice, mapper, (float)accL / (float)stepInTicks);
					accL = 0L;
				}
			}
				
			previous = DateTime.Now;
		}
		public void ReduceSpace()
		{
			//data holder
			Triangulation triangulation = GameObject.Find("Triangulation").GetComponent<Triangulation>(); 
			triangulation.Clear(); 
			
			if (floor == null) {
				floor = (GameObject)GameObject.Find ("Floor");
				
				if (mapper == null) {
					mapper = floor.GetComponent<Mapper> ();
					
					if (mapper == null)
						mapper = floor.AddComponent<Mapper> ();
					
					mapper.hideFlags = HideFlags.None;
					
				}
				drawer = floor.gameObject.GetComponent<MapperEditorDrawer> ();
				if (drawer == null) {
					drawer = floor.gameObject.AddComponent<MapperEditorDrawer> ();
					drawer.hideFlags = HideFlags.HideInInspector;
				}
			}
			
			Cell[][] obstacles = null; 
			
			
			mapper.ComputeTileSize (SpaceState.Editor, floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize);
			
			obstacles = mapper.ComputeObstacles ();
			
			//First geometry is the outer one
			List<Quadrilater> geos = new List<Quadrilater>();
			
			
			//Floor
			Vector3[] f = new Vector3[4];
			MeshFilter mesh = (MeshFilter)(floor.GetComponent("MeshFilter")) ;
			Vector3[] t = mesh.sharedMesh.vertices; 
			
			Quadrilater tempGeometry = new Quadrilater(); 
			
			
			tempGeometry.vertex[0] = mesh.transform.TransformPoint(t[0]);
			tempGeometry.vertex[2] = mesh.transform.TransformPoint(t[120]);
			tempGeometry.vertex[1] = mesh.transform.TransformPoint(t[110]);
			tempGeometry.vertex[3] = mesh.transform.TransformPoint(t[10]);
			
			tempGeometry.vertex[0].y = 1; 
			tempGeometry.vertex[1].y = 1; 
			tempGeometry.vertex[2].y = 1; 
			tempGeometry.vertex[3].y = 1; 
			
			tempGeometry.SetLine(); 
			
			geos.Add(tempGeometry); 
			
			GameObject[] obs = GameObject.FindGameObjectsWithTag("Obs");
			
			foreach(GameObject o in obs)
			{
				mesh = (MeshFilter)(o.GetComponent("MeshFilter")) ;
				t = mesh.sharedMesh.vertices; 
				
				tempGeometry = new Quadrilater(); 
				
				tempGeometry.vertex[0] = mesh.transform.TransformPoint(t[6]);
				tempGeometry.vertex[1] = mesh.transform.TransformPoint(t[8]);
				tempGeometry.vertex[3] = mesh.transform.TransformPoint(t[7]);
				tempGeometry.vertex[2] = mesh.transform.TransformPoint(t[9]);
				
				tempGeometry.vertex[0].y = 1; 
				tempGeometry.vertex[2].y = 1; 
				tempGeometry.vertex[1].y = 1; 
				tempGeometry.vertex[3].y = 1; 
				
				tempGeometry.SetLine(); 
				geos.Add(tempGeometry); 
			}


			//lines are defined by all the points in  obs
			List<Line> lines = new List<Line>(); 
			foreach(Quadrilater g in geos)
			{
				for(int i = 0; i< g.vertex.Length; i+=1)
				{
					if(i<g.vertex.Length -1)
						lines.Add(new Line(g.vertex[i],g.vertex[i+1]));
					else 	       
						lines.Add(new Line(g.vertex[0],g.vertex[i]));
					//triangulation.points.Add(g.vertex[i]);	      
					//triangulation.colours.Add(Color.cyan);	      
				}
				
			}




			
			List<Quadrilater> covered = new List<Quadrilater>(); 
			List<Line> linesLinking = new List<Line>(); 

			List<Quadrilater> toCheck = new List<Quadrilater>(); 

			for(int i = 1; i<geos.Count;i++)
			{
				toCheck.Add(geos[i]);
			}


			foreach(Quadrilater q in toCheck)
			{
				q.SetVoisins(toCheck);
				//q.DrawVoisin(); 
			}


			//Now link the closest one to the polygon.

			//Minimum spanning tree.


			Quadrilater start = geos[0].findClosestQuad(geos[0].vertex[0],toCheck,new List<Quadrilater>());
			linesLinking.Add(geos[0].GetClosestLine(start,toCheck));

			start.visited = true;

			List<Quadrilater> toCheckNode = new List<Quadrilater>(); 
			toCheckNode.Add(start); 



			Line LinetoAdd = start.voisinsLine[0];

			while(LinetoAdd != null)
			{
				LinetoAdd = null; 
				Quadrilater qToAdd =null; 

				//Check all 
				foreach(Quadrilater q in toCheckNode)
				{
					
					for(int i = 0; i<q.voisins.Count;i++)
				    {
						if(! q.voisins[i].visited)
						{
							if(LinetoAdd != null)
							{
								//get the shortest line
								if(LinetoAdd.Magnitude()>=q.voisinsLine[i].Magnitude())
								{
									LinetoAdd = q.voisinsLine[i];
									qToAdd = q.voisins[i]; 
									 
								}
							}
							else 
							{
								qToAdd = q.voisins[i]; 
								LinetoAdd = q.voisinsLine[i];
							}
						}
						else
						{
							continue; 
						}
					}
				}
				if(LinetoAdd!= null)
				{
					linesLinking.Add (LinetoAdd); 
					qToAdd.visited = true; 
					toCheckNode.Add(qToAdd); 
				}
			}



			foreach(Line l in linesLinking)
			{
				triangulation.linesMinSpanTree.Add(l); 
			}

			//Triangulate
			for (int i = 0; i < geos.Count; i++)
			{
				
				for(int j = i+1; j < geos.Count; j++)
				{
					
					for(int w = 0; w<geos[i].vertex.Length; w++)
					{
						
						for(int z = 0; z<geos[j].vertex.Length; z++)
						{
							
							List<Line> toAdd = new List<Line>(); 
							
							Boolean foundBreak = false; 
							
							foreach (Line l in lines)
							{
								
								if( LineIntersection(geos[i].vertex[w], geos[j].vertex[z],
								                     l.vertex[0],l.vertex[1]))
								{
									
									foundBreak = true; 
									break; 
								}								
								
							}
							if(!foundBreak)
							{	
								//Debug.DrawLine(geos[i].vertex[w], geos[j].vertex[z], Color.blue);
								lines.Add(new Line(geos[i].vertex[w], geos[j].vertex[z])); 		
							}	
						}
					}
				}
			}
			
			//Find the centers 
			List<Triangle> triangles = new List<Triangle>(); 
			//Well why be efficient when you can be not efficient
			foreach (Line l in lines)
			{
				Vector3 v1 = l.vertex[0]; 
				Vector3 v2 = l.vertex[1];
				foreach (Line l2 in lines)
				{
					if (l == l2)
						continue;
					Vector3 v3 = Vector3.zero; 
					
					
					if (l2.vertex[0].Equals(v2))
					{
						v3 = l2.vertex[1];
						//have to check if closes
					}
					else if (l2.vertex[1].Equals(v2))
					{
						v3 = l2.vertex[0];
					}
					
					if(v3 != Vector3.zero)
					{
						foreach (Line l3 in lines)
						{
							if(l3 == l2 || l3 == l)
								continue; 
							if( (l3.vertex[0].Equals(v1) && l3.vertex[1].Equals(v3))
							   || (l3.vertex[1].Equals(v1) && l3.vertex[0].Equals(v3)))
							{
								//Debug.DrawLine(v1,v2,Color.red); 
								//Debug.DrawLine(v2,v3,Color.red); 
								//Debug.DrawLine(v3,v1,Color.red); 
								
								//Add the traingle
								Triangle toAddTriangle = new Triangle(
									v1,triangulation.points.IndexOf(v1),
									v2,triangulation.points.IndexOf(v2),
									v3,triangulation.points.IndexOf(v3));
								
								
								Boolean isAlready = false; 
								foreach(Triangle tt in triangles)
								{
									if (tt.Equals(toAddTriangle))
									{
										//Debug.Log(toAddTriangle.refPoints[0]+", "+
										//          toAddTriangle.refPoints[1]+", "+
										//          toAddTriangle.refPoints[2]+", "); 
										isAlready = true; 
										break; 
									}
									
								}
								if(!isAlready)
								{
									triangles.Add(toAddTriangle);
								}
								
							}
						}
					}
				}
			}
			
			
			//Find shared edge and triangle structure
			
			foreach(Triangle tt in triangles)
			{
				foreach(Triangle ttt in triangles)
				{
					if(tt == ttt)
						continue; 
					tt.ShareEdged(ttt,linesLinking);
					
				}
				
			}

			triangles[0].SetColour();

			//Count Where to put guards 
			List<Vector3> points = new List<Vector3>(); 
			List<Color> coloursPoints = new List<Color>(); 

			int[] count = new int[3];
			//0 red, 1 blue, 2 green

			foreach(Triangle tt in triangles)
			{
				//foreach(Vector3 v in tt.vertex)
				for(int j = 0; j<tt.vertex.Length;j++)
				{
					bool vectorToAdd = true;

					for(int i = 0; i<points.Count;i++)
				    {
						if(points[i] == tt.vertex[j] && coloursPoints[i] == tt.colourVertex[j])
							vectorToAdd = false; 

					
					}

					if(vectorToAdd)
					{
						points.Add(tt.vertex[j]); 
						coloursPoints.Add(tt.colourVertex[j]); 
					}

				}
			}

			foreach(Color c in coloursPoints)
			{
				if(c == Color.red)
					count[0]++; 
				else if(c == Color.blue)
					count[1]++; 
				else
					count[2]++; 
					
			}
			//Debug.Log(count[0]); 
			//Debug.Log(count[1]); 
			//Debug.Log(count[2]); 


			//Get points with the least colour
			Color cGuard = Color.cyan; 
			int lowest = 100000000; 

			for(int i = 0;i<count.Length;i++)
			{
				if(count[i]<lowest)
				{
					if(i == 0)
						cGuard = Color.red;
					else if(i == 1)
						cGuard = Color.blue;
					else
						cGuard = Color.green;
					lowest = count[i];
				}
			}

			List<Vector3> posGuard = new List<Vector3>(); 

			for(int i = 0;i<coloursPoints.Count;i++)
			{
				if (coloursPoints[i] == cGuard)
				{
					triangulation.AddPoint(points[i]);
					posGuard.Add(points[i]);
				}
			}

			triangulation.triangles = triangles; 


			//Put the cameras
			GameObject guard = GameObject.Find("Enemy") as GameObject;
			GameObject waypoint = GameObject.Find("Waypoint");
			
			//find enemies and clear them
			//put them in a place holder

			GameObject holder = GameObject.Find("Enemies");
			GameObject wayHolder = GameObject.Find("Waypoints");
			//Clear the enemies

			if(holder == null)
			{	
			 	holder = new GameObject(); 
				holder.name = "Enemies";
			}
			if(wayHolder == null)
			{	
			 	wayHolder = new GameObject(); 
				wayHolder.name = "Waypoints";
			}    
			//Clearing the place for new enemies
			GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
			GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Waypoint");

			//Clear the enemies

		    foreach (GameObject g in enemies) 
		    {
				DestroyImmediate(g);
			}
			foreach (GameObject g in waypoints) 
		    {
				DestroyImmediate(g);
			}	
			    
		    


			foreach(Vector3 v in posGuard)
			{
				GameObject g = Instantiate(guard,v,Quaternion.identity) as GameObject;	
				g.transform.parent = holder.transform; 
				g.tag = "Enemy";
				
				List<GameObject>ways = new List<GameObject>(); 
				
				//Set orientation
				//Find obstacle linked to. 
				foreach(Quadrilater q in geos)
				{
					Line[] lView = q.GetLine(v);  
					GameObject way = null; 
					if(lView.Length>0 )
					{
						foreach(Line ll in lView)
						{

							way = Instantiate(waypoint,ll.GetOther(v),
								Quaternion.identity) as GameObject;
							way.transform.parent = wayHolder.transform;
							way.tag = "Waypoint";
							//ll.DrawLine(Color.red); 
							ways.Add(way);
						}
						Vector3 center = q.GetCenterQuad();
						Vector3 toAdd = (center - v).normalized;

						if(geos[0] == q)
							toAdd = (v - center).normalized;							

						way = Instantiate(waypoint,v - (toAdd ),
							Quaternion.identity) as GameObject;
						way.transform.parent = wayHolder.transform;
						way.tag = "Waypoint";

						ways.Add(way);

						way = Instantiate(waypoint,v - (toAdd ),
							Quaternion.identity) as GameObject;
						
						way.transform.parent = wayHolder.transform;
						way.tag = "Waypoint";

						ways.Add(way);

						//Connect the waypoints
						break;
					}

				}
				//for(int i = 0; i<ways.Count;i++)
				//{
				

				RotationWaypoint wGauche = ways[0].GetComponent<RotationWaypoint>();
				RotationWaypoint wDroite = ways[1].GetComponent<RotationWaypoint>();

				RotationWaypoint wDevant1 = ways[2].GetComponent<RotationWaypoint>();
				RotationWaypoint wDevant2 = ways[3].GetComponent<RotationWaypoint>();

				wGauche.gameObject.name ="Gauche";
				wDroite.gameObject.name ="Droite";
				wDevant1.gameObject.name ="Devant1";
				wDevant2.gameObject.name ="Devant2";

				wGauche.next = wDevant1; 
				wDevant1.next = wDroite;
				wDroite.next = wDevant2;
				wDevant2.next = wGauche; 

				wGauche.lookDir = (wGauche.transform.position - v).normalized; 
				wDroite.lookDir = (wDroite.transform.position - v).normalized; 
				wDevant1.lookDir = (wDevant1.transform.position - v).normalized; 
				wDevant2.lookDir = (wDevant2.transform.position - v).normalized; 
				
				Enemy e = g.GetComponent<Enemy>();
				e.target = wDevant1; 
				Vector3 temp = (wDevant1.transform.position - v).normalized;
				temp *= 0.3f; 

				g.transform.position = v + temp;

				for(int i = 0; i<1000;i++)
				{
					bool free = true; 
					foreach(Quadrilater q in geos)
					{
						if(q == geos[0])
							continue; 
						if(q.Collide(g.transform.position))
							free = false; 
						if(!free)
							break;
					}

					if(free)
						break; 

					//Try a new position at random.

					temp = new Vector3(UnityEngine.Random.Range(-1f,1f),0, UnityEngine.Random.Range(-1f,1f));
					temp.Normalize(); 
					temp *= 0.3f; 

					g.transform.position = v + temp;
					
				}
				


				//}	

			}
		}

		public void QuadrilateralizationSpace()
		{
			//data holder
			Triangulation triangulation = GameObject.Find("Triangulation").GetComponent<Triangulation>(); 
			triangulation.points.Clear();
			triangulation.colours.Clear(); 

			if (floor == null) {
				floor = (GameObject)GameObject.Find ("Floor");
				
				if (mapper == null) {
					mapper = floor.GetComponent<Mapper> ();
					
					if (mapper == null)
						mapper = floor.AddComponent<Mapper> ();
					
					mapper.hideFlags = HideFlags.None;
					
				}
				drawer = floor.gameObject.GetComponent<MapperEditorDrawer> ();
				if (drawer == null) {
					drawer = floor.gameObject.AddComponent<MapperEditorDrawer> ();
					drawer.hideFlags = HideFlags.HideInInspector;
				}
			}
			
			Cell[][] obstacles = null; 
			
			
			mapper.ComputeTileSize (SpaceState.Editor, floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize);
			
			obstacles = mapper.ComputeObstacles ();
			
			//First geometry is the outer one
			List<Quadrilater> geos = new List<Quadrilater>();
			
			
			//Floor
			Vector3[] f = new Vector3[4];
			MeshFilter mesh = (MeshFilter)(floor.GetComponent("MeshFilter")) ;
			Vector3[] t = mesh.sharedMesh.vertices; 
			
			Quadrilater tempGeometry = new Quadrilater(); 
			
			
			tempGeometry.vertex[0] = mesh.transform.TransformPoint(t[0]);
			tempGeometry.vertex[2] = mesh.transform.TransformPoint(t[120]);
			tempGeometry.vertex[1] = mesh.transform.TransformPoint(t[110]);
			tempGeometry.vertex[3] = mesh.transform.TransformPoint(t[10]);
			
			tempGeometry.vertex[0].y = 1; 
			tempGeometry.vertex[1].y = 1; 
			tempGeometry.vertex[2].y = 1; 
			tempGeometry.vertex[3].y = 1; 
			
			tempGeometry.SetLine(); 

			geos.Add(tempGeometry); 

			GameObject[] obs = GameObject.FindGameObjectsWithTag("Obs");

			foreach(GameObject o in obs)
			{
				mesh = (MeshFilter)(o.GetComponent("MeshFilter")) ;
				t = mesh.sharedMesh.vertices; 
				
				tempGeometry = new Quadrilater(); 
				
				tempGeometry.vertex[0] = mesh.transform.TransformPoint(t[6]);
				tempGeometry.vertex[1] = mesh.transform.TransformPoint(t[8]);
				tempGeometry.vertex[3] = mesh.transform.TransformPoint(t[7]);
				tempGeometry.vertex[2] = mesh.transform.TransformPoint(t[9]);
				
				tempGeometry.vertex[0].y = 1; 
				tempGeometry.vertex[2].y = 1; 
				tempGeometry.vertex[1].y = 1; 
				tempGeometry.vertex[3].y = 1; 
				
				tempGeometry.SetLine(); 
				geos.Add(tempGeometry); 
			}
			//lines are defined by all the points in  obs
			List<Line> lines = new List<Line>(); 
			foreach(Quadrilater g in geos)
			{
				for(int i = 0; i< g.vertex.Length; i+=1)
				{
					if(i<g.vertex.Length -1)
						lines.Add(new Line(g.vertex[i],g.vertex[i+1]));
					else 	       
						lines.Add(new Line(g.vertex[0],g.vertex[i]));
					//triangulation.points.Add(g.vertex[i]);	      
					//triangulation.colours.Add(Color.cyan);	      
				}
				
			}


			 

			List<Quadrilater> candidates = getQuadrilater(lines,geos.ToArray()); 

			foreach(Quadrilater q in candidates)
			{
				triangulation.AddPoint(q.GetCenterQuad(),q.c); 
				q.DrawDebug(); 
			}

			Debug.Log(candidates.Count); 
		}


		public List<Quadrilater> getQuadrilater(List<Line> lines,Quadrilater[] geos)
		{
			List<Quadrilater> candidates = new List<Quadrilater>(); 
			
			//Starting line



			List<Line> linesUpdated = new List<Line>();

			//foreach(Line l in lines)
			//	linesUpdated.Add(l); 

			foreach(Line l in lines)
			//for(int i = 0; i<linesUpdated.Count;i++)
			{
				//Line l = linesUpdated[i];
				//Otherside line
				Quadrilater toAddQuad = null ; 
				Line l1 = null;
				Line l2 = null; 

				//foreach(Line ll in linesUpdated)
				foreach(Line ll in lines)
				{
					if(ll.ShareVertex(l) || l == ll)
						continue; 
					
					l1 = new Line(l.vertex[0],ll.vertex[0]);
					l2 = new Line(l.vertex[1],ll.vertex[1]); 
					
					//Cannot cross
					if(l1.LineIntersection(l2))
					{
						l1 = new Line(l.vertex[1],ll.vertex[0]);
						l2 = new Line(l.vertex[0],ll.vertex[1]);
					}
					var r1 = new Line(l1.vertex[0],l2.vertex[1]);
					var r2 = new Line(l1.vertex[1],l2.vertex[0]); 
					
					if(!r1.LineIntersection(r2))
						continue; 
					
					
					//cannot be a duplicate
					bool duplicate = false; 

					foreach(Line lCheck in lines)
					{	
						
						if(lCheck == l || ll == lCheck ) 
							continue; 
						//lCheck.DrawLine(Color.green);
						
						if( l1 == lCheck || l2 == lCheck ||
						    lCheck.LineIntersection(l1) || lCheck.LineIntersection(l2) 
						    ||
							lCheck.LineIntersection(r1) || lCheck.LineIntersection(r2) 
						   )	
						{
							
							duplicate = true; 
							break; 
						}
					}

					
					
					if(duplicate)
						continue; 
					
					Quadrilater tQuad = new Quadrilater(l,ll,l1,l2); 

					//tQuad.DrawDebug(); 	

					foreach(Quadrilater q in candidates)
					{
						foreach(Line lQ in q.lines)
						{
							if(lQ.LineIntersection(l2) || lQ.LineIntersection(l1))
							{
								duplicate = true; 
								break; 
							}
						}
					}
					
					if(duplicate)
						continue; 
					
					
					
					foreach(Quadrilater q in candidates)
					{
						if(q.Collide(tQuad) || q.Equals(tQuad))
						{	
							duplicate = true;
							break; 
						}

					}

					foreach(Quadrilater q in geos)
					{
						if(q.Equals(tQuad))
						{	
							duplicate = true;
							break; 
						}
						
					}
					if(duplicate)
						continue; 

					//tQuad.DrawDebug();

					if(toAddQuad != null)
					{
						if(tQuad.GetLength() < toAddQuad.GetLength())
						{
							toAddQuad = tQuad; 
						}
					}
					else
						toAddQuad = tQuad; 

					

					//break;
				}
				if(toAddQuad!=null && !candidates.Contains(toAddQuad) && ! geos.Contains(toAddQuad))
				{ 

					candidates.Add(toAddQuad);
					candidates[candidates.Count-1].DrawDebug(); 
					linesUpdated.Add(l1); 
					linesUpdated.Add(l2); 
				}




			}
			return candidates;
		}

		public void TriangulationSpace()
		{
			//Compute one step of the discritzation
			//Find this is the view

			if (floor == null) {
				floor = (GameObject)GameObject.Find ("Floor");
				
				if (mapper == null) {
					mapper = floor.GetComponent<Mapper> ();
					
					if (mapper == null)
						mapper = floor.AddComponent<Mapper> ();
					
					mapper.hideFlags = HideFlags.None;
					
				}
				drawer = floor.gameObject.GetComponent<MapperEditorDrawer> ();
				if (drawer == null) {
					drawer = floor.gameObject.AddComponent<MapperEditorDrawer> ();
					drawer.hideFlags = HideFlags.HideInInspector;
				}
			}
			
			Cell[][] obstacles = null; 


			mapper.ComputeTileSize (SpaceState.Editor, floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize);

			obstacles = mapper.ComputeObstacles ();

			//First geometry is the outer one
			List<Geometry> geos = new List<Geometry>();


			//Floor
			Vector3[] f = new Vector3[4];
			MeshFilter mesh = (MeshFilter)(floor.GetComponent("MeshFilter")) ;
			Vector3[] t = mesh.sharedMesh.vertices; 

			Geometry tempGeometry = new Geometry(); 


			tempGeometry.vertex[0] = mesh.transform.TransformPoint(t[0]);
			tempGeometry.vertex[2] = mesh.transform.TransformPoint(t[120]);
			tempGeometry.vertex[1] = mesh.transform.TransformPoint(t[110]);
			tempGeometry.vertex[3] = mesh.transform.TransformPoint(t[10]);

			tempGeometry.vertex[0].y = 1; 
			tempGeometry.vertex[1].y = 1; 
			tempGeometry.vertex[2].y = 1; 
			tempGeometry.vertex[3].y = 1; 

			
			geos.Add(tempGeometry);



			GameObject[] obs = GameObject.FindGameObjectsWithTag("Obs");

			//data holder
			Triangulation triangulation = GameObject.Find("Triangulation").GetComponent<Triangulation>(); 
			triangulation.points.Clear();
			triangulation.colours.Clear(); 

			//Only one geometry for now
			
			foreach(GameObject o in obs)
			{
				mesh = (MeshFilter)(o.GetComponent("MeshFilter")) ;
				t = mesh.sharedMesh.vertices; 
				
				tempGeometry = new Geometry(); 
				
				tempGeometry.vertex[0] = mesh.transform.TransformPoint(t[6]);
				tempGeometry.vertex[1] = mesh.transform.TransformPoint(t[8]);
				tempGeometry.vertex[3] = mesh.transform.TransformPoint(t[7]);
				tempGeometry.vertex[2] = mesh.transform.TransformPoint(t[9]);

				tempGeometry.vertex[0].y = 1; 
				tempGeometry.vertex[2].y = 1; 
				tempGeometry.vertex[1].y = 1; 
				tempGeometry.vertex[3].y = 1; 
				
				
				geos.Add(tempGeometry); 

			}

			//lines are defined by all the points in  obs
			List<Line> lines = new List<Line>(); 
			foreach(Geometry g in geos)
			{
				for(int i = 0; i< g.vertex.Length; i+=1)
			    {
					if(i<g.vertex.Length -1)
						lines.Add(new Line(g.vertex[i],g.vertex[i+1]));
					else 	       
						lines.Add(new Line(g.vertex[0],g.vertex[i]));
					//triangulation.points.Add(g.vertex[i]);	      
					//triangulation.colours.Add(Color.cyan);	      
				}

			}

			foreach (Line l in lines)
			{
				//Debug.DrawLine(l.vertex[0],l.vertex[1],Color.blue);
				
			}
			//Lines are also the one added. 

			//Compare each point to every point 

			for (int i = 0; i < geos.Count; i++)
			{

				for(int j = i+1; j < geos.Count; j++)
				{

					for(int w = 0; w<geos[i].vertex.Length; w++)
					{

						for(int z = 0; z<geos[j].vertex.Length; z++)
						{
													
							List<Line> toAdd = new List<Line>(); 

							Boolean foundBreak = false; 

							foreach (Line l in lines)
							{

								if( LineIntersection(geos[i].vertex[w], geos[j].vertex[z],
								                           l.vertex[0],l.vertex[1]))
								{
									 
									foundBreak = true; 
									break; 
								}								
								   
							}
							if(!foundBreak)
							{	
								//Debug.DrawLine(geos[i].vertex[w], geos[j].vertex[z], Color.blue);
								lines.Add(new Line(geos[i].vertex[w], geos[j].vertex[z])); 		
							}	
						}
					}
				}
			}

			//Find the centers 
			List<Triangle> triangles = new List<Triangle>(); 
			//Well why be efficient when you can be not efficient
			foreach (Line l in lines)
			{
				Vector3 v1 = l.vertex[0]; 
				Vector3 v2 = l.vertex[1];
				foreach (Line l2 in lines)
				{
					if (l == l2)
						continue;
					Vector3 v3 = Vector3.zero; 


					if (l2.vertex[0].Equals(v2))
					{
						v3 = l2.vertex[1];
						//have to check if closes
					}
					else if (l2.vertex[1].Equals(v2))
					{
						v3 = l2.vertex[0];
					}

					if(v3 != Vector3.zero)
					{
						foreach (Line l3 in lines)
						{
							if(l3 == l2 || l3 == l)
								continue; 
							if( (l3.vertex[0].Equals(v1) && l3.vertex[1].Equals(v3))
							   || (l3.vertex[1].Equals(v1) && l3.vertex[0].Equals(v3)))
							{
								//Debug.DrawLine(v1,v2,Color.red); 
								//Debug.DrawLine(v2,v3,Color.red); 
								//Debug.DrawLine(v3,v1,Color.red); 

								//Add the traingle
								Triangle toAddTriangle = new Triangle(
									v1,triangulation.points.IndexOf(v1),
									v2,triangulation.points.IndexOf(v2),
									v3,triangulation.points.IndexOf(v3));
								                                       

								Boolean isAlready = false; 
								foreach(Triangle tt in triangles)
								{
									if (tt.Equals(toAddTriangle))
									{
										//Debug.Log(toAddTriangle.refPoints[0]+", "+
										//          toAddTriangle.refPoints[1]+", "+
										//          toAddTriangle.refPoints[2]+", "); 
										isAlready = true; 
										break; 
									}

								}
								if(!isAlready)
								{
									triangles.Add(toAddTriangle);
								}
									
							}
						}
					}
				}
			}


			//Find shared edge and triangle structure

			foreach(Triangle tt in triangles)
			{
				foreach(Triangle ttt in triangles)
				{
					if(tt == ttt)
						continue; 
					tt.ShareEdged(ttt);
					
				}

			}

			triangulation.triangles = triangles; 



		}

		private Boolean LineIntersect(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
		{
			//Debug.Log(a); 
			//Debug.Log(b); 
			//Debug.Log(c); 
			//Debug.Log(d); 

			Vector2 u = new Vector2(b.x,b.z) - new Vector2(a.x,a.z);
			Vector2 p0 = new Vector2(a.x,a.z); Vector2 p1 = new Vector2(b.x,b.z); 

			Vector2 v = new Vector2(d.x,d.z) - new Vector2(c.x,c.z);
			Vector2 q0 = new Vector2(c.x,c.z); Vector2 q1 = new Vector2(d.x,d.z);

			Vector2 w = new Vector2(a.x,a.z) - new Vector2(d.x,d.z);


			//if (u.x * v.y - u.y*v.y == 0)
			//	return true;

			double s = (v.y* w.x - v.x*w.y) / (v.x*u.y - v.y*u.x);
			double t = (u.x*w.y-u.y*w.x) / (u.x*v.y- u.y*v.x); 
			//Debug.Log(s); 
			//Debug.Log(t); 

			if ( (s>0 && s< 1) && (t>0 && t< 1) )
				return true;

			return false; 
		}

		private Boolean LineIntersection(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
		{



			// a-b
			// c-d
			//if the same lines

			//When share a point use the other algo
			if(a.Equals(c) || a.Equals(d) || b.Equals(c) || b.Equals(d))
				return LineIntersect(a,b,c,d); 




			return CounterClockWise(a,c,d) != CounterClockWise(b,c,d) && 
				CounterClockWise(a,b,c) != CounterClockWise(a,b,d);

			//if( CounterClockWise(a,c,d) == CounterClockWise(b,c,d))
			//	return false;
			//else if (CounterClockWise(a,b,c) == CounterClockWise(a,b,d))
			//	return false; 
			//else 
			//	return true; 


		}

		private Boolean CounterClockWise(Vector3 v1,Vector3 v2,Vector3 v3)
		{
			//v1 = a,b
			//v2 = c,d
			//v3 = e,f

			float a = v1.x, b = v1.z;  
			float c = v2.x, d = v2.z;  
			float e = v3.x, f = v3.z;  

			if((f-b)*(c-a)> (d-b)*(e-a))
				return true;
			else
				return false; 
		}


		private void ClearPathsRepresentation () {
			toggleStatus.Clear ();
			
			foreach (GameObject obj in players.Values)
				GameObject.DestroyImmediate (obj);
				
			players.Clear ();
			Resources.UnloadUnusedAssets ();
		}
		
		private void SetupArrangedPaths (List<Path> paths) {
			arrangedByTime = new List<Path> ();
			arrangedByTime.AddRange (paths);
			arrangedByTime.Sort (new Analyzer.TimeComparer ());
				
			arrangedByLength = new List<Path> ();
			arrangedByLength.AddRange (paths);
			arrangedByLength.Sort (new Analyzer.Length2dComparer ());
				
			arrangedByDanger = new List<Path> ();
			arrangedByDanger.AddRange (paths);
			arrangedByDanger.Sort (new Analyzer.DangerComparer ());
				
			arrangedByDanger3 = new List<Path> ();
			arrangedByDanger3.AddRange (paths);
			arrangedByDanger3.Sort (new Analyzer.Danger3Comparer ());
				
			arrangedByDanger3Norm = new List<Path> ();
			arrangedByDanger3Norm.AddRange (paths);
			arrangedByDanger3Norm.Sort (new Analyzer.Danger3NormComparer ());
				
			arrangedByLoS = new List<Path> ();
			arrangedByLoS.AddRange (paths);
			arrangedByLoS.Sort (new Analyzer.LoSComparer ());
				
			arrangedByLoS3 = new List<Path> ();
			arrangedByLoS3.AddRange (paths);
			arrangedByLoS3.Sort (new Analyzer.LoS3Comparer ());
				
			arrangedByLoS3Norm = new List<Path> ();
			arrangedByLoS3Norm.AddRange (paths);
			arrangedByLoS3Norm.Sort (new Analyzer.LoS3NormComparer ());
				
			arrangedByCrazy = new List<Path> ();
			arrangedByCrazy.AddRange (paths);
			arrangedByCrazy.Sort (new Analyzer.CrazyComparer ());
				
			arrangedByVelocity = new List<Path> ();
			arrangedByVelocity.AddRange (paths);
			arrangedByVelocity.Sort (new Analyzer.VelocityComparer ());
		}
		
		private void ComputeHeatMap (List<Path> paths) {
			heatMap = Analyzer.Compute2DHeatMap (paths, gridSize, gridSize, out maxHeatMap);
				
			drawer.heatMapMax = maxHeatMap;
			drawer.heatMap = heatMap;
				
			int[] maxHeatMap3d;
			drawer.heatMap3d = Analyzer.Compute3DHeatMap (paths, gridSize, gridSize, timeSamples, out maxHeatMap3d);

			drawer.heatMapMax3d = maxHeatMap3d;

			drawer.rrtMap = rrt.explored;
			drawer.tileSize.Set (SpaceState.Editor.tileSize.x, SpaceState.Editor.tileSize.y);
		}

		private void ComputeClusters () {
			if (MapperEditor.grid != null) {
				Dictionary<int, List<Path>> clusterMap = new Dictionary<int, List<Path>> ();
				foreach (Path currentPath in paths) {
					
					Node cur = currentPath.points [currentPath.points.Count - 1];
					Node par = cur.parent;
					while (cur.parent != null) {
						
						Vector3 p1 = cur.GetVector3 ();
						Vector3 p2 = par.GetVector3 ();
						Vector3 pd = p1 - p2;
						
						float pt = (cur.t - par.t);
						
						// Navigate through time to find the right cells to start from
						for (int t = 0; t < pt; t++) {
							
							float delta = ((float)t) / pt;
							
							Vector3 pos = p2 + pd * delta;
							int pX = Mathf.FloorToInt (pos.x);
							int pY = Mathf.FloorToInt (pos.z);
							
							short i = 1;
							if (fullMap [par.t + t] [pX] [pY].cluster > 0) {
								
								while (i <= 256) {
									if ((fullMap [par.t + t] [pX] [pY].cluster & i) > 0) {
										List<Path> inside;
										clusterMap.TryGetValue (i, out inside);
										
										if (inside == null) {
											inside = new List<Path> ();
											clusterMap.Add (i, inside);
										}
										
										if (!inside.Contains (currentPath))
											inside.Add (currentPath);
									}
									
									
									i *= 2;
								}
							}
						}
						
						cur = par;
						par = par.parent;
					}
				}
				
				ClustersRoot root = new ClustersRoot ();
				int j = 0;//for colours
				foreach (int n in clusterMap.Keys) {
					MetricsRoot cluster = new MetricsRoot ();
					cluster.number = n + "";
					
					
					foreach (Path path in clusterMap[n]) {
						cluster.everything.Add (new PathResults (path, null));
						switch (j) {
						case 0:
							path.color = Color.red;
							break; 
						case 1:
							path.color = Color.blue;
							break; 
						case 2:
							path.color = Color.green;
							break; 
						case 3:
							path.color = Color.magenta;
							break; 
						}
					}
					j++; 
					root.everything.Add (cluster);
				}
				
				XmlSerializer ser = new XmlSerializer (typeof(ClustersRoot), new Type[] {
					typeof(MetricsRoot),
					typeof(PathResults),
					typeof(PathValue),
					typeof(Value)
				});
				
				using (FileStream stream = new FileStream ("clusterresults.xml", FileMode.Create)) {
					ser.Serialize (stream, root);
					stream.Flush ();
					stream.Close ();
				}
			}
		}
	
		private void BatchComputing () {
			ResultsRoot root = new ResultsRoot ();
				
			float speed = GameObject.FindGameObjectWithTag ("AI").GetComponent<Player> ().speed;
				
			int gridsize, timesamples, rrtattemps, att = 1;
				
			//for (int att = 1; att <= 31; att++) {
				
			// Grid varying batch tests
				
			using (FileStream stream = new FileStream ("gridvary" + att + ".xml", FileMode.Create)) {
					
				rrtattemps = 30000;
				timesamples = 1200;
					
				for (gridsize = 60; gridsize <= 160; gridsize += 5) {
						
					Debug.Log ("Gridsize attemps " + gridsize + " Memory: " + GC.GetTotalMemory (true) + " Date: " + System.DateTime.Now.ToString ());
								
					fullMap = mapper.PrecomputeMaps (SpaceState.Editor, floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridsize, timesamples, stepSize);
								
					startX = (int)((start.transform.position.x - floor.collider.bounds.min.x) / SpaceState.Editor.tileSize.x);
					startY = (int)((start.transform.position.z - floor.collider.bounds.min.z) / SpaceState.Editor.tileSize.y);
					endX = (int)((end.transform.position.x - floor.collider.bounds.min.x) / SpaceState.Editor.tileSize.x);
					endY = (int)((end.transform.position.z - floor.collider.bounds.min.z) / SpaceState.Editor.tileSize.y);
	
					ResultBatch job = new ResultBatch ();
					job.gridSize = gridsize;
					job.timeSamples = timesamples;
					job.rrtAttemps = rrtattemps;
								
					TimeSpan average = new TimeSpan (0, 0, 0, 0, 0);
					List<Node> path = null;
								
					for (int it = 0; it < 155;) {
						Result single = new Result ();
									
						DateTime before = System.DateTime.Now;
						path = rrt.Compute (startX, startY, endX, endY, rrtattemps, speed, fullMap, false);
						rrt.tree = null;
						DateTime after = System.DateTime.Now;
						TimeSpan delta = after - before;
									
						average += delta;
						single.timeSpent = delta.TotalMilliseconds;
									
						if (path != null && path.Count > 0) {
							it++;
							job.results.Add (single);
						}
						job.totalTries++;
						if (job.totalTries > 100 && job.results.Count == 0)
							it = 200;
					}
						
					// Force the garbage collector to pick this guy before instantiating the next map to avoid memory leak in the Large Object Heap
					fullMap = null;
								
					job.averageTime = (double)average.TotalMilliseconds / (double)job.totalTries;
								
					root.everything.Add (job);
						
				}
					
				Debug.Log ("Serializing 1");
					
				XmlSerializer ser = new XmlSerializer (typeof(ResultsRoot), new Type[] {
					typeof(ResultBatch),
					typeof(Result)
				});
				ser.Serialize (stream, root);
				stream.Flush ();
				stream.Close ();
			}
				
			// Time varying batch tests
				
			using (FileStream stream = new FileStream ("timevary" + att + ".xml", FileMode.Create)) {
				
				root.everything.Clear ();
					
				gridsize = 60;
				rrtattemps = 30000;
					
				for (timesamples = 500; timesamples <= 3500; timesamples += 100) {
						
					Debug.Log ("Timesamples attemps " + timesamples + " Memory: " + GC.GetTotalMemory (true) + " Date: " + System.DateTime.Now.ToString ());
						
					fullMap = mapper.PrecomputeMaps (SpaceState.Editor, floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timesamples, stepSize);
						
					startX = (int)((start.transform.position.x - floor.collider.bounds.min.x) / SpaceState.Editor.tileSize.x);
					startY = (int)((start.transform.position.z - floor.collider.bounds.min.z) / SpaceState.Editor.tileSize.y);
					endX = (int)((end.transform.position.x - floor.collider.bounds.min.x) / SpaceState.Editor.tileSize.x);
					endY = (int)((end.transform.position.z - floor.collider.bounds.min.z) / SpaceState.Editor.tileSize.y);
						
					ResultBatch job = new ResultBatch ();
					job.gridSize = gridsize;
					job.timeSamples = timesamples;
					job.rrtAttemps = rrtattemps;
						
					TimeSpan average = new TimeSpan (0, 0, 0, 0, 0);
					List<Node> path = null;
						
					for (int it = 0; it < 155;) {
						Result single = new Result ();
							
						DateTime before = System.DateTime.Now;
						path = rrt.Compute (startX, startY, endX, endY, rrtattemps, speed, fullMap, false);
						DateTime after = System.DateTime.Now;
						TimeSpan delta = after - before;
							
						average += delta;
						single.timeSpent = delta.TotalMilliseconds;
							
						if (path.Count > 0) {
							it++;
							job.results.Add (single);
						}
						job.totalTries++;
						if (job.totalTries > 100 && job.results.Count == 0)
							it = 200;
					}
					fullMap = null;
						
					job.averageTime = (double)average.TotalMilliseconds / (double)job.totalTries;
						
					root.everything.Add (job);
						
				}
					
				Debug.Log ("Serializing 2");
					
				XmlSerializer ser = new XmlSerializer (typeof(ResultsRoot), new Type[] {
					typeof(ResultBatch),
					typeof(Result)
				});
				ser.Serialize (stream, root);
				stream.Flush ();
				stream.Close ();
			}
				
			// Attemp varying batch tests
				
			using (FileStream stream = new FileStream ("attempvary" + att + ".xml", FileMode.Create)) {
				
				root.everything.Clear ();
					
				gridsize = 60;
				timesamples = 1200;
	
				fullMap = mapper.PrecomputeMaps (SpaceState.Editor, floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timesamples, stepSize);
					
				startX = (int)((start.transform.position.x - floor.collider.bounds.min.x) / SpaceState.Editor.tileSize.x);
				startY = (int)((start.transform.position.z - floor.collider.bounds.min.z) / SpaceState.Editor.tileSize.y);
				endX = (int)((end.transform.position.x - floor.collider.bounds.min.x) / SpaceState.Editor.tileSize.x);
				endY = (int)((end.transform.position.z - floor.collider.bounds.min.z) / SpaceState.Editor.tileSize.y);
					
				for (rrtattemps = 5000; rrtattemps <= 81000; rrtattemps += 3000) {
						
					Debug.Log ("RRT attemps " + rrtattemps + " Memory: " + GC.GetTotalMemory (true) + " Date: " + System.DateTime.Now.ToString ());
	
					ResultBatch job = new ResultBatch ();
					job.gridSize = gridsize;
					job.timeSamples = timesamples;
					job.rrtAttemps = rrtattemps;
	
					TimeSpan average = new TimeSpan (0, 0, 0, 0, 0);
					List<Node> path = null;
	
					for (int it = 0; it < 155;) {
						Result single = new Result ();
							
						DateTime before = System.DateTime.Now;
						path = rrt.Compute (startX, startY, endX, endY, rrtattemps, speed, fullMap, false);
						DateTime after = System.DateTime.Now;
						TimeSpan delta = after - before;
							
						average += delta;
						single.timeSpent = delta.TotalMilliseconds;
							
						if (path.Count > 0) {
							it++;
							job.results.Add (single);
						}
						job.totalTries++;
						if (job.totalTries > 100 && job.results.Count == 0)
							it = 200;
					}
						
					job.averageTime = (double)average.TotalMilliseconds / (double)job.totalTries;
							
					root.everything.Add (job);
						
				}
					
				fullMap = null;
					
				XmlSerializer ser = new XmlSerializer (typeof(ResultsRoot), new Type[] {
					typeof(ResultBatch),
					typeof(Result)
				});
				ser.Serialize (stream, root);
				stream.Flush ();
				stream.Close ();
			}
				
			//}
		}
		
		// Resets the AI back to it's original position
		private void ResetAI () {
			GameObject[] objs = GameObject.FindGameObjectsWithTag ("AI") as GameObject[];
			foreach (GameObject ob in objs)
				ob.GetComponent<Player> ().ResetSimulation ();
			
			objs = GameObject.FindGameObjectsWithTag ("Enemy") as GameObject[];
			foreach (GameObject ob in objs) 
				ob.GetComponent<Enemy> ().ResetSimulation ();
			
			timeSlice = 0;
			
			
		}
		
		// Updates everyone's position to the current timeslice
		private void UpdatePositions (int t, Mapper mapper, float diff = 0f) {
			for (int i = 0; i < SpaceState.Editor.enemies.Length; i++) {
				if (SpaceState.Editor.enemies [i] == null)
					continue;
				
				Vector3 pos;
				Quaternion rot;
				
				if (t == 0 || diff == 0) {
					pos = SpaceState.Editor.enemies [i].positions [t];
					rot = SpaceState.Editor.enemies [i].rotations [t];	
				} else {
					pos = SpaceState.Editor.enemies [i].transform.position;
					rot = SpaceState.Editor.enemies [i].transform.rotation;
				}
				
				if (diff > 0 && t + 1 < SpaceState.Editor.enemies [i].positions.Length) {
					pos += (SpaceState.Editor.enemies [i].positions [t + 1] - SpaceState.Editor.enemies [i].positions [t]) * diff;
					//rot = Quaternion.Lerp(rot, SpaceState.Editor.enemies[i].rotations[t+1], diff);
				}
				
				SpaceState.Editor.enemies [i].transform.position = pos;
				SpaceState.Editor.enemies [i].transform.rotation = rot;
			}
			
			foreach (KeyValuePair<Path, GameObject> each in players) {
				bool used = false;
				toggleStatus.TryGetValue (each.Key, out used);
				if (used) {
					Node p = null;
					foreach (Node n in each.Key.points) {
						if (n.t > t) {
							p = n;
							break;
						}
					}
					if (p != null) {
						Vector3 n2 = p.GetVector3 ();
						Vector3 n1 = p.parent.GetVector3 ();
						Vector3 pos = n1 * (1 - (float)(t - p.parent.t) / (float)(p.t - p.parent.t)) + n2 * ((float)(t - p.parent.t) / (float)(p.t - p.parent.t));
						
						pos.x *= SpaceState.Editor.tileSize.x;
						pos.z *= SpaceState.Editor.tileSize.y;
						pos.x += floor.collider.bounds.min.x;
						pos.z += floor.collider.bounds.min.z;
						pos.y = 0f;

						each.Value.transform.position = pos;
					}
				}
			}
		}
		
		private void StorePositions () {
			GameObject[] objs = GameObject.FindGameObjectsWithTag ("Enemy") as GameObject[];
			for (int i = 0; i < objs.Length; i++) {
				objs [i].GetComponent<Enemy> ().SetInitialPosition ();
			}
			objs = GameObject.FindGameObjectsWithTag ("AI") as GameObject[];
			for (int i = 0; i < objs.Length; i++) {
				objs [i].GetComponent<Player> ().SetInitialPosition ();
			}
		}
		
	}
}