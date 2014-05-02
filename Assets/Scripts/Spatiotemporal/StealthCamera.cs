using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using GeometryLib;

namespace Spatiotemporal {
	[ExecuteInEditMode]
	public class StealthCamera : StealthFov {
		public enum Type: byte {
			Sweeping, Rotating
		};
		private enum Sense: byte {
			CW, CCW
		}
		private enum Motion: byte {
			Turning, Pausing
		}
	
		public Type type_ = Type.Sweeping;
		public float omega_ = 10; // Deg per sec
		public float amplitude_ = 120; // Deg per sweep
		public float pause_ = 0; // Sec
		
		public int cameraID = 0;
		
		public Type type {
			get { return type_; }
			set {
				if (type_ != value) {
					type_ = value;
					dirty = true;
					Validate();
				}
			}
		}
		
		public float omega {
			get { return omega_; }
			set {
				if (omega_ != value) {
					omega_ = value;
					dirty = true;
					Validate();
				}
			}
		}
		
		public float amplitude {
			get { return amplitude_; }
			set {
				if (amplitude_ != value) {
					amplitude_ = value;
					dirty = true;
					Validate();
				}
			}
		}
		
		public float pause {
			get { return pause_; }
			set {
				if (pause_ != value) {
					pause_ = value;
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
			
			
			
			cameraID = map.GetCameras ().Count;
			gameObject.name = "Camera " + cameraID;
			
			Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/CameraMat.mat", typeof(Material));
			gameObject.renderer.material = mat;
		}
		
		public override List<Shape3> Shapes()
		{
			List<Shape3> shLst = new List<Shape3> ();
			
			float rot = 0.0f;
			float time = 0.0f;
			float pauseTime = 0.0f;
			Sense sense;
			if (type == Type.Rotating) {
				sense = amplitude > 0 ? Sense.CW : Sense.CCW;
			} else {
				sense = omega > 0 ? Sense.CW : Sense.CCW;
			}
			Motion motion = Motion.Turning;
			int numSub = Mathf.FloorToInt((map.timeLength-position.y) * map.subdivisionsPerSecond);
			float timeStep = map.timeLength / Mathf.FloorToInt((map.timeLength) * map.subdivisionsPerSecond);
			for (int i = 0; i < numSub; i++) {
				Gizmos.color = new Color (1.0f, 0.1f, 0.2f);
				
				Shape3 vision = FieldOfView.Vertices(viewDistance, fieldOfView, frontSegments, position + new Vector3(0, time, 0), rot + rotation);
				shLst.Add (StealthFov.Occlude(map, vision, position + new Vector3(0, time, 0), viewDistance));
				
				if (type == Type.Rotating) {
					if (motion == Motion.Turning) {
						rot += timeStep * omega;
						if (rot > 360.0f) {
							pauseTime = (rot - 360.0f) / omega;
							if (pauseTime < pause) {
								motion = Motion.Pausing;
								rot = 0.0f;
							} else {
								float overPause = pauseTime - pause;
								rot = overPause * omega;
							}
						}
					} else if (motion == Motion.Pausing) {
						if (pauseTime > pause) {
							float overPause = pauseTime - pause;
							motion = Motion.Turning;
							rot = overPause * omega;
						}
						pauseTime += timeStep;
					}
				} else {
					if (motion == Motion.Turning) {
						rot += (sense == Sense.CW ? timeStep * Mathf.Abs(omega) : -timeStep * Mathf.Abs(omega));
						if (rot > amplitude * 0.5f) {
							sense = Sense.CCW;
							pauseTime = (rot - amplitude * 0.5f) / Mathf.Abs(omega);
							if (pauseTime < pause) {
								motion = Motion.Pausing;
								rot = amplitude * 0.5f;
							} else {
								float overPause = pauseTime - pause;
								rot = amplitude * 0.5f - overPause * Mathf.Abs(omega);
							}
						} else if (rot < -amplitude * 0.5f) {
							sense = Sense.CW;
							pauseTime = (-rot - amplitude * 0.5f) / Mathf.Abs(omega);
							if (pauseTime < pause) {
								motion = Motion.Pausing;
								rot = -amplitude * 0.5f;
							} else {
								float overPause = pauseTime - pause;
								rot = -amplitude * 0.5f + overPause * Mathf.Abs(omega);
							}
						}
					} else if (motion == Motion.Pausing) {
						if (pauseTime > pause) {
							float overPause = pauseTime - pause;
							motion = Motion.Turning;
							if (sense == Sense.CW) {
								rot = -amplitude * 0.5f + overPause * Mathf.Abs(omega);
							} else {
								rot = amplitude * 0.5f - overPause * Mathf.Abs(omega);
							}
						}
						pauseTime += timeStep;
					}
				}
				time += timeStep;
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
			
			if (amplitude < 0.001f) {
				amplitude = 0.001f;
			}
			if (pause < 0) {
				pause = 0;
			}
			
			if (dirty) {
				UpdateMesh();
				dirty = false;
			}
		}
	}
}
