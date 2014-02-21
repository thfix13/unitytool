using UnityEngine;
using System.Collections;

public class Line 
{

	public Vector3[] vertex = new Vector3[2];



	public Line(Vector3 v1, Vector3 v2)
	{
		vertex[0] = v1; 
		vertex[1] = v2; 
	}

	public  bool Equals(Line l)
	{
		return this.MidPoint().Equals(l.MidPoint());
		//return vertex[0].Equals(l.vertex[0]) && vertex[1].Equals(l.vertex[1]); 
		
	}
	public static Line Zero
	{
		//get the person name 
		get { return new Line(Vector3.zero,Vector3.zero); }
	}
	public Vector3 MidPoint()
	{
		return new Vector3( (vertex[0].x + vertex[1].x)/2,
		                   (vertex[0].y + vertex[1].y)/2,
		                   (vertex[0].z + vertex[1].z)/2);
	}
}
