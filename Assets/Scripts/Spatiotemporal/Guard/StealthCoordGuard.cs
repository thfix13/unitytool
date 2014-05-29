using UnityEngine;
using System.Collections.Generic;

using GeometryLib;

namespace Spatiotemporal {
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
			var lst = new List<StealthGuardPosition>();
	
			foreach (Transform child in gameObject.transform) {
				if (child.GetComponent<StealthGuardPosition>()) {
					lst.Add(child.GetComponent<StealthGuardPosition>());
				}
			}
			
			lst.Sort( (p1, p2) => Mathf.RoundToInt(p1.time_ * 100 - p2.time_ * 100) );
	
			return lst;
		}
		
		public GameObject AddCoordinate() {
			List<StealthGuardPosition> positions = getSGP ();
	
			StealthGuardPosition last = null;
			if (positions.Count > 0) {
				last = positions[positions.Count - 1];
			}
	
			// disable once CompareOfFloatsByEqualityOperator
			if (last != null && last.time == map.timeLength)
				return null;
	
			var go = new GameObject ("Position " + (positions.Count + 1));
			go.transform.parent = transform;
			go.AddComponent ("StealthGuardPosition");
	
			if (last != null) {
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
			var poses = new List<Pose>();
			
			foreach (StealthGuardPosition sgp in getSGP()) {
				var p = new Pose(sgp.position-position, Quaternion.Euler(0, sgp.rotation-rotation, 0));
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
			base.Validate();
		}
	}
}