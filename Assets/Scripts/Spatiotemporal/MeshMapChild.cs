using UnityEngine;

public abstract class MeshMapChild : MapChild {
	protected MeshFilter mf
	{
		get { return gameObject.GetComponent<MeshFilter> (); }
	}
	
	new protected void Awake ()
	{
		base.Awake();
		
		if (gameObject.GetComponent<MeshFilter> () == null) {
			gameObject.AddComponent ("MeshFilter");
		}
		
		if (gameObject.GetComponent<MeshRenderer> () == null)
			gameObject.AddComponent ("MeshRenderer");
		
		CreateMesh();
	}
	
	public abstract void CreateMesh();
	
	public abstract void UpdateMesh();
}