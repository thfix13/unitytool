using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class Waypoint : MonoBehaviour
{
	public Waypoint prev;
	public Waypoint next;
	public Enemy center;
	public int type = 0;
	public static bool debug = true;
	private float c, m1, m2, m3, m4, n1, n2, n3, n4, dx, dy, det1, det2, det3, det4;
	private Vector3 q0, q1;
	private Vector3 handle1, handle2;
	private Vector3 radiusDir1, radiusDir2, n11, n12, n21, n22, s, e, dir1, dir2;

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
	
	void OnDrawGizmos ()
	{
		if (type != 0) {
			// Moving Enemies
			if (type == 1) {
				Gizmos.color = Color.Lerp (Color.green, Color.red, 0.2f);
				Vector3 pos = new Vector3 (this.transform.position.x, 0, this.transform.position.z);
				Vector3 direction = new Vector3 (next.transform.position.x, 0, next.transform.position.z) - new Vector3 (this.transform.position.x, 0, this.transform.position.z);
				if (this.next != null) {
					Gizmos.DrawRay (pos, direction);	
					Vector3 right = Quaternion.LookRotation (direction) * Quaternion.Euler (0, 180 + 25.0f, 0) * new Vector3 (0, 0, 1);
					Vector3 left = Quaternion.LookRotation (direction) * Quaternion.Euler (0, 180 - 25.0f, 0) * new Vector3 (0, 0, 1);
					Gizmos.DrawRay (pos + direction, right * 0.75f);
					Gizmos.DrawRay (pos + direction, left * 0.75f);
				}
			}
			// Rotational Cameras
			if (type == 2) {
				Gizmos.color = Color.Lerp (Color.red, Color.magenta, 0.2f);
				
				c = 0.0f;
				s = transform.position;
				e = next.transform.position;
		
				radiusDir1 = transform.position - center.transform.position;
				radiusDir2 = next.transform.position - center.transform.position;
		
				n11 = Vector3.Cross (radiusDir1, new Vector3 (0.0f, 1.0f, 0.0f));
				n12 = Vector3.Cross (new Vector3 (0.0f, 1.0f, 0.0f), radiusDir1);
				n21 = Vector3.Cross (radiusDir2, new Vector3 (0.0f, 1.0f, 0.0f));
				n22 = Vector3.Cross (new Vector3 (0.0f, 1.0f, 0.0f), radiusDir2);
				
				dx = e.x - s.x;
				dy = e.z - s.z;
				
				det1 = n21.x * n11.z - n21.z * n11.x;
				det2 = n22.x * n11.z - n22.z * n11.x;
				det3 = n21.x * n12.z - n21.z * n12.x;
				det4 = n22.x * n12.z - n22.z * n12.x;
				
				m1 = (dy * n21.x - dx * n21.z) / det1;
				n1 = (dy * n11.x - dx * n11.z) / det1;
				m2 = (dy * n22.x - dx * n22.z) / det2;
				n2 = (dy * n11.x - dx * n11.z) / det2;
				m3 = (dy * n21.x - dx * n21.z) / det3;
				n3 = (dy * n12.x - dx * n12.z) / det3;
				m4 = (dy * n22.x - dx * n22.z) / det4;
				n4 = (dy * n12.x - dx * n12.z) / det4;
				
				if (m1 > 0 && n1 > 0) {
					handle1 = s + n11 * 0.4f;
					handle2 = e + n21 * 0.4f;
					dir1 = n11;
					dir2 = n21;
				} else if (m2 > 0 && n2 > 0) {
					handle1 = s + n11 * 0.4f;
					handle2 = e + n22 * 0.4f;
					dir1 = n11;
					dir2 = n22;
				} else if (m3 > 0 && n3 > 0) {
					handle1 = s + n12 * 0.4f;
					handle2 = e + n21 * 0.4f;
					dir1 = n12;
					dir2 = n21;
				} else if (m4 > 0 && n4 > 0) {
					handle1 = s + n12 * 0.4f;
					handle2 = e + n22 * 0.4f;
					dir1 = n12;
					dir2 = n22;
				}			
				
				q0 = CalculateBezierPoint (c, s, handle1, handle2, e);
				
				while (c <= 1) {
					if (c == 0.0f) {
						Vector3 right = Quaternion.LookRotation (dir1) * Quaternion.Euler (0, 180 + 25.0f, 0) * new Vector3 (0, 0, 1);
						Vector3 left = Quaternion.LookRotation (dir1) * Quaternion.Euler (0, 180 - 25.0f, 0) * new Vector3 (0, 0, 1);					
						Gizmos.DrawRay (s, -right * 0.5f);
						Gizmos.DrawRay (s, -left * 0.5f);
					}
					if (c == 1.0f) {
						Vector3 right = Quaternion.LookRotation (dir2) * Quaternion.Euler (0, 180 + 25.0f, 0) * new Vector3 (0, 0, 1);
						Vector3 left = Quaternion.LookRotation (dir2) * Quaternion.Euler (0, 180 - 25.0f, 0) * new Vector3 (0, 0, 1);					
						Gizmos.DrawRay (e, -right * 0.5f);
						Gizmos.DrawRay (e, -left * 0.5f);						
					}
					c += 0.01f;
					q1 = CalculateBezierPoint (c, s, handle1, handle2, e);
					Gizmos.DrawLine (q0, q1);
					q0 = q1;
				}
			}
		}
		if (debug) {
			Gizmos.color = Color.red;
			Gizmos.DrawSphere (transform.position, 0.1f);
			if (next) {
				Gizmos.DrawLine ( transform.position, next.transform.position);
			}
		}
	}

	private Vector3 CalculateBezierPoint (float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float u;
		float uu;
		float uuu;
		float tt;
		float ttt;
		Vector3 p;
		u = 1 - t;
		uu = u * u;
		uuu = uu * u;
		tt = t * t;
		ttt = tt * t;
		p = uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
		return p;
	}
}

/*public class RotationWaypoint : Waypoint {
	
	public Vector3 lookDir;

}

public class WaitingWaypoint : Waypoint {
	
	public float waitingTime;
	
	[HideInInspector]
	public Dictionary<int, float> times = new Dictionary<int, float>();
	
}*/