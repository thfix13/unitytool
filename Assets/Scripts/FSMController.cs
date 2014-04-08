using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

public class FSMController : MonoBehaviour {
	
	public static List<FSM> FSMList = new List<FSM> ();
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public static void RunFSM () {
		foreach (FSM currentFSM in FSMList) {
			currentFSM.Run ();	
		}
	}
}
