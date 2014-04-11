using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

public class FSM : MonoBehaviour
{
	public int startIndex, endIndex, currentTick = 0;
	public Vector3 position;
	public List<List<Waypoint>> sequence = new List<List<Waypoint>> ();
	public GameObject enemyPrefab = null, waypointPrefab = null, floor = null;
	public Skeletonization sBoundary = null;
	
	public enum States
	{
		PAUSE = 1,
		MOVE, 
		ROTATE
	};
	
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
	
	public void Run ()
	{
		int index = UnityEngine.Random.Range (1, System.Enum.GetValues (typeof(States)).Length + 1);
		Debug.Log (index);
		// Switch between different status
		
		// Pause
		if (index == (int)States.PAUSE) {
			// Waiting waypoint
			GameObject wwp = GameObject.Instantiate (waypointPrefab, position, Quaternion.identity) as GameObject;
			wwp.AddComponent ("WaitingWaypoint");
			DestroyImmediate (wwp.GetComponent ("Waypoint"));
			WaitingWaypoint wwpScript;
			wwpScript = wwp.GetComponent ("Waypoint") as WaitingWaypoint;
			wwpScript.waitingTime = FSMController.timeInterval * 1 / 10f;
			List<Waypoint> newList = new List<Waypoint> ();
			newList.Add (wwpScript);
			sequence.Add (newList);
		} 
		
		// Move
		else if (index == (int)States.MOVE) {
			// Initial the head of list for each interval
			Vector3 endVec = sBoundary.finalGraphNodesList.ElementAt (endIndex).Pos (floor);
			GameObject wp = GameObject.Instantiate (waypointPrefab, endVec, Quaternion.identity) as GameObject;
			Waypoint wpScript;
			wpScript = wp.GetComponent ("Waypoint") as Waypoint;
			List<Waypoint> newList = new List<Waypoint> ();
			newList.Add (wpScript);
			sequence.Add (newList);
			
			// Give predictions in current timeInterval
			// Total length the guard could travel
			float totalDist = this.gameObject.GetComponent <Enemy> ().moveSpeed * FSMController.timeInterval * 1 / 10f;
			while (totalDist >= Mathf.Epsilon) {
				// Deduct next section's length
				totalDist -= Vector3.Distance (position, endVec);
				if (totalDist >= Mathf.Epsilon) {
					int neighborIndex = UnityEngine.Random.Range (0, sBoundary.finalGraphNodesList.ElementAt (endIndex).neighbors.Count);
					int tempIndex = sBoundary.finalGraphNodesList.IndexOf (sBoundary.finalGraphNodesList.ElementAt (endIndex).neighbors.ElementAt (neighborIndex));
					startIndex = endIndex;
					position = endVec;
					endVec = sBoundary.finalGraphNodesList.ElementAt (tempIndex).Pos (floor);
					endIndex = tempIndex;
					GameObject twp = GameObject.Instantiate (waypointPrefab, endVec, Quaternion.identity) as GameObject;
					Waypoint twpScript;
					twpScript = twp.GetComponent ("Waypoint") as Waypoint;
					newList.Last ().next = twpScript;
					newList.Add (twpScript);
				} else {
					Vector3 dir = endVec - position;
					float ratio = (Vector3.Distance (position, endVec) + totalDist) / Vector3.Distance (position, endVec);
					position += new Vector3 (dir.x * ratio, dir.y * ratio, dir.z * ratio);
				}
			}
		} else {
			Vector3 startVec = sBoundary.finalGraphNodesList.ElementAt (startIndex).Pos (floor);
			Vector3 endVec = sBoundary.finalGraphNodesList.ElementAt (endIndex).Pos (floor);
			Vector3 dir = endVec - startVec;
			Vector3 n1 = Vector3.Cross (dir, new Vector3 (0.0f, 1.0f, 0.0f));
			Vector3 n2 = Vector3.Cross (new Vector3 (0.0f, 1.0f, 0.0f), dir);
			
			GameObject rwp1 = GameObject.Instantiate (waypointPrefab, startVec, Quaternion.identity) as GameObject;
			GameObject rwp2 = GameObject.Instantiate (waypointPrefab, endVec, Quaternion.identity) as GameObject;
			GameObject rwp3 = GameObject.Instantiate (waypointPrefab, n1, Quaternion.identity) as GameObject;
			GameObject rwp4 = GameObject.Instantiate (waypointPrefab, n2, Quaternion.identity) as GameObject;
			rwp1.AddComponent ("RotationWaypoint");
			rwp2.AddComponent ("RotationWaypoint");
			rwp3.AddComponent ("RotationWaypoint");
			rwp4.AddComponent ("RotationWaypoint");
			DestroyImmediate (rwp1.GetComponent ("Waypoint"));
			DestroyImmediate (rwp2.GetComponent ("Waypoint"));
			DestroyImmediate (rwp3.GetComponent ("Waypoint"));
			DestroyImmediate (rwp4.GetComponent ("Waypoint"));
			RotationWaypoint rwpScript1;
			RotationWaypoint rwpScript2;
			RotationWaypoint rwpScript3;
			RotationWaypoint rwpScript4;
			rwpScript1 = rwp1.GetComponent ("Waypoint") as RotationWaypoint;
			rwpScript2 = rwp2.GetComponent ("Waypoint") as RotationWaypoint;
			rwpScript3 = rwp3.GetComponent ("Waypoint") as RotationWaypoint;
			rwpScript4 = rwp4.GetComponent ("Waypoint") as RotationWaypoint;
			rwpScript1.next = rwpScript3;
			rwpScript3.next = rwpScript2;
			rwpScript2.next = rwpScript4;
			rwpScript4.next = rwpScript1;
			
			rwpScript1.lookDir = startVec;
			rwpScript2.lookDir = endVec;
			rwpScript3.lookDir = n1;
			rwpScript4.lookDir = n2;
			
			List<Waypoint> newList = new List<Waypoint> ();
			newList.Add (rwpScript1);
			newList.Add (rwpScript2);
			newList.Add (rwpScript3);
			newList.Add (rwpScript4);
			sequence.Add (newList);
		} 
	}
}
