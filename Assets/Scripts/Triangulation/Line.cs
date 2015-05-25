using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 
using Vectrosity; 


//Changes Made:
//Used my implementation of line intersection function called: LineIntersectionMuntac (will change name later)
//Another function called GetIntersectionPoint
[Serializable]
public class Line : IEquatable<Line>
{

	public Vector3[] vertex = new Vector3[2];
	public Color[] colours = new Color[2]; 
	public string name = "Vector Line";

	public Line(Vector3 v1, Vector3 v2)
	{
		vertex[0] = v1; 
		vertex[1] = v2; 
		colours[0] = Color.cyan;
		colours[1] = Color.cyan; 
	}
	public float specialeps = 1e-6f;
	public bool floatCompareLIN ( float a, float b ){
		return Math.Abs (a - b) < specialeps;
	}
	
	public bool floatCompareLIN ( float a, float b, string condition ){
		switch (condition) {
		case(">="):
			if (a > b || Math.Abs (a - b) < specialeps)
				return true;
			break;
		case("=="):
			if (Math.Abs (a - b) < specialeps)
				return true;
			break;
		case("<="):
			if (a < b || Math.Abs (a - b) < specialeps)
				return true;
			break;
		}
		return false;
	}
	public bool VectorApprox ( List<Vector3> obs_pts, Vector3 interPt ){
		foreach (Vector3 v in obs_pts) {
			if( Math.Abs (v.x - interPt.x) < eps && Math.Abs (v.z - interPt.z) < eps )
				return true;
		}
		return false;
	}
	public bool VectorApprox ( Vector3 a, Vector3 b ){
		if( Math.Abs (a.x - b.x) < eps && Math.Abs (a.z - b.z) < eps )
			return true;
		else
			return false;
	}
	
