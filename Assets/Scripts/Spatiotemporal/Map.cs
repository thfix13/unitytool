using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Map : MonoBehaviour {
	public bool dirty = true;

	public Vector3 dimensions = new Vector3 (100.0f, 60.0f, 100.0f);
	public float sub_ = 1;

	public float sizeX
	{
		get { return dimensions.x; }
		set {
			if (dimensions.x != value) {
				dirty = true;
				dimensions.x = value;
				Validate();
			}
		}
	}
	public float timeLength
	{
		get { return dimensions.y; }
		set {
			if (dimensions.y != value) {
				dirty = true;
				dimensions.y = value;
				Validate();
			}
		}
	}
	public float sizeZ
	{
		get { return dimensions.z; }
		set {
			if (dimensions.z != value) {
				dirty = true;
				dimensions.z = value;
				Validate();
			}
		}
	}
	
	public float subdivisionsPerSecond
	{
		get { return sub_; }
		set {
			if (sub_ != value) {
				dirty = true;
				sub_ = value;
				Validate();
			}
		}
	}
	
	private MeshFilter mf
	{
		get {return gameObject.GetComponent<MeshFilter> ();}
	}

	void Awake()
	{
		
		PrefabUtility.DisconnectPrefabInstance(gameObject);
		// Count number of maps
		int mapID = GameObject.FindObjectsOfType(typeof(Map)).Length;
		
//		if (mapID > 1) {
//			Debug.LogError("There can be only one map");
//			Object.DestroyImmediate(gameObject);
//		}
		
		gameObject.name = "Map " + (mapID);

		if (gameObject.GetComponent<MeshFilter> () == null)
			gameObject.AddComponent ("MeshFilter");
		if (gameObject.GetComponent<MeshRenderer> () == null)
			gameObject.AddComponent ("MeshRenderer");
		if (gameObject.GetComponent<MeshCollider> () == null)
			gameObject.AddComponent ("MeshCollider");

		Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/MapMat.mat", typeof(Material));
		gameObject.renderer.material = mat;

		CreateMesh ();
	}

	void Start ()
	{

	}

	void Update ()
	{
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = new Quaternion (0, 0, 0, 1);
		gameObject.transform.localScale = Vector3.one;

		Validate ();
	}

	public void Validate() 
	{
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		
		float x, y, z;
		x = sizeX < 1.0f ? 1.0f : sizeX;
		y = timeLength < 1.0f ? 1.0f : timeLength;
		z = sizeZ < 1.0f ? 1.0f : sizeZ;
		
		dimensions.x = x;
		dimensions.y = y;
		dimensions.z = z;
		
		if (sub_ < 0.1f) {
			sub_ = 0.1f;
		}
		if (sub_ > 4f) {
			sub_ = 4f;
		}
		
		if (dirty) {
			dirty = false;
			UpdateMesh();
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = new Color (0.5f, 0.5f, 0.5f);
		Gizmos.DrawWireCube (new Vector3(0.0f, timeLength * 0.5f, 0.0f),
		                     new Vector3(sizeX, timeLength, sizeZ));
	}
	
	public List<StealthPlayer> GetPlayers()
	{
		List<StealthPlayer> lst = new List<StealthPlayer>();
		
		foreach (Transform child in gameObject.transform) {
			if (child.GetComponent<StealthPlayer>()) {
				lst.Add(child.GetComponent<StealthPlayer>());
			}
		}
		
		return lst;
	}
	
	public List<MapChild> GetChildren()
	{
		List<MapChild> lst = new List<MapChild>();
		
		foreach (Transform child in gameObject.transform) {
			if (child.GetComponent<MapChild>()) {
				lst.Add(child.GetComponent<MapChild>());
			}
		}
		
		return lst;
	}

	public List<StealthObstacle> GetObstacles()
	{
		List<StealthObstacle> lst = new List<StealthObstacle>();
		
		foreach (Transform child in gameObject.transform) {
			if (child.GetComponent<StealthObstacle>()) {
				lst.Add(child.GetComponent<StealthObstacle>());
			}
		}
		
		return lst;
	}

	public List<StealthGuard> GetGuards()
	{
		List<StealthGuard> lst = new List<StealthGuard>();
		
		foreach (Transform child in gameObject.transform) {
			if (child.GetComponent<StealthGuard>()) {
				lst.Add(child.GetComponent<StealthGuard>());
			}
		}
		
		return lst;
	}

	public List<StealthCamera> GetCameras()
	{
		List<StealthCamera> lst = new List<StealthCamera> ();

		foreach (Transform child in gameObject.transform) {
			if (child.GetComponent<StealthCamera>()) {
				lst.Add(child.GetComponent<StealthCamera>());
			}
		}
		
		return lst;
	}

	private Vector3[] Vertices()
	{
		return new Vector3[]{
			new Vector3(0.5f * sizeX, 0, 0.5f * sizeZ),
			new Vector3(0.5f * sizeX, 0, -0.5f * sizeZ),
			new Vector3(-0.5f * sizeX, 0, -0.5f * sizeZ),
			new Vector3(-0.5f * sizeX, 0, 0.5f * sizeZ),
			new Vector3(0.5f * sizeX, timeLength, 0.5f * sizeZ),
			new Vector3(0.5f * sizeX, timeLength, -0.5f * sizeZ),
			new Vector3(-0.5f * sizeX, timeLength, -0.5f * sizeZ),
			new Vector3(-0.5f * sizeX, timeLength, 0.5f * sizeZ),
		};
	}

	public void CreateMesh()
	{
		Mesh m = new Mesh ();
		m.name = "Map Space";
		m.vertices = Vertices ();
		m.triangles = new int[]{
			0, 1, 2, 0, 2, 3,
			4, 1, 0, 4, 5, 1,
			2, 1, 5, 2, 5, 6,
			2, 7, 3, 2, 6, 7,
			3, 7, 4, 3, 4, 0
		};
		m.uv = new Vector2[]{
			new Vector2(0,0),
			new Vector2(1,0),
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(0,0),
			new Vector2(1,0),
			new Vector2(0,1),
			new Vector2(1,1)};
		m.RecalculateNormals();
		mf.sharedMesh = m;
	}

	public void UpdateMesh()
	{
		mf.sharedMesh.vertices = Vertices ();
		
		Mesh m = mf.sharedMesh;
		
		gameObject.GetComponent<MeshCollider>().sharedMesh = null;
		gameObject.GetComponent<MeshCollider>().sharedMesh = m;
		
		foreach (MapChild mc in GetChildren()) {
			mc.dirty = true;
			mc.Validate();
		}
	}
}
