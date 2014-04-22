using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Generator : MonoBehaviour
{
	public static string behaviourSequence = "dld";
	public List<Waypoint> sequence = new List<Waypoint> ();

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	public static void GenerateBehaviourSequence (int iterations)
	{
		string tempString = String.Copy (behaviourSequence);

		for (int iter = 0; iter < iterations; iter++) {
			List<char> charList = tempString.ToCharArray ().ToList ();
			for (int pos = 0; pos < tempString.Length; pos++) {
				if (charList.ElementAt (pos) == 'd') {
					int r = UnityEngine.Random.Range (0, 3);
					if (r == 0) {
						charList.RemoveAt (pos);
						charList.Insert (pos, 'p');
					} else if (r == 1) {
						charList.RemoveAt (pos);
						charList.Insert (pos, 'f');
					} else if (r == 2) {
						charList.RemoveAt (pos);
						charList.Insert (pos, 'r');
					} 
				} else if (charList.ElementAt (pos) == 'l') {
					int r = UnityEngine.Random.Range (0, 2);
					if (r == 0) {
						charList.Insert (pos + 1, 'd');
						charList.Insert (pos + 2, 'l');
					} else if (r == 1) {
						charList.RemoveAt (pos);
						charList.Insert (pos, 'z');
						charList.Insert (pos + 1, 'd');
						charList.Insert (pos + 2, 'z');
						charList.Insert (pos + 3, 'd');
						charList.Insert (pos + 4, 'z');
					}
				} else if (charList.ElementAt (pos) == 'z') {
					int r = UnityEngine.Random.Range (0, 2);
					if (r == 0) {
						charList.RemoveAt (pos);
						charList.Insert (pos, 'l');
						charList.Insert (pos + 1, 'd');
						charList.Insert (pos + 2, 'l');
					} else if (r == 1) {
						charList.Insert (pos + 1, 'd');
						charList.Insert (pos + 2, 'z');
						charList.Insert (pos + 3, 'd');
						charList.Insert (pos + 4, 'z');
					}
				}
			}
			tempString = new string (charList.ToArray ());
		}
		behaviourSequence = tempString;
		Debug.Log (behaviourSequence);
	}
	
	public static void Parse ()
	{
//		char[] charArray = behaviourSequence.ToCharArray ();
//		for (int i = 0; i < charArray.Length; i++) {
//			if (charArray [i] == 'd') {
//				GameObject wp = GameObject.Instantiate (waypointPrefab, endVec, Quaternion.identity) as GameObject;
//				Waypoint wpScript;
//				wpScript = wp.GetComponent ("Waypoint") as Waypoint;
//				sequence.Add (wpScript);
//			} else if (charArray [i] == 'p') {
//				GameObject rwp = GameObject.Instantiate (waypointPrefab, startVec, Quaternion.identity) as GameObject;
//				rwp.AddComponent ("RotationWaypoint");
//				DestroyImmediate (rwp.GetComponent ("Waypoint"));
//				RotationWaypoint rwpScript;
//				rwpScript = rwp.GetComponent ("Waypoint") as RotationWaypoint;
//				rwpScript.lookDir = new Vector3 (0.0f, 0.0f, 0.0f);
//			} else if (charArray [i] == 'f') {
//				GameObject rwp = GameObject.Instantiate (waypointPrefab, startVec, Quaternion.identity) as GameObject;
//				rwp.AddComponent ("RotationWaypoint");
//				DestroyImmediate (rwp.GetComponent ("Waypoint"));
//				RotationWaypoint rwpScript;
//				rwpScript = rwp.GetComponent ("Waypoint") as RotationWaypoint;				
//			} else if (charArray [i] == 'r') {
//				GameObject rwp = GameObject.Instantiate (waypointPrefab, startVec, Quaternion.identity) as GameObject;
//				rwp.AddComponent ("RotationWaypoint");
//				DestroyImmediate (rwp.GetComponent ("Waypoint"));
//				RotationWaypoint rwpScript;
//				rwpScript = rwp.GetComponent ("Waypoint") as RotationWaypoint;				
//			}
//		}
	}
}
