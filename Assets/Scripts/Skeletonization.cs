using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Skeletonization
{
	private int imax = 0, jmax = 0;
	public Cell[][] obs = null;
	public List<Cell> boundaryArray = new List<Cell> ();
	public List<int> boundaryXArray = new List<int> ();
	public List<int> boundaryZArray = new List<int> ();
	public List<int> nearestIndice = new List<int> ();
	
	// Store a collection of valid roots of contours
	public List<ContourNode> contoursList = new List<ContourNode> ();
	
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
	
	public void extractContours (GameObject floor)
	{
		imax = (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x);
		jmax = (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y);
		for (int i = 0; i < imax + 1; i++) {
			for (int j = 0; j < jmax + 1; j++) {
				if (obs [i] [j].visited) {
					continue;	
				}
				if (obs [i] [j].blocked == true) {
					obs [i] [j].visited = true;
					continue;
				}
				ContourNode root = new ContourNode (0, 0);
				depthFirstSearch (i, j, root);
				if (root.children.Count != 0) {
					contoursList.Add (root);
				}
			}
		}
	}
	
	private void depthFirstSearch (int i, int j, ContourNode parent)
	{
		// Check if it is a node
		// ->
		if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i + 1] [j].nearestVoronoiCentre 
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i - 1] [j].blocked == false && obs [i + 1] [j].blocked == false
			&& obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
		    && obs [i] [j].visited == false && obs[i+1][j].node == false) 
		{
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);

			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;
			
			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i - 1, j, cn);
			depthFirstSearch (i + 1, j, cn);
			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
			depthFirstSearch (i - 1, j + 1, cn);
			depthFirstSearch (i - 1, j - 1, cn);
		}
		//<-
		else if (i > 0 && j > 0 && i < imax && j < jmax 
			&& obs [i] [j].nearestVoronoiCentre == obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i - 1] [j].nearestVoronoiCentre 
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i - 1] [j].blocked == false && obs [i + 1] [j].blocked == false
			&& obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
		    && obs [i] [j].visited == false && obs[i-1][j].node == false) {
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);
			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;
			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i - 1, j, cn);
			depthFirstSearch (i + 1, j, cn);
			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
			depthFirstSearch (i - 1, j + 1, cn);
			depthFirstSearch (i - 1, j - 1, cn);
		}
		//^
		//|
		else if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i + 1] [j].nearestVoronoiCentre 
			&& obs [i] [j].nearestVoronoiCentre != obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i - 1] [j].blocked == false && obs [i + 1] [j].blocked == false
			&& obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
		    && obs [i] [j].visited == false && obs[i][j+1].node == false) {
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);
			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;
			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i - 1, j, cn);
			depthFirstSearch (i + 1, j, cn);
			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
			depthFirstSearch (i - 1, j + 1, cn);
			depthFirstSearch (i - 1, j - 1, cn);
		}
		// |
		// _
		else if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i + 1] [j].nearestVoronoiCentre 
			&& obs [i] [j].nearestVoronoiCentre != obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i - 1] [j].blocked == false && obs [i + 1] [j].blocked == false
			&& obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
		    && obs [i] [j].visited == false && obs[i][j-1].node == false) {
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);
			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;
			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i - 1, j, cn);
			depthFirstSearch (i + 1, j, cn);
			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
			depthFirstSearch (i - 1, j + 1, cn);
			depthFirstSearch (i - 1, j - 1, cn);
		}

		//  r
		// ccr 
		//  c
		else if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
		         && obs [i] [j].visited == false && ( ! obs[i+1][j].node || !obs[i][j+1].node  ) ) {
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);

			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;

			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i - 1, j, cn);
			depthFirstSearch (i + 1, j, cn);
			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
			depthFirstSearch (i - 1, j + 1, cn);
			depthFirstSearch (i - 1, j - 1, cn);
		}
		//  c
		// ccr
		//  r
		else if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
		    && obs [i] [j].visited == false && (! obs[i][j-1].node || ! obs[i+1][j].node )) {
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);
			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;
			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i - 1, j, cn);
			depthFirstSearch (i + 1, j, cn);
			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
			depthFirstSearch (i - 1, j + 1, cn);
			depthFirstSearch (i - 1, j - 1, cn);
		}

		//  c
		// rcc
		//  r

		else if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
	        && obs [i] [j].visited == false && (! obs[i-1][j].node || ! obs[i][j-1].node )) {
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);
			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;
			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i - 1, j, cn);
			depthFirstSearch (i + 1, j, cn);
			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
			depthFirstSearch (i - 1, j + 1, cn);
			depthFirstSearch (i - 1, j - 1, cn);
		}

		//  r
		// rcc
		//  c

		else if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
	        && obs [i] [j].visited == false && (! obs[i-1][j].node || ! obs[i][j+1].node )) {
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);
			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;
			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i - 1, j, cn);
			depthFirstSearch (i + 1, j, cn);
			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
			depthFirstSearch (i - 1, j + 1, cn);
			depthFirstSearch (i - 1, j - 1, cn);
		}
		// c 
		//rccc
		// c

		else if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i - 1] [j].nearestVoronoiCentre == obs [i - 2] [j].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
	        && obs [i] [j].visited == false && ! obs[i-1][j].node ) {
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);
			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;
			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i - 1, j, cn);
			depthFirstSearch (i + 1, j, cn);

			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
			depthFirstSearch (i - 1, j + 1, cn);
			depthFirstSearch (i - 1, j - 1, cn);
		}
		//  c
		//cccr
		//  c
		else if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i + 1] [j].nearestVoronoiCentre == obs [i + 2] [j].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
	        && obs [i] [j].visited == false && ! obs[i+1][j].node) {
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);
			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;
			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i - 1, j, cn);
			depthFirstSearch (i + 1, j, cn);
			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
			depthFirstSearch (i - 1, j + 1, cn);
			depthFirstSearch (i - 1, j - 1, cn);
		}
		// 
		// c
		//ccc
		// r
		// r
		else if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j - 1].nearestVoronoiCentre == obs [i] [j - 2].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
	        && obs [i] [j].visited == false && ! obs[i][j-1].node) {
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);
			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;
			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i - 1, j, cn);
			depthFirstSearch (i + 1, j, cn);
			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
			depthFirstSearch (i - 1, j + 1, cn);
			depthFirstSearch (i - 1, j - 1, cn);

		}
		// r
		//ccc
		// c
		// c
		else if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j -1].nearestVoronoiCentre == obs [i] [j - 2].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
	        && obs [i] [j].visited == false && ! obs[i][j+1].node) {
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);
			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;
			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i - 1, j, cn);
			depthFirstSearch (i + 1, j, cn);
			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
			depthFirstSearch (i - 1, j + 1, cn);
			depthFirstSearch (i - 1, j - 1, cn);
		}


		else if (i == 0 && j > 0 && j < jmax
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
			&& obs [i] [j].visited == false) {
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);

			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;
			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i + 1, j, cn);
			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
		}
		//
		else if (j == 0 && i > 0 && i < imax
			&& obs [i] [j].nearestVoronoiCentre != obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i - 1] [j].blocked == false && obs [i] [j + 1].blocked == false
			&& obs [i] [j].visited == false) {
			// Create a new contour node and assign parent-child relations
			ContourNode cn = new ContourNode (i, j);
			cn.parent = parent;
			parent.children.Add (cn);
			// Set visited to true
			obs [i] [j].visited = true;
			obs [i] [j].node = true;
			// Dig deeper
			depthFirstSearch (i, j + 1, cn);
			depthFirstSearch (i, j - 1, cn);
			depthFirstSearch (i + 1, j, cn);
			depthFirstSearch (i + 1, j + 1, cn);
			depthFirstSearch (i + 1, j - 1, cn);
		}

		return;	
	}
}
