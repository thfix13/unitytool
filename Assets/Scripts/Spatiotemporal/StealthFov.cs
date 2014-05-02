using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public abstract class StealthFov : MeshMapChild {
	
	public float viewDist_ = 30f;
	public float fieldOfView_ = 50f;
	public int frontSegments_ = 8;
	
	public SetOfPoints setOfPoints = new SetOfPoints();
	
	public float viewDistance {
		get { return viewDist_; }
		set {
			if (value != viewDist_) {
				dirty = true;
				viewDist_ = value;
				Validate();
			}
		}
	}
	
	public float fieldOfView {
		get { return fieldOfView_; }
		set {
			if (value != fieldOfView_) {
				dirty = true;
				fieldOfView_ = value;
				Validate();
			}
		}
	}
	
	public int frontSegments {
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
	private List<Shape3> shapes {
		get {
			if (shapes_ == null || dirty) {
				shapes_ = Shapes();
			}
			return shapes_;
		}
	}
	
	public Shape3 convexHull {
		get {
			return setOfPoints.ConvexHull();
		}
	}
	
	public float easiness {
		get {
			float volume = convexHull.Area * map.timeLength;
			return (volume - vlm_)/volume;
		}
	}
	
	private float vlm_ = 0;
	public float shVolume {
		get {
			return vlm_;
		}
	}
	
	new protected void Awake() {
		base.Awake();
		
		gameObject.name = "Field of view";
		
		Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/ShadowVolumeMat.mat", typeof(Material));
		gameObject.renderer.material = mat;
	}
	
	public override void MapChanged()
	{
		
	}
	
	public void OnDrawGizmos()
	{
		
		foreach (Edge3Abs e in setOfPoints.ConvexHull()) {
			Gizmos.DrawLine(e.a, e.b);
		}
	}
	
	public override void CreateMesh()
	{
		Mesh m = new Mesh();
		m.name = "FoV mesh";
		mf.sharedMesh = m;
		UpdateMesh ();
	}
	
	public override void UpdateMesh()
	{
		if (map == null)
			return;
		List<Vector3> vertexList = new List<Vector3> ();

		//int numSub = Mathf.FloorToInt((map.timeLength-position.y) * subdivisionsPerSecond);
		float timeStep = map.timeLength / Mathf.FloorToInt((map.timeLength) * map.sub_);

		vlm_ = 0;
		setOfPoints.points.Clear();
		foreach (Shape3 s in shapes) {
			foreach (Vector3 v3 in s.Vertices()) {
				setOfPoints.AddPoint(new Vector3(v3.x, 0, v3.z));
			}
			vlm_ += s.Area / map.subdivisionsPerSecond;
			
			
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

		List<int> triangles = new List<int> ();

		int v = 0;
		int sh = 0;
		int maxS = shapes.Count;
		foreach (Shape3 s in shapes) {
			sh++;
			int count = s.Count;

			// Bottom
			for (int i = 1; i < count; i++) {
				triangles.Add(v);
				triangles.Add(v + count - i);
				triangles.Add(v + count - i - 1);
			}

			// Top
			if (sh <= maxS-1) {
				for (int i = 1; i < count; i++) {
					triangles.Add(v + count);
					triangles.Add(v + count +i);
					triangles.Add(v + count +i+1);
				}
			}

			for (int i = 0; i < count; i++) {
				if (sh <= maxS-1) {
					triangles.Add (v + count);
					triangles.Add (v);
					triangles.Add (v + count + 1);
					if (i < count - 1) {
						triangles.Add (v + count + 1);
						triangles.Add (v);
						triangles.Add (v + 1);
					} else {
						triangles.Add (v - i + count);
						triangles.Add (v);
						triangles.Add (v - i);
					}
					
				}
				v+= 1;
			}
			v+= count;
		}

		m.triangles = triangles.ToArray ();

		m.uv = new Vector2[mf.sharedMesh.vertices.Length];

		m.RecalculateNormals ();

		mf.sharedMesh = m;
	}
	
	public abstract List<Shape3> Shapes();
	/*{
		List<Shape3> shLst = new List<Shape3> ();
		
		float tm = 0.0f;
		int numSub = Mathf.FloorToInt(map.timeLength * map.sub_);
		float timeStep = map.timeLength / numSub;
		
		for (int i = 0; i < numSub; i++) {
			Shape3 vision = Vertices(viewDist_, fieldOfView_, frontSegments_, position + new Vector3(0, tm, 0), rotationQ.eulerAngles.y);
			shLst.Add (Occlude(map, vision, position + new Vector3(0, tm, 0), viewDist_));
			
			tm += timeStep;
		}
		
		return shLst;
	} */
	
	public static Shape3 Occlude(Map map, Shape3 vision, Vector3 position, float viewDist) {
		
		Shape3 vision_ = new Shape3 ();
		foreach (Edge3Abs e in vision) {
			vision_.addVertex(e.a);
		}
		
		Vector3 center = vision_[0];

		// Left handed iterator of the vision shape
		IEnumerator visionIteratorLH = vision_.GetEnumerator ();

		foreach (StealthObstacle o in map.GetObstacles()) {
			if (Mathf.Sqrt((o.position.x - center.x)*(o.position.x - center.x) + (o.position.z - center.z)*(o.position.z - center.z)) > viewDist +  o.dimensions.magnitude*0.5f) {
				continue;
			}
			
			// First part of the occluded shape
			Shape3 occludedLeft = new Shape3 ();
			// Middle part (clipped by shadow) of the occluded shape (if any)
			Shape3 occludedMiddle = new Shape3 ();
			// Last part of the occluded shape (if clipping occured)
			Shape3 occludedRight = new Shape3 ();


			Shape3 shadow = o.ShadowPolygon(position, viewDist);
			// Right handed iterator of the shadow polygon
			IEnumerator shadowIterator;

			Edge3Abs intersecting = new Edge3Abs(Vector3.zero, Vector3.zero);
			while(visionIteratorLH.MoveNext()) {
				Edge3Abs e = (Edge3Abs)visionIteratorLH.Current;

				occludedLeft.addVertex(e.a);

				shadowIterator = shadow.Reverse();
				float distance = float.PositiveInfinity;
				while(shadowIterator.MoveNext()) {

					Edge3Abs se = (Edge3Abs)shadowIterator.Current;
					Vector3 intersection;
					if (!float.IsNaN((intersection = e.IntersectXZ(se)).x)) {
						if (Vector3.Distance(e.a, intersection) < distance) {
							distance = Vector3.Distance(e.a, intersection);
							e.b = intersection;
							occludedMiddle.addVertex(e.b);
							intersecting = se;
							intersecting.b = e.b;
						}
					}
				}
				if (intersecting.a != intersecting.b) break;
			}

			if (intersecting.a != intersecting.b) {
				// Traverse the shadow shape right-handedly up to the intersection segment
				shadowIterator = shadow.Reverse();
				int offset = -1;
				while (shadowIterator.MoveNext()) {
					if (((Edge3Abs) shadowIterator.Current).b == intersecting.a) {
						break;
					}

					offset -= 1;
				}

				shadow.Offset(offset);

				shadowIterator = shadow.Reverse();
				while (shadowIterator.MoveNext()) {
					Edge3Abs se = (Edge3Abs) shadowIterator.Current;

					visionIteratorLH = vision_.GetEnumerator();
					bool intersect = false;
					while (visionIteratorLH.MoveNext()) {
						Edge3Abs e = (Edge3Abs) visionIteratorLH.Current;
						Vector3 intersection = e.IntersectXZ(se);
						if (!float.IsNaN (intersection.x) && intersecting.b != intersection) {
							se.b = intersection;
							occludedRight.addVertex(se.b);
							intersect= true;
							break;
						}
					}

					if (intersect)
						break;
					else
						occludedMiddle.addVertex (se.b);
				}

				while (visionIteratorLH.MoveNext()) {
					Edge3Abs e = (Edge3Abs)visionIteratorLH.Current;
					
					occludedRight.addVertex (e.a);
				}
			}



			vision_.Clear();
			foreach (Edge3Abs e in occludedLeft) {
				vision_.addVertex(e.a);
			}
			foreach (Edge3Abs e in occludedMiddle) {
				vision_.addVertex(e.a);
			}
			foreach (Edge3Abs e in occludedRight) {
				vision_.addVertex(e.a);
			}
			visionIteratorLH = vision_.GetEnumerator ();
		}

		return vision_;
	}
	
	public static Shape3 Vertices(float viewDist, float fov, int frontSegments, Vector3 position, float rotation) {
		if (frontSegments < 1) {
			frontSegments = 1;
		}
		if (fov < 0) {
			fov = 0;
		}
		if (fov > 360) {
			fov = 360;
		}
		if (viewDist < 0) {
			viewDist = 0;
		}

		Shape3 shape = new Shape3 ();
		shape.addVertex (position);

		float step = fov / frontSegments;
		float halfFov = fov * 0.5f;
		for (int i = 0; i < frontSegments + 1; i++) {
			shape.addVertex (new Vector3(
				Mathf.Cos((halfFov - i*step - rotation) * Mathf.Deg2Rad) * viewDist,
				0,
				Mathf.Sin((halfFov - i*step - rotation) * Mathf.Deg2Rad) * viewDist
				) + position);
		}

		return shape;
	}
	
	public override void Validate()
	{
		position.y = 0;
		
		if (fieldOfView_ < 1f) {
			fieldOfView_ = 1f;
		}
		if (fieldOfView_ > 359f) {
			fieldOfView_ = 359f;
		}
		
		if (frontSegments_ < 1) {
			frontSegments_ = 1;
		}
		
		if (dirty) {
			UpdateMesh();
			dirty = false;
		}
	}
}