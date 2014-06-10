using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Triangulation))]
public class TriangulationEditor : Editor {

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		if(GUILayout.Button("Triangulate"))
		{
			//Start the triangulation
			GameObject g = GameObject.Find("Triangulation"); 

			if(g != null)
			{
				Triangulation tObject = g.GetComponent<Triangulation>(); 
				tObject.TriangulationSpace(); 
			}
		}
		if(GUILayout.Button("TriangulateCurves"))
		{
			GameObject g = GameObject.Find("Triangulation"); 
			
			if(g != null)
			{
				Triangulation tObject = g.GetComponent<Triangulation>(); 
				tObject.ShowTriangulation(); 
			}	
		}


	}
}
