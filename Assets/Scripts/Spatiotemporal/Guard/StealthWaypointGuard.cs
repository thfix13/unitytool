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
				GameObject go = new GameObject();
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
			List<Pose> lst = new List<Pose>();
			
			Waypoint wp;
			
			if (waypoints == null || waypoints.first == null) {
				lst.Add(new Pose(Vector3.zero, Quaternion.identity));
			} else {
				wp = waypoints.first;
				float currTime = 0;
				float currRotation = 0;
				Vector3 currPos = wp.transform.position;
				
				if (typeof(RotationWaypoint).IsAssignableFrom(wp.GetType())) {
					currRotation = ((RotationWaypoint)wp).theta;
				}
				
				Pose p = new Pose(new Vector3(currPos.x, currTime, currPos.z), Quaternion.Euler(0, currRotation, 0));
				
				if (typeof(WaitingWaypoint).IsAssignableFrom(wp.GetType())) {
					currTime += ((WaitingWaypoint)wp).waitingTime;
				}
				
				while ((wp = wp.next) != null) {
					
					if (typeof(RotationWaypoint).IsAssignableFrom(wp.GetType())) {
						RotationWaypoint curr = (RotationWaypoint)wp;
						// Speed =  dist/time
						// time = dist/speed
						currTime += Mathf.Abs((curr.theta - currRotation)/maxOmega_);
						if (curr.theta - currRotation < 0) {
							p.omega = -maxOmega_;
						} else {
							p.omega = maxOmega_;
						}
						currRotation = curr.theta;
						p.velocity = new Vector3(0, 1, 0);
					} else if (typeof(WaitingWaypoint).IsAssignableFrom(wp.GetType())) {
						WaitingWaypoint curr = (WaitingWaypoint)wp;
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
	
			if (dirty) {
				UpdateMesh ();
				dirty = false;
			}
		}
	}
}