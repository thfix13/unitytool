using UnityEngine;
using GeometryLib;

namespace Spatiotemporal {
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
			Shape3 shape = StealthFov.Vertices(guard.viewDistance, guard.fieldOfView, guard.frontSegments, position, rotation);
			
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
}