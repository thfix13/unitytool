using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class FieldOfView : MonoBehaviour {

	public float viewDist, fov;
	public int frontSegments;
	public Vector3 position;
	public float rotation;

	public Map map
	{
		get {
			if (gameObject.activeInHierarchy ) {
				if (transform.parent == null)
					return null;
				return (Map)transform.parent.gameObject.GetComponent<Map>();
			}
			return null;
		}
	}
	
	void Start () {
	
	}
	
	void Update () {
		transform.position = Vector3.zero;
		transform.rotation = new Quaternion (0, 0, 0, 1);
		transform.localScale = Vector3.one;
	}

	void OnDrawGizmos()
	{

		Shape3 fovShape = Vertices (viewDist, fov, frontSegments, position, rotation);
		Gizmos.color = new Color (1.0f, 1.0f, 1.0f, 0.5f);
		foreach (Edge3Abs e in fovShape) {
			Gizmos.DrawLine(e.a, e.b);
		}

		fovShape = Occlude (map, fovShape, position, viewDist);

		bool toggle = true;
		float step = 360.0f / fovShape.Count;
		float degree = -step;
		foreach (Edge3Abs e in fovShape) {
			toggle = !toggle;
			Gizmos.color = HSV.Color(degree += 0, toggle ? 1.0f : 0.5f, toggle ? 1.0f : 0.5f);
			Gizmos.DrawLine(e.a, e.b);
		}

		Gizmos.color = new Color (1.0f, 0.5f, 0.0f, 0.5f);
		foreach (StealthObstacle o in map.GetObstacles()) {
			foreach (Edge3Abs e in o.ShadowPolygon(position, viewDist)) {
				Gizmos.DrawLine(e.a, e.b);
			}
		}
	}

	public static Shape3 Occlude(Map map, Shape3 vision, Vector3 position, float viewDist) {

		Shape3 vision_ = new Shape3 ();
		foreach (Edge3Abs e in vision) {
			vision_.addVertex(e.a);
		}

		// Left handed iterator of the vision shape
		IEnumerator visionIteratorLH = vision_.GetEnumerator ();

		foreach (StealthObstacle o in map.GetObstacles()) {
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
}
