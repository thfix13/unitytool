using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 
using Vectrosity; 


//Changes Made:
//Used my implementation of line intersection function called: LineIntersectionMuntac (will change name later)
//Another function called GetIntersectionPoint
[Serializable]
public class Line :IEquatable<Line>
{ 
	public static float eps = 1e-5f;//the margin of accuracy for all floating point equivalence checks
	public Vector3[] vertex = new Vector3[2];
	public Color[] colours = new Color[2]; 
	public string name = "Vector Line";
	public List<Vector2> listGrid;
	public List<int> valueGrid = new List<int>();
	public Color costColor = Color.black;
	


	public Line(Vector3 v1, Vector3 v2)
	{
		vertex[0] = v1; 
		vertex[1] = v2; 
		colours[0] = Color.cyan;
		colours[1] = Color.cyan; 
	}
	
	public  bool Equals(Line l)
	{
		if (VectorApprox (l.vertex [0], vertex [0]) && VectorApprox (l.vertex [1], vertex [1]))
			return true;
		else if (VectorApprox (l.vertex [0], vertex [1]) && VectorApprox (l.vertex [1], vertex [0]))
			return true;
		else
			return false;
	}
	
	public static Line Zero
	{
		get { return new Line(Vector3.zero,Vector3.zero); }
	}
	
	public Vector3 MidPoint()
	{
		return new Vector3( (vertex[0].x + vertex[1].x)/2f,
		                   (vertex[0].y + vertex[1].y)/2f,
		                   (vertex[0].z + vertex[1].z)/2f);
	}
	
	public Vector3 GetOther(Vector3 v)
	{
		if( VectorApprox( vertex[0], v ) )
			return vertex[1];
		else
			return vertex[0];
	}
	
	public void DrawLine(Color c)
	{
		Debug.DrawLine(this.vertex[0],this.vertex[1],c); 
	}
	
	public void DrawLine()
	{
		Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
		                    UnityEngine.Random.Range(0.0f,1.0f),
		                    UnityEngine.Random.Range(0.0f,1.0f)) ;
		
