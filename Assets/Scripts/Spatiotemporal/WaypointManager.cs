using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Objects;

[ExecuteInEditMode]
public class WaypointManager : MonoBehaviour {
	public Waypoint first = null;
	public Waypoint last = null;
	public int count = 0;
	public WaypointManagerListener listener;
	
	void Awake() {
		name = "Waypoint Manager";
		transform.position = Vector3.zero;
	}
	
	public void SetListener(WaypointManagerListener l) {
		listener = l;
	}
	
	public void ChangeOccured() {
		if (listener != null)
			listener.Notify();
	}
	
	public void Clear() {
		count = 0;
		first = null;
		last = null;
		
		List<Transform> children = new List<Transform>();
		foreach (Transform child in transform) {
			children.Add(child);
		}
		foreach (Transform child in children) {
			GameObject.DestroyImmediate(child.gameObject);
		}
		
		ChangeOccured();
	}
	
	public void AddWaypoint() {
		GameObject go = new GameObject();
		go.AddComponent("Waypoint");
		Waypoint w = go.GetComponent<Waypoint>();
		w.manager = this;
		if (count > 0) {
			go.transform.position = last.transform.position;
			last.next = w;
			last = w;
		} else {
			first = w;
			last = w;
			go.transform.position = transform.position;
		}
		count++;
		Selection.activeObject = go;
		go.name = "Waypoint " + count;
		go.transform.parent = transform;
	}
	
	public void AddWaitingWaypoint() {
		GameObject go = new GameObject();
		go.AddComponent("WaitingWaypoint");
		WaitingWaypoint w = go.GetComponent<WaitingWaypoint>();
		w.manager = this;
		if (count > 0) {
			go.transform.position = last.transform.position;
			last.next = w;
			last = w;
		} else {
			first = w;
			last = w;
			go.transform.position = transform.position;
		}
		count++;
		Selection.activeObject = go;
		go.name = "Waypoint " + count + " (waiting)";
		go.transform.parent = transform;
	}
	
	public void AddRotationWaypoint() {
		GameObject go = new GameObject();
		go.AddComponent("RotationWaypoint");
		Waypoint w = go.GetComponent<Waypoint>();
		w.manager = this;
		if (count > 0) {
			if (last != null) {
				go.transform.position = last.transform.position;
				last.next = w;
				last = w;
			} else {
				last = first;
				while (last.next != null) {
					last = last.next;
				}
				go.transform.position = last.transform.position;
				last.next = w;
				last = w;
			}
		} else {
			first = w;
			last = w;
			go.transform.position = transform.position;
		}
		count++;
		Selection.activeObject = go;
		go.name = "Waypoint " + count + " (rotation)";
		go.transform.parent = transform;
	}
}

public interface WaypointManagerListener {
	void Notify();
}