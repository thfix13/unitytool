using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using GeometryLib;

namespace Spatiotemporal {
	[ExecuteInEditMode]
	public abstract class StealthGuard : StealthFov {
		public float maxOmega_ = 2*Mathf.PI; // Deg per sec
		public float maxSpeed_ = 1; // Units per sec
		
		public int guardID = 0;
		
		// disable CompareOfFloatsByEqualityOperator
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
			
			var mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/GuardMat.mat", typeof(Material));
			gameObject.renderer.material = mat;
			
		}
		
		public abstract List<Pose> getPositions();
		
		public override List<Shape3> Shapes()
		{
			var shLst = new List<Shape3> ();
			
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
				shLst.Add(Occlude(position + pos, rotation + rot));
				
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
						// disable once ConvertIfStatementToConditionalTernaryExpression
						if (positions.Count > ind + 1) {
							next = positions[ind + 1];
						} else {
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
			base.Validate();
		}
	}
}
