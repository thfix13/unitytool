using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace Objects {
	public class WaypointGeo : MonoBehaviour {
		
		public WaypointGeo next;
		public WaypointGeo distractPoint;
		//types of waypoints will be wait, move, and rotate.
		//thus you will do that thing, at the given speed until you match the objects stuff.
		public string type = "wait";
		public float movSpeed = 1;
		public float rotSpeed = 1;
		public int waitTime = 1;

		public static bool debug = true;
		
		// Use this for initialization
		void Start () {
		}
		
		// Update is called once per frame
		void Update () {
		}
		
		void OnDrawGizmos () {
			Gizmos.color = Color.white;
			if (debug)
				Gizmos.DrawSphere (transform.position, 0.1f);
		}
	}
}

