
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(StealthPlayerPosition))]
public class StealthPlayerPositionEditor : Editor {
	private StealthPlayerPosition pp;

	void Awake() {
		pp = (StealthPlayerPosition)target;
	}
	
	public override void OnInspectorGUI()
	{
		Vector2 vel = new Vector2(pp.velocity.x, pp.velocity.z);
		vel = EditorGUILayout.Vector2Field("Velocity:", vel);
		pp.velocity = new Vector3(vel.x, 1, vel.y);
		
		if (GUILayout.Button("Clear velocity")) {
		    pp.velocity = Vector3.zero;
	    }
	
		pp.time = EditorGUILayout.FloatField("Time:", pp.time);
		
		if (pp.player.GetType().IsAssignableFrom(typeof(StealthCoordPlayer))) {
			if (GUILayout.Button("Add position")) {
				Selection.activeGameObject = ((StealthCoordPlayer)pp.player).AddCoordinate();
				
			}
		}
		
		
		if (GUILayout.Button("Select Guard")) {
			Selection.activeTransform = pp.player.transform;
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
			Tools.current = Tool.None;
		} else if (lastTool == Tool.Move) {
			Vector3 result = Handles.PositionHandle(pp.position, Quaternion.Euler(0, 0, 0));
			if (result != pp.position) {
				pp.time = result.y;
			}

		} else if (lastTool == Tool.Scale) {
			Vector3 result = Handles.ScaleHandle (new Vector3 (pp.velocity.x, 1, pp.velocity.z),
				pp.position, Quaternion.Euler(0, 0, 0), HandleUtility.GetHandleSize(pp.position));
			if (result != new Vector3 (pp.velocity.x, 1, pp.velocity.z)) {
				pp.velocity = new Vector3(result.x, 1, result.z);
			}
		}
	}
}
