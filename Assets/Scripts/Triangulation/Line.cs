using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 
using Vectrosity; 


//Changes Made:
//Used my implementation of line intersection function called: LineIntersectionMuntac (will change name later)
//Another function called GetIntersectionPoint
[Serializable]
public class Line 
{
	public float eps = 1e-5f;//the margin of accuracy for all floating point equivalence checks
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

	public void DrawVector(GameObject parent)
	{
//		Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
//		                           UnityEngine.Random.Range(0.0f,1.0f),
//		                           UnityEngine.Random.Range(0.0f,1.0f)) ;
//
		Color c = Color.white;
		//VectorLine line = new VectorLine("Line",vertex,c,null,2.0f);
		VectorLine line = new VectorLine("Line",vertex,c,null,0.1f);
		line.vectorObject.transform.parent = parent.transform;
		line.vectorObject.name = name;
		line.Draw3D();
	}

	public void DrawVector(GameObject parent,Color c)
	{
		VectorLine line = new VectorLine("Line",vertex,c,null,2.0f);
		//VectorLine line = new VectorLine("Line",vertex,c,null,0.1f);
		//VectorLine line = new VectorLine("Line",vertex,c,null,0.03f);
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

	public bool isEndPoint( Vector3 v ){
		if (VectorApprox (v, vertex [0]) || VectorApprox (v, vertex [1]))
			return true;
		else
			return false;
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
//		//Sorting the order of points
//		Vector3 a = getMinVert (this.vertex [0], this.vertex [1]);
//		Vector3 b = this.vertex[1];
//		if( a.Equals( this.vertex[1] ) )
//			b = this.vertex[0];
//		Vector3 c = getMinVert (param.vertex [0], param.vertex [1]);
//		Vector3 d = param.vertex[1];
//		if( c.Equals( param.vertex[1] ) )
//			d = param.vertex[0];

		Vector2 u = new Vector2 (b.x, b.z) - new Vector2 (a.x, a.z);
		Vector2 p0 = new Vector2 (a.x, a.z);
		
		Vector2 v = new Vector2 (d.x, d.z) - new Vector2 (c.x, c.z);
		Vector2 q0 = new Vector2 (c.x, c.z);
		
		double numerator1 = CrossProduct ((q0 - p0), v);
		double numerator2 = CrossProduct ((q0 - p0), u);
		double denom = CrossProduct (u, v);
		
		//Case 1 - Colinear
		//if ( denom == 0 && numerator2 == 0 ) {
		if ( floatCompare((float)denom, 0) && floatCompare((float)numerator2,0) ) {
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
		//if (denom == 0 && numerator2 != 0)
		if ( floatCompare((float)denom, 0) && !floatCompare((float)numerator2,0) )
			return 0;
		
		//Case 4 - Intersects
		double s = numerator1 / denom;
		double t = numerator2 / denom;
		
		//if ((s > 0 && s < 1) && (t > 0 && t < 1))
		if ((s > 0 + eps && s < 1 - eps) && (t > 0 + eps && t < 1 - eps))
			return 1;
		
		return 0;
	}

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
		
		//Case 1 - Colinear
		//if ( denom == 0 && numerator2 == 0 ) {
		if ( floatCompare((float)denom, 0) && floatCompare((float)numerator2,0) ) {
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
		//if (denom == 0 && numerator2 != 0)
		if ( floatCompare((float)denom, 0) && !floatCompare((float)numerator2,0) )
			return 0;
		
		//Case 4 - Intersects
		double s = numerator1 / denom;
		double t = numerator2 / denom;
		
		//if ((s > 0 && s < 1) && (t > 0 && t < 1))
		if ((s > 0 + eps && s < 1 - eps) && (t > 0 + eps && t < 1 - eps))
			return 1;
		
		return 0;
	}

	public int LineIntersectMuntacDebug (Line param){
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
		Debug.Log ("Denom " + denom + " num2" + numerator2);
		//Case 1 - Colinear
		//if ( denom == 0 && numerator2 == 0 ) {
		if ( floatCompare((float)denom, 0) && floatCompare((float)numerator2,0) ) {
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
		//if (denom == 0 && numerator2 != 0)
		if ( floatCompare((float)denom, 0) && !floatCompare((float)numerator2,0) )
			return 0;
		
		//Case 4 - Intersects
		double s = numerator1 / denom;
		double t = numerator2 / denom;
		Debug.Log ("s " + s + " t" + t);
		//if ((s > 0 && s < 1) && (t > 0 && t < 1))
		if ((s > 0 + eps && s < 1 - eps) && (t > 0 + eps && t < 1 - eps))
			return 1;
		
		return 0;
	}



//	public int LineIntersectMuntacDebug2 (Line param){
//		Vector3 a = this.vertex [0];
//		Vector3 b = this.vertex[1];
//		Vector3 c = param.vertex [0];
//		Vector3 d = param.vertex [1];
//
//		Vector2 u = new Vector2 (b.x, b.z) - new Vector2 (a.x, a.z);
//		Vector2 p0 = new Vector2 (a.x, a.z);
//		
//		Vector2 v = new Vector2 (d.x, d.z) - new Vector2 (c.x, c.z);
//		Vector2 q0 = new Vector2 (c.x, c.z);
//		
//		double numerator1 = CrossProduct ((q0 - p0), v);
//		double numerator2 = CrossProduct ((q0 - p0), u);
//		double denom = CrossProduct (u, v);
//		
//		//Case 1 - Colinear
//		//if ( denom == 0 && numerator2 == 0 ) {
//		Debug.Log ("Denom and Num" + denom + " " + numerator2);
//		if ( floatCompare((float)denom, 0) && floatCompare((float)numerator2,0) ) {
//			//Case 2 - Colinear and Overlapping
//			//if( Vector2.Dot( (q0 - p0), u ) >= 0 && Vector2.Dot( (q0 - p0), u ) <= Vector2.Dot( u, u ) )
//			if( floatCompare( Vector2.Dot( (q0 - p0), u ), 0, ">=" )
//			   && floatCompare( Vector2.Dot( (q0 - p0), u ), Vector2.Dot( u, u ), "<=" ) )
//				return 2;
//			//if( Vector2.Dot( (p0 - q0), v ) >= 0 && Vector2.Dot( (p0 - q0), v ) <= Vector2.Dot( v, v ) )
//			if( floatCompare( Vector2.Dot( (p0 - q0), v ), 0, ">=" )
//			   && floatCompare( Vector2.Dot( (p0 - q0), v ), Vector2.Dot( v, v ), "<=" ) )
//				return 2;
//			Debug.Log ("Colinear but not");
//			return 0;
//		}
//		//Case 3 - Parallel
//		//if (denom == 0 && numerator2 != 0)
//		if ( floatCompare((float)denom, 0) && !floatCompare((float)numerator2,0) )
//			return 0;
//		
//		//Case 4 - Intersects
//		double s = numerator1 / denom;
//		double t = numerator2 / denom;
//		
//		//if ((s > 0 && s < 1) && (t > 0 && t < 1))
//		Debug.Log ("s and t" + s + "," + t);
//		if ((s > 0 + eps && s < 1 - eps) && (t > 0 + eps && t < 1 - eps))
//			return 1;
//		
//		return 0;
//	}

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

	//Detects regular intersections UNLESS the lines share a vertex
	//An endpoint of one line could intersect the middle of another line (i.e. such an intersection would count as
	//intersectsion) and be detected
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
		//if ( denom == 0 && numerator2 == 0 ) {
		if ( floatCompare((float)denom, 0) && floatCompare((float)numerator2,0) ) {
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
		//if (denom == 0 && numerator2 != 0)
		if ( floatCompare((float)denom, 0) && !floatCompare((float)numerator2,0) )
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

	public int LineIntersectRegular (Line param){
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

	public bool POL( Vector3 pt ){
		return (this.PointOnLine (pt) && this.PointOnLineB (pt));
	}

	public bool PointOnLine( Vector3 pt ){
		if (floatCompare (new Line (this.vertex [0], pt).Magnitude () 
		                  + new Line (this.vertex [1], pt).Magnitude (), this.Magnitude ()))
			return true;
		else
			return false;
	}

	public bool PointOnLineB( Vector3 pt ){
		Vector3 a = this.vertex [0];
		Vector3 b = this.vertex [1];
		Vector3 c = pt;
		//Debug.Log ("IsLeft Value: " + ((b.x - a.x) * (pt.z - a.z) - (b.z - a.z) * (c.x - a.x)));
		return floatCompare( ((b.x - a.x)*(pt.z - a.z) - (b.z - a.z)*(c.x - a.x)), 0 );
	}

//	public bool PointOnLine( Vector3 pt ){
//		Vector3 diff = vertex[1] - vertex[0];
//		float grad = 0, cx, cz;
//		float pa, pb;
//		//if (diff.x != 0 && diff.z != 0) {
//		if( !floatCompare(diff.x, 0) && !floatCompare(diff.z, 0) ){
//			grad = diff.z / diff.x;
//			pa = pt.z - vertex[0].z;
//			pb = (grad * pt.x) - vertex[0].x;
//			if( floatCompare(pa, pb) )
//			   return true;
//		}
//		else if( floatCompare(diff.x, 0) ){//A Z-axis parallel line
//			if( floatCompare( pt.x, vertex[0].x )
//			   && ( floatCompare( pt.z, Math.Min(vertex[0].z, vertex[1].z), ">=" ) 
//			        && floatCompare( pt.z, Math.Max(vertex[0].z, vertex[1].z), "<=" ) ) )
//			   return true;
//		}
//		else if( floatCompare(diff.z, 0) ){//An x-axis parallel line
//			if( pt.z == vertex[0].z  && (pt.x >= Math.Min(vertex[0].x, vertex[1].x) &&
//			                             pt.x <= Math.Max(vertex[0].x, vertex[1].x) ) )
//			   return true;
//		}
//		return false;
//	}
//
//
//	public bool PointOnLineB( Vector3 pt ){
//		if (pt.x >= Math.Min (vertex [0].x, vertex [1].x) && pt.x <= Math.Max (vertex [0].x, vertex [1].x) 
//				&& pt.z >= Math.Min (vertex [0].z, vertex [1].z) && pt.z <= Math.Max (vertex [0].z, vertex [1].z)) {
//				Vector3 a = this.vertex [0];
//				Vector3 b = this.vertex [1];
//				Vector3 c = pt;
//				return ((b.x - a.x) * (pt.z - a.z) - (b.z - a.z) * (c.x - a.x)) == 0;
//		} 
//		else
//			return false;
//	}

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
	public bool VectorApprox ( Vector3 a, Vector3 b ){
		if( Math.Abs (a.x - b.x) < eps && Math.Abs (a.z - b.z) < eps )
			return true;
		else
			return false;
	}

	public bool floatCompare ( float a, float b ){
		return Math.Abs (a - b) < eps;
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
