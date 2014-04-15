using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 

public class Quadrilater 
{

	public Vector3[] vertex = new Vector3[4];

	
	//Reference to points in Triangulation gameobject data holder
	//public int[] refPoints = new int[3]; 
	
	public List<Quadrilater> voisins = new List<Quadrilater>();  
	
	public Triangulation data; 
	
	public Quadrilater(Vector3 v1, Vector3 v2,Vector3 v3, Vector3 v4)
	{
		vertex[0]=v1; 
		vertex[1]=v2; 
		vertex[2]=v3;
		vertex[4]=v4; 
	}

	
	public Line[] getLines()
	{
		Line[] l = new Line[4];
		l[0] = new Line(vertex[0],vertex[1]);
		l[1] = new Line(vertex[1],vertex[2]);
		l[2] = new Line(vertex[2],vertex[3]);
		l[3] = new Line(vertex[3],vertex[0]);
		
		return l; 
	}
	
	
	
	public Vector3 GetCenterQuad()
	{
		return new Vector3( (vertex[0].x + vertex[1].x +vertex[2].x + vertex[3].x) / 4,
		                   (vertex[0].y + vertex[1].y +vertex[2].y + vertex[3].y) / 4,
		                   (vertex[0].z + vertex[1].z +vertex[2].z + vertex[3].z) / 4);
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


