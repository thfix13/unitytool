using System;
using UnityEngine;

[System.Serializable]
public class Cell
{
	public bool blocked = false;
	public bool seen = false;
	public bool safe = false;
	public bool noisy = false;
	public bool waypoint = false;
	public int nearestVoronoiCentre = -1;
	// if blocked => isVoronoiBoundary = -1; if not blocked && not boundary => isVoronoiBoundary = 0; if not block && boundary => isVoronoiBoundary = 1;
	public int isVoronoiBoundary = -1;
	// if is wall => isNextToWall = -1; if is not next to wall => isNextToWall = 0; if next to wall => isNextToWall = 1;
	public int isNextToWall = -1;
	// if is visited => isVisited = true
	public bool visited = false;
	public bool node = false; 

	public Boolean IsWalkable ()
	{
		return safe || (!(blocked || seen));
	}
	
	public Cell Copy ()
	{
		Cell copy = new Cell ();
		copy.blocked = this.blocked;
		copy.seen = this.seen;
		copy.safe = this.safe;
		copy.safe = this.safe;
		copy.waypoint = this.waypoint;
		copy.nearestVoronoiCentre = this.nearestVoronoiCentre;
		copy.isVoronoiBoundary = this.isVoronoiBoundary;
		copy.isNextToWall = this.isNextToWall;
		copy.visited = this.visited;
		return copy;
	}
}
