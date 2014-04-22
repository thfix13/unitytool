using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 

public class Quadrilater 
{

	public Vector3[] vertex = new Vector3[4];
	public Line[] lines = new Line[4];
	public PolygonCollider2D collider = new PolygonCollider2D(); 


	public Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
	                    UnityEngine.Random.Range(0.0f,1.0f),
	                    UnityEngine.Random.Range(0.0f,1.0f)) ;



	//Reference to points in Triangulation gameobject data holder
	//public int[] refPoints = new int[3]; 
	
	public List<Quadrilater> voisins = new List<Quadrilater>();  
	public List<Line> voisinsLine = new List<Line>();  

	//for MPS
	public bool visited = false; 	

	public Triangulation data; 
	public Quadrilater()
	{}
	public Quadrilater(Vector3 v1, Vector3 v2,Vector3 v3, Vector3 v4)
	{
		//Make sure of the connectivity

		vertex[0]=v1; 
		vertex[1]=v2; 
		vertex[2]=v3;
		vertex[3]=v4; 

		SetLine(); 
	}

	public Quadrilater(Line l1, Line l2,Line l3, Line l4)
	{
		lines[0] = l1;
		lines[1] = l2;
		lines[2] = l3;
		lines[3] = l4;

		
	}

	//

	public Line[] GetLine(Vector3 v)
	{
		SetLine(); 
		List<Line> toReturn = new List<Line>();
		foreach(Line l in lines)
		{
			l.DrawLine(Color.red);	

			foreach(Vector3 vLine in l.vertex)
			{
				if(vLine == v)
				{
					toReturn.Add(l);
				}
			}
		}
		return toReturn.ToArray(); 
	}

	public void SetVoisins(List<Quadrilater> geos)
	{
		foreach(Quadrilater q in geos)
		{
			Line voisin = this.GetClosestLine(q,geos);
			if(voisin != null)
			{
				voisins.Add (q); 
				voisinsLine.Add(voisin); 
			}
		}
	}

	public void DrawVoisin()
	{
		foreach(Line l in voisinsLine)
		{
			l.DrawLine(); 
		}
	}
	public void SetLine()
	{
		for(int i = 0; i<vertex.Length;i++)
		{
			if(i<vertex.Length-1)
			{
				lines[i]=(new Line(vertex[i],vertex[i+1])); 
			}
			else
			{
				lines[i]=(new Line(vertex[i],vertex[0])); 
			}
		}
	}
	public float GetLength()
	{
		float length = 0; 

		foreach(Line l in lines)
			length += l.Magnitude(); 
		return length; 
	}

	public Line[] getLines()
	{
		return lines; 
	}
	
	
	
	public Vector3 GetCenterQuad()
	{
		vertex = GetPoint(); 
		return new Vector3( (vertex[0].x + vertex[1].x +vertex[2].x + vertex[3].x) / 4,
		                   (vertex[0].y + vertex[1].y +vertex[2].y + vertex[3].y) / 4,
		                   (vertex[0].z + vertex[1].z +vertex[2].z + vertex[3].z) / 4);
	}

	public int SharedPoints(Quadrilater q)
	{
		int nb = 0; 
		foreach(Vector3 v1 in vertex)
		{
			foreach(Vector3 v2 in q.vertex)
			{
				if(v1.Equals(v2))
					nb+=1; 
			}
		}
		return nb; 
	}


	public Line ShareEdged(Quadrilater t)
	{
		if(this.Equals(t))
			return Line.Zero; 
		
		Line[] l1 = getLines(); 
		Line[] l2 = t.getLines(); 
		
		for(int i = 0; i<4; i++)
		{
			for(int j = 0; j<4;j++)
			{
				if(l1[i].Equals(l2[j]))
				{
					if(! voisins.Contains(t) && this != t)
						voisins.Add(t); 
					
					//Debug.DrawLine(this.GetCenterQuad(),l1[i].MidPoint(),Color.cyan);
					
					return l1[i]; 
				}
			}
		}
		
		
		
		return Line.Zero; 
		
	}
	public Vector3[] GetPoint()
	{
		Vector3[] t = new Vector3[4];
		t[0] = lines[0].vertex[0];
		t[1] = lines[0].vertex[1];
		foreach(Line l in lines)
		{
			if(l == lines[0])
				continue; 
			if(l.vertex[0] == t[1])
			{
				t[2] = l.vertex[1];
				break; 
			}
			if(l.vertex[1] == t[1])
			{	
				t[2] = l.vertex[0];
				break; 
			}
		}
		foreach(Line l in lines)
		{
			if(l == lines[0])
				continue; 
			if(l.vertex[0] == t[0])
			{
				t[3] = l.vertex[1];
				break; 
			}
			if(l.vertex[1] == t[0])
			{
				t[3] = l.vertex[0];
				break; 
			}
		}
		return t; 
	}
	public bool Collide(Vector3 v)
	{
		//Check if the points are within 
		float maxx = -100000, minx = 1000000, maxy=-10000000, miny=10000000; 
		foreach(Vector3 v1 in GetPoint())
		{
			if(v1.x>maxx)
				maxx = v1.x;
			if(v1.x<minx)
				minx = v1.x; 
			if(v1.z>maxy)
				maxy = v1.z;
			if(v1.z<miny)
				miny = v1.z; 
			
		}
		//points
		if(v.x > minx + 0.1 && v.x < maxx - 0.1 && 
		   v.z > miny + 0.1 && v.z < maxy - 0.1)
			return true; 
		

		return false; 
	}
	public bool Collide(Quadrilater q)
	{


		//Check if the points are within 
		float maxx = -100000, minx = 1000000, maxy=-10000000, miny=10000000; 
		foreach(Vector3 v in GetPoint())
		{
			if(v.x>maxx)
				maxx = v.x;
			if(v.x<minx)
				minx = v.x; 
			if(v.z>maxy)
				maxy = v.z;
			if(v.z<miny)
				miny = v.z; 

		}



		//points
		foreach(Vector3 v in q.GetPoint())
		{
			if(v.x > minx + 0.1 && v.x < maxx - 0.1 && 
			   v.z > miny + 0.1 && v.z < maxy - 0.1)
				return true; 

		}
		//midpoints
		 
		foreach(Line l in q.lines)
		{
			bool sharedLine = false; 
			foreach(Line ll in lines)
			{
				if(ll.Equals(l))
				{
					sharedLine = true; 
					break; 
				}
			}
			if(sharedLine)
				continue; 

			Vector3 v = l.MidPoint(); 
			if(v.x > minx + 0.1 && v.x < maxx - 0.1 && 
			   v.z > miny + 0.1 && v.z < maxy - 0.1)
				return true; 
			
		}

		return false; 
	}
	public bool Collide(Line l)
	{

		
		//Check if the points are within 
		float maxx = -100000, minx = 1000000, maxy=-10000000, miny=10000000; 
		foreach(Vector3 v in GetPoint())
		{
			if(v.x>maxx)
				maxx = v.x;
			if(v.x<minx)
				minx = v.x; 
			if(v.z>maxy)
				maxy = v.z;
			if(v.z<miny)
				miny = v.z; 
			
		}
		
		
		
		//points
		foreach(Vector3 v in l.vertex)
		{
			if(v.x > minx + 0.01  && v.x < maxx -0.01  && 
			   v.z > miny + 0.01 && v.z < maxy -0.01 )
				return true; 
			
		}

		foreach(Line ll in lines)
		{
			if(ll.LineIntersection(l))
				return true;
		}

		
		return false; 
	}

	public Quadrilater findClosestQuad(Vector3 v,List<Quadrilater> geos,List<Quadrilater> already)
	{
		Quadrilater toReturn = null; 
		float dist = 100000000; 
		
		foreach(Quadrilater q in geos)
		{
			if(q == this || already.Contains(q))
				continue; 
			
			Line l = this.GetClosestLine(v,q,geos); 
			
			if(l == null)
				continue;
			
			if( l.Magnitude() < dist)
			{
				toReturn = q; 
				dist = l.Magnitude (); 
			}
		}
		
		return toReturn; 
		
	}

	public Quadrilater findClosestQuad(List<Quadrilater> geos,List<Quadrilater> already)
	{
		Quadrilater toReturn = null; 
		float dist = 100000000; 

		foreach(Quadrilater q in geos)
		{
			if(q == this || already.Contains(q))
				continue; 
			 
			Line l = this.GetClosestLine(q,geos); 

			if(l == null)
				continue;

			if( l.Magnitude() < dist)
			{
				toReturn = q; 
				dist = l.Magnitude (); 
			}
		}

		return toReturn; 

	}



	public Line GetClosestLine(Quadrilater q, List<Quadrilater> geos)
	{
		Line toReturn = null;
		float dist = 1000000; 

		foreach(Vector3 v1 in this.vertex)
		{
			foreach(Vector3 v2 in q.vertex)
			{
				Line l = new Line(v1,v2);

				//Check collision
				bool collisionFree = true;
				
				foreach(Quadrilater qCollision in geos)
				{

					if(qCollision.Collide(l))
						collisionFree = false; 
				}
				
				if(!collisionFree)
				{
					continue; 
				}

				if(l.Magnitude()<dist)
				{
					toReturn = l; 
					dist = l.Magnitude(); 
				}
			}
		}
		return toReturn; 
	}
	public Line GetClosestLine(Vector3 v,Quadrilater q, List<Quadrilater> geos)
	{
		Line toReturn = null;
		float dist = 1000000; 
		

		foreach(Vector3 v2 in q.vertex)
		{
			Line l = new Line(v,v2);
			
			//Check collision
			bool collisionFree = true;
			
			foreach(Quadrilater qCollision in geos)
			{
				if(this == qCollision)
					continue; 
				if(qCollision.Collide(l))
					collisionFree = false; 
			}
			
			if(!collisionFree)
			{
				continue; 
			}
			
			if(l.Magnitude()<dist)
			{
				toReturn = l; 
				dist = l.Magnitude(); 
			}
		}

		return toReturn; 
	}
	public void DrawDebug()
	{
		//Add get lines method. 
		foreach(Line l in getLines())
		{
			Debug.DrawLine(l.vertex[0],l.vertex[1],c); 
		}
		/*
		c = Color.red; 

		Vector3[] points = GetPoint(); 

		for(int i = 0; i<points.Length; i++)
		{
			if(i<points.Length-1)
				Debug.DrawLine(points[i],points[i+1],c);
			else
				Debug.DrawLine(points[i],points[0],c);
				
		}
		*/
	}

	public void DrawDebug(Color c)
	{
		//Add get lines method. 
		foreach(Line l in getLines())
		{
			Debug.DrawLine(l.vertex[0],l.vertex[1],c); 
		}
	}
	public bool Equals(Quadrilater t)
	{
		return GetCenterQuad().Equals(t.GetCenterQuad());
	}

	public Line[] GetSharedLines()
	{
		List<Line> t = new List<Line>();
		foreach(Quadrilater tt in voisins)
		{
			if(!t.Contains(ShareEdged(tt)))
				t.Add(ShareEdged(tt)); 
		}
		
		return t.ToArray(); 
	}

	public override string ToString()
	{
		return GetCenterQuad().ToString();
	}
	
}

class QuadrilaterEqualityComparer : IEqualityComparer<Quadrilater>
{
	
	public bool Equals(Quadrilater b1, Quadrilater b2)
	{
		return b1.Equals(b2);
	}
	
	
	public int GetHashCode(Quadrilater bx)
	{
		int hCode = (int)(bx.GetCenterQuad().sqrMagnitude);
		return hCode.GetHashCode();
	}
	
}


