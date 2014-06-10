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
using ClusteringSpace;
using Vectrosity; 

namespace EditorArea {
	public class MapperWindowEditor : EditorWindow {

		// Data holders
		private static Cell[][][] fullMap, original;
		public static List<Path> paths = new List<Path> (), deaths = new List<Path>();

		// Parameters with default values
		public static int timeSamples = 2000, attemps = 25000, iterations = 1, gridSize = 60, ticksBehind = 0;
		private static bool drawMap = false, drawNeverSeen = false, drawHeatMap = false, drawHeatMap3d = false, drawDeathHeatMap = false, drawDeathHeatMap3d = false, drawCombatHeatMap = false, drawPath = true, smoothPath = true, drawFoVOnly = false, drawCombatLines = false, simulateCombat = false;
		private static float stepSize = 1 / 10f, crazySeconds = 5f, playerDPS = 10;
		private static int randomSeed = -1;
		
		// Clustering
		public static String[] distMetrics = new String[] { "Frechet (L1) (fastest)", "Frechet (L1) 3D", "Frechet (Euclidean)", "Area (Interpolation) 3D", "Area (Triangulation)", "Time (no x,y)" };
		public static Color[] colors = new Color[] { Color.blue, Color.green, Color.magenta, Color.red, Color.yellow, Color.black, Color.grey };
		public static String[] colorStrings = new String[] { "Blue", "Green", "Magenta", "Red", "Yellow", "Black", "Grey"};
		private static int numClusters = 4, distMetric = 0, chosenFileIndex = -1, currentColor = 0, curCluster = 0;
		private static List<Path> clusterCentroids = new List<Path>();
		private static List<PathCollection> clusters20 = new List<PathCollection>();
		private static bool[] showPaths = new bool[colors.Count()];
		private static bool autoSavePaths = true;
		public static bool scaleTime = false, altCentroidComp = false;
		public int numberLines = 20; 
		public float interpolationValue = 0.0f;
		public float interpolationValueCheck = 0.0f; 

		// Computed parameters
		private static int[,] heatMap, deathHeatMap, combatHeatMap;
		private static int[][,] heatMap3d, deathHeatMap3d;
		private static GameObject start = null, end = null, floor = null, playerPrefab = null;
		private static Dictionary<Path, bool> toggleStatus = new Dictionary<Path, bool> ();
		private static Dictionary<Path, GameObject> players = new Dictionary<Path, GameObject> ();
		private static int startX, startY, endX, endY, maxHeatMap, timeSlice, imported = 0;
		private static bool seeByTime, seeByLength, seeByDanger, seeByLoS, seeByDanger3, seeByLoS3, seeByDanger3Norm, seeByLoS3Norm, seeByCrazy, seeByVelocity;
		private static List<Path> arrangedByTime, arrangedByLength, arrangedByDanger, arrangedByLoS, arrangedByDanger3, arrangedByLoS3, arrangedByDanger3Norm, arrangedByLoS3Norm, arrangedByCrazy, arrangedByVelocity;

		// Helping stuff
		private static Vector2 scrollPos = new Vector2 ();
		private static GameObject playerNode;
		private List<Tuple<Vector3, string>> textDraw = new List<Tuple<Vector3, string>>();
		private int lastTime = timeSlice;
		private long stepInTicks = 0L, playTime = 0L;
		private static bool simulated = false, playing = false;
		private Mapper mapper;
		private RRTKDTreeCombat rrt = new RRTKDTreeCombat ();
		private RRTKDTree oldRrt = new RRTKDTree ();
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

			if (GUILayout.Button ((MapperEditor.editGrid ? "Finish Editing" : "Edit Grid"))) {
				if (floor != null) {
					MapperEditor.editGrid = !MapperEditor.editGrid;
					Selection.activeGameObject = mapper.gameObject;
				}
			}
	
			EditorGUILayout.LabelField ("");
			
			#endregion
			
			// ----------------------------------
			
			#region 2. Units
			
			EditorGUILayout.LabelField ("2. Units");

			playerDPS = EditorGUILayout.Slider("Player DPS", playerDPS, 0.1f, 100f);
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
				
				original = mapper.PrecomputeMaps (SpaceState.Editor, floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timeSamples, stepSize, ticksBehind, baseMap);

				drawer.fullMap = original;
				float maxSeenGrid;
				drawer.seenNeverSeen = Analyzer.ComputeSeenValuesGrid (original, out maxSeenGrid);
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
			randomSeed = EditorGUILayout.IntSlider("Random Seed", randomSeed, -1, 10000);
			smoothPath = EditorGUILayout.Toggle ("Smooth path", smoothPath);
			simulateCombat = EditorGUILayout.Toggle ("Simulate combat", simulateCombat);

