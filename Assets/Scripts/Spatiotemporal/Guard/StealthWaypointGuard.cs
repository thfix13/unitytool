using UnityEngine;
using System.Collections.Generic;

using Objects;
using GeometryLib;

namespace Spatiotemporal {
	[ExecuteInEditMode]
	public class StealthWaypointGuard : StealthGuard, WaypointManagerListener {
		
		public WaypointManager waypoints;
		
		new protected void Awake() {
			base.Awake();
			transform.position = Vector3.zero;
			
			if (waypoints == null)  {
				name = "WaypointPlayer";
				var go = new GameObject();
				go.transform.parent = transform;
				go.AddComponent("WaypointManager");
				waypoints = go.GetComponent<WaypointManager>();
				waypoints.SetListener(this);
			} else {
				waypoints.SetListener(this);
			}
			
			name = "WaypointGuard " + guardID;
		}
		
		public override List<Pose> getPositions()
		{
			var lst = new List<Pose>();
			
			Waypoint wp;
			
			if (waypoints == null || waypoints.first == null) {
				lst.Add(new Pose(Vector3.zero, Quaternion.identity));
			} else {
				wp = waypoints.first;
				float currTime = 0;
				float currRotation = 0;
				Vector3 currPos = wp.transform.position;
				
				var rotationWaypoint = wp as RotationWaypoint;
				if (rotationWaypoint != null) {
					currRotation = rotationWaypoint.theta;
				}
				
				var p = new Pose(new Vector3(currPos.x, currTime, currPos.z), Quaternion.Euler(0, currRotation, 0));
				
				var waitingWaypoint = wp as WaitingWaypoint;
				if (waitingWaypoint != null) {
					currTime += waitingWaypoint.waitingTime;
				}
				
				while ((wp = wp.next) != null) {
					
					rotationWaypoint = wp as RotationWaypoint;
					if (rotationWaypoint != null) {
						RotationWaypoint curr = rotationWaypoint;
						// Speed =  dist/time
						// time = dist/speed
						currTime += Mathf.Abs((curr.theta - currRotation) / maxOmega_);
						p.omega = curr.theta - currRotation < 0 ?
							-maxOmega_ : maxOmega_;
						currRotation = curr.theta;
						p.velocity = new Vector3(0, 1, 0);
					} else {
						waitingWaypoint = wp as WaitingWaypoint;
						if (waitingWaypoint != null) {
							WaitingWaypoint curr = waitingWaypoint;
							currTime += curr.waitingTime;
							p.velocity = new Vector3(0, 1, 0);
						} else {
							Vector3 diff = wp.transform.position - currPos;
							diff.y = 0;
							currTime += diff.magnitude / maxSpeed_;
							diff.Normalize();
							p.velocity = diff * maxSpeed_;
							p.velocity.y = 1;
							p.velocity = Quaternion.Euler(0, -p.rotation, 0) * p.velocity;
							currPos = wp.transform.position;
						}
					}
					lst.Add(p);
					p = new Pose(new Vector3(currPos.x, currTime, currPos.z), Quaternion.Euler(0, currRotation, 0));
				}
				lst.Add(p);
			}
			
			return lst;
		}
		
		public void Notify() {
			dirty = true;
			Validate();
		}
		
		public override void UpdateMesh() {
			base.UpdateMesh();
			foreach (StealthPlayer sp in map.GetPlayers()) {
				sp.dirty = true;
				sp.Validate();
			}
			gameObject.GetComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
		}
		
		new public void Update()
		{
			base.Update();
			waypoints.SetListener(this);
		}
		
		new public void Validate() {
			transform.position = Vector3.zero;
	
			base.Validate();
		}
	}
}