using UnityEngine;
using UnityEditor;

using Spatiotemporal;

[CustomEditor(typeof(StealthWaypointPlayer))]
public class StealthWaypointPlayerEditor : Editor {
	private StealthWaypointPlayer p;
	
	void Awake()
	{
		p = (StealthWaypointPlayer)target;
	}
	
	public override void OnInspectorGUI()
	{
		p.radius = EditorGUILayout.FloatField("Radius", p.radius);
		p.maxSpeed = EditorGUILayout.FloatField("Max Speed", p.maxSpeed);
		
		if (GUILayout.Button("Add Accessibility Surface")) {
			GameObject go = new GameObject();
			go.transform.parent = p.map.transform;
			go.AddComponent("AccessibilitySurface");
			go.GetComponent<AccessibilitySurface>().enabled = false;
			go.name = "Accesibility Surface";
			go.GetComponent<AccessibilitySurface>().player = p;
			go.GetComponent<AccessibilitySurface>().enabled = true;
			p.accSurf = go.GetComponent<AccessibilitySurface>();
		}
		
		if (GUILayout.Button("Select Waypoints")) {
			Selection.activeTransform = p.waypoints.transform;
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
	}
}