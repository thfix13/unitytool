using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor; 


[ExecuteInEditMode]
public class CubeVision : MonoBehaviour {

	public List<Vector3> pointsCanSee = new List<Vector3>(); 
	public bool selected = false; 
	public List<GameObject> friends = new List<GameObject>(); 

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
			//Friends links
			foreach(GameObject g in friends)
			{
				Vector3 v = g.transform.position; 
				Debug.DrawLine(gameObject.transform.position,v,Color.red);
			}
		}
		else
		{
			selected = false; 
		}
	}
}
