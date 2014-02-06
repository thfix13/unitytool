using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Skeletonization
{
	public Cell[][] obs = null;
	public List<Cell> boundaryArray = new List<Cell> ();
	public List<int> boundaryXArray = new List<int> ();
	public List<int> boundaryZArray = new List<int> ();
	public List<Cell>[] contourArray = null;
	public List<int>[] contourXArray = null;
	public List<int>[] contourZArray = null;
	public List<int> nearestIndice = new List<int> ();
	
	public void calculateBoundaries (GameObject floor)
	{
		// This function marks the boundaries.
		for (int i = 0; i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x) + 1; i++) {
			for (int j = 0; j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y) + 1; j++) {
				if (obs [i] [j].blocked == true) {
					obs [i] [j].isNextToWall = -1;
					continue;
				}
				if ((i > 0 && obs [i - 1] [j].blocked == true) ||
					(j > 0 && obs [i] [j - 1].blocked == true) ||
					(i > 0 && j > 0 && obs [i - 1] [j - 1].blocked == true) ||
					(i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x) && (obs [i + 1] [j].blocked == true)) ||
					(j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y) && (obs [i] [j + 1].blocked == true)) ||
					(i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x) && j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y) && (obs [i + 1] [j + 1].blocked == true)) ||
					(i == 0) || (j == 0) ||
					(i == (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x)) ||
					(j == (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y))) {
					boundaryArray.Add (obs [i] [j]);
					boundaryXArray.Add (i);
					boundaryZArray.Add (j);
					obs [i] [j].isNextToWall = 1;
				}
				obs [i] [j].isNextToWall = 0;
			}
		}
	}
	
//	public void distributeToContours (GameObject floor)
//	{
//		int contourIndex = 0;
//		for (int index = 0; index < boundaryArray.Count; index++) {
//			int i = boundaryXArray.ElementAt (index);
//			int j = boundaryZArray.ElementAt (index);
//			bool needBackward = false;
//			// Visit four neighbours
//			if (i > 0 && obs [i - 1] [j].isNextToWall == 1 && !needBackward) {
//				
//			}
//			if (obs [i] [j+1].isNextToWall == 1 && !needBackward) {
//				
//			}
//			if (obs [i] [])
//		}
//		
//		for (int i = 0; i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x) + 1; i++) {
//			for (int j = 0; j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y) + 1; j++) {
//				// if blocked, continue
//				if (obs [i] [j].blocked == true) {
//					obs [i] [j].visited = true;
//					continue;	
//				}
//				
//				// if visited, continue
//				if (obs [i] [j].visited) {
//					continue;	
//				}
//				
//				if ((i > 0 && obs [i - 1] [j].blocked == true) ||
//					(j > 0 && obs [i] [j - 1].blocked == true) ||
//					(i > 0 && j > 0 && obs [i - 1] [j - 1].blocked == true) ||
//					(i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x) && (obs [i + 1] [j].blocked == true)) ||
//					(j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y) && (obs [i] [j + 1].blocked == true)) ||
//					(i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x) && j < (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y) && (obs [i + 1] [j + 1].blocked == true)) ||
//					(i == 0) || (j == 0) ||
//					(i == (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x)) ||
//					(j == (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y))) {
//					if (contourArray [contourIndex] == null) {
//						contourArray [contourIndex] = new List<Cell> ();
//						contourArray [contourIndex].Add (obs [i] [j]);
//					}
//					if (contourXArray [contourIndex] == null) {
//						contourXArray [contourIndex] = new List<Cell> ();
//						contourXArray [contourIndex].Add (i);
//					}
//					if (contourZArray [contourIndex] == null) {
//						contourZArray [contourIndex] = new List<Cell> ();
//						contourZArray [contourIndex].Add (j);
//					}
//					
//					
//				}
//				
//				obs [i] [j].visited = true;
//				contourIndex ++;
//			}
//		}
//	}
	
	public void boundaryPointsFlooding (GameObject floor)
	{
		int numOfPoints = boundaryArray.Count;
		
		List<int> iList = new List<int> ();
		List<int> jList = new List<int> ();
		
		for (int cnt = 0; cnt < numOfPoints; cnt++) {
			int i = boundaryXArray.ElementAt (cnt);
			int j = boundaryZArray.ElementAt (cnt);
			obs [i] [j].nearestVoronoiCentre = cnt;
			nearestIndice.Add (cnt);
			iList.Add (i);
			jList.Add (j);
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
			
			if (i < (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x)
				&& obs [i + 1] [j].nearestVoronoiCentre == -1 && obs [i + 1] [j].blocked == false) {
				obs [i + 1] [j].nearestVoronoiCentre = obs [i] [j].nearestVoronoiCentre;
				iList.Add (i + 1);
				jList.Add (j);
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
			
			if (i > 0 && obs [i - 1] [j].nearestVoronoiCentre == -1 && obs [i - 1] [j].blocked == false) {
				obs [i - 1] [j].nearestVoronoiCentre = obs [i] [j].nearestVoronoiCentre;
				iList.Add (i - 1);
				jList.Add (j);
				nearestIndice.Add (obs [i] [j].nearestVoronoiCentre);
				// Debug.Log ("I belong to enemy " + obs [i] [j].nearestVoronoiCentre);
			}
			
			iList.RemoveAt (0);
			jList.RemoveAt (0);
		}
	}
}
