using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace Objects {
	[ExecuteInEditMode]
	public class Waypoint : MonoBehaviour {
		
		public Waypoint next;
		
		[HideInInspector]
		public WaypointManager manager;
		
		public static bool debug = false;
		
		protected void Gizmo() {
			Gizmos.color = Color.white;
			if (next != null) {
				Vector3 diff = next.transform.position - transform.position;
				Gizmos.matrix = Matrix4x4.TRS(0.5f*(transform.position + next.transform.position), Quaternion.Euler(0, -Mathf.Atan2(diff.z, diff.x)*Mathf.Rad2Deg - 90, 0), Vector3.one);
				
				Gizmos.DrawFrustum(new Vector3(0, 0, -diff.magnitude*0.1f), Mathf.Atan2(2, diff.magnitude*0.1f)*Mathf.Rad2Deg, diff.magnitude*0.05f, 0, 1);
				Gizmos.matrix = Matrix4x4.identity;
				Gizmos.DrawLine(transform.position, next.transform.position);
			}
		}
		
		void OnDrawGizmos () {
			Gizmos.color = new Color(1, 1, 1, 0.5f);
			Gizmos.DrawSphere(transform.position, 0.2f);
			Gizmo();
		}
		
		protected void Update() {
			if (next != null) {
				if (typeof(WaitingWaypoint).IsAssignableFrom(next.GetType()) || typeof(RotationWaypoint).IsAssignableFrom(next.GetType())) {
					next.transform.position = transform.position;
				} else {
					if (manager != null) {
						manager.ChangeOccured();
					}
				}
			} else {
				if (manager != null) {
					manager.ChangeOccured();
				}
			}
			
			transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
		}
	}
}