			if (GUILayout.Button ("(WIP) Compute 3D A* Path")) {
				float playerSpeed = GameObject.FindGameObjectWithTag ("AI").GetComponent<Player> ().speed;
				
				//Check the start and the end and get them from the editor. 
				if (start == null) {
					start = GameObject.Find ("Start");
				}
				if (end == null) {
					end = GameObject.Find ("End");	
				}

				startX = (int)((start.transform.position.x - floor.collider.bounds.min.x) / SpaceState.Editor.tileSize.x);
				startY = (int)((start.transform.position.z - floor.collider.bounds.min.z) / SpaceState.Editor.tileSize.y);
				endX = (int)((end.transform.position.x - floor.collider.bounds.min.x) / SpaceState.Editor.tileSize.x);
				endY = (int)((end.transform.position.z - floor.collider.bounds.min.z) / SpaceState.Editor.tileSize.y);

				paths.Clear ();
				deaths.Clear ();
				ClearPathsRepresentation ();
				arrangedByCrazy = arrangedByDanger = arrangedByDanger3 = arrangedByDanger3Norm = arrangedByLength = arrangedByLoS = arrangedByLoS3 = arrangedByLoS3Norm = arrangedByTime = arrangedByVelocity = null;

				Exploration.DavAStar3d astar3d = new DavAStar3d();
				List<Node> nodes = astar3d.Compute(startX, startY, endX, endY, original, playerSpeed);

				if (nodes.Count > 0) {
					paths.Add (new Path (nodes));
					toggleStatus.Add (paths.Last (), true);
					paths.Last ().color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
				}
			}

			if (GUILayout.Button ("Compute Path")) {
				float playerSpeed = GameObject.FindGameObjectWithTag ("AI").GetComponent<Player> ().speed;
				float playerMaxHp = GameObject.FindGameObjectWithTag ("AI").GetComponent<Player> ().maxHp;
				
				//Check the start and the end and get them from the editor. 
				if (start == null) {
					start = GameObject.Find ("Start");
				}
				if (end == null) {
					end = GameObject.Find ("End");	
				}
				
				paths.Clear ();
				deaths.Clear ();
				ClearPathsRepresentation ();
				arrangedByCrazy = arrangedByDanger = arrangedByDanger3 = arrangedByDanger3Norm = arrangedByLength = arrangedByLoS = arrangedByLoS3 = arrangedByLoS3Norm = arrangedByTime = arrangedByVelocity = null;

				// Prepare start and end position
				startX = (int)((start.transform.position.x - floor.collider.bounds.min.x) / SpaceState.Editor.tileSize.x);
				startY = (int)((start.transform.position.z - floor.collider.bounds.min.z) / SpaceState.Editor.tileSize.y);
				endX = (int)((end.transform.position.x - floor.collider.bounds.min.x) / SpaceState.Editor.tileSize.x);
				endY = (int)((end.transform.position.z - floor.collider.bounds.min.z) / SpaceState.Editor.tileSize.y);

				GameObject[] hps = GameObject.FindGameObjectsWithTag("HealthPack");
				HealthPack[] packs = new HealthPack[hps.Length];
				for (int i = 0; i < hps.Length; i++) {
					packs[i] = hps[i].GetComponent<HealthPack>();
					packs[i].posX = (int)((packs[i].transform.position.x - floor.collider.bounds.min.x) / SpaceState.Editor.tileSize.x);
					packs[i].posZ = (int)((packs[i].transform.position.z - floor.collider.bounds.min.z) / SpaceState.Editor.tileSize.y);
				}

				// Update the parameters on the RRT class
				rrt.min = floor.collider.bounds.min;
				rrt.tileSizeX = SpaceState.Editor.tileSize.x;
				rrt.tileSizeZ = SpaceState.Editor.tileSize.y;
				rrt.enemies = SpaceState.Editor.enemies;
				rrt.packs = packs;
				rrt.simulateCombat = simulateCombat;

				int seed = randomSeed;
				if (randomSeed != -1)
					UnityEngine.Random.seed = randomSeed;
				else {
					DateTime now = DateTime.Now;
					seed = now.Millisecond + now.Second + now.Minute + now.Hour + now.Day + now.Month+ now.Year;
					UnityEngine.Random.seed = seed;
				}

				List<Node> nodes = null;
				for (int it = 0; it < iterations; it++) {

					// Make a copy of the original map
					fullMap = new Cell[original.Length][][];
					for (int t = 0; t < original.Length; t++) {
						fullMap[t] = new Cell[original[0].Length][];
						for (int x = 0; x < original[0].Length; x++) {
							fullMap[t][x] = new Cell[original[0][0].Length];
							for (int y = 0; y < original[0][0].Length; y++)
								fullMap[t][x][y] = original[t][x][y].Copy();
						}
					}
					
					// Use the copied map so the RRT can modify it
					foreach (Enemy e in SpaceState.Editor.enemies) {
						for (int t = 0; t < original.Length; t++)
							for (int x = 0; x < original[0].Length; x++)
								for (int y = 0; y < original[0][0].Length; y++)
									if (e.seenCells[t][x][y] != null)
										e.seenCells[t][x][y] = fullMap[t][x][y];

						// TODO: Need to make a backup of the enemies positions, rotations and forwards
						
					}
					// We have this try/catch block here to account for the issue that we don't solve when we find a path when t is near the limit
					try {
						nodes = rrt.Compute (startX, startY, endX, endY, attemps, stepSize, playerMaxHp, playerSpeed, playerDPS, fullMap, smoothPath);
						// Did we found a path?
						if (nodes.Count > 0) {
							paths.Add (new Path (nodes));
							toggleStatus.Add (paths.Last (), true);
							paths.Last ().color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
						}
						// Grab the death list
						foreach (List<Node> deathNodes in rrt.deathPaths) {
							deaths.Add(new Path(deathNodes));
						}
					} catch (Exception e) {
						Debug.LogWarning("Skip errored calculated path");
						Debug.LogException(e);
						// This can happen in two different cases:
						// In line 376 by having a duplicate node being picked (coincidence picking the EndNode as visiting but the check is later on)
						// We also cant just bring the check earlier since there's data to be copied (really really rare cases)
						// The other case is yet unkown, but it's a conicidence by trying to insert a node in the tree that already exists (really rare cases)
					}
				}
				// Set the map to be drawn
				drawer.fullMap = fullMap;
				ComputeHeatMap (paths, deaths);

				// Compute the summary about the paths and print it
				String summary = "Summary:\n";
				summary += "Seed used:" + seed;
				summary += "\nSuccessful paths found: " + paths.Count;
				summary += "\nDead paths: " + deaths.Count;

				// How many paths killed how many enemies
				Dictionary<int, int> map = new Dictionary<int, int>();
				for (int i = 0; i <= SpaceState.Editor.enemies.Length; i++)
					map.Add(i, 0);
				foreach (Path p in paths) {
					int killed = 0;
					foreach (Node n in p.points)
						if (n.died != null)
							killed++;

					if (map.ContainsKey(killed))
						map[killed]++;
					else
						map.Add(killed, 1);
				}

				foreach (int k in map.Keys) {
					summary += "\n" + map[k] + " paths killed " + k + " enemies";
				}

				// Debug.Log(summary);
			}
			
