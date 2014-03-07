using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GraphNode
{
	public int x1, y1, x2, y2;
	public List<GraphNode> neighbors = new List<GraphNode> ();
	
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
