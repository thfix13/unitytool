//using UnityEngine;
//using UnityEditor;
//
//[CustomEditor(typeof(FoV))]
//public class FoVEditor : Editor {
//	private FoV f;
//	
//	void Awake()
//	{
//		f = (FoV)target;
//	}
//	
//	public override void OnInspectorGUI()
//	{
//		f.posX = EditorGUILayout.FloatField ("X Position:", f.posX);
//		f.posZ = EditorGUILayout.FloatField ("Z Position:", f.posZ);
//		f.rotation = EditorGUILayout.FloatField ("Rotation:", f.rotation);
//		f.viewDistance = EditorGUILayout.FloatField("View Distance:", f.viewDistance);
//		f.fieldOfView = EditorGUILayout.FloatField("Field of view:", f.fieldOfView);
//		f.frontSegments = EditorGUILayout.IntField("Front segments", f.frontSegments);
//		f.spline = (Spline)EditorGUILayout.ObjectField("Spline", f.spline, typeof(Spline));
//		
//		if (f.spline != null) {
//			GUILayout.Label("" + f.spline.GetShape().handedness);
//		}
//	}
//	
//	Tool lastTool = Tool.None;
//	
//	void OnEnable()
//	{
//		lastTool = Tools.current;
//		Tools.current = Tool.None;
//	}
//	
//	void OnDisable()
//	{
//		Tools.current = lastTool;
//	}
//	
//	void OnSceneGUI ()
//	{
//		if (Tools.current != Tool.None) {
//			lastTool = Tools.current;
//			Tools.current = Tool.None;
//		}
//		
//		if (lastTool == Tool.Rotate) {
//			Quaternion result = Handles.RotationHandle(f.rotationQ, f.posV);
//			if (result != f.rotationQ) {
//				f.rotation = result.eulerAngles.y;
//			}
//		} else if (lastTool == Tool.Move) {
//			Vector3 result = Handles.PositionHandle(f.posV, f.rotationQ);
//			if (result != f.posV) {
//				f.posX = result.x;
//				f.posZ = result.z;
//			}
//
//		} else if (lastTool == Tool.Scale) {
//			Vector3 result = Handles.ScaleHandle (new Vector3 (f.viewDistance, 1, f.fieldOfView),
//				                                      f.posV, f.rotationQ,
//				                                      HandleUtility.GetHandleSize(f.posV));
//
//			if (result != new Vector3(f.viewDistance, 1, f.fieldOfView)) {
//				f.viewDistance = result.x;
//				f.fieldOfView = result.z;
//			}
//		}
//		
//		
//	}
//}