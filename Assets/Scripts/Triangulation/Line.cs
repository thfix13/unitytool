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

    public bool debuggery = false;
    public bool debuggery2 = false;

    public Vector3[] vertex = new Vector3[2];
	public Color[] colours = new Color[2]; 
	public string name = "Vector Line";

	public List<Vector2> listGrid;
	public List<int> valueGrid = new List<int>();
	public Color costColor = Color.black;
    private float threshold = 0.00025f;

	//Material lineMaterial = Resources.Load ("Arrow", typeof(Material)) as Material;
	//Texture2D backTex = Resources.Load ("arrowStart", typeof(Texture2D)) as Texture2D;

	public Line(Vector3 v1, Vector3 v2)
	{


		vertex[0] = v1; 
		vertex[1] = v2; 
		colours[0] = Color.cyan;
		colours[1] = Color.cyan; 
	}

	public  bool Equals(Line l)
	{

        if( (vertex[0] - l.vertex[0]).magnitude < threshold) {
            if((vertex[1] - l.vertex[1]).magnitude < threshold) {
                return true;
            }
            else {
                return false;
            }
        }
        else if( (vertex[0] - l.vertex[1]).magnitude < threshold) {
            if((vertex[1] - l.vertex[0]).magnitude < threshold){
                return true;
            }
            else {
                return false;
            }
        }
        else {
            return false;
        }

        //Comparing midpoints using equals is just dumb.
		//return this.MidPoint().Equals(l.MidPoint());
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
		

		VectorLine line = new VectorLine(name,new List<Vector3>(vertex),2.0f);
		line.color = c;
		line.rectTransform.SetParent(parent.transform);
		//line.rectTransform.name = name;
		line.Draw3D();
	}

	public void DrawVector(GameObject parent)
	{
		Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
		                           UnityEngine.Random.Range(0.0f,1.0f),
		                           UnityEngine.Random.Range(0.0f,1.0f)) ;

		VectorLine line = new VectorLine("Line",new List<Vector3>(vertex),8.0f);
		line.color = c;
		line.rectTransform.SetParent(parent.transform);
		//line.rectTransform.name = name;
		line.Draw3D();
	}

	public void DrawVector(GameObject parent,Color c)
	{
	
		VectorLine line = new VectorLine("Line",new List<Vector3>(vertex),2.0f);
		line.color = c;
		line.rectTransform.SetParent(parent.transform);
		line.Draw3D();
	}

	public void DrawVector(GameObject parent,Color c, float linewidth)
	{
		
		VectorLine line = new VectorLine("Line",new List<Vector3>(vertex),linewidth);
		line.color = c;
		line.rectTransform.SetParent(parent.transform);
		line.Draw3D();
	}
	public void DrawArrow(GameObject parent, Color c, float linewidth){
		VectorLine line = new VectorLine("Line",new List<Vector3>(vertex), linewidth);
		line.color = c;
		line.rectTransform.SetParent(parent.transform);
		
		line.endCap = "Arrow";
		line.Draw3D ();


	}

	public float fractionSizeArrow = 0.1f;
	public float angleArrow = 15f;


	public void DrawManArrow(GameObject parent, Color c, float linewidth){
		VectorLine line = new VectorLine("Line",new List<Vector3>(vertex), linewidth);
		line.color = c;
		//line.rectTransform = new GameObject().transform;

		line.Draw3D ();
		//Debug.Log (line);
		//Debug.Log (line.rectTransform);
		//Debug.Log (parent);
		//Debug.Log (parent.transform);
		line.rectTransform.SetParent(parent.transform);


		//line.Draw();


		Vector3 startPoint = vertex[0];
		Vector3 endPoint = vertex[1];
		//float dist = Vector2.Distance(startPoint, endPoint);
		//float size = fractionSizeArrow * dist;


		Vector3 linePoint1 = endPoint + ((startPoint-endPoint) * fractionSizeArrow);
		Vector3 linePoint2 = linePoint1;

		linePoint1.x = endPoint.x + ((linePoint1.x - endPoint.x) * Mathf.Cos (Mathf.Deg2Rad * angleArrow)) - ((linePoint1.z - endPoint.z) * Mathf.Sin (Mathf.Deg2Rad * angleArrow));
		linePoint1.z = endPoint.z + ((linePoint1.x - endPoint.x) * Mathf.Sin (Mathf.Deg2Rad * angleArrow)) + ((linePoint1.z - endPoint.z) * Mathf.Cos (Mathf.Deg2Rad * angleArrow));

		linePoint2.x = endPoint.x + ((linePoint2.x - endPoint.x) * Mathf.Cos (Mathf.Deg2Rad * -angleArrow)) - ((linePoint2.z - endPoint.z) * Mathf.Sin (Mathf.Deg2Rad * -angleArrow));
		linePoint2.z = endPoint.z+ ((linePoint2.x - endPoint.x) * Mathf.Sin (Mathf.Deg2Rad * -angleArrow)) + ((linePoint2.z - endPoint.z) * Mathf.Cos (Mathf.Deg2Rad * -angleArrow));
	
		Vector3[] vertex1 = new Vector3[] {linePoint1, endPoint};
		Vector3[] vertex2 = new Vector3[] {linePoint2, endPoint};

		VectorLine line1 = new VectorLine("Line",new List<Vector3>(vertex1), linewidth);
		line1.color = c;
		//line1.rectTransform = new GameObject().transform;
		line1.rectTransform.SetParent(parent.transform);
		//line1.Draw ();
		line1.Draw3D ();

		VectorLine line2 = new VectorLine("Line",new List<Vector3>(vertex2), linewidth);
		line2.color = c;
		//line2.rectTransform = new GameObject().transform;
		line2.rectTransform.SetParent(parent.transform);
	
		//line2.Draw ();
		line2.Draw3D ();




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
		
		Vector2 p0 = new Vector2(a.x,a.z);
        Vector2 p1 = new Vector2(b.x,b.z);
        Vector2 u = p1 - p0;
		
		Vector2 q0 = new Vector2(c.x,c.z);
        Vector2 q1 = new Vector2(d.x,d.z);
        Vector2 v = q1 - q0;

        Vector2 w = new Vector2(a.x,a.z) - new Vector2(d.x,d.z);
		
		
		//if (u.x * v.y - u.y*v.y == 0)
		//	return true;
		
		float s = (v.y* w.x - v.x*w.y) / (v.x*u.y - v.y*u.x);
		float t = (u.x*w.y-u.y*w.x) / (u.x*v.y- u.y*v.x); 
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
		
		
		Vector2 p0 = new Vector2 (a.x, a.z);
		Vector2 p1 = new Vector2 (b.x, b.z);
        Vector2 u = p1 - p0;
		
		Vector2 q0 = new Vector2 (c.x, c.z);
		Vector2 q1 = new Vector2 (d.x, d.z);
        Vector2 v = q1 - q0;
		
		Vector2 w = new Vector2 (a.x, a.z) - new Vector2 (d.x, d.z);
		
		
		//if (u.x * v.y - u.y*v.y == 0)
		//	return true;
		
		float s = (v.y * w.x - v.x * w.y) / (v.x * u.y - v.y * u.x);
		//float t = (u.x * w.y - u.y * w.x) / (u.x * v.y - u.y * v.x); 
		
        
        //Debug.Log(s); 
		//Debug.Log(t); 
		

			//Interpolation
		Vector3 r = a + (b-a)*(float)s; 
		return r; 
		//}
		


		//return Vector3.zero; 
	}
	private bool CounterClockWise(Vector3 v1,Vector3 v2,Vector3 v3)
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

    //Intersection Detection
    // 0 -- two distinct lines
    // 1 -- normal intersection
    // 2-6 = lines are colinear and overlapping -> line1 = p0-p1, line2 = q0-q1
    //2 - > p0 q1
    //3 -> q0 p1
    //4 -> q1 p1
    //5 -> p0 p1
    //6 -> q0 q1
    public int LineIntersectMuntac(Line param) {
        Vector3 a = this.vertex[0];
        Vector3 b = this.vertex[1];
        Vector3 c = param.vertex[0];
        Vector3 d = param.vertex[1];

        
        Vector2 p0 = new Vector2(a.x, a.z);
        Vector2 p1 = new Vector2(b.x, b.z);
        Vector2 u = p1 - p0;

        Vector2 q0 = new Vector2(c.x, c.z);
        Vector2 q1 = new Vector2(d.x, d.z);
        Vector2 v = q1 - q0;

        float numerator1 = CrossProduct((q0 - p0), v);
        float numerator2 = CrossProduct((q0 - p0), u);
        float denom = CrossProduct(u, v);

        float posNum1 = numerator1;
        float posNum2 = numerator2;
        float posDenom = denom;
        if(numerator1 < 0)
        {
            posNum1 = -numerator1;
        }
        if(numerator2 < 0)
        {
            posNum2 = -numerator2;
        }
        if(denom < 0)
        {
            posDenom = -denom;
        }

        if (debuggery2) {
            Debug.Log(p0 + "," + p1 + "," + q0 + "," + q1);
            Debug.Log(u + "," + p0 + "," + v + "," + q0);
            Debug.Log(numerator1 + "," + numerator2 + "," + denom);
            //Debug.Log(Mathf.Approximately(numerator1, 0) + "," + Mathf.Approximately(numerator2, 0) + "," + Mathf.Approximately(denom, 0));
            Debug.Log(posDenom + "," + posNum1 + "," + posNum2);
            Debug.Log(Vector2.Dot((q0 - p0), u) + "," + Vector2.Dot(u, u));
            Debug.Log(Vector2.Dot((p0 - q0), v) + "," + Vector2.Dot(v, v));
            Debug.Log((p0 - q0).magnitude);
            Debug.Log(p0 + "," + q0);
            /*
            Debug.Log((p0 - q1).magnitude < threshold);
            Debug.Log((p0 - q1).magnitude);
            Debug.Log(p0);
            Debug.Log(q1);
            Debug.Log(p0 - q1);
            Debug.Log(Vector2.Dot((p0 - q1), (p0 - q1)));
            Debug.Log((p0 - q1).x);
            Debug.Log((p0 - q1).y);*/

        }
       // if (debuggery)
        //{
            //if (Mathf.Approximately(a.z, 7.5f) && Mathf.Approximately(c.z, 7.5f) && Mathf.Approximately(b.z, 7.5f) && Mathf.Approximately(d.z, 7.5f))
         /*if((Mathf.Abs(a.z+7.5f) < 0.5) && (Mathf.Abs(b.z + 7.5f) < 0.5) && (Mathf.Abs(c.z + 7.5f) < 0.5) && (Mathf.Abs(d.z + 7.5f) < 0.5))
            {
            if(posDenom > threshold || posNum1 > threshold) { 
            Debug.Log(a + "," + b + "," + c + "," + d);
            Debug.Log(u + "," + p0 + "," + v + "," + q0);
            Debug.Log(numerator1 + "," + numerator2 + "," + denom);
            //Debug.Log(Mathf.Approximately(numerator1, 0) + "," + Mathf.Approximately(numerator2, 0) + "," + Mathf.Approximately(denom, 0));
            Debug.Log(posDenom + "," + posNum1 + "," + posNum2);
            Debug.Log(Vector2.Dot((q0 - p0), u) + "," + Vector2.Dot(u, u));
            Debug.Log(Vector2.Dot((p0 - q0), v) + "," + Vector2.Dot(v, v));
            Debug.Log((p0 - q0).magnitude);
            Debug.Log(p0 + "," + q0);
            }


        }*/
       // }


        //Case 1 - Colinear

        if((posDenom < threshold) && (posNum1 < threshold)) {
            //Debug.Log("CASE2");
            //Debug.Log(p0 + "," + p1 + "," + q0 + "," + q1);
            //if (Mathf.Approximately(denom, 0) && Mathf.Approximately(numerator1, 0)) {
            //if(denom == 0 && numerator1 == 0) { 
            //Case 2 - Colinear and Overlapping
            /*if (debuggery)
            {*/
            /*
            if ((Mathf.Abs(a.x + 7.5f) < 0.5) && (Mathf.Abs(b.x + 7.5f) < 0.5) && (Mathf.Abs(c.x + 7.5f) < 0.5) && (Mathf.Abs(d.x + 7.5f) < 0.5)) {
                Debug.Log("CASE2");
                Debug.Log(p0 + "," + p1 + "," + q0 + "," + q1);

                Debug.Log(Vector2.Dot((q0 - p0), u) + "," + Vector2.Dot(u, u));
                Debug.Log(Vector2.Dot((p0 - q0), v) + "," + Vector2.Dot(v, v));
                Debug.Log((p0 - q0).magnitude);
                Debug.Log((p1 - q0).magnitude);
                Debug.Log((p1 - q0).magnitude < threshold);
                Debug.Log(p0 + "," + q0);
            }*/
            /*}*/

            if (Vector2.Dot((q0 - p0), u) > threshold && Vector2.Dot((q0 - p0), u) < Vector2.Dot(u, u))
            {
                //Debug.Log(Vector2.Dot((q0 - p0), u));
                //Debug.Log(Vector2.Dot(u, u));
                //Debug.Log("Returning 2");
                if (Vector2.Dot((p0 - q0), v) > threshold && Vector2.Dot((p0 - q0), v) < Vector2.Dot(v, v)) {
                    return 4;
                }
                else {
                    return 2;
                }
            }
            if (Vector2.Dot((p0 - q0), v) > threshold && Vector2.Dot((p0 - q0), v) < Vector2.Dot(v, v))
            {
                //Debug.Log("Returning 3");
                if (((p0 - q0).magnitude + u.magnitude) > v.magnitude) {
                    return 3;
                }
                else {
                    return 6;
                }
            }
            if((p0- q0).magnitude < threshold) { 
            //if (Mathf.Approximately((p0 - q0).magnitude, 0)){
                //Debug.Log("Returning 4");
                if(Vector2.Dot(u, v)> 0) {
                    if(u.magnitude > v.magnitude) {
                        return 5;
                    }
                    else {
                        return 6;
                    }
                }
                else {
                    return 4;
                }
            }
            if ((p1 - q0).magnitude < threshold) {
                if(Vector2.Dot(u, v) > 0) {
                    return 2;
                }
                else {
                    return 5;
                }                
            }
            return 0;
        }
        //Debug.Log("CASE NOT 2");

        //Case 3 - Parallel
        //if (Mathf.Approximately(denom, 0) && !Mathf.Approximately(numerator2, 0)){
        if(denom == 0 && numerator2 == 0) { 
            return 0;
        }


        //Case 4 - Intersects
        float s = numerator1 / denom;
        float t = numerator2 / denom;

        if(((p1 - q0).magnitude < threshold) || ((p1 - q1).magnitude < threshold) || ((p0 - q0).magnitude < threshold) || ((p0 - q1).magnitude < threshold)){
            return 0;
        }

        if ((s > 0 && s < 1) && (t > 0 && t < 1)) {
            return 1;
        }
		return 0; 
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
		
		float numerator1 = CrossProduct ((q0 - p0), v);
		float numerator2 = CrossProduct ((q0 - p0), u);
		float denom = CrossProduct (u, v);

        float posNum1 = numerator1;
        float posNum2 = numerator2;
        float posDenom = denom;
        if (numerator1 < 0)
        {
            posNum1 = -numerator1;
        }
        if (numerator2 < 0)
        {
            posNum2 = -numerator2;
        }
        if (denom < 0)
        {
            posDenom = -denom;
        }

        //Case 1 - Colinear
        if ((posDenom < 0.00001) && (posNum1 < 0.00001)){
       // 
            //if ( Mathf.Approximately(denom, 0) && Mathf.Approximately(numerator1, 0)) {
       // if (denom == 0 && numerator1 == 0){
            //Case 2 - Colinear and Overlapping
            //ACBD
            if ( Vector2.Dot( (q0 - p0), u ) > 0 && Vector2.Dot( (q0 - p0), u ) <= Vector2.Dot( u, u ) )
				return 2;
            //CABD
            if ( Vector2.Dot( (p0 - q0), v ) > 0 && Vector2.Dot( (p0 - q0), v ) <= Vector2.Dot( v, v ) )
				return 3;
			return 0;
		}
		//Case 3 - Parallel
		if (denom == 0 && numerator2 != 0)
			return 0;
		
		//Case 4 - Intersects
		float s = numerator1 / denom;
		float t = numerator2 / denom;
		
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
		
		float numerator1 = CrossProduct ((q0 - p0), v);
		//float numerator2 = CrossProduct ((q0 - p0), u);
		float denom = CrossProduct (u, v);


        float s = numerator1 / denom;
		//float t = numerator2 / denom;
		
		Vector3 r = a + (b-a)*(float)s; 
		return r;
	}

	private float CrossProduct( Vector2 a, Vector2 b ){
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

	public bool PointOnLine( Vector3 pt ){
		Vector3 diff = vertex[1] - vertex[0];
		//float grad = 0, cx, cz;
        //No idea what cx and cz were supposed to be.
        float grad = 0;
		float pa, pb;
		if (diff.x != 0 && diff.y != 0) {
			grad = diff.z / diff.x;
//			if( (pt.z - vertex[0].z) == ( (grad * pt.x) - vertex[0].x ) )
//				return true;
			pa = pt.z - vertex[0].z;
			pb = (grad * pt.x) - vertex[0].x;
			if( Math.Abs(pa - pb) < 0.1f)
			   return true;
		}
		else if( diff.x == 0 ){//A Z-axis parallel line
			if( pt.x == vertex[0].x  && (pt.z >= Math.Min(vertex[0].z, vertex[1].z) &&
			                             pt.z <= Math.Max(vertex[0].z, vertex[1].z) ) )
			   return true;
		}
		else if( diff.z == 0 ){//An x-axis parallel line
			if( pt.z == vertex[0].z  && (pt.x >= Math.Min(vertex[0].x, vertex[1].x) &&
			                             pt.x <= Math.Max(vertex[0].x, vertex[1].x) ) )
			   return true;
		}
		return false;
	}

    public override string ToString()
    {
        return vertex[0] + "," + vertex[1];
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
