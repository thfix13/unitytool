using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(StealthWaypointGuard))]
public class StealthWaypointGuardEditor : Editor {
	private StealthWaypointGuard g;
	
	void Awake()
	{
		g = (StealthWaypointGuard)target;
	}
	
	public override void OnInspectorGUI()
	{
		g.viewDistance = EditorGUILayout.FloatField("View Distance:", g.viewDistance);
		g.fieldOfView = EditorGUILayout.FloatField("Field of view:", g.fieldOfView);
		g.frontSegments = EditorGUILayout.IntField("Front segments:", g.frontSegments);
		g.maxSpeed = EditorGUILayout.FloatField("Max speed:", g.maxSpeed);
		g.maxOmega = EditorGUILayout.FloatField("Max angular speed:", g.maxOmega);
		
		if (GUILayout.Button("Select Waypoints")) {
			Selection.activeTransform = g.waypoints.transform;
		}
		
		GUILayout.Label("");
		GUILayout.Label("Easiness: " + (g.easiness*100) + "%");
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
	}
}