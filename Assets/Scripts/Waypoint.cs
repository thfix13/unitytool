using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class Waypoint : MonoBehaviour {
	
	public Waypoint next;
	public Enemy center;
	public int type = 0;
	public static bool debug = true;

//	private float c;
//	private Vector3 q0, q1;
//	private Transform start, end, handle1, handle2;
//	private Vector3 radiusDir1, radiusDir2, n11, n12, n21, n22;

	// Use this for initialization
	void Start () {
//		c = 0.0f;
//
//		start.position = this.transform.position;
//		end.position = next.transform.position;
//
//		radiusDir1 = this.transform.position - center.transform.position;
//		radiusDir2 = next.transform.position - center.transform.position;
//
//		n11 = Vector3.Cross (radiusDir1, new Vector3(0.0f, 1.0f, 0.0f));
//		n12 = Vector3.Cross (new Vector3(0.0f, 1.0f, 0.0f), radiusDir1);
//		n21 = Vector3.Cross (radiusDir2, new Vector3(0.0f, 1.0f, 0.0f));
//		n22 = Vector3.Cross (new Vector3(0.0f, 1.0f, 0.0f), radiusDir2);

//		Vector3 a = this.transform.position;
//		Vector3 c = next.transform.position;
//		Vector3 ac = c - a;
//		float kac = ac.z / ac.x;
//		float kcd1 = n11.z / n11.x;
//		float kcd2 = n12.z / n12.x;
//		float kcd3 = n21.z / n21.x;
//		float kcd4 = n22.z / n22.x;
//
//		if () {
//
//		} else if () {
//
//		} else if () {
//
//		} else if () {
//
//		}
//		q0 = CalculateBezierPoint(c, start.position, handle1.position, handle2.position, end.position);
	}
	
	// Update is called once per frame
	void Update () {
//		if (type == 2) {
//			if (c <= 100) {
//				c += 0.01;
//				q1 = CalculateBezierPoint (c, start.position, handle1.position, handle2.position, end.position);
//				Gizmos.DrawLine (q0, q1, Color.Lerp( Color.red, Color.magenta, 0.2f), 1000);
//				q0 = q1;
//			}
//		}
	}
	
	void OnDrawGizmos () {
		if (type != 0) {
			if (type == 1) {
				Gizmos.color = Color.Lerp( Color.green, Color.red, 0.2f);
			}
			if (type == 2) {

				Gizmos.color = Color.Lerp( Color.red, Color.magenta, 0.2f);
			}
			Vector3 pos = new Vector3(this.transform.position.x, 0, this.transform.position.z);
			Vector3 direction = new Vector3(next.transform.position.x, 0, next.transform.position.z) - new Vector3(this.transform.position.x, 0, this.transform.position.z);
			if (this.next != null) {
				Gizmos.DrawRay(pos, direction);	
				Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 25.0f, 0) * new Vector3(0, 0, 1);
				Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 25.0f, 0) * new Vector3(0, 0, 1);
				Gizmos.DrawRay(pos + direction, right * 0.75f);
				Gizmos.DrawRay(pos + direction, left * 0.75f);
			}
		}
		if (debug)
			Gizmos.DrawSphere(transform.position, 0.1f);
	}

//	private Vector3 CalculateBezierPoint (float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
//		float u;
//		float uu;
//		float uuu;
//		float tt;
//		float ttt;
//		Vector3 p;
//		u = 1 - t;
//		uu = u * u;
//		uuu = uu * u;
//		tt = t * t;
//		ttt = tt * t;
//		p = uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
//		return p;
//	}
}

/*public class RotationWaypoint : Waypoint {
	
	public Vector3 lookDir;

}

public class WaitingWaypoint : Waypoint {
	
	public float waitingTime;
	
	[HideInInspector]
	public Dictionary<int, float> times = new Dictionary<int, float>();
	
}*/