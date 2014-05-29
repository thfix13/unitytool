using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using GeometryLib;

namespace Spatiotemporal {
	[ExecuteInEditMode]
	public abstract class StealthFov : MeshMapChild {
		public static bool debug = true;
		
		public float viewDist_ = 30f;
		
		public float fieldOfView_ = 50f;
		public static float minFov { get { return 1;} }
		public static float maxFov { get { return 359;} }
		
		public int frontSegments_ = 8;
		public int maxSegments { get { return Mathf.CeilToInt(fieldOfView_/22.5f); } }
		public int minSegments { get { return Mathf.CeilToInt(fieldOfView_/90); } }
		
		public float viewDistance
		{
			get { return viewDist_; }
			set {
				// disable once CompareOfFloatsByEqualityOperator
				if (value != viewDist_) {
					dirty = true;
					viewDist_ = value;
					Validate();
				}
			}
		}
		
		public float fieldOfView
		{
			get { return fieldOfView_; }
			set {
				// disable once CompareOfFloatsByEqualityOperator
				if (value != fieldOfView_) {
					dirty = true;
					fieldOfView_ = value;
					Validate();
				}
			}
		}
		
		public int frontSegments
		{
			get { return frontSegments_; }
			set {
				if (value != frontSegments_) {
					dirty = true;
					frontSegments_ = value;
					Validate();
				}
			}
		}
		
		private List<Shape3> shapes_ = null;
		private List<Shape3> shapes
		{
			get {
				if (shapes_ == null || dirty) {
					shapes_ = Shapes();
				}
				return shapes_;
			}
		}
		
		public static bool calculateEasiness = true;
		public SetOfPoints setOfPoints = new SetOfPoints();
		public Shape3 convexHull { get { return setOfPoints.ConvexHull(); } }
		
		public float easiness
		{
			get {
				if (!calculateEasiness) return float.NaN;
				float volume = convexHull.Area * map.timeLength;
				return Mathf.Max(0, (volume - vlm_)/volume);
			}
		}
		
		public SetOfPoints combined = new SetOfPoints();
		public float combinedEasiness
		{
			get {
				if (!calculateEasiness) return float.NaN;
				combined.points.Clear();
				
				foreach (Vector3 v in setOfPoints.points) {
					combined.AddPoint(v);
				}
				
				float totalVol = 0;
				Shape3 cvHull = convexHull;
				foreach (StealthGuard sg in map.GetGuards()) {
					if (cvHull.SATCollision(sg.convexHull)) {
						foreach(Vector3 v in sg.setOfPoints.points) {
							combined.AddPoint(v);
						}
						totalVol += sg.shVolume;
					}
				}
				foreach (StealthCamera c in map.GetCameras()) {
					if (cvHull.SATCollision(c.convexHull)) {
						foreach(Vector3 v in c.setOfPoints.points) {
							combined.AddPoint(v);
						}
						totalVol += c.shVolume;
					}
				}
				
				float volume = combined.ConvexHull().Area * map.timeLength;
				return Mathf.Max(0, (volume - vlm_)/volume);
			}
		}
		
		private float vlm_ = 0;
		public float shVolume { get { return vlm_; } }
		
		new protected void Awake()
		{
			base.Awake();
			
			gameObject.name = "Field of view";
			
			var mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/ShadowVolumeMat.mat", typeof(Material));
			gameObject.renderer.material = mat;
		}
		
		public override void MapChanged()
		{
			
		}
		
