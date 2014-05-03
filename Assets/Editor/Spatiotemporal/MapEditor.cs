using UnityEngine;
using UnityEditor;

using Spatiotemporal;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor {
	private Map m;

	void Awake ()
	{
		m = (Map)target;
	}

	public override void OnInspectorGUI()
	{
		m.master = (Mapper)EditorGUILayout.ObjectField("Master", m.master, typeof(Mapper));
		m.sizeX = EditorGUILayout.FloatField ("Size X:", m.sizeX);
		m.timeLength = EditorGUILayout.FloatField ("Time length:", m.timeLength);
		m.sizeZ = EditorGUILayout.FloatField ("Size Z:", m.sizeZ);
		m.subdivisionsPerSecond = EditorGUILayout.FloatField("Subdivisions Per Second", m.subdivisionsPerSecond);
		
		if (GUILayout.Button("Add Obstacle")) {
			GameObject go = new GameObject();
			go.transform.parent = m.transform;
			go.AddComponent("StealthObstacle");
			Selection.activeTransform = go.transform;
		}
		
		if (GUILayout.Button("Add Coordinate Guard")) {
			GameObject go = new GameObject();
			go.transform.parent = m.transform;
			go.AddComponent("StealthCoordGuard");
			Selection.activeTransform = go.transform;
		}
		
		if (GUILayout.Button("Add Waypoint Guard")) {
			GameObject go = new GameObject();
			go.transform.parent = m.transform;
			go.AddComponent("StealthWaypointGuard");
			Selection.activeTransform = go.transform;
		}
		
		if (GUILayout.Button("Add Camera")) {
			GameObject go = new GameObject();
			go.transform.parent = m.transform;
			go.AddComponent("StealthCamera");
			Selection.activeTransform = go.transform;
		}
		
		if (GUILayout.Button("Add Coordinate Player")) {
			GameObject go = new GameObject();
			go.transform.parent = m.transform;
			go.AddComponent("StealthCoordPlayer");
			Selection.activeTransform = go.transform;
		}
		
		if (GUILayout.Button("Add Waypoint Player")) {
			GameObject go = new GameObject();
			go.transform.parent = m.transform;
			go.AddComponent("StealthWaypointPlayer");
			Selection.activeTransform = go.transform;
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

		Vector3 result = Handles.ScaleHandle (new Vector3 (m.sizeX, m.timeLength, m.sizeZ),
		                    Vector3.zero, new Quaternion (0, 0, 0, 1),
		                    HandleUtility.GetHandleSize(Vector3.zero));
		m.sizeX = result.x;
		m.timeLength = result.y;
		m.sizeZ = result.z;


	}
}
