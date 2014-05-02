using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace Objects {
	public class RotationWaypoint : Waypoint {
		
		public float theta {
			get {
				return Mathf.Atan2(lookDir.z, lookDir.x);
			}
			set {
				lookDir.x = Mathf.Cos(value);
				lookDir.z = Mathf.Sin(value);
				lookDir.y = 0;
			}
		}
		
		public Vector3 lookDir;
		
		void OnDrawGizmos () {
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(transform.position, 0.19f);
			Gizmo();
		}
	}
}
