
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

[ExecuteInEditMode]
public class ShapeLoader : MonoBehaviour, Obstacle {
	public bool dirty = false;
	private Shape3 shape = new Shape3();
	public TextAsset text_;
	public TextAsset text {
		get {
			return text_;
		}
		set {
			if (text_ != value) {
				dirty = true;
				text_ = value;
			}
		}
	}
	
	public void Start() {
		dirty = true;
	}
	
	public void OnDrawGizmos() {
		foreach (Edge3Abs e in shape) {
			Gizmos.DrawLine(e.a, e.b);
		}
		
		foreach(Edge3Abs e in ShadowPolygon(transform.position, 100)) {
			Gizmos.DrawLine(e.a, e.b);
		}
	}
	
	public void Update() {
		if (dirty) {
			shape = read();
			dirty = false;
		}
	}
	
	public Shape3 read() {
		Shape3 ret = new Shape3();
		
		string[] lines = text_.text.Split('\n');
		
		bool first = true;
		foreach (string line in lines) {
			if (first) {
				first = false;
				continue;
			}
			string[] elm = line.Split('\t');
			if (elm.Length < 4)
				continue;
			float x = float.Parse(elm[1].Split(' ')[0]);
			float y = float.Parse(elm[2].Split(' ')[0]);
			float z = float.Parse(elm[3].Split(' ')[0]);
			
			ret.addVertex(new Vector3(x, y, z));
		}
		
		return ret;
	}
	
	public Shape3 ShadowPolygon(Vector3 viewpoint, float viewDistance)
	{
		Shape3 obsShape = shape;
		
		List<pair> polar = new List<pair>();
		
		int i = 0;
		foreach(Edge3Abs e in obsShape) {
			Vector3 v = e.a - viewpoint;
			float angle = Mathf.Atan2(v.z, v.x);
			polar.Add(new pair(angle, v.magnitude, i++));
		}
		
		polar.Sort(delegate(pair p1, pair p2) {
			if (p1.angle - p2.angle != 0)
				return p1.angle.CompareTo(p2.angle);
			return p1.distance.CompareTo(p2.distance);
		});
		
		
		return new Shape3();
	}
}

struct pair {
	public float angle;
	public float distance;
	public int index;
	
	public pair(float angle, float distance, int index) {
		this.angle = angle;
		this.distance = distance;
		this.index = index;
	}
}