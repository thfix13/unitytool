using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 

public class Triangle{
	public Vector3[] vertex = new Vector3[3];

	public List<Triangle> voisins = new List<Triangle>();  

	public Triangle(Vector3 v1, Vector3 v2,Vector3 v3)
	{
		vertex[0]=v1; 
		vertex[1]=v2; 
		vertex[2]=v3; 


	}

	public Line[] getLines()
	{
		Line[] l = new Line[3];
		l[0] = new Line(vertex[0],vertex[1]);
		l[1] = new Line(vertex[1],vertex[2]);
		l[2] = new Line(vertex[0],vertex[2]);
		
		return l; 
	}



	public Vector3 GetCenterTriangle()
	{
		return new Vector3( (vertex[0].x + vertex[1].x +vertex[2].x ) / 3,
		                    (vertex[0].y + vertex[1].y +vertex[2].y ) / 3,
		                   (vertex[0].z + vertex[1].z +vertex[2].z ) / 3);
	}

	public Line ShareEdged(Triangle t)
	{
		if(this.Equals(t))
			return Line.Zero; 
			
		Line[] l1 = getLines(); 
		Line[] l2 = t.getLines(); 

		for(int i = 0; i<3; i++)
		{
			for(int j = 0; j<3;j++)
			{
				if(l1[i].Equals(l2[j]))
				{
					if(! voisins.Contains(t) && this != t)
						voisins.Add(t); 

					//Debug.DrawLine(this.GetCenterTriangle(),l1[i].MidPoint(),Color.cyan);

					return l1[i]; 
				}
			}
		}



		return Line.Zero; 

	}
	public bool Equals(Triangle t)
	{
		return GetCenterTriangle().Equals(t.GetCenterTriangle());
	}
	public Line[] GetSharedLines()
	{
		List<Line> t = new List<Line>();
		foreach(Triangle tt in voisins)
		{
			if(!t.Contains(ShareEdged(tt)))
				t.Add(ShareEdged(tt)); 
		}

		return t.ToArray(); 
	}


}
class TriangleEqualityComparer : IEqualityComparer<Triangle>
{
	
	public bool Equals(Triangle b1, Triangle b2)
	{
		return b1.Equals(b2);
	}
	
	
	public int GetHashCode(Triangle bx)
	{
		int hCode = (int)(bx.GetCenterTriangle().sqrMagnitude);
		return hCode.GetHashCode();
	}
	
}
