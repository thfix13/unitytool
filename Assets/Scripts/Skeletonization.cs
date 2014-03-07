using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Skeletonization
{
	private int imax = 0, jmax = 0;
	public Cell[][] obs = null;
	
	// --Obsolete--
	public List<Cell> boundaryArray = new List<Cell> ();
	public List<int> boundaryXArray = new List<int> ();
	public List<int> boundaryZArray = new List<int> ();
	public List<int> nearestIndice = new List<int> ();
	public List<ContourNode> contoursList = new List<ContourNode> ();
	// --Obsolete--
	
	public int boundaryIndex = 0;
	public List<List<Cell>> boundaryContoursList = new List<List<Cell>> ();
	public List<Cell> freeCells = new List<Cell> ();
	
	// Roadmap Stuff
	public List<RoadmapNode> roadmapNodesList = new List<RoadmapNode> ();
	public Dictionary<string, RoadmapNode> roadmapDictionary = new Dictionary<string, RoadmapNode> ();
	
	public void identifyObstacleContours (GameObject floor)
	{
		imax = (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x);
		jmax = (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y);
		for (int i = 0; i < imax + 1; i++) {
			for (int j = 0; j < jmax + 1; j++) {
				if (obs [i] [j].obsVisited) {
					continue;	
				}
				boundaryIndex++;
				depthFirstSearchForObstacles(i, j, boundaryIndex);
				if (boundaryContoursList.Count != boundaryIndex) {
					boundaryIndex--;	
				}
			}	
		}
	}
	
	private void depthFirstSearchForObstacles (int i, int j, int boundaryIndex) 
	{
		if (obs [i][j].obsVisited) {
			return;	
		}
		
		if ((obs [i][j].blocked == true || (obs [i] [j].blocked == false && (i == 0 || j == 0 
			|| i == imax
			|| j == jmax))) && obs[i][j].obsVisited == false) {
			
			obs [i] [j].obsVisited = true;	
			
			if(i > 0 && j > 0 && i < imax && j < jmax && (!obs [i - 1] [j].blocked || !obs [i + 1] [j].blocked || !obs [i] [j + 1].blocked || !obs [i] [j - 1].blocked
				|| !obs [i - 1] [j + 1].blocked || !obs [i + 1] [j + 1].blocked || !obs [i + 1] [j - 1].blocked || !obs [i - 1] [j - 1].blocked)) {
				obs [i] [j].obstacleBelongTo = boundaryIndex;
				
				if (boundaryContoursList.Count != boundaryIndex) {
					List<Cell> boundary = new List<Cell> ();
					boundaryContoursList.Add (boundary);
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				} else if (boundaryContoursList.Count == boundaryIndex) {
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				}
				// Dig deeper
				depthFirstSearchForObstacles (i - 1, j, boundaryIndex);
				depthFirstSearchForObstacles (i, j + 1, boundaryIndex);
				depthFirstSearchForObstacles (i + 1, j, boundaryIndex);
				depthFirstSearchForObstacles (i, j - 1, boundaryIndex);
			}
			
			// Boundary of the map
			if (i == 0 && j > 0 && j < jmax && (!obs [i + 1] [j].blocked || !obs [i] [j - 1].blocked || !obs [i] [j + 1].blocked)) {
				obs [i] [j].obstacleBelongTo = boundaryIndex;
				if (boundaryContoursList.Count != boundaryIndex) {
					List<Cell> boundary = new List<Cell> ();
					boundaryContoursList.Add (boundary);
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				} else if (boundaryContoursList.Count == boundaryIndex) {
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				}
				depthFirstSearchForObstacles (i, j + 1, boundaryIndex);
				depthFirstSearchForObstacles (i + 1, j, boundaryIndex);
				depthFirstSearchForObstacles (i, j - 1, boundaryIndex);
			}
			if (j == 0 && i > 0 && i < imax && (!obs [i - 1] [j].blocked || !obs [i + 1] [j].blocked || !obs [i][j+1].blocked)) {
				obs [i] [j].obstacleBelongTo = boundaryIndex;
				if (boundaryContoursList.Count != boundaryIndex) {
					List<Cell> boundary = new List<Cell> ();
					boundaryContoursList.Add (boundary);
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				} else if (boundaryContoursList.Count == boundaryIndex) {
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				}
				depthFirstSearchForObstacles (i - 1, j, boundaryIndex);
				depthFirstSearchForObstacles (i, j + 1, boundaryIndex);
				depthFirstSearchForObstacles (i + 1, j, boundaryIndex);
			}
			if (i == imax && j > 0 && j < jmax
				&& (!obs [i -1] [j].blocked || !obs [i][j-1].blocked || !obs [i][j+1].blocked)) {
				obs [i] [j].obstacleBelongTo = boundaryIndex;
				if (boundaryContoursList.Count != boundaryIndex) {
					List<Cell> boundary = new List<Cell> ();
					boundaryContoursList.Add (boundary);
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				} else if (boundaryContoursList.Count == boundaryIndex) {
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				}
				depthFirstSearchForObstacles (i - 1, j, boundaryIndex);
				depthFirstSearchForObstacles (i, j + 1, boundaryIndex);
				depthFirstSearchForObstacles (i, j - 1, boundaryIndex);	
			}
			if (j == jmax && i > 0 && i < imax
				&& (!obs [i -1] [j].blocked || !obs [i][j-1].blocked || !obs [i + 1][j].blocked)) {
				obs [i] [j].obstacleBelongTo = boundaryIndex;
				if (boundaryContoursList.Count != boundaryIndex) {
					List<Cell> boundary = new List<Cell> ();
					boundaryContoursList.Add (boundary);
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				} else if (boundaryContoursList.Count == boundaryIndex) {
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				}
				depthFirstSearchForObstacles (i - 1, j, boundaryIndex);
				depthFirstSearchForObstacles (i + 1, j, boundaryIndex);
				depthFirstSearchForObstacles (i, j - 1, boundaryIndex);
			}
			if (i == 0 && j == 0 && (!obs [i] [j + 1].blocked || !obs [i + 1] [j].blocked)) {
				obs [i] [j].obstacleBelongTo = boundaryIndex;
				if (boundaryContoursList.Count != boundaryIndex) {
					List<Cell> boundary = new List<Cell> ();
					boundaryContoursList.Add (boundary);
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				} else if (boundaryContoursList.Count == boundaryIndex) {
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				}
				depthFirstSearchForObstacles (i, j + 1, boundaryIndex);
				depthFirstSearchForObstacles (i + 1, j, boundaryIndex);
			}
			if (i == 0 && j == jmax && (!obs [i] [j - 1].blocked || !obs [i + 1] [j].blocked)) {	
				obs [i] [j].obstacleBelongTo = boundaryIndex;
				if (boundaryContoursList.Count != boundaryIndex) {
					List<Cell> boundary = new List<Cell> ();
					boundaryContoursList.Add (boundary);
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				} else if (boundaryContoursList.Count == boundaryIndex) {
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				}
				depthFirstSearchForObstacles (i, j - 1, boundaryIndex);
				depthFirstSearchForObstacles (i + 1, j, boundaryIndex);
			}
			if (j == 0 && i == imax && (!obs [i - 1] [j].blocked || !obs [i] [j + 1].blocked)) {
				if (boundaryContoursList.Count != boundaryIndex) {
					List<Cell> boundary = new List<Cell> ();
					boundaryContoursList.Add (boundary);
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				} else if (boundaryContoursList.Count == boundaryIndex) {
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				}
				depthFirstSearchForObstacles (i, j + 1, boundaryIndex);
				depthFirstSearchForObstacles (i - 1, j, boundaryIndex);
			}
			if (j == jmax && i == imax && (!obs [i - 1] [j].blocked || !obs [i] [j - 1].blocked)) {
				obs [i] [j].obstacleBelongTo = boundaryIndex;
				if (boundaryContoursList.Count != boundaryIndex) {
					List<Cell> boundary = new List<Cell> ();
					boundaryContoursList.Add (boundary);
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				} else if (boundaryContoursList.Count == boundaryIndex) {
					boundaryContoursList.ElementAt (boundaryIndex - 1).Add (obs [i] [j]);
				}
				depthFirstSearchForObstacles (i, j - 1, boundaryIndex);
				depthFirstSearchForObstacles (i - 1, j, boundaryIndex);
			}
			
			// Not boundary but blocked
			if (i >0 && j > 0 && i < imax && j < jmax 
				&& obs [i - 1] [j].blocked && obs [i - 1] [j + 1].blocked && obs [i] [j + 1].blocked
				&& obs [i + 1] [j + 1].blocked && obs [i + 1] [j].blocked && obs [i + 1] [j - 1].blocked
				&& obs [i] [j - 1].blocked && obs [i - 1] [j - 1].blocked ) {
				obs [i] [j].obstacleBelongTo = 0;	
			}
			if (i == 0 && j > 0 && j < jmax
				&& obs [i] [j + 1].blocked && obs [i + 1] [j + 1].blocked && obs [i + 1] [j].blocked && obs [i + 1] [j - 1].blocked
				&& obs [i] [j - 1].blocked) {
				obs [i] [j].obstacleBelongTo = 0;	
			}
			if (j == 0 && i > 0 && i < imax
				&& obs [i - 1] [j].blocked && obs [i - 1] [j + 1].blocked && obs [i] [j + 1].blocked
				&& obs [i + 1] [j + 1].blocked && obs [i + 1] [j].blocked ) {
				obs [i] [j].obstacleBelongTo = 0;
			}
			if (i == imax && j > 0 && j < jmax
				&& obs [i - 1] [j].blocked && obs [i - 1] [j + 1].blocked && obs [i] [j + 1].blocked
				&& obs [i] [j - 1].blocked && obs [i - 1] [j - 1].blocked ) {
				obs [i] [j].obstacleBelongTo = 0;
			}
			if (j == jmax && i > 0 && i < imax
				&& obs [i - 1] [j].blocked && obs [i + 1] [j].blocked && obs [i + 1] [j - 1].blocked
				&& obs [i] [j - 1].blocked && obs [i - 1] [j - 1].blocked ) {
				obs [i] [j].obstacleBelongTo = 0;
			}
			if (i == 0 && j == 0 && obs [i] [j + 1].blocked && obs [i + 1] [j + 1].blocked && obs [i + 1] [j].blocked ) {
				obs [i] [j].obstacleBelongTo = 0;
			}
			if (i == 0 && j == jmax && obs [i] [j - 1].blocked && obs [i + 1] [j - 1].blocked && obs [i] [j - 1].blocked ) {
				obs [i] [j].obstacleBelongTo = 0;
			}
			if (i == imax && j == jmax && obs [i - 1] [j].blocked && obs [i - 1] [j - 1].blocked && obs [i] [j - 1].blocked ) {
				obs [i] [j].obstacleBelongTo = 0;
			}
			if (i == imax && j == 0 && obs [i - 1] [j].blocked && obs [i - 1] [j + 1].blocked && obs [i] [j + 1].blocked ) {
				obs [i] [j].obstacleBelongTo = 0;
			}
			
		}
		return;
	}
	
	public void boundaryContoursFlooding (GameObject floor) 
	{
		imax = (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x);
		jmax = (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y);
		
		calculateFreeCells (floor);
		while (freeCells.Count != 0) {
		//for (int a = 0; a < 6; a++) {
			foreach (List<Cell> boundaryContour in boundaryContoursList) {
				int numOfCells = boundaryContour.Count;
				bool changedLT = false, changedRT = false, changedRB = false, changedLB = false;
				for (int cnt = 0; cnt < numOfCells; cnt++) {
					Cell boundaryCell = boundaryContour.First ();
					if (boundaryCell.i > 0 && obs [boundaryCell.i - 1] [boundaryCell.j].obstacleBelongTo == -1) {
						obs [boundaryCell.i - 1] [boundaryCell.j].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i - 1] [boundaryCell.j]);
						freeCells.Remove (obs [boundaryCell.i - 1] [boundaryCell.j]);
					}
					if (boundaryCell.i > 0 && boundaryCell.j < jmax && obs [boundaryCell.i - 1] [boundaryCell.j + 1].obstacleBelongTo == -1) {
						changedLT = true;
						obs [boundaryCell.i - 1] [boundaryCell.j + 1].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i - 1] [boundaryCell.j + 1]);
						freeCells.Remove (obs [boundaryCell.i - 1] [boundaryCell.j + 1]);
					}
					if (boundaryCell.j < jmax && obs [boundaryCell.i] [boundaryCell.j + 1].obstacleBelongTo == -1) {
						obs [boundaryCell.i] [boundaryCell.j + 1].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i] [boundaryCell.j + 1]);
						freeCells.Remove (obs [boundaryCell.i] [boundaryCell.j + 1]);
					}
					if (boundaryCell.i < imax && boundaryCell.j < jmax && obs [boundaryCell.i + 1] [boundaryCell.j + 1].obstacleBelongTo == -1) {
						changedRT = true;
						obs [boundaryCell.i + 1] [boundaryCell.j + 1].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i + 1] [boundaryCell.j + 1]);
						freeCells.Remove (obs [boundaryCell.i + 1] [boundaryCell.j + 1]);
					}
					if (boundaryCell.i < imax && obs [boundaryCell.i + 1] [boundaryCell.j].obstacleBelongTo == -1) {
						obs [boundaryCell.i + 1] [boundaryCell.j].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i + 1] [boundaryCell.j]);
						freeCells.Remove (obs [boundaryCell.i + 1] [boundaryCell.j]);
					}
					if (boundaryCell.i < imax && boundaryCell.j > 0 && obs [boundaryCell.i + 1] [boundaryCell.j - 1].obstacleBelongTo == -1) {
						changedRB = true;
						obs [boundaryCell.i + 1] [boundaryCell.j - 1].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i + 1] [boundaryCell.j - 1]);	
						freeCells.Remove (obs [boundaryCell.i + 1] [boundaryCell.j - 1]);
					}
					if (boundaryCell.j > 0 && obs [boundaryCell.i] [boundaryCell.j - 1].obstacleBelongTo == -1) {
						obs [boundaryCell.i] [boundaryCell.j - 1].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i] [boundaryCell.j - 1]);
						freeCells.Remove (obs [boundaryCell.i] [boundaryCell.j - 1]);
					}
					if (boundaryCell.i > 0 && boundaryCell.j > 0 && obs [boundaryCell.i - 1] [boundaryCell.j - 1].obstacleBelongTo == -1) {
						changedLB = true;
						obs [boundaryCell.i - 1] [boundaryCell.j - 1].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i - 1] [boundaryCell.j - 1]);
						freeCells.Remove (obs [boundaryCell.i - 1] [boundaryCell.j - 1]);
					}
					
					// Dealing with collision
					if (changedLT) {
						if (boundaryCell.i > 0 && boundaryCell.j < jmax && ((obs [boundaryCell.i - 1] [boundaryCell.j].obstacleBelongTo != -1
							&& obs [boundaryCell.i - 1] [boundaryCell.j].obstacleBelongTo != 0
							&& obs [boundaryCell.i - 1] [boundaryCell.j].obstacleBelongTo != boundaryCell.obstacleBelongTo)
							|| (obs [boundaryCell.i] [boundaryCell.j + 1].obstacleBelongTo != -1
							&& obs [boundaryCell.i] [boundaryCell.j + 1].obstacleBelongTo != 0
							&& obs [boundaryCell.i] [boundaryCell.j + 1].obstacleBelongTo != boundaryCell.obstacleBelongTo))) {
							obs [boundaryCell.i - 1] [boundaryCell.j + 1].obstacleBelongTo = -1;
						}
					}
					if (changedRT) {
						if (boundaryCell.i < imax && boundaryCell.j < jmax && ((obs [boundaryCell.i] [boundaryCell.j + 1].obstacleBelongTo != -1
							&& obs [boundaryCell.i] [boundaryCell.j + 1].obstacleBelongTo != 0
							&& obs [boundaryCell.i] [boundaryCell.j + 1].obstacleBelongTo != boundaryCell.obstacleBelongTo)
							|| (obs [boundaryCell.i + 1] [boundaryCell.j].obstacleBelongTo != -1
							&& obs [boundaryCell.i + 1] [boundaryCell.j].obstacleBelongTo != 0
							&& obs [boundaryCell.i + 1] [boundaryCell.j].obstacleBelongTo != boundaryCell.obstacleBelongTo))) {
							obs [boundaryCell.i + 1] [boundaryCell.j + 1].obstacleBelongTo = -1;
						}
					}
					if (changedRB) {
						if (boundaryCell.i < imax && boundaryCell.j > 0 && ((obs [boundaryCell.i + 1] [boundaryCell.j].obstacleBelongTo != -1
							&& obs [boundaryCell.i + 1] [boundaryCell.j].obstacleBelongTo != 0
							&& obs [boundaryCell.i + 1] [boundaryCell.j].obstacleBelongTo != boundaryCell.obstacleBelongTo)
							|| (obs [boundaryCell.i] [boundaryCell.j - 1].obstacleBelongTo != -1
							&& obs [boundaryCell.i] [boundaryCell.j - 1].obstacleBelongTo != 0
							&& obs [boundaryCell.i] [boundaryCell.j - 1].obstacleBelongTo != boundaryCell.obstacleBelongTo))) {
							obs [boundaryCell.i + 1] [boundaryCell.j - 1].obstacleBelongTo = -1;
						}
					}
					if (changedLB) {
						if (boundaryCell.i > 0 && boundaryCell.j > 0 && ((obs [boundaryCell.i - 1] [boundaryCell.j].obstacleBelongTo != -1
							&& obs [boundaryCell.i - 1] [boundaryCell.j].obstacleBelongTo != 0
							&& obs [boundaryCell.i - 1] [boundaryCell.j].obstacleBelongTo != boundaryCell.obstacleBelongTo)
							|| (obs [boundaryCell.i] [boundaryCell.j - 1].obstacleBelongTo != -1
							&& obs [boundaryCell.i] [boundaryCell.j - 1].obstacleBelongTo != 0
							&& obs [boundaryCell.i] [boundaryCell.j - 1].obstacleBelongTo != boundaryCell.obstacleBelongTo))) {
							obs [boundaryCell.i - 1] [boundaryCell.j - 1].obstacleBelongTo = -1;;
						}
					}
					boundaryContour.RemoveAt (0);
					changedLT = changedRT = changedRB = changedLB = false;
				}
			}
		}
	}
	
	private void calculateFreeCells (GameObject floor)
	{
		for (int i = 0; i < imax + 1; i++) {
			for (int j = 0; j < jmax + 1; j++) {
				if (obs [i] [j].obstacleBelongTo == -1) {
					freeCells.Add (obs [i] [j]);
				}
			}	
		}
	}
	
	public void extractRoadmaps (GameObject floor) 
	{
		imax = (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x);
		jmax = (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y);
		
		constructDictionary ();
		
		for (int i = 0; i < imax + 1; i++) {
			for (int j = 0; j < jmax + 1; j++) {
				if (obs [i] [j].obstacleBelongTo == 0) {
					continue;
				}
				RoadmapNode root = new RoadmapNode (-1, -1, -1, -1);
				depthFirstSearchForRoadmaps (i, j, i, j + 1, root);
				depthFirstSearchForRoadmaps (i, j, i + 1, j, root);
				if (root.children.Count != 0) {
					roadmapNodesList.Add (root);
				}
			}
		}
	}
	
	private void constructDictionary () 
	{
		for (int i = 0; i < imax + 1; i++) {
			for (int j = 0; j < jmax + 1; j++) {
				RoadmapNode rn1 = new RoadmapNode (i, j, i, j + 1);
				RoadmapNode rn2 = new RoadmapNode (i, j, i + 1, j);
				int i2 = i + 1, j2 = j + 1;
				roadmapDictionary.Add (i + ", " + j + ", " + i + ", " + j2, rn1);
				roadmapDictionary.Add (i + ", " + j + ", " + i2 + ", " + j, rn2);
			}
		}
	}
	
	private void depthFirstSearchForRoadmaps (int i1, int j1, int i2, int j2, RoadmapNode parent)
	{
		if (roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2].isVisited == true) {
			return;	
		}
		// Guarantee x1 <= x2 && y1 <= y2 to avoid duplicated roadmap nodes
		if ((i2 - i1 == 0) && (j2 - j1 == 1)) {
			if (j1 < jmax && obs [i1] [j1].obstacleBelongTo != obs [i2] [j2].obstacleBelongTo && obs [i2] [j2].obstacleBelongTo != 0) {
				roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2].isVisited = true;
				roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2].parent = parent;
				parent.children.Add (roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);
				// Dig deeper
				depthFirstSearchForRoadmaps (i1 + 1, j1, i1 + 1, j1 + 1, roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);
				depthFirstSearchForRoadmaps (i1 - 1, j1, i1 - 1, j1 + 1, roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);
				depthFirstSearchForRoadmaps (i1 - 1, j1 + 1, i1, j1 + 1, roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);
				depthFirstSearchForRoadmaps (i1, j1 + 1, i1 + 1, j1 + 1, roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);
				depthFirstSearchForRoadmaps (i1 - 1, j1, i1, j1, roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);
				depthFirstSearchForRoadmaps (i1, j1, i1 + 1, j1, roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);			
			}
		}
		if ((i2 - i1 == 1) && (j2 - j1 == 0)) {
			if (i1 < imax && obs [i1] [j1].obstacleBelongTo != obs [i2] [j2].obstacleBelongTo && obs [i2] [j2].obstacleBelongTo != 0) {
				roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2].isVisited = true;
				roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2].parent = parent;
				parent.children.Add (roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);
				// Dig deeper
				depthFirstSearchForRoadmaps (i1, j1 + 1, i1 + 1, j1 + 1, roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);
				depthFirstSearchForRoadmaps (i1, j1 - 1, i1 + 1, j1 - 1, roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);
				depthFirstSearchForRoadmaps (i1, j1, i1, j1 + 1, roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);
				depthFirstSearchForRoadmaps (i1, j1 - 1, i1, j1, roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);
				depthFirstSearchForRoadmaps (i1 + 1, j1, i1 + 1, j1 + 1, roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);
				depthFirstSearchForRoadmaps (i1 + 1, j1 - 1, i1 + 1, j1, roadmapDictionary [i1 + ", " + j1 + ", " + i2 + ", " + j2]);
			}
		}
		return;
	}
	
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
	
	public void selectSuperNodes ()
	{
		foreach (RoadmapNode root in roadmapNodesList) {
			RoadmapNode currentNode = root;
			foreach (RoadmapNode rn in currentNode.children) {
				rn.isKept = true;
				recurse (rn);
			}
		}
	}
	
	// Recurse through the connected roadmap
	private void recurse (RoadmapNode currentRoadmapNode)
	{
		// -
		if (currentRoadmapNode.x1 == currentRoadmapNode.x2 && currentRoadmapNode.y2 - currentRoadmapNode.y1 == 1 && currentRoadmapNode.isChecked == false) {
			// check top-left
			if (roadmapDictionary [(currentRoadmapNode.x2 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].isVisited == true
				&& roadmapDictionary [(currentRoadmapNode.x2 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].isChecked == false) {
				currentRoadmapNode.isKept = true;
				recurseChildren (roadmapDictionary [(currentRoadmapNode.x2 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2]);					
			}
			// check top-right
			if (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2].isChecked == false) {
				currentRoadmapNode.isKept = true;
				recurseChildren (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2]);										
			}
			// check bot-left
			if (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isVisited == true
				&& roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isChecked == false) {
				currentRoadmapNode.isKept = true;
				recurseChildren (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1]);										
			}
			// check bot-right
			if (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1].isChecked == false) {				
				currentRoadmapNode.isKept = true;
				recurseChildren (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1]);										
			}
		}
		// |
		if (currentRoadmapNode.y1 == currentRoadmapNode.y2 && currentRoadmapNode.x2 - currentRoadmapNode.x1 == 1 && currentRoadmapNode.isChecked == false) {
			// check top-left
			if (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)].isChecked == false) {				
				currentRoadmapNode.isKept = true;
				recurseChildren (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)]);					
			}
			// check top-right
			if (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].isChecked == false) {					
				currentRoadmapNode.isKept = true;
				recurseChildren (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)]);										
			}
			// check bot-left
			if (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isChecked == false) {
				currentRoadmapNode.isKept = true;
				recurseChildren (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1]);										
			}
			// check bot-right
			if (roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].isChecked == false) {					
				currentRoadmapNode.isKept = true;
				recurseChildren (roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2]);										
			}
		}
		if (currentRoadmapNode.children.Count == 0) {
			// Assign a node at the end of roadmap
			currentRoadmapNode.isKept = true;
			currentRoadmapNode.isChecked = true;
			return;	
		} else {
			foreach (RoadmapNode rn in currentRoadmapNode.children) {
				currentRoadmapNode.isChecked = true;
				recurse (rn);	
			}
		}
		return;
	}
	
	// Ignore redundant nodes produced by Zig-zag 
	private void recurseChildren (RoadmapNode currentRoadmapNode) 
	{
		// |
		if (currentRoadmapNode.y1 == currentRoadmapNode.y2 && currentRoadmapNode.x2 - currentRoadmapNode.x1 == 1) {
			// Check top-left edge
			if (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)].isChecked == false) {
				currentRoadmapNode.isChecked = true;
				recurseChildren (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)]);					
			}
			// Check top-right edge
			if (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].isChecked == false) {
				currentRoadmapNode.isChecked = true;
				recurseChildren (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)]);										
			}
			// Check bot-left edge
			if (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isChecked == false) {
				currentRoadmapNode.isChecked = true;
				recurseChildren (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1]);										
			}
			// Check bot-right edge
			if (roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].isChecked == false) {
				currentRoadmapNode.isChecked = true;
				recurseChildren (roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2]);										
			}
			// Check top edge
			if (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].isChecked == false) {
				currentRoadmapNode.isChecked = true;
				currentRoadmapNode.isKept = true;
				return;
			}
			// Check bot edge
			if (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1)].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1)].isChecked == false) {
				currentRoadmapNode.isChecked = true;
				currentRoadmapNode.isKept = true;
				return;
			}
			return;
		}
		// -
		if (currentRoadmapNode.x1 == currentRoadmapNode.x2 && currentRoadmapNode.y2 - currentRoadmapNode.y1 == 1) {
			// Check top-left edge
			if (roadmapDictionary [(currentRoadmapNode.x2 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].isVisited == true
				&& roadmapDictionary [(currentRoadmapNode.x2 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].isChecked == false) {
				currentRoadmapNode.isChecked = true;
				recurseChildren (roadmapDictionary [(currentRoadmapNode.x2 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2]);					
			}
			// Check top-right edge
			if (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2].isChecked == false) {
				currentRoadmapNode.isChecked = true;
				recurseChildren (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2]);										
			}
			// Check bot-left edge
			if (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isVisited == true
				&& roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isChecked == false) {
				currentRoadmapNode.isChecked = true;
				recurseChildren (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1]);										
			}
			// Check bot-right edge
			if (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1].isVisited == true
				&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1].isChecked == false) {
				currentRoadmapNode.isChecked = true;
				recurseChildren (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1]);										
			}
			// Check left edge
			if (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x2 - 1) + ", " + currentRoadmapNode.y2].isVisited == true
				&& roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x2 - 1) + ", " + currentRoadmapNode.y2].isChecked == false) {
				currentRoadmapNode.isChecked = true;
				currentRoadmapNode.isKept = true;
				return;
			}
			// Check right edge
			if (roadmapDictionary [(currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2].isVisited == true
				&& roadmapDictionary [(currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2].isChecked == false) {
				currentRoadmapNode.isChecked = true;
				currentRoadmapNode.isKept = true;			
				return;
			}
			return;
		}
		return;
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
