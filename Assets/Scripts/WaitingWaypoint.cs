using UnityEngine;
using System.Collections.Generic;

namespace Objects {
	public class WaitingWaypoint : Waypoint {
		
		public float waitingTime;
		[HideInInspector]
		public Dictionary<int, float>
			times = new Dictionary<int, float> ();
		
	}
}