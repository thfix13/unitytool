using UnityEditor;
using UnityEngine;

using Objects;

[CustomEditor(typeof(Waypoint), true)]
public class WaypointEditor : Editor {
	// Custom Editors are only called when the object is selected.
	// To do Gizmos draws, the code should be placed in the inspected class
	
	private static bool debug = true;
	private int i;
	
	private Waypoint w;
	
	void Awake() {
		w = (Waypoint)target;
	}
	
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		SceneView.RepaintAll();
		
		
		if (((Waypoint) target).next == null) {
			if (GUILayout.Button("Add Waypoint")) {
				w.manager.AddWaypoint();
			}
			if (GUILayout.Button("Add Waiting Waypoint")) {
				w.manager.AddWaitingWaypoint();
			}
			if (GUILayout.Button("Add Rotation Waypoint")) {
				w.manager.AddRotationWaypoint();
		    }
		} else {
			if (!typeof(WaitingWaypoint).IsAssignableFrom(w.GetType()) && !typeof(RotationWaypoint).IsAssignableFrom(w.GetType())) {
				EditorGUILayout.LabelField("");
			}
			if (GUILayout.Button("Select next")) {
				Selection.activeObject = ((Waypoint) target).next.gameObject;
			}
		}
		
	}
	
	public void OnSceneGUI() {
		Waypoint wp = (Waypoint) target;
		
		if (debug && wp.next != null) {
			Quaternion q = wp.transform.rotation;
			if (wp.next.transform.position - wp.transform.position != Vector3.zero)
				q.SetLookRotation(wp.next.transform.position - wp.transform.position);
			Handles.ArrowCap(0, wp.transform.position, q, HandleUtility.GetHandleSize(wp.transform.position));
			Handles.DrawLine(wp.transform.position, wp.next.transform.position);
		}
	}
}