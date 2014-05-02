using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using Objects;
using GeometryLib;

namespace Spatiotemporal {
	[ExecuteInEditMode]
	public abstract class StealthGuard : StealthFov {
		public float maxOmega_ = 2*Mathf.PI; // Deg per sec
		public float maxSpeed_ = 1; // Units per sec
		
		public int guardID = 0;
		
		public float maxOmega {
			get { return maxOmega_; }
			set {
				if (maxOmega_ != value) {
					maxOmega_ = value;
					dirty = true;
					Validate();
				}
			}
		}
		
		public float maxSpeed {
			get { return maxSpeed_; }
			set {
				if (maxSpeed_ != value) {
					maxSpeed_ = value;
					dirty = true;
					Validate();
				}
			}
		}
		
		new protected void Awake() {
			if (gameObject.GetComponent<MeshCollider>() == null) {
				gameObject.AddComponent("MeshCollider");
				gameObject.GetComponent<MeshCollider>().isTrigger = true;
			}
			
			base.Awake();
			
			guardID = map.GetGuards ().Count;
			gameObject.name = "Guard " + guardID;
			
			Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/GuardMat.mat", typeof(Material));
			gameObject.renderer.material = mat;
			
		}
		
		public abstract List<Pose> getPositions();
		
		public override List<Shape3> Shapes()
		{
			List<Shape3> shLst = new List<Shape3> ();
			
			List<Pose> positions = getPositions();
			Pose? current = positions[0];
			
			Pose? next = null;
			if (positions.Count > 1) {
				next = positions[1];
			}
			
			float rot = 0.0f;
			Vector3 pos = Vector3.zero;
			if (current != null) {
				rot = current.Value.rotation;
				pos = current.Value.position;
			}
			
			
			int numSub = Mathf.FloorToInt((map.timeLength-position.y) * map.subdivisionsPerSecond);
			float timeStep = map.timeLength / Mathf.FloorToInt((map.timeLength) * map.subdivisionsPerSecond);
			
			int ind = 1;
			for (int i = 0; i < numSub; i++) {
				Shape3 vision = FieldOfView.Vertices(viewDistance, fieldOfView, frontSegments, position + pos, rotation + rot);
				shLst.Add(StealthFov.Occlude(map, vision, position + pos, viewDistance));
				
				pos += timeStep * (current.Value.rotationQ * current.Value.velocity);
				rot += timeStep * current.Value.omega;
				
				if (next != null) {
					if (next.Value.time <= pos.y) {
						float over = pos.y - next.Value.time;
						pos -= over * (current.Value.rotationQ * current.Value.velocity);
						rot -= over * current.Value.omega;
						
						pos += over * (next.Value.rotationQ * next.Value.velocity);
						rot += over * next.Value.omega;
						
						current = next;
						if (positions.Count > ind + 1)
							next = positions[ind + 1];
						else {
							next = null;
						}
						ind++;
					}
				}
				
			}
			
			return shLst;
		}
		
		public override void UpdateMesh() {
			base.UpdateMesh();
			foreach (StealthPlayer sp in map.GetPlayers()) {
				sp.dirty = true;
				sp.Validate();
			}
			gameObject.GetComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
		}
		
		new public void Validate() {
			position.y = 0;
	
			if (dirty) {
				UpdateMesh ();
				dirty = false;
			}
		}
	}
	
	[ExecuteInEditMode]
	public class StealthGuardPosition : MonoBehaviour {
		public Vector3 velocity_ = new Vector3(0, 1, 0);
		public float time_ = 0.0f;
		public float omega_ = 0.0f;
	
		public Vector3 velocity
		{
			get {return velocity_; }
			set {
				if (velocity_ != value) {
					velocity_ = value;
					guard.dirty = true;
					Validate();
				}
				
			}
		}
		
		public Vector3 rotVelocity
		{
			get { return Quaternion.Euler(0, rotation, 0) * velocity_; }
		}
		
		public float time
		{
			get {return time_; }
			set {
				if (time_ != value) {
					time_ = value;
					guard.dirty = true;
					Validate();
				}
			}
		}
		
		public float omega
		{
			get {return omega_; }
			set {
				if (omega_ != value) {
					omega_ = value;
					guard.dirty = true;
					Validate();
				}
			}
		}
		
		public Vector3 position
		{
			get {
				if (before == null) {
					return guard.position;
				} else {
					float dt = time - before.time;
					return before.position + dt * before.rotVelocity;
				}
			}
		}
		
		public float rotation
		{
			get {
				if (before == null) {
					return guard.rotation;
				} else {
					float dt = time - before.time;
					return before.rotation + dt * before.omega;
				}
			}
		}
	
		public StealthCoordGuard guard
		{
			get {
				if (gameObject.activeInHierarchy) {
					if (transform.parent == null)
						return null;
					return (StealthCoordGuard)transform.parent.gameObject.GetComponent<StealthCoordGuard>();
				}
				return null;
			}
		}
		