	public bool floatCompare ( float a, float b ){
		return Math.Abs (a - b) < eps;
	}
	public float eps = 1e-5f;//the margin of accuracy for all floating point equivalence checks
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
	public Vector3 getMinVert( Vector3 a, Vector3 b ){
		if ( floatCompare( a.x, b.x ) ){
			if( a.z < b.z )
				return a;
			else
				return b;
		}
		else if ( a.x < b.x )
			return a;
		else
			return b;
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
	//Ignores shared endpoint collision
	public int LineIntersectMuntacGM (Line param){
		Vector3 a = getMinVert (this.vertex [0], this.vertex [1]);
		Vector3 b = this.vertex[1];
		if( a.Equals( this.vertex[1] ) )
			b = this.vertex[0];
		Vector3 c = getMinVert (param.vertex [0], param.vertex [1]);
		Vector3 d = param.vertex[1];
		if( c.Equals( param.vertex[1] ) )
			d = param.vertex[0];
		
		Vector2 u = new Vector2 (b.x, b.z) - new Vector2 (a.x, a.z);
		Vector2 p0 = new Vector2 (a.x, a.z);
		
		Vector2 v = new Vector2 (d.x, d.z) - new Vector2 (c.x, c.z);
		Vector2 q0 = new Vector2 (c.x, c.z);
		
		double numerator1 = CrossProduct ((q0 - p0), v);
		double numerator2 = CrossProduct ((q0 - p0), u);
		double denom = CrossProduct (u, v);
		//Debug.Log ("Denom " + denom + " num2 " + numerator2);
		//Case 1 - Colinear
		//if ( denom == 0 && numerator2 == 0 ) {
		
		if ( floatCompareLIN((float)denom, 0) && floatCompareLIN((float)numerator2,0) ) {
			//Case 2 - Colinear and Overlapping
			//Filter out colinear lines that only overlap at a single endpoint
			if ( ShareVertex (param) ){
				Vector3 shared = getSharedVertex( param );
				Line joint = new Line ( GetOther( shared ), param.GetOther( shared ) );
				if( joint.POL( shared ) )
					return 0;
			}
			//if( Vector2.Dot( (q0 - p0), u ) >= 0 && Vector2.Dot( (q0 - p0), u ) <= Vector2.Dot( u, u ) )
			if( floatCompareLIN( Vector2.Dot( (q0 - p0), u ), 0, ">=" )
			   && floatCompareLIN( Vector2.Dot( (q0 - p0), u ), Vector2.Dot( u, u ), "<=" ) )
				return 2;
			//if( Vector2.Dot( (p0 - q0), v ) >= 0 && Vector2.Dot( (p0 - q0), v ) <= Vector2.Dot( v, v ) )
			if( floatCompareLIN( Vector2.Dot( (p0 - q0), v ), 0, ">=" )
			   && floatCompareLIN( Vector2.Dot( (p0 - q0), v ), Vector2.Dot( v, v ), "<=" ) )
				return 2;
			return 0;
		}
		//Case 3 - Parallel
		//if (denom == 0 && numerator2 != 0)
		if ( floatCompareLIN((float)denom, 0) && !floatCompareLIN((float)numerator2,0) )
			return 0;
		
		//Case 4 - Intersects
		double s = numerator1 / denom;
		double t = numerator2 / denom;
		//Debug.Log ("s " + s + " t" + t);
		//if ((s > 0 && s < 1) && (t > 0 && t < 1))
		if ( (floatCompareLIN( (float)s, 0f, ">=")  && floatCompareLIN((float)s, 1f, "<="))
		    && (floatCompareLIN( (float)t, 0f, ">=")  && floatCompareLIN((float)t, 1f, "<=")) ){
			if( ShareVertex(param) )
				return 0;
			else if( POL(param.vertex[0]) || POL(param.vertex[1]) 
			        || param.POL(vertex[0]) || param.POL (vertex[1])
			        || CounterClockCheck(param) )
				return 1;
			else
				return 0;
		}		
		return 0;
	}
	public bool PointOnLineB( Vector3 pt ){
		Vector3 a = this.vertex [0];
		Vector3 b = this.vertex [1];
		Vector3 c = pt;
		//Debug.Log ("IsLeft Value: " + ((b.x - a.x) * (pt.z - a.z) - (b.z - a.z) * (c.x - a.x)));
		return floatCompare( ((b.x - a.x)*(pt.z - a.z) - (b.z - a.z)*(c.x - a.x)), 0 );
	}
	public bool POL( Vector3 pt ){
		return (this.PointOnLine (pt) && this.PointOnLineB (pt));
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
	
	public int PointIsLeftInt( Vector3 pt ){
		Vector3 a = this.vertex [0];
		Vector3 b = this.vertex [1];
		Vector3 c = pt;
		float val = ((b.x - a.x) * (pt.z - a.z) - (b.z - a.z) * (c.x - a.x));
		if (val < 0)
			return -1;
		else if( val > 0 )
			return 1;
		else
			return 0;
	}
	//Checks if lines cross each other in any kind of x shape
	public bool CounterClockCheck( Line param ){
		int val1 = PointIsLeftInt (param.vertex [0]);
		int val2 = PointIsLeftInt (param.vertex [1]);
		bool flagA = false;
		bool flagB = false;
		if( (val1 == -1 && val2 == 1) || (val1 == 1 && val2 == -1) )
			flagA = true;
		val1 = param.PointIsLeftInt (vertex [0] );
		val2 = param.PointIsLeftInt (vertex [1]);
		if( (val1 == -1 && val2 == 1) || (val1 == 1 && val2 == -1) )
			flagB = true;
		return (flagA && flagB);
	}
	public  bool Equals(Line l)
	{
		//if(this.MidPoint().Equals(l.MidPoint()))
		if(comparePoints(this.MidPoint(),l.MidPoint()))
		{
			if((comparePoints(vertex[0],l.vertex[1]) && comparePoints(vertex[1],l.vertex[0])) || (comparePoints(vertex[0],l.vertex[0]) && comparePoints(vertex[1],l.vertex[1])))
			{
				return true;
			}
		}
		return false;
		//return vertex[0].Equals(l.vertex[0]) && vertex[1].Equals(l.vertex[1]); 
		
	}
	public static Line Zero
	{
		//get the person name 
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
		if(vertex[0]==v)
			return vertex[1];
		return vertex[0];
	}
	public void DrawLine(Color c)
	{
		Debug.DrawLine(this.vertex[0],this.vertex[1],c);
	}
	public void DrawLine(GameObject parent,Color c)
	{
		VectorLine line = new VectorLine("Line",vertex,c,null,2.0f);
		line.vectorObject.transform.parent = parent.transform;
		line.vectorObject.name = name;
		line.Draw3D();
	}

	public void DrawLine()
	{
		//Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),UnityEngine.Random.Range(0.0f,1.0f),UnityEngine.Random.Range(0.0f,1.0f)) ;
		Color c = Color.red;
		Debug.DrawLine(this.vertex[0],this.vertex[1],c); 
	}
	public void DrawVector(GameObject parent)
	{
		Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
		                           UnityEngine.Random.Range(0.0f,1.0f),
		                           UnityEngine.Random.Range(0.0f,1.0f)) ;

		VectorLine line = new VectorLine("Line",vertex,c,null,2.0f);
		line.vectorObject.transform.parent = parent.transform;
		line.vectorObject.name = name;
		line.Draw3D();
	}
	public void DrawVector(GameObject parent,Color c)
	{
	
		VectorLine line = new VectorLine("Line",vertex,c,null,2.0f);
		line.vectorObject.transform.parent = parent.transform;
		line.Draw3D();
	}
	public bool ShareVertex(Line l)
	{
		foreach(Vector3 v in vertex)
		{
			foreach(Vector3 w in l.vertex)
			{
				if(v.Equals(w))
					return true; 
			}
		}
		return false; 
	}


	public bool LineIntersection(Line l)
	{
		Vector3 a = l.vertex[0]; 
		Vector3 b = l.vertex[1];
		Vector3 c = vertex[0];
		Vector3 d = vertex[1];
		
		
		// a-b
		// c-d
		//if the same lines
		
		//When share a point use the other algo
		if(a.Equals(c) || a.Equals(d) || b.Equals(c) || b.Equals(d))
			return LineIntersect(a,b,c,d); 
		
		
		
		
		return CounterClockWise(a,c,d) != CounterClockWise(b,c,d) && 
			CounterClockWise(a,b,c) != CounterClockWise(a,b,d);
		
		//if( CounterClockWise(a,c,d) == CounterClockWise(b,c,d))
		//	return false;
		//else if (CounterClockWise(a,b,c) == CounterClockWise(a,b,d))
		//	return false; 
		//else 
		//	return true; 
		
		
	}
	public float LengthOfLine()
	{
		return Vector3.Distance (vertex [0], vertex [1]);
	}
	public float Magnitude()
	{
		return (vertex[0]-vertex[1]).magnitude; 
	}
	private bool LineIntersect(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		//Debug.Log(a); 
		//Debug.Log(b); 
		//Debug.Log(c); 
		//Debug.Log(d); 
		
		Vector2 u = new Vector2(b.x,b.z) - new Vector2(a.x,a.z);
		Vector2 p0 = new Vector2(a.x,a.z); Vector2 p1 = new Vector2(b.x,b.z); 
		
		Vector2 v = new Vector2(d.x,d.z) - new Vector2(c.x,c.z);
		Vector2 q0 = new Vector2(c.x,c.z); Vector2 q1 = new Vector2(d.x,d.z);
		
		Vector2 w = new Vector2(a.x,a.z) - new Vector2(d.x,d.z);
		
		
		//if (u.x * v.y - u.y*v.y == 0)
		//	return true;
		
		double s = (v.y* w.x - v.x*w.y) / (v.x*u.y - v.y*u.x);
		double t = (u.x*w.y-u.y*w.x) / (u.x*v.y- u.y*v.x); 
		//Debug.Log(s); 
		//Debug.Log(t); 
		
		if ( (s>0 && s< 1) || (t>0 && t< 1) )
			return true;
		
		return false; 
	}
	public Vector3 LineIntersectionVect(Line l)
	{
		Vector3 a = l.vertex[0]; 
		Vector3 b = l.vertex[1];
		Vector3 c = vertex[0];
		Vector3 d = vertex[1];

		return LineIntersectVect(a,b,c,d);
	}
	private Vector3 LineIntersectVect (Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		//Debug.Log(a); 
		//Debug.Log(b); 
		//Debug.Log(c); 
		//Debug.Log(d); 
		
		Vector2 u = new Vector2 (b.x, b.z) - new Vector2 (a.x, a.z);
		Vector2 p0 = new Vector2 (a.x, a.z);
		Vector2 p1 = new Vector2 (b.x, b.z); 
		
		Vector2 v = new Vector2 (d.x, d.z) - new Vector2 (c.x, c.z);
		Vector2 q0 = new Vector2 (c.x, c.z);
		Vector2 q1 = new Vector2 (d.x, d.z);
		
		Vector2 w = new Vector2 (a.x, a.z) - new Vector2 (d.x, d.z);
		
		
		//if (u.x * v.y - u.y*v.y == 0)
		//	return true;
		
		double s = (v.y * w.x - v.x * w.y) / (v.x * u.y - v.y * u.x);
		double t = (u.x * w.y - u.y * w.x) / (u.x * v.y - u.y * v.x); 
		//Debug.Log(s); 
		//Debug.Log(t); 
		

			//Interpolation
		Vector3 r = a + (b-a)*(float)s; 
		return r; 
		//}
		


		//return Vector3.zero; 
	}
	public bool CounterClockWise(Vector3 v1,Vector3 v2,Vector3 v3)
	{
		//v1 = a,b
		//v2 = c,d
		//v3 = e,f
		
		float a = v1.x, b = v1.z;  
		float c = v2.x, d = v2.z;  
		float e = v3.x, f = v3.z;  
		
		if((f-b)*(c-a)> (d-b)*(e-a))
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
			if( Vector2.Dot( (q0 - p0), u ) >= 0 && Vector2.Dot( (q0 - p0), u ) <= Vector2.Dot( u, u ) )
				return 2;
			if( Vector2.Dot( (p0 - q0), v ) >= 0 && Vector2.Dot( (p0 - q0), v ) <= Vector2.Dot( v, v ) )
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
	private bool comparePoints(Vector3 v1,Vector3 v2)
	{
		//old //float limit = 0.0001f;
		float limit = 0.01f;
		float minVal = Vector3.SqrMagnitude (v1 - v2);
		if (minVal <= limit)
			return true;
		return false;
	}
	public bool CommonEndPoint(Line param)
	{
		if( comparePoints(vertex[0],param.vertex[0]) || comparePoints(vertex[0],param.vertex[1])
		   || comparePoints(vertex[1],param.vertex[0]) || comparePoints(vertex[1],param.vertex[1]))
			return true;
		return false;
	}
	public Vector3? getCommonEndPoint(Line param)
	{
		if(CommonEndPoint(param))
		{
			if( comparePoints(vertex[0],param.vertex[0]) || comparePoints(vertex[0],param.vertex[1]))
			{
				return vertex[0];
			}
			else if(comparePoints(vertex[1],param.vertex[0]) || comparePoints(vertex[1],param.vertex[1]))
			{
				return vertex[1];
			}   
			   
		}
		return null;
	}
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
			if( Vector2.Dot( (q0 - p0), u ) >= 0 && Vector2.Dot( (q0 - p0), u ) <= Vector2.Dot( u, u ) )
				return 2;
			if( Vector2.Dot( (p0 - q0), v ) >= 0 && Vector2.Dot( (p0 - q0), v ) <= Vector2.Dot( v, v ) )
				return 2;
			return 0;
		}
		//Case 3 - Parallel
		if (denom == 0 && numerator2 != 0)
			return 0;
		
		//Case 4 - Intersects
		double s = numerator1 / denom;
		double t = numerator2 / denom;
		
//		if ((s >= 0 && s <= 1) && (t >= 0 && t <= 1))
		if ((s >= -0.0001f && s <= 1.0001f) && (t >= -0.0001f && t <= 1.0001f)){
			if( vertex[0].Equals(param.vertex[0]) || vertex[0].Equals(param.vertex[1])
			   || vertex[1].Equals(param.vertex[0]) || vertex[1].Equals(param.vertex[1]))
				return 0;
			else
				return 1;
		}
		
		return 0; 
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

	private double CrossProduct( Vector2 a, Vector2 b ){
		return (a.x * b.y) - (a.y * b.x);
	}

	int EndPointIntersecion( Vector3 pt, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4 ){
			Vector3 pt2;
		//Find common endpoint
		if (v1 == v3)
			pt2 = v1;
		else if( v1 == v4 )
			pt2 = v1;
		else if( v2 == v3 )
			pt2 = v2;
		else if( v2 == v4 )
			pt2 = v2;
		else
			return 0;
		
		return 1;
		/*if (Math.Abs (pt.x - pt2.x) < 0.01 && Math.Abs (pt.z - pt2.z) < 0.01)
			return 1; //Endpoint and intersection point same
		else
			return 2;*/
	}
	public List<Vector3> PointsOnEitherSide(float incremnt)
	{
		float gradx = (vertex [1].x - vertex [0].x);
		float gradz = (vertex [1].z - vertex [0].z);
		List<Vector3> pair = new List<Vector3> ();
		Vector3 mp = MidPoint();
		//float incremnt = 0.02f;
		float c = 0.0f;

		if(Mathf.Abs(gradz)>0.00001f)
		{

			float m=-gradx/gradz;
			//Debug.Log("NOT || to z axis. m="+m+" gradz="+gradz);
			c = mp.z-m*mp.x;
			pair.Add(new Vector3(mp.x+incremnt,mp.y,m*(mp.x+incremnt)+c));
			pair.Add(new Vector3(mp.x-incremnt,mp.y,m*(mp.x-incremnt)+c));
			//Debug.Log(pair[0]+" , "+pair[1]);
		}
		else
		{
			//Debug.Log("|| to z axis");
			pair.Add(new Vector3(mp.x,mp.y,mp.z+incremnt));
			pair.Add(new Vector3(mp.x,mp.y,mp.z-incremnt));
			//Debug.Log(pair[0]+" , "+pair[1]);
		}
		return pair;
	}
	public bool PointOnLine( Vector3 pt )
	{
		float limit1 = 0.0001f;
		float limit2 = 0.0001f;
		//Debug.Log ("In PointOnLine");
		float minVal = Mathf.Min(Vector3.SqrMagnitude (vertex [0] - pt),Vector3.SqrMagnitude (vertex [1] - pt));
		//Debug.Log (minVal);
		if (minVal <= limit1)
			//if(vertex[0]==pt || vertex[1]==pt)
			//if (Mathf.Abs(Vector3.Magnitude(vertex [0] - pt)) < limit1 || Mathf.Abs(Vector3.Magnitude (vertex [1] - pt)) < limit1)
			return true;
		if (pt.z >= Mathf.Min (vertex [0].z, vertex [1].z) && pt.z <= Mathf.Max (vertex [0].z, vertex [1].z)
		    &&	pt.x >= Mathf.Min (vertex [0].x, vertex [1].x) && pt.x <= Mathf.Max (vertex [0].x, vertex [1].x))
		{
		}
		else
			return false;

		float val = 0.0f;
		float gradx = (vertex [1].x - vertex [0].x);
		if(gradx!=0)
		{
			float m=(vertex[1].z - vertex[0].z)/gradx;
			//Debug.Log ("Slope = "+m);
			float c = vertex[1].z-m*vertex[1].x;
			val = pt.x*m+c-pt.z;
			if(Mathf.Abs(val)<=limit2)
				return true;
		}
		else
		{
			val = vertex[1].x-pt.x;
			if(Mathf.Abs(val)<=limit2)
				return true;
		}
		//Debug.Log ("Line is " + vertex [1] + " to " + vertex [0]);
		//Debug.Log ("Not on line = "+val+", Pt = "+pt);
		return false;
		Vector3 diff = vertex[1] - vertex[0];




		if (pt.z >= Math.Min (vertex [0].z, vertex [1].z) && pt.z <= Math.Max (vertex [0].z, vertex [1].z)
					&&	pt.x >= Math.Min (vertex [0].x, vertex [1].x) && pt.x <= Math.Max (vertex [0].x, vertex [1].x))
		{
			float dist = Mathf.Abs(diff.z*pt.x-diff.x*pt.z -vertex[0].x*vertex[1].z+vertex[1].x*vertex[0].z);
			dist=dist/Mathf.Sqrt(diff.x*diff.x+diff.z*diff.z);
			//Debug.Log("Point on line checking. Dist = "+dist);
			if(dist<limit2)
			{
				//Debug.Log("Point on line found");
				return true;
			}
		}
				
			return false;
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
