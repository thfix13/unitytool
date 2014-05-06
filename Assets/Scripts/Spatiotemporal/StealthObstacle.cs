using UnityEngine;
using UnityEditor;
using System.Collections;

using GeometryLib;

namespace Spatiotemporal {
	[ExecuteInEditMode]
	public class StealthObstacle : MeshMapChild, Obstacle {
		public Vector3 dimensions = new Vector3(10.0f, 10.0f, 10.0f);
		
		public int obstacleID;
		
		public float sizeX
		{
			get { return dimensions.x; }
			set {
				if (dimensions.x != value) {
					dimensions.x = value;
					dirty = true;
					Validate();
				}
			}
		}
		
		public float sizeZ
		{
			get { return dimensions.z; }
			set {
				if (dimensions.z != value) {
					dimensions.z = value;
					dirty = true;
					Validate();
				}
			}
		}
		
		new protected void Awake()
		{
			base.Awake();
			
			if (gameObject.GetComponent<MeshCollider>() == null) {
				MeshCollider mc = (MeshCollider)gameObject.AddComponent("MeshCollider");
				mc.convex = true;
				mc.isTrigger = true;
			}
			
			rotation = 1;
			
			obstacleID = map.GetObstacles ().Count;
			gameObject.name = "Obstacle " + obstacleID;
			
			Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/ObstacleMat.mat", typeof(Material));
			gameObject.renderer.material = mat;
		}
		
		void OnDrawGizmos()
		{
			Gizmos.color = new Color (0.5f, 0.5f, 0.5f);
			Gizmos.matrix = Matrix4x4.TRS (position, rotationQ, Vector3.one);
	
			Gizmos.DrawWireCube (new Vector3(0.0f, map.timeLength * 0.5f, 0.0f), dimensions);
		}
		
		public Vector3[] Vertices()
		{
			Quaternion rot = rotationQ;
			Vector3 pos = position;
			return new Vector3[]{
				rot * new Vector3( sizeX*0.5f, 0,  sizeZ*0.5f) + pos,
				rot * new Vector3( sizeX*0.5f, 0, -sizeZ*0.5f) + pos,
				rot * new Vector3(-sizeX*0.5f, 0, -sizeZ*0.5f) + pos,
				rot * new Vector3(-sizeX*0.5f, 0,  sizeZ*0.5f) + pos,
				rot * new Vector3( sizeX*0.5f, map.timeLength,  sizeZ*0.5f) + pos,
				rot * new Vector3( sizeX*0.5f, map.timeLength, -sizeZ*0.5f) + pos,
				rot * new Vector3(-sizeX*0.5f, map.timeLength, -sizeZ*0.5f) + pos,
				rot * new Vector3(-sizeX*0.5f, map.timeLength,  sizeZ*0.5f) + pos,
			};
		}
		
