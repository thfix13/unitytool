using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public abstract class MapChild : MonoBehaviour {
	public Vector3 position = Vector3.zero;
	public Quaternion rotationQ = new Quaternion(0, 0, 0, 1);
	
	public bool dirty = true;
	
	public float posX
	{
		get {return position.x; }
		set {
			if (position.x != value) {
				position.x = value;
				dirty = true;
				Validate();
			}
			
		}
	}
	
	public float time
	{
		get {return position.y; }
		set {
			if (position.y != value) {
				position.y = value;
				dirty = true;
				Validate();
			}
		}
	}
	
	public float posZ
	{
		get {return position.z; }
		set {
			if (position.z != value) {
				position.z = value;
				dirty = true;
				Validate();
			}
		}
	}
	
	public float rotation
	{
		get {return rotationQ.eulerAngles.y; }
		set {
			if (rotationQ.eulerAngles.y != value) {
				rotationQ = Quaternion.Euler(0, value, 0);
				dirty = true;
				Validate();
			}
			
		}
	}
	
	public Map map
	{
		get {
			if (gameObject.activeInHierarchy) {
				if (transform.parent == null)
					return null;
				return (Map)transform.parent.gameObject.GetComponent<Map>();
			}
			return null;
		}
	}
	
	protected void Awake ()
	{
//		if (map == null) {
//			Object.DestroyImmediate(gameObject);
//			Debug.LogError("Parentless Map Child instantiated.");
//			return;
//		}
	}
	
	protected void Start ()
	{
		
	}
	
	protected void Update ()
	{
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = new Quaternion (0, 0, 0, 1);
		gameObject.transform.localScale = Vector3.one;
		
		Validate();
	}
	
	public abstract void MapChanged();
	
	public abstract void Validate();
}