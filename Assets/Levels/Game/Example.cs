using UnityEngine;
using System.Collections;

public class Example : MonoBehaviour
{
	public int num;
	void OnGUI() {
		if(GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 300, 500),"Quick Play"))
		{
			Application.LoadLevel (2);
			Debug.Log ("Clicked the button Quick play which gets you to level 1");
		}
		if (GUI.Button (new Rect (10, 10, 10, 10), "Start Level")) {
			Application.LoadLevel(num);		
		}
		if (GUI.Button (new Rect (10, 10, 50, 50), "Levels")) 
		{
			Application.LoadLevel(1);
			Debug.Log("Clicked on the Levels button");
		}		
	}
}