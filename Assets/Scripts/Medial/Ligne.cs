// ------------------------------------------------------------------------------
//  Code derived from Triangulation/Line.cs
//  Color wasn't required for the line. Basic functionality like intersection etc used
// ------------------------------------------------------------------------------
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Medial{
	public class Ligne: IEquatable<Ligne>
	{

		public Vector3[] vertex = new Vector3[2];
		public int[]vertexIndex= new int[2];
		private Ligne(){}
		public Ligne(Vector3 v1, Vector3 v2, int v1index, int v2index)
		{
			vertex[0] = v1; 
			vertex[1] = v2; 
			vertexIndex[0]=v1index;
			vertexIndex[1]=v2index;
		}
		public bool ContainsVertex(int vertex){
			return vertex==this.vertexIndex[0]||vertex==this.vertexIndex[1];
		}
		public  bool Equals(Ligne l)
		{
			return this.MidPoint().Equals(l.MidPoint());
			
		}

		public override bool Equals(System.Object e){
			return e!=null && this.MidPoint().Equals(((Ligne)e).MidPoint()) ;
		}

	//	public static Ligne Zero
	//	{
	//		get { return new Ligne(Vector3.zero,Vector3.zero); }
	//	}
		public Vector3 MidPoint()
		{
			return new Vector3( (vertex[0].x + vertex[1].x)/2,
			                   (vertex[0].y + vertex[1].y)/2,
			                   (vertex[0].z + vertex[1].z)/2);
		}

		/// <summary>
		/// Get a point distance A away from the midpoint of line and at angle A
		/// </summary>
		/// <returns>The point at D and angle a.</returns>
		/// <param name="d">D.</param>
		/// <param name="a">The alpha component.</param>
		public Vector3 getPointAtDAndAngleA(float d, float a){
			return  this.MidPoint()+ Quaternion.AngleAxis(a,Vector3.up)*Vector3.forward*d;
		}

		public Vector3 GetOther(Vector3 v)
		{
			if(vertex[0]==v)
				return vertex[1];
			return vertex[0];
		}
		
		public void DrawLine(Color c)
		{
	//		Debug.Log(this.vertex[0]+" "+this.vertex[1]);
			Debug.DrawLine(this.vertex[0],this.vertex[1],c,10000f); 
		}
		public void DrawLine()
		{
			Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
			                    UnityEngine.Random.Range(0.0f,1.0f),
			                    UnityEngine.Random.Range(0.0f,1.0f)) ;
			
			Debug.DrawLine(this.vertex[0],this.vertex[1],c,10000f); 
		}
		public bool ShareVertex(Ligne l)
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
		
		/// <summary>
		/// Does L intersects 'this' object?
		/// </summary>
		public bool LineIntersection(Ligne L)
		{
			Vector3 a = L.vertex[0]; 
			Vector3 b = L.vertex[1];
			Vector3 c = vertex[0];
			Vector3 d = vertex[1];
			
			
			// a-b
			// c-d
			//if the same lines

			//When they are parallel and collinear, they intersect
			//they might share some point here, if they are parallel
			Vector3 cross= Vector3.Cross(a-b,c-d);
			if(cross==Vector3.zero && Vector3.Cross(a-c,a-b)== Vector3.zero && this.LinesOverlap(L))
				return true;

			//When share a point, say that they don't intersect
			if(a.Equals(c) || a.Equals(d) || b.Equals(c) || b.Equals(d))
				return false;//LineIntersect(a,b,c,d); 
			
			return CounterClockWise(a,c,d) != CounterClockWise(b,c,d) && 
				CounterClockWise(a,b,c) != CounterClockWise(a,b,d);
			
			//if( CounterClockWise(a,c,d) == CounterClockWise(b,c,d))
			//	return false;
			//else if (CounterClockWise(a,b,c) == CounterClockWise(a,b,d))
			//	return false; 
			//else 
			//	return true; 
			
			
		}
		private bool LinesOverlap(Ligne l){
			Vector3 a = l.vertex[0]; 
			Vector3 b = l.vertex[1];
			Vector3 c = vertex[0];
			Vector3 d = vertex[1];
			var dot= Vector3.Dot(b-a,c-a);
			var dist= Vector3.Distance(a,b);
			dist*=dist;
			if(dot>0 && dot< dist)
				return true;
			dot= Vector3.Dot(b-a,d-a);
			if(dot>0 && dot< dist)
				return true;
			return false;

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
//			Vector2 p0 = new Vector2(a.x,a.z); Vector2 p1 = new Vector2(b.x,b.z); 
			
			Vector2 v = new Vector2(d.x,d.z) - new Vector2(c.x,c.z);
//			Vector2 q0 = new Vector2(c.x,c.z); Vector2 q1 = new Vector2(d.x,d.z);
			
			Vector2 w = new Vector2(a.x,a.z) - new Vector2(d.x,d.z);
			
			
			//if (u.x * v.y - u.y*v.y == 0)
			//	return true;
			
			double s = (v.y* w.x - v.x*w.y) / (v.x*u.y - v.y*u.x);
			double t = (u.x*w.y-u.y*w.x) / (u.x*v.y- u.y*v.x); 
			//Debug.Log(s); 
			//Debug.Log(t); 
			
			if ( (s>0 && s< 1) && (t>0 && t< 1) )
				return true;
			
			return false; 
		}
		public static bool CounterClockWise(Vector3 v1,Vector3 v2,Vector3 v3)
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
	}
}


