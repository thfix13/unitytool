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
		private static bool drawMap = false, drawNeverSeen = false, drawHeatMap = false, drawHeatMap3d = false, drawDeathHeatMap = false, drawDeathHeatMap3d = false, drawCombatHeatMap = false, drawPath = true, smoothPath = true, drawFoVOnly = false, drawCombatLines = false, simulateCombat = false, limitImportablePathsToCurLevel = true;
		private static float stepSize = 1 / 10f, crazySeconds = 5f, playerDPS = 10;
		private static int randomSeed = -1;
		private static int nbPaths = -1, nbBatch = -1,fileloaded = -1;
		public static bool only2DTriangulation = true; 
		// Clustering
		private static String[] distMetrics = new String[] { "Frechet", "Area (Triangulation)", "Area (Interpolation) 3D", "Hausdorff" };
		private static String[] clustAlgs = new String[] { "KMeans++", "DBScan" };
		private static String[] clustAlgsShort = new String[] { "KM", "DBS" };
		private static String[] distMetricsShort = new String[] { "FRE", "TRI", "INTPOL", "H" };
		private static String[] dimensions = new String[] { "X", "Y", "Time", "Danger", "LOS", "Near Miss", "Health" };
		private static String[] dimensionsShort = new String[] { "X", "Y", "T", "DNG", "LOS", "NM", "H" };
		public static bool[] dimensionEnabled = new bool[] { true, true, false, false, false, false, false };
		public static Color[] colors = new Color[] { Color.blue, Color.green, Color.magenta, Color.red, Color.yellow, Color.black, Color.cyan, new Color32(164, 211, 238, 255), new Color32(189, 252, 201, 255), new Color32(255, 165, 0, 255), new Color32(255, 182, 193, 255), new Color32(0, 206, 209, 255), new Color32(102, 205, 170, 255), new Color32(128, 128, 0, 255), new Color32(210, 180, 140, 255), new Color32(160, 82, 45, 255), new Color32(197, 193, 170, 255), Color.grey };
		private static String[] colorStrings = new String[] { "Blue", "Green", "Magenta", "Red", "Yellow", "Black", "Cyan", "Light Sky Blue", "Mint", "Orange", "Light Pink", "Turquoise", "Aquamarine", "Olive", "Tan", "Brown", "Bright Grey", "Grey"};
		public static int clustAlg = 1;
		private static int numClusters = 4, distMetric = 0, chosenFileIndex = -1, currentColor = 0, numPasses = 1, rdpTolerance = 4, maxClusters = 4, minPathsForCluster = 3;
		private static float dbsScanEps = 15.0f;
		private static List<Path> clusterCentroids = new List<Path>(), origPaths = new List<Path>();
		private static bool[] showPaths = new bool[colors.Count()];
		private static bool autoSavePaths = true, discardHighDangerPaths = true, drawHeatMapColored = false, useColors = false, showNoise = false;
		public static bool altCentroidComp = false, useScalable = false;
	//	public int numberLines = 20; 
	//	public float interpolationValue = 0.0f;
	//	public float interpolationValueCheck = 0.0f; 
		public static bool[] drawHeatMapColors = new bool[MapperWindowEditor.colors.Count()];
		LevelRepresentation rep;

		// Computed parameters
		private static int[,] heatMap, deathHeatMap, combatHeatMap;
		private static int[][,] heatMap3d, deathHeatMap3d, heatMapColored;
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
			resetClusteringData();
			origPaths = new List<Path>();
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
			
			if (GUILayout.Button ("Precompute Maps"))
			{
				precomputeMaps();
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
				useColors = false;
				
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
				
				
				for (int count = 0; count < paths.Count(); count ++)
				{
					paths[count].name = count.ToString();
				}
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
			
			DirectoryInfo pathsDir = new DirectoryInfo(".");
			FileInfo[] pathsInfo = pathsDir.GetFiles("*.xml");
			
			List<String> goodPathFiles = new List<String>();
			for (int count = 0; count < pathsInfo.Count(); count ++)
			{ // find clustering results file to display in load menu.
				String[] temp = pathsInfo[count].Name.Split(new Char[]{'_'});
				if (limitImportablePathsToCurLevel && temp[0] != nameFile)
					continue;
				
				goodPathFiles.Add(pathsInfo[count].Name);
			}

			 

			nbPaths = (int)EditorGUILayout.IntField("nbPaths",nbPaths);


			String[] pathFileNames = goodPathFiles.ToArray();
			chosenFileIndex = EditorGUILayout.Popup("Import paths", chosenFileIndex, pathFileNames);
			if (chosenFileIndex != -1)
			{
				
				fileloaded = chosenFileIndex; 

				useColors = false;
				paths.Clear ();
				ClearPathsRepresentation ();
				resetClusteringData();
				origPaths = new List<Path>();
				
				List<Path> pathsImported = PathBulk.LoadPathsFromFile(pathFileNames[chosenFileIndex]);

				List<Path> tempRandomPaths = new List<Path>(); 

				if(nbPaths != -1)
				{
					for(int i =0; i<nbPaths;i++)
					{
						int posRandom = UnityEngine.Random.Range(0,pathsImported.Count); 
						if(!tempRandomPaths.Contains(pathsImported[posRandom]))
							tempRandomPaths.Add(pathsImported[posRandom]);
					}

					pathsImported = tempRandomPaths;
				}
				foreach (Path p in pathsImported) {
					if (p.points.Last().playerhp <= 0) {
						deaths.Add(p);
					} else {
						p.name = "Imported " + (++imported);
						p.color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), 
							                     UnityEngine.Random.Range (0.0f, 1.0f), 
							                     UnityEngine.Random.Range (0.0f, 1.0f));
						toggleStatus.Add (p, true);
					}
					paths.Add(p);
				}

				precomputeMaps();

				ComputeHeatMap (paths, deaths);




				SetupArrangedPaths (paths);
				
				for (int count = 0; count < paths.Count(); count ++)
				{
					paths[count].name = count.ToString();
				}

				foreach (Path p in paths)
				{ // check if x != xD, etc.
					foreach (Node n in p.points)
					{
						if (n.x != 0 && n.xD == 0) n.xD = n.x;
						if (n.y != 0 && n.yD == 0) n.yD = n.y;
						if (n.t != 0 && n.tD == 0) n.tD = n.t;
					}
				}
				
				//Store the paths in the data holder for 3d debug view
				GameObject dataCurve = GameObject.Find("DataPath");
					
				if (dataCurve != null)
				{
					Debug.Log("hello");
					dataCurve.GetComponent<PathsHolder>().paths = paths;
					
				}


		//		rep = new LevelRepresentation();
		//		LevelRepresentation.tileSize = SpaceState.Editor.tileSize;
		//		Vector2 blah = new Vector2(floor.collider.bounds.min.x, floor.collider.bounds.min.z);
		//		LevelRepresentation.zero = blah;
		//		rep.loadStealthLevel();
				
		/*		paths.Clear();
				ClearPathsRepresentation();
				foreach (ClusteringSpace.Line l in rep.obstacles)
				{
					Path p = new Path();
					p.points.Add(new Node(l.end.x, l.end.y, 1));
					p.points.Add(new Node(l.start.x, l.start.y, 1));
					for (int i = p.points.Count - 1; i > 0; i--) {
						p.points [i].parent = p.points [i - 1];
					}
					paths.Add(p);
					toggleStatus.Add(p, true);
				} */
				
				chosenFileIndex = -1;
			}
			
			limitImportablePathsToCurLevel = EditorGUILayout.Toggle ("Hide paths for other levels", limitImportablePathsToCurLevel);
			
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
			
			if (GUILayout.Button ("Compute node danger values"))
			{
				Analyzer.ComputeNodeDangerValues (paths, SpaceState.Editor.enemies, floor.collider.bounds.min, SpaceState.Editor.tileSize.x, SpaceState.Editor.tileSize.y, original, drawer.seenNeverSeen, drawer.seenNeverSeenMax);
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
	//		EditorGUILayout.LabelField ("");

/*			numberLines =  EditorGUILayout.IntField("number of lines", numberLines); 

			if (GUILayout.Button ("Draw lines for 2 random paths"))
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
				
				//clear the previous line
				GameObject lineHolder = GameObject.Find("Lines"); 
				if(lineHolder)
					DestroyImmediate(lineHolder); 

				lineHolder = new GameObject(); 
				lineHolder.name = "Lines";

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
			if (GUILayout.Button("Triangulate 2 random curves"))
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

			EditorGUILayout.LabelField ("");*/

			clustAlg = EditorGUILayout.Popup("Clustering alg: ", clustAlg, clustAlgs);

			if (clustAlg == 0) //kmeans
			{
				numClusters = EditorGUILayout.IntSlider ("Number of clusters", numClusters, 1, 7);
				numPasses = EditorGUILayout.IntSlider ("Number of passes", numPasses, 1, 500);
				useScalable = EditorGUILayout.Toggle("Use scalable version", useScalable);
			}
			else if (clustAlg == 1)
			{
				maxClusters = EditorGUILayout.IntSlider("Max clusters", maxClusters, 1, colors.Count()-1);
				dbsScanEps = EditorGUILayout.FloatField("Eps value", dbsScanEps);
				minPathsForCluster = EditorGUILayout.IntSlider("Min paths for cluster", minPathsForCluster, 1, 15);
				showNoise = EditorGUILayout.Toggle("Show noise", showNoise);
			}
			int prevMetric = distMetric;
			if ( (distMetric == 0 || distMetric == 3) && dimensionEnabled[3])
				discardHighDangerPaths = EditorGUILayout.Toggle("Discard high danger paths", discardHighDangerPaths);
			distMetric = EditorGUILayout.Popup("Dist metric:", distMetric, distMetrics);
			
			if (discardHighDangerPaths)
			{
				for (int c = 0; c < paths.Count(); c ++)
				{
					if (paths[c].danger3 > 0.0014)
					{
						paths.Remove(paths[c]);
					}
				}

				origPaths = new List<Path>();
				
				for (int count = 0; count < paths.Count(); count ++)
				{
					paths[count].name = count.ToString();
				}
			}
			
			if (prevMetric != distMetric)
			{
				resetClusteringData();
			}
			if (distMetric == 0 || distMetric == 3)
			{
				for (int count = 0; count < dimensions.Count(); count ++)
				{
					bool prevValue = dimensionEnabled[count];
					dimensionEnabled[count] = EditorGUILayout.Toggle(dimensions[count], dimensionEnabled[count]);
					if (prevValue != dimensionEnabled[count])
					{
						resetClusteringData();
					}
				}
			}
			if (distMetric == 1)
			{
				//This is used directly trhough the static reference 
				//by AreaDist.cs
				only2DTriangulation = EditorGUILayout.Toggle("Only 2d triangulation", only2DTriangulation);
			}
			nbBatch = (int)EditorGUILayout.IntField("nbBatches",nbBatch);

			String title = "Cluster on path similarity"; 

			if (nbBatch>0)
				title = nbBatch + " batchs cluster on path similarity";

			if (GUILayout.Button (title))
			{

				int counter = 0; 
				do //For batch computation
				{
					Debug.Log("Pass nb " + counter);
					resetClusteringData();
					//Reload the data  

					if(nbBatch>0)
					{
						if(fileloaded == -1)
						{
							Debug.Log("no previous file loaded, please load some traces if you want to to do batches.");

							return;
						}
						chosenFileIndex = fileloaded;

						useColors = false;
						paths.Clear ();
						ClearPathsRepresentation ();
						resetClusteringData();
						origPaths = new List<Path>();
						
						List<Path> pathsImported = PathBulk.LoadPathsFromFile(pathFileNames[chosenFileIndex]);

						List<Path> tempRandomPaths = new List<Path>(); 

						if(nbPaths != -1)
						{
							for(int i =0; i<nbPaths;i++)
							{
								int posRandom = UnityEngine.Random.Range(0,pathsImported.Count); 
								if(!tempRandomPaths.Contains(pathsImported[posRandom]))
									tempRandomPaths.Add(pathsImported[posRandom]);
							}

							pathsImported = tempRandomPaths;
						}
						foreach (Path p in pathsImported) {
							if (p.points.Last().playerhp <= 0) {
								deaths.Add(p);
							} else {
								p.name = "Imported " + (++imported);
								p.color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), 
									                     UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
								toggleStatus.Add (p, true);
							}
							paths.Add(p);
						}

						precomputeMaps();

						ComputeHeatMap (paths, deaths);
						SetupArrangedPaths (paths);
						
						for (int count = 0; count < paths.Count(); count ++)
						{
							paths[count].name = count.ToString();
						}

						foreach (Path p in paths)
						{ // check if x != xD, etc.
							foreach (Node n in p.points)
							{
								if (n.x != 0 && n.xD == 0) n.xD = n.x;
								if (n.y != 0 && n.yD == 0) n.yD = n.y;
								if (n.t != 0 && n.tD == 0) n.tD = n.t;
							}
						}

						chosenFileIndex = -1; 
					}



					if (paths.Count < numClusters)
					{
						Debug.Log("You have less paths than you have desired clusters - either compute more paths or decrease cluster amount.");
						return;
					}

					int numSelectedDimensions = 0;
					for (int dim = 0; dim < dimensionEnabled.Count(); dim ++)
					{
						if (dimensionEnabled[dim])
						{
							numSelectedDimensions ++;
						}
					}
				
					if (numSelectedDimensions < 1 && (distMetric == 0 || distMetric == 3))
					{
						Debug.Log("You must first select at least one dimension.");
						return;
					}
					
					if (origPaths.Count() == 0)
					{
						origPaths = new List<Path>();
						foreach (Path p in paths)
						{
							origPaths.Add(new Path(p));
						}
						Clustering.initWithPaths(paths, (numSelectedDimensions > 1) ? true : false);
					}
					
					paths.Clear();
					paths = new List<Path>();
					foreach (Path p in origPaths)
					{
						paths.Add(new Path(p));
					}
									
					useColors = true;
					
					// scan the paths and remove any avg. centroid paths that were computed
					foreach (Path p in paths)
					{
						if (p.name == "AvgCentroid")
						{
							Debug.Log("Removing avg. centroid");
							paths.Remove(p);
						}
					}

					KMeans.clustTime = new System.Diagnostics.Stopwatch();
					KMeans.distTime = new System.Diagnostics.Stopwatch();

					System.Diagnostics.Stopwatch clustTime = new System.Diagnostics.Stopwatch();
					clustTime.Start();

					double clustVal = 0.0;

					if (clustAlg == 0) // kmeans++
					{
						if (paths.Count > 99)
						{
							Debug.Log("Paths: " + paths.Count());
							List<PathCollection> clusters = KMeans.DoKMeans(paths, paths.Count/20, distMetric, numPasses);
							clustVal = KMeans.clustVal;
					
							List<Path> tempCentroids = new List<Path>();
							foreach(PathCollection pc in clusters)
							{
						//		if (altCentroidComp) tempCentroids.Add(pc.getCenterDistPath());
						//		else
									tempCentroids.Add(pc.Centroid);
							}
					
					//		if (altCentroidComp) altCentroidComp = false;
					
							double[] weights = new double[paths.Count()];
							for(int i = 0; i < paths.Count(); i ++) { weights[i] = 1.0; }
							List<PathCollection> newClusters = KMeans.DoKMeans(tempCentroids, numClusters, distMetric, numPasses, weights);
					
							paths.Clear ();
							ClearPathsRepresentation ();

							clusterCentroids.Clear();
							int cluster = 0;
							foreach(PathCollection pc in newClusters)
							{
								Path centroid = pc.Centroid;
								centroid.color = colors[cluster];
								centroid.color.a = 0.5f;
								pc.Add(centroid);
								clusterCentroids.Add(centroid);
								if (!paths.Contains(centroid))
								{
									paths.Add(centroid);
									toggleStatus.Add(paths.Last(), true);
								}
									
								cluster ++;
							}

							for (int c = 0; c < newClusters.Count; c ++)
							{
								for (int c2 = 0; c2 < tempCentroids.Count; c2 ++)
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
							Debug.Log("<99 paths");
							List<PathCollection> clusters = KMeans.DoKMeans(paths, numClusters, distMetric, numPasses);
							for (int i = 0; i < clusters.Count(); i ++)
							{
								Debug.Log("Cluster #" + i + " has " + clusters[i].Count() + " paths");
							}
							clustVal = KMeans.clustVal;
						
							clusterCentroids.Clear();
							foreach(PathCollection pc in clusters)
							{
		//						clusterCentroids.Add(pc.Centroid);
						//		if (altCentroidComp) clusterCentroids.Add(pc.getCenterDistPath());
						//		else
									clusterCentroids.Add(pc.Centroid);
							}
									
							paths.Clear ();
							deaths.Clear ();
							ClearPathsRepresentation ();

							for(int c = 0; c < clusters.Count; c ++)
							{
								foreach(Path path in clusters[c])
								{
									//Debug.Log(c);
									path.color = colors[c];
									if (path.Equals(clusterCentroids[c]))
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
					else if (clustAlg == 1)
					{ // dbscan
						List<PathCollection> clusters = DBSCAN.DoDBSCAN(paths, distMetric, dbsScanEps, minPathsForCluster);
						
						paths.Clear ();
						deaths.Clear ();
						ClearPathsRepresentation ();
						
						Debug.Log("Num clusters returned from DBSCAN: " + clusters.Count);
						numClusters = clusters.Count;
						
						if (clusters.Count > maxClusters)
						{ // too many clusters. reduce using kmeans.
							List<Path> clusterCentroids = new List<Path>();
							foreach(PathCollection c in clusters)
							{
								c.UpdateCentroid();
								clusterCentroids.Add(c.Centroid);
							}
							
							double[] weights = new double[paths.Count()];
							for(int i = 0; i < paths.Count(); i ++) { weights[i] = 1.0; }
							List<PathCollection> newClusters = KMeans.DoKMeans(clusterCentroids, maxClusters, distMetric, 5, weights);
							
							for (int c = 0; c < newClusters.Count; c ++)
							{
								for (int c2 = 0; c2 < clusterCentroids.Count; c2 ++)
								{
									if (newClusters[c].Contains(clusterCentroids[c2]))
									{ // then all paths of clusters[c2] list should be of the same color!
										foreach (Path path in clusters[c2])
										{
											path.color = colors[c];
											if (path.Equals(clusterCentroids[c2]))
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
							for(int c = 0; c < clusters.Count; c ++)
							{
								foreach(Path path in clusters[c])
								{
									path.color = colors[c];
									if (!paths.Contains(path))
									{
										paths.Add(path);
										toggleStatus.Add(paths.Last (), true);
									}
								}
							}
						}
						
						int noisy = 0;
						foreach (Path path in origPaths)
						{
							if (!paths.Contains(path))
							{
								path.clusterID = Path.NOISE;
								noisy ++;
								path.color = colors[colors.Count() - 1];
								path.color.a = 1;
								
								paths.Add(path);
								toggleStatus.Add(paths.Last(), true);
							}
						}
						Debug.Log("Noisy paths: " + noisy);
					}
					
					clustTime.Stop();

					TimeSpan totalTime = clustTime.Elapsed;
					Debug.Log ("Total: " + totalTime + ", clust val: " + clustVal);
					
					if (autoSavePaths)
					{
						String currentTime = System.DateTime.Now.ToString("s");
						currentTime = currentTime.Replace(':', '-');
						String totalTimeStr = new DateTime(Math.Abs(totalTime.Ticks)).ToString("HHmmss");
						
						String distMetricStr = distMetricsShort[distMetric];
						if (distMetric == 0 || distMetric == 3)
						{ // frechet, hausdorff
							distMetricStr += "-";
							for (int dim = 0; dim < dimensionEnabled.Count(); dim ++)
							{
								if (dimensionEnabled[dim])
								{
									distMetricStr += dimensionsShort[dim];
								}
							}
						}
						String NameFile = "clusteringdata/" + nameFile + "_" + 
							clustAlgsShort[clustAlg] + "-" + numClusters + "c-" + distMetricStr + 
							"-" + paths.Count() + "p-" + (int)clustVal + "v-" + totalTimeStr + "t@" + 
							currentTime;
						//if batch please add the batch
						if(nbBatch>0)
							NameFile+= "_"+(counter+1)+"_of_"+nbBatch;

						NameFile+=".xml";
						PathBulk.SavePathsToFile (NameFile, paths);
					}
					
					for (int color = 0; color < colors.Count(); color ++)
					{
						showPaths[color] = (color < numClusters) ? true : false;
					}

					counter ++; //batch counter
					
				}while(counter < nbBatch);

			}


			if (clustAlg == 1)
			{
				foreach (Path p in paths)
				{
					if (p.clusterID == Path.NOISE)
					{
						p.color.a = (showNoise ? 1 : 0);
					}
				}
			}
			
			if (GUILayout.Button("Generate graph (Mac only)"))
			{
				ClustersRoot root = new ClustersRoot ();
				for (int n = 0; n < clusterCentroids.Count(); n ++)
				{
					MetricsRoot cluster = new MetricsRoot ();
					cluster.number = n + "";
					
					foreach (Path p in paths)
					{
						if (p.color.r == clusterCentroids[n].color.r
							&& p.color.g == clusterCentroids[n].color.g
							&& p.color.b == clusterCentroids[n].color.b)
						{
							cluster.everything.Add(new PathResults(p, null));
						}
					}
					
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
				
				System.Diagnostics.Process.Start(Application.dataPath + "/graphgen");
				
				while(!System.IO.File.Exists("graphlog.txt"))
				{
					// wait...
				}
				
				String text = System.IO.File.ReadAllText("graphlog.txt");
				System.IO.File.Delete("graphlog.txt");
				
				String[] lines = text.Split(new char[] {'\n'});
				foreach (String line in lines)
				{
					Debug.Log(line);
				}
			}
			
			DirectoryInfo dir = new DirectoryInfo("clusteringdata/");
			FileInfo[] info = dir.GetFiles("*.xml");
			
			List<String> goodFiles = new List<String>();
			for (int count = 0; count < info.Count(); count ++)
			{ // find clustering results file to display in load menu.
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
				resetClusteringData();
				origPaths = new List<Path>();
				useColors = true;
				
				List<Path> pathsImported = PathBulk.LoadPathsFromFile ("clusteringdata/" + fileNames[chosenFileIndex]);
								
				clusterCentroids.Clear();
				clusterCentroids = new List<Path>();
				
				int curColor = 0;
				
				foreach (Path p in pathsImported) {
					if (p.color.a == 0.5
						&& curColor < colors.Count()
						&& p.color.r == colors[curColor].r
						&& p.color.g == colors[curColor].g
						&& p.color.b == colors[curColor].b)
					{
						clusterCentroids.Add(p);
						curColor ++;
					}
					toggleStatus.Add (p, true);
					paths.Add(p);
				}
				
				if (drawer == null) precomputeMaps();
			//	ComputeHeatMap(paths, deaths);
				
			//	heatMapColored = Analyzer.Compute2DHeatMapColored (paths, gridSize, gridSize, out maxHeatMap);
			//	drawer.heatMapColored = heatMapColored;
			//	drawer.heatMapMax = maxHeatMap;
			//	drawer.tileSize.Set (SpaceState.Editor.tileSize.x, SpaceState.Editor.tileSize.y);
				
				for (int count = 0; count < paths.Count(); count ++)
				{
					paths[count].name = count.ToString();
				}
				
				for (int color = 0; color < colors.Count(); color ++)
				{
					showPaths[color] = true; //(color < curColor) ? true : false;
				}
				
				chosenFileIndex = -1;
			}
			
			EditorGUILayout.LabelField ("");
			
			if (GUILayout.Button("Load platformer level"))
			{
				if (drawer == null) precomputeMaps();				
				rep = new LevelRepresentation();
				LevelRepresentation.tileSize = SpaceState.Editor.tileSize;
				LevelRepresentation.zero.Set(floor.collider.bounds.min.x, floor.collider.bounds.min.z);
				rep.loadPlatformerLevel();
			}
			
			if (GUILayout.Button("Import Platformer Paths & RDP"))
			{ // imports paths, sets up obstacles in viewer.
				if (rep == null) Debug.Log("Warning - no level representation (did you load level above?)");
				paths.Clear ();
				ClearPathsRepresentation ();
				resetClusteringData();
				clusterCentroids.Clear();
				origPaths = new List<Path>();
				
				DirectoryInfo batchDir = new DirectoryInfo("batchpaths/");
				FileInfo[] batchInfo = batchDir.GetFiles("*.xml");
			
				List<String> pathFilenames = new List<String>();
				for (int count = 0; count < batchInfo.Count(); count ++)
				{
					if (batchInfo[count].Name == "levelinfo.xml") continue;
					pathFilenames.Add(batchInfo[count].Name);
				}
				
				List<Path> pathsImported = new List<Path>();
				int pathNum = 0;
				foreach (String file in pathFilenames)
				{
					XmlSerializer ser = new XmlSerializer (typeof(PlatformerPathBridge));
					PlatformerPathBridge loaded = null;
					using (FileStream stream = new FileStream ("batchpaths/" + file, FileMode.Open)) {
						loaded = (PlatformerPathBridge)ser.Deserialize (stream);
						stream.Close ();
					}
					
					List<Node> pathPoints = new List<Node>();
					for (int count = 0; count < loaded.positionsField.Count(); count ++)
					{
						pathPoints.Add(new Node(loaded.positionsField[count].xField, loaded.positionsField[count].yField, count));
					}
					if (pathPoints.Count() <= 3) continue;
					
					Path path = new Path();
					path.points = LineReduction.DouglasPeuckerReduction(rep, pathPoints, rdpTolerance, true); //shortestPathAroundObstacles(pathPoints)));
					path.name = pathNum.ToString();
					pathNum ++;
					
					pathsImported.Add(path);
				}
				
				// Setup parenting
				foreach (Path p in pathsImported) {
					for (int i = p.points.Count - 1; i > 0; i--) {
						p.points [i].parent = p.points [i - 1];
					}
				}
					
				foreach (Path p in pathsImported)
				{
					p.color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
					toggleStatus.Add (p, true);
					paths.Add(p);
				}
				
		/*		for (int count = 0; count < paths.Count(); count ++)
				{
					paths[count].name = count.ToString();
				}*/
			}
			rdpTolerance = EditorGUILayout.IntField("RDP Tolerance", rdpTolerance);
			if (GUILayout.Button("RDP (simple path cleaning)"))
			{
				if (rep == null)
				{
					Debug.Log("No level representation (only supports platformer paths currently)!");
					return;
				}

				List<Path> newPaths = new List<Path>();
				foreach (Path p in paths)
				{
					Path reducedPath = new Path(p);
					reducedPath.points = LineReduction.DouglasPeuckerReduction(rep, p.points, rdpTolerance, true);
					reducedPath.color = new Color(p.color.r, p.color.g, p.color.b, p.color.a);
					for (int i = reducedPath.points.Count - 1; i > 0; i--) {
						reducedPath.points [i].parent = reducedPath.points [i - 1];
					}
					if (!newPaths.Contains(reducedPath))
					{
						newPaths.Add(reducedPath);
					}
				}

				ClearPathsRepresentation ();
				paths.Clear();
				resetClusteringData();
				
				foreach (Path p in newPaths)
				{
					paths.Add(p);
					toggleStatus.Add(paths.Last(), true);
				}
			}
			if (GUILayout.Button("Reduce to shortest paths"))
			{
				if (rep == null)
				{
					Debug.Log("No level representation (only supports platformer paths currently)!");
					return;
				}
				
				ClearPathsRepresentation ();
				for (int count = 0; count < paths.Count; count ++)
				{
					paths[count] = new Path(LineReduction.shortestPathAroundObstacles(rep, paths[count].points));
					toggleStatus.Add (paths[count], true);
				}
			}
			
			EditorGUILayout.LabelField ("");
			for (int count = 0; count < colors.Count(); count ++)
			{
				showPaths[count] = EditorGUILayout.Toggle(colorStrings[count], showPaths[count]);
			}
			if (useColors)
			{ // paths have been clustered, or results imported, so enforce color checkboxes.
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
			if (GUILayout.Button ("Show all colors"))
			{
				for(int i = 0; i < showPaths.Count(); i ++) showPaths[i] = true;
				foreach (Path p in paths) p.color.a = 1;
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
						showPaths[color] = false;
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
						if (p.Equals(clusterCentroids[colorIndex]) || (p.color.a == 0.5 && p.color.r == colors[colorIndex].r && p.color.g == colors[colorIndex].g && p.color.b == colors[colorIndex].b))
						{
							p.color.a = 1;
						}
						else p.color.a = 0;
					}
				}
			}
			if (GUILayout.Button ("Show avg. centroid for current color"))
			{
				int colorIndex = -1;
				int numSelectedColors = 0;
				for (int color = 0; color < colors.Count(); color ++)
				{
					if (showPaths[color])
					{
						numSelectedColors ++;
						colorIndex = color;
						showPaths[color] = false;
					}
				}
				
				if (colorIndex == -1 || numSelectedColors > 1)
				{
					Debug.Log("You must first select exactly one color above.");
				}
				else
				{
					PathCollection coloredPaths = new PathCollection();
					foreach (Path p in paths)
					{
						if (p.color.r == colors[colorIndex].r && p.color.g == colors[colorIndex].g && p.color.b == colors[colorIndex].b)
						{
							coloredPaths.Add(new Path(p));
						}
						p.color.a = 0; // set all paths to be invisible.
					}
					
					Path avgCentroid = coloredPaths.getAveragedCentroid();
					avgCentroid.color = colors[colorIndex];
					avgCentroid.name = "AvgCentroid";
					if (!paths.Contains(avgCentroid))
					{
						paths.Add(avgCentroid);
						toggleStatus.Add(paths.Last(), true);
					}
				}
			}
			if (!drawHeatMapColored)
			{
				if (GUILayout.Button ("Show heat map for current colors"))
				{
					drawHeatMapColored = true;
					drawer.drawMap = drawMap = true;
					drawer.drawPath = drawPath = false;
					
					for (int i = 0; i < colors.Count(); i ++)
					{
						drawHeatMapColors[i] = showPaths[i];
					}
				}
			}
			else
			{
				if (GUILayout.Button ("Hide heat map"))
				{
					drawHeatMapColored = false;
					drawer.drawMap = drawMap = false;
					drawer.drawPath = drawPath = true;
					
					for (int i = 0; i < colors.Count(); i ++)
					{
						drawHeatMapColors[i] = false;
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
						//pos.y = 0f;
						pos.y = (p.danger3*100);

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
					if (!paths.Contains(path))
					{
						paths.Add(path);
						toggleStatus.Add(paths.Last (), true);						
					}
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

		public static void resetClusteringData()
		{
			KMeans.reset();
			DBSCAN.reset();			
			clusterCentroids.Clear();
			clusterCentroids = new List<Path>();
		}
		
		private void precomputeMaps()
		{
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
	}
}