			String nameFile = EditorApplication.currentScene;
			nameFile = nameFile.Replace(".unity","");

			nameFile = nameFile.Replace("Assets/Levels/","");
			
			if (GUILayout.Button ("(DEBUG) Export Paths")) {
				List<Path> all = new List<Path>();
				all.AddRange(paths);
				all.AddRange(deaths);

				PathBulk.SavePathsToFile (nameFile + "_paths.xml", all);
			}
			
			if (GUILayout.Button ("(DEBUG) Import Paths")) {
				paths.Clear ();
				ClearPathsRepresentation ();
				
				List<Path> pathsImported = PathBulk.LoadPathsFromFile (nameFile + "_paths.xml");
				
				foreach (Path p in pathsImported) {
					if (p.points.Last().playerhp <= 0) {
						deaths.Add(p);
					} else {
						p.name = "Imported " + (++imported);
						p.color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
						toggleStatus.Add (p, true);
					}
					paths.Add(p);
				}
				ComputeHeatMap (paths, deaths);
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
			drawCombatHeatMap = EditorGUILayout.Toggle ("-> Draw combat heat map", drawCombatHeatMap);
			drawHeatMap3d = EditorGUILayout.Toggle ("-> Draw heat map 3d", drawHeatMap3d);
			drawDeathHeatMap = EditorGUILayout.Toggle ("-> Draw death heat map", drawDeathHeatMap);
			drawDeathHeatMap3d = EditorGUILayout.Toggle ("--> Draw 3d death heat map", drawDeathHeatMap3d);
			drawPath = EditorGUILayout.Toggle ("Draw path", drawPath);
			drawCombatLines = EditorGUILayout.Toggle ("Draw combat lines", drawCombatLines);
			
			if (drawer != null) {
				drawer.heatMap = null;
				drawer.heatMap3d = null;
				drawer.deathHeatMap = null;
				drawer.deathHeatMap3d = null;
				drawer.combatHeatMap = null;

				if (drawHeatMap) {
					if (drawCombatHeatMap)
						drawer.combatHeatMap = combatHeatMap;

					else if (drawHeatMap3d)
						drawer.heatMap3d = heatMap3d;

					else if (drawDeathHeatMap) {
						if (drawDeathHeatMap3d)
							drawer.deathHeatMap3d = deathHeatMap3d;
						else
							drawer.deathHeatMap = deathHeatMap;
					}

					else
						drawer.heatMap = heatMap;
				}
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
				//Analyzer.ComputePathsTimeValues (paths);
				//Analyzer.ComputePathsLengthValues (paths);
				//Analyzer.ComputePathsVelocityValues (paths);
				//Analyzer.ComputePathsLoSValues (paths, SpaceState.Editor.enemies, floor.collider.bounds.min, SpaceState.Editor.tileSize.x, SpaceState.Editor.tileSize.y, original, drawer.seenNeverSeen, drawer.seenNeverSeenMax);
				Analyzer.ComputePathsDangerValues (paths, SpaceState.Editor.enemies, floor.collider.bounds.min, SpaceState.Editor.tileSize.x, SpaceState.Editor.tileSize.y, original, drawer.seenNeverSeen, drawer.seenNeverSeenMax);
				//Analyzer.ComputeCrazyness (paths, original, Mathf.FloorToInt (crazySeconds / stepSize));
				//Analyzer.ComputePathsVelocityValues (paths);
				
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
			
			#region 7. Clustering
						
			EditorGUILayout.LabelField ("");
			EditorGUILayout.LabelField ("7. Clustering");
			EditorGUILayout.LabelField ("");

			if (GUILayout.Button ("Select 2 random paths"))
			{
				GameObject g = GameObject.Find("DataPath") as GameObject;
				if (!g)
				{	
					g = new GameObject(); 
					g.name = "DataPath"; 
					g.AddComponent("PathsHolder"); 
				}

				PathsHolder data = g.GetComponent("PathsHolder") as PathsHolder; 
				 
				if(paths.Count!=0)
				{
					data.paths.Clear();
					data.paths.Add ( paths[UnityEngine.Random.Range(0,paths.Count)]);
					data.paths.Add ( paths[UnityEngine.Random.Range(0,paths.Count)]);
				}
				else
				{
					Debug.Log("Add paths to collection"); 
				}
			}

			numberLines =  EditorGUILayout.IntField("number of lines", numberLines); 

			if (GUILayout.Button ("Draw lines"))
			{

				//clear the previous line
				GameObject lineHolder = GameObject.Find("Lines"); 
				if(lineHolder)
					DestroyImmediate(lineHolder); 

				lineHolder = new GameObject(); 
				lineHolder.name = "Lines"; 

				//Get the data
				GameObject g = GameObject.Find("DataPath") as GameObject;
				if(!g)
				{
					Debug.Log("No paths");
					return;
				}
				PathsHolder data = g.GetComponent("PathsHolder") as PathsHolder; 
				if(data.paths.Count==0)
				{
					Debug.Log("No paths"); 
					return; 
				}

				//First line

				VectorLine line1 = new VectorLine("1",data.paths[0].getPoints3D(),Color.red,null,30.0f);

				line1.Draw3D();

				line1.vectorObject.transform.parent = lineHolder.transform;

				//Second line

				VectorLine line2 = new VectorLine("2",data.paths[1].getPoints3D(),Color.blue,null,30.0f);
				
				line2.Draw3D();
				
				line2.vectorObject.transform.parent = lineHolder.transform;



				//Draw the lines in between
					//Find the first set of points
				Vector3[] set1 = GetSetPointsWithN(data.paths[0].getPoints3D(),numberLines); 
				Vector3[] set2 = GetSetPointsWithN(data.paths[1].getPoints3D(),numberLines); 

				List<Vector3> linesInterpolated = new List<Vector3>(); 

				for(int i = 0; i<set1.Length;i++)
				{
					linesInterpolated.Add(set1[i]);
					linesInterpolated.Add(set2[i]);

				}

				VectorLine line3 = new VectorLine("3",linesInterpolated.ToArray(),Color.green,null,20.0f);
				
				line3.Draw3D();
				
				line3.vectorObject.transform.parent = lineHolder.transform;
				
				double area = AreaDist.areaFromInterpolation3D(data.paths[0], data.paths[1]);
				Debug.Log("Area between paths: " + area);
			}
			if(GUILayout.Button("Triangle 2 random Curves"))
			{

				GameObject k = GameObject.Find("DataPath") as GameObject;
				PathsHolder data = k.GetComponent("PathsHolder") as PathsHolder; 
				 
				if(paths.Count!=0)
				{
					data.paths.Clear();
					data.paths.Add ( paths[UnityEngine.Random.Range(0,paths.Count)]);
					data.paths.Add ( paths[UnityEngine.Random.Range(0,paths.Count)]);
				}

				GameObject g = GameObject.Find("Triangulation"); 
				
				if(g != null)
				{
					Triangulation tObject = g.GetComponent<Triangulation>(); 
					tObject.ShowTriangulation(); 
				}	
			}
			/*
			//Draw multiple lines over the interpolation
			if (GUILayout.Button ("Muliple Lines"))
			{
				GameObject lineHolder = GameObject.Find("LinesMulitple"); 
				if(lineHolder)
					DestroyImmediate(lineHolder); 
				
				lineHolder = new GameObject(); 
				lineHolder.name = "LinesMulitple"; 
				
				
				//Third line
				Vector3 [] points3 = { new Vector3(1,3,14),new Vector3(8,7,4),new Vector3(8,7,4),new Vector3(8,3,6),
					new Vector3(8,3,6),new Vector3(7,15,6)};
				
				VectorLine line3 = new VectorLine("3",points3,Color.cyan,null,10.0f);
				
				
				line3.vectorObject.transform.parent = lineHolder.transform;
				

				
				
				line3.Draw3D(); 

				int n = 50;
				
				List<Vector3> pairs = new List<Vector3>(); 

				float lengthLine = 0; 
				
				for(int i =0; i<points3.Length; i+=2)
				{
					Vector3 t = points3[i]-points3[i+1];
					lengthLine += t.magnitude; 
				}

				n = n-1; 

				for(int j = 0; j<=n; j++)
				{
					float interpolation = (float)j/(float)n;

					Vector3 pointToGo = Vector3.zero; 
					

					
					//Find between which point the interpolation belongs
					float lineAt = 0.0f;
					for(int i =0; i<points3.Length; i+=2)
					{
						Vector3 t = points3[i]-points3[i+1];
						
						if(interpolation > (lineAt/lengthLine)  && interpolation <= (t.magnitude+lineAt)/lengthLine)
						{
							//We are int
							float linter = interpolation *lengthLine;
							linter-=lineAt;
							float newInter = linter/t.magnitude;
							
							pointToGo  = points3[i] + (points3[i+1] - points3[i])*newInter   ;
						}
						lineAt+=t.magnitude; 
					}
					if(interpolation == 0)
						pointToGo = points3[0];
					if(interpolation >=1)
						pointToGo = points3[points3.Length - 1];



					pairs.Add(Vector3.zero);
					pairs.Add(pointToGo); 

				}

				VectorLine line4 = new VectorLine("2",pairs.ToArray(),Color.blue,null,2.0f);

				line4.vectorObject.transform.parent = lineHolder.transform;
				
				line4.Draw3D(); 
				
			
			}


			//Fun with line interpolation here
			interpolationValue = EditorGUILayout.Slider("Interpolation",interpolationValue,0.0f,1.0f,null); 

			if(interpolationValue != interpolationValueCheck)
			{
				interpolationValueCheck = interpolationValue; 


				//clear the previous line
				GameObject lineHolder = GameObject.Find("LinesTest"); 
				if(lineHolder)
					DestroyImmediate(lineHolder); 
				
				lineHolder = new GameObject(); 
				lineHolder.name = "LinesTest"; 
				

				
				//First line
				Vector3[] points = {new Vector3(0,0,0),new Vector3(0,10,0)};
				VectorLine line1 = new VectorLine("1",points,Color.red,null,10.0f);
				
				
				line1.vectorObject.transform.parent = lineHolder.transform;
				
				//Second line
				Vector3 [] points2 = { new Vector3(0,0,0),new Vector3(10,16,0)};
				
				VectorLine line2 = new VectorLine("2",points2,Color.blue,null,10.0f);
				
				
				line2.vectorObject.transform.parent = lineHolder.transform;
				//Draw the lines in between

				//Third line
				Vector3 [] points3 = { points[1],points2[1],points2[1],new Vector3(8,7,4),
					new Vector3(8,7,4),new Vector3(3,15,2)};
				
				VectorLine line3 = new VectorLine("3",points3,Color.cyan,null,10.0f);
				
				
				line3.vectorObject.transform.parent = lineHolder.transform;

				//Third line


				//Find the second point to go

				int n = 100;

				List<Vector3> pairs = new List<Vector3>(); 




				Vector3 pointToGo = Vector3.zero; 

				float lengthLine = 0; 
				for(int i =0; i<points3.Length; i+=2)
				{
					Vector3 t = points3[i]-points3[i+1];
					lengthLine += t.magnitude; 
				}

				//Find between which point the interpolation belongs
				float lineAt = 0.0f;
				for(int i =0; i<points3.Length; i+=2)
				{
					Vector3 t = points3[i]-points3[i+1];
					
					if(interpolationValue > (lineAt/lengthLine)  && interpolationValue <= (t.magnitude+lineAt)/lengthLine)
					{
						//We are int
						float linter = interpolationValue *lengthLine;
						linter-=lineAt;
						float newInter = linter/t.magnitude;

						pointToGo  = points3[i] + (points3[i+1] - points3[i])*newInter   ;
					}
					lineAt+=t.magnitude; 
				}
				if(interpolationValue == 0)
					pointToGo = points3[0];
				if(interpolationValue >=1)
					pointToGo = points3[points3.Length - 1];


				Vector3 [] points4 = { points[0],   pointToGo };
				
				VectorLine line4 = new VectorLine("4",points4,Color.gray,null,5.0f);
				
				
				line4.vectorObject.transform.parent = lineHolder.transform;


			
				line3.Draw3D(lineHolder.transform);
				line4.Draw3D(lineHolder.transform);





			}
			*/
			EditorGUILayout.LabelField ("");
			
			numClusters = EditorGUILayout.IntSlider ("Number of clusters", numClusters, 1, 7);
			
			int prevMetric = distMetric;
			distMetric = EditorGUILayout.Popup("Dist metric:", distMetric, distMetrics);
			
			if (prevMetric != distMetric && (distMetric == 1 || distMetric == 3 || distMetric == 5)) { scaleTime = true; }
			scaleTime = EditorGUILayout.Toggle("Scale time", scaleTime);
			altCentroidComp = EditorGUILayout.Toggle("Alt centroid computation", altCentroidComp);
			
			if (GUILayout.Button ("Cluster on path similarity"))
			{
				if (paths.Count < numClusters)
				{
					Debug.Log("You have less paths than you have desired clusters - either compute more paths or decrease cluster amount.");
					return;
				}
				
				if (altCentroidComp)
				{
					for (int i = 0; i < paths.Count; i ++)
					{ // make each path have same # of points
						// find the highest time value!
						double maxTime = Double.NegativeInfinity;
						foreach (Path p in paths)
						{
							foreach (Node n in p.points)
							{
								if (n.t > maxTime)
								{
									maxTime = n.t;
								}
							}
						}
						Vector3[] set1 = MapperWindowEditor.GetSetPointsWithN(paths[i].getPoints3D(), (int)Math.Sqrt(maxTime), false);
						Debug.Log("Paths now have " + Math.Sqrt(maxTime) + " points.");
						List<Node> nodes = new List<Node>();
						foreach(Vector3 v in set1)
						{
							if (v.x == 0 && v.y == 0 && v.z == 0) continue;
							nodes.Add(new Node((int)v.x, (int)v.z, (int)v.y));
						}
						paths[i] = new Path(nodes);
					}
				}

				KMeans.clustTime = new System.Diagnostics.Stopwatch();
				KMeans.distTime = new System.Diagnostics.Stopwatch();

				if (paths.Count > 99)
				{
					List<PathCollection> clusters = KMeans.DoKMeans(paths, paths.Count/20, distMetric);
				
					List<Path> tempCentroids = new List<Path>();
					foreach(PathCollection pc in clusters)
					{
						if (altCentroidComp) tempCentroids.Add(pc.getCenterDistPath());
						else tempCentroids.Add(pc.Centroid);
					}
				
					List<PathCollection> newClusters = KMeans.DoKMeans(tempCentroids, numClusters, distMetric);
		
					clusterCentroids.Clear();
					foreach(PathCollection pc in newClusters)
					{
						clusterCentroids.Add(pc.Centroid);
						paths.Add(pc.Centroid);
						toggleStatus.Add(paths.Last(), true);
					}
				
					paths.Clear ();
					deaths.Clear ();
					ClearPathsRepresentation ();

					for (int c = 0; c < newClusters.Count; c ++)
					{
						for (int c2 = 0; c2 < clusters.Count; c2 ++)
						{
							if (newClusters[c].Contains(tempCentroids[c2]))
							{ // then all paths of clusters[c2] list should be of the same color!
								foreach (Path path in clusters[c2])
								{
									path.color = colors[c];
									if (path.Equals(tempCentroids[c2]))
									{
										path.color.a = 0.5f;
									}
									if (!paths.Contains(path))
									{
										paths.Add(path);
										toggleStatus.Add(paths.Last (), true);
									}
								}
							}
						}
					}
				}
				else
				{
					List<PathCollection> clusters = KMeans.DoKMeans(paths, numClusters, distMetric);
					
					clusterCentroids.Clear();
					foreach(PathCollection pc in clusters)
					{
//						clusterCentroids.Add(pc.Centroid);
						if (altCentroidComp) clusterCentroids.Add(pc.getCenterDistPath());
						else clusterCentroids.Add(pc.Centroid);
					}
								
					paths.Clear ();
					deaths.Clear ();
					ClearPathsRepresentation ();

					for(int c = 0; c < clusters.Count; c ++)
					{
						foreach(Path path in clusters[c])
						{
							path.color = colors[c];
							if (path.Equals(clusterCentroids[c]))
							{
								path.color.a = 0.5f;
							}
							paths.Add(path);
							toggleStatus.Add(paths.Last (), true);
						}
					}
				}
				
				Debug.Log ("Clust elapsed time: " + KMeans.clustTime.Elapsed);
				Debug.Log ("Dist elapsed time: " + KMeans.distTime.Elapsed);
				TimeSpan totalTime = KMeans.clustTime.Elapsed + KMeans.distTime.Elapsed;
				Debug.Log ("Total: " + totalTime);
				
				if (autoSavePaths)
				{
					String currentTime = System.DateTime.Now.ToString("s");
					currentTime = currentTime.Replace(':', '-');
					String totalTimeStr = new DateTime(Math.Abs(totalTime.Ticks)).ToString("HHmmss");
					PathBulk.SavePathsToFile ("clusteringdata/" + nameFile + "_" + numClusters + "c-" + distMetric + "d-" + paths.Count() + "p-" + totalTimeStr + "t@" + currentTime + ".xml", paths);
				}
				
				for (int color = 0; color < colors.Count(); color ++)
				{
					showPaths[color] = (color < numClusters) ? true : false;
				}
			}
			
			if (GUILayout.Button ("Cluster with optimal k (2-7)"))
			{
				if (paths.Count() < 7)
				{
					Debug.Log("You must have at least 100 paths to perform this operation!");
					return;
				}
				
				List<PathCollection> clusters = KMeans.DoKMeans(paths, paths.Count/20, distMetric);
			
				List<Path> tempCentroids = new List<Path>();
				foreach(PathCollection pc in clusters)
				{
					tempCentroids.Add(pc.Centroid);
				}
				
				double maxDistanceBetweenClusters = Double.NegativeInfinity;
				int clusterNumOfMaxDist = -1;
				
				for (int numClusters_ = 7; numClusters_ > 1; numClusters_ --)
				{	
					List<PathCollection> newClusters = KMeans.DoKMeans(tempCentroids, numClusters_, distMetric);
					
					clusterCentroids.Clear();
					foreach(PathCollection pc in newClusters)
					{
						clusterCentroids.Add(pc.Centroid);
					}
					
					paths.Clear ();
					deaths.Clear ();
					ClearPathsRepresentation ();

					for (int c = 0; c < newClusters.Count; c ++)
					{
						for (int c2 = 0; c2 < clusters.Count; c2 ++)
						{
							if (newClusters[c].Contains(clusters[c2].Centroid))
							{ // then all paths of clusters[c2] list should be of the same color!
								foreach (Path path in clusters[c2])
								{
									path.color = colors[c];
									if (path.Equals(clusters[c2].Centroid))
									{
										path.color.a = 0.5f;
									}
									if (!paths.Contains(path))
									{
										paths.Add(path);
										toggleStatus.Add(paths.Last (), true);
									}
								}
							}
						}
					}

					PathBulk.SavePathsToFile ("clusteringdata/" + nameFile + "_" + numClusters_ + "c-" + distMetric + "d-" + paths.Count() + "p.xml", paths);
				
					double totalDist = 0;
					int numDist = 0;
					for (int i = 0; i < clusterCentroids.Count(); i ++)
					{
						for (int j = i+1; j < clusterCentroids.Count(); j ++)
						{
							numDist ++;
							totalDist += KMeans.FindDistance(clusterCentroids[i], clusterCentroids[j]);
						}
					}
					
					double avgDist = totalDist / numDist;
					if (avgDist > maxDistanceBetweenClusters)
					{
						clusterNumOfMaxDist = numClusters_;
						maxDistanceBetweenClusters = avgDist;
					}
				}
				
				// find cluster # that on average maximizes the distances between cluster centroids.				
				Debug.Log("Optimal cluster #: " + clusterNumOfMaxDist + ".");
				
				// Display the optimal clustering.
				List<Path> pathsImported = PathBulk.LoadPathsFromFile ("clusteringdata/" + nameFile + "_" + clusterNumOfMaxDist + "c-" + distMetric + "d-" + paths.Count() + "p.xml");

				paths.Clear ();
				ClearPathsRepresentation ();
								
				foreach (Path p in pathsImported)
				{
					toggleStatus.Add (p, true);
					paths.Add(p);
				}
			}
			
		/*	if (GUILayout.Button ("Cluster 20"))
			{
				KMeans.clustTime = new System.Diagnostics.Stopwatch();
				KMeans.distTime = new System.Diagnostics.Stopwatch();
				
				clusters20 = KMeans.DoKMeans(paths, 50, distMetric);
				
				clusterCentroids.Clear();
				foreach(PathCollection pc in clusters20)
				{
					clusterCentroids.Add(pc.Centroid);
				}
						
				paths.Clear ();
				deaths.Clear ();
				ClearPathsRepresentation ();

				for(int c = 0; c < clusters20.Count; c ++)
				{
					foreach(Path path in clusters20[c])
					{
						path.color = colors[c % colors.Count()];
						if (path.Equals(clusters20[c].Centroid))
						{
							path.color.a = 0.5f;
						}
						paths.Add(path);
						toggleStatus.Add(paths.Last (), true);
					}
				}
				
				Debug.Log ("Clust elapsed time: " + KMeans.clustTime.Elapsed);
				Debug.Log ("Dist elapsed time: " + KMeans.distTime.Elapsed);
				TimeSpan totalTime = KMeans.clustTime.Elapsed + KMeans.distTime.Elapsed;
				Debug.Log ("Total: " + totalTime);
			}
			if (GUILayout.Button ("Next"))
			{
				//clusters20
				foreach (Path p in paths)
				{
					p.color.a = 0;
				}
				foreach (Path clusterPath in clusters20[curCluster])
				{
					foreach (Path p in paths)
					{
						if (clusterPath == p)
						{
							p.color.a = 1;
						}
					}
				}
				curCluster = (curCluster + 1) % 100;
			}*/

		//	autoSavePaths = EditorGUILayout.Toggle("Autosave cluster results", autoSavePaths);
			
			DirectoryInfo dir = new DirectoryInfo("clusteringdata/");
			FileInfo[] info = dir.GetFiles("*.xml");
			
			List<String> goodFiles = new List<String>();
			for (int count = 0; count < info.Count(); count ++)
			{
				String[] temp = info[count].Name.Split(new Char[]{'_'});
				if (temp[0] == nameFile)
				{
					goodFiles.Add(info[count].Name);
				}				
			}
			
			String[] fileNames = goodFiles.ToArray();
			chosenFileIndex = EditorGUILayout.Popup("Load saved results", chosenFileIndex, fileNames);
			if (chosenFileIndex != -1)
			{
				paths.Clear ();
				ClearPathsRepresentation ();
				
				List<Path> pathsImported = PathBulk.LoadPathsFromFile ("clusteringdata/" + fileNames[chosenFileIndex]);
				
				foreach (Path p in pathsImported) {
					toggleStatus.Add (p, true);
					paths.Add(p);
				}
				//SetupArrangedPaths (paths);
				
				chosenFileIndex = -1;
			}
			
			EditorGUILayout.LabelField ("");
			for (int count = 0; count < colors.Count(); count ++)
			{
				showPaths[count] = EditorGUILayout.Toggle(colorStrings[count], showPaths[count]);
			}
			if (GUILayout.Button ("Show selected colors"))
			{
				List<Color> selectedColors = new List<Color>();
				for (int color = 0; color < colors.Count(); color ++)
				{
					if (showPaths[color])
					{
						selectedColors.Add(colors[color]);
					}
				}
				foreach (Path p in paths)
				{
					bool contained = false;
					foreach (Color color in selectedColors)
					{
						if (p.color.r == color.r && p.color.g == color.g && p.color.b == color.b)
						{
							p.color.a = 1;
							contained = true;
							break;
						}
					}
					
					if (!contained) p.color.a = 0;
				}
			}
			if (GUILayout.Button ("Show next color"))
			{
				int numPaths = 0;
				do
				{
					currentColor = (currentColor + 1) % colors.Count();
					for (int color = 0; color < colors.Count(); color ++)
					{
						if (color == currentColor) showPaths[color] = true;
						else showPaths[color] = false;
					}
				
					foreach (Path p in paths)
					{
						if (p.color.r == colors[currentColor].r && p.color.g == colors[currentColor].g && p.color.b == colors[currentColor].b)
						{
							numPaths ++;
							p.color.a = 1;
						}
						else p.color.a = 0;
					}
				} while(numPaths == 0);
			}
			if (GUILayout.Button ("Show centroid path for current color"))
			{
				int colorIndex = -1;
				int numSelectedColors = 0;
				for (int color = 0; color < colors.Count(); color ++)
				{
					if (showPaths[color])
					{
						numSelectedColors ++;
						colorIndex = color;
					}
				}
				
				if (colorIndex == -1 || numSelectedColors > 1)
				{
					Debug.Log("You must first select exactly one color above.");
				}
				else
				{
					foreach (Path p in paths)
					{
						if (p.Equals(clusterCentroids[colorIndex]) || (p.color.a == 0.5 && p.color.Equals(colors[colorIndex])))
						{
							p.color.a = 1;
						}
						else p.color.a = 0;
					}
				}
			}
			
			#endregion
			
			// ----------------------------------
			
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
				drawer.drawCombatLines = drawCombatLines;
				drawer.paths = toggleStatus;
				drawer.textDraw = textDraw;
				
			}
			
			if (original != null && lastTime != timeSlice) {
				lastTime = timeSlice;
				UpdatePositions (timeSlice, mapper);
			}
			
			SceneView.RepaintAll ();

		}
			
