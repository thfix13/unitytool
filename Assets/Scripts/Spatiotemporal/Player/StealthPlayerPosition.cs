using UnityEngine;

namespace Spatiotemporal {
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
				// disable once CompareOfFloatsByEqualityOperator
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
					return transform.parent == null ?
						null : (StealthPlayer)transform.parent.gameObject.GetComponent<StealthPlayer>();
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
			
			velocity_.y = 1.0f;
			
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
}