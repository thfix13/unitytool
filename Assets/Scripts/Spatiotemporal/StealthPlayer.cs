using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Objects;
using GeometryLib;

namespace Spatiotemporal {
	[ExecuteInEditMode]
	public abstract class StealthPlayer : MeshMapChild {
		public float maxSpeed_ = 1;
		public float maxOmega_ = 1;
		public float radius_ = 2.0f;
		
		public AccessibilitySurface accSurf = null;
		
		public float maxSpeed {
			get { return maxSpeed_; }
			set {
				if (maxSpeed_ != value) {
					dirty = true;
					maxSpeed_ = value;
					Validate();
				}
			}
		}
		
		public float maxOmega {
			get { return maxOmega_; }
			set {
				if (maxOmega_ != value) {
					dirty = true;
					maxOmega_ = value;
					Validate();
				}
			}
		}
		
		public float radius {
			get { return radius_; }
			set {
				if (radius_ != value) {
					dirty = true;
					radius_ = value;
					Validate();
				}
			}
		}
		
		new protected void Awake()
		{
			
			if (gameObject.GetComponent<MeshCollider>() == null) {
				gameObject.AddComponent("MeshCollider");
			}
			
			if (gameObject.GetComponent<Rigidbody>() == null) {
				Rigidbody rb = (Rigidbody)gameObject.AddComponent("Rigidbody");
				rb.useGravity = false;
			}
			
			base.Awake();
			
			gameObject.layer = 2;
			
			Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/PlayerMat.mat", typeof(Material));
			gameObject.renderer.material = mat;
		}
		
		void OnDestroy() {
			if (accSurf != null) {
				DestroyImmediate(accSurf);
			}
		}
		
		public abstract bool Collide();
		
		public override void MapChanged()
		{
			
		}
		
		public abstract List<Pose> getPositions();
		
		public override void CreateMesh() {
			List<Pose> pos = getPositions();
			
			mf.sharedMesh = null;
			Mesh m = new Mesh();
			m.name = "Player trail";
			Vector3[] vertices;
			bool capIt = false;
			// Last position is at the ceiling
			if (pos[pos.Count-1].position.y >= map.timeLength) {
				vertices = new Vector3[8*pos.Count+2];
			// Last position must be capped by ceiling
			} else {
				vertices = new Vector3[8*(pos.Count+1)+2];
				capIt = true;
			}
			
			int cap1 = vertices.Length-2;
			int cap2 = vertices.Length-1;
			
			Vector3 curr = pos[0].position;
			int ind = 0;
			Pose prev = pos[0];
			foreach (Pose spp in pos) {
				curr += prev.velocity*(spp.time - prev.time);
				for (int d = 0; d < 8; d++) {
					vertices[ind++] = new Vector3(curr.x + Mathf.Cos(-d*0.25f*Mathf.PI)*radius, curr.y, curr.z + Mathf.Sin(-d*0.25f*Mathf.PI)*radius);
				}
				prev = spp;
			}
			if (capIt) {
				curr += prev.velocity*(map.timeLength - prev.time);
				for (int d = 0; d < 8; d++) {
					vertices[ind++] = new Vector3(curr.x + Mathf.Cos(-d*0.25f*Mathf.PI)*radius, curr.y, curr.z + Mathf.Sin(-d*0.25f*Mathf.PI)*radius);
				}
			}
			vertices[cap1] = new Vector3(pos[0].posX, 0, pos[0].posZ);
			if (capIt) {
				vertices[cap2] = new Vector3(curr.x, map.timeLength, curr.z);
			} else {
				vertices[cap2] = new Vector3(curr.x, curr.y, curr.z);
			}
			
			
			m.vertices = vertices;
			
			int[] triangles = new int[((vertices.Length-2)/8-1)*16*3+16*3];
			ind = 0;
			for (int i=0; i<vertices.Length-2-8; i+=8) {
				for (int j=0; j<8; j++) {
					
					if (j < 7) {
						triangles[ind++] = 0 + i + j;
						triangles[ind++] = 1 + i + j;
						triangles[ind++] = 9 + i + j;
						triangles[ind++] = 0 + i + j;
						triangles[ind++] = 9 + i + j;
						triangles[ind++] = 8 + i + j;
					} else {
						triangles[ind++] = 7 + i;
						triangles[ind++] = 0 + i;
						triangles[ind++] = 8 + i;
						triangles[ind++] = 7 + i;
						triangles[ind++] = 8 + i;
						triangles[ind++] = 15 + i;
					}
				}
			}
			
			// Caps
			for (int i=0; i < 8; i++) {
				triangles[ind++] = cap1;
				triangles[ind++] = (1 + i) % 8;
				triangles[ind++] = 0 + i;
			}
			int end = vertices.Length-2-8;
			for (int i=0; i < 8; i++) {
				triangles[ind++] = cap2;
				triangles[ind++] = 0 + i + end;
				triangles[ind++] = (1 + i) % 8 + end;
			}
			
			m.triangles = triangles;
			m.uv = new Vector2[vertices.Length];
			m.RecalculateNormals();
			
			mf.sharedMesh = m;
			gameObject.GetComponent<MeshCollider>().sharedMesh = m;
		}
		