		public static Vector3[] GetSetPointsWithN(Vector3[] points3,int n, bool zeroVector = true)
		{
			List<Vector3> pairs = new List<Vector3>(); 
			
			float lengthLine = 0; 
			
			for(int i =0; i<points3.Length; i+=2)
			{
				Vector3 t = points3[i]-points3[i+1];
				lengthLine += t.magnitude; 
			}
			
			n = n-1; 
			
			for(int j = 0; j<=n; j++)
			{
				float interpolation = (float)j/(float)n;
				
				Vector3 pointToGo = Vector3.zero; 
				
				//Find between which point the interpolation belongs
				float lineAt = 0.0f;
				for(int i =0; i<points3.Length; i+=2)
				{
					Vector3 t = points3[i]-points3[i+1];
					
					if(interpolation > (lineAt/lengthLine)  && interpolation <= (t.magnitude+lineAt)/lengthLine)
					{
						//We are int
						float linter = interpolation *lengthLine;
						linter-=lineAt;
						float newInter = linter/t.magnitude;
						
						pointToGo  = points3[i] + (points3[i+1] - points3[i])*newInter   ;
					}
					lineAt+=t.magnitude; 
				}
				if(interpolation == 0)
					pointToGo = points3[0];
				if(interpolation >=1)
					pointToGo = points3[points3.Length - 1];
				
				if (zeroVector) pairs.Add(Vector3.zero);
				pairs.Add(pointToGo); 	
			}
			return pairs.ToArray();
		}

