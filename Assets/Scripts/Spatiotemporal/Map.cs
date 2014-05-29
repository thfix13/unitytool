using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using GeometryLib;

namespace Spatiotemporal {
	[ExecuteInEditMode]
	public class Map : MonoBehaviour, IObstacle {
		public bool dirty = true;
	
		public Vector3 dimensions = new Vector3 (100.0f, 60.0f, 100.0f);
		public float sub_ = 1;
		public bool clipMap_ = true;
		
		public bool clipMap
		{
			get { return clipMap_; }
			set {
				if (clipMap_ != value) {
					dirty = true;
					clipMap_ = value;
					Validate();
				}
			}
		}
		
		public Vector3 position { get { return Vector3.zero; } }
		public float rotation { get { return 0.0f; } }
		
		// disable CompareOfFloatsByEqualityOperator
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
		
		private MeshFilter mf { get {return gameObject.GetComponent<MeshFilter> ();} }
	
		void Awake()
		{
			gameObject.name = "Map";
			
			if (gameObject.GetComponent<MeshFilter> () == null)
				gameObject.AddComponent ("MeshFilter");
			if (gameObject.GetComponent<MeshRenderer> () == null)
				gameObject.AddComponent ("MeshRenderer");
			if (gameObject.GetComponent<MeshCollider> () == null)
				gameObject.AddComponent ("MeshCollider");
			
			var mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/MapMat.mat", typeof(Material));
			gameObject.renderer.material = mat;
		}
		
		void Start ()
		{
	
		}
		
		void OnEnable() {
			CreateMesh();
		}
		
		void Reset()
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
			var lst = new List<StealthPlayer>();
			
			foreach (Transform child in gameObject.transform) {
				if (child.GetComponent<StealthPlayer>()) {
					lst.Add(child.GetComponent<StealthPlayer>());
				}
			}
			
			return lst;
		}
		
		public List<MapChild> GetChildren()
		{
			var lst = new List<MapChild>();
			
			foreach (Transform child in gameObject.transform) {
				if (child.GetComponent<MapChild>()) {
					lst.Add(child.GetComponent<MapChild>());
				}
			}
			
			return lst;
		}
	
		public List<IObstacle> GetObstacles()
		{
			var lst = new List<IObstacle>();
			
			foreach (Transform child in gameObject.transform) {
				if (child.GetComponent<StealthObstacle>()) {
					lst.Add(child.GetComponent<StealthObstacle>());
				}
			}
			if (clipMap_) {
				lst.Add(this);
			}
			
			return lst;
		}
	
		public List<StealthGuard> GetGuards()
		{
			var lst = new List<StealthGuard>();
			
			foreach (Transform child in gameObject.transform) {
				if (child.GetComponent<StealthGuard>()) {
					lst.Add(child.GetComponent<StealthGuard>());
				}
			}
			
			return lst;
		}
	
		public List<StealthCamera> GetCameras()
		{
			var lst = new List<StealthCamera> ();
	
			foreach (Transform child in gameObject.transform) {
				if (child.GetComponent<StealthCamera>()) {
					lst.Add(child.GetComponent<StealthCamera>());
				}
			}
			
			return lst;
		}
	
		private Vector3[] Vertices()
		{
			return new []{
				// Bottom
				new Vector3( 0.5f * sizeX, 0,  0.5f * sizeZ),
				new Vector3( 0.5f * sizeX, 0, -0.5f * sizeZ),
				new Vector3(-0.5f * sizeX, 0, -0.5f * sizeZ),
				new Vector3(-0.5f * sizeX, 0,  0.5f * sizeZ),
				// North
				new Vector3( 0.5f * sizeX, 0,  0.5f * sizeZ),
				new Vector3(-0.5f * sizeX, 0,  0.5f * sizeZ),
				new Vector3(-0.5f * sizeX, timeLength,  0.5f * sizeZ),
				new Vector3( 0.5f * sizeX, timeLength,  0.5f * sizeZ),
				// West
				new Vector3( 0.5f * sizeX, 0,  0.5f * sizeZ),
				new Vector3( 0.5f * sizeX, timeLength,  0.5f * sizeZ),
				new Vector3( 0.5f * sizeX, timeLength, -0.5f * sizeZ),
				new Vector3( 0.5f * sizeX, 0, -0.5f * sizeZ),
				// South
				new Vector3(-0.5f * sizeX, 0, -0.5f * sizeZ),
				new Vector3( 0.5f * sizeX, 0, -0.5f * sizeZ),
				new Vector3( 0.5f * sizeX, timeLength, -0.5f * sizeZ),
				new Vector3(-0.5f * sizeX, timeLength, -0.5f * sizeZ),
				// East
				new Vector3(-0.5f * sizeX, 0, -0.5f * sizeZ),
				new Vector3(-0.5f * sizeX, timeLength, -0.5f * sizeZ),
				new Vector3(-0.5f * sizeX, timeLength,  0.5f * sizeZ),
				new Vector3(-0.5f * sizeX, 0,  0.5f * sizeZ),
			};
		}
	
		public void CreateMesh()
		{
			var m = new Mesh ();
			m.name = "Map Space";
			m.vertices = Vertices ();
			m.triangles = new []{
				// Bottom
				0, 1, 2, 0, 2, 3,
				// North
				4, 5, 6, 4, 6, 7,
				// West
				8, 9, 10, 8, 10, 11,
				// South
				12, 13, 14, 12, 14, 15,
				// East
				16, 17, 18, 16, 18, 19
			};
			m.uv = new Vector2[m.vertexCount];
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
		
		public Shape3 GetShape() {
			var ret = new Shape3();
			ret.AddVertex(new Vector3( sizeX*0.5001f, 0, sizeZ*0.5001f));
			ret.AddVertex(new Vector3( sizeX*0.5001f, 0,-sizeZ*0.5001f));
			ret.AddVertex(new Vector3(-sizeX*0.5001f, 0,-sizeZ*0.5001f));
			ret.AddVertex(new Vector3(-sizeX*0.5001f, 0, sizeZ*0.5001f));
			return ret;
		}
		
		public Shape3 ShadowPolygon(Vector3 viewpoint, float viewDistance){
			return GetShape();
		}
	}
	
	[ExecuteInEditMode]
	public abstract class MapChild : MonoBehaviour {
		public Vector3 position = Vector3.zero;
		public Quaternion rotationQ = new Quaternion(0, 0, 0, 1);
		
		public bool dirty = true;
		
		public float posX
		{
			get {return position.x; }
			set {
				// disable once CompareOfFloatsByEqualityOperator
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
				// disable once CompareOfFloatsByEqualityOperator
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
				// disable once CompareOfFloatsByEqualityOperator
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
				// disable once CompareOfFloatsByEqualityOperator
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
					return transform.parent == null ?
						null : (Map)transform.parent.gameObject.GetComponent<Map>();
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
}