		public StealthGuardPosition before = null;
		public StealthGuardPosition after = null;
	
		public Map map
		{
			get {
				if (gameObject.activeInHierarchy ) {
					if (guard == null)
						return null;
					return guard.map;
				}
				return null;
			}
		}
	
		private void Gizmo() {
			Shape3 shape = FieldOfView.Vertices(guard.viewDistance, guard.fieldOfView, guard.frontSegments, position, rotation);
			
			foreach (Edge3Abs e in shape) {
				Gizmos.DrawLine(e.a, e.b);
			}
		}
	
		void OnDrawGizmos()
		{
			Gizmos.color = new Color (0.5f, 0.5f, 0.5f);
			Gizmo ();
			Gizmos.color = new Color(0.3f, 1.0f, 0.8f, 0.5f);
			Gizmos.DrawSphere(position, 1);
		}
	
		void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color (1.0f, 1.0f, 1.0f);
			Gizmo ();
			Gizmos.color = new Color(0f, 1.0f, 0.5f, 1.0f);
			Gizmos.DrawSphere(position, 1);
		}
	
		void OnDestroy()
		{
			if (before != null)
				before.after = after;
			if (after != null)
				after.before = before;
		}
	
		public void Validate ()
		{
			if (before == null) {
				if (time != 0) {
						time = 0;
				}
			} else {
				if (time_ < before.time_ + 0.1f)
					time_ = before.time_ + 0.1f;
			}
			if (after == null) {
				if (time_ > map.timeLength - 0.1f) {
					time_ = map.timeLength - 0.1f;
				}
			} else {
				if (time_ > after.time_ - 0.1f)
					time_ = after.time_ - 0.1f;
			}
			
			if (omega_ > guard.maxOmega) {
				omega_ = guard.maxOmega;
			}
			
			if (omega_ < -guard.maxOmega) {
				omega_ = -guard.maxOmega;
			}
			
			if (velocity_.y != 1.0f) {
				velocity_.y = 1.0f;
			}
			
			Vector2 v = new Vector2(velocity_.x, velocity.z);
			if (v.magnitude > guard.maxSpeed) {
				v *= guard.maxSpeed/v.magnitude;
				velocity_.x = v.x;
				velocity_.z = v.y;
			}
	
			guard.Validate();
		}
	}
	
	[ExecuteInEditMode]
	public class StealthCoordGuard : StealthGuard {
		
		new protected void Awake() {
			if (getPositions().Count == 0) {
				AddCoordinate();
			}
			
			
			base.Awake();
			
			
			
			name = "CoordGuard " + guardID;
		}
		
		public List<StealthGuardPosition> getSGP()
		{
			List<StealthGuardPosition> lst = new List<StealthGuardPosition>();
	
			foreach (Transform child in gameObject.transform) {
				if (child.GetComponent<StealthGuardPosition>()) {
					lst.Add(child.GetComponent<StealthGuardPosition>());
				}
			}
			
			lst.Sort(
				delegate(StealthGuardPosition p1, StealthGuardPosition p2) {
					return Mathf.RoundToInt(p1.time_*100 - p2.time_*100);
				}
			);
	
			return lst;
		}
		
		public GameObject AddCoordinate() {
			List<StealthGuardPosition> positions = getSGP ();
	
			StealthGuardPosition last = null;
			if (positions.Count > 0) {
				last = positions[positions.Count - 1];
			}
	
			if (last != null && last.time == map.timeLength)
				return null;
	
			GameObject go = new GameObject ("Position " + (positions.Count + 1));
			go.transform.parent = transform;
			go.AddComponent ("StealthGuardPosition");
	
			if (last != null) {
				Debug.Log(last.time);
				
				StealthGuardPosition gp = go.GetComponent<StealthGuardPosition> ();
				
				gp.velocity = last.velocity;
				gp.omega = last.omega;
				gp.before = last;
				last.after = gp;
				gp.time_ = last.time_ + 0.1f;
			}
			
			return go;
		}
		
		public override List<Pose> getPositions() {
			List<Pose> poses = new List<Pose>();
			
			foreach (StealthGuardPosition sgp in getSGP()) {
				Pose p = new Pose(sgp.position-position, Quaternion.Euler(0, sgp.rotation-rotation, 0));
				p.velocity = sgp.velocity;
				p.omega = sgp.omega;
				poses.Add(p);
			}
			
			return poses;
		}
		
		public override void UpdateMesh() {
			base.UpdateMesh();
			foreach (StealthPlayer sp in map.GetPlayers()) {
				sp.dirty = true;
				sp.Validate();
			}
			gameObject.GetComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
		}
		
		new public void Validate() {
			position.y = 0;
	
			if (dirty) {
				UpdateMesh ();
				dirty = false;
			}
		}
	}
	
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
