using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Common;
using Objects;
using Extra;

public class RuntimeController : MonoBehaviour, NodeProvider {
	
	public GameObject floor, end;
	public int gridSize = 60;
	public float stepSize = 0.1f;
	public bool smoothPlayerPath = false;
	//
	private Mapper mapper;
	private Cell[][] obstaclesMap;
	private Cell[][][] fullMap;
	private int endX, endY;
	private GameObject player;
	private Player play;
	private List<List<Vector2>> cells;
	private Path playerPath;
	private List<Vector3> playerPoints;
	private List<Quaternion> playerRot;
	private List<Vector2> playerLOS;
	private List<bool> playerSeen;
	
	
	// Use this for initialization
	void Start () {

		// First prepare the mapper class
		if (floor == null)
			floor = GameObject.Find ("Floor");
		if (mapper == null && floor != null) {
			mapper = floor.GetComponent<Mapper> ();
			if (floor == null)
				mapper = floor.AddComponent<Mapper> ();
		} else {
			Debug.LogError("No floor set, can't continue");
		}
		
		// Then, we setup the enviornment needed to make it work
		if (mapper != null) {
			mapper.ComputeTileSize (SpaceState.Running, floor.collider.bounds.min, floor.collider.bounds.max, gridSize, gridSize);
			obstaclesMap = mapper.ComputeObstacles ();
			
			GameObject[] en = GameObject.FindGameObjectsWithTag ("Enemy") as GameObject[];
			Enemy[] enemies = new Enemy[en.Length];

			for (int i = 0; i < en.Length; i++) {
				enemies [i] = en [i].GetComponent<Enemy> ();
				enemies [i].positions = new Vector3[10000];
				enemies [i].forwards = new Vector3[10000];
				enemies [i].rotations = new Quaternion[10000];
				enemies [i].cells = new Vector2[10000][];
				enemies [i].seesPlayer = new bool[10000];
			}
			play.cells = new Vector2[10000][];
			
			cells = new List<List<Vector2>> ();
			// Prepare the cells by enemy
			for (int i = 0; i < enemies.Length; i++) {
				cells.Add (new List<Vector2> ());
			}
			
			fullMap = new Cell[10000][][];
			
			SpaceState.Running.fullMap = fullMap;
			SpaceState.Running.enemies = enemies;

			GameObject tempPlayerNode = GameObject.Find("TempPlayerNode");
			if (tempPlayerNode != null) {
				while (tempPlayerNode.transform.childCount > 0)
					GameObject.DestroyImmediate(tempPlayerNode.transform.GetChild(0).gameObject);
				
				GameObject.DestroyImmediate(tempPlayerNode);
			}
			
			player = GameObject.FindGameObjectWithTag ("Player");
			if (player == null)
				player = GameObject.FindGameObjectWithTag ("AI");

			play = player.GetComponent<Player>();

			playerPath = new Path (new List<Node> ());
			playerPoints = new List<Vector3> ();
			playerRot = new List<Quaternion>();
			playerLOS = new List<Vector2>();
			playerSeen = new List<bool>();
			
			if (end == null) {
				end = GameObject.Find ("End");	
			}

			endX = (int)((end.transform.position.x - floor.collider.bounds.min.x) / SpaceState.Running.tileSize.x);
			endY = (int)((end.transform.position.z - floor.collider.bounds.min.z) / SpaceState.Running.tileSize.y);
			

			obstaclesMap [endX] [endY].goal = true;
			
			// Run this once before enemies moving so we compute the first iteration of map
			acc += stepSize + 1;
			LateUpdate ();
		}
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	private float acc = 0f;
	private Node last;
	
	// After all Update() are called, this method is invoked
	void LateUpdate () {
		if (acc > stepSize) {
			bool seen = false;

			for (int en = 0; en < SpaceState.Running.enemies.Length; en++)
				cells [en].Clear ();

			//CELLS IS A LIST OF LISTS OF VECTOR 2s
			//in other words a 2d array, 
			//1d -> enemy
			//2d -> seen cells for that enemy (vector 2 of x and y)
			Cell[][] computed = mapper.ComputeMap (obstaclesMap, SpaceState.Running.enemies, cells);
			fullMap [SpaceState.Running.timeSlice] = computed;
			
			// Store the seen cells in the enemy class
			List<Vector2>[] arr = cells.ToArray ();
			for (int i = 0; i < SpaceState.Running.enemies.Length; i++) {

				SpaceState.Running.enemies [i].cells [SpaceState.Running.timeSlice] = arr [i].ToArray ();
				SpaceState.Running.enemies [i].positions [SpaceState.Running.timeSlice] = SpaceState.Running.enemies [i].transform.position;
				SpaceState.Running.enemies [i].rotations [SpaceState.Running.timeSlice] = SpaceState.Running.enemies [i].transform.rotation;
				SpaceState.Running.enemies [i].forwards [SpaceState.Running.timeSlice] = SpaceState.Running.enemies [i].transform.forward;
				arr [i].Clear ();

				//If player is seen by enemy
				bool seesPlayer = false;
				foreach(Vector2 v in SpaceState.Running.enemies[i].cells[SpaceState.Running.timeSlice])
				{
					if(play.inCell(SpaceState.Running.timeSlice,(int)v.x,(int)v.y))
					{
						seesPlayer = true;
						seen = true;
					}
				}
				SpaceState.Running.enemies[i].seesPlayer[SpaceState.Running.timeSlice] = (seesPlayer);
			}



			
			
			Vector2 pos = new Vector2 ((player.transform.position.x - SpaceState.Running.floorMin.x) / SpaceState.Running.tileSize.x, (player.transform.position.z - SpaceState.Running.floorMin.z) / SpaceState.Running.tileSize.y);
			int mapX = (int)pos.x;
			int mapY = (int)pos.y;
			Quaternion mapRot = player.transform.rotation;
			float mapRange = play.flashlight.getLightVector().x;
			float mapAngle = play.flashlight.getLightVector().y;


			//float mapRange = 

			Node curr = new Node ();
			curr.t = SpaceState.Running.timeSlice;
			curr.x = mapX;
			curr.y = mapY;
			curr.rot = mapRot;
			curr.visAngle = mapAngle;
			curr.visRange = mapRange;
			curr.seen = seen;
			curr.cell = fullMap [curr.t] [curr.x] [curr.y];
			curr.parent = last;
			last = curr;


			if(curr.x == endX && curr.y == endY)
				SpaceState.Running.fullMap [SpaceState.Running.timeSlice - 1] [curr.x] [curr.y].goal = true;
			
			playerPath.points.Add (last);
			playerPoints.Add (player.transform.position);
			playerRot.Add (player.transform.rotation);
			playerLOS.Add (play.flashlight.getLightVector());
			playerSeen.Add (seen);
			SpaceState.Running.timeSlice++;
			acc -= stepSize;
		}
		acc += Time.deltaTime;
	}
	
	public void OnApplicationQuit () {
		if (smoothPlayerPath) {
			Node final = null;

			foreach (Node each in playerPath.points) {
				final = each;
				while (Extra.Collision.SmoothNode(final, this, SpaceState.Running, true)) {
				}
			}
				
			playerPath.points.Clear ();
				
			while (final != null) {
				playerPath.points.Add (final);
				final = final.parent;
			}
			playerPath.points.Reverse ();				
		}

		playerPath.color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));

		List<Path> paths = new List<Path> ();
		paths.Add (playerPath);
		PathBulk.SavePathsToFile ("playerPath.xml", paths);
		new PathML (SpaceState.Running).SavePathsToFile ("playerML.xml", playerPoints, playerRot, playerLOS);
	}

	public Node GetNode (int t, int x, int y) {
		Node n3 = new Node ();
		n3.cell = SpaceState.Running.fullMap [t] [x] [y];
		n3.x = x;
		n3.t = t;
		n3.y = y;
		return n3;
	}
}