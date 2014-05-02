using UnityEngine;
using UnityEditor;

using Spatiotemporal;

[CustomEditor(typeof(AccessibilitySurface))]
public class AccessibilitySurfaceEditor : Editor {
	private AccessibilitySurface s;
	
	void Awake()
	{
		s = (AccessibilitySurface)target;
	}
	
	public override void OnInspectorGUI()
	{
		s.density = EditorGUILayout.FloatField("Density", s.density);
		s.dirty = EditorGUILayout.Toggle("Refresh", s.dirty);
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
			Tools.current = Tool.None;
		} else if (lastTool == Tool.Scale) {
			Tools.current = Tool.None;
		}
	}
}