		public override void CreateMesh() {
			Mesh m = new Mesh ();
			m.name = "Obstacle Prism";
			m.vertices = Vertices ();
			m.triangles = new int[]{
				0, 2, 1, 0, 3, 2,
				4, 0, 1, 4, 1, 5,
				2, 5, 1, 2, 6, 5,
				2, 3, 7, 2, 7, 6,
				3, 0, 4, 3, 4, 7,
				4, 5, 6, 4, 6, 7
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
		
		public override void UpdateMesh()
		{
			mf.sharedMesh.vertices = Vertices ();
			mf.sharedMesh.RecalculateNormals();
		}
		
		public override void MapChanged()
		{
			
		}
		
		public override void Validate()
		{
			if (map != null) {
				if (posX > map.sizeX * 0.5f) {
					posX = map.sizeX * 0.5f;
				}
				if (posX < - map.sizeX * 0.5f) {
					posX = - map.sizeX * 0.5f;
				}
				if (posZ > map.sizeZ * 0.5f) {
					posZ = map.sizeZ * 0.5f;
				}
				if (posZ < - map.sizeZ * 0.5f) {
					posZ = - map.sizeZ * 0.5f;
				}
			}
			
			if (sizeX < 0.1f || float.IsNaN(sizeX))
				sizeX = 0.1f;
			if (sizeZ < 0.1f || float.IsNaN(sizeZ))
				sizeZ = 0.1f;
			
			dimensions.y = map.timeLength;
			position.y = 0.0f;
			
			if (dirty) {
				dirty = false;
				UpdateMesh();
			}
		}
		
		public Shape3 GetShape()
		{
			Shape3 ret = new Shape3();
			ret.addVertex (rotationQ * new Vector3 (sizeX * 0.5f, 0, sizeZ * 0.5f) + position);
			ret.addVertex (rotationQ * new Vector3(sizeX*0.5f, 0, -sizeZ*0.5f) + position);
			ret.addVertex (rotationQ * new Vector3(-sizeX*0.5f, 0, -sizeZ*0.5f) + position);
			ret.addVertex (rotationQ * new Vector3(-sizeX*0.5f, 0, sizeZ*0.5f) + position);
			
			return ret;
		}
		
		public Shape3 ShadowPolygon(Vector3 viewpoint, float viewDistance)
		{
			Shape3 obsShape = new Shape3 ();
	
			obsShape.addVertex (rotationQ * new Vector3(sizeX * 0.5f, viewpoint.y, sizeZ * 0.5f) + position);
			obsShape.addVertex (rotationQ * new Vector3(sizeX*0.5f, viewpoint.y, -sizeZ*0.5f) + position);
			obsShape.addVertex (rotationQ * new Vector3(-sizeX*0.5f, viewpoint.y, -sizeZ*0.5f) + position);
			obsShape.addVertex (rotationQ * new Vector3(-sizeX*0.5f, viewpoint.y, sizeZ*0.5f) + position);
	
			int first = 13; // bogus int for signalling uninitialization
	
			// Detect whether an edge should be projected,
			// or rather stick to the obstacle
			bool[] projectedEdge = new bool[4];
			float farthest = 0;
			int count = 0;
			int i = 0;
			
			bool inside = obsShape.PointInside(viewpoint);
			
			foreach (Edge3Abs e in obsShape) {
				if ((e.a - viewpoint).magnitude > farthest)
					farthest = (e.a - viewpoint).magnitude;
				
				projectedEdge[i] = !(inside || !(Vector3.Cross(e.a - viewpoint, e.GetDiff()).y > 0));
				
				if (!projectedEdge[i]) {
					count++;
					if (first == 13)
						first = i;
					if (first == (i + 1) % 4)
						first = i;
				}
				
				i++;
			}
	
			if (viewDistance > farthest)
				farthest = viewDistance;
			farthest *= 2f;
	
			int v = 0;
			Shape3 shape = new Shape3 ();
			if (count == 4) {
				IEnumerator obsShapeRH = obsShape.Reverse();
				while(obsShapeRH.MoveNext()) {
					Edge3Abs e = (Edge3Abs)obsShapeRH.Current;
					//shape.addVertex ((e.a - viewpoint).normalized * farthest + viewpoint);
					shape.addVertex (e.a);
				}
			} else {
	
				for (int j = first; j < 4 + first; j++) {
					if (projectedEdge [j % 4]) {
						// Should add an edge from unprojected to projected
						if (!projectedEdge [((j - 1) % 4 + 4) % 4]) {
								shape.addVertex ((obsShape [j % 4] - viewpoint).normalized * farthest + viewpoint);
						}
						shape.addVertex((obsShape [(j + 1) % 4] - viewpoint).normalized * farthest + viewpoint);
					} else {
						if (v == 0) {
							shape.addVertex (obsShape [j % 4]);
							shape.addVertex (obsShape [(j + 1) % 4]);
							v = 2;
						} else {
							shape.addVertex (obsShape [(j + 1) % 4]);
						}
					}
				}
	
			}
			return shape;
		}
	}
	
	public interface Obstacle {
		Shape3 ShadowPolygon(Vector3 viewpoint, float viewDistance);
	}
}