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
			if(myLine == l)
				return false; 
		}

		//Now we check count the intersection
		Vector3 mid = l.MidPoint(); 
		//TODO: CHange this for finding the minimum

		return PointInside(mid);

	}

	public bool PointInside( Vector3 pt )
	{
		Line lray = new Line(pt, new Vector3(-100,-100)); 
		int count = 0; 
		foreach(Line myLine in edges){
			if(myLine.LineIntersection(lray))
				count++; 
		}
		return count%2 == 1; 
	}

	public List<Vector3> GetVertex()
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
		return vertex;
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

	public void BoundGeometry(Vector3[] boundary){
		//Debug.Log (boundary [0] + " " + boundary [2]);
		List<Vector3> newPts = new List<Vector3>();
		List<Line> removeLines = new List<Line> ();
		foreach (Line l in edges) {
			Vector3 diff = l.vertex[1] - l.vertex[0];
			float grad = 0, cx, cz;
			if( diff.x != 0 ) grad = diff.z / diff.x;//A Z-axis parallel line
			cx = (-l.vertex[1].x * grad) + l.vertex[1].z;
			cz = -l.vertex[1].z + (l.vertex[1].x * grad);


			bool bothPtsOut = true;
			for( int i = 0; i < 2; i++ ){
				if( (l.vertex[i].x > boundary[0].x || l.vertex[i].x < boundary[2].x) || (l.vertex[i].z > boundary[0].z && l.vertex[i].z < boundary[2].z) )
					bothPtsOut = true;
				else{
					bothPtsOut = false;
					break;
				}
			}

			if( bothPtsOut ){
				removeLines.Add(l);
				continue;
			}

			for( int i = 0; i < 2; i++ ){
				bool xchanged = false;

				if( l.vertex[i].x > boundary[0].x ){
					l.vertex[i].x = boundary[0].x;
					if( diff.x != 0 )
						l.vertex[i].z = boundary[0].x * grad + cx;
					break;
				}
				else if( l.vertex[i].x < boundary[2].x ){
					l.vertex[i].x = boundary[2].x;
					if( diff.x != 0 )
						l.vertex[i].z = boundary[2].x * grad + cx;
					break;
				}

				if( l.vertex[i].z > boundary[0].z ){
					l.vertex[i].z = boundary[0].z;
					if( diff.x != 0 )
						l.vertex[i].x = (boundary[0].z - cz) / grad;
				}
				else if( l.vertex[i].z < boundary[2].z ){
					l.vertex[i].z = boundary[2].z;
					if( diff.x != 0 )
						l.vertex[i].x = (boundary[2].z - cz) / grad;
				}
			}
		}
		foreach (Line l in removeLines)
			edges.Remove (l);
	}

	public Geometry GeometryMerge( Geometry G2 ){
		if (GeometryInside (G2))
			return this;
		else if (G2.GeometryInside (this))
			return G2;

		Geometry tempGeometry = new Geometry ();
		//Two Geometry objects - G1 and G2
		Geometry G1 = this;
		//Create new called G3 which starts as an union of G1 and G2
		Geometry G3 = new Geometry ();
		foreach (Line l in G1.edges)
			G3.edges.Add(l);		
		foreach (Line l in G2.edges)
			G3.edges.Add(l);		
		
		//Check for intersection points among lines in G3
		for (int i = 0; i < G3.edges.Count; i++) {
			for( int j = i + 1; j < G3.edges.Count; j++ ) {
				Line LA = G3.edges[i];
				Line LB = G3.edges[j];
				int caseType = LA.LineIntersectMuntac( LB );
				if( caseType == 1 ){//Regular intersections
					Vector3 pt = LA.GetIntersectionPoint( LB );
					G3.edges.Add( new Line( pt, LA.vertex[0] ) );
					G3.edges.Add( new Line( pt, LA.vertex[1] ) );
					G3.edges.Add( new Line( pt, LB.vertex[0] ) );
					G3.edges.Add( new Line( pt, LB.vertex[1] ));
					G3.edges.RemoveAt(j);
					G3.edges.RemoveAt(i);
					i--;
					break;
				}
			}
		}
		//Check: Points inside Polygon
		//Check all midpoint of each line in G3 to see if it lies in G1 or G2. If inside remove.
		Geometry toReturn = new Geometry();

		foreach(Line l in G3.edges){
			if(!G1.LineInside(l) && !G2.LineInside(l))
				toReturn.edges.Add(l);
		}
//		this.edges.Clear ();
//		foreach (Line l in toReturn.edges)
//			this.edges.Add (l);
		return toReturn;
	}

	public Geometry GeometryMergeInner( Geometry G2 ){
		Geometry tempGeometry = new Geometry ();
		//Two Geometry objects - G1 and G2
		Geometry G1 = this;
		//Create new called G3 which starts as an union of G1 and G2
		Geometry G3 = new Geometry ();
		foreach (Line l in G1.edges)
			G3.edges.Add(l);		
		foreach (Line l in G2.edges)
			G3.edges.Add(l);		
		
		//Check for intersection points among lines in G3
		for (int i = 0; i < G3.edges.Count; i++) {
			for( int j = i + 1; j < G3.edges.Count; j++ ) {
				Line LA = G3.edges[i];
				Line LB = G3.edges[j];
				int caseType = LA.LineIntersectMuntac( LB );
				if( caseType == 1 ){//Regular intersections
					Vector3 pt = LA.GetIntersectionPoint( LB );
					G3.edges.Add( new Line( pt, LA.vertex[0] ) );
					G3.edges.Add( new Line( pt, LA.vertex[1] ) );
					G3.edges.Add( new Line( pt, LB.vertex[0] ) );
					G3.edges.Add( new Line( pt, LB.vertex[1] ));
					G3.edges.RemoveAt(j);
					G3.edges.RemoveAt(i);
					i--;
					break;
				}
			}
		}
		//Check: Points inside Polygon
		//Check all midpoint of each line in G3 to see if it lies in G1 or G2. If inside remove.
		Geometry toReturn = new Geometry();
		
		foreach(Line l in G3.edges){
			if(!G2.LineInside(l))
				toReturn.edges.Add(l);
		}
		//Check pt inside in G2
		foreach (Line l in toReturn.edges) {
			if( G2.LineInside( l ) ){
				toReturn.edges.Remove(l);
				break;
			}
		}
		return toReturn;
	}

	public bool GeometryIntersect( Geometry G2 ){
		if (GeometryInside (G2))
			return true;
		else if (G2.GeometryInside (this))
			return true;

		foreach( Line La in this.edges ){
			foreach( Line Lb in G2.edges ){
				if( La.LineIntersectMuntac( Lb ) > 0 )
					return true;
			}
		}
		return false;
	}
	
	private double CrossProduct( Vector2 a, Vector2 b ){
		return (a.x * b.y) - (a.y * b.x);
	}

	public bool GeometryInside( Geometry G2 ){
		foreach (Line L in G2.edges) {
			if( !PointInside( L.vertex[0] ) )
				return false;
			if( !PointInside( L.vertex[1] ) )
				return false;
		}
		return true;
	}

	public bool GeometryLineIntersect( Line param ){
		foreach (Line L in edges) 
		{
			if(L == param)
				continue; 
			//if( L.LineIntersectMuntac( param ) > 0 )
			if( L.LineIntersection( param ) )
				return true;
		}
		return false;
	}
}
