using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CreateLayeredMesh))]
public class  MedialEditor: Editor {
	int sg=-1, sgnew;
	GameObject g=null;
	CreateLayeredMesh obj;
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		if(g==null)
		{
			g = GameObject.Find("Medial");
			obj=g.GetComponent<CreateLayeredMesh>();
		}
		var text = new string[] {"Moving_gaurd", "Convex", "Arena"};
		sgnew=GUILayout.SelectionGrid(sg, text, 1, EditorStyles.radioButton);
		if(sgnew!=sg){
			obj.buildArena(sgnew);
			sg=sgnew;
		}
		if(GUILayout.Button("Get Medial Skeleton"))
		{
			
//			obj.BuildMedial();
		}
//		if(GUILayout.Button("Get Medial Skeleton"))
//		{
//			
//			obj.BuildMedial();
//		}
	}
}
[CustomEditor(typeof(Moving_guard_arena))]
public class  MedialEditor2: Editor {
	GameObject g=null;
	Moving_guard_arena obj;
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		if(g==null)
		{
			g = GameObject.Find("Medial");
			obj=g.GetComponent<Moving_guard_arena>();
		}

		if(GUILayout.Button("Get Medial Skeleton"))
		{
			
			obj.buildMedial();
		}
		GUILayout.BeginHorizontal("box");
		if(GUILayout.Button("Project",GUILayout.Width(100)))
		{
			
			obj.projectPath();
		}
		if(GUILayout.Button("Pause/Play",GUILayout.Width(100)))
		{
			
			obj.playpause();
		}
		GUILayout.EndHorizontal();
	}
}