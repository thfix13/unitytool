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
	
	// Graph Stuff
	public List<GraphNode> graphNodesList = new List<GraphNode> ();
	public List<GraphNode> finalGraphNodesList = new List<GraphNode> ();
	
	public void identifyObstacleContours (GameObject floor)
	{
		imax = (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x);
		jmax = (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y);
		for (int i = 0; i < imax + 1; i++) {
			for (int j = 0; j < jmax + 1; j++) {
				boundaryIndex++;
				depthFirstSearchForObstacles (i, j, boundaryIndex);
				if (boundaryContoursList.Count != boundaryIndex) {
					boundaryIndex--;	
				}
			}	
		}
	}
	
	private void depthFirstSearchForObstacles (int i, int j, int boundaryIndex)
	{
		// Skip if the node has been already visited
		if (obs [i] [j].obsVisited) {
			return;	
		}
		
		if ((obs [i] [j].blocked == true || (obs [i] [j].blocked == false && (i == 0 || j == 0 || i == imax || j == jmax)))) {
			obs [i] [j].obsVisited = true;	
			
			if ((i > 0 && j > 0 && i < imax && j < jmax) && (!obs [i - 1] [j].blocked || !obs [i + 1] [j].blocked || !obs [i] [j + 1].blocked || !obs [i] [j - 1].blocked
				|| !obs [i - 1] [j + 1].blocked || !obs [i + 1] [j + 1].blocked || !obs [i + 1] [j - 1].blocked || !obs [i - 1] [j - 1].blocked)) {
				obs [i] [j].obstacleBelongTo = boundaryIndex;
				// Add to boundary contours list
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
			if (j == 0 && i > 0 && i < imax && (!obs [i - 1] [j].blocked || !obs [i + 1] [j].blocked || !obs [i] [j + 1].blocked)) {
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
				&& (!obs [i - 1] [j].blocked || !obs [i] [j - 1].blocked || !obs [i] [j + 1].blocked)) {
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
				&& (!obs [i - 1] [j].blocked || !obs [i] [j - 1].blocked || !obs [i + 1] [j].blocked)) {
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
			
			// Not boundary but blocked, set obstacleBelongTo to 0
			if (i > 0 && j > 0 && i < imax && j < jmax 
				&& obs [i - 1] [j].blocked && obs [i - 1] [j + 1].blocked && obs [i] [j + 1].blocked
				&& obs [i + 1] [j + 1].blocked && obs [i + 1] [j].blocked && obs [i + 1] [j - 1].blocked
				&& obs [i] [j - 1].blocked && obs [i - 1] [j - 1].blocked) {
				obs [i] [j].obstacleBelongTo = 0;	
			}
			if (i == 0 && j > 0 && j < jmax
				&& obs [i] [j + 1].blocked && obs [i + 1] [j + 1].blocked && obs [i + 1] [j].blocked && obs [i + 1] [j - 1].blocked
				&& obs [i] [j - 1].blocked) {
				obs [i] [j].obstacleBelongTo = 0;	
			}
			if (j == 0 && i > 0 && i < imax
				&& obs [i - 1] [j].blocked && obs [i - 1] [j + 1].blocked && obs [i] [j + 1].blocked
				&& obs [i + 1] [j + 1].blocked && obs [i + 1] [j].blocked) {
				obs [i] [j].obstacleBelongTo = 0;
			}
			if (i == imax && j > 0 && j < jmax
				&& obs [i - 1] [j].blocked && obs [i - 1] [j + 1].blocked && obs [i] [j + 1].blocked
				&& obs [i] [j - 1].blocked && obs [i - 1] [j - 1].blocked) {
				obs [i] [j].obstacleBelongTo = 0;
			}
			if (j == jmax && i > 0 && i < imax
				&& obs [i - 1] [j].blocked && obs [i + 1] [j].blocked && obs [i + 1] [j - 1].blocked
				&& obs [i] [j - 1].blocked && obs [i - 1] [j - 1].blocked) {
				obs [i] [j].obstacleBelongTo = 0;
			}
			if (i == 0 && j == 0 && obs [i] [j + 1].blocked && obs [i + 1] [j + 1].blocked && obs [i + 1] [j].blocked) {
				obs [i] [j].obstacleBelongTo = 0;
			}
			if (i == 0 && j == jmax && obs [i] [j - 1].blocked && obs [i + 1] [j - 1].blocked && obs [i] [j - 1].blocked) {
				obs [i] [j].obstacleBelongTo = 0;
			}
			if (i == imax && j == jmax && obs [i - 1] [j].blocked && obs [i - 1] [j - 1].blocked && obs [i] [j - 1].blocked) {
				obs [i] [j].obstacleBelongTo = 0;
			}
			if (i == imax && j == 0 && obs [i - 1] [j].blocked && obs [i - 1] [j + 1].blocked && obs [i] [j + 1].blocked) {
				obs [i] [j].obstacleBelongTo = 0;
			}
		}
		return;
	}
	
	public void boundaryContoursFlooding (GameObject floor)
	{
		imax = (int)(floor.collider.bounds.size.x / SpaceState.TileSize.x);
		jmax = (int)(floor.collider.bounds.size.z / SpaceState.TileSize.y);
		
		// Gather all the free cells with obstacleBelongTo = -1
		calculateFreeCells (floor);
		while (freeCells.Count != 0) {
			foreach (List<Cell> boundaryContour in boundaryContoursList) {
				int numOfCells = boundaryContour.Count;
				bool changedLT = false, changedRT = false, changedRB = false, changedLB = false;
				for (int cnt = 0; cnt < numOfCells; cnt++) {
					Cell boundaryCell = boundaryContour.First ();
					// Left
					if (boundaryCell.i > 0 && obs [boundaryCell.i - 1] [boundaryCell.j].obstacleBelongTo == -1) {
						obs [boundaryCell.i - 1] [boundaryCell.j].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i - 1] [boundaryCell.j]);
						freeCells.Remove (obs [boundaryCell.i - 1] [boundaryCell.j]);
					}
					// Left-top
					if (boundaryCell.i > 0 && boundaryCell.j < jmax && obs [boundaryCell.i - 1] [boundaryCell.j + 1].obstacleBelongTo == -1) {
						changedLT = true;
						obs [boundaryCell.i - 1] [boundaryCell.j + 1].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i - 1] [boundaryCell.j + 1]);
						freeCells.Remove (obs [boundaryCell.i - 1] [boundaryCell.j + 1]);
					}
					// Top
					if (boundaryCell.j < jmax && obs [boundaryCell.i] [boundaryCell.j + 1].obstacleBelongTo == -1) {
						obs [boundaryCell.i] [boundaryCell.j + 1].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i] [boundaryCell.j + 1]);
						freeCells.Remove (obs [boundaryCell.i] [boundaryCell.j + 1]);
					}
					// Right-top
					if (boundaryCell.i < imax && boundaryCell.j < jmax && obs [boundaryCell.i + 1] [boundaryCell.j + 1].obstacleBelongTo == -1) {
						changedRT = true;
						obs [boundaryCell.i + 1] [boundaryCell.j + 1].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i + 1] [boundaryCell.j + 1]);
						freeCells.Remove (obs [boundaryCell.i + 1] [boundaryCell.j + 1]);
					}
					// Right
					if (boundaryCell.i < imax && obs [boundaryCell.i + 1] [boundaryCell.j].obstacleBelongTo == -1) {
						obs [boundaryCell.i + 1] [boundaryCell.j].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i + 1] [boundaryCell.j]);
						freeCells.Remove (obs [boundaryCell.i + 1] [boundaryCell.j]);
					}
					// Right-bottom
					if (boundaryCell.i < imax && boundaryCell.j > 0 && obs [boundaryCell.i + 1] [boundaryCell.j - 1].obstacleBelongTo == -1) {
						changedRB = true;
						obs [boundaryCell.i + 1] [boundaryCell.j - 1].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i + 1] [boundaryCell.j - 1]);	
						freeCells.Remove (obs [boundaryCell.i + 1] [boundaryCell.j - 1]);
					}
					// Bottom
					if (boundaryCell.j > 0 && obs [boundaryCell.i] [boundaryCell.j - 1].obstacleBelongTo == -1) {
						obs [boundaryCell.i] [boundaryCell.j - 1].obstacleBelongTo = boundaryCell.obstacleBelongTo;
						boundaryContour.Add (obs [boundaryCell.i] [boundaryCell.j - 1]);
						freeCells.Remove (obs [boundaryCell.i] [boundaryCell.j - 1]);
					}
					// Left-bottom
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
							obs [boundaryCell.i - 1] [boundaryCell.j - 1].obstacleBelongTo = -1;
							;
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
			if (j1 < jmax && obs [i1] [j1].obstacleBelongTo != obs [i2] [j2].obstacleBelongTo && obs [i2] [j2].obstacleBelongTo != 0 && obs [i1] [j1].obstacleBelongTo != 0) {
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
		// Guarantee x1 <= x2 && y1 <= y2 to avoid duplicated roadmap nodes
		if ((i2 - i1 == 1) && (j2 - j1 == 0)) {
			if (i1 < imax && obs [i1] [j1].obstacleBelongTo != obs [i2] [j2].obstacleBelongTo && obs [i2] [j2].obstacleBelongTo != 0 && obs [i1] [j1].obstacleBelongTo != 0) {
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
		// foreach contour
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
			if (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].isVisited == true) {
				currentRoadmapNode.isChecked = true;
				currentRoadmapNode.isKept = true;
				return;
			}
			// Check bot edge
			if (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1)].isVisited == true) {
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
			if (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x2 - 1) + ", " + currentRoadmapNode.y2].isVisited == true) {
				currentRoadmapNode.isChecked = true;
				currentRoadmapNode.isKept = true;
				return;
			}
			// Check right edge
			if (roadmapDictionary [(currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2].isVisited == true) {
				currentRoadmapNode.isChecked = true;
				currentRoadmapNode.isKept = true;			
				return;
			}
			return;
		}
		return;
	}
	
	public void initializeGraph ()
	{
		// Collect all graph nodes
		foreach (RoadmapNode root in roadmapNodesList) {
			RoadmapNode currentNode = root;
			foreach (RoadmapNode rn in currentNode.children) {
				GraphNode gn = new GraphNode (rn.x1, rn.y1, rn.x2, rn.y2);
				graphNodesList.Add (gn);
				depthFirstSearchForGraphNodes (rn, gn);
			}
		}
		
		// Cure for cross
		foreach (GraphNode gn in graphNodesList) {
			// -
			if (gn.x1 == gn.x2 && gn.y2 - gn.y1 == 1) {
				GraphNode topNode = null, botNode = null, rightNode = null;
				int topExist = 0, botExist = 0, rightExist = 0;
				foreach (GraphNode sgn in graphNodesList) {
					if (sgn.y1 == sgn.y2 && sgn.x2 - sgn.x1 == 1 && sgn.x1 == gn.x1 && sgn.y1 == gn.y2) {
						topNode = sgn;
						topExist = 1;
					}
					if (sgn.y1 == sgn.y2 && sgn.x2 - sgn.x1 == 1 && sgn.x1 == gn.x1 && sgn.y1 == gn.y1) {
						botNode = sgn;
						botExist = 1;
					}
				}
				foreach (GraphNode gnn in gn.neighbors) {
					if (gnn.x1 == gnn.x2 && gnn.y2 - gnn.y1 == 1 && gnn.x1 - gn.x1 == 1) {
						rightNode = gnn;
						rightExist = 1;
					}
					if (gnn == topNode && topExist == 1) {
						topExist = 2;	
					}
					if (gnn == botNode && botExist == 1) {
						botExist = 2;	
					}
				}
				if (rightNode != null) {
					foreach (GraphNode gnnn in rightNode.neighbors) {
						if (gnnn == topNode) {
							topExist = 3;
						}
						if (gnnn == botNode) {
							botExist = 3;	
						}
					}
				}
				if (topExist == 1 && botExist == 1 && rightExist == 1) {
					gn.neighbors.Add (botNode);
					botNode.neighbors.Add (gn);
				}
			}
		}
		
		// Reconstruct a graph
		foreach (GraphNode gn in graphNodesList) {
			GraphNode graphNode = new GraphNode (gn.x1, gn.y1, gn.x2, gn.y2);
			finalGraphNodesList.Add (graphNode);
		}
		foreach (GraphNode gn in graphNodesList) {
			foreach (GraphNode neighbor in gn.neighbors) {
				foreach (GraphNode finalGraphNode in finalGraphNodesList) {
					if (finalGraphNode.x1 == gn.x1 && finalGraphNode.y1 == gn.y1 && finalGraphNode.x2 == gn.x2 && finalGraphNode.y2 == gn.y2) {
						foreach (GraphNode finalNeighbor in finalGraphNodesList) {
							if (finalNeighbor.x1 == neighbor.x1 && finalNeighbor.y1 == neighbor.y1 && finalNeighbor.x2 == neighbor.x2 && finalNeighbor.y2 == neighbor.y2) {
								if (!finalGraphNode.neighbors.Contains (finalNeighbor)) {
									finalGraphNode.neighbors.Add (finalNeighbor);
								}
								if (!finalNeighbor.neighbors.Contains (finalGraphNode)) {
									finalNeighbor.neighbors.Add (finalGraphNode);
								}
							}
						}
		
					}
				}
			}
		}
	
		foreach (GraphNode currentGraphNode in finalGraphNodesList) {
			// Is a tail node
			if (currentGraphNode.neighbors.Count == 1) {
				currentGraphNode.isTail = true;
				// Debug.Log ("Cur: x1:" + currentGraphNode.x1 + ", y1: " + currentGraphNode.y1 + ", x2: " + currentGraphNode.x2 + ", y2: " + currentGraphNode.y2);	
				int minDist = int.MaxValue;
				GraphNode nearestNode = null;
				GraphNode theOnlyNeighbor = currentGraphNode.neighbors.ElementAt (0);
				int xVector = theOnlyNeighbor.x1 - currentGraphNode.x1;
				if (xVector == 0) {
					xVector = theOnlyNeighbor.x2 - currentGraphNode.x2;	
				}
				if (xVector > 0) {
					foreach (GraphNode gn in finalGraphNodesList) {
						if (gn.x1 - currentGraphNode.x1 < 0 || gn.x2 - currentGraphNode.x2 < 0) {
							int tempDist = gn.distance (currentGraphNode);
							if (tempDist < minDist) {
								minDist = tempDist;
								nearestNode = gn;
							}
						}
					}
				}
				if (xVector < 0) {
					foreach (GraphNode gn in finalGraphNodesList) {
						if (gn.x1 - currentGraphNode.x1 > 0 || gn.x2 - currentGraphNode.x2 > 0) {
							int tempDist = gn.distance (currentGraphNode);
							if (tempDist < minDist) {
								minDist = tempDist;
								nearestNode = gn;
							}
						}
					}					
				}
				if (xVector == 0) {
					int yVector = theOnlyNeighbor.y1 - currentGraphNode.y1;
					if (yVector == 0) {
						yVector = theOnlyNeighbor.y2 - currentGraphNode.y2;	
					}
					if (yVector > 0) {
						foreach (GraphNode gn in finalGraphNodesList) {
							if (gn.y1 - currentGraphNode.y1 < 0 || gn.y2 - currentGraphNode.y2 < 0) {
								int tempDist = gn.distance (currentGraphNode);
								if (tempDist < minDist) {
									minDist = tempDist;
									nearestNode = gn;
								}
							}
						}
					}
					if (yVector < 0) {
						foreach (GraphNode gn in finalGraphNodesList) {
							if (gn.y1 - currentGraphNode.y1 > 0 || gn.y2 - currentGraphNode.y2 > 0) {
								int tempDist = gn.distance (currentGraphNode);
								if (tempDist < minDist) {
									minDist = tempDist;
									nearestNode = gn;
								}
							}
						}					
					}					
				}
				if (nearestNode != null) {
					currentGraphNode.neighbors.Add (nearestNode);
					nearestNode.neighbors.Add (currentGraphNode);
				}
			}
		}
	}
	
	private void depthFirstSearchForGraphNodes (RoadmapNode currentRoadmapNode, GraphNode prevGraphNode)
	{
		RoadmapNode parentRoadmapNode = currentRoadmapNode.parent;
		//Debug.Log ("Prev: x1:" + prevGraphNode.x1 + ", y1: " + prevGraphNode.y1 + ", x2: " + prevGraphNode.x2 + ", y2: " + prevGraphNode.y2);		
		//Debug.Log ("Cur: x1:" + currentRoadmapNode.x1 + ", y1: " + currentRoadmapNode.y1 + ", x2: " + currentRoadmapNode.x2 + ", y2: " + currentRoadmapNode.y2);		
		GraphNode gn = new GraphNode (prevGraphNode.x1, prevGraphNode.y1, prevGraphNode.x2, prevGraphNode.y2);
		if (parentRoadmapNode.x1 != -1 && parentRoadmapNode.y1 != -1 && parentRoadmapNode.x2 != -1 && parentRoadmapNode.y2 != -1) {
			if (currentRoadmapNode.isKept == true) {
				gn.setIndice (currentRoadmapNode.x1, currentRoadmapNode.y1, currentRoadmapNode.x2, currentRoadmapNode.y2);
				graphNodesList.Add (gn);
				gn.neighbors.Add (prevGraphNode);
				prevGraphNode.neighbors.Add (gn);
				currentRoadmapNode.previous = gn;
				gn.isVisited = true;
				
				// Deal with corner cases leading to wrong previous graphnode
				// -
				if (currentRoadmapNode.x1 == currentRoadmapNode.x2 && currentRoadmapNode.y2 - currentRoadmapNode.y1 == 1) {
					if (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y2].isKept == false 
						&& roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y2].isVisited == true) {
						roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y2].previous = gn;					
					}
					if (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2].isKept == false
						&& roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2].isVisited == true) {
						roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2].previous = gn;
					}
					if (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isKept == false
						 && roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isVisited == true) {
						roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].previous = gn;
					}
					if (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1].isKept == false
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1].isVisited == true) {
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1].previous = gn;
					}
					if (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2].isKept == false
						&& roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2].isVisited == true) {
						roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2].previous = gn;	
					}
					if (roadmapDictionary [(currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y2].isKept == false
						&& roadmapDictionary [(currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y2].isVisited == true) {
						roadmapDictionary [(currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y2].previous = gn;	
					}
				}
				// |
				if (currentRoadmapNode.x2 - currentRoadmapNode.x1 == 1 && currentRoadmapNode.y1 == currentRoadmapNode.y2) {
					if (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)].isKept == false
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)].isVisited == true) {
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)].previous = gn;
					}
					if (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isKept == false
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isVisited == true) {
						roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].previous = gn;
					}
					if (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].isKept == false
						&& roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].isVisited == true) {
						roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].previous = gn;
					}
					if (roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].isKept == false
						&& roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].isVisited == true) {
						roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].previous = gn;
					}
					if (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].isKept == false
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].isVisited == true) {
						roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].previous = gn;					
					}	
					if (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1)].isKept == false
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1)].isVisited == true) {
						roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1)].previous = gn;	
					}			
				}
			}
			
			// 
			if (currentRoadmapNode.isKept == false) {
				// Deal with corner cases leading to wrong previous graphnode
				// -
				if (currentRoadmapNode.x1 == currentRoadmapNode.x2 && currentRoadmapNode.y2 - currentRoadmapNode.y1 == 1) {
					if (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y2].isKept == true 
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0)
						!= roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y2]
						&& (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].parent != roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y2])) {
//						|| roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.Contains (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y2]))) {
						roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y2].previous = new GraphNode ();
						roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y2].previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
						gn.setIndice (currentRoadmapNode.x1 - 1, currentRoadmapNode.y2, currentRoadmapNode.x1, currentRoadmapNode.y2);					
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
					}
					if (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2].isKept == true
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0)
						!= roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2]
						&& (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].parent != roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2])) {
//						|| roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.Contains (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2]))) {
						roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2].previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + (currentRoadmapNode.x2 + 1) + ", " + currentRoadmapNode.y2].previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
						gn.setIndice (currentRoadmapNode.x2, currentRoadmapNode.y2, currentRoadmapNode.x2 + 1, currentRoadmapNode.y2);
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);					
					}
					if (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isKept == true
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0)
						!= roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1]
						&& (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].parent != roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1])) {
//						|| roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.Contains (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1]))) {
						roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].previous = new GraphNode ();
						roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
						gn.setIndice (currentRoadmapNode.x1 - 1, currentRoadmapNode.y1, currentRoadmapNode.x1, currentRoadmapNode.y1);
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);										
					}
					if (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1].isKept == true
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0)
						!= roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1]
						&& (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].parent != roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1])) {
//						|| roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.Contains (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1]))) {
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1].previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1].previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
						gn.setIndice (currentRoadmapNode.x1, currentRoadmapNode.y1, currentRoadmapNode.x1 + 1, currentRoadmapNode.y1);
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);					
					}
					if (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2].isKept == true
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0)
						!= roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2]
						&& (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].parent != roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2])) {
//						|| roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.Contains (roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2]))) {
						roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2].previous = new GraphNode ();
						roadmapDictionary [(currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 - 1) + ", " + currentRoadmapNode.y2].previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
						gn.setIndice (currentRoadmapNode.x1 - 1, currentRoadmapNode.y1, currentRoadmapNode.x1 - 1, currentRoadmapNode.y2);
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);					
					}
					if (roadmapDictionary [(currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y2].isKept == true
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0)
						!= roadmapDictionary [(currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y2]
						&& (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].parent != roadmapDictionary [(currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y2])) {
//						|| roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.Contains (roadmapDictionary [(currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y2]))) {
						roadmapDictionary [(currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y2].previous = new GraphNode ();
						roadmapDictionary [(currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y1 + ", " + (currentRoadmapNode.x1 + 1) + ", " + currentRoadmapNode.y2].previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
						gn.setIndice (currentRoadmapNode.x1 + 1, currentRoadmapNode.y1, currentRoadmapNode.x1 + 1, currentRoadmapNode.y2);
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);										
					}
				}
				// |
				if (currentRoadmapNode.x2 - currentRoadmapNode.x1 == 1 && currentRoadmapNode.y1 == currentRoadmapNode.y2) {
					// --------------------
					if (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)].isKept == true
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0)
						!= roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)]
						&& (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].parent != roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)])) {
//						|| roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.Contains (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)]))) {
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)].previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1)].previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
						gn.setIndice (currentRoadmapNode.x1, currentRoadmapNode.y1, currentRoadmapNode.x1, currentRoadmapNode.y1 + 1);					
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
						
					}
					if (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].isKept == true
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0)
						!= roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1]
						&& (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].parent != roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1])) {
//						|| roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.Contains (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1]))) {
						roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1].previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
						gn.setIndice (currentRoadmapNode.x1, currentRoadmapNode.y1 - 1, currentRoadmapNode.x1, currentRoadmapNode.y1);					
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
					}
					if (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].isKept == true
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0)
						!= roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)]
						&& (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].parent != roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)])) {
//						|| roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.Contains (roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)]))) {
						roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2 + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
						gn.setIndice (currentRoadmapNode.x2, currentRoadmapNode.y2, currentRoadmapNode.x2, currentRoadmapNode.y2 + 1);					
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
					}
					if (roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].isKept == true
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0)
						!= roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2]
						&& (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].parent != roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2])) {
//						|| roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.Contains (roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2]))) {
						roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1) + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
						gn.setIndice (currentRoadmapNode.x2, currentRoadmapNode.y2 - 1, currentRoadmapNode.x2, currentRoadmapNode.y2);					
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
					}
					if (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].isKept == true
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0)
						!= roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)]
						&& (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].parent != roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)])) {
//						|| roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.Contains (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)]))) {
						roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].previous = new GraphNode ();	
						roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 + 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 + 1)].previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
						gn.setIndice (currentRoadmapNode.x1, currentRoadmapNode.y1 + 1, currentRoadmapNode.x2, currentRoadmapNode.y2 + 1);					
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
					}	
					if (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1)].isKept == true
						&& roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0) 
						!= roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1)]
						&& (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].parent != roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1)])) {
//						|| roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.Contains (roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1)]))) {
						roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1)].previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + (currentRoadmapNode.y1 - 1) + ", " + currentRoadmapNode.x2 + ", " + (currentRoadmapNode.y2 - 1)].previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
						gn.setIndice (currentRoadmapNode.x1, currentRoadmapNode.y1 - 1, currentRoadmapNode.x2, currentRoadmapNode.y2 - 1);					
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous = new GraphNode ();
						roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].children.ElementAt (0).previous.setIndice (gn.x1, gn.y1, gn.x2, gn.y2);
					}
				}
			}
		}
		if (currentRoadmapNode.children.Count == 0) {
			// Connect tail node to a reasonable intermediate node by flooding
			return;	
		} else {
			foreach (RoadmapNode rn in currentRoadmapNode.children) {
				if (rn.isVisited == true && rn.previous == null) {
					if (roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].previous != null) {
						gn = roadmapDictionary [currentRoadmapNode.x1 + ", " + currentRoadmapNode.y1 + ", " + currentRoadmapNode.x2 + ", " + currentRoadmapNode.y2].previous;
					}
					rn.previous = gn;	
				}
				depthFirstSearchForGraphNodes (rn, rn.previous);	
			}
		}
		return;
	}
	
	public void cleanUp (GameObject floor)
	{
		foreach (GraphNode g in finalGraphNodesList) {
			if (!g.isRemoved) {
				int layerMask = 1 << 8;
				bool seen = true; 
				
				while (seen) {
					seen = false; 
					// Copy current graph node's neighbors to temp
					List<GraphNode> temp = null; 
					GraphNode[] t = new GraphNode[g.neighbors.Count]; 
					g.neighbors.CopyTo (t);
					temp = t.ToList ();
	
					foreach (GraphNode n in g.neighbors) {
						if (n.neighbors.Count == 2) {
							GraphNode m = (g == n.neighbors [0]) ? n.neighbors [1] : n.neighbors [0];
							//ray casting
							Vector3 v1 = g.Pos (floor);
							Vector3 v2 = m.Pos (floor);
							Vector3 dir = v2 - v1;
							float dist = Vector3.Distance (v1, v2) + Mathf.Epsilon;
							// not Collided
							if (! Physics.Raycast (v1, dir, dist, layerMask)) {
								// Insert a new node here to judge whether we should delete n
								// Obtain the mid-point of n and m
								Vector3 v3 = n.Pos (floor);
								Vector3 midpoint = Vector3.Slerp (v3, v2, 0.5f);
								Vector3 dir2 = midpoint - v1;
								float dist2 = Vector3.Distance (v1, midpoint) + Mathf.Epsilon;
								// If the ray towards midpoint goes through any obstacle
								if (!Physics.Raycast (v1, dir2, dist2, layerMask)) {
									seen = true; 
									// Changing neighbors for g
									temp.Remove (n);
									temp.Add (m); 
									// Changing neigbhors for m
									m.neighbors.Remove (n); 
									m.neighbors.Add (g); 
									// Changing neighbors for n
									n.neighbors.Clear (); 
									n.isRemoved = true;
									break; 
								}
							}
						}
					}
					// Update g's neighbors
					g.neighbors = temp; 
				}
			}
		}
		
		// For M.G.S only
//		finalGraphNodesList.ElementAt (28).neighbors.Remove (finalGraphNodesList.ElementAt (30));
//		finalGraphNodesList.ElementAt (30).neighbors.Remove (finalGraphNodesList.ElementAt (28));
		
		return;
	}

	public void clearGraph ()
	{
		obs = null;

		boundaryIndex = 0;
		boundaryContoursList.Clear ();
		freeCells.Clear ();

		graphNodesList.Clear ();
		finalGraphNodesList.Clear ();

		roadmapDictionary.Clear ();
		roadmapNodesList.Clear ();

		graphNodesList.Clear ();
		finalGraphNodesList.Clear ();
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
		    && obs [i] [j].visited == false && obs [i + 1] [j].node == false) {
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
		if (i > 0 && j > 0 && i < imax && j < jmax 
			&& obs [i] [j].nearestVoronoiCentre == obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i - 1] [j].nearestVoronoiCentre 
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i - 1] [j].blocked == false && obs [i + 1] [j].blocked == false
			&& obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
		    && obs [i] [j].visited == false && obs [i - 1] [j].node == false) {
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
		if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i + 1] [j].nearestVoronoiCentre 
			&& obs [i] [j].nearestVoronoiCentre != obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i - 1] [j].blocked == false && obs [i + 1] [j].blocked == false
			&& obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
		    && obs [i] [j].visited == false && obs [i] [j + 1].node == false) {
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
		if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i + 1] [j].nearestVoronoiCentre 
			&& obs [i] [j].nearestVoronoiCentre != obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i - 1] [j].blocked == false && obs [i + 1] [j].blocked == false
			&& obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
		    && obs [i] [j].visited == false && obs [i] [j - 1].node == false) {
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
		if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
		         && obs [i] [j].visited == false && (! obs [i + 1] [j].node || !obs [i] [j + 1].node)) {
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
		if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
		    && obs [i] [j].visited == false && (! obs [i] [j - 1].node || ! obs [i + 1] [j].node)) {
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

		if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
	        && obs [i] [j].visited == false && (! obs [i - 1] [j].node || ! obs [i] [j - 1].node)) {
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
		if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
	        && obs [i] [j].visited == false && (! obs [i - 1] [j].node || ! obs [i] [j + 1].node)) {
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

		if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i - 1] [j].nearestVoronoiCentre == obs [i - 2] [j].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
	        && obs [i] [j].visited == false && ! obs [i - 1] [j].node) {
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
		if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i + 1] [j].nearestVoronoiCentre == obs [i + 2] [j].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
	        && obs [i] [j].visited == false && ! obs [i + 1] [j].node) {
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
		if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j - 1].nearestVoronoiCentre == obs [i] [j - 2].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
	        && obs [i] [j].visited == false && ! obs [i] [j - 1].node) {
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
		if (i > 0 && j > 0 && i < imax && j < jmax
			&& obs [i] [j].nearestVoronoiCentre == obs [i - 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i] [j - 1].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre == obs [i + 1] [j].nearestVoronoiCentre
			&& obs [i] [j].nearestVoronoiCentre != obs [i] [j + 1].nearestVoronoiCentre
			&& obs [i] [j - 1].nearestVoronoiCentre == obs [i] [j - 2].nearestVoronoiCentre
			&& obs [i + 1] [j].blocked == false && obs [i] [j - 1].blocked == false && obs [i] [j + 1].blocked == false
	        && obs [i] [j].visited == false && ! obs [i] [j + 1].node) {
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
		if (i == 0 && j > 0 && j < jmax
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
		if (j == 0 && i > 0 && i < imax
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
