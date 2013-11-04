using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class MapperWindowEditor : EditorWindow
{
	
	// Data holders
	public static Cell[][][] fullMap;
	public static Cell[][] obs;
	public static List<Path> paths = new List<Path> ();
	public static List<Node> mostDanger = null, shortest = null, lengthiest = null, fastest = null, longest = null;
	// Parameters
	public static int startX, startY, maxHeatMap, endX = 27, endY = 27, timeSlice, timeSamples = 800, attemps = 25000, iterations = 5, gridSize = 60, ticksBehind = 0, numOfEnemies = 0, numOfRegions = 0, numOfCameras = 0, numOfRotCameras = 0, iterations2 = 5;
	public static bool drawMap = true, drawMoveMap = false, drawMoveUnits = false, drawNeverSeen = false, draw3dExploration = false, drawHeatMap = false, drawHeatMap3d = false, drawPath = false, drawVoronoi = false, smoothPath = true, drawShortestPath = false, drawLongestPath = false, drawLengthiestPath = false, drawFastestPath = false, drawMostDangerousPath = false, drawFoVOnly = false, seeByTime = false, seeByLength = false, seeByDanger = false, seeByLoS = false, seeByDanger3 = false, seeByLoS3 = false, seeByDanger3Norm = false, seeByLoS3Norm = false, seeByCrazy = false, seeByVelocity = false;
	public static bool randomOpEnables = false, setPathOpEnables = false;
	public static bool setEnemiesFoldout = false, queryVoronoiDiagramFoldout = false, computePathFoldout = false, default1 = true, default2 = true, default3 = true;
	public static float stepSize = 1 / 10f, crazySeconds = 5f;
	public static int[,] heatMap;
	public static GameObject start = null, end = null, floor = null, playerPrefab = null, enemyPrefab = null, waypointPrefab = null;
	public static List<GameObject> waypoints = new List<GameObject> ();
	public static Dictionary<Path, bool> toggleStatus = new Dictionary<Path, bool> ();
	public static Dictionary<Path, GameObject> players = new Dictionary<Path, GameObject> ();
	// Helping stuff
	public static List<Path> arrangedByTime, arrangedByLength, arrangedByDanger, arrangedByLoS, arrangedByDanger3, arrangedByLoS3, arrangedByDanger3Norm, arrangedByLoS3Norm, arrangedByCrazy, arrangedByVelocity;
	private static Vector2 scrollPos = new Vector2 ();
	private int lastTime = timeSlice;
	private static bool simulated = false, playing = false;
	private static float playTime = 0f;
	private Mapper mapper;
	private RRTKDTree rrt = new RRTKDTree ();
	private MapperEditorDrawer drawer;
	private static GameObject[] centreObjects = null;
	
	[MenuItem("Window/Mapper")]
	static void Init ()
	{
		MapperWindowEditor window = (MapperWindowEditor)EditorWindow.GetWindow (typeof(MapperWindowEditor));
		window.title = "Mapper";
		window.ShowTab ();
	}
	
	void OnGUI ()
	{
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
				if (mapper == null) {
					mapper = floor.AddComponent<Mapper> ();
				}
			}
		} 
		
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
		ticksBehind = EditorGUILayout.IntSlider (new GUIContent ("Ticks behind", "Number of ticks that the FoV will remain seen after the enemy has no visibility on that cell (prevents noise/jitter like behaviours)"), ticksBehind, 0, 100);
		
		if (GUILayout.Button ("Precompute Maps")) {
			
			//Find this is the view
			if (playerPrefab == null) {
				//Debug.Log("No playerPrefab"); 
				//playerPrefab = (GameObject)(Resources.Load( "../Prefab/Player.prefab", typeof(GameObject)));
				playerPrefab = GameObject.Find ("Player"); 
			}
			if (floor == null) {
				floor = (GameObject)GameObject.Find ("Floor");
				
				gridSize = EditorGUILayout.IntSlider ("Grid size", gridSize, 10, 300);
				
				if (mapper == null) {
					mapper = floor.GetComponent<Mapper> ();
					
					if (mapper == null)
						mapper = floor.AddComponent<Mapper> ();
					
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
			
			fullMap = mapper.PrecomputeMaps (floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timeSamples, stepSize, ticksBehind);
			drawer.fullMap = fullMap;
			float maxSeenGrid;
			drawer.seenNeverSeen = Analyzer.ComputeSeenValuesGrid (fullMap, out maxSeenGrid);
			drawer.seenNeverSeenMax = maxSeenGrid;
			drawer.tileSize = SpaceState.TileSize;
			drawer.zero.Set (floor.collider.bounds.min.x, floor.collider.bounds.min.z);
			
			ResetAI ();
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
		
		// Future work planned, allow the RRT to pass through this safe spots
		/*someBoolean = EditorGUILayout.Foldout (someBoolean, "Passby Waypoints");
		if (someBoolean) {
			for (int i = 0; i < waypoints.Count; i++) {
				EditorGUILayout.BeginHorizontal ();
				
				waypoints [i] = (GameObject)EditorGUILayout.ObjectField ("N:" + (i + 1), waypoints [i], typeof(GameObject), true);
				
				if (GUILayout.Button ("X", GUILayout.MaxWidth (20f))) {
					waypoints.RemoveAt (i);
					i--;
				}
				EditorGUILayout.EndHorizontal ();
			}
			
			GameObject newone = null;
			newone = (GameObject)EditorGUILayout.ObjectField ("N:" + (waypoints.Count + 1), newone, typeof(GameObject), true);
				
			if (newone != null)
				waypoints.Add (newone);
		}*/
		
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
			toggleStatus.Clear ();
			arrangedByCrazy = arrangedByDanger = arrangedByDanger3 = arrangedByDanger3Norm = arrangedByLength = arrangedByLoS = arrangedByLoS3 = arrangedByLoS3Norm = arrangedByTime = arrangedByVelocity = null;
			
			startX = (int)((start.transform.position.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x);
			startY = (int)((start.transform.position.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y);
			endX = (int)((end.transform.position.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x);
			endY = (int)((end.transform.position.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y);
			
			rrt.min = floor.collider.bounds.min;
			rrt.tileSizeX = SpaceState.TileSize.x;
			rrt.tileSizeZ = SpaceState.TileSize.y;
			rrt.enemies = SpaceState.Enemies;
			
			List<Node> nodes = null;
			for (int it = 0; it < iterations; it++) {
				nodes = rrt.Compute (startX, startY, endX, endY, attemps, speed, fullMap, smoothPath);
				if (nodes.Count > 0) {
					paths.Add (new Path (nodes));
					toggleStatus.Add (paths.Last (), true);
					paths.Last ().color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
				}
			}
			heatMap = Analyzer.Compute2DHeatMap (paths, gridSize, gridSize, out maxHeatMap);
			
			Debug.Log ("Paths found: " + paths.Count);
			
			drawer.heatMapMax = maxHeatMap;
			drawer.heatMap = heatMap;
			
			int[] maxHeatMap3d;
			drawer.heatMap3d = Analyzer.Compute3DHeatMap (paths, gridSize, gridSize, timeSamples, out maxHeatMap3d);
			drawer.heatMapMax3d = maxHeatMap3d;
						
			drawer.rrtMap = rrt.explored;
			drawer.tileSize.Set (SpaceState.TileSize.x, SpaceState.TileSize.y);
			shortest = fastest = longest = lengthiest = mostDanger = null;
			
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
		/*drawMoveMap = EditorGUILayout.Toggle ("Move map Y-axis", drawMoveMap);
		drawMoveUnits = EditorGUILayout.Toggle ("Move units Y-axis", drawMoveUnits);
		draw3dExploration = EditorGUILayout.Toggle ("Draw 3D exploration", draw3dExploration);*/
		
		if (drawer != null) {
			if (drawHeatMap3d)
				drawer.heatMap = null;
			else
				drawer.heatMap = heatMap;
		}
		
		EditorGUILayout.LabelField ("");
		
		if (GUILayout.Button ("Play")) {
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
			toggleStatus.Clear ();
			
			foreach (GameObject obj in players.Values)
				GameObject.DestroyImmediate (obj);
			
			players.Clear ();
			Resources.UnloadUnusedAssets ();
			
			int i = 1;
			foreach (Path path in paths) {
				path.name = "Path " + (i++);
				path.color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
				toggleStatus.Add (path, false);
				path.ZeroValues ();
			}
			
			Analyzer.ComputePathsTimeValues (paths);
			Analyzer.ComputePathsLengthValues (paths);
			Analyzer.ComputePathsVelocityValues (paths);
			Analyzer.ComputePathsLoSValues (paths, SpaceState.Enemies, floor.collider.bounds.min, SpaceState.TileSize.x, SpaceState.TileSize.y, fullMap, drawer.seenNeverSeen, drawer.seenNeverSeenMax);
			Analyzer.ComputePathsDangerValues (paths, SpaceState.Enemies, floor.collider.bounds.min, SpaceState.TileSize.x, SpaceState.TileSize.y, fullMap, drawer.seenNeverSeen, drawer.seenNeverSeenMax);
			Analyzer.ComputeCrazyness (paths, fullMap, Mathf.FloorToInt (crazySeconds / stepSize));
			Analyzer.ComputePathsVelocityValues (paths);
			
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
		
		// ----------------------------------
		
		#region Random Enemies
		
		EditorGUILayout.LabelField ("");
		EditorGUILayout.LabelField ("7. Random Enemies");
		
		if (waypointPrefab == null) {
			waypointPrefab = Resources.Load ("Waypoint") as GameObject;
			if (waypointPrefab != null) {
				Debug.Log ("Loading waypoint prefab from resources folder successfully!");
				PCG.Initialize (waypointPrefab);
			} else {
				waypointPrefab = GameObject.FindGameObjectWithTag ("Waypoint");
				if (waypointPrefab != null) {
					Debug.Log ("Loading waypoint prefab from scene successfully!");
				}
			}
		}
		
		randomOpEnables = mapper != null ? true : false;
		GUI.enabled = randomOpEnables;
		
		if (default1 == true) {
			default1 = false;
			setEnemiesFoldout = EditorGUILayout.Foldout (true, "Set Enemies");
		} else {
			setEnemiesFoldout = EditorGUILayout.Foldout (setEnemiesFoldout, "Set Enemies");
		}
		
		if (randomOpEnables) {
		}
		
		if (setEnemiesFoldout) {
			enemyPrefab = Resources.Load ("Enemy") as GameObject;
			enemyPrefab = (GameObject)EditorGUILayout.ObjectField ("Enemy Prefab", enemyPrefab, typeof(GameObject), false);
			numOfEnemies = EditorGUILayout.IntField ("Number of enemies", numOfEnemies);
			// Rules between numOfEnemies and numOfRegions; could apply art gallery theorem here
			numOfRegions = numOfEnemies != 0 ? 2 * numOfEnemies - 1 : 0;
			// Stationary cameras
			numOfCameras = EditorGUILayout.IntField ("Number of cameras", numOfCameras);
			// Rotational cameras
			numOfRotCameras = EditorGUILayout.IntField ("Number of rot cam", numOfRotCameras);
			
			if (GUILayout.Button ("Populate Enemies")) {
				clearUpEnemies ();
				clearUpPaths ();
				obs = mapper.ComputeObstacles ();
				Cell[][] grid = MapperEditor.grid;
				if (grid != null) {
					for (int x = 0; x < obs.Length; x++) {
						for (int y = 0; y < obs[x].Length; y++)
							if (grid [x] [y] != null)
								obs [x] [y] = grid [x] [y];
					}
				}	
				
				if (enemyPrefab != null) {
					Debug.Log ("Loading enemy prefab from resources folder successfully!");	
				} else {
					enemyPrefab = GameObject.FindGameObjectWithTag ("Enemy");
					if (enemyPrefab != null) {
						Debug.Log ("Loading enemy prefab from scene successfully!");	
					}
				}
				
				// ----------------------------------Moving Enemies-----------------------------------//
				
				for (int i = 0; i < numOfRegions; i++) {
					Vector3 position;
					do {
						position = new Vector3 (UnityEngine.Random.Range (floor.collider.bounds.min.x, floor.collider.bounds.max.x), 0.3f, UnityEngine.Random.Range (floor.collider.bounds.min.z, floor.collider.bounds.max.z));
					} while (obs[(int)((position.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x)][(int)((position.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y)].blocked == true);
					GameObject enemy = GameObject.Instantiate (enemyPrefab, position, Quaternion.identity) as GameObject;
					enemy.transform.localScale = new Vector3 (0.4f, 0.4f, 0.4f);
				}
				
				centreObjects = GameObject.FindGameObjectsWithTag ("Enemy").OrderBy (go => go.transform.position.x).ToArray ();
				// Hide all region centres
				GameObject[] enemyObjects = null;
				enemyObjects = GameObject.FindGameObjectsWithTag ("Enemy").OrderBy (go => go.transform.position.x).ToArray ();
				foreach (GameObject enemyObject in enemyObjects) {
					Enemy enemyScript;
					enemyScript = enemyObject.GetComponent ("Enemy") as Enemy;
					enemyScript.LineForFOV = new Color (1.0f, 1.0f, 1.0f, 0.0f);
					enemyObject.renderer.enabled = false;
				}
				
				// Calculate different voronoi regions and visualization is ready
				PCG.CalculateVoronoiRegions (floor, obs, numOfEnemies, numOfRegions);
				drawer.voronoiGrid = obs;
				
				// Select [numOfEnemies] regions with maximum area
				int[] maxAreaIndexArray = PCG.SelectMaximumRegions ();
				
				// Show region centres with [numOfEnemies] regions with maximum area
				for (int i = 0; i < numOfEnemies; i++) {
					Enemy enemyRestoredScript;
					enemyRestoredScript = enemyObjects.ElementAt (maxAreaIndexArray [i]).GetComponent ("Enemy") as Enemy;
					enemyRestoredScript.LineForFOV = new Color (1.0f, 0.3f, 0.0f, 1.0f);
					enemyObjects.ElementAt (maxAreaIndexArray [i]).renderer.enabled = true;
				}
				
				// -------------------------------Stationary Cameras-------------------------------- //
				
				for (int i = 0; i < numOfCameras; i++) {
					Vector3 position;
					do {
						position = new Vector3 (UnityEngine.Random.Range (floor.collider.bounds.min.x, floor.collider.bounds.max.x), 0.3f, UnityEngine.Random.Range (floor.collider.bounds.min.z, floor.collider.bounds.max.z));
					} while (obs[(int)((position.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x)][(int)((position.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y)].blocked == true);
					GameObject enemy = GameObject.Instantiate (enemyPrefab, position, Quaternion.identity) as GameObject;
					enemy.transform.localScale = new Vector3 (0.4f, 0.4f, 0.4f);
					
					Enemy scScript;
					scScript = enemy.GetComponent ("Enemy") as Enemy;
					scScript.rotationSpeed = 100;
					scScript.moveSpeed = 1;
					
					// Find aiming position
					Vector3 defaultPos = enemy.transform.position;
					Vector3 tempVector = new Vector3 (0.0f, 0.0f, 0.0f);
					Vector3 defaultDir = new Vector3 (0.0f, 0.0f, 0.0f);
					// rangeArray[0] = minX, rangeArray[1] = minY, rangeArray[2] = maxX, rangeArray[3] = maxY;
					float[] rangeArray = new float[4];
					
					float maxDistance = 0.0f;
					List<Vector3> lookingDirVec = new List<Vector3> ();
					lookingDirVec.Add (new Vector3 (0.0f, 0.0f, 1.0f));
					lookingDirVec.Add (new Vector3 (1.0f, 0.0f, 1.0f));
					lookingDirVec.Add (new Vector3 (1.0f, 0.0f, 0.0f));
					lookingDirVec.Add (new Vector3 (1.0f, 0.0f, -1.0f));
					lookingDirVec.Add (new Vector3 (0.0f, 0.0f, -1.0f));
					lookingDirVec.Add (new Vector3 (-1.0f, 0.0f, -1.0f));
					lookingDirVec.Add (new Vector3 (-1.0f, 0.0f, 0.0f));
					lookingDirVec.Add (new Vector3 (-1.0f, 0.0f, 1.0f));
					// Foreach direction in 8 directions
					foreach (Vector3 vdir in lookingDirVec) {
						// Eliminate the possibility that a stationary guard could see the goal postion or starting postion
						rangeArray = calculateRange (vdir, defaultPos, scScript.fovDistance, scScript.fovAngle);
						if (((GameObject.FindGameObjectWithTag ("End").transform.position.x < rangeArray [0] || GameObject.FindGameObjectWithTag ("End").transform.position.x > rangeArray [2]
							|| GameObject.FindGameObjectWithTag ("End").transform.position.y < rangeArray [1] || GameObject.FindGameObjectWithTag ("End").transform.position.y > rangeArray [3])
							&& (GameObject.FindGameObjectWithTag ("Start").transform.position.x < rangeArray [0] || GameObject.FindGameObjectWithTag ("Start").transform.position.x > rangeArray [2]
							|| GameObject.FindGameObjectWithTag ("Start").transform.position.y < rangeArray [1] || GameObject.FindGameObjectWithTag ("Start").transform.position.y > rangeArray [3]))
							|| (Physics.Raycast (defaultPos, new Vector3 (GameObject.FindGameObjectWithTag ("End").transform.position.x - defaultPos.x, 0.0f, GameObject.FindGameObjectWithTag ("End").transform.position.z - defaultPos.z), scScript.fovDistance)
							|| Physics.Raycast (defaultPos, new Vector3 (GameObject.FindGameObjectWithTag ("Start").transform.position.x - defaultPos.x, 0.0f, GameObject.FindGameObjectWithTag ("Start").transform.position.z - defaultPos.z), scScript.fovDistance))) {
							RaycastHit hit;
							if (Physics.Raycast (defaultPos, vdir, out hit)) {
								Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
								Debug.Log (hit.distance);
								if (hit.distance > maxDistance) {
									maxDistance = hit.distance;
									tempVector = vdir;
								}
							} else {
								if (vdir.x == 0.0f && vdir.z == 1.0f) {
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (defaultPos.z - floor.collider.bounds.min.z);
									if (defaultPos.z - floor.collider.bounds.min.z > maxDistance) {
										maxDistance = defaultPos.z - floor.collider.bounds.min.z;
										tempVector = vdir;
									}
								} else if (vdir.x == 1.0f && vdir.z == 1.0f) {
									float a1 = Mathf.Sqrt (2.0f * Mathf.Pow (defaultPos.z - floor.collider.bounds.min.z, 2.0f));
									float a2 = Mathf.Sqrt (2.0f * Mathf.Pow (floor.collider.bounds.max.x - defaultPos.x, 2.0f));
									float solution = a1 > a2 ? a2 : a1;
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (solution);
									if (solution > maxDistance) {
										maxDistance = solution;
										tempVector = vdir;
									}
								} else if (vdir.x == 1.0f && vdir.z == 0.0f) {
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (floor.collider.bounds.max.x - defaultPos.x);
									if (floor.collider.bounds.max.x - defaultPos.x > maxDistance) {
										maxDistance = floor.collider.bounds.max.x - defaultPos.x;
										tempVector = vdir;
									}
								} else if (vdir.x == 1.0f && vdir.z == -1.0f) {
									float a1 = Mathf.Sqrt (2.0f * Mathf.Pow (floor.collider.bounds.max.x - defaultPos.x, 2.0f));
									float a2 = Mathf.Sqrt (2.0f * Mathf.Pow (floor.collider.bounds.max.z - defaultPos.z, 2.0f));
									float solution = a1 > a2 ? a2 : a1;
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (solution);
									if (solution > maxDistance) {
										maxDistance = solution;
										tempVector = vdir;
									}
								} else if (vdir.x == 0.0f && vdir.z == -1.0f) {
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (floor.collider.bounds.max.z - defaultPos.z);
									if (floor.collider.bounds.max.z - defaultPos.z > maxDistance) {
										maxDistance = floor.collider.bounds.max.z - defaultPos.z;
										tempVector = vdir;
									}
								} else if (vdir.x == -1.0f && vdir.z == -1.0f) {
									float a1 = Mathf.Sqrt (2.0f * Mathf.Pow (defaultPos.x - floor.collider.bounds.min.x, 2.0f));
									float a2 = Mathf.Sqrt (2.0f * Mathf.Pow (floor.collider.bounds.max.z - defaultPos.z, 2.0f));
									float solution = a1 > a2 ? a2 : a1;
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (solution);
									if (solution > maxDistance) {
										maxDistance = solution;
										tempVector = vdir;
									}
								} else if (vdir.x == -1.0f && vdir.z == 0.0f) {
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (defaultPos.x - floor.collider.bounds.min.x);
									if (defaultPos.x - floor.collider.bounds.min.x > maxDistance) {
										maxDistance = defaultPos.x - floor.collider.bounds.min.x;
										tempVector = vdir;
									}
								} else if (vdir.x == -1.0f && vdir.z == 1.0f) {
									float a1 = Mathf.Sqrt (2.0f * Mathf.Pow (defaultPos.x - floor.collider.bounds.min.x, 2.0f));
									float a2 = Mathf.Sqrt (2.0f * Mathf.Pow (defaultPos.z - floor.collider.bounds.min.z, 2.0f));
									float solution = a1 > a2 ? a2 : a1;
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (solution);
									if (solution > maxDistance) {
										maxDistance = solution;
										tempVector = vdir;
									}
								}
							}
						}
					}
					defaultDir = tempVector;
					
					// Create waiting waypoint aimed at
					Vector3 wwpPos = new Vector3 (defaultPos.x + defaultDir.x * scScript.fovDistance / Mathf.Sqrt (defaultDir.x * defaultDir.x + defaultDir.z * defaultDir.z), 0.0f,
						defaultPos.z + defaultDir.z * scScript.fovDistance / Mathf.Sqrt (defaultDir.x * defaultDir.x + defaultDir.z * defaultDir.z));
					GameObject wwp = GameObject.Instantiate (waypointPrefab, wwpPos, Quaternion.identity) as GameObject;
					wwp.AddComponent ("WaitingWaypoint");
					DestroyImmediate (wwp.GetComponent ("Waypoint"));
					WaitingWaypoint wwpScript;
					wwpScript = wwp.GetComponent ("Waypoint") as WaitingWaypoint;
					wwpScript.next = wwpScript;
					wwpScript.waitingTime = 1000000.0f;
					scScript.target = wwpScript;
					scScript.transform.LookAt (wwpPos);
				}	
				
				//---------------------------------Rotational Cameras-----------------------------------//
				
				for (int i = 0; i < numOfRotCameras; i++) {
					Vector3 position;
					do {
						position = new Vector3 (UnityEngine.Random.Range (floor.collider.bounds.min.x, floor.collider.bounds.max.x), 0.3f, UnityEngine.Random.Range (floor.collider.bounds.min.z, floor.collider.bounds.max.z));
					} while (obs[(int)((position.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x)][(int)((position.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y)].blocked == true);
					GameObject enemy = GameObject.Instantiate (enemyPrefab, position, Quaternion.identity) as GameObject;
					enemy.transform.localScale = new Vector3 (0.4f, 0.4f, 0.4f);
					
					Enemy rcScript;
					rcScript = enemy.GetComponent ("Enemy") as Enemy;
					rcScript.rotationSpeed = 100;
					rcScript.moveSpeed = 1;
					
					// Find aiming position
					Vector3 defaultPos = enemy.transform.position;
					// rangeArray[0] = minX, rangeArray[1] = minY, rangeArray[2] = maxX, rangeArray[3] = maxY;
					float[] rangeArray = new float[4];
					
					List<Vector3> lookingDirVec = new List<Vector3> ();
					lookingDirVec.Add (new Vector3 (0.0f, 0.0f, 1.0f));
					lookingDirVec.Add (new Vector3 (1.0f, 0.0f, 1.0f));
					lookingDirVec.Add (new Vector3 (1.0f, 0.0f, 0.0f));
					lookingDirVec.Add (new Vector3 (1.0f, 0.0f, -1.0f));
					lookingDirVec.Add (new Vector3 (0.0f, 0.0f, -1.0f));
					lookingDirVec.Add (new Vector3 (-1.0f, 0.0f, -1.0f));
					lookingDirVec.Add (new Vector3 (-1.0f, 0.0f, 0.0f));
					lookingDirVec.Add (new Vector3 (-1.0f, 0.0f, 1.0f));
					
					List<float> distanceList = new List<float> ();
					
					// Foreach direction in 8 directions
					foreach (Vector3 vdir in lookingDirVec) {
						// Eliminate the possibility that a stationary guard could see the goal postion or starting postion
						rangeArray = calculateRange (vdir, defaultPos, rcScript.fovDistance, rcScript.fovAngle);
						if (((GameObject.FindGameObjectWithTag ("End").transform.position.x < rangeArray [0] || GameObject.FindGameObjectWithTag ("End").transform.position.x > rangeArray [2]
							|| GameObject.FindGameObjectWithTag ("End").transform.position.y < rangeArray [1] || GameObject.FindGameObjectWithTag ("End").transform.position.y > rangeArray [3])
							&& (GameObject.FindGameObjectWithTag ("Start").transform.position.x < rangeArray [0] || GameObject.FindGameObjectWithTag ("Start").transform.position.x > rangeArray [2]
							|| GameObject.FindGameObjectWithTag ("Start").transform.position.y < rangeArray [1] || GameObject.FindGameObjectWithTag ("Start").transform.position.y > rangeArray [3]))
							|| (Physics.Raycast (defaultPos, new Vector3 (GameObject.FindGameObjectWithTag ("End").transform.position.x - defaultPos.x, 0.0f, GameObject.FindGameObjectWithTag ("End").transform.position.z - defaultPos.z), rcScript.fovDistance)
							|| Physics.Raycast (defaultPos, new Vector3 (GameObject.FindGameObjectWithTag ("Start").transform.position.x - defaultPos.x, 0.0f, GameObject.FindGameObjectWithTag ("Start").transform.position.z - defaultPos.z), rcScript.fovDistance))) {
							RaycastHit hit;
							if (Physics.Raycast (defaultPos, vdir, out hit)) {
								Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
								Debug.Log (hit.distance);
								distanceList.Add (hit.distance);
							} else {
								if (vdir.x == 0.0f && vdir.z == 1.0f) {
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (defaultPos.z - floor.collider.bounds.min.z);
									distanceList.Add (defaultPos.z - floor.collider.bounds.min.z);
								} else if (vdir.x == 1.0f && vdir.z == 1.0f) {
									float a1 = Mathf.Sqrt (2.0f * Mathf.Pow (defaultPos.z - floor.collider.bounds.min.z, 2.0f));
									float a2 = Mathf.Sqrt (2.0f * Mathf.Pow (floor.collider.bounds.max.x - defaultPos.x, 2.0f));
									float solution = a1 > a2 ? a2 : a1;
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (solution);
									distanceList.Add (solution);
								} else if (vdir.x == 1.0f && vdir.z == 0.0f) {
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (floor.collider.bounds.max.x - defaultPos.x);
									distanceList.Add (floor.collider.bounds.max.x - defaultPos.x);
								} else if (vdir.x == 1.0f && vdir.z == -1.0f) {
									float a1 = Mathf.Sqrt (2.0f * Mathf.Pow (floor.collider.bounds.max.x - defaultPos.x, 2.0f));
									float a2 = Mathf.Sqrt (2.0f * Mathf.Pow (floor.collider.bounds.max.z - defaultPos.z, 2.0f));
									float solution = a1 > a2 ? a2 : a1;
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (solution);
									distanceList.Add (solution);
								} else if (vdir.x == 0.0f && vdir.z == -1.0f) {
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (floor.collider.bounds.max.z - defaultPos.z);
									distanceList.Add (floor.collider.bounds.max.z - defaultPos.z);
								} else if (vdir.x == -1.0f && vdir.z == -1.0f) {
									float a1 = Mathf.Sqrt (2.0f * Mathf.Pow (defaultPos.x - floor.collider.bounds.min.x, 2.0f));
									float a2 = Mathf.Sqrt (2.0f * Mathf.Pow (floor.collider.bounds.max.z - defaultPos.z, 2.0f));
									float solution = a1 > a2 ? a2 : a1;
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (solution);
									distanceList.Add (solution);
								} else if (vdir.x == -1.0f && vdir.z == 0.0f) {
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (defaultPos.x - floor.collider.bounds.min.x);
									distanceList.Add (defaultPos.x - floor.collider.bounds.min.x);
								} else if (vdir.x == -1.0f && vdir.z == 1.0f) {
									float a1 = Mathf.Sqrt (2.0f * Mathf.Pow (defaultPos.x - floor.collider.bounds.min.x, 2.0f));
									float a2 = Mathf.Sqrt (2.0f * Mathf.Pow (defaultPos.z - floor.collider.bounds.min.z, 2.0f));
									float solution = a1 > a2 ? a2 : a1;
									Debug.Log ("(" + vdir.x + "," + vdir.z + ")");
									Debug.Log (solution);
									distanceList.Add (solution);
								}
							}
						} else {
							distanceList.Add (-1.0f);
						}
					}
					
					int cnt = 0;
					int maxIndex = 0;
					float maxDistance = 0.0f;
					foreach (float tempDistance in distanceList) {
						if (tempDistance > maxDistance) {
							maxDistance = tempDistance;
							maxIndex = cnt;
						}
						cnt++;
					}
					
					int cnt2 = 0;
					int maxIndex2 = 0;
					float maxDistance2 = 0.0f;
					foreach (float tempDistance in distanceList) {
						if (tempDistance > maxDistance2 && cnt2 != maxIndex) {
							maxDistance2 = tempDistance;
							maxIndex2 = cnt2;
						}
						cnt2++;
					}
					
					// Create rotation waypoint aimed at
					Vector3 rwpPos1 = new Vector3 (defaultPos.x + lookingDirVec.ElementAt (maxIndex).x * rcScript.fovDistance / Mathf.Sqrt (lookingDirVec.ElementAt (maxIndex).x * lookingDirVec.ElementAt (maxIndex).x + lookingDirVec.ElementAt (maxIndex).z * lookingDirVec.ElementAt (maxIndex).z), 0.0f,
						defaultPos.z + lookingDirVec.ElementAt (maxIndex).z * rcScript.fovDistance / Mathf.Sqrt (lookingDirVec.ElementAt (maxIndex).x * lookingDirVec.ElementAt (maxIndex).x + lookingDirVec.ElementAt (maxIndex).z * lookingDirVec.ElementAt (maxIndex).z));
					Vector3 rwpPos2 = new Vector3 (defaultPos.x + lookingDirVec.ElementAt (maxIndex2).x * rcScript.fovDistance / Mathf.Sqrt (lookingDirVec.ElementAt (maxIndex2).x * lookingDirVec.ElementAt (maxIndex2).x + lookingDirVec.ElementAt (maxIndex2).z * lookingDirVec.ElementAt (maxIndex2).z), 0.0f,
						defaultPos.z + lookingDirVec.ElementAt (maxIndex2).z * rcScript.fovDistance / Mathf.Sqrt (lookingDirVec.ElementAt (maxIndex2).x * lookingDirVec.ElementAt (maxIndex2).x + lookingDirVec.ElementAt (maxIndex2).z * lookingDirVec.ElementAt (maxIndex2).z));
					GameObject rwp1 = GameObject.Instantiate (waypointPrefab, rwpPos1, Quaternion.identity) as GameObject;
					GameObject rwp2 = GameObject.Instantiate (waypointPrefab, rwpPos2, Quaternion.identity) as GameObject;
					rwp1.AddComponent ("RotationWaypoint");
					rwp2.AddComponent ("RotationWaypoint");
					DestroyImmediate (rwp1.GetComponent ("Waypoint"));
					DestroyImmediate (rwp2.GetComponent ("Waypoint"));
					RotationWaypoint rwpScript1;
					RotationWaypoint rwpScript2;
					rwpScript1 = rwp1.GetComponent ("Waypoint") as RotationWaypoint;
					rwpScript2 = rwp2.GetComponent ("Waypoint") as RotationWaypoint;
					rwpScript1.next = rwpScript2;
					rwpScript2.next = rwpScript1;
					rwpScript1.lookDir = new Vector3 (lookingDirVec.ElementAt (maxIndex).x, lookingDirVec.ElementAt (maxIndex).y, lookingDirVec.ElementAt (maxIndex).z);
					rwpScript2.lookDir = new Vector3 (lookingDirVec.ElementAt (maxIndex2).x, lookingDirVec.ElementAt (maxIndex2).y, lookingDirVec.ElementAt (maxIndex2).z);
					rcScript.target = rwpScript1;
					rcScript.transform.LookAt (rwpPos1);
				}
				
				
				setPathOpEnables = true;

			}
			EditorGUILayout.LabelField ("");
		}
		
		if (default2 == true) {
			default2 = false;
			queryVoronoiDiagramFoldout = EditorGUILayout.Foldout (true, "Query Voronoi Diagram Info");
		} else {
			queryVoronoiDiagramFoldout = EditorGUILayout.Foldout (queryVoronoiDiagramFoldout, "Query Voronoi Diagram Info");
		}
		if (queryVoronoiDiagramFoldout) { 
			// Show the number of Voronoi centres
			numOfRegions = numOfEnemies != 0 ? 2 * numOfEnemies - 1 : 0;
			EditorGUILayout.TextField ("Number of regions", numOfRegions.ToString ());
			// Toggle between visualizing or not
			drawVoronoi = EditorGUILayout.Toggle ("Draw Voronoi Cells", drawVoronoi);
			EditorGUILayout.LabelField ("");
		}
		
		// Path
		GUI.enabled = setPathOpEnables;
		
		if (default3 == true) {
			default3 = false;
			computePathFoldout = EditorGUILayout.Foldout (true, "Compute Path");
		} else {
			computePathFoldout = EditorGUILayout.Foldout (computePathFoldout, "Compute Path");
		}
		iterations2 = EditorGUILayout.IntSlider ("Iterations", iterations2, 1, 20);
		if (computePathFoldout) {
			if (GUILayout.Button ("Set Patroling Path")) {
				clearUpPaths ();
				PCG.PathInVoronoiRegion (floor, obs, iterations2);
				PCG.DestroyVoronoiCentre ();
				// StorePositions ();
			}
		}
		
		GUI.enabled = true;
		
		
		#endregion
		
		
		foreach (KeyValuePair<Path, bool> p in toggleStatus) {
			if (p.Value) {
				if (!players.ContainsKey (p.Key)) {
					GameObject player = GameObject.Instantiate (playerPrefab) as GameObject;
					player.transform.position.Set (p.Key.points [0].x, 0f, p.Key.points [0].y);
					players.Add (p.Key, player);
					Material m = new Material (player.renderer.sharedMaterial);
					m.color = p.Key.color;
					player.renderer.material = m;
					Resources.UnloadUnusedAssets ();
				} else {
					players [p.Key].SetActive (true);
				}
			} else {
				if (players.ContainsKey (p.Key)) {
					players [p.Key].SetActive (false);
				}
			}
		}
		
		EditorGUILayout.EndScrollView ();
		
		// ----------------------------------
		
		if (drawer != null) {
			drawer.timeSlice = timeSlice;
			drawer.draw3dExploration = draw3dExploration;
			drawer.drawHeatMap = drawHeatMap;
			drawer.drawMap = drawMap;
			drawer.drawFoVOnly = drawFoVOnly;
			drawer.drawMoveMap = drawMoveMap;
			drawer.drawNeverSeen = drawNeverSeen;
			drawer.drawPath = drawPath;
			drawer.paths = toggleStatus;
			drawer.drawVoronoi = drawVoronoi;
			
		}
		
		if (fullMap != null && lastTime != timeSlice) {
			lastTime = timeSlice;
			UpdatePositions (timeSlice, mapper);
		}
		
		SceneView.RepaintAll ();
	}
	
	// Calculate range
	private float[] calculateRange (Vector3 vdir, Vector3 defaultPos, float fovDistance, float fovAngle)
	{
		float[] range = new float[4];
		float minX, minY, maxX, maxY;
		float cosTheta, sinTheta, x, y, xA, yA, xB, yB;
		Vector2 pA, pB, pC, dir;
		pC = new Vector2 (defaultPos.x, defaultPos.z);
		dir = new Vector2 (vdir.x, vdir.z);
		
		cosTheta = dir.x / Mathf.Sqrt (dir.x * dir.x + dir.y * dir.y);
		sinTheta = dir.y / Mathf.Sqrt (dir.x * dir.x + dir.y * dir.y);
		x = fovDistance;
		y = fovDistance * Mathf.Tan ((fovAngle / 2) / 180.0f * Mathf.PI);
		xA = cosTheta * x - sinTheta * y + pC.x;
		yA = sinTheta * x + cosTheta * y + pC.y;
		xB = cosTheta * x + sinTheta * y + pC.x;
		yB = sinTheta * x - cosTheta * y + pC.y;
		pA = new Vector2 (xA, yA);
		pB = new Vector2 (xB, yB);
		
		// min and max
		maxX = Mathf.Max (Mathf.Max (pA.x, pB.x), pC.x);
		minX = Mathf.Min (Mathf.Min (pA.x, pB.x), pC.x);
		maxY = Mathf.Max (Mathf.Max (pA.y, pB.y), pC.y);
		minY = Mathf.Min (Mathf.Min (pA.y, pB.y), pC.y);
		
		range [0] = minX;
		range [1] = minY;
		range [2] = maxX;
		range [3] = maxY;
		
		return range;
	}
	
	private void clearUpEnemies ()
	{
		GameObject[] gos = null;
		gos = GameObject.FindGameObjectsWithTag ("Enemy").OrderBy (go => go.transform.position.x).ToArray ();
		if (gos != null) {
			foreach (GameObject g in gos) {
				DestroyImmediate (g);	
			}
		}
	}
	
	private void clearUpPaths ()
	{
		GameObject[] wos = null;
		wos = GameObject.FindGameObjectsWithTag ("Waypoint").ToArray ();
		if (wos != null) {
			foreach (GameObject w in wos) {
				DestroyImmediate (w);	
			}
		}
	}

	void BatchComputing ()
	{
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
							
				fullMap = mapper.PrecomputeMaps (floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridsize, timesamples, stepSize);
							
				startX = (int)((start.transform.position.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x);
				startY = (int)((start.transform.position.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y);
				endX = (int)((end.transform.position.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x);
				endY = (int)((end.transform.position.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y);

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
				
			XmlSerializer ser = new XmlSerializer (typeof(ResultsRoot), new Type[] {typeof(ResultBatch), typeof(Result)});
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
					
				fullMap = mapper.PrecomputeMaps (floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timesamples, stepSize);
					
				startX = (int)((start.transform.position.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x);
				startY = (int)((start.transform.position.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y);
				endX = (int)((end.transform.position.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x);
				endY = (int)((end.transform.position.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y);
					
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
				
			XmlSerializer ser = new XmlSerializer (typeof(ResultsRoot), new Type[] {typeof(ResultBatch), typeof(Result)});
			ser.Serialize (stream, root);
			stream.Flush ();
			stream.Close ();
		}
			
		// Attemp varying batch tests
			
		using (FileStream stream = new FileStream ("attempvary" + att + ".xml", FileMode.Create)) {
			
			root.everything.Clear ();
				
			gridsize = 60;
			timesamples = 1200;

			fullMap = mapper.PrecomputeMaps (floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timesamples, stepSize);
				
			startX = (int)((start.transform.position.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x);
			startY = (int)((start.transform.position.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y);
			endX = (int)((end.transform.position.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x);
			endY = (int)((end.transform.position.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y);
				
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
				
			XmlSerializer ser = new XmlSerializer (typeof(ResultsRoot), new Type[] {typeof(ResultBatch), typeof(Result)});
			ser.Serialize (stream, root);
			stream.Flush ();
			stream.Close ();
		}
			
		//}
	}
	
	void CalculateVoronoi ()
	{
		if (floor == null) {
			floor = GameObject.FindGameObjectWithTag ("Floor");
		}
		
		
		//Get all the point to calculate the voronoi
		GameObject[] gos;
		List<Vector2> pointsVoronoi = new List<Vector2> ();
		gos = GameObject.FindGameObjectsWithTag ("Voronoi").OrderBy (go => go.transform.position.x).ToArray ();
		
		foreach (GameObject g in gos) {
			pointsVoronoi.Add (new Vector2 (g.transform.position.x, g.transform.position.z));
		}
		
		//Debug.DrawLine(gos[0].transform.position,gos[1].transform.position);
		//Triangulator triangulator = new Triangulator();
		//triangulator.CreateInfluencePolygon(pointsVoronoi.ToArray());
	}
	
	public void Update ()
	{
		if (playing) {
			playTime += 1 / 100f;
			if (playTime > stepSize) {
				playTime = 0f;
				timeSlice++;
				if (timeSlice >= timeSamples) {
					timeSlice = 0;
				}
				drawer.timeSlice = timeSlice;
				UpdatePositions (timeSlice, mapper);
			}
		}
	}
	
	// Resets the AI back to it's original position
	public void ResetAI ()
	{
		GameObject[] objs = GameObject.FindGameObjectsWithTag ("AI") as GameObject[];
		foreach (GameObject ob in objs)
			ob.GetComponent<Player> ().ResetSimulation ();
		
		objs = GameObject.FindGameObjectsWithTag ("Enemy") as GameObject[];
		foreach (GameObject ob in objs) 
			ob.GetComponent<Enemy> ().ResetSimulation ();
	}
	
	// Updates everyone's position to the current timeslice
	public void UpdatePositions (int t, Mapper mapper)
	{
		for (int i = 0; i < SpaceState.Enemies.Length; i++) {
			if (SpaceState.Enemies [i] == null)
				continue;
			
			Vector3 pos = SpaceState.Enemies [i].positions [t];
			if (drawMoveUnits)
				pos.y = t;
			SpaceState.Enemies [i].transform.position = pos;
			SpaceState.Enemies [i].transform.rotation = SpaceState.Enemies [i].rotations [t];
		}
		
		/*GameObject[] objs = GameObject.FindGameObjectsWithTag ("AI") as GameObject[];
		for (int i = 0; i < objs.Length; i++) {
			if (path != null && path.Count > 0) {
				Node p = null;
				foreach (Node n in path) {
					if (n.t > t) {
						p = n;
						break;
					}
				}
				if (p != null) {
					Vector3 n2 = p.GetVector3 ();
					Vector3 n1 = p.parent.GetVector3 ();
					Vector3 pos = n1 * (1 - (float)(t - p.parent.t) / (float)(p.t - p.parent.t)) + n2 * ((float)(t - p.parent.t) / (float)(p.t - p.parent.t));
					
					pos.x *= mapper.tileSizeX;
					pos.z *= mapper.tileSizeZ;
					pos.x += floor.collider.bounds.min.x;
					pos.z += floor.collider.bounds.min.z;
					if (!drawMoveUnits) {
						pos.y = 0f;
					}
					objs [i].transform.position = pos;
				}
			
			}
		}*/
		
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
					
					pos.x *= SpaceState.TileSize.x;
					pos.z *= SpaceState.TileSize.y;
					pos.x += floor.collider.bounds.min.x;
					pos.z += floor.collider.bounds.min.z;
					if (!drawMoveUnits) {
						pos.y = 0f;
					}
					each.Value.transform.position = pos;
				}
			}
		}
	}
	
	public void StorePositions ()
	{
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
