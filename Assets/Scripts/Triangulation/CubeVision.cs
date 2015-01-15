using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor; 


[ExecuteInEditMode]
public class CubeVision : MonoBehaviour {

	public List<Vector3> pointsCanSee = new List<Vector3>(); 
	public bool selected = false; 
	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{

		if(Selection.activeGameObject == gameObject)
		{
			foreach(Vector3 v in pointsCanSee)
			{
				Debug.DrawLine(gameObject.transform.position,v,Color.blue);
			}
		}
		else
		{
			selected = false; 
		}
	}
}
