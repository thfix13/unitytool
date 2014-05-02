using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using GeometryLib;

namespace Spatiotemporal {
	[ExecuteInEditMode]
	public class AccessibilitySurface : MeshMapChild {
		public StealthPlayer player;
		public float density_ = 0.2f;
		
		public float density {
			get { return density_; }
			set {
				if (density_ != value) {
					dirty = true;
					density_ = value;
					Validate();
				}
			}
		}
		
		new protected void Start()
		{
			base.Start();
			
			gameObject.name = "Accesibility Surface";
			
			Material mat = (Material)AssetDatabase.LoadAssetAtPath("Assets/Materials/SurfMat.mat", typeof(Material));
			gameObject.renderer.material = mat;
		}
		
		private Vector3[] Vertices() {
			float[, ] wv = Wavefront();
			
			int width = Mathf.CeilToInt(map.dimensions.x*density_);
			int height = Mathf.CeilToInt(map.dimensions.z*density_);
			
			Vector3[] vertices = new Vector3[width*height];
			
			
			float xpos = -map.sizeX * 0.5f;
			float ypos = -map.sizeZ * 0.5f;
			float stepx = map.sizeX / width;
			float stepy = map.sizeZ / height;
			for(int y = 0; y < height; y++) {
				xpos = -map.sizeX * 0.5f;
				for (int x = 0; x < width; x++) {
					if (wv[x, y] != 1) {
						vertices[y * width + x] = new Vector3(xpos, (wv[x, y]-2) / density_ / player.maxSpeed, ypos);
					} else {
						vertices[y * width + x] = new Vector3(xpos, -1, ypos);
					}
					
					xpos += stepx;
				}
				ypos += stepy;
			}
			
			return vertices;
		}
		
		public override void CreateMesh()
		{
			Mesh m = new Mesh ();
			m.name = "Access. Surface";
			m.vertices = Vertices ();
			
			int width = Mathf.CeilToInt(map.dimensions.x*density_);
			int height = Mathf.CeilToInt(map.dimensions.z*density_);
			
			int[] tris = new int[(width - 1)*(height - 1)*6];
			
			int tri = 0;
			for (int y = 0; y < height - 1; y++) {
				for (int x = 0; x < width - 1; x++) {
					int index = y * width + x;
					
					tris[tri++] = index;
					tris[tri++] = index + width + 1;
					tris[tri++] = index + 1;
					
					
					tris[tri++] = index;
					tris[tri++] = index + width;
					tris[tri++] = index + width + 1;
					
				}
			}
			
			m.triangles = tris;
			
			m.uv = new Vector2[m.vertices.Length];
			m.RecalculateNormals();
			mf.sharedMesh = m;
		}
		
		public override void UpdateMesh()
		{
			
		}
		
		public override void MapChanged()
		{
			
		}
		
		public override void Validate()
		{
			if (density_ < 0.1f)
				density_ = 0.1f;
			
			int width = Mathf.CeilToInt(map.dimensions.x*density_);
			int height = Mathf.CeilToInt(map.dimensions.z*density_);
			
			while (width * height * density_ > 65536) {
				density_ -= 0.01f;
			}
			
			if (dirty) {
				CreateMesh();
				dirty = false;
			}
		}
		
		public void OnDrawGizmos()
		{
	//		float[, ] wavefront = Wavefront();
	//		
	//		Vector3 offset = -map.dimensions * 0.5f;
	//		
	//		for (int x = 0; x < wavefront.GetLength(0); x++){
	//			for (int y = 0; y < wavefront.GetLength(1); y++) {
	//				Gizmos.color = new Color(wavefront[x, y] * 0.01f, wavefront[x, y] * 0.01f, wavefront[x, y] * 0.01f);
	//				Gizmos.DrawCube(new Vector3(x/density, wavefront[x, y] * 0.01f, y/density) + offset, new Vector3(0.5f/density, wavefront[x, y]*0.05f, 0.5f/density));
	//			}
	//		}
		}
		
