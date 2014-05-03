using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ============================================================================
// Small library of 2D geometry primitives and functions, where everything is
// considered to be projected on the XZ-Plane.
// ============================================================================

namespace GeometryLib {
	/**
	 * An edge between two points (a and b).
	 */
	public struct Edge3Abs {
		public Vector3 a;
		public Vector3 b;
	
		/**
		 * Constructs an edge between a and b.
		 */
		public Edge3Abs(Vector3 a, Vector3 b)
		{
			this.a = a;
			this.b = b;
		}
		
		/**
		 * Converts this Edge3Abs to an Edge3Rel and returns that.
		 */
		public Edge3Rel ToRel()
		{
			return new Edge3Rel (a, b - a);
		}
	
		/**
		 * Returns the vector from a to b.
		 */
		public Vector3 GetDiff() {
			return b - a;
		}
	
		/**
		 * Performs an intersection test in 2D on the XZ plane between two line segments.
		 * Returns:
		 *     The intersection point if there is an intersection
		 *     Vector3(Float.nan, Float.nan, Float.nan) if there is not
		 */
		public Vector3 IntersectXZ(Edge3Abs other) {
			Vector3 p1 = a;
			Vector3 p2 = b;
	
			Vector3 q1 = other.a;
			Vector3 q2 = other.b;
	
			float d = (p1.x-p2.x)*(q1.z-q2.z) - (p1.z-p2.z)*(q1.x-q2.x);
	
			if (d == 0)
				return new Vector3 (float.NaN, float.NaN, float.NaN);
	
			float xi = ((q1.x-q2.x)*(p1.x*p2.z-p1.z*p2.x)-(p1.x-p2.x)*(q1.x*q2.z-q1.z*q2.x))/d;
	
			if (xi < Mathf.Min (p1.x, p2.x) || xi > Mathf.Max (p1.x, p2.x))
				return new Vector3 (float.NaN, float.NaN, float.NaN);
			if (xi < Mathf.Min (q1.x, q2.x) || xi > Mathf.Max (q1.x, q2.x))
				return new Vector3 (float.NaN, float.NaN, float.NaN);
	
			float yi = ((q1.z-q2.z)*(p1.x*p2.z-p1.z*p2.x)-(p1.z-p2.z)*(q1.x*q2.z-q1.z*q2.x))/d;
	
			return new Vector3 (xi, a.y, yi);
		}
		
		/**
		 * Performs an intersection test in 2D on the XZ plane between two line segments.
		 * Returns:
		 *     The intersection point if there is an intersection
		 *     Vector3(Float.nan, Float.nan, Float.nan) if there is not
		 */
		public Vector3 IntersectXZ(Edge3Rel other) {
			return IntersectXZ (other.ToAbs ());
		}
		
		/**
		 * Equality test with another edge.
		 */ 
		public override bool Equals (object other)
		{
			if (other is Edge3Abs) {
				return (a == ((Edge3Abs)other).a && b == ((Edge3Abs)other).b) ||
					(b == ((Edge3Abs)other).a && a == ((Edge3Abs)other).a);
			} else if (other is Edge3Rel) {
				return (a == ((Edge3Rel)other).pos && b == ((Edge3Rel)other).pos + ((Edge3Rel)other).vec) ||
					(a == ((Edge3Rel)other).pos + ((Edge3Rel)other).vec && b == ((Edge3Rel)other).pos);
			}
			return false;
		}
	
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}
	
	/**
	 * An edge between a point (pos) and its offset (vec)
	 */
	public struct Edge3Rel {
		public Vector3 pos;
		public Vector3 vec;
		
		/**
		 * Constructs an edge from a point, to another with the specified offset.
		 */
		public Edge3Rel(Vector3 pos, Vector3 vec)
		{
			this.pos = pos;
			this.vec = vec;
		}
		
		/**
		 * Converts this edge to an Edge3Abs, and returns that.
		 */
		public Edge3Abs ToAbs()
		{
			return new Edge3Abs (pos, pos + vec);
		}
		
		/**
		 * Performs an intersection test in 2D on the XZ plane between two line segments.
		 * Returns:
		 *     The intersection point if there is an intersection
		 *     Vector3(Float.nan, Float.nan, Float.nan) if there is not
		 */
		public Vector3 IntersectXZ(Edge3Abs other) {
			return (ToAbs ().IntersectXZ (other));
		}
		
		/**
		 * Performs an intersection test in 2D on the XZ plane between two line segments.
		 * Returns:
		 *     The intersection point if there is an intersection
		 *     Vector3(Float.nan, Float.nan, Float.nan) if there is not
		 */
		public Vector3 IntersectXZ(Edge3Rel other) {
			return (ToAbs ().IntersectXZ (other.ToAbs()));
		}
		
		/**
		 * Equality test with another edge.
		 */
		public override bool Equals (object other)
		{
			if (other is Edge3Abs) {
				return (pos == ((Edge3Abs)other).a && pos + vec == ((Edge3Abs)other).b) ||
					(pos == ((Edge3Abs)other).b && pos + vec == ((Edge3Abs)other).a);
			} else if (other is Edge3Rel) {
				return (pos == ((Edge3Rel)other).pos && vec == ((Edge3Rel)other).vec) ||
					(pos == ((Edge3Rel)other).pos + vec && vec == -((Edge3Rel)other).vec);
			}
			return false;
		}
	
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}
	
