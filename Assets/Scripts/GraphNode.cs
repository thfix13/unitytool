using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GraphNode
{
	public int x1, y1, x2, y2;
	public bool isVisited = false;
	public bool isTail = false;
	public List<GraphNode> neighbors = new List<GraphNode> ();
	
	public GraphNode ()
	{
		this.x1 = 0;
		this.y1 = 0;
		this.x2 = 0;
		this.y2 = 0;
	}
	public Vector3 Pos(GameObject floor)
	{
		float posX=0, posY=0;
		if (x1 == x2 && y2 - y1 == 1) {
			posX = SpaceState.TileSize.x * x1 + SpaceState.TileSize.x / 2.0f + floor.collider.bounds.min.x;
			posY = SpaceState.TileSize.y * y1 + SpaceState.TileSize.y + floor.collider.bounds.min.z;
		}
		if (y1 == y2 && x2 - x1 == 1) {
			posX = SpaceState.TileSize.x * x2 + floor.collider.bounds.min.x;
			posY = SpaceState.TileSize.y * y1 + SpaceState.TileSize.y / 2.0f + floor.collider.bounds.min.z;
		}
		return new Vector3 (posX, 0f, posY);
	}

	public GraphNode (int i1, int j1, int i2, int j2)
	{
		this.x1 = i1;
		this.y1 = j1;
		this.x2 = i2;
		this.y2 = j2;
	}
	
	public void setIndice (int i1, int j1, int i2, int j2)
	{
		this.x1 = i1;
		this.y1 = j1;
		this.x2 = i2;
		this.y2 = j2;
	}
	
	public int distance (GraphNode gn)
	{
		return (Mathf.Abs (gn.x1 - this.x1) + Mathf.Abs (gn.y1 - this.y1) + Mathf.Abs (gn.x2 - this.x2) + Mathf.Abs (gn.y2 - this.y2)) / 2;	
	}
}
