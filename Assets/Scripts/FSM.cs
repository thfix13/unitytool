using UnityEngine;
using System.Collections;

public class FSM : MonoBehaviour
{
	
	public enum States
	{
		PAUSE = 1,
		MOVE
	};
	
	public States currentState;
	public float timeElapse = 0.0f;
	
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
		timeElapse += 1 / 400f;
		if (timeElapse > 5.0f) {
			timeElapse = 0.0f;
			//Debug.Log ("Called every 5 secs");
			int index = UnityEngine.Random.Range (1, System.Enum.GetValues (typeof(States)).Length);
			if (index == (int)States.PAUSE) {
				
			} else if (index == (int)States.MOVE) {
				
			} else {
				
			} 
		}
	}
}
