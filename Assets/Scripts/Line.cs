using UnityEngine;
using System.Collections;

public class Line 
{

	public Vector3[] vertex = new Vector3[2];

	public Line(Vector3 v1, Vector3 v2)
	{
		vertex[0] = v1; 
		vertex[1] = v2; 
	}
}
