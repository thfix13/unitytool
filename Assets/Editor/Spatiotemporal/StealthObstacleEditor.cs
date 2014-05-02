using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StealthObstacle))]
public class StealthObstacleEditor : Editor {
	private StealthObstacle o;
	
	void Awake()
	{
		o = (StealthObstacle)target;
	}
	
	public override void OnInspectorGUI()
	{
		float result;
		result = EditorGUILayout.FloatField ("X Position:", o.posX);
		if (o.posX != result) {
			o.posX = result;
			o.map.dirty = true;
			o.map.Validate();
		}
		result = EditorGUILayout.FloatField ("Z Position:", o.posZ);
		if (o.posZ != result) {
			o.posZ = result;
			o.map.dirty = true;
			o.map.Validate();
		}
		result = EditorGUILayout.FloatField ("Rotation:", o.rotation);
		if (o.rotation != result) {
			o.rotation = result;
			o.map.dirty = true;
			o.map.Validate();
		}
		result = EditorGUILayout.FloatField ("Size X:", o.sizeX);
		if (o.sizeX != result) {
			o.sizeX = result;
			o.map.dirty = true;
			o.map.Validate();
		}
		result = EditorGUILayout.FloatField ("Size Z:", o.sizeZ);
		if (o.sizeZ != result) {
			o.sizeZ = result;
			o.map.dirty = true;
			o.map.Validate();
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
			Quaternion result = Handles.RotationHandle(o.rotationQ, o.position);
			if (result != o.rotationQ) {
				o.rotation = result.eulerAngles.y;
				o.map.dirty = true;
				o.map.Validate();
			}
		} else if (lastTool == Tool.Move) {
			Vector3 result = Handles.PositionHandle(o.position, o.rotationQ);
			if (result != o.position) {
				o.posX = result.x;
				o.posZ = result.z;
				o.map.dirty = true;
				o.map.Validate();
			}
		} else if (lastTool == Tool.Scale) {
			Vector3 result = Handles.ScaleHandle (o.dimensions, o.position, o.rotationQ,
			                                      HandleUtility.GetHandleSize(Vector3.zero));
			if (result != o.dimensions) {
				o.sizeX = result.x;
				o.sizeZ = result.z;
				o.map.dirty = true;
				o.map.Validate();
			}
			
		}
		
		
	}
}