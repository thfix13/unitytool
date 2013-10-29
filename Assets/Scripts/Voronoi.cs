using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

public class Voronoi : MonoBehaviour {
	
	public static int numOfEnemies = 0;
	public static int numOfRegions = 0;
	public static GameObject[] gos = null;
	public static GameObject waypointPrefab = null;
	public static List<Vector2> voronoiCentre = null;
	public static List<int> nearestIndice = null;
	
	public static int[] maxAreaIndexHolder = null;	
	public static int maxAreaIndex = 0;
	public static int[] cntArray = null;
//	public static Array<int> cntList = null;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public static void Initialize (GameObject wpPrefab) {
		waypointPrefab = wpPrefab;
	}
	
	public static void CalculateVoronoiRegions(GameObject floor, Cell[][] obs, int n1, int n2) {
		
		//Get all the points to calculate the voronoi 
		gos = GameObject.FindGameObjectsWithTag ("Enemy").OrderBy (go => go.transform.position.x).ToArray ();
		voronoiCentre = new List<Vector2> ();	
		nearestIndice = new List<int> ();
		numOfEnemies = n1;
		numOfRegions = n2;
		
		foreach (GameObject g in gos) {
			voronoiCentre.Add (new Vector2 (g.transform.position.x - floor.collider.bounds.min.x, g.transform.position.z - floor.collider.bounds.min.z));
		}
		
		for (int i = 0; i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x) + 1; i++) {
			float posX;
			posX = i * SpaceState.TileSize.x + SpaceState.TileSize.x / 2.0f;
			for (int j = 0; j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y) + 1; j++) {
				if (obs [i] [j].blocked == true) {
					obs [i] [j].nearestVoronoiCentre = -1;
					continue;
				}
				int cnt = 0, nearestIndex = 0;
				float posY, tempDis;
				float minDistance = float.MaxValue;
				posY = j * SpaceState.TileSize.y + SpaceState.TileSize.y / 2.0f;
				Vector2 tilePos = new Vector2 (posX, posY);
				foreach (Vector2 enemyPos in voronoiCentre) {
					tempDis = Vector2.Distance (tilePos, enemyPos);
					if (tempDis < minDistance) {
						nearestIndex = cnt;
						minDistance = tempDis;
						//Debug.Log(minDistance);
					}
					cnt++;
				}
				// Flooding; need visualization here
				// The following commented line of code would reduce computational efficiency significantly
				// Debug.Log ("I belong to enemy " + nearestIndex);
				obs [i] [j].nearestVoronoiCentre = nearestIndex;
				nearestIndice.Add (nearestIndex);
			}
		}	
	}
	
	public static int[] SelectMaximumRegions () {
		
		// cntList = new Array<int> (numOfEnemies);
		cntArray = new int[numOfRegions + 1];
		maxAreaIndexHolder = new int[numOfEnemies];
		// counting
		foreach (int i in nearestIndice) {
//			switch (i) {
//			case 0:
//				cntArray [0]++;
//				break;
//			case 1:
//				cntArray [1]++;
//				break;
//			case 2:
//				cntArray [2]++;
//				break;
//			case 3:
//				cntArray [3]++;
//				break;
//			case 4:
//				cntArray [4]++;
//				break;
//			}
			cntArray [i] ++;
		}
		
		int maxArea = 0, tempMaxArea = 0;
		for (int j = 0; j < numOfEnemies; j++) { 
			for (int i = 0; i < cntArray.Length; i++) {
				tempMaxArea = cntArray [i];
				if (tempMaxArea > maxArea) {
					maxArea = tempMaxArea;
					maxAreaIndex = i;
				}
			}
			maxAreaIndexHolder [j] = maxAreaIndex;
			cntArray [maxAreaIndex] = 0;
			maxArea = 0;
		}
		
		return maxAreaIndexHolder;
	}
	
	public static void PathInVoronoiRegion (GameObject floor, Cell[][] obs, int iterations) {
		
		for (int m = 0; m < numOfEnemies; m++) {
			//Pick two random nodes in max area and be as large as possible after a certain iteration
			Vector3 randomPos1, randomPos2, dir;
			Vector3 node1 = new Vector3 (0.0f, 0.0f, 0.0f);
			Vector3 node2 = new Vector3 (0.0f, 0.0f, 0.0f);
			float maxDis = float.MinValue;
			float tempDist = 0.0f;
			// This could be another parameter
			for (int iter = 1; iter < iterations; iter++) {
				do {
					do {
						randomPos1 = new Vector3 (UnityEngine.Random.Range (floor.collider.bounds.min.x, floor.collider.bounds.max.x), 0.3f, UnityEngine.Random.Range (floor.collider.bounds.min.z, floor.collider.bounds.max.z));
					} while (obs[(int)((randomPos1.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x)][(int)((randomPos1.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y)].blocked == true ||
						obs[(int)((randomPos1.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x)][(int)((randomPos1.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y)].nearestVoronoiCentre != maxAreaIndexHolder[m]);
					do {
						randomPos2 = new Vector3 (UnityEngine.Random.Range (floor.collider.bounds.min.x, floor.collider.bounds.max.x), 0.3f, UnityEngine.Random.Range (floor.collider.bounds.min.z, floor.collider.bounds.max.z));	
					} while (obs[(int)((randomPos2.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x)][(int)((randomPos2.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y)].blocked == true ||
						obs[(int)((randomPos2.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x)][(int)((randomPos2.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y)].nearestVoronoiCentre != maxAreaIndexHolder[m]);
					tempDist = Vector3.Distance (randomPos1, randomPos2);
					dir = new Vector3 (randomPos2.x - randomPos1.x, randomPos2.y - randomPos1.y, randomPos2.z - randomPos1.z);
				} while ((tempDist < maxDis) || (Physics.Raycast(randomPos1, dir, tempDist) == true));
				node1.x = randomPos1.x;
				node1.y = randomPos1.y;
				node1.z = randomPos1.z;
				node2.x = randomPos2.x;
				node2.y = randomPos2.y;
				node2.z = randomPos2.z;
				maxDis = tempDist;
			}
			Debug.Log ("From: [" + (int)((node1.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x) + "][" + 
				(int)((node1.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y) + "] To: [" + 
				(int)((node2.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x) + "][" + 
				(int)((node2.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y) + "]. ");
				
			//Debug.Log("The longest path is from" + node1 + "to" + node2 + ".");
			Debug.Log ("The longest path is " + maxDis);
			
			//Move between two nodes
			Vector3 p1 = new Vector3 (node1.x, node1.y, node1.z);
			Vector3 p2 = new Vector3 (node2.x, node2.y, node2.z);
			//Waypoint wp1, wp2;
			GameObject wp1 = GameObject.Instantiate (waypointPrefab, p1, Quaternion.identity) as GameObject;
			GameObject wp2 = GameObject.Instantiate (waypointPrefab, p2, Quaternion.identity) as GameObject;
			Waypoint wpScript1, wpScript2;
			wpScript1 = wp1.GetComponent ("Waypoint") as Waypoint;
			wpScript2 = wp2.GetComponent ("Waypoint") as Waypoint;
			wpScript1.next = wpScript2;
			wpScript2.next = wpScript1;
			
			Debug.Log ("GameObject[" + maxAreaIndexHolder [m] + "]");
			//Set the new patrol path
			gos [maxAreaIndexHolder [m]].GetComponent<Enemy> ().moveSpeed = 1;
			gos [maxAreaIndexHolder [m]].GetComponent<Enemy> ().rotationSpeed = 30;
			gos [maxAreaIndexHolder [m]].GetComponent<Enemy> ().target = wpScript2;
			gos [maxAreaIndexHolder [m]].transform.position = new Vector3 (wp1.transform.position.x, 
				wp1.transform.position.y, wp1.transform.position.z);
		}
	}
	
	public static void DestroyVoronoiCentre () {
		//Destroy other voronoi centres and complete the scene
		foreach (GameObject g in gos) {
			for (int n = 0; n < numOfEnemies; n++) {
				if (g == gos [maxAreaIndexHolder [n]]) {
					break;
				}
				if (n == numOfEnemies - 1) {
					GameObject.DestroyImmediate (g);		
				}
			}
		}
	}
}
