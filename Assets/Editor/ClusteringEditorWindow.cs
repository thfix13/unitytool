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
	public enum ClustEnv { ENV_STEALTH, ENV_PLATFORM, ENV_PUZZLE };
	public class ClusteringEditorWindow : EditorWindow {

		// Data holders
		private static Cell[][][] fullMap, original;
		public static List<Path> paths = new List<Path> ();

		// Parameters with default values
		public static int timeSamples = 2000, attemps = 25000, iterations = 1, gridSize = 60, ticksBehind = 0;
		private static bool drawHeatMap = false, drawPath = true, limitImportablePathsToCurLevel = true;
		private static float stepSize = 1 / 10f;
		private static int nbPaths = -1, nbBatch = -1,fileloaded = -1;

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
		private static int numClusters = 4, distMetric = 0, chosenFileIndex = -1, currentColor = 0, numPasses = 1, rdpTolerance = 4, maxClusters = 17, minPathsForCluster = 5, genre = 0;
		private static float dbsScanEps = 11.0f;
		private static List<Path> clusterCentroids = new List<Path>(), origPaths = new List<Path>();
		private static bool[] showPaths = new bool[colors.Count()];
		private static bool autoSavePaths = true, discardHighDangerPaths = true, drawHeatMapColored = false, useColors = false, showNoise = false;
		public static bool altCentroidComp = false, useScalable = false;
		public static bool[] drawHeatMapColors = new bool[ClusteringEditorWindow.colors.Count()];
		public static bool drawAllHeatMap = true;
		LevelRepresentation rep;
		public static bool only2DTriangulation = true;
		public static ClustEnv clustEnvironment = ClustEnv.ENV_STEALTH;
		private static String[] pathFileNames;
		private static String nameFile;

		// Computed parameters
		private static int[,] heatMap;
		private static GameObject start = null, end = null, floor = null, playerPrefab = null;
		private static Dictionary<Path, bool> toggleStatus = new Dictionary<Path, bool> ();
		private static Dictionary<Path, GameObject> players = new Dictionary<Path, GameObject> ();
		private static int startX, startY, endX, endY, maxHeatMap, timeSlice, imported = 0;
		private static int[][,] heatMapColored;

		// Helping stuff
		private static Vector2 scrollPos = new Vector2 ();
		private static GameObject playerNode;
		private List<Tuple<Vector3, string>> textDraw = new List<Tuple<Vector3, string>>();
		private int lastTime = timeSlice;
		private Mapper mapper;
		private ClusteringEditorDrawer drawer;
		private DateTime previous = DateTime.Now;
		private static bool simulated = false, playing = false;
		private long stepInTicks = ((long)(stepSize * 10000000L)), playTime = 0L;
		private long accL = 0L;
		
		[MenuItem("Window/Clustering")]
		static void Init () {
			ClusteringEditorWindow window = (ClusteringEditorWindow)EditorWindow.GetWindow (typeof(ClusteringEditorWindow));
			window.title = "Clustering";
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
					drawer = floor.gameObject.GetComponent<ClusteringEditorDrawer> ();
					if (drawer == null) {
						drawer = floor.gameObject.AddComponent<ClusteringEditorDrawer> ();
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
			
			EditorGUILayout.LabelField ("1. Setup");

			String[] genres = { "Stealth", "Platformer", "Puzzle" };
			genre = EditorGUILayout.Popup("Game genre", genre, genres);
			
			#region 1. Stealth/MGS
			
			if (genre == (int)ClustEnv.ENV_STEALTH)
			{
				nameFile = EditorApplication.currentScene;
				nameFile = nameFile.Replace(".unity","");
				nameFile = nameFile.Replace("Assets/Levels/","");
			
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

				pathFileNames = goodPathFiles.ToArray();
				chosenFileIndex = EditorGUILayout.Popup("Import paths", chosenFileIndex, pathFileNames);
				if (chosenFileIndex != -1)
				{
					fileloaded = chosenFileIndex; 

					clustEnvironment = ClustEnv.ENV_STEALTH;
					useColors = false;
					paths.Clear ();
					ClearPathsRepresentation ();
					resetClusteringData();
					origPaths = new List<Path>();
					precomputeMaps();
					gridSize = 60;
				
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
						p.name = "Imported " + (++imported);
						p.color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), 
							                     UnityEngine.Random.Range (0.0f, 1.0f), 
							                     UnityEngine.Random.Range (0.0f, 1.0f));
						toggleStatus.Add (p, true);
						paths.Add(p);
					}

					precomputeMaps();

					ComputeHeatMap (paths);
				
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
					Debug.Log(paths.Count + " paths imported");
				
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
			
				nbPaths = (int)EditorGUILayout.IntField("Max num to import", nbPaths);
			
			}
			
			#endregion
						
			// ----------------------------------
			
			#region 1. Platformer
			
			if (genre == (int)ClustEnv.ENV_PLATFORM)
			{
				DirectoryInfo pathsDir = new DirectoryInfo("./levels/platformer/");
				DirectoryInfo[] pathsInfo = pathsDir.GetDirectories();
			
				List<String> goodPathFiles = new List<String>();
				for (int count = 0; count < pathsInfo.Count(); count ++)
				{ // find clustering results file to display in load menu.
					goodPathFiles.Add(pathsInfo[count].Name);
				}

				pathFileNames = goodPathFiles.ToArray();
				chosenFileIndex = EditorGUILayout.Popup("Import level & paths", chosenFileIndex, pathFileNames);
				if (chosenFileIndex != -1)
				{
					String levelPath = "./levels/platformer/" + pathFileNames[chosenFileIndex];
					clustEnvironment = ClustEnv.ENV_PLATFORM;
					if (drawer == null) precomputeMaps();
					rep = new LevelRepresentation();
					rep.loadPlatformerLevel(levelPath);

					paths.Clear ();
					ClearPathsRepresentation ();
					resetClusteringData();
					clusterCentroids.Clear();
					origPaths = new List<Path>();
					
					GameObject floorObj = GameObject.Find("Floor");
					floorObj.transform.localScale = new Vector3(50, 1, 50);
					SceneView.lastActiveSceneView.pivot = new Vector3(0, 0, 0);
					SceneView.lastActiveSceneView.Repaint();
				
					DirectoryInfo batchDir = new DirectoryInfo(levelPath);
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
						using (FileStream stream = new FileStream (levelPath + "/" + file, FileMode.Open)) {
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
						path.points = pathPoints; // LineReduction.DouglasPeuckerReduction(rep, pathPoints, rdpTolerance, true);
						path.name = pathNum.ToString();
						pathNum ++;
												
						pathsImported.Add(path);
					}
					
					// get translation for points with vals < 0
					
					bool negative = false;
					double lowestNegativeX = 0;
					double lowestNegativeY = 0;
					foreach (Path p in pathsImported) {
						for (int i = 0; i < p.points.Count; i++) {
							if (p.points[i].x < 0 || p.points[i].y < 0) {
								negative = true;
								if (p.points[i].xD < lowestNegativeX)
									lowestNegativeX = p.points[i].xD;
								if (p.points[i].yD < lowestNegativeY)
									lowestNegativeY = p.points[i].yD;
							}
						}
					}
					
					if (negative) {
						foreach (Path p in pathsImported) {
							for (int i = 0; i < p.points.Count; i++) {
								p.points[i].x += -(int)lowestNegativeX;
								p.points[i].y += -(int)lowestNegativeY;
								p.points[i].xD += -lowestNegativeX;
								p.points[i].yD += -lowestNegativeY;
							}
						}							
					}
					rep.updateObstaclePos(-lowestNegativeX, -lowestNegativeY);
					
				/*	double highestVal = 0;
					foreach (Path p in pathsImported) {
						for (int i = 0; i < p.points.Count; i++) {
							if (p.points[i].xD > highestVal)
								highestVal = p.points[i].xD;
							if (p.points[i].yD > highestVal)
								highestVal = p.points[i].yD;
						}
					}
					
					gridSize = (int)highestVal + 1; */
					gridSize = 500;
				
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
					
					ComputeHeatMap (paths);
				
			/*		for (int count = 0; count < paths.Count(); count ++)
					{
						paths[count].name = count.ToString();
					}*/
					
					chosenFileIndex = -1;
				}
			
				rdpTolerance = EditorGUILayout.IntField("RDP Tolerance", rdpTolerance);
				if (GUILayout.Button("RDP (simple path cleaning)"))
				{
					if (rep == null)
					{
						Debug.Log("No level representation!");
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
				if (GUILayout.Button("Shorter paths"))
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
			}
			
			#endregion
			
			// ----------------------------------
			
			#region 1. Puzzle
			
			if (genre == (int)ClustEnv.ENV_PUZZLE)
			{
				DirectoryInfo pathsDir = new DirectoryInfo("./levels/puzzle/");
				DirectoryInfo[] pathsInfo = pathsDir.GetDirectories();
			
				List<String> goodPathFiles = new List<String>();
				for (int count = 0; count < pathsInfo.Count(); count ++)
				{ // find clustering results file to display in load menu.
					goodPathFiles.Add(pathsInfo[count].Name);
				}

				pathFileNames = goodPathFiles.ToArray();
				chosenFileIndex = EditorGUILayout.Popup("Import level & paths", chosenFileIndex, pathFileNames);
				
				if (chosenFileIndex != -1)
				{
					String levelPath = "./levels/puzzle/" + pathFileNames[chosenFileIndex];
					
					clustEnvironment = ClustEnv.ENV_PUZZLE;
					if (drawer == null) precomputeMaps();
					rep = new LevelRepresentation();
					rep.loadPuzzleLevel(levelPath);
					
					paths.Clear ();
					ClearPathsRepresentation ();
					resetClusteringData();
					clusterCentroids.Clear();
					origPaths = new List<Path>();
					clearScreen();
					GameObject cubeObj = GameObject.Find("Cube");
					GameObject.DestroyImmediate(cubeObj);
					
					SceneView.lastActiveSceneView.pivot = new Vector3(0, 0, 0);
					SceneView.lastActiveSceneView.Repaint();
				
				    Texture2D tex = null;
			        byte[] fileData;
			        if (File.Exists(levelPath + "/ss.png"))
					{
						Debug.Log("Drawing");
			            fileData = File.ReadAllBytes(levelPath + "/ss.png");
			            tex = new Texture2D(2, 2);
			            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
				        GameObject texBG = GameObject.CreatePrimitive(PrimitiveType.Cube);
						texBG.transform.position = new Vector3(493, 0, -302);
						texBG.transform.localScale = new Vector3(-32*32, 0, -32*24);
						MeshRenderer rend = texBG.GetComponent<MeshRenderer>();
						rend.material.mainTexture = tex;
						rend.material.shader = Shader.Find("Self-Illumin/VertexLit");
			        }
				
					DirectoryInfo batchDir = new DirectoryInfo(levelPath);
					FileInfo[] batchInfo = batchDir.GetFiles("*");
			
					List<String> pathFilenames = new List<String>();
					for (int count = 0; count < batchInfo.Count(); count ++)
					{
						if (batchInfo[count].Name == "ss.png") continue;
						pathFilenames.Add(batchInfo[count].Name);
					}
				
					List<Path> pathsImported = new List<Path>();
					int pathNum = 0;
					foreach (String file in pathFilenames)
					{
						String sol;
						using (var sr = new StreamReader(levelPath + "/" + file)) {
							sol = sr.ReadLine();
					    }
					
						List<Node> pathPoints = new List<Node>();
						pathPoints.Add(new Node(rep.startPos.x, rep.startPos.y, 0));
						for (int count = 0; count < sol.Length; count ++)
						{
							if (sol[count] == 'D')
								pathPoints.Add(new Node(pathPoints.Last().x, pathPoints.Last().y-1, 10*(count+1)));
							else if (sol[count] == 'U')
								pathPoints.Add(new Node(pathPoints.Last().x, pathPoints.Last().y+1, 10*(count+1)));
							else if (sol[count] == 'R')
								pathPoints.Add(new Node(pathPoints.Last().x+1, pathPoints.Last().y, 10*(count+1)));
							else if (sol[count] == 'L')
								pathPoints.Add(new Node(pathPoints.Last().x-1, pathPoints.Last().y, 10*(count+1)));
							else if (sol[count] == ' ')
								pathPoints.Add(new Node(pathPoints.Last().x, pathPoints.Last().y, 10*(count+1)));
						}
					
						Path path = new Path();
						path.points = pathPoints;
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
				
					chosenFileIndex = -1;
				}	
			}
						
			#endregion
			
			EditorGUILayout.LabelField ("");
			
			// ----------------------------------
			
			#region 2. Visualization
			
			EditorGUILayout.LabelField ("2. Visualization");
			
			timeSlice = EditorGUILayout.IntSlider ("Time", timeSlice, 0, timeSamples - 1);
			if (GUILayout.Button (playing ? "Stop" : "Play")) {
				playing = !playing;
			}
			drawHeatMap = EditorGUILayout.Toggle ("Draw heatmap", drawHeatMap);
			drawPath = EditorGUILayout.Toggle ("Draw paths", drawPath);
			
			if (drawer != null) {
		//		drawer.heatMap = null;

		//		if (drawHeatMap) {
					drawer.heatMap = heatMap;
		//		}
			}
			
			if (GUILayout.Button ("Show all colors"))
			{
				for(int i = 0; i < showPaths.Count(); i ++) showPaths[i] = true;
				foreach (Path p in paths) p.color.a = 1;
				
				drawAllHeatMap = true;
				
				currentColor = -1;
				
				foreach (KeyValuePair<Path, GameObject> each in players) {
					
					GameObject.DestroyImmediate(each.Value);
				}
				players.Clear();
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
				
				foreach (KeyValuePair<Path, GameObject> each in players) {
					
					GameObject.DestroyImmediate(each.Value);
				}
				players.Clear();
			}
			
			EditorGUILayout.LabelField ("");
			
			#endregion
			
			#region 3. Clustering
						
			EditorGUILayout.LabelField ("3. Clustering");

			clustAlg = EditorGUILayout.Popup("Algorithm", clustAlg, clustAlgs);

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
				
		//		for (int count = 0; count < paths.Count(); count ++)
		//		{
		//			paths[count].name = count.ToString();
		//		}
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
				only2DTriangulation = EditorGUILayout.Toggle("Only 2d triangulation", 
					only2DTriangulation);
			}
			nbBatch = (int)EditorGUILayout.IntField("Num batches",nbBatch);

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
							p.name = "Imported " + (++imported);
							p.color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), 
								                     UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
							toggleStatus.Add (p, true);
							paths.Add(p);
						}

						precomputeMaps();

						ComputeHeatMap (paths);
												
		//				for (int count = 0; count < paths.Count(); count ++)
		//				{
		//					paths[count].name = count.ToString();
		//				}

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
						Debug.Log("You have less paths than you have desired clusters - either compute/import more paths or decrease cluster amount.");
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

					int noisy = 0;
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
						String clustAlgStr = clustAlgsShort[clustAlg];
						if (clustAlg == 1)
						{
							clustAlgStr += "-" + (int)dbsScanEps + "eps-" + minPathsForCluster + "minpath-" + noisy + "noisy";
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
			
			if (GUILayout.Button("Generate stat graph (Mac only)"))
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
				
				heatMapColored = Analyzer.Compute2DHeatMapColored (paths, gridSize, gridSize, out maxHeatMap);
				drawer.heatMapColored = heatMapColored;
				drawer.heatMapMax = maxHeatMap;
				if (clustEnvironment == ClustEnv.ENV_STEALTH)
					drawer.tileSize.Set (SpaceState.Editor.tileSize.x, SpaceState.Editor.tileSize.y);
				
			//	for (int count = 0; count < paths.Count(); count ++)
			//	{
			//		paths[count].name = count.ToString();
			//	}
				
				for (int color = 0; color < colors.Count(); color ++)
				{
					showPaths[color] = true; //(color < curColor) ? true : false;
				}
				
				chosenFileIndex = -1;
			}
									
			/*if (GUILayout.Button("Use orig paths with current colors"))
			{
				// assume saved results are already loaded in.
				List<Path> resultPaths = new List<Path>();
				foreach (Path p in paths)
				{
					resultPaths.Add(new Path(p));
				}
				List<Color> pathColors = new List<Color>();
				foreach (Path p in paths)
				{
					pathColors.Add(new Color(p.color.r, p.color.g, p.color.b, p.color.a));
				}
				
				// load the original paths in.
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
					path.points = pathPoints; //LineReduction.DouglasPeuckerReduction(rep, pathPoints, rdpTolerance, true); //shortestPathAroundObstacles(pathPoints)));
					path.name = pathNum.ToString(); // name comes from the order in which it is loaded - which comes from the natural ordering of the files 1.xml 2.xml etc.
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
					// loop through resultPaths for path with same name and use same color as that one
					bool found = false;
					for (int count = 0; count < resultPaths.Count(); count ++)
					{
						if (p.name == resultPaths[count].name)
						{
							p.color = new Color(pathColors[count].r, pathColors[count].g, pathColors[count].b, pathColors[count].a);
							found = true;
						}
					}
					if (!found) {
						Debug.Log("error");
						p.color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
					}
					toggleStatus.Add (p, true);
					paths.Add(p);
				}
			}*/
			
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
				if (GUILayout.Button ("Show heatmap for current colors"))
				{
					drawAllHeatMap = false;
					drawHeatMapColored = true;
					drawer.drawPath = drawPath = false;
					
					for (int i = 0; i < colors.Count(); i ++)
					{
						drawHeatMapColors[i] = showPaths[i];
					}
				}
			}
			else
			{
				if (GUILayout.Button ("Hide heatmap for current colors"))
				{
					drawAllHeatMap = true;
					drawHeatMapColored = false;
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
						if ( currentColor > -1 && ( p.Key.color.r != colors[currentColor].r || p.Key.color.g != colors[currentColor].g || p.Key.color.b != colors[currentColor].b ) ) continue;

						if (!players.ContainsKey (p.Key)) {
							GameObject player = GameObject.Instantiate (playerPrefab) as GameObject;
							player.transform.position.Set (p.Key.points [0].x, 0f, p.Key.points [0].y);
							player.transform.parent = playerNode.transform;
							players.Add (p.Key, player);
							Material m = new Material (player.renderer.sharedMaterial);
							m.color = p.Key.color;
							player.renderer.material = m;
							player.renderer.material.shader = Shader.Find("Self-Illumin/VertexLit");
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
				drawer.drawPath = drawPath;
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
				
		private void ComputeHeatMap (List<Path> paths) {
			heatMap = Analyzer.Compute2DHeatMap (paths, gridSize, gridSize, out maxHeatMap);

			drawer.heatMapMax = maxHeatMap;
			drawer.heatMap = heatMap;
			
			if (ClusteringEditorWindow.clustEnvironment == ClustEnv.ENV_STEALTH)
				drawer.tileSize.Set (SpaceState.Editor.tileSize.x, SpaceState.Editor.tileSize.y);
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
						
						pos.x *= 32; //SpaceState.Editor.tileSize.x;
						pos.z *= 32; //SpaceState.Editor.tileSize.y;
//						GameObject lev = GameObject.Find("Cube");
				//		pos.x += lev.transform.position.x; // floor.collider.bounds.min.x;
				//		pos.z += lev.transform.position.z; //floor.collider.bounds.min.z;
						//pos.y = 0f;
				//		pos.y = (p.danger3*100);

						each.Value.transform.position = pos;
						each.Value.transform.localScale = new Vector3(32, 0, 32);
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
			
			ClusteringEditorWindow window = (ClusteringEditorWindow)EditorWindow.GetWindow (typeof(ClusteringEditorWindow));
			EditorUtility.SetDirty (window);
			window.Repaint();
			window.Update();
			window.OnGUI();
			
			window.drawer = floor.gameObject.GetComponent<ClusteringEditorDrawer> ();
			
				window.drawer.timeSlice = timeSlice;
				window.drawer.drawHeatMap = drawHeatMap;
				window.drawer.drawPath = drawPath;
				window.drawer.paths = toggleStatus;
				window.drawer.textDraw = window.textDraw;
			
			if (original != null && window.lastTime != timeSlice) {
				window.lastTime = timeSlice;
				window.UpdatePositions (timeSlice, window.mapper);
			}
			
			
			SceneView.RepaintAll ();			
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
				drawer = floor.gameObject.GetComponent<ClusteringEditorDrawer> ();
				if (drawer == null) {
					drawer = floor.gameObject.AddComponent<ClusteringEditorDrawer> ();
					drawer.hideFlags = HideFlags.HideInInspector;
				}
			}
			
			if (!simulated) {
				StorePositions ();
				simulated = true;
			}
			
			Cell[][] baseMap = null;
			if (ClusteringEditor.grid != null) {
				Cell[][] obstacles = mapper.ComputeObstacles ();
				baseMap = new Cell[gridSize][];
				for (int x = 0; x < gridSize; x++) {
					baseMap [x] = new Cell[gridSize];
					for (int y = 0; y < gridSize; y++) {
						baseMap [x] [y] = ClusteringEditor.grid [x] [y] == null ? obstacles [x] [y] : ClusteringEditor.grid [x] [y];
					}
				}
			}
			
			original = mapper.PrecomputeMaps (SpaceState.Editor, floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize, timeSamples, stepSize, ticksBehind, baseMap);

			drawer.fullMap = original;
			if (clustEnvironment == ClustEnv.ENV_STEALTH)
			{
				drawer.tileSize = SpaceState.Editor.tileSize;
				drawer.zero.Set (floor.collider.bounds.min.x, floor.collider.bounds.min.z);				
			}
			
			previous = DateTime.Now;
		}
		
		private void clearScreen()
		{
			// destroy current floor
			GameObject levelObj = GameObject.Find("Level"); 
			for (int i = levelObj.transform.childCount - 1; i > -1; i--)
			{
			    GameObject.DestroyImmediate(levelObj.transform.GetChild(i).gameObject);
			}
			while (true)
			{
				GameObject enemyObj = GameObject.Find("Enemy");
				if (enemyObj == null) break;
				GameObject.DestroyImmediate(enemyObj);
			}
			GameObject wayptObj = GameObject.Find("Waypoints"); 
			for (int i = wayptObj.transform.childCount - 1; i > -1; i--)
			{
			    GameObject.DestroyImmediate(wayptObj.transform.GetChild(i).gameObject);
			}
		}
	}
}