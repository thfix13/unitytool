using UnityEngine;
using System.Collections;
using UnityEditor;
	namespace Medial{
//	[CustomEditor(typeof(CreateLayeredMesh))]
	public class AlphaNumericSort : BaseHierarchySort
	{
		public override int Compare(GameObject lhs, GameObject rhs)
		{
			if (lhs == rhs) return 0;
			if (lhs == null) return -1;
			if (rhs == null) return 1;
			
			return EditorUtility.NaturalCompare(lhs.name, rhs.name);
		}
	}
	[CustomEditor(typeof(_CreateLayeredMesh_ConvexExample))]
	public class  MedialEditor: Editor {
		int sg=-1, sgnew;
		GameObject g=null;
		_CreateLayeredMesh_ConvexExample obj;
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			if(g==null)
			{
				g = GameObject.Find("Medial");
				obj=g.GetComponent<_CreateLayeredMesh_ConvexExample>();
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

			if(GUILayout.Button("Get Medial Skeleton+Create Graph",GUILayout.Width(200)))
			{
				
				obj.buildMedial();
			}
			GUILayout.BeginHorizontal("box");
//			if(GUILayout.Button("Create graph",GUILayout.Width(50)))
//			{
//				obj.Addextraedges();
//			}
			if(GUILayout.Button("Remove Vs",GUILayout.Width(100)))
			{
				obj.RemoveVs();
			}
			if(GUILayout.Button("Add edges",GUILayout.Width(100)))
			{
				obj.Addextraedges();
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal("box");
			if(GUILayout.Button("Show Path",GUILayout.Width(50)))
			{
				
				obj.showPath();
			}
			if(GUILayout.Button("Project Path",GUILayout.Width(50)))
			{
				
				obj.projectPathOn2D();
			}
			if(GUILayout.Button("Pause/Play",GUILayout.Width(75)))
			{
				
				obj.playpause();
			}
			if(GUILayout.Button("GoBack",GUILayout.Width(50)))
			{
				
				obj.goBack();
			}
			if(GUILayout.Button("Reset",GUILayout.Width(50)))
			{
				
				obj.Reset();
			}
			GUILayout.EndHorizontal();
		}
	}
}