using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoadmapNode
{
	public int x1, y1, x2, y2;
	public float posX, posY;
	public bool isKept = false;
	public bool isVisited = false;
	public bool isChecked = false;
	
	// Parenting
	public RoadmapNode parent = null;
	public List<RoadmapNode> children = new List<RoadmapNode> ();
	
	public RoadmapNode (int i1, int j1, int i2, int j2)
	{
		// Two neighbors' indice
		this.x1 = i1;
		this.y1 = j1;
		this.x2 = i2;
		this.y2 = j2;
		
		// Line segment's midpoint position
		this.posX = SpaceState.TileSize.x * (i1 + i2) / 2.0f + SpaceState.TileSize.x / 2.0f;
		this.posY = SpaceState.TileSize.y * (j1 + j2) / 2.0f + SpaceState.TileSize.y / 2.0f; 
	}
}
