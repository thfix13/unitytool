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
				if (obs [i] [j].obsVisited) {
					continue;	
				}
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
		if (obs [i] [j].obsVisited) {
			return;	
		}
		
		if ((obs [i] [j].blocked == true || (obs [i] [j].blocked == false && (i == 0 || j == 0 
			|| i == imax
			|| j == jmax))) && obs [i] [j].obsVisited == false) {
			
			obs [i] [j].obsVisited = true;	
			
			if ((i > 0 && j > 0 && i < imax && j < jmax) && (!obs [i - 1] [j].blocked || !obs [i + 1] [j].blocked || !obs [i] [j + 1].blocked || !obs [i] [j - 1].blocked
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
			
			// Not boundary but blocked
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
		
		// Collect all graph edges
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
	
	public void merge ()
	{
		// Operate on the adjacent list	
//		foreach (GraphNode currentGraphNode in finalGraphNodesList) {
//			foreach (GraphNode otherGraphNode in finalGraphNodesList) {
//				if (otherGraphNode != currentGraphNode) {
//					int distance = currentGraphNode.distance (otherGraphNode);
//					if (distance == 1) {
//						if (!(currentGraphNode.x1 == otherGraphNode.x1 && currentGraphNode.x2 == otherGraphNode.x2) || (currentGraphNode.y1 == otherGraphNode.y1 && currentGraphNode.y2 == otherGraphNode.y2)) {
//						}
//					}
//				}
//			}
//		}
	}
	
	public void cleanUp (GameObject floor)
	{
		// Creating a new list of graph node and a list of key nodes
		List<GraphNode> tempList = new List<GraphNode> ();
//		List<int> keyNodesIndexList = new List<int> ();
		
		foreach (GraphNode gn in finalGraphNodesList) {
			GraphNode newGraphNode = new GraphNode (gn.x1, gn.y1, gn.x2, gn.y2);
			if (gn.isTail) {
				newGraphNode.isTail = true;
			}
			tempList.Add (newGraphNode);
		}
		
		// GraphNode keyNode = new GraphNode (finalGraphNodesList.ElementAt (0).x1, finalGraphNodesList.ElementAt (0).y1, finalGraphNodesList.ElementAt (0).x2, finalGraphNodesList.ElementAt (0).y2);


		foreach (GraphNode g in finalGraphNodesList) {
			int layerMask = 1 << 8;
			bool seen = true; 

			while (seen) {
				seen = false; 
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
							seen = true; 
							//Changing neigbhoor for g
							temp.Remove (n);
							temp.Add (m); 
							//changing neigbhors for m
							m.neighbors.Remove (n); 
							m.neighbors.Add (g); 
							n.neighbors.Clear (); 
							break; 
						}
					}

				}
				g.neighbors = temp; 
			}
		}
		
		// For M.G.S only
		finalGraphNodesList.ElementAt (28).neighbors.Remove (finalGraphNodesList.ElementAt (30));
		finalGraphNodesList.ElementAt (30).neighbors.Remove (finalGraphNodesList.ElementAt (28));
		
		return; 


//		GraphNode prevNode = tempList.ElementAt (0), curNode = tempList.ElementAt (0);
//		float prevPosX = 0.0f, prevPosY = 0.0f, curPosX = 0.0f, curPosY = 0.0f;
//		int nodeIndex = -1, prevIndex = 0, tailIndex = 0;
//		
//		while (nodeIndex < tempList.Count - 2) {
//			nodeIndex ++;
//			// Calculate positions
//			if (prevNode.x1 == prevNode.x2 && prevNode.y2 - prevNode.y1 == 1) {
//				prevPosX = SpaceState.TileSize.x * prevNode.x1 + SpaceState.TileSize.x / 2.0f + floor.collider.bounds.min.x;
//				prevPosY = SpaceState.TileSize.y * prevNode.y1 + SpaceState.TileSize.y + floor.collider.bounds.min.z;
//			}
//			if (prevNode.y1 == prevNode.y2 && prevNode.x2 - prevNode.x1 == 1) {
//				prevPosX = SpaceState.TileSize.x * prevNode.x2 + floor.collider.bounds.min.x;
//				prevPosY = SpaceState.TileSize.y * prevNode.y1 + SpaceState.TileSize.y / 2.0f + floor.collider.bounds.min.z;
//			}
//			if (curNode.x1 == curNode.x2 && curNode.y2 - curNode.y1 == 1) {
//				curPosX = SpaceState.TileSize.x * curNode.x1 + SpaceState.TileSize.x / 2.0f + floor.collider.bounds.min.x;
//				curPosY = SpaceState.TileSize.y * curNode.y1 + SpaceState.TileSize.y + floor.collider.bounds.min.z;
//			}
//			if (curNode.y1 == curNode.y2 && curNode.x2 - curNode.x1 == 1) {
//				curPosX = SpaceState.TileSize.x * curNode.x2 + floor.collider.bounds.min.x;
//				curPosY = SpaceState.TileSize.y * curNode.y1 + SpaceState.TileSize.y / 2.0f + floor.collider.bounds.min.z;
//			}
//			if (finalGraphNodesList.ElementAt (nodeIndex).isTail && nodeIndex != 0) {
//				Debug.Log ("Hey! I am here!");
//				Debug.Log ("x1: " + finalGraphNodesList.ElementAt (nodeIndex).x1 + " ,y1: " + finalGraphNodesList.ElementAt (nodeIndex).y1);
//				tailIndex = nodeIndex;
//				prevIndex = tempList.IndexOf (prevNode);
//				if (prevIndex + 1 < nodeIndex) {
//					for (int id = prevIndex + 1; id < nodeIndex; id ++) {
//						finalGraphNodesList.ElementAt (id).neighbors.Clear ();
//					}
//					finalGraphNodesList.ElementAt (prevIndex).neighbors.Remove (finalGraphNodesList.ElementAt(prevIndex + 1));
//					finalGraphNodesList.ElementAt (nodeIndex).neighbors.Remove (finalGraphNodesList.ElementAt (nodeIndex - 1));
//					finalGraphNodesList.ElementAt (prevIndex).neighbors.Add (finalGraphNodesList.ElementAt (nodeIndex));
//					finalGraphNodesList.ElementAt (nodeIndex).neighbors.Add (finalGraphNodesList.ElementAt (prevIndex));					
//				}
//				prevIndex = keyNodesIndexList.Last ();
//				keyNodesIndexList.Remove (keyNodesIndexList.Last ());
//				prevNode = tempList.ElementAt (prevIndex);
//				curNode = tempList.ElementAt (nodeIndex + 1);
//			} else {
//				if (curNode.x1 != prevNode.x1 || curNode.y1 != prevNode.y1 || curNode.x2 != prevNode.x2 || curNode.y2 != prevNode.y2) {
//					Vector3 v1 = new Vector3 (prevPosX, 0f, prevPosY);
//					Vector3 v2 = new Vector3 (curPosX, 0f, curPosY);
//					Vector3 dir = new Vector3 (v2.x - v1.x, v2.y - v1.y, v2.z - v1.z);
//					float dist = Vector3.Distance (v1, v2);
//					// blocked
//			 		if (Physics.Raycast (v1, dir, dist)) {
//						if (finalGraphNodesList.ElementAt (nodeIndex).neighbors.Count == 3) {
//							// Put it into key nodes list
//							if (!keyNodesIndexList.Contains (nodeIndex)) {
//								keyNodesIndexList.Add (nodeIndex);
//							}
//							prevIndex = tempList.IndexOf (prevNode);
//							prevNode = tempList.ElementAt (nodeIndex);
//							curNode = tempList.ElementAt (nodeIndex + 1);
//							int lastIndex = tailIndex > (prevIndex + 1) ? tailIndex : (prevIndex + 1);
//							if (lastIndex < nodeIndex - 1) {
//								for (int id = lastIndex; id < nodeIndex - 1; id ++) {
//									finalGraphNodesList.ElementAt (id).neighbors.Clear ();
//								}
//								finalGraphNodesList.ElementAt (prevIndex).neighbors.Remove (finalGraphNodesList.ElementAt(prevIndex + 1));
//								finalGraphNodesList.ElementAt (nodeIndex - 1).neighbors.Remove (finalGraphNodesList.ElementAt (nodeIndex - 2));
//								finalGraphNodesList.ElementAt (prevIndex).neighbors.Add (finalGraphNodesList.ElementAt (nodeIndex - 1));
//								finalGraphNodesList.ElementAt (nodeIndex - 1).neighbors.Add (finalGraphNodesList.ElementAt (prevIndex));
//							}
//						} else {
//							prevIndex = tempList.IndexOf (prevNode);
//							prevNode = tempList.ElementAt (nodeIndex - 1);
//							curNode = tempList.ElementAt (nodeIndex);
//							// Delete indice between prevIndex and nodeIndex - 1
//							int lastIndex = tailIndex > (prevIndex + 1) ? tailIndex : (prevIndex + 1);
//							if (lastIndex < nodeIndex - 1) {
//								for (int id = lastIndex; id < nodeIndex - 1; id ++) {
//									finalGraphNodesList.ElementAt (id).neighbors.Clear ();
//								}
//								finalGraphNodesList.ElementAt (prevIndex).neighbors.Remove (finalGraphNodesList.ElementAt(prevIndex + 1));
//								finalGraphNodesList.ElementAt (nodeIndex - 1).neighbors.Remove (finalGraphNodesList.ElementAt (nodeIndex - 2));
//								finalGraphNodesList.ElementAt (prevIndex).neighbors.Add (finalGraphNodesList.ElementAt (nodeIndex - 1));
//								finalGraphNodesList.ElementAt (nodeIndex - 1).neighbors.Add (finalGraphNodesList.ElementAt (prevIndex));
//							}
//							nodeIndex --;
//						}
//					} else {
//						curNode = tempList.ElementAt (nodeIndex + 1);
//						if (finalGraphNodesList.ElementAt (nodeIndex).neighbors.Count == 3) {
//							// Put it into key nodes list
//							if (!keyNodesIndexList.Contains (nodeIndex)) {
//								keyNodesIndexList.Add (nodeIndex);
//							}
//							prevIndex = tempList.IndexOf (prevNode);
//							prevNode = tempList.ElementAt (nodeIndex);
//							curNode = tempList.ElementAt (nodeIndex + 1);
//							int lastIndex = tailIndex > (prevIndex + 1) ? tailIndex : (prevIndex + 1);
//							if (lastIndex < nodeIndex) {
//								for (int id = lastIndex; id < nodeIndex; id ++) {
//									finalGraphNodesList.ElementAt (id).neighbors.Clear ();
//								}
//								finalGraphNodesList.ElementAt (prevIndex).neighbors.Remove (finalGraphNodesList.ElementAt(prevIndex + 1));
//								finalGraphNodesList.ElementAt (nodeIndex).neighbors.Remove (finalGraphNodesList.ElementAt (nodeIndex - 1));
//								finalGraphNodesList.ElementAt (prevIndex).neighbors.Add (finalGraphNodesList.ElementAt (nodeIndex));
//								finalGraphNodesList.ElementAt (nodeIndex).neighbors.Add (finalGraphNodesList.ElementAt (prevIndex));
//							}
//						} 
//					}
//				} else {
//					curNode = tempList.ElementAt (nodeIndex + 1);
//				}
//			}
//		}
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