		public void OnDrawGizmos()
		{
			if (calculateEasiness) {
				Gizmos.color = Color.white;
				foreach (Edge3Abs e in setOfPoints.ConvexHull()) {
					Gizmos.DrawLine(e.a, e.b);
				}
			}
			
			if (!debug) {
				return;
			}
			
			foreach (IObstacle so in map.GetObstacles()) {
				
				if (fieldOfView_ > 180)
					Gizmos.DrawLine(position, position + rotationQ * new Vector3(-viewDist_, 0, 0));
				
				
				bool cont = true;
				foreach (Edge3Abs e in so.GetShape()) {
					if (Vector3.Distance(e.ClosestTo(new Vector3(position.x, 0, position.z)), new Vector3(position.x, 0, position.z)) <= viewDist_) {
						cont = false;
						break;
					}
				}
				if (cont || Vector3.Distance(so.position, position) > viewDistance + Mathf.Sqrt(so.sizeX*so.sizeX+so.sizeZ*so.sizeZ)*0.5f) {
					Gizmos.color = new Color32(0, 255, 128, 255);
				} else {
					Gizmos.color = new Color32(255, 128, 0, 255);
				}
				Shape3 sp = so.ShadowPolygon(position, viewDistance);
				Gizmos.DrawSphere(sp[0], 1);
				foreach (Edge3Abs e in sp) {
					Gizmos.DrawLine(e.a, e.b);
				}
			}
		}
		
		public override void CreateMesh()
		{
			var m = new Mesh();
			m.name = "FoV mesh";
			mf.sharedMesh = m;
			UpdateMesh ();
		}
		
		public override void UpdateMesh()
		{
			// TODO: Proper, non-slab, mesh generation
			if (map == null) return;
			
			List<Shape3> shapes = this.shapes;
			int estimTriangles = 2*shapes.Count * (2 * (2 + frontSegments_));
			var vertexList = new List<Vector3> (estimTriangles);
			var triangles = new List<int> (estimTriangles);
			
			float timeStep = map.timeLength / Mathf.FloorToInt((map.timeLength) * map.sub_);
	
			vlm_ = 0;
			if (calculateEasiness) setOfPoints.points.Clear();
			foreach (Shape3 s in shapes) {
				if (calculateEasiness) {
					// Add the vertices in the set of points
					foreach (Vector3 v3 in s.Vertices()) {
						setOfPoints.AddPoint(new Vector3(v3.x, 0, v3.z));
					}
					vlm_ += s.Area / map.subdivisionsPerSecond;
				}
				
				foreach (Edge3Abs e in s) {
					vertexList.Add(e.a);
				}
				foreach (Edge3Abs e in s) {
					vertexList.Add(e.a + new Vector3(0, timeStep, 0));
				}
			}
	
			Mesh m = mf.sharedMesh;
	
			m.Clear ();
	
			m.vertices = vertexList.ToArray ();
			
			int v = 0;
			int sh = 0;
			int maxS = shapes.Count;
			foreach (Shape3 s in shapes) {
				sh++;
				int count = s.Count;
	
				// Bottom
				for (int i = 1; i < count - 1; i++) {
					triangles.Add(v);
					triangles.Add(v + count - i);
					triangles.Add(v + count - i - 1);
				}
	
				// Top
				if (sh <= maxS) {
					for (int i = 1; i < count - 1; i++) {
						triangles.Add(v + count);
						triangles.Add(v + count +i);
						triangles.Add(v + count +i+1);
					}
				}
				
				// Sides
				for (int i = 0; i < count; i++) {
					if (sh <= maxS) {
						if (i < count - 1) {
							triangles.Add (v + i + count);
							triangles.Add (v + i);
							triangles.Add (v + i + count + 1);
							
							triangles.Add (v + i + count + 1);
							triangles.Add (v + i);
							triangles.Add (v + i + 1);
						} else {
							triangles.Add (v + i + count);
							triangles.Add (v + i);
							triangles.Add (v + count);
							
							triangles.Add (v + i - i + count);
							triangles.Add (v + i);
							triangles.Add (v + i - i);
						}
					}
					
				}
				v+= 2* count;
			}
	
			m.triangles = triangles.ToArray ();
	
			m.uv = new Vector2[mf.sharedMesh.vertices.Length];
	
			m.RecalculateNormals ();
	
			mf.sharedMesh = m;
		}
		
		public abstract List<Shape3> Shapes();
		
