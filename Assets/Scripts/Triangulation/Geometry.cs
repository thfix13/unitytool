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

		foreach (Line l in edges) {
			l.DrawVector (parent, c);
			c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
			              UnityEngine.Random.Range(0.0f,1.0f),
			              UnityEngine.Random.Range(0.0f,1.0f)) ;
		}
		//DrawVertex (parent);
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

	public bool LineInside(Line l)
	{
		//Test if one of my line
		//This is not the best version should check is colinear inastead!
		foreach(Line myLine in edges)
		{
			foreach(Vector3 v1 in myLine.vertex)
			{
				foreach(Vector3 v2 in l.vertex)
				{
					if(v1 == v2)
						return false; 
				}
			}
		}

		//Now we check count the intersection
		Vector3 mid = l.MidPoint(); 
		//TODO: CHange this for finding the minimum
		Line lray = new Line(mid, new Vector3(-10,-10)); 
		int count = 0; 
		foreach(Line myLine in edges)
		{
			if(myLine.LineIntersection(lray))
			{
				count++; 
			}

		}

		return count%2 == 1; 
	}

	public void DrawVertex(GameObject parent)
	{
		//Find vertex
		List<Vector3> vertex = new List<Vector3>(); 
		foreach(Line l in edges)
		{
			foreach(Vector3 v in l.vertex)
			{
				if(!vertex.Contains(v))
					vertex.Add(v); 

			}

		}

		//Draw
		foreach(Vector3 v in vertex)
		{
			GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			inter.transform.position = v;
			inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f); 
			inter.transform.parent = parent.transform;
		}

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

					Vector3 pos = l1.LineIntersectionVect(l2);
					GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					inter.transform.position = pos;
					inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f); 
					inter.transform.parent = parent.transform; 
				} 
			}
		}
		 
	}
}
