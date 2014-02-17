using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ContourNode
{
	public int i = 0, j = 0;
	public ContourNode parent = null;
	public List<ContourNode> children = new List<ContourNode> ();
	
	public ContourNode (int i, int j)
	{
		this.i = i;
		this.j = j;
	}
}
