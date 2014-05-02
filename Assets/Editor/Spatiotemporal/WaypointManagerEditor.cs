using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(WaypointManager))]
public class WaypointManagerEditor : Editor {
	// Custom Editors are only called when the object is selected.
	// To do Gizmos draws, the code should be placed in the inspected class
	
	private WaypointManager wm;
	
	void Awake() {
		wm = (WaypointManager)target;
	}
	
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		
		if (GUILayout.Button("Add Waypoint")) {
			wm.AddWaypoint();
		}
		if (GUILayout.Button("Add Waiting Waypoint")) {
			wm.AddWaitingWaypoint();
		}
		if (GUILayout.Button("Add Rotation Waypoint")) {
			wm.AddRotationWaypoint();
	    }
		
		GUILayout.Label("");
		
		if (GUILayout.Button("Clear")) {
			wm.Clear();
		}
	}
}