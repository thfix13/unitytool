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

	public Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
	                           UnityEngine.Random.Range(0.0f,1.0f),
	                           UnityEngine.Random.Range(0.0f,1.0f)) ;

	public Triangle(Vector3 v1, Vector3 v2,Vector3 v3)
	{
		vertex[0]=v1; 
		vertex[1]=v2; 
		vertex[2]=v3;

		colourVertex[0] = Color.cyan;
		colourVertex[1] = Color.cyan;
		colourVertex[2] = Color.cyan;
	}
	public Triangle(Vector3 v1,int i, Vector3 v2,int j,Vector3 v3,int k)
	{
		vertex[0]=v1; 
		vertex[1]=v2; 
		vertex[2]=v3; 

		refPoints[0] = i; 
		refPoints[1] = j; 
		refPoints[2] = k; 

		colourVertex[0] = Color.cyan;
		colourVertex[1] = Color.cyan;
		colourVertex[2] = Color.cyan;

	}

	public Line[] getLines()
	{
		Line[] l = new Line[3];
		l[0] = new Line(vertex[0],vertex[1]);
		l[1] = new Line(vertex[1],vertex[2]);
		l[2] = new Line(vertex[0],vertex[2]);
		
		return l; 
	}

	public void DrawDebug()
	{
		//Add get lines method. 
		foreach(Line l in getLines())
		{
			Vector3 t1 = l.vertex[0] + ((GetCenterTriangle() - l.vertex[0]).normalized * 0.1f) ; 
			Vector3 t2 = l.vertex[1] + ((GetCenterTriangle() - l.vertex[1]).normalized * 0.1f) ; 
			Debug.DrawLine(t1,t2,c); 
		}

	}
	public Vector3[] getVertexMiddle()
	{
		Vector3[] toReturn = new Vector3[3];
		toReturn[0] = vertex[0] + ((GetCenterTriangle() - vertex[0]).normalized * 0.2f) ; 
		toReturn[1] = vertex[1] + ((GetCenterTriangle() - vertex[1]).normalized * 0.2f) ; 
		toReturn[2] = vertex[2] + ((GetCenterTriangle() - vertex[2]).normalized * 0.2f) ; 
		return toReturn; 
	}
	public Vector3 GetCenterTriangle()
	{
		return new Vector3( (vertex[0].x + vertex[1].x + vertex[2].x ) / 3,
		                    (vertex[0].y + vertex[1].y + vertex[2].y ) / 3,
		                    (vertex[0].z + vertex[1].z + vertex[2].z ) / 3);
	}
	public void SetColour()
	{
		//data.points.AddRange(this.vertex);

		this.colourVertex[0]=Color.blue;
		this.colourVertex[1]=Color.red;
		this.colourVertex[2]=Color.green;

		


		foreach(Triangle t in voisins)
		{

			//Debug.DrawLine(this.GetCenterTriangle(),t.GetCenterTriangle(),Color.blue);

			t.SetColour(this); 
		}

	}

	public void SetColour(Triangle ttt)
	{
		if(colourVertex[0] != Color.cyan ||
		   colourVertex[1] != Color.cyan ||
		   colourVertex[2] != Color.cyan )
			return;

		//Get the colour to put
		int indexSum = 0;
		int sum = 0;  

		for(int i = 0; i<ttt.vertex.Length; i++)
	    {

			for(int j = 0; j<vertex.Length; j++)
			{
				
				if(ttt.vertex[i].Equals(vertex[j]))
				{

					this.colourVertex[j] = ttt.colourVertex[i];

					//Debug.Log(ttt.colourVertex[i]);

					indexSum += j; 

					if(this.colourVertex[j] == Color.red)
						sum+=1; 
					else if(this.colourVertex[j] == Color.blue)
						sum+=2; 
					else if(this.colourVertex[j] == Color.green)
						sum+=3; 
				}
			}
		}

		//Debug.Log(indexSum); 

		if(6 - sum == 1)
			this.colourVertex[ (3 - indexSum)] = Color.red;
		else if(6 - sum == 2)
			this.colourVertex[3 - indexSum] = Color.blue;
		else if(6 - sum == 3)
			this.colourVertex[3 - indexSum] = Color.green;


		//return;



		foreach(Triangle t in voisins)
		{
			if(t == ttt)
				continue; 
			t.SetColour(this); 
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

	public Line ShareEdged(Triangle t, List<Line> toSkip)
	{
		if(this.Equals(t))
			return Line.Zero; 
		
		Line[] l1 = getLines(); 
		Line[] l2 = t.getLines(); 
		
		for(int i = 0; i<3; i++)
		{
			bool toSkipLine = false; 
			foreach(Line l in toSkip)
			{
				if(l.Equals(l1[i]))
					toSkipLine = true;
			}
			if(toSkipLine)
				continue; 
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
