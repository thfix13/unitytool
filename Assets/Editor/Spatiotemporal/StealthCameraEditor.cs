using UnityEngine;
using UnityEditor;
using System.Collections;

using Spatiotemporal;

[CustomEditor(typeof(StealthCamera))]
public class StealthCameraEditor : Editor {
	private StealthCamera c;

	void Awake() {
		c = (StealthCamera)target;
	}
	
	public override void OnInspectorGUI()
	{
		c.rotation = EditorGUILayout.FloatField ("Rotation", c.rotation);
		c.position = EditorGUILayout.Vector3Field ("Position", c.position);
		c.type = (StealthCamera.Type) EditorGUILayout.EnumPopup ("Type", c.type);
		c.omega = EditorGUILayout.FloatField ("Angular speed", c.omega);
		if (c.type == StealthCamera.Type.Sweeping)
			c.amplitude = EditorGUILayout.FloatField ("Amplitude", c.amplitude);
		c.viewDistance = EditorGUILayout.FloatField ("View Distance", c.viewDistance);
		c.fieldOfView = EditorGUILayout.FloatField ("Field of View", c.fieldOfView);
		c.pause = EditorGUILayout.FloatField ("Pause", c.pause);
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
			Quaternion result = Handles.RotationHandle(c.rotationQ, c.position);
			if (result.eulerAngles.y != c.rotation) {
				c.rotation = result.eulerAngles.y;
			}
		} else if (lastTool == Tool.Move) {
			Vector3 result = Handles.PositionHandle(c.position, c.rotationQ);
			if (result != c.position) {
				c.posX = result.x;
				c.posZ = result.z;
			}

		} else if (lastTool == Tool.Scale) {

			if (c.type == StealthCamera.Type.Sweeping) {
				Vector3 result = Handles.ScaleHandle (new Vector3 (c.viewDistance, c.omega, c.amplitude),
				                                      c.position, c.rotationQ,
				                                      HandleUtility.GetHandleSize(c.position));
				if (result != new Vector3(c.viewDistance, c.omega, c.amplitude)) {
					c.viewDistance = result.x;
					c.omega = result.y;
					c.amplitude = result.z;
				}

			} else {
				Vector3 result = Handles.ScaleHandle (new Vector3 (c.viewDistance, c.omega, c.fieldOfView),
				                                      c.position, c.rotationQ,
														HandleUtility.GetHandleSize(c.position));

				if (result != new Vector3(c.viewDistance, c.omega, c.fieldOfView)) {
					c.viewDistance = result.x;
					c.omega = result.y;
					c.fieldOfView = result.z;
				}
			}
		}
	}
}
