using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using GeometryLib;

namespace Spatiotemporal {
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
			var lst = new List<Pose>();
	
			foreach (Transform child in gameObject.transform) {
				if (child.GetComponent<StealthPlayerPosition>()) {
					StealthPlayerPosition spp = child.GetComponent<StealthPlayerPosition>();
					var p = new Pose(spp.position, Quaternion.identity);
					p.velocity = spp.velocity_;
					p.omega = 0;
					lst.Add(p);
				}
			}
			
			lst.Sort( (p1, p2) => Mathf.RoundToInt(p1.time * 100 - p2.time * 100) );
	
			return lst;
		}
		
		public List<StealthPlayerPosition> getSPP()
		{
			var lst = new List<StealthPlayerPosition>();
	
			foreach (Transform child in gameObject.transform) {
				if (child.GetComponent<StealthPlayerPosition>()) {
					StealthPlayerPosition spp = child.GetComponent<StealthPlayerPosition>();
					lst.Add(spp);
				}
			}
			
			lst.Sort( (p1, p2) => Mathf.RoundToInt(p1.time * 100 - p2.time * 100) );
	
			return lst;
		}
		
		public GameObject AddCoordinate() {
			List<StealthPlayerPosition> positions = getSPP ();
	
			StealthPlayerPosition last = null;
			if (positions.Count > 0) {
				last = positions[positions.Count - 1];
			}
	
			// disable once CompareOfFloatsByEqualityOperator
			if (last != null && last.time == map.timeLength)
				return null;
	
			var go = new GameObject ("Position " + (positions.Count + 1));
			go.transform.parent = transform;
			go.AddComponent ("StealthPlayerPosition");
	
			if (last != null) {
				
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