		Debug.DrawLine(this.vertex[0],this.vertex[1],c); 
	}

	public void SetColour(float inter)
	{
		
	}
	
	
	public void DrawVector(GameObject parent,float inter)
	{
		String name = "Line";
		
		costColor = Color.Lerp(Color.green, Color.red, inter);
		Color c = costColor;
		
		
		VectorLine line = new VectorLine(name,vertex,c,null,2.0f);
		line.vectorObject.transform.parent = parent.transform;
		line.vectorObject.name = name;
		line.Draw3D();
	}
	
	public void DrawVector(GameObject parent)
	{
				Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
				                           UnityEngine.Random.Range(0.0f,1.0f),
				                           UnityEngine.Random.Range(0.0f,1.0f)) ;
		
		// Color c = Color.blue;
		VectorLine line = new VectorLine("Line",vertex,c,null,2.0f);
		line.vectorObject.transform.parent = parent.transform;
		line.vectorObject.name = name;
		line.Draw3D();
	}
	
	public void DrawVector()
	{
		GameObject parent = GameObject.Find("temp");
		Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
		                           UnityEngine.Random.Range(0.0f,1.0f),
		                           UnityEngine.Random.Range(0.0f,1.0f)) ;
		
		// Color c = Color.blue;
		VectorLine line = new VectorLine("Line",vertex,c,null,2.0f);
		line.vectorObject.transform.parent = parent.transform;
		line.vectorObject.name = name;
		line.Draw3D();
	}


	public void DrawVector(Color c)
	{
		GameObject parent = GameObject.Find("temp");
		
		// Color c = Color.blue;
		VectorLine line = new VectorLine("Line",vertex,c,null,2.0f);
		line.vectorObject.transform.parent = parent.transform;
		line.vectorObject.name = name;
		line.Draw3D();
	}

	public void DrawVector(GameObject parent,Color c)
	{
		VectorLine line = new VectorLine("Line",vertex,c,null,2.0f);
		line.vectorObject.transform.parent = parent.transform;
		line.vectorObject.name = name;
		line.Draw3D();
	}
	
	public bool ShareVertex(Line l)
	{
		foreach(Vector3 v in vertex)
		{
			foreach(Vector3 w in l.vertex)
			{
				if(VectorApprox(v, w))
					return true; 
			}
		}
		return false; 
	}
	
	public bool ContainsVertex(Vector3 v)
	{
		if(VectorApprox(v,vertex[0])||VectorApprox(v,vertex[1]))
			return true; 
		else 
			return false; 
	}

	public Vector3 getNotSharedVertex(Line l)
	{
		if(l.ContainsVertex(vertex[0]))
			return vertex[1];
		else
			return vertex[0];
	}
	public Vector3 getOtherVertex(Vector3 v)
	{
		if (VectorApprox(v,vertex[0]))
			return vertex[1];
		else
			return vertex[0];
	}
	public Vector3 getSharedVertex(Line l)
	{
		foreach(Vector3 v in vertex)
		{
			foreach(Vector3 w in l.vertex)
			{
				if(VectorApprox(v, w))
					return v;
			}
		}
		return vertex[0]; 
	}
	
	public float Magnitude()
	{
		return (vertex[0]-vertex[1]).magnitude; 
	}
	
	private bool CounterClockWise(Vector3 v1,Vector3 v2,Vector3 v3){
		float a = v1.x, b = v1.z;  
		float c = v2.x, d = v2.z;  
		float e = v3.x, f = v3.z;  
		
		if( (f-b)*(c-a) > (d-b)*(e-a))//PP: add eps to right?
			return true;
		else
			return false; 
	}
	
	public bool LineInterIsLeft( Line param ){
		bool firstA, firstB, secondA, secondB;
		bool first = false, second = false;
		firstA = CounterClockWise (vertex [0], vertex [1], param.vertex [0]);
		firstB = CounterClockWise (vertex [0], vertex [1], param.vertex [1]);
		if ((!firstA && firstB) || (firstA && !firstB))
			first = true;
		secondA = CounterClockWise (param.vertex [0], param.vertex [1], vertex [0]);
		secondB = CounterClockWise (param.vertex [0], param.vertex [1], vertex [1]);
		if ((!secondA && secondB) || (secondA && !secondB))
			second = true;
		if( first && second )
			return true;
		else
			return false;
	}
	
	//Detects Intersection when endpoints are not overlapping. Returns 1 if true.
	//Returns 2 if lines are colinear and overlapping (excludes endpoints) TODO: Understand colinearity and implement properly
	//Returns 0 if no overlapping
	public int LineIntersectMuntac (Line param){
		Vector3 a = this.vertex [0];
		Vector3 b = this.vertex[1];
		Vector3 c = param.vertex [0];
		Vector3 d = param.vertex [1];
		
		Vector2 u = new Vector2 (b.x, b.z) - new Vector2 (a.x, a.z);
		Vector2 p0 = new Vector2 (a.x, a.z);
		
		Vector2 v = new Vector2 (d.x, d.z) - new Vector2 (c.x, c.z);
		Vector2 q0 = new Vector2 (c.x, c.z);
		
		double numerator1 = CrossProduct ((q0 - p0), v);
		double numerator2 = CrossProduct ((q0 - p0), u);
		double denom = CrossProduct (u, v);
		
		//Case 1 - Colinear
		if ( denom == 0 && numerator2 == 0 ) {
			//Case 2 - Colinear and Overlapping
			//if( Vector2.Dot( (q0 - p0), u ) >= 0 && Vector2.Dot( (q0 - p0), u ) <= Vector2.Dot( u, u ) )
			if( floatCompare( Vector2.Dot( (q0 - p0), u ), 0, ">=" )
			   && floatCompare( Vector2.Dot( (q0 - p0), u ), Vector2.Dot( u, u ), "<=" ) )
				return 2;
			//if( Vector2.Dot( (p0 - q0), v ) >= 0 && Vector2.Dot( (p0 - q0), v ) <= Vector2.Dot( v, v ) )
			if( floatCompare( Vector2.Dot( (p0 - q0), v ), 0, ">=" )
			   && floatCompare( Vector2.Dot( (p0 - q0), v ), Vector2.Dot( v, v ), "<=" ) )
				return 2;
			return 0;
		}
		//Case 3 - Parallel
		if (denom == 0 && numerator2 != 0)
			return 0;
		
		//Case 4 - Intersects
		double s = numerator1 / denom;
		double t = numerator2 / denom;
		
		if ((s > 0 && s < 1) && (t > 0 && t < 1))
			return 1;
		
		return 0;
	}
	
	//Detects regular intersections unless the lines share a vertex
	public int LineIntersectMuntacEndPt (Line param){
		Vector3 a = this.vertex [0];
		Vector3 b = this.vertex[1];
		Vector3 c = param.vertex [0];
		Vector3 d = param.vertex [1];
		
		Vector2 u = new Vector2 (b.x, b.z) - new Vector2 (a.x, a.z);
		Vector2 p0 = new Vector2 (a.x, a.z);
		
		Vector2 v = new Vector2 (d.x, d.z) - new Vector2 (c.x, c.z);
		Vector2 q0 = new Vector2 (c.x, c.z);
		
		double numerator1 = CrossProduct ((q0 - p0), v);
		double numerator2 = CrossProduct ((q0 - p0), u);
		double denom = CrossProduct (u, v);
		
		//Case 1 - Colinear
		if ( denom == 0 && numerator2 == 0 ) {
			//Case 2 - Colinear and Overlapping
			//if( Vector2.Dot( (q0 - p0), u ) >= 0 && Vector2.Dot( (q0 - p0), u ) <= Vector2.Dot( u, u ) )
			if( floatCompare( Vector2.Dot( (q0 - p0), u ), 0f, ">=" ) && floatCompare( Vector2.Dot( (q0 - p0), u ), Vector2.Dot( u, u ), "<=" ) )
				return 2;
			//if( Vector2.Dot( (p0 - q0), v ) >= 0 && Vector2.Dot( (p0 - q0), v ) <= Vector2.Dot( v, v ) )
			if( floatCompare( Vector2.Dot( (p0 - q0), v ), 0f, ">=" ) && floatCompare( Vector2.Dot( (p0 - q0), v ), Vector2.Dot( v, v ), "<=" ) )
				return 2;
			return 0;
		}
		//Case 3 - Parallel
		if (denom == 0 && numerator2 != 0)
			return 0;
		
		//Case 4 - Intersects
		double s = numerator1 / denom;
		double t = numerator2 / denom;
		
		if ( (floatCompare( (float)s, 0f, ">=")  && floatCompare((float)s, 1f, "<="))
		    && (floatCompare( (float)t, 0f, ">=")  && floatCompare((float)t, 1f, "<=")) ){
			if( ShareVertex(param) )
				return 0;
			else
				return 1;
		}
		return 0; 
	}
	
	private double CrossProduct( Vector2 a, Vector2 b ){
		return (a.x * b.y) - (a.y * b.x);
	}
	
	public Vector3 GetIntersectionPoint (Line param){
		Vector3 a = this.vertex [0];
		Vector3 b = this.vertex [1];
		Vector3 c = param.vertex [0];
		Vector3 d = param.vertex [1];
		
		Vector2 u = new Vector2 (b.x, b.z) - new Vector2 (a.x, a.z);
		Vector2 p0 = new Vector2 (a.x, a.z);
		
		Vector2 v = new Vector2 (d.x, d.z) - new Vector2 (c.x, c.z);
		Vector2 q0 = new Vector2 (c.x, c.z);
		
		double numerator1 = CrossProduct ((q0 - p0), v);
		double numerator2 = CrossProduct ((q0 - p0), u);
		double denom = CrossProduct (u, v);
		
		double s = numerator1 / denom;
		double t = numerator2 / denom;
		
		Vector3 r = a + (b-a)*(float)s; 
		return r;
	}
	
	public bool PointIsLeft( Vector3 pt ){
		Vector3 a = this.vertex [0];
		Vector3 b = this.vertex [1];
		Vector3 c = pt;
		return ((b.x - a.x)*(pt.z - a.z) - (b.z - a.z)*(c.x - a.x)) > 0;
	}
	
	public bool PointIsLeft( Line param ){
		Vector3 x = this.getSharedVertex (param);
		if (x == param.vertex [0])
			return PointIsLeft (param.vertex [1]);
		else
			return PointIsLeft (param.vertex [0]);
	}
	
	public bool VectorApprox ( List<Vector3> obs_pts, Vector3 interPt ){
		foreach (Vector3 v in obs_pts) {
			if( Math.Abs (v.x - interPt.x) < eps && Math.Abs (v.z - interPt.z) < eps )
				return true;
		}
		return false;
	}
	public static bool VectorApprox ( Vector3 a, Vector3 b ){
		if( Math.Abs (a.x - b.x) < eps && Math.Abs (a.z - b.z) < eps )
			return true;
		else
			return false;
	}
	
	public bool floatCompare ( float a, float b ){
		return Math.Abs (a - b) < eps;
	}
	

	public List<Vector3> BreakConstantRate(float rate)
	{
		Vector3 dir = vertex[1] - vertex[0];
		
		

		float maxDistance = dir.magnitude; 

		dir.Normalize();
		dir *= rate;

		//Now lets move on the line dir + v0 until greater than 
		//v1. 
		List<Vector3> toReturn = new List<Vector3>(); 
		toReturn.Add(vertex[0]);




		float distDone = 0; 
		do
		{
			distDone+=dir.magnitude;
			if(distDone<maxDistance)				
				toReturn.Add(toReturn[toReturn.Count-1]+dir);
		}
		while(distDone<maxDistance);


		// foreach(Vector3 v in toReturn)
		// {
		// 	DrawSphere(v,"");
		// }


		return toReturn; 
	}

	public bool floatCompare ( float a, float b, string condition ){
		switch (condition) {
		case(">="):
			if (a > b || Math.Abs (a - b) < eps)
				return true;
			break;
		case("=="):
			if (Math.Abs (a - b) < eps)
				return true;
			break;
		case("<="):
			if (a < b || Math.Abs (a - b) < eps)
				return true;
			break;
		}
		return false;
	}
	public bool LineAlmostEquals(Line l)
	{
		Vector3 m1 = l.MidPoint(); 
		Vector3 m2 = this.MidPoint(); 

		return VectorApprox(m1,m2);
	}
	public int GetHashCode(Line bx)
	{
		
		int hCode = (int)(bx.MidPoint().sqrMagnitude);
		return hCode.GetHashCode();
	}
	void DrawSphere( Vector3 v,string s )
	{

		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.name = "Balle " + s;
		inter.tag = "sphere";	
		inter.transform.position = v;
		inter.transform.localScale = new Vector3(0.1f,0.1f,0.1f); 
		inter.transform.parent = temp.transform;
		//inter.gameObject.name = vlcnt.ToString();
	}
}

class LineEqualityComparer : IEqualityComparer<Line>
{
	
	public bool Equals(Line b1, Line b2)
	{

		return b1.Equals(b2);
	}
	
	
	public int GetHashCode(Line bx)
	{
		
		int hCode = (int)(bx.MidPoint().sqrMagnitude);
		return hCode.GetHashCode();
	}	
}
