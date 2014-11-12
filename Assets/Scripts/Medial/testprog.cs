using UnityEngine;
using System.Collections;
//using System;
//using System.Linq;
//using System.Collections.Generic;
using System.Diagnostics;
public class testprog : MonoBehaviour {

	// Use this for initialization
	void Start () {
		ProcessStartInfo startInfo = new ProcessStartInfo()
		{
			FileName = "/Users/dhsingh/Documents/Thesis/SM03Skeleton/run.sh",
			Arguments = "cube2_multi.ply2",
		};
		Process proc = new Process()
		{
			StartInfo = startInfo,
		};
		proc.Start();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
