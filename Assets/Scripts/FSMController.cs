using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

public class FSMController : MonoBehaviour
{
	public static int timeInterval = 100;
	public static int timeElapse = 0, timeBehind = 0;
	public static List<FSM> FSMList = new List<FSM> ();

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	public static void RunFSM (int timeStamps)
	{
		while (timeBehind < timeStamps) {
			if ((timeBehind + 1) % timeInterval == 0) {
				// if (timeBehind == 0) {
				foreach (FSM currentFSM in FSMList) {
					currentFSM.Run ();	
				}
			}
			timeBehind ++;
		}
		
		// Accomplish the data structure
		foreach (FSM currentFSM in FSMList) {
			foreach (List<Waypoint> lwp in currentFSM.sequence) {
				int index = currentFSM.sequence.IndexOf (lwp);
				if (index != currentFSM.sequence.Count - 1) {
					lwp.Last ().next = currentFSM.sequence.ElementAt (index + 1).ElementAt (0);
				} else {
					// Waiting waypoint
					if (lwp.Count == 1) {
						lwp.Last ().next = currentFSM.sequence.ElementAt (index - 1).Last ();
					} else {
						lwp.Last ().next = lwp.ElementAt (lwp.Count - 2);
					}
				}
			}
		}
		return;
	}
}