		private float ToTheta(Vector3 p)
		{
			var diff = new Vector3(p.x - position.x, 0, p.z - position.z);
			diff = Quaternion.Euler(0, -rotation, 0) * diff;
			
			return Mathf.Atan2(diff.z, diff.x);
		}
		
		private float ToDist(Vector3 p)
		{
			return new Vector3(position.x - p.x, 0, position.z - p.z).magnitude;
		}
		
		public Shape3 Occlude(Vector3 position, float rotation)
		{
			var vision_ = new Shape3 ();
			foreach (Edge3Abs e in Vertices(position, rotation)) {
				vision_.AddVertex(e.a);
			}
			
			foreach (IObstacle o in map.GetObstacles()) {
				
				// Very broad pruning
				if (Vector3.Distance(o.position, new Vector3(position.x, 0, position.z)) > viewDist_ + Mathf.Sqrt(o.sizeX*o.sizeX+o.sizeZ*o.sizeZ)*0.5f) {
					continue;
				}
				
				// Not-so-broad pruning
				bool cont = true;
				foreach (Edge3Abs e in o.GetShape()) {
					if (Vector3.Distance(e.ClosestTo(new Vector3(position.x, 0, position.z)), new Vector3(position.x, 0, position.z)) <= viewDist_) {
						cont = false;
						break;
					}
				}
				if (cont) continue;
				
				Shape3[] shadows = {o.ShadowPolygon(position, viewDist_)};
				
				
				foreach (Shape3 shadow in shadows) {
					vision_ = shadow.PointInside(position) ?
						vision_.ClipIn(shadow) : vision_.ClipOut(shadow);
					
					int offset = -1;
					// Realign positions
					for (int i = 0; i < vision_.Count; i++) {
						Vector3 v = vision_[i];
						v.y = position.y;
						vision_[i] = v;
						
						if (v == position) {
							offset = i;
						}
					}
					// Clip may offset the shape, but the center should be at 0
					if (offset > 0) vision_.Offset(offset);
				}
			}
			
			
			return vision_;
		}
		
		public Shape3 Vertices(Vector3 position, float rotation)
		{
			if (frontSegments < 1) {
				frontSegments = 1;
			}
	
			var shape = new Shape3 ();
			shape.AddVertex (position);
	
			float step = fieldOfView_ / frontSegments;
			float halfFov = fieldOfView_ * 0.5f;
			for (int i = 0; i < frontSegments + 1; i++) {
				shape.AddVertex (new Vector3(
					Mathf.Cos((halfFov - i*step - rotation) * Mathf.Deg2Rad) * viewDist_,
					0,
					Mathf.Sin((halfFov - i*step - rotation) * Mathf.Deg2Rad) * viewDist_
					) + position);
			}
	
			return shape;
		}
		
		public override void Validate()
		{
			position.y = 0;
			
			// Position clipping
			if (position.x > 0.5f * map.dimensions.x) {
				position.x = 0.5f * map.dimensions.x;
			}
			
			if (position.z > 0.5f * map.dimensions.z) {
				position.z = 0.5f * map.dimensions.z;
			}
			
			if (position.x < -0.5f * map.dimensions.x) {
				position.x = -0.5f * map.dimensions.x;
			}
			
			if (position.z < -0.5f * map.dimensions.z) {
				position.z = -0.5f * map.dimensions.z;
			}
			
			// Field of view clipping
			if (fieldOfView_ < 1f) {
				fieldOfView_ = 1f;
			}
			if (fieldOfView_ > 359) {
				fieldOfView_ = 359f;
			}
			
			// Front segments clipping
			if (frontSegments_ < minSegments) {
				frontSegments_ = minSegments;
			}
			
			if (frontSegments_ > maxSegments) {
				frontSegments_ = maxSegments;
			}
			
			// View distance clipping
			if (viewDist_ < 0.1f) {
				viewDist_ = 0.1f;
			}
			
			if (dirty) {
				UpdateMesh();
				dirty = false;
			}
		}
	}
}