		public override void UpdateMesh() {
			
		}
	}
	
	[ExecuteInEditMode]
	public class StealthPlayerPosition : MonoBehaviour {
		public Vector3 velocity_ = new Vector3(0, 1, 0);
		public float time_ = 0.0f;
	
		public Vector3 velocity
		{
			get {return velocity_; }
			set {
				if (velocity_ != value) {
					velocity_ = value;
					player.dirty = true;
					Validate();
				}
			}
		}
		
		public float time
		{
			get {return time_; }
			set {
				if (time_ != value) {
					time_ = value;
					player.dirty = true;
					Validate();
				}
			}
		}
		
		public Vector3 position
		{
			get {
				if (before == null) {
					return player.position;
				} else {
					float dt = time - before.time;
					return before.position + before.velocity * dt;
				}
			}
		}
	
		public StealthPlayer player
		{
			get {
				if (gameObject.activeInHierarchy) {
					if (transform.parent == null)
						return null;
					return (StealthPlayer)transform.parent.gameObject.GetComponent<StealthPlayer>();
				}
				return null;
			}
		}
		
		public StealthPlayerPosition before = null;
		public StealthPlayerPosition after = null;
	
		public Map map
		{
			get {
				if (gameObject.activeInHierarchy ) {
					if (player == null)
						return null;
					return player.map;
				}
				return null;
			}
		}
	
		private void Gizmo() {
			Gizmos.DrawSphere(position, player.radius);
		}
	
		void OnDrawGizmos()
		{
			Gizmos.color = new Color (1f, .5f, 0.25f, 0.5f);
			Gizmo ();
		}
	
		void OnDrawGizmosSelected()
		{
			Gizmos.color = new Color (1.0f, .75f, .25f, 0.75f);
			Gizmo ();
		}
	
		void OnDestroy()
		{
			if (before != null)
				before.after = after;
			if (after != null)
				after.before = before;
		}
		
		public void SubValidate()
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
			
			if (velocity_.y != 1.0f) {
				velocity_.y = 1.0f;
			}
			
			Vector2 v = new Vector2(velocity_.x, velocity.z);
			if (v.magnitude > player.maxSpeed) {
				v *= player.maxSpeed/v.magnitude;
				velocity_.x = v.x;
				velocity_.z = v.y;
			}
		}
		
		public void Validate ()
		{
			SubValidate();
	
			player.Validate();
		}
	}

	
	[ExecuteInEditMode]
	public class StealthCoordPlayer : StealthPlayer {
		
		new protected void Awake()
		{
			if (getPositions().Count == 0) {
				AddCoordinate();
			}
			
			base.Awake();
			
			name = "CoordPlayer";
		}
		
		public override bool Collide()
		{
			List<StealthPlayerPosition> pos = getSPP();
			Vector3 curr = pos[0].position;
			foreach (StealthPlayerPosition spp in pos) {
				float timeLength;
				if (spp.after != null) {
					timeLength = spp.after.time - spp.time;
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
			}
			
			return false;
		}
		
		public override void MapChanged()
		{
			
		}
		
		public override List<Pose> getPositions()
		{
			List<Pose> lst = new List<Pose>();
	
			foreach (Transform child in gameObject.transform) {
				if (child.GetComponent<StealthPlayerPosition>()) {
					StealthPlayerPosition spp = child.GetComponent<StealthPlayerPosition>();
					Pose p = new Pose(spp.position, Quaternion.identity);
					p.velocity = spp.velocity_;
					p.omega = 0;
					lst.Add(p);
				}
			}
			
			lst.Sort(
				delegate(Pose p1, Pose p2) {
					return Mathf.RoundToInt(p1.time*100 - p2.time*100);
				}
			);
	
			return lst;
		}
		
		public List<StealthPlayerPosition> getSPP()
		{
			List<StealthPlayerPosition> lst = new List<StealthPlayerPosition>();
	
			foreach (Transform child in gameObject.transform) {
				if (child.GetComponent<StealthPlayerPosition>()) {
					StealthPlayerPosition spp = child.GetComponent<StealthPlayerPosition>();
					lst.Add(spp);
				}
			}
			
			lst.Sort(
				delegate(StealthPlayerPosition p1, StealthPlayerPosition p2) {
					return Mathf.RoundToInt(p1.time*100 - p2.time*100);
				}
			);
	
			return lst;
		}
		
		public GameObject AddCoordinate() {
			List<StealthPlayerPosition> positions = getSPP ();
	
			StealthPlayerPosition last = null;
			if (positions.Count > 0) {
				last = positions[positions.Count - 1];
			}
	
			if (last != null && last.time == map.timeLength)
				return null;
	
			GameObject go = new GameObject ("Position " + (positions.Count + 1));
			go.transform.parent = transform;
			go.AddComponent ("StealthPlayerPosition");
	
			if (last != null) {
				Debug.Log(last.time);
				
				StealthPlayerPosition pp = go.GetComponent<StealthPlayerPosition> ();
				
				pp.velocity = last.velocity;
				pp.before = last;
				last.after = pp;
				pp.time_ = last.time_ + 0.1f;
			}
			
			return go;
		}
		
		public override void Validate()
		{
			position.y = 0;
			
			if (map != null) {
				if (posX > map.sizeX * 0.5f) {
					posX = map.sizeX * 0.5f;
				}
				if (posX < -map.sizeX * 0.5f) {
					posX = -map.sizeX * 0.5f;
				}
				if (posZ > map.sizeZ * 0.5f) {
					posZ = map.sizeZ * 0.5f;
				}
				if (posZ < -map.sizeZ * 0.5f){
					posZ = -map.sizeZ * 0.5f;
				}
			}
			
			time = 0;
			
			if (radius_ < 0.1f) {
				radius_ = 0.1f;
			}
			
			if (maxSpeed_ < 0) {
				maxSpeed_ = 0;
			}
			
			if (dirty) {
				foreach (StealthPlayerPosition spp in getSPP()) {
					spp.SubValidate();
				}
				dirty = false;
				CreateMesh();
				
				if (accSurf != null) {
					accSurf.dirty = true;
					accSurf.Validate();
				}
				
				if (Collide()) {
					Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/PlayerCollMat.mat", typeof(Material));
					gameObject.renderer.material = mat;
				} else {
					Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/PlayerMat.mat", typeof(Material));
					gameObject.renderer.material = mat;
				}
			}
		}
	}
	
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
				GameObject go = new GameObject();
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
						currPos = wp.transform.position;
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
					Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/PlayerCollMat.mat", typeof(Material));
					gameObject.renderer.material = mat;
				} else {
					Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/PlayerMat.mat", typeof(Material));
					gameObject.renderer.material = mat;
				}
			}
		}
	}
}
