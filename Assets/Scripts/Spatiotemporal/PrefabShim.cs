using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PrefabShim : MonoBehaviour {
	void Start ()
	{
		
	}
	
	void Update ()
	{
		if (transform.parent == null) {
			MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
		
			foreach (MonoBehaviour mb in components) {
				if (!mb.enabled)
					mb.enabled = true;
			}
			
			Debug.Log(transform.parent);
			
			DestroyImmediate(this);
		}
	}
}