	/**
	 * An edge between two (externally) indexed vertices.
	 */
	public struct IndexEdge {
		public int i1;
		public int i2;
		
		/**
		 * Constructs an edge between two indices.
		 */
		public IndexEdge(int a, int b) {
			i1 = a;
			i2 = b;
		}
		
		/**
		 * Returns the Edge3Abs corresponding to this IndexEdge, in a given array of vertices.
		 */
		public Edge3Abs GetEdge(Vector3[] vertices) {
			return new Edge3Abs (vertices[i1 % vertices.Length], vertices[i2 % vertices.Length]);
		}
		
		/**
		 * Equality test with another IndexEdge.
		 */
		public override bool Equals (object other)
		{
			if (other is IndexEdge) {
				return ((IndexEdge)other).i1 == i1 && ((IndexEdge)other).i2 == i2;
			}
			return false;
		}
	
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}
	
	/**
	 * A Pose has a position, a rotation, a velocity and an angular velocity.
	 */ 
	public struct Pose {
		public Vector3 position;
		public Quaternion rotationQ;
		public Vector3 velocity;
		public float omega;
		
		/**
		 * Constructs a pose with no speed whatsoever.
		 */
		public Pose(Vector3 position, Quaternion rotation) {
			this.position = position;
			this.rotationQ = rotation;
			velocity = Vector3.zero;
			velocity.y = 1;
			omega = 0;
		}
		
		/**
		 * Constructs a pose with no speed whatsoever.
		 */
		public Pose(float x, float time, float z, float rotation) {
			this.position = new Vector3(x, time, z);
			this.rotationQ = Quaternion.Euler(0, rotation, 0);
			velocity = Vector3.zero;
			velocity.y = 1;
			omega = 0;
		}
		
		public float posX {
			get {
				return position.x;
			}
			set {
				position.x = value;
			}
		}
		
		public float time {
			get {
				return position.y;
			}
			set {
				position.y = value;
			}
		}
		
		public float posZ {
			get {
				return position.z;
			}
			set {
				position.z = value;
			}
		}
		
		public float rotation {
			get {
				return rotationQ.eulerAngles.y;
			}
			set {
				rotationQ = Quaternion.Euler(0, value, 0);
			}
		}
		
		public float velX {
			get {
				return velocity.x;
			}
			set {
				velocity.x = value;
			}
		}
		
		public float velZ {
			get {
				return velocity.z;
			}
			set {
				velocity.z = value;
			}
		}
	}
	
	/**
	 * Handedness of a shape.
	 */
	public enum Handedness {
		Right, Left, Unknown
	}
	
	/**
	 * A circle with a center and a radius.
	 * Or a sphere, really.
	 */
	public struct Circle {
		public Vector3 center;
		public float radius;
	}
	
	public class Shape3: IEnumerable {
		private List<Vector3> vertices = new List<Vector3>();
		private Handedness hand = Handedness.Unknown;
		
		/**
		 * Returns the handedness of this shape by verifying which area is positive.
		 * If the handedness is already known, and the shape has not changed,
		 * no recalculation is done.
		 */
		public Handedness handedness {
			get {
			
				if (Count < 3) {
					hand = Handedness.Unknown;
					return hand;
				}
				
				if (hand == Handedness.Unknown) {
					
					float sum = 0;
					
					for (int i = 0; i < Count; i++) {
						Edge3Abs e1 = GetEdge(i);
						Edge3Abs e2 = GetEdge(i+1);
						
						sum += Vector3.Cross(e1.GetDiff(), e2.GetDiff()).y;
					}
					
					if (sum == 0) {
						hand = Handedness.Unknown;
					} else if (sum < 0) {
						hand = Handedness.Right;
					} else {
						hand = Handedness.Left;
					}
				}
				
				return hand;
			}
		}
		
		/**
		 * TODO
		 */
		public Circle BoundingCircle() {
			Circle ret = new Circle();
			
			return ret;
		}
		
		/**
		 * Verifies whether this shape collides with another by verifying
		 * whether any of the other shape's points lie inside this shape.
		 * Hence it being dumb.
		 */ 
		public bool DumbCollision(Shape3 other) {
			if (Count > other.Count) {
				return other.DumbCollision(this);
			}
			
			foreach (Vector3 p in vertices) {
				if (other.PointInside(p)) {
					return true;
				}
			}
			return false;
		}
		
		/**
		 * Adds a vertex to the shape.
		 */
		public void addVertex(Vector3 vert) {
			hand = Handedness.Unknown;
			vertices.Add (vert);
		}
	
		/**
		 * Blanks the shape.
		 */
		public void Clear() {
			hand = Handedness.Unknown;
			vertices.Clear ();
		}
		
		/**
		 * Wraparounds the vertices by a certain offset.
		 */
		public void Offset(int offset) {
			offset = ((offset % vertices.Count) + vertices.Count) % vertices.Count;
			List<Vector3> newList = new List<Vector3> ();
	
			for (int i = offset; i - offset < vertices.Count; i++) {
				newList.Add(vertices[i % vertices.Count]);
			}
	
			vertices = newList;
		}
		
		/**
		 * Returns all the vertices, in an array.
		 */ 
		public Vector3[] Vertices() {
			return vertices.ToArray();
		}
	
		/**
		 * Enumerator for the vertices in their insertion order.
		 */
		public IEnumerator GetEnumerator()
		{
			for (int i=0; i<vertices.Count; i++) {
				yield return new Edge3Abs(vertices[i], vertices[(i+1) % vertices.Count]);
			}
		}
		
		/**
		 * Enumerator for the vertices in the reverse order.
		 */
		public IEnumerator Reverse()
		{
			for (int i = vertices.Count; i > 0; i--) {
				yield return new Edge3Abs(vertices[i % vertices.Count], vertices[i - 1]);
			}
		}
		
		/**
		 * Vertex getter[].
		 */ 
		public Vector3 this[int i]
		{
			get { return vertices [i]; }
			set { vertices [i] = value; }
		}
		
		/**
		 * Returns the edge following the ith vertex.
		 */ 
		public Edge3Abs GetEdge(int i)
		{
			i = ((i % vertices.Count) + vertices.Count) % vertices.Count;
			return new Edge3Abs (vertices [i], vertices [(i + 1) % vertices.Count]);
		}
	
		/**
		 * Number of vertices in this shape.
		 */ 
		public int Count {
			get {
				return vertices.Count;
			}
		}
		
		/**
		 * Returns whether a points lies within the shape or not.
		 */ 
		public bool PointInside(Vector3 point) {
			bool c = false;
			int i = 0, j = 0;
	
			for (i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++) {
				if ( ((vertices[i].z > point.z) != (vertices[j].z > point.z)) &&
				     (point.x < (vertices[j].x - vertices[i].x) * (point.z - vertices[i].z) / (vertices[j].z - vertices[i].z) + vertices[i].x) )
					c = !c;
			}
	
			return c;
		}
		
		/**
		 * 2D area of the shape, as projected on the XZ-plane.
		 */ 
		public float Area {
			get {
				// Find lowest z for offset
				float lowestZ = float.PositiveInfinity;
				foreach (Vector3 v in vertices) {
					if (v.z < lowestZ) {
						lowestZ = v.z;
					}
				}
				
				float area = 0;
				foreach (Edge3Abs e in this) {
					float subArea = 0.5f * (e.a.z + e.b.z) + lowestZ;
					subArea *= (e.b.x - e.a.x);
					area += subArea;
				}
				
				return Mathf.Abs(area);
			}
		}
	}
	
	public class SetOfPoints {
		public HashSet<Vector3> points;
		Shape3 hull;
		bool dirty;
		
		/**
		 * Constructs an empty set of point.
		 */ 
		public SetOfPoints() {
			points = new HashSet<Vector3>();
			hull = new Shape3();
			dirty = true;
		}
		
		/**
		 * Adds a point to the set of points.
		 * Returns the number of points after inserting the point.
		 */
		public int AddPoint(Vector3 point)
		{
			points.Add(point);
			dirty = true;
			return points.Count;
		}
		
		/**
		 * Returns the convex hull of the set of point. Calculated with the gift-wrapping algorithm (O(nh)).
		 * If the set of point has not changed since the last invocation, then the convex hull will not
		 * be re-calculated.
	 	 */
		public Shape3 ConvexHull()
		{
			if (dirty) {
				List<Vector3> ptList = points.ToList();
				hull.Clear();
				
				if (ptList.Count == 0) {
					return hull;
				} else if (ptList.Count == 1) {
					hull.addVertex(ptList[0]);
					return hull;
				}
				
				Vector3 pointOnHull = LeftMost(ptList);
				Vector3 endpoint;
				Vector3 start = pointOnHull;
				
				do {
					hull.addVertex(pointOnHull);
					endpoint = ptList[0];
					for (int j = 1; j < ptList.Count; j++) {
						if (pointOnHull == endpoint || LeftOfLine(ptList[j], pointOnHull, endpoint)) {
							endpoint = ptList[j];
						}
					}
					pointOnHull = endpoint;
				} while (start != endpoint);
				dirty = false;
				return hull;
			} else {
				return hull;
			}
		}
		
		/**
		 * Returns the point that is the left most, on the X-axis.
		 */ 
		private static Vector3 LeftMost(List<Vector3> points) {
			Vector3 leftMost = points[0];
			float leftX = leftMost.x;
			
			foreach (Vector3 p in points) {
				if (p.x < leftX) {
					leftMost = p;
					leftX = p.x;
				}
			}
			
			return leftMost;
		}
		
		private static bool LeftOfLine(Vector3 p2, Vector3 p0, Vector3 p1) {
			return (p1.x - p0.x)*(p2.z - p0.z) - (p2.x - p0.x)*(p1.z - p0.z) > 0;
		}
	}
}
