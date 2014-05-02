using UnityEngine;
using UnityEditor;

using Spatiotemporal;

[CustomEditor(typeof(StealthFov))]
public class StealthFovEditor : Editor {
	private StealthFov f;
	
	void Awake()
	{
		f = (StealthFov)target;
	}
	
	public override void OnInspectorGUI()
	{
		f.posX = EditorGUILayout.FloatField ("X Position:", f.posX);
		f.posZ = EditorGUILayout.FloatField ("Z Position:", f.posZ);
		f.rotation = EditorGUILayout.FloatField ("Rotation:", f.rotation);
		f.viewDistance = EditorGUILayout.FloatField("View Distance:", f.viewDistance);
		f.fieldOfView = EditorGUILayout.FloatField("Field of view:", f.fieldOfView);
		f.frontSegments = EditorGUILayout.IntField("Front segments", f.frontSegments);
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
			Quaternion result = Handles.RotationHandle(f.rotationQ, f.position);
			if (result != f.rotationQ) {
				f.rotation = result.eulerAngles.y;
			}
		} else if (lastTool == Tool.Move) {
			Vector3 result = Handles.PositionHandle(f.position, f.rotationQ);
			if (result != f.position) {
				f.posX = result.x;
				f.posZ = result.z;
			}

		} else if (lastTool == Tool.Scale) {
			Vector3 result = Handles.ScaleHandle (new Vector3 (f.viewDistance, 1, f.fieldOfView),
				                                      f.position, f.rotationQ,
				                                      HandleUtility.GetHandleSize(f.position));

			if (result != new Vector3(f.viewDistance, 1, f.fieldOfView)) {
				f.viewDistance = result.x;
				f.fieldOfView = result.z;
			}
		}
		
		
	}
}