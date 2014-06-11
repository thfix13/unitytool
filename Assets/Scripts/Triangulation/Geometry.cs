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

		foreach(Line l in edges)
			l.DrawVector(parent,c);
	}

	public bool Collision(Geometry g)
	{
		foreach(Line l1 in edges)
		{
			foreach(Line l2 in g.edges)
			{
				if(l1 == l2)
					continue;
				if(l1.LineIntersection(l2))
					return true; 
			}
		}
		return false; 
	}
	public void CollisionDraw(Geometry g, GameObject parent)
	{
		foreach(Line l1 in edges)
		{
			foreach(Line l2 in g.edges)
			{
				if(l1 == l2)
					continue;
				if(l1.LineIntersection(l2))
				{
					
				} 
			}
		}
		 
	}
}
