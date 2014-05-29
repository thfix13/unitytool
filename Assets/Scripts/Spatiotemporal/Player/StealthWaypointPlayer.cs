using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Objects;
using GeometryLib;

namespace Spatiotemporal {
	[ExecuteInEditMode]
	public class StealthWaypointPlayer : StealthPlayer, WaypointManagerListener {
		
		public WaypointManager waypoints;
		
		new public float posX
		{
			get {
				if (waypoints == null || waypoints.first == null) 
					return position.x;
				Waypoint first = waypoints.first;
				return first.transform.position.x;
			}
		}
		
		new public float posZ
		{
			get {
				if (waypoints == null || waypoints.first == null) 
					return position.z;
				Waypoint first = waypoints.first;
				return first.transform.position.z;
			}
		}
		
		new protected void Awake()
		{	
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
			
			name = "WaypointPlayer";
		}
		
		public override bool Collide()
		{
			List<Pose> poses = getPositions();
			
			Vector3 curr = poses[0].position;
			int ind = 0;
			foreach (Pose spp in poses) {
				float timeLength;
				if (ind+1 < poses.Count) {
					timeLength = poses[ind+1].time - spp.time;
				} else {
					timeLength = map.timeLength - spp.time;
				}
				Vector3 step = spp.velocity * timeLength;
				for (int d = 0; d < 8; d++) {
					RaycastHit hit;
					bool coll = Physics.Raycast(new Vector3(curr.x + Mathf.Cos(-d*0.25f*Mathf.PI)*radius, curr.y, curr.z + Mathf.Sin(-d*0.25f*Mathf.PI)*radius), step.normalized, out hit, step.magnitude);
					if (coll) {
						return true;
					}
				}
				
				curr += step;
				ind++;
			}
			
			return false;
		}
		
		public override void MapChanged()
		{
			
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
		
		public void Notify()
		{
			dirty = true;
			Validate();
		}
		
		public override void Validate()
		{
			position = Vector3.zero;
			waypoints.SetListener(this);
			
			time = 0;
			
			if (radius_ < 0.1f) {
				radius_ = 0.1f;
			}
			
			if (maxSpeed_ < 0) {
				maxSpeed_ = 0;
			}
			
			if (dirty) {
				dirty = false;
				CreateMesh();
				
				if (accSurf != null) {
					accSurf.dirty = true;
					accSurf.Validate();
				}
				
				if (Collide()) {
					var mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/PlayerCollMat.mat", typeof(Material));
					gameObject.renderer.material = mat;
				} else {
					var mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/PlayerMat.mat", typeof(Material));
					gameObject.renderer.material = mat;
				}
			}
		}
	}
}
	