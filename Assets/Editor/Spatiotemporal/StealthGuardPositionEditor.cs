using UnityEngine;
using UnityEditor;
using System.Collections;

using Spatiotemporal;

[CustomEditor(typeof(StealthGuardPosition))]
public class StealthGuardPositionEditor : Editor {
	private StealthGuardPosition gp;

	void Awake() {
		gp = (StealthGuardPosition)target;
	}
	
	public override void OnInspectorGUI()
	{
		Vector2 vel = new Vector2(gp.velocity.z, gp.velocity.x);
		vel = EditorGUILayout.Vector2Field("Velocity:", vel);
		gp.velocity = new Vector3(vel.y, 1, vel.x);
		if (GUILayout.Button("Clear velocity")) {
		    gp.velocity = Vector3.zero;
	    }
		gp.time = EditorGUILayout.FloatField("Time:", gp.time);
		gp.omega = EditorGUILayout.FloatField("Omega", gp.omega);
		if (GUILayout.Button("Clear Angular speed")) {
			gp.omega = 0;
		}
		
		GUILayout.Label("");
		
		if (GUILayout.Button("Add position")) {
			Selection.activeGameObject = gp.guard.AddCoordinate();
		}
		
		if (GUILayout.Button("Select Guard")) {
			Selection.activeTransform = gp.guard.transform;
		}
	}

	Tool lastTool = Tool.None;
	
	void OnEnable()
	{
		lastTool = Tools.current;
		Tools.current = Tool.None;
	}
	
	void OnDisable()
	{
		Tools.current = lastTool;
	}

	void OnSceneGUI ()
	{
		if (Tools.current != Tool.None) {
			lastTool = Tools.current;
			Tools.current = Tool.None;
		}

		if (lastTool == Tool.Rotate) {
			if (gp.before != null) {
				Quaternion result = Handles.RotationHandle(Quaternion.Euler(0, gp.before.omega, 0), gp.position);
				if (result.eulerAngles.y != gp.before.omega) {
					gp.before.omega = result.eulerAngles.y;
				}
			} else {
				Tools.current = Tool.None;
			}
		} else if (lastTool == Tool.Move) {
			Vector3 result = Handles.PositionHandle(gp.position, Quaternion.Euler(0, gp.rotation, 0));
			if (result != gp.position) {
				gp.time = result.y;
			}

		} else if (lastTool == Tool.Scale) {
			Vector3 result = Handles.ScaleHandle (new Vector3 (gp.velocity.x, gp.omega, gp.velocity.z),
				gp.position, Quaternion.Euler(0, gp.rotation, 0), HandleUtility.GetHandleSize(gp.position));
			if (result != new Vector3 (gp.velocity.x, gp.omega, gp.velocity.z)) {
				gp.velocity = new Vector3(result.x, 1, result.z);
				gp.omega = result.y;
			}
		}
	}
}
