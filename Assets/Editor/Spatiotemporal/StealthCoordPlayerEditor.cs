using UnityEngine;
using UnityEditor;

using Spatiotemporal;

[CustomEditor(typeof(StealthCoordPlayer))]
public class StealthCoordPlayerEditor : Editor {
	private StealthCoordPlayer p;
	
	void Awake()
	{
		p = (StealthCoordPlayer)target;
	}
	
	public override void OnInspectorGUI()
	{
		p.radius = EditorGUILayout.FloatField("Radius", p.radius);
		p.maxSpeed = EditorGUILayout.FloatField("Max Speed", p.maxSpeed);
		
		if (GUILayout.Button("Add position")) {
			GameObject pp = p.AddCoordinate();
			if (pp != null) {
				Selection.activeTransform = pp.transform;
			}
		}
		
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
			Vector3 result = Handles.PositionHandle(p.position, p.rotationQ);
			if (result != p.position) {
				p.posX = result.x;
				p.posZ = result.z;
			}
		} else if (lastTool == Tool.Scale) {
			Tools.current = Tool.None;
		}
		
		
	}
}