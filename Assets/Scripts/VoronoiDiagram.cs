using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class VoronoiDiagram
{
	public int numOfGuards = 0, numOfRegions = 0;
	public Cell[][] obs = null;
	public List<Vector2> voronoiCentre = null;
	public List<int> nearestIndice = null;
	public int[] maxAreaIndexHolder = null;
	public int maxAreaIndex = 0;
	public int[] cntArray = null;
	public List<Cell>[] boundaryArray = null;
	public List<int>[] boundaryXArray = null;
	public List<int>[] boundaryZArray = null;
	
	public void calculateVoronoiRegions (GameObject floor, int n1, int n2, GameObject[] gos)
	{
		//Get all the points to calculate the voronoi 
		voronoiCentre = new List<Vector2> ();	
		nearestIndice = new List<int> ();
		numOfGuards = n1;
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
				// Flooding;
				// The following commented line of code would reduce computational efficiency significantly
				// Debug.Log ("I belong to enemy " + nearestIndex);
				obs [i] [j].nearestVoronoiCentre = nearestIndex;
				nearestIndice.Add (nearestIndex);
			}
		}	
	}
	
	public void calculateVoronoiRegionsUsingFlooding (GameObject floor, int n1, int n2, GameObject[] gos)
	{
		nearestIndice = new List<int> ();
		numOfGuards = n1;
		numOfRegions = n2;
		
		List<int> iList = new List<int> ();
		List<int> jList = new List<int> ();
		int cnt = 0;
		
		// Initializing lists
		foreach (GameObject go in gos) {
			int i = (int)((go.transform.position.x - floor.collider.bounds.min.x) / SpaceState.TileSize.x);
			int j = (int)((go.transform.position.z - floor.collider.bounds.min.z) / SpaceState.TileSize.y);
			obs [i] [j].nearestVoronoiCentre = cnt;
			nearestIndice.Add (cnt);
			// Debug.Log ("I belong to enemy " + cnt);
			iList.Add (i);
			jList.Add (j);
			cnt++;
		}
		
		while (iList.Count != 0) {
			int i = iList.ElementAt (0);
			int j = jList.ElementAt (0);
			
			// Traverse 8 neighbors
			if (j > 0 && obs [i] [j - 1].nearestVoronoiCentre == -1 && obs [i] [j - 1].blocked == false) {
				obs [i] [j - 1].nearestVoronoiCentre = obs [i] [j].nearestVoronoiCentre;
				iList.Add (i);
				jList.Add (j - 1);
				nearestIndice.Add (obs [i] [j].nearestVoronoiCentre);
				// Debug.Log ("I belong to enemy " + obs [i] [j].nearestVoronoiCentre);
			}
			
			if (j > 0 && i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x)
				&& obs [i + 1] [j - 1].nearestVoronoiCentre == -1 && obs [i + 1] [j - 1].blocked == false) {
				obs [i + 1] [j - 1].nearestVoronoiCentre = obs [i] [j].nearestVoronoiCentre;
				iList.Add (i + 1);
				jList.Add (j - 1);
				nearestIndice.Add (obs [i] [j].nearestVoronoiCentre);
				// Debug.Log ("I belong to enemy " + obs [i] [j].nearestVoronoiCentre);
			}
			
			if (i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x)
				&& obs [i + 1] [j].nearestVoronoiCentre == -1 && obs [i + 1] [j].blocked == false) {
				obs [i + 1] [j].nearestVoronoiCentre = obs [i] [j].nearestVoronoiCentre;
				iList.Add (i + 1);
				jList.Add (j);
				nearestIndice.Add (obs [i] [j].nearestVoronoiCentre);
				// Debug.Log ("I belong to enemy " + obs [i] [j].nearestVoronoiCentre);
			}
			
			if (i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x) && j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y)
				&& obs [i + 1] [j + 1].nearestVoronoiCentre == -1 && obs [i + 1] [j + 1].blocked == false) {
				obs [i + 1] [j + 1].nearestVoronoiCentre = obs [i] [j].nearestVoronoiCentre;
				iList.Add (i + 1);
				jList.Add (j + 1);
				nearestIndice.Add (obs [i] [j].nearestVoronoiCentre);
				// Debug.Log ("I belong to enemy " + obs [i] [j].nearestVoronoiCentre);
			}
			
			if (j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y)
				&& obs [i] [j + 1].nearestVoronoiCentre == -1 && obs [i] [j + 1].blocked == false) {
				obs [i] [j + 1].nearestVoronoiCentre = obs [i] [j].nearestVoronoiCentre;
				iList.Add (i);
				jList.Add (j + 1);
				nearestIndice.Add (obs [i] [j].nearestVoronoiCentre);
				// Debug.Log ("I belong to enemy " + obs [i] [j].nearestVoronoiCentre);
			}
			
			if (i > 0 && j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y)
				&& obs [i - 1] [j + 1].nearestVoronoiCentre == -1 && obs [i - 1] [j + 1].blocked == false) {
				obs [i - 1] [j + 1].nearestVoronoiCentre = obs [i] [j].nearestVoronoiCentre;
				iList.Add (i - 1);
				jList.Add (j + 1);
				nearestIndice.Add (obs [i] [j].nearestVoronoiCentre);
				// Debug.Log ("I belong to enemy " + obs [i] [j].nearestVoronoiCentre);
			}
			
			if (i > 0 && obs [i - 1] [j].nearestVoronoiCentre == -1 && obs [i - 1] [j].blocked == false) {
				obs [i - 1] [j].nearestVoronoiCentre = obs [i] [j].nearestVoronoiCentre;
				iList.Add (i - 1);
				jList.Add (j);
				nearestIndice.Add (obs [i] [j].nearestVoronoiCentre);
				// Debug.Log ("I belong to enemy " + obs [i] [j].nearestVoronoiCentre);
			}
			
			if (i > 0 && j > 0 && obs [i - 1] [j - 1].nearestVoronoiCentre == -1 && obs [i - 1] [j - 1].blocked == false) {
				obs [i - 1] [j - 1].nearestVoronoiCentre = obs [i] [j].nearestVoronoiCentre;
				iList.Add (i - 1);
				jList.Add (j - 1);
				nearestIndice.Add (obs [i] [j].nearestVoronoiCentre);
				// Debug.Log ("I belong to enemy " + obs [i] [j].nearestVoronoiCentre);
			}
			iList.RemoveAt(0);
			jList.RemoveAt(0);
		}
	}
	
	public void calculateBoundaries (GameObject floor)
	{
		boundaryArray = new List<Cell>[numOfRegions];
		boundaryXArray = new List<int>[numOfRegions];
		boundaryZArray = new List<int>[numOfRegions];
		for (int i = 0; i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x) + 1; i++) {
			for (int j = 0; j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y) + 1; j++) {
				if (obs [i] [j].blocked == true) {
					obs [i] [j].isVoronoiBoundary = -1;
					obs [i] [j].isNextToWall = -1;
					continue;
				}
				if (obs [i] [j].nearestVoronoiCentre == -1) {
					continue;
				}
				if ((i > 0 && (obs [i - 1] [j].blocked == true || obs [i - 1] [j].nearestVoronoiCentre != obs [i] [j].nearestVoronoiCentre)) 
					|| (j > 0 && (obs [i] [j - 1].blocked == true || obs [i] [j - 1].nearestVoronoiCentre != obs [i] [j].nearestVoronoiCentre))
					|| (i > 0 && j > 0 && (obs [i - 1] [j - 1].blocked == true || obs [i - 1] [j - 1].nearestVoronoiCentre != obs [i] [j].nearestVoronoiCentre))
					|| (i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x) && (obs [i + 1] [j].blocked == true || obs [i + 1] [j].nearestVoronoiCentre != obs [i] [j].nearestVoronoiCentre))
					|| (j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y) && (obs [i] [j + 1].blocked == true || obs [i] [j + 1].nearestVoronoiCentre != obs [i] [j].nearestVoronoiCentre))
					|| (i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x) && j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y) && (obs [i + 1] [j + 1].blocked == true || obs [i + 1] [j + 1].nearestVoronoiCentre != obs [i] [j].nearestVoronoiCentre))) {
					obs [i] [j].isVoronoiBoundary = 1;
					if (boundaryArray [obs [i] [j].nearestVoronoiCentre] == null) {
						boundaryArray [obs [i] [j].nearestVoronoiCentre] = new List<Cell> ();
						boundaryArray [obs [i] [j].nearestVoronoiCentre].Add (obs [i] [j]);
					} else {
						boundaryArray [obs [i] [j].nearestVoronoiCentre].Add (obs [i] [j]);	
					}
					if (boundaryXArray [obs [i] [j].nearestVoronoiCentre] == null) {
						boundaryXArray [obs [i] [j].nearestVoronoiCentre] = new List<int> ();
						boundaryXArray [obs [i] [j].nearestVoronoiCentre].Add (i);
					} else {
						boundaryXArray [obs [i] [j].nearestVoronoiCentre].Add (i);	
					}
					if (boundaryZArray [obs [i] [j].nearestVoronoiCentre] == null) {
						boundaryZArray [obs [i] [j].nearestVoronoiCentre] = new List<int> ();
						boundaryZArray [obs [i] [j].nearestVoronoiCentre].Add (j);
					} else {
						boundaryZArray [obs [i] [j].nearestVoronoiCentre].Add (j);	
					}
				} else {
					obs [i] [j].isVoronoiBoundary = 0;	
				}
				if ((i > 0 && obs [i - 1] [j].blocked == true)
					|| (j > 0 && obs [i] [j - 1].blocked == true)
					|| (i > 0 && j > 0 && obs [i - 1] [j - 1].blocked == true)
					|| (i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x) && obs [i + 1] [j].blocked == true)
					|| (j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y) && obs [i] [j + 1].blocked == true)
					|| (i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x) && j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y) && obs [i + 1] [j + 1].blocked == true)) {
					obs [i] [j].isNextToWall = 1;
				} else {
					obs [i] [j].isNextToWall = 0;	
				}
			}
		}
	}
	
	public int[] selectMaximumRegions ()
	{
		cntArray = new int[numOfRegions];
		maxAreaIndexHolder = new int[numOfGuards];
		// counting
		foreach (int i in nearestIndice) {
			cntArray [i] ++;
		}
		
		int maxArea = 0, tempMaxArea = 0;
		for (int j = 0; j < numOfGuards; j++) { 
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
}
