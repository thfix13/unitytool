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
	public static int startX, startY, maxHeatMap, endX = 27, endY = 27, timeSlice, timeSamples = 800, attemps = 25000, iterations = 5, gridSize = 75, ticksBehind = 0, numOfEnemies = 0, numOfRegionsForEnemies = 0, numOfCameras = 0, numOfRegionsForCameras = 0, iterations2 = 5, iterations3 = 5, noeB = 1, nocB = 1, noreB = 0, norcB = 0, numOfGuards = 0, numOfGuards2 = 1, iterations4 = 3, iterations5 = 2, nogB = 2, noiB = 4;
	public static int pLine = 50, pDot = 50, pSplit = 50, pZigZag = 50, pPause = 25, pSwipe = 25, pFullRotate = 25, pNinety = 25, pLine2 = 50, pDot2 = 50, pSplit2 = 50, pZigZag2 = 50, pPause2 = 25, pSwipe2 = 25, pFullRotate2 = 25, pNinety2 = 25;
	public static bool drawMap = true, drawMoveMap = false, drawMoveUnits = false, drawNeverSeen = false, draw3dExploration = false, drawHeatMap = false, drawHeatMap3d = false, drawPath = false, 
		drawVoronoiForEnemies = false, drawVoronoiForCameras = false, drawVoronoiForBoundaries = false, drawBoundaries = false, drawRoadmaps = false, drawRoadmaps2 = false, drawRoadmaps3 = false, drawGraph = false, drawGraph2 = false,
		smoothPath = true, drawShortestPath = false, drawLongestPath = false, drawLengthiestPath = false, drawFastestPath = false, drawMostDangerousPath = false, drawFoVOnly = true, seeByTime = false, seeByLength = false, seeByDanger = false, seeByLoS = false, seeByDanger3 = false, seeByLoS3 = false, seeByDanger3Norm = false, seeByLoS3Norm = false, seeByCrazy = false, seeByVelocity = false;
	public static bool randomOpEnables = false, setPathOpEnables = false, setRotationOpEnables = false, boundariesFloodingOpEnables = false, extractRoadmapOpEnables = false, initializeGraphOpEnables = false, mergeOpEnables = false, moreStepsBtnEnables = false, shortcutBtnEnables = false, behaviorOpEnables = false;
	public static bool setEnemiesFoldout = false, setCameraFoldout = false, queryeVoronoiDiagramFoldout = false, querycVoronoiDiagramFoldout = false, computePathFoldout = false, setEnemiesFoldout2 = false, setGraphFoldout = false, setShortcutFoldout = false, setRhythmsFoldout = false, setMultipleBehavioursFoldout = false, setPossibilitiesFoldout = false, setMultipleBehavioursFoldout2 = false, setPossibilitiesFoldout2 = false, default1 = true, default2 = true, default3 = true, default4 = true, default5 = true, default6 = true, default7 = false, default8 = true, default9 = true, default10 = false, default11 = true, default12 = true, default13 = true, default14 = true;
	public static bool moreStepsClicked = false, shortcutClicked = false;
	public static float stepSize = 1 / 10f, crazySeconds = 5f;
	public static int[,] heatMap;
	public static GameObject start = null, end = null, floor = null, playerPrefab = null, enemyPrefab = null, waypointPrefab = null, enemyPrefab2 = null;
	public static List<GameObject> waypoints = new List<GameObject> ();
	public static Dictionary<Path, bool> toggleStatus = new Dictionary<Path, bool> ();
	public static Dictionary<Path, GameObject> players = new Dictionary<Path, GameObject> ();
	public static List<float> ratios = new List<float> ();
	// Helping stuff
	public static List<Path> arrangedByTime, arrangedByLength, arrangedByDanger, arrangedByLoS, arrangedByDanger3, arrangedByLoS3, arrangedByDanger3Norm, arrangedByLoS3Norm, arrangedByCrazy, arrangedByVelocity;
	private static Vector2 scrollPos = new Vector2 ();
	private int lastTime = timeSlice;
	private static bool simulated = false, playing = false;
	private static float playTime = 0f;
	private Mapper mapper;
	private RRTKDTree rrt = new RRTKDTree ();
	private MapperEditorDrawer drawer;
	private GameObject[] enemyObjects = null, cameraObjects = null, pathObjects = null, angleObjects = null, enemypathObjects = null;
	private List<GameObject> playerObjects = new List<GameObject> ();
	
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

			moreStepsClicked = false;
			shortcutClicked = false;
		} 
		
		if (GUILayout.Button ("Precompute Maps Over Time-Preserving")) {
			
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
				StorePositionsOverTimePreserving ();
				simulated = true;
			}
			
			fullMap = mapper.PrecomputeMapsOverTimePreserving (floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timeSamples, stepSize, ticksBehind);
			drawer.fullMap = fullMap;
			float maxSeenGrid;
			drawer.seenNeverSeen = Analyzer.ComputeSeenValuesGrid (fullMap, out maxSeenGrid);
			drawer.seenNeverSeenMax = maxSeenGrid;
			drawer.tileSize = SpaceState.TileSize;
			drawer.zero.Set (floor.collider.bounds.min.x, floor.collider.bounds.min.z);
			
			ResetAIOverTimePreserving ();
			
			moreStepsClicked = false;
			shortcutClicked = false;
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
			} else {
				waypointPrefab = GameObject.FindGameObjectWithTag ("Waypoint");
				if (waypointPrefab != null) {
					Debug.Log ("Loading waypoint prefab from scene successfully!");
				}
			}
		}
		
		if (enemyPrefab == null) {
			enemyPrefab = Resources.Load ("Enemy") as GameObject;
			if (enemyPrefab != null) {
				Debug.Log ("Loading enemy prefab from resources folder successfully!");	
			} else {
				enemyPrefab = GameObject.FindGameObjectWithTag ("Enemy");
				if (enemyPrefab != null) {
					Debug.Log ("Loading enemy prefab from scene successfully!");	
				}
			}
		}
		
		if (enemyPrefab2 == null) {
			enemyPrefab2 = Resources.Load ("Enemy2") as GameObject;
			if (enemyPrefab2 != null) {
				Debug.Log ("Loading enemy prefab 2 from resources folder successfully!");	
			} else {
				enemyPrefab2 = GameObject.FindGameObjectWithTag ("Enemy");
				if (enemyPrefab2 != null) {
					Debug.Log ("Loading enemy prefab 2 from scene successfully!");	
				}
			}
		}
		
		randomOpEnables = fullMap != null ? true : false;
		GUI.enabled = randomOpEnables;
		
		if (default1 == true) {
			default1 = false;
			setEnemiesFoldout = EditorGUILayout.Foldout (true, "Set Moving Enemies");
		} else {
			setEnemiesFoldout = EditorGUILayout.Foldout (setEnemiesFoldout, "Set Moving Enemies");
		}
		
		if (setEnemiesFoldout) {
			enemyPrefab = (GameObject)EditorGUILayout.ObjectField ("Enemy Prefab", enemyPrefab, typeof(GameObject), false);
			numOfEnemies = EditorGUILayout.IntField ("Number of enemies", numOfEnemies);
			// Rules between numOfEnemies and numOfRegionsForEnemies; could apply art gallery theorem here
			numOfRegionsForEnemies = numOfEnemies != 0 ? 2 * numOfEnemies - 1 : 0;
			
			// ----------------------------------Moving Enemies-----------------------------------//
			if (GUILayout.Button ("Populate Moving Enemies")) {
				// Clear up enemies and their paths in the scene
				PCG.ClearUpEnemies (enemyObjects);
				PCG.ClearUpPaths (pathObjects);

				if (obs == null) { 
					obs = mapper.ComputeObstacles ();
					Cell[][] grid = MapperEditor.grid;
					if (grid != null) {
						for (int x = 0; x < obs.Length; x++) {
							for (int y = 0; y < obs[x].Length; y++)
								if (grid [x] [y] != null)
									obs [x] [y] = grid [x] [y];
						}
					}
					PCG.Initialize (enemyPrefab, waypointPrefab, obs);
				} else {
					PCG.ResetEnemiesObs ();	
				}
				
				PCG.numOfEnemies = numOfEnemies;
				PCG.numOfRegionsForEnemies = numOfRegionsForEnemies;
				// Populate region centres as enemies
				enemyObjects = PCG.PopulateEnemies (floor).ToArray ();
				
				// Hide all region centres
				foreach (GameObject enemyObject in enemyObjects) {
					Enemy enemyScript;
					enemyScript = enemyObject.GetComponent ("Enemy") as Enemy;
					enemyScript.LineForFOV = new Color (1.0f, 1.0f, 1.0f, 0.0f);
					enemyObject.renderer.enabled = false;
				}
				
				// Calculate different voronoi regions and visualization is ready
				// PCG.vEnemy.calculateVoronoiRegions (floor, PCG.numOfEnemies, PCG.numOfRegionsForEnemies, enemyObjects);
				PCG.vEnemy.calculateVoronoiRegionsUsingFlooding (floor, PCG.numOfEnemies, PCG.numOfRegionsForEnemies, enemyObjects);
				drawer.eVoronoiGrid = PCG.vEnemy.obs;
				
				// Select [numOfEnemies] regions with maximum area
				int[] maxAreaIndexArrayForEnemies = PCG.vEnemy.selectMaximumRegions ();
				
				// Show region centres with [numOfEnemies] regions with maximum area
				for (int i = 0; i < numOfEnemies; i++) {
					Enemy enemyRestoredScript;
					enemyRestoredScript = enemyObjects.ElementAt (maxAreaIndexArrayForEnemies [i]).GetComponent ("Enemy") as Enemy;
					enemyRestoredScript.LineForFOV = new Color (1.0f, 0.3f, 0.0f, 1.0f);
					enemyObjects.ElementAt (maxAreaIndexArrayForEnemies [i]).renderer.enabled = true;
					Material tempMaterial = new Material (enemyObjects.ElementAt (maxAreaIndexArrayForEnemies [i]).renderer.sharedMaterial);
					tempMaterial.color = new Color (0.0f, 0.55f, 0.55f, 1.0f);
					enemyObjects.ElementAt (maxAreaIndexArrayForEnemies [i]).renderer.sharedMaterial = tempMaterial;
				}
				
				PCG.maxAreaIndexHolderE = maxAreaIndexArrayForEnemies;
				
				// Enable generating paths
				setPathOpEnables = true;
			}
			
			EditorGUILayout.LabelField ("");
		}
		
		if (default2 == true) {
			default2 = false;
			queryeVoronoiDiagramFoldout = EditorGUILayout.Foldout (true, "Query Voronoi Diagram Info for Enemies");
		} else {
			queryeVoronoiDiagramFoldout = EditorGUILayout.Foldout (queryeVoronoiDiagramFoldout, "Query Voronoi Diagram Info for Enemies");
		}
		if (queryeVoronoiDiagramFoldout) { 
			// Show the number of Voronoi centres
			numOfRegionsForEnemies = numOfEnemies != 0 ? 2 * numOfEnemies - 1 : 0;
			EditorGUILayout.TextField ("Number of regions", numOfRegionsForEnemies.ToString ());
			// Toggle between visualizing or not
			bool checkedBeforeE = drawVoronoiForEnemies;
			drawVoronoiForEnemies = EditorGUILayout.Toggle ("Draw Voronoi Cells", drawVoronoiForEnemies);
			bool checkedAfterE = drawVoronoiForEnemies;
			if (checkedBeforeE != checkedAfterE && drawVoronoiForCameras) {
				drawVoronoiForCameras = false;	
			}
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
				PCG.ClearUpPaths (pathObjects);
				pathObjects = PCG.PathInVoronoiRegion (floor, PCG.vEnemy.obs, iterations2).ToArray ();
				PCG.DestroyVoronoiCentreForEnemies ();
				StorePositions ();
			}
		}
		EditorGUILayout.LabelField ("");
		
		GUI.enabled = true;
		EditorGUILayout.LabelField ("8. Random Cameras");
		GUI.enabled = randomOpEnables;
		
		if (default4 == true) {
			default4 = false;
			setCameraFoldout = EditorGUILayout.Foldout (true, "Set Rotational Cameras");
		} else {
			setCameraFoldout = EditorGUILayout.Foldout (setCameraFoldout, "Set Rotational Cameras");
		}
		if (setCameraFoldout) {
			enemyPrefab = (GameObject)EditorGUILayout.ObjectField ("Enemy Prefab", enemyPrefab, typeof(GameObject), false);
			numOfCameras = EditorGUILayout.IntField ("Number of cameras", numOfCameras);
			
			// ----------------------------------Rotational Cameras-----------------------------------//
			if (GUILayout.Button ("Populate Rotational Cameras")) {
				PCG.ClearUpCameras (cameraObjects);
				PCG.ClearUpAngles (angleObjects);
				PCG.numOfCameras = numOfCameras;
				PCG.numOfRegionsForCameras = numOfRegionsForCameras;
				
				if (obs == null) { 
					obs = mapper.ComputeObstacles ();
					Cell[][] grid = MapperEditor.grid;
					if (grid != null) {
						for (int x = 0; x < obs.Length; x++) {
							for (int y = 0; y < obs[x].Length; y++)
								if (grid [x] [y] != null)
									obs [x] [y] = grid [x] [y];
						}
					}
					PCG.Initialize (enemyPrefab, waypointPrefab, obs);
				} else {
					PCG.ResetCamerasObs ();	
				}
				
				cameraObjects = PCG.PopulateCameras (floor).ToArray ();

				// Hide all region centres
				foreach (GameObject cameraObject in cameraObjects) {
					Enemy cameraScript;
					cameraScript = cameraObject.GetComponent ("Enemy") as Enemy;
					cameraScript.LineForFOV = new Color (1.0f, 1.0f, 1.0f, 0.0f);
					cameraScript.renderer.enabled = false;
				}
				
				// Calculate different voronoi regions and visualization is ready
				// PCG.vCamera.calculateVoronoiRegions (floor, PCG.vCamera.obs, PCG.numOfCameras, PCG.numOfRegionsForCameras, cameraObjects);
				PCG.vCamera.calculateVoronoiRegionsUsingFlooding (floor, PCG.numOfCameras, PCG.numOfRegionsForCameras, cameraObjects);
				PCG.vCamera.calculateBoundaries (floor);
				drawer.cVoronoiGrid = PCG.vCamera.obs;
				
				// Select [numOfCameras] regions with maximum area
				int[] maxAreaIndexArrayForCameras = PCG.vCamera.selectMaximumRegions ();
				
				// Show region centres with [numOfCameras] regions with maximum area
				for (int i = 0; i < numOfCameras; i++) {
					Enemy cameraRestoredScript;
					cameraRestoredScript = cameraObjects.ElementAt (maxAreaIndexArrayForCameras [i]).GetComponent ("Enemy") as Enemy;
					cameraRestoredScript.LineForFOV = new Color (1.0f, 0.3f, 0.0f, 1.0f);
					cameraObjects.ElementAt (maxAreaIndexArrayForCameras [i]).renderer.enabled = true;
					Material tempMaterial = new Material (cameraObjects.ElementAt (maxAreaIndexArrayForCameras [i]).renderer.sharedMaterial);
					tempMaterial.color = new Color (0.75f, 0.3f, 0.15f, 1.0f);
					cameraObjects.ElementAt (maxAreaIndexArrayForCameras [i]).renderer.sharedMaterial = tempMaterial;
					cameraRestoredScript.rotationSpeed = 50;
					cameraRestoredScript.moveSpeed = 1;
				}
				
				PCG.maxAreaIndexHolderC = maxAreaIndexArrayForCameras;
				
				setRotationOpEnables = true;
			}
			EditorGUILayout.LabelField ("");	
		}
		
		if (default5 == true) {
			default5 = false;
			querycVoronoiDiagramFoldout = EditorGUILayout.Foldout (true, "Query Voronoi Diagram Info");
		} else {
			querycVoronoiDiagramFoldout = EditorGUILayout.Foldout (querycVoronoiDiagramFoldout, "Query Voronoi Diagram Info");
		}
		if (querycVoronoiDiagramFoldout) { 
			// Show the number of Voronoi centres
			numOfRegionsForCameras = numOfCameras != 0 ? 2 * numOfCameras - 1 : 0;
			EditorGUILayout.TextField ("Number of regions", numOfRegionsForCameras.ToString ());
			// Toggle between visualizing or not
			bool checkedBeforeC = drawVoronoiForCameras;
			drawVoronoiForCameras = EditorGUILayout.Toggle ("Draw Voronoi Cells", drawVoronoiForCameras);
			bool checkedAfterC = drawVoronoiForCameras;
			if (checkedBeforeC != checkedAfterC && drawVoronoiForEnemies) {
				drawVoronoiForEnemies = false;
			}
			EditorGUILayout.LabelField ("");
		}
		
		// Rotation
		GUI.enabled = setRotationOpEnables;
		
		if (default6 == true) {
			default6 = false;
			computePathFoldout = EditorGUILayout.Foldout (true, "Compute Rotation");
		} else {
			computePathFoldout = EditorGUILayout.Foldout (computePathFoldout, "Compute Rotation");
		}
		iterations3 = EditorGUILayout.IntSlider ("Iterations", iterations3, 1, 20);
		if (computePathFoldout) {
			if (GUILayout.Button ("Set Rotation Directions")) {
				PCG.ClearUpAngles (angleObjects);
				angleObjects = PCG.RotationInVoronoiRegion (floor, PCG.vCamera.obs, iterations3).ToArray ();
				PCG.DestroyVoronoiCentreForCameras ();
				StorePositions ();
			}
		}
		EditorGUILayout.LabelField ("");

		GUI.enabled = true;

		if (GUILayout.Button ("Reset")) {
			PCG.ClearUpEnemies (enemyObjects);
			PCG.ClearUpPaths (pathObjects);
			PCG.ClearUpCameras (cameraObjects);
			PCG.ClearUpAngles (angleObjects);

			fullMap = null;
			drawer.fullMap = fullMap;
			simulated = false;

			foreach (GameObject p in playerObjects) {
				DestroyImmediate (p);
			}
			players.Clear ();
			playing = false;
			ResetAI ();

			obs = null;
			PCG.vEnemy.obs = null;
			drawer.eVoronoiGrid = PCG.vEnemy.obs;
			PCG.vCamera.obs = null;
			drawer.cVoronoiGrid = PCG.vCamera.obs;

			numOfEnemies = 0;
			numOfCameras = 0;
			numOfRegionsForEnemies = 0;
			numOfRegionsForCameras = 0;

			setPathOpEnables = false;
			setRotationOpEnables = false;
		}
		
		#endregion
		
		#region Batch
		
		EditorGUILayout.LabelField ("");
		EditorGUILayout.LabelField ("9. Batch Computing for Enemies");

		GUI.enabled = true;
		noeB = EditorGUILayout.IntField ("Number of enemies", noeB);
		
		if (GUILayout.Button ("Batch Computation for Enemies")) {
			
			BResultsRoot2 root = new BResultsRoot2 ();
			using (FileStream stream = new FileStream ("Ratio with respect to Guards.xml", FileMode.Create)) {
				for (noeB = 0; noeB <= 8; noeB++) {
					BResultBatch2 job = new BResultBatch2 ();
					job.numOfGuards = noeB;
					
					noreB = noeB != 0 ? 2 * noeB - 1 : 0;
					
					for (int batchIter = 0; batchIter < 2; batchIter ++) {
						// Create moving enemies and rotational cameras
						PCG.ClearUpEnemies (enemyObjects);
						PCG.ClearUpPaths (pathObjects);
				
						if (obs == null) { 
							obs = mapper.ComputeObstacles ();
							Cell[][] grid = MapperEditor.grid;
							if (grid != null) {
								for (int x = 0; x < obs.Length; x++) {
									for (int y = 0; y < obs[x].Length; y++)
										if (grid [x] [y] != null)
											obs [x] [y] = grid [x] [y];
								}
							}
							PCG.Initialize (enemyPrefab, waypointPrefab, obs);
						} else {
							PCG.ResetEnemiesObs ();	
						}
								
						PCG.numOfEnemies = noeB;
						PCG.numOfRegionsForEnemies = noreB;
						// Populate region centres as enemies
						enemyObjects = PCG.PopulateEnemies (floor).ToArray ();
								
						// Hide all region centres
						foreach (GameObject enemyObject in enemyObjects) {
							Enemy enemyScript;
							enemyScript = enemyObject.GetComponent ("Enemy") as Enemy;
							enemyScript.LineForFOV = new Color (1.0f, 1.0f, 1.0f, 0.0f);
							enemyObject.renderer.enabled = false;
						}
								
						// Calculate different voronoi regions and visualization is ready
						// PCG.vEnemy.calculateVoronoiRegions (floor, PCG.numOfEnemies, PCG.numOfRegionsForEnemies, enemyObjects);
						PCG.vEnemy.calculateVoronoiRegionsUsingFlooding (floor, PCG.numOfEnemies, PCG.numOfRegionsForEnemies, enemyObjects);
						//drawer.eVoronoiGrid = PCG.vEnemy.obs;
								
						// Select [numOfEnemies] regions with maximum area
						int[] maxAreaIndexArrayForEnemies = PCG.vEnemy.selectMaximumRegions ();
								
						// Show region centres with [numOfEnemies] regions with maximum area
						for (int i = 0; i < noeB; i++) {
							Enemy enemyRestoredScript;
							enemyRestoredScript = enemyObjects.ElementAt (maxAreaIndexArrayForEnemies [i]).GetComponent ("Enemy") as Enemy;
							enemyRestoredScript.LineForFOV = new Color (1.0f, 0.3f, 0.0f, 1.0f);
							enemyObjects.ElementAt (maxAreaIndexArrayForEnemies [i]).renderer.enabled = true;
		
						}
								
						PCG.maxAreaIndexHolderE = maxAreaIndexArrayForEnemies;
								
						int batchIter2 = 20;
						pathObjects = PCG.PathInVoronoiRegion (floor, PCG.vEnemy.obs, batchIter2).ToArray ();
						PCG.DestroyVoronoiCentreForEnemies ();
							
						// Store positions all together
						StorePositions ();
						// Precompute maps again
						fullMap = mapper.PrecomputeMaps (floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timeSamples, stepSize, ticksBehind);
						ResetAI ();
						// Compute paths
						float speed = GameObject.FindGameObjectWithTag ("AI").GetComponent<Player> ().speed;
						//Check the start and the end and get them from the editor. 
						if (start == null) {
							start = GameObject.Find ("Start");
						}
						if (end == null) {
							end = GameObject.Find ("End");	
						}
								
						paths.Clear ();
						//toggleStatus.Clear ();
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
						iterations = 20;
						for (int it = 0; it < iterations; it++) {
							nodes = rrt.Compute (startX, startY, endX, endY, attemps, speed, fullMap, smoothPath);
							if (nodes.Count > 0) {
								paths.Add (new Path (nodes));
								//toggleStatus.Add (paths.Last (), true);
								//paths.Last ().color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
							}
						}
						//heatMap = Analyzer.Compute2DHeatMap (paths, gridSize, gridSize, out maxHeatMap);
								
						//Debug.Log ("Paths found: " + paths.Count);
						//				Debug.Log ("Ratio: " + (float)paths.Count / iterations);
						ratios.Add ((float)paths.Count / iterations);
						shortest = fastest = longest = lengthiest = mostDanger = null;
						
						BResult2 rs = new BResult2 ();
						rs.ratio = (float)paths.Count / iterations;
						job.results.Add (rs);
					} // end of for
					
					int ratioCnt = 0;
					float sumRatio = 0.0f, averageRatio = 0.0f;
					foreach (float r in ratios) {
						sumRatio += r;
						ratioCnt ++;
					}
					averageRatio = sumRatio / ratioCnt;
					job.averageRatio = averageRatio;
					root.everything.Add (job);
//			        Debug.Log ("Moving Enemies: " + noeB + ", Rotational Cameras: " + nocB + " is: " + averageRatio);
				}
				
				XmlSerializer ser = new XmlSerializer (typeof(BResultsRoot2), new Type[] {
					typeof(BResultBatch2),
					typeof(BResult2)
				});
				ser.Serialize (stream, root);
				stream.Flush ();
				stream.Close ();
			}
		}
		
		EditorGUILayout.LabelField ("");
		EditorGUILayout.LabelField ("9. Batch Computing for Cameras");

		GUI.enabled = true;
		nocB = EditorGUILayout.IntField ("Number of cameras", nocB);
		
		if (GUILayout.Button ("Batch Computation for Cameras")) {
			
			BResultsRoot2 root = new BResultsRoot2 ();
			using (FileStream stream = new FileStream ("Ratio with respect to Cameras.xml", FileMode.Create)) {
				for (nocB = 0; nocB <= 3; noeB++) {
					BResultBatch2 job = new BResultBatch2 ();
					job.numOfCameras = nocB;
					
					norcB = nocB != 0 ? 2 * nocB - 1 : 0;
					
					for (int batchIter = 0; batchIter < 2; batchIter ++) {
														
						PCG.ClearUpCameras (cameraObjects);
						PCG.ClearUpAngles (angleObjects);
						PCG.numOfCameras = nocB;
						PCG.numOfRegionsForCameras = norcB;
								
						if (obs == null) { 
							obs = mapper.ComputeObstacles ();
							Cell[][] grid = MapperEditor.grid;
							if (grid != null) {
								for (int x = 0; x < obs.Length; x++) {
									for (int y = 0; y < obs[x].Length; y++)
										if (grid [x] [y] != null)
											obs [x] [y] = grid [x] [y];
								}
							}
							PCG.Initialize (enemyPrefab, waypointPrefab, obs);
						} else {
							PCG.ResetCamerasObs ();	
						}
								
						cameraObjects = PCG.PopulateCameras (floor).ToArray ();
				
						// Hide all region centres
						foreach (GameObject cameraObject in cameraObjects) {
							Enemy cameraScript;
							cameraScript = cameraObject.GetComponent ("Enemy") as Enemy;
							cameraScript.LineForFOV = new Color (1.0f, 1.0f, 1.0f, 0.0f);
							cameraScript.renderer.enabled = false;
						}
								
						// Calculate different voronoi regions and visualization is ready
						// PCG.vCamera.calculateVoronoiRegions (floor, PCG.vCamera.obs, PCG.numOfCameras, PCG.numOfRegionsForCameras, cameraObjects);
						PCG.vCamera.calculateVoronoiRegionsUsingFlooding (floor, PCG.numOfCameras, PCG.numOfRegionsForCameras, cameraObjects);
						PCG.vCamera.calculateBoundaries (floor);
						//drawer.cVoronoiGrid = PCG.vCamera.obs;
								
						// Select [numOfCameras] regions with maximum area
						int[] maxAreaIndexArrayForCameras = PCG.vCamera.selectMaximumRegions ();
								
						// Show region centres with [numOfCameras] regions with maximum area
						for (int i = 0; i < nocB; i++) {
							Enemy cameraRestoredScript;
							cameraRestoredScript = cameraObjects.ElementAt (maxAreaIndexArrayForCameras [i]).GetComponent ("Enemy") as Enemy;
							cameraRestoredScript.LineForFOV = new Color (1.0f, 0.3f, 0.0f, 1.0f);
							cameraObjects.ElementAt (maxAreaIndexArrayForCameras [i]).renderer.enabled = true;
							cameraObjects.ElementAt (maxAreaIndexArrayForCameras [i]).renderer.sharedMaterial.color = new Color (0.7f, 0.3f, 0.2f, 1.0f);
							cameraRestoredScript.rotationSpeed = 50;
							cameraRestoredScript.moveSpeed = 1;
						}
								
						PCG.maxAreaIndexHolderC = maxAreaIndexArrayForCameras;
						int batchIter3 = 20;
						angleObjects = PCG.RotationInVoronoiRegion (floor, PCG.vCamera.obs, batchIter3).ToArray ();
						PCG.DestroyVoronoiCentreForCameras ();
							
						// Store positions all together
						StorePositions ();
						// Precompute maps again
						fullMap = mapper.PrecomputeMaps (floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timeSamples, stepSize, ticksBehind);
						ResetAI ();
						// Compute paths
						float speed = GameObject.FindGameObjectWithTag ("AI").GetComponent<Player> ().speed;
						//Check the start and the end and get them from the editor. 
						if (start == null) {
							start = GameObject.Find ("Start");
						}
						if (end == null) {
							end = GameObject.Find ("End");	
						}
								
						paths.Clear ();
						//toggleStatus.Clear ();
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
						iterations = 20;
						for (int it = 0; it < iterations; it++) {
							nodes = rrt.Compute (startX, startY, endX, endY, attemps, speed, fullMap, smoothPath);
							if (nodes.Count > 0) {
								paths.Add (new Path (nodes));
								//toggleStatus.Add (paths.Last (), true);
								//paths.Last ().color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
							}
						}
						//heatMap = Analyzer.Compute2DHeatMap (paths, gridSize, gridSize, out maxHeatMap);
								
						//Debug.Log ("Paths found: " + paths.Count);
						//				Debug.Log ("Ratio: " + (float)paths.Count / iterations);
						ratios.Add ((float)paths.Count / iterations);
						shortest = fastest = longest = lengthiest = mostDanger = null;
						
						BResult2 rs = new BResult2 ();
						rs.ratio = (float)paths.Count / iterations;
						job.results.Add (rs);
					} // end of for
					
					int ratioCnt = 0;
					float sumRatio = 0.0f, averageRatio = 0.0f;
					foreach (float r in ratios) {
						sumRatio += r;
						ratioCnt ++;
					}
					averageRatio = sumRatio / ratioCnt;
					job.averageRatio = averageRatio;
					root.everything.Add (job);
//			        Debug.Log ("Moving Enemies: " + noeB + ", Rotational Cameras: " + nocB + " is: " + averageRatio);
				}
				
				XmlSerializer ser = new XmlSerializer (typeof(BResultsRoot2), new Type[] {
					typeof(BResultBatch2),
					typeof(BResult2)
				});
				ser.Serialize (stream, root);
				stream.Flush ();
				stream.Close ();
			}
		}
		GUI.enabled = true;
		
		#endregion
		
		#region Skeleton
		
		EditorGUILayout.LabelField ("");
		EditorGUILayout.LabelField ("10. Skeleton");

		if (randomOpEnables && !shortcutClicked) {
			moreStepsBtnEnables = true;
		}
		if (randomOpEnables && shortcutClicked) {
			moreStepsBtnEnables = false;
		}
		if (randomOpEnables && !moreStepsClicked) {
			shortcutBtnEnables = true;
		}
		if (randomOpEnables && moreStepsClicked) {
			shortcutBtnEnables = false;
		}
    
		// Build the graph step by step
		GUI.enabled = moreStepsBtnEnables;

		if (default7 == true) {
			default7 = false;
			setGraphFoldout = EditorGUILayout.Foldout (true, "Generate Graph in Steps");
		} else {
			setGraphFoldout = EditorGUILayout.Foldout (setGraphFoldout, "Generate Graph in Steps");
		}

		if (setGraphFoldout) {
			// Select boundaries
			if (GUILayout.Button ("Select Boundaries")) {
				if (obs == null) { 
					obs = mapper.ComputeObstacles ();
					Cell[][] grid = MapperEditor.grid;
					if (grid != null) {
						for (int x = 0; x < obs.Length; x++) {
							for (int y = 0; y < obs[x].Length; y++) {
								if (grid [x] [y] != null) {
									obs [x] [y] = grid [x] [y];	
								}
							}
						}
					}
					PCG.InitializeSkeleton (obs);
				}
				PCG.sBoundary.identifyObstacleContours (floor);
				drawer.sBoundaryGrid = PCG.sBoundary.obs;

				boundariesFloodingOpEnables = true;
				moreStepsClicked = true;
			}

			drawBoundaries = EditorGUILayout.Toggle ("Draw Boundaries", drawBoundaries);

			// Flooding from boundaries
			GUI.enabled = boundariesFloodingOpEnables;

			if (GUILayout.Button ("Boundaries Flooding")) {
				PCG.sBoundary.boundaryContoursFlooding (floor);
				extractRoadmapOpEnables = true;
			}

			drawRoadmaps2 = EditorGUILayout.Toggle ("Draw Roadmaps", drawRoadmaps2);

			// Extract the roadmap
			GUI.enabled = extractRoadmapOpEnables;

			if (GUILayout.Button ("Extract Roadmaps")) {
				PCG.sBoundary.extractRoadmaps (floor);
				drawer.roadmapNodesList = PCG.sBoundary.roadmapNodesList;
				PCG.sBoundary.selectSuperNodes ();
				initializeGraphOpEnables = true;
			}
			
			drawRoadmaps3 = EditorGUILayout.Toggle ("Draw Super Nodes", drawRoadmaps3);
			
			//		if (GUILayout.Button ("Select Boundaries")) {
			//			if (obs == null) { 
			//				obs = mapper.ComputeObstacles ();
			//				Cell[][] grid = MapperEditor.grid;
			//				if (grid != null) {
			//					for (int x = 0; x < obs.Length; x++) {
			//						for (int y = 0; y < obs[x].Length; y++)
			//							if (grid [x] [y] != null)
			//								obs [x] [y] = grid [x] [y];
			//					}
			//				}
			//				PCG.InitializeSkeleton (obs);
			//			}
			//			PCG.sBoundary.calculateBoundaries (floor);
			//		}
			//		
			//		if (GUILayout.Button ("Flooding")) {
			//			PCG.sBoundary.boundaryPointsFlooding (floor);
			//			drawer.sBoundaryGrid = PCG.sBoundary.obs;
			//		}
			//		
			//		drawVoronoiForBoundaries = EditorGUILayout.Toggle ("Draw Voronoi Cells", drawVoronoiForBoundaries);
			//		
			//		if (GUILayout.Button ("Retrieve Roadmap")) {
			//			PCG.sBoundary.extractContours (floor);	
			//			drawer.contoursList = PCG.sBoundary.contoursList;
			//		}
			//		
			//		drawRoadmaps = EditorGUILayout.Toggle ("Draw Roadmaps", drawRoadmaps);

			// Build graph
			GUI.enabled = initializeGraphOpEnables;

			if (GUILayout.Button ("Initialize Graph")) {
				PCG.sBoundary.initializeGraph ();
				drawer.graphNodesList = PCG.sBoundary.finalGraphNodesList;
				mergeOpEnables = true;
			}

			// Clean up redundant nodes
			GUI.enabled = mergeOpEnables;
			if (GUILayout.Button ("Merge")) {
				PCG.sBoundary.cleanUp (floor);	
				behaviorOpEnables = true;
			}
			
			drawGraph = EditorGUILayout.Toggle ("Draw Graph", drawGraph);
		}

		// Build the graph in One-click
		GUI.enabled = shortcutBtnEnables;

		if (default8 == true) {
			default8 = false;
			setShortcutFoldout = EditorGUILayout.Foldout (true, "Generate Graph with One-Click");
		} else {
			setShortcutFoldout = EditorGUILayout.Foldout (setShortcutFoldout, "Generate Graph with One-Click");
		}

		if (setShortcutFoldout) {
			if (GUILayout.Button ("Generate Graph")) {
				if (obs == null) { 
					obs = mapper.ComputeObstacles ();
					Cell[][] grid = MapperEditor.grid;
					if (grid != null) {
						for (int x = 0; x < obs.Length; x++) {
							for (int y = 0; y < obs[x].Length; y++) {
								if (grid [x] [y] != null) {
									obs [x] [y] = grid [x] [y];	
								}
							}
						}
					}
					PCG.InitializeSkeleton (obs);
				}
				
				PCG.sBoundary.identifyObstacleContours (floor);
				drawer.sBoundaryGrid = PCG.sBoundary.obs;
				
				PCG.sBoundary.boundaryContoursFlooding (floor);
				PCG.sBoundary.extractRoadmaps (floor);
				drawer.roadmapNodesList = PCG.sBoundary.roadmapNodesList;
				
				PCG.sBoundary.selectSuperNodes ();
				PCG.sBoundary.initializeGraph ();
				drawer.graphNodesList = PCG.sBoundary.finalGraphNodesList;
				
				PCG.sBoundary.cleanUp (floor);
				
				behaviorOpEnables = true;
				shortcutClicked = true;
			}

			drawGraph2 = EditorGUILayout.Toggle ("Draw Graph", drawGraph2);
		}

		GUI.enabled = randomOpEnables;

		if (GUILayout.Button ("Clear Graph")) {
			obs = null;
			PCG.sBoundary.clearGraph ();
			drawer.sBoundaryGrid = PCG.sBoundary.obs;
			behaviorOpEnables = false;
			shortcutClicked = false;
			moreStepsClicked = false;
			boundariesFloodingOpEnables = false;
			extractRoadmapOpEnables = false;
			initializeGraphOpEnables = false;
			mergeOpEnables = false;
		}

		GUI.enabled = true;
		
		#endregion
		
		#region Behaviors
		
		EditorGUILayout.LabelField ("");
		EditorGUILayout.LabelField ("11. Behaviours");

		GUI.enabled = behaviorOpEnables;

		if (default9 == true) {
			default9 = false;
			setMultipleBehavioursFoldout = EditorGUILayout.Foldout (true, "Set Behaviours");
		} else {
			setMultipleBehavioursFoldout = EditorGUILayout.Foldout (setMultipleBehavioursFoldout, "Set Behaviours");
		}
		
		if (setMultipleBehavioursFoldout) {
			enemyPrefab = (GameObject)EditorGUILayout.ObjectField ("Enemy Prefab", enemyPrefab, typeof(GameObject), false);
			numOfGuards = EditorGUILayout.IntField ("Number of guards", numOfGuards);
			iterations4 = EditorGUILayout.IntSlider ("Iterations", iterations4, 0, 10);

			if (default10 == true) {
				default10 = false;
				setPossibilitiesFoldout = EditorGUILayout.Foldout (true, "Possibility Controller");
			} else {
				setPossibilitiesFoldout = EditorGUILayout.Foldout (setPossibilitiesFoldout, "Possibility Controller");
			}

			// Possibility controller
			if (setPossibilitiesFoldout) {
				pLine = EditorGUILayout.IntSlider ("P1", pLine, 0, 100);
				pDot = EditorGUILayout.IntSlider ("P2", pDot, 0, 100);
				pSplit = EditorGUILayout.IntSlider ("P3", 100 - pZigZag, 0, 100);
				pZigZag = EditorGUILayout.IntSlider ("P4", 100 - pSplit, 0, 100);
				pPause = EditorGUILayout.IntSlider ("P5", 100 - pSwipe - pFullRotate - pNinety, 0, 100);
				pSwipe = EditorGUILayout.IntSlider ("P6", 100 - pPause - pFullRotate - pNinety, 0, 100);
				pFullRotate = EditorGUILayout.IntSlider ("P6", 100 - pPause - pSwipe - pNinety, 0, 100);
				pNinety = EditorGUILayout.IntSlider ("P7", 100 - pPause - pSwipe - pFullRotate, 0, 100);
			}

			// Populate guards with multiple behaviours along their routes
			if (GUILayout.Button ("Populate Guards")) {
				PCG.ClearBehaviours ();
				PCG.ClearUpObjects (enemypathObjects);
				PCG.numOfGuards = numOfGuards;
				enemypathObjects = PCG.PopulateGuardsWithBehaviours (enemyPrefab, waypointPrefab, floor, iterations4, pLine, pDot, pSplit, pZigZag, pPause, pSwipe, pFullRotate, pNinety).ToArray ();
				StorePositions ();
			}
			
			if (GUILayout.Button ("Clear Behaviours")) {
				PCG.ClearBehaviours ();
				PCG.ClearUpObjects (enemypathObjects);
			}
		}
		
		GUI.enabled = true;

		if (GUILayout.Button ("Reset")) {
			PCG.ClearUpObjects (enemypathObjects);

			fullMap = null;
			drawer.fullMap = fullMap;
			simulated = false;

			foreach (GameObject p in playerObjects) {
				DestroyImmediate (p);
			}
			players.Clear ();
			playing = false;
			ResetAI ();

			obs = null;
			PCG.vEnemy.obs = null;
			drawer.eVoronoiGrid = PCG.vEnemy.obs;
			PCG.vCamera.obs = null;
			drawer.cVoronoiGrid = PCG.vCamera.obs;
			
			numOfEnemies = 0;
			numOfCameras = 0;
			numOfRegionsForEnemies = 0;
			numOfRegionsForCameras = 0;
			setPathOpEnables = false;
			setRotationOpEnables = false;

			boundariesFloodingOpEnables = false;
			extractRoadmapOpEnables = false;
			initializeGraphOpEnables = false;
			mergeOpEnables = false;

			PCG.sBoundary.clearGraph ();
			drawer.sBoundaryGrid = PCG.sBoundary.obs;
			drawer.roadmapNodesList.Clear ();
			drawer.graphNodesList.Clear ();

			PCG.ClearBehaviours ();

			behaviorOpEnables = false;
			randomOpEnables = false;
			shortcutClicked = false;
			moreStepsClicked = false;
			shortcutBtnEnables = false;
			moreStepsBtnEnables = false;
		}
		
		#endregion
		
		#region Behaviours with Time-preserving
		
		EditorGUILayout.LabelField ("");
		EditorGUILayout.LabelField ("12. Behaviours with Time-preserving");
		
		GUI.enabled = behaviorOpEnables;
		
		if (default13 == true) {
			default13 = false;
			setMultipleBehavioursFoldout2 = EditorGUILayout.Foldout (true, "Set Behaviours with Time-preserving");
		} else {
			setMultipleBehavioursFoldout2 = EditorGUILayout.Foldout (setMultipleBehavioursFoldout2, "Set Behaviours with Time-preserving");
		}
		
		if (setMultipleBehavioursFoldout2) {
			enemyPrefab2 = (GameObject)EditorGUILayout.ObjectField ("Enemy Prefab", enemyPrefab2, typeof(GameObject), false);
			numOfGuards2 = EditorGUILayout.IntField ("Number of guards", numOfGuards2);
			iterations5 = EditorGUILayout.IntSlider ("Iterations", iterations5, 0, 10);
			
			if (default14 == true) {
				default14 = false;
				setPossibilitiesFoldout2 = EditorGUILayout.Foldout (true, "Possibility Controller");
			} else {
				setPossibilitiesFoldout2 = EditorGUILayout.Foldout (setPossibilitiesFoldout2, "Possibility Controller");
			}
			
			// Possibility controller
			if (setPossibilitiesFoldout2) {
				pLine2 = EditorGUILayout.IntSlider ("P1", pLine2, 0, 100);
				pDot2 = EditorGUILayout.IntSlider ("P2", pDot2, 0, 100);
				pSplit2 = EditorGUILayout.IntSlider ("P3", 100 - pZigZag2, 0, 100);
				pZigZag2 = EditorGUILayout.IntSlider ("P4", 100 - pSplit2, 0, 100);
				pPause2 = EditorGUILayout.IntSlider ("P5", 100 - pSwipe2 - pFullRotate2 - pNinety2, 0, 100);
				pSwipe2 = EditorGUILayout.IntSlider ("P6", 100 - pPause2 - pFullRotate2 - pNinety2, 0, 100);
				pFullRotate2 = EditorGUILayout.IntSlider ("P6", 100 - pPause2 - pSwipe2 - pNinety2, 0, 100);
				pNinety2 = EditorGUILayout.IntSlider ("P7", 100 - pPause2 - pSwipe2 - pFullRotate2, 0, 100);
			}
			
			// Populate guards with multiple behaviours along their routes
			if (GUILayout.Button ("Populate Guards with Time-preserving")) {
				PCG.ClearBehaviours ();
				PCG.ClearUpObjects (enemypathObjects);
				PCG.numOfGuards = numOfGuards2;
				timeSamples = (int)(PCG.InitializeGuardsWithBehavioursOverTimePreservingFromLoadedPath (enemyPrefab2, waypointPrefab, floor) / stepSize);
				enemypathObjects = PCG.PopulateGuardsWithBehavioursOverTimePreserving (timeSamples, iterations5, pLine2, pDot2, pSplit2, pZigZag2, pPause2, pSwipe2, pFullRotate2, pNinety2).ToArray ();
				StorePositionsOverTimePreserving ();
			}
			
			if (GUILayout.Button ("Clear Behaviours")) {
				PCG.ClearBehaviours ();
				PCG.ClearUpObjects (enemypathObjects);
			}
		}
		
		GUI.enabled = true;
		
		if (GUILayout.Button ("Reset")) {
			PCG.ClearUpObjects (enemypathObjects);
			
			fullMap = null;
			drawer.fullMap = fullMap;
			simulated = false;
			
			foreach (GameObject p in playerObjects) {
				DestroyImmediate (p);
			}
			players.Clear ();
			playing = false;
			ResetAIOverTimePreserving ();
			
			obs = null;
			PCG.vEnemy.obs = null;
			drawer.eVoronoiGrid = PCG.vEnemy.obs;
			PCG.vCamera.obs = null;
			drawer.cVoronoiGrid = PCG.vCamera.obs;
			
			numOfEnemies = 0;
			numOfCameras = 0;
			numOfRegionsForEnemies = 0;
			numOfRegionsForCameras = 0;
			setPathOpEnables = false;
			setRotationOpEnables = false;
			
			boundariesFloodingOpEnables = false;
			extractRoadmapOpEnables = false;
			initializeGraphOpEnables = false;
			mergeOpEnables = false;
			
			PCG.sBoundary.clearGraph ();
			drawer.sBoundaryGrid = PCG.sBoundary.obs;
			drawer.roadmapNodesList.Clear ();
			drawer.graphNodesList.Clear ();
			
			PCG.ClearBehaviours ();
			
			behaviorOpEnables = false;
			randomOpEnables = false;
			shortcutClicked = false;
			moreStepsClicked = false;
			shortcutBtnEnables = false;
			moreStepsBtnEnables = false;
		}		
		#endregion
		
		#region Flows and Cuts
	
		EditorGUILayout.LabelField ("");
		EditorGUILayout.LabelField ("12. Flows and Cuts");
		
		GUI.enabled = behaviorOpEnables;

		if (default12 == true) {
			default12 = false;
			setEnemiesFoldout2 = EditorGUILayout.Foldout (true, "Set Guards");
		} else {
			setEnemiesFoldout2 = EditorGUILayout.Foldout (setEnemiesFoldout2, "Set Guards");
		}
		
		if (setEnemiesFoldout2) {
			enemyPrefab = (GameObject)EditorGUILayout.ObjectField ("Enemy Prefab", enemyPrefab, typeof(GameObject), false);
			numOfEnemies = EditorGUILayout.IntField ("Number of enemies", numOfEnemies);
			numOfCameras = EditorGUILayout.IntField ("Number of cameras", numOfCameras);	

			// ----------------------------------Mixed Guards-----------------------------------//
			if (GUILayout.Button ("Populate Guards")) {
				// Clear up enemies and their paths in the scene
				PCG.ClearUpObjects (enemypathObjects);
				PCG.numOfEnemies = numOfEnemies;
				PCG.numOfCameras = numOfCameras;
				
				// Populate guards in the graph
				enemypathObjects = PCG.PopulateGuardsInGraph (enemyPrefab, waypointPrefab, floor).ToArray ();
				StorePositions ();
			}
		}
		
		GUI.enabled = true;
		
		if (GUILayout.Button ("Reset")) {
			PCG.ClearUpObjects (enemypathObjects);
			fullMap = null;
			drawer.fullMap = fullMap;
			simulated = false;
			foreach (GameObject p in playerObjects) {
				DestroyImmediate (p);
			}
			players.Clear ();
			playing = false;
			ResetAI ();
			obs = null;
			PCG.vEnemy.obs = null;
			drawer.eVoronoiGrid = PCG.vEnemy.obs;
			PCG.vCamera.obs = null;
			drawer.cVoronoiGrid = PCG.vCamera.obs;

			numOfEnemies = 0;
			numOfCameras = 0;
			numOfRegionsForEnemies = 0;
			numOfRegionsForCameras = 0;
			setPathOpEnables = false;
			setRotationOpEnables = false;
			//randomOpEnables = false;
			
			boundariesFloodingOpEnables = false;
			extractRoadmapOpEnables = false;
			initializeGraphOpEnables = false;
			mergeOpEnables = false;
			behaviorOpEnables = false;
		}
		
		GUI.enabled = true;
		
		#endregion
		
		#region Rhythm
		
		EditorGUILayout.LabelField ("");
		EditorGUILayout.LabelField ("13. Rhythm");
		
		GUI.enabled = behaviorOpEnables;
		
		if (default11 == true) {
			default11 = false;
			setRhythmsFoldout = EditorGUILayout.Foldout (true, "Set Rhythms");
		} else {
			setRhythmsFoldout = EditorGUILayout.Foldout (setRhythmsFoldout, "Set Rhythms");
		}
		
		if (setRhythmsFoldout) {
			enemyPrefab = (GameObject)EditorGUILayout.ObjectField ("Enemy Prefab", enemyPrefab, typeof(GameObject), false);
			numOfGuards = EditorGUILayout.IntField ("Number of guards", numOfGuards);
			
			if (GUILayout.Button ("Populate Guards")) {
				PCG.ClearUpObjects (enemypathObjects);
				// numofene?
				PCG.numOfGuards = numOfGuards;
				PCG.PopulateGuardsWithRhythms (enemyPrefab, waypointPrefab, floor);
				// Run all the finite state machine instances assigned to each guard
				FSMController.RunFSM (timeSamples);
			}
			
			if (GUILayout.Button ("Prepare for Simulation")) {
				StorePositionsOverRhythm ();
				fullMap = mapper.PrecomputeMapsOverRhythm (floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timeSamples, stepSize, ticksBehind);
				drawer.fullMap = fullMap;
				float maxSeenGrid;
				drawer.seenNeverSeen = Analyzer.ComputeSeenValuesGrid (fullMap, out maxSeenGrid);
				drawer.seenNeverSeenMax = maxSeenGrid;
				drawer.tileSize = SpaceState.TileSize;
				drawer.zero.Set (floor.collider.bounds.min.x, floor.collider.bounds.min.z);
				
				ResetAI ();
			}
			
//			EditorGUILayout.LabelField ("");
//			EditorGUILayout.LabelField ("Rythm Indicator");
		}
		
		#endregion

		#region BatchComputingForBehaviours
		
		GUI.enabled = true;
		
		EditorGUILayout.LabelField ("");
		EditorGUILayout.LabelField ("14. Batch Computing For Behaviours");
		
		nogB = EditorGUILayout.IntField ("Number of guards", nogB);
		noiB = EditorGUILayout.IntField ("Number of iterations", noiB);
		
		if (GUILayout.Button ("Batch Computation For Guards and Paths")) {
			
			// Retrieve the graph for testing level
			if (obs == null) { 
				obs = mapper.ComputeObstacles ();
				Cell[][] grid = MapperEditor.grid;
				if (grid != null) {
					for (int x = 0; x < obs.Length; x++) {
						for (int y = 0; y < obs[x].Length; y++) {
							if (grid [x] [y] != null) {
								obs [x] [y] = grid [x] [y];	
							}
						}
					}
				}
				PCG.InitializeSkeleton (obs);
			}


			PCG.sBoundary.identifyObstacleContours (floor);			
			PCG.sBoundary.boundaryContoursFlooding (floor);
			PCG.sBoundary.extractRoadmaps (floor);			
			PCG.sBoundary.selectSuperNodes ();
			PCG.sBoundary.initializeGraph ();			
			PCG.sBoundary.cleanUp (floor);
			
			BResultsRoot root = new BResultsRoot ();
			using (FileStream stream = new FileStream ("Ratio with respect to Guards and Iterations.xml", FileMode.Create)) {
				// Guards number varying from 1 to 8
				for (int nog = 1; nog <= nogB; nog++) {
					// Iterations number varying from 0 to 3
					for (int noi = 0; noi <= noiB; noi++) {
						BResultBatch job = new BResultBatch ();
						job.numOfGuards = nog;
						job.numOfIterations = noi;
						// For each test we want 20 trials
						for (int batchIter = 0; batchIter < 30; batchIter ++) {					
							// Clear up
							PCG.ClearBehaviours ();
							PCG.numOfGuards = nog;
							PCG.ClearUpObjects (enemypathObjects);
						
							// Populate guards on the graph
							enemypathObjects = PCG.PopulateGuardsWithBehavioursAndSaveToFile (enemyPrefab, waypointPrefab, floor, noi, pLine, pDot, pSplit, pZigZag, pPause, pSwipe, pFullRotate, pNinety).ToArray ();
							StorePositions ();
					
							// Precompute maps again
							fullMap = mapper.PrecomputeMaps (floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timeSamples, stepSize, ticksBehind);
							ResetAI ();
					
							// Compute paths
							float speed = GameObject.FindGameObjectWithTag ("AI").GetComponent<Player> ().speed;
					
							//Check the start and the end and get them from the editor. 
							if (start == null) {
								start = GameObject.Find ("Start");
							}
							if (end == null) {
								end = GameObject.Find ("End");	
							}
					
							paths.Clear ();
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
							iterations = 1;
							for (int it = 0; it < iterations; it++) {
								nodes = rrt.Compute (startX, startY, endX, endY, attemps, speed, fullMap, smoothPath);
								if (nodes.Count > 0) {
									paths.Add (new Path (nodes));
								}
							}
							
							Analyzer.ComputePathsDangerValues (paths, SpaceState.Enemies, floor.collider.bounds.min, SpaceState.TileSize.x, SpaceState.TileSize.y, fullMap, drawer.seenNeverSeen, drawer.seenNeverSeenMax);
							
							BResult rs = new BResult ();
							rs.ratio = (float)paths.Count / iterations;
							foreach (Path path in paths) {
								rs.listOfDanger3.Add (path.danger3);	
							}
							job.results.Add (rs);
							ratios.Add ((float)paths.Count / iterations);

							shortest = fastest = longest = lengthiest = mostDanger = null;
						} // end of for
				
						int ratioCnt = 0;
						float sumRatio = 0.0f, averageRatio = 0.0f;
						foreach (float r in ratios) {
							sumRatio += r;
							ratioCnt ++;
						}
						averageRatio = sumRatio / ratioCnt;
						job.averageRatio = averageRatio;
						root.everything.Add (job);
					}
				}
				
				XmlSerializer ser = new XmlSerializer (typeof(BResultsRoot), new Type[] {
								typeof(BResultBatch),
								typeof(BResult)
				});
				ser.Serialize (stream, root);
				stream.Flush ();
				stream.Close ();
			}
		}
		
		if (GUILayout.Button ("Exported Longest Path")) {
			PathSelector ps = new PathSelector ("_2_guards_0_iterations_saved_path.txt");
			ps.selectLongestPath ();
			ps.saveLongestPathsToFile ();
		}
		
		if (GUILayout.Button ("Batch Computation For Single Behaviour")) {
			BResultsRoot root = new BResultsRoot ();
			using (FileStream stream = new FileStream ("Ratio with respect to single behaviour.xml", FileMode.Create)) {
				// Iterations number varying from 0 to 4
				for (int noi = 0; noi <= noiB; noi++) {
					BResultBatch job = new BResultBatch ();
					// Add multiple parameters here
					job.numOfIterations = noi;
					// For each test we want 20 trials
					for (int batchIter = 0; batchIter < 10; batchIter ++) {					
						// Clear up
						PCG.ClearBehaviours ();
						PCG.numOfGuards = 2;
						PCG.ClearUpObjects (enemypathObjects);
						
						// Populate guards on the graph
						enemypathObjects = PCG.PopulateGuardsWithBehavioursFromLoadedPath (enemyPrefab, waypointPrefab, floor, noi, 100, 100, pSplit, pZigZag, 100, 0, 0, 0).ToArray ();
						StorePositions ();
					
						// Precompute maps again
						fullMap = mapper.PrecomputeMaps (floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timeSamples, stepSize, ticksBehind);
						ResetAI ();
					
						// Compute paths
						float speed = GameObject.FindGameObjectWithTag ("AI").GetComponent<Player> ().speed;
					
						//Check the start and the end and get them from the editor. 
						if (start == null) {
							start = GameObject.Find ("Start");
						}
						if (end == null) {
							end = GameObject.Find ("End");	
						}
					
						paths.Clear ();
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
						iterations = 10;
						for (int it = 0; it < iterations; it++) {
							nodes = rrt.Compute (startX, startY, endX, endY, attemps, speed, fullMap, smoothPath);
							if (nodes.Count > 0) {
								paths.Add (new Path (nodes));
							}
						}
						BResult rs = new BResult ();
						rs.ratio = (float)paths.Count / iterations;
						job.results.Add (rs);
						ratios.Add ((float)paths.Count / iterations);

						shortest = fastest = longest = lengthiest = mostDanger = null;
					} // end of for
				
					int ratioCnt = 0;
					float sumRatio = 0.0f, averageRatio = 0.0f;
					foreach (float r in ratios) {
						sumRatio += r;
						ratioCnt ++;
					}
					averageRatio = sumRatio / ratioCnt;
					job.averageRatio = averageRatio;
					root.everything.Add (job);
				}
				
				XmlSerializer ser = new XmlSerializer (typeof(BResultsRoot), new Type[] {
								typeof(BResultBatch),
								typeof(BResult)
				});
				ser.Serialize (stream, root);
				stream.Flush ();
				stream.Close ();
			}			
		}
		
		if (GUILayout.Button ("Single Behaviour Over Time Preserving")) {
			BResultsRoot root = new BResultsRoot ();
			using (FileStream stream = new FileStream ("Ratio with respect to single behaviour over time preserving.xml", FileMode.Create)) {
				// Iterations number varying from 0 to 4
				for (int noi = 0; noi <= noiB; noi++) {
					BResultBatch job = new BResultBatch ();
					// Add multiple parameters here
					job.numOfIterations = noi;
					// For each test we want 20 trials
					for (int batchIter = 0; batchIter < 20; batchIter ++) {					
						// Clear up
						PCG.ClearBehaviours ();
						PCG.numOfGuards = 4;
						PCG.ClearUpObjects (enemypathObjects);
						timeSamples = (int)(PCG.InitializeGuardsWithBehavioursOverTimePreservingFromLoadedPath (enemyPrefab2, waypointPrefab, floor) / stepSize);
						// Populate guards on the graph
						enemypathObjects = PCG.PopulateGuardsWithBehavioursOverTimePreservingFromLoadedPath (timeSamples, noi, 100, 100, pSplit, pZigZag, 100, 0, 0, 0).ToArray ();
						StorePositionsOverTimePreserving ();
					
						// Precompute maps again
						fullMap = mapper.PrecomputeMapsOverTimePreserving (floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timeSamples, stepSize, ticksBehind);
						ResetAIOverTimePreserving ();
					
						// Compute paths
						float speed = GameObject.FindGameObjectWithTag ("AI").GetComponent<Player> ().speed;
					
						//Check the start and the end and get them from the editor. 
						if (start == null) {
							start = GameObject.Find ("Start");
						}
						if (end == null) {
							end = GameObject.Find ("End");	
						}
					
						paths.Clear ();
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
						iterations = 10;
						for (int it = 0; it < iterations; it++) {
							nodes = rrt.Compute (startX, startY, endX, endY, attemps, speed, fullMap, smoothPath);
							if (nodes.Count > 0) {
								paths.Add (new Path (nodes));
							}
						}
						BResult rs = new BResult ();
						rs.ratio = (float)paths.Count / iterations;
						job.results.Add (rs);
						ratios.Add ((float)paths.Count / iterations);

						shortest = fastest = longest = lengthiest = mostDanger = null;
					} // end of for
				
					int ratioCnt = 0;
					float sumRatio = 0.0f, averageRatio = 0.0f;
					foreach (float r in ratios) {
						sumRatio += r;
						ratioCnt ++;
					}
					averageRatio = sumRatio / ratioCnt;
					job.averageRatio = averageRatio;
					root.everything.Add (job);
				}
				
				XmlSerializer ser = new XmlSerializer (typeof(BResultsRoot), new Type[] {
								typeof(BResultBatch),
								typeof(BResult)
				});
				ser.Serialize (stream, root);
				stream.Flush ();
				stream.Close ();
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
					playerObjects.Add (player);
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
			drawer.drawVoronoiForEnemies = drawVoronoiForEnemies;
			drawer.drawVoronoiForCameras = drawVoronoiForCameras;
			drawer.drawVoronoiForBoundaries = drawVoronoiForBoundaries;
			drawer.drawBoundaries = drawBoundaries;
			drawer.drawRoadmaps = drawRoadmaps;
			drawer.drawRoadmaps2 = drawRoadmaps2;
			drawer.drawRoadmaps3 = drawRoadmaps3;
			drawer.drawGraph = drawGraph;
			drawer.drawGraph2 = drawGraph2;
		}
		
		if (fullMap != null && lastTime != timeSlice) {
			lastTime = timeSlice;
			UpdatePositions (timeSlice, mapper);
		}
		
		SceneView.RepaintAll ();
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
				
				// Do something else
				
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
	
	public void ResetAIOverTimePreserving ()
	{
		GameObject[] objs = GameObject.FindGameObjectsWithTag ("AI") as GameObject[];
		foreach (GameObject ob in objs)
			ob.GetComponent<Player> ().ResetSimulation ();
		
		objs = GameObject.FindGameObjectsWithTag ("Enemy") as GameObject[];
		foreach (GameObject ob in objs) 
			ob.GetComponent<Enemy2> ().ResetSimulation ();
	}
	
	// Updates everyone's position to the current timeslice
	public void UpdatePositions (int t, Mapper mapper)
	{
		for (int i = 0; i < SpaceState.Enemies2.Length; i++) {
			if (SpaceState.Enemies2 [i] == null)
				continue;
			
			Vector3 pos = SpaceState.Enemies2 [i].positions [t];
			if (drawMoveUnits)
				pos.y = t;
			SpaceState.Enemies2 [i].transform.position = pos;
			SpaceState.Enemies2 [i].transform.rotation = SpaceState.Enemies2 [i].rotations [t];
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
	
	public void StorePositionsOverRhythm ()
	{
		GameObject[] objs = GameObject.FindGameObjectsWithTag ("Enemy") as GameObject[];
		for (int i = 0; i < objs.Length; i++) {
			objs [i].GetComponent<Enemy> ().SetInitialPositionOverRhythm (0);
		}
		objs = GameObject.FindGameObjectsWithTag ("AI") as GameObject[];
		for (int i = 0; i < objs.Length; i++) {
			objs [i].GetComponent<Player> ().SetInitialPosition ();
		}	
	}
	
	public void StorePositionsOverTimePreserving ()
	{
		GameObject[] objs = GameObject.FindGameObjectsWithTag ("Enemy") as GameObject[];
		for (int i = 0; i < objs.Length; i++) {
			objs [i].GetComponent<Enemy2> ().SetInitialPosition ();
		}
		objs = GameObject.FindGameObjectsWithTag ("AI") as GameObject[];
		for (int i = 0; i < objs.Length; i++) {
			objs [i].GetComponent<Player> ().SetInitialPosition ();
		}
	}
}