		public void Update () {
			textDraw.Clear();
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
		
		private static void ClearPathsRepresentation () {
			toggleStatus.Clear ();
			
			foreach (GameObject obj in players.Values)
				GameObject.DestroyImmediate (obj);
				
			players.Clear ();

			GameObject.DestroyImmediate(GameObject.Find("TempPlayerNode"));

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
		
		private void ComputeHeatMap (List<Path> paths, List<Path> deaths = null) {
			heatMap = Analyzer.Compute2DHeatMap (paths, gridSize, gridSize, out maxHeatMap);
			drawer.heatMapMax = maxHeatMap;
			drawer.heatMap = heatMap;

			deathHeatMap = Analyzer.ComputeDeathHeatMap (deaths, gridSize, gridSize, out maxHeatMap);
			drawer.deathHeatMapMax = maxHeatMap;

			int[] maxHeatMap3d;
			heatMap3d = Analyzer.Compute3DHeatMap (paths, gridSize, gridSize, timeSamples, out maxHeatMap3d);
			drawer.heatMapMax3d = maxHeatMap3d;

			deathHeatMap3d = Analyzer.Compute3dDeathHeatMap(deaths, gridSize, gridSize, timeSamples, out maxHeatMap3d);
			drawer.deathHeatMapMax3d = maxHeatMap3d;

			combatHeatMap = Analyzer.Compute2DCombatHeatMap(paths, deaths, gridSize, gridSize, out maxHeatMap);
			drawer.combatHeatMap2dMax = maxHeatMap;

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
						path = oldRrt.Compute (startX, startY, endX, endY, rrtattemps, speed, fullMap, false);
						oldRrt.tree = null;
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
						path = oldRrt.Compute (startX, startY, endX, endY, rrtattemps, speed, fullMap, false);
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
						path = oldRrt.Compute (startX, startY, endX, endY, rrtattemps, speed, fullMap, false);
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
						textDraw.Add(Tuple.New<Vector3, string>(new Vector3(pos.x, pos.y + 1f, pos.z), "H:" + p.playerhp));
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
		
		public static void updatePaths(List<PathCollection> clusters)
		{
			// This function was supposed to update the path colors in the editor display while the clustering process
			// was running, but it doesn't work. It does cause the Unity window to request window focus, though.
						
			paths.Clear ();
			deaths.Clear ();
			ClearPathsRepresentation ();
			
			for(int c = 0; c < clusters.Count; c ++)
			{
				foreach(Path path in clusters[c])
				{
					path.color = colors[c%colors.Count()];
					paths.Add(path);
					toggleStatus.Add(paths.Last (), true);
				}
			}
			
			MapperWindowEditor window = (MapperWindowEditor)EditorWindow.GetWindow (typeof(MapperWindowEditor));
			EditorUtility.SetDirty (window);
			window.Repaint();
			window.Update();
			window.OnGUI();
			
			window.drawer = floor.gameObject.GetComponent<MapperEditorDrawer> ();
			
				window.drawer.timeSlice = timeSlice;
				window.drawer.drawHeatMap = drawHeatMap;
				window.drawer.drawMap = drawMap;
				window.drawer.drawFoVOnly = drawFoVOnly;
				window.drawer.drawNeverSeen = drawNeverSeen;
				window.drawer.drawPath = drawPath;
				window.drawer.drawCombatLines = drawCombatLines;
				window.drawer.paths = toggleStatus;
				window.drawer.textDraw = window.textDraw;
			
			if (original != null && window.lastTime != timeSlice) {
				window.lastTime = timeSlice;
				window.UpdatePositions (timeSlice, window.mapper);
			}
			
			
			SceneView.RepaintAll ();
			
			MapperEditor.editGrid = true;
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
		}
	}
}