using System;
using System.Collections.Generic;
using UnityEngine;
using Common;
using Exploration;
using Objects;

public class Geometry
{
	//public Vector3 [] vertex = new Vector3[4]; 
	public List<Line> edges = new List<Line> ();
	


	public void DrawGeometry(GameObject parent)
	{
		Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
                           UnityEngine.Random.Range(0.0f,1.0f),
                           UnityEngine.Random.Range(0.0f,1.0f)) ;
	}
}