		public float[, ] Wavefront()
		{
			int width = Mathf.CeilToInt(map.dimensions.x*density_);
			int height = Mathf.CeilToInt(map.dimensions.z*density_);
			
			int offX = Mathf.CeilToInt(width / 2f);
			int offY = Mathf.CeilToInt(height / 2f);
			
			float[, ] ret = new float[width, height];
			
			// Draw the obstacles
			foreach (StealthObstacle so in map.GetObstacles()) {
				Shape3 shape = so.GetShape();
				
				foreach(Edge3Abs e in shape) {
					// Bresenham
					int x0 = Mathf.FloorToInt(e.a.x*density_);
					int y0 = Mathf.FloorToInt(e.a.z*density_);
					int x1 = Mathf.FloorToInt(e.b.x*density_);
					int y1 = Mathf.FloorToInt(e.b.z*density_);
					
					int dx = x0 > x1 ? x0 - x1 : x1 - x0;
					int dy = y0 > y1 ? y0 - y1 : y1 - y0;
					int sx, sy;
					if (x0 < x1) sx = 1; else sx = -1;
					if (y0 < y1) sy = 1; else sy = -1;
					int err = dx - dy;
					
					while (true) {
						if (x0 + offX >= 0 && x0 + offX < width && y0 + offY >= 0 && y0 + offY < height)
							ret[x0 + offX, y0 + offY] = 1;
						if (x0 == x1 && y0 == y1) break;
						int e2 = 2*err;
						if (e2 > -dy) {
							err = err - dy;
							x0 = x0 + sx;
						}
						if (e2 < dx) {
							err = err + dx;
							y0 = y0 + sy;
						}
					}
				}
				
				
				// Flood fill
				Stack s = new Stack();
				s.Push(new FillNode(Mathf.FloorToInt(so.position.x*density_), Mathf.FloorToInt(so.position.z*density_), 1));
				int iter = 0;
				while(s.Count > 0) {
					if (++iter > 1000000)
						break;
					FillNode n = (FillNode)s.Pop();
					if (ret[n.x + offX, n.y + offY] == 0) {
						ret[n.x + offX, n.y + offY] = 1;
						if (n.x + offX > 0)
							s.Push(new FillNode(n.x - 1, n.y, 1));
						if (n.x + offX < width - 1)
							s.Push(new FillNode(n.x + 1, n.y, 1));
						if (n.y + offY > 0)
							s.Push(new FillNode(n.x, n.y - 1, 1));
						if (n.y + offY < height - 1)
							s.Push(new FillNode(n.x, n.y + 1, 1));
					}
				}
			}
			
			Queue q = new Queue();
			if (player != null)
				q.Enqueue(new FillNode(Mathf.FloorToInt(player.posX*density_), Mathf.FloorToInt(player.posZ*density_), 2));
			else
				q.Enqueue(new FillNode(Mathf.FloorToInt(0), Mathf.FloorToInt(0), 2));
			
			float sqrt = Mathf.Sqrt(2);
			int iter2 = 0;
			while (q.Count > 0) {
				if (++iter2 > 10000000) {
					break;
				}
				FillNode n = (FillNode)q.Dequeue();
				if (ret[n.x + offX, n.y + offY] == 0 || ret[n.x + offX, n.y + offY] > n.val) {
					ret[n.x + offX, n.y + offY] = n.val;
				
					if (n.x + offX > 0) // East
							q.Enqueue(new FillNode(n.x - 1, n.y, n.val + 1));
					
					if (n.x + offX > 0 && n.y + offY < height - 1) // NE
							q.Enqueue(new FillNode(n.x - 1, n.y + 1, n.val + sqrt));
					
					if (n.x + offX < width - 1) // West
							q.Enqueue(new FillNode(n.x + 1, n.y, n.val + 1));
					
					if (n.x + offX < width - 1 && n.y + offY < height - 1) // NW
							q.Enqueue(new FillNode(n.x + 1, n.y + 1, n.val + sqrt));
					
					if (n.y + offY > 0) // South
							q.Enqueue(new FillNode(n.x, n.y - 1, n.val + 1));
					
					if (n.x + offX > 0 && n.y + offY > 0) // SE
							q.Enqueue(new FillNode(n.x - 1, n.y - 1, n.val + sqrt));
					
					if (n.y + offY < height - 1) // North
							q.Enqueue(new FillNode(n.x, n.y + 1, n.val + 1));
					
					if (n.x + offX < width - 1 && n.y + offY > 0) // SW
							q.Enqueue(new FillNode(n.x + 1, n.y - 1, n.val + sqrt));
				}
				
			}
			
			return ret;
		}
	}
	
	public struct FillNode {
		public int x, y;
		public float val;
		
		public FillNode(int x, int y, float v) {
			this.x = x;
			this.y = y;
			val = v;
		}
	}
}
