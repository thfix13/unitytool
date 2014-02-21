using UnityEngine;
using System.Collections;

public class Triangle{
	public Vector3[] vertex = new Vector3[3];

	public Triangle(Vector3 v1, Vector3 v2,Vector3 v3)
	{
		vertex[0]=v1; 
		vertex[1]=v2; 
		vertex[2]=v3; 


	}

	public Vector3 GetCenterTriangle()
	{
		return new Vector3( (vertex[0].x + vertex[1].x +vertex[2].x ) / 3,
		                    (vertex[0].y + vertex[1].y +vertex[2].y ) / 3,
		                   (vertex[0].z + vertex[1].z +vertex[2].z ) / 3);
	}

	public bool ShareEdged(Triangle t)
	{
		int i = 0; 
		int j = 1; 

		//check with all faces
		for(int a = 0; a<1; a++)
		{
			if(vertex[i].Equals(t.vertex[a]) && vertex[j].Equals(t.vertex[a+1]) )
				return true; 
		}
		if(vertex[i].Equals(t.vertex[0]) && vertex[j].Equals(t.vertex[2]) )
			return true;

		i = 1; j = 2; 
		for(int a = 0; a<1; a++)
		{
			if(vertex[i].Equals(t.vertex[a]) && vertex[j].Equals(t.vertex[a+1]) )
				return true; 
		}
		if(vertex[i].Equals(t.vertex[0]) && vertex[j].Equals(t.vertex[2]) )
			return true;

		i = 0; j = 2; 
		for(int a = 0; a<1; a++)
		{
			if(vertex[i].Equals(t.vertex[a]) && vertex[j].Equals(t.vertex[a+1]) )
				return true; 
		}
		if(vertex[i].Equals(t.vertex[0]) && vertex[j].Equals(t.vertex[2]) )
			return true;

		return false; 

	}

}
