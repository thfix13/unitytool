using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 

public class Triangle
{
	public Vector3[] vertex = new Vector3[3];
	public Color[] colourVertex = new Color[3];

	//Reference to points in Triangulation gameobject data holder
	public int[] refPoints = new int[3]; 

	public List<Triangle> voisins = new List<Triangle>();  

	public Triangulation data; 

	public Triangle(Vector3 v1, Vector3 v2,Vector3 v3)
	{
		vertex[0]=v1; 
		vertex[1]=v2; 
		vertex[2]=v3; 
	}
	public Triangle(Vector3 v1,int i, Vector3 v2,int j,Vector3 v3,int k)
	{
		vertex[0]=v1; 
		vertex[1]=v2; 
		vertex[2]=v3; 

		refPoints[0] = i; 
		refPoints[1] = j; 
		refPoints[2] = k; 

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
	public void SetColour()
	{
		//data.points.AddRange(this.vertex);

		data.colours[refPoints[0]]=(Color.blue);
		data.colours[refPoints[1]]=(Color.red);
		data.colours[refPoints[2]]=(Color.green);

		


		foreach(Triangle t in voisins)
		{
			t.SetColour2(); 
			 
		}

	}

	public void SetColour2()
	{

		//Find the point with no colour
		int point = -1; 
		int sum = 0; 
		foreach(int i in refPoints)
		{
			if(data.colours[i] == Color.cyan)
			{
				point = i;

			}
			else if(data.colours[i] == Color.blue)
				sum +=1; 
			else if(data.colours[i] == Color.red)
				sum +=2; 
			else if(data.colours[i] == Color.green)
				sum +=3; 

		}
		if(point == -1 || sum == 6)
			return; 
		//Add missing colour
		if(6-sum == 1)
			data.colours[point] = Color.blue;
		if(6-sum == 2)
			data.colours[point] = Color.red;
		if(6-sum == 3)
			data.colours[point] = Color.green;

		foreach(Triangle t in voisins)
		{
			t.SetColour2();  
		}
	}
	public void PrintRefPosition()
	{
		Debug.Log(this.refPoints[0]+", "+
		          this.refPoints[1]+", "+
		          this.refPoints[2]); 
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
	public override string ToString()
	{
		return GetCenterTriangle().ToString();
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
