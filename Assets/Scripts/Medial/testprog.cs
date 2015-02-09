using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//using System;
//using System.Linq;
//using System.Collections.Generic;
using System.Diagnostics;
public class testprog : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Vector3 one= new Vector3(0f,0f,0f),two=new Vector3(1f,0f,0f), three= new Vector3(1f,0f,1f), four= new Vector3(0f,0f,1f);
		Vector3 one1= new Vector3(0.5f,1f,0f),two1=new Vector3(1.5f,1f,0f), three1= new Vector3(1.5f,1f,1f), four1= new Vector3(0.5f,1f,1f);
		List<string> input = new List<string>{ "Brachiosaurus", 
			"Amargasaurus", 
			"Mamenchisaurus","asdasd","oinonasd" };
		List<string> output=input.GetRange(1,2);


		//writePLY(new List<Vector3>{one,two,three,four},new List<Vector3>{one1,two1,three1,four1},new List<List<int>>{new List<int>{0,1,2,3}});
	}
	public static void udl(object s){
		UnityEngine.Debug.Log(s);
	}
	void writePLY(List<Vector3> vertices_orig, List<Vector3>vertices_final, List<List<int>> polygons){
		
		foreach(var polygon in polygons){
			int i=0,j=0, n=polygon.Count;
			//ply_triangles.Add("3 "+);
			for(i=0; i<n;i++){
				String s= "3 "+i+" "+((i+1)%n)+" "+((i%n)+n);
				String t= "3 "+((i%n)+n)+" "+((i+1)%n)+" "+(((i+1)%n)+n);
				udl (s);
				udl (t);
			}
		}
		
	}
	// Update is called once per frame
	void Update () {
	
	}
}
