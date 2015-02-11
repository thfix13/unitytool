using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Extra;
using System;
using Common;

namespace Objects {
	public class EnemyGeo : MonoBehaviour {
		
		//public WaypointGeo target;
		//public float moveSpeed;
		//public float rotationSpeed;
		public float fovAngle = 33;
		public float fovDistance = 5;
		public float radius = 0.5f;
		//public float dps = 2;
		//public float maxHealth = 100;
		// The first index is always the time span you want to peek

		private int playingTime = 0;

		/*
		[HideInInspector]
		public Vector3[] positions;
		[HideInInspector]
		public Vector3[] forwards;
		[HideInInspector]
		public Quaternion[] rotations;
		//
		private Waypoint dummyTarget;
		private Vector3 dummyPosition;
		private Quaternion dummyRotation;
		//
		*/
		public Vector3 initialPosition;
		public Vector3 initialForward;
		//private Quaternion initialRotation;
		public WaypointGeo initialTarget;
		//
		//private Vector3 currentPosition;
		//private Quaternion currentRotation;
		public Color LineForFOV = new Color (1.0f, 0.3f, 0.0f, 1f);

		public void setInitialOrientationToCurrent(){
			initialPosition = transform.position;
			initialForward = transform.forward;
		}


		public Vector3 getPosition(int t){
			computeAtTime(t);

			return posT;
		}
		public Vector3 getForward(int t){
			computeAtTime(t);

			return forT;
		}

		private int curT;
		private Vector3 posT;
		private Vector3 forT;

		int timeLeft;
		WaypointGeo nextWay;
		Vector3 pos;
		Vector3 forw;
		
		
		private void computeAtTime(int t){
			if(t != curT){
				curT = t;
				timeLeft = t;
				nextWay = initialTarget;
				pos = initialPosition;
				forw = initialForward;

				while(timeLeft > 0){

					switch (nextWay.type)
					{
					case "wait" :
						timeLeft = timeLeft - nextWay.waitTime;
						break;
					case "move" :
						tryMove(nextWay);
						break;
					case "rot" :
						tryRot(nextWay);
						break;
					default:
						Debug.LogError("UNIDENTIFIED WAYPOINT FOUND, EVERTHING WILL NOW BREAK");
						break;
					}

					nextWay = nextWay.next;
				}
					 

			}
		}

		private void tryMove(WaypointGeo trgt){
			float distance = Vector3.Distance(pos, trgt.transform.position);
			int steps = Mathf.CeilToInt(distance / trgt.movSpeed);
			if(steps > timeLeft){
				pos = Vector3.Lerp(pos, trgt.transform.position, ((float)steps) / ((float)timeLeft));
			}
			else{
				timeLeft = timeLeft - steps;
				pos = trgt.transform.position;
			}
		}

		private void tryRot(WaypointGeo trgt){
					float angleDif = Vector3.Angle(forw, trgt.transform.forward);
					int steps = Mathf.CeilToInt(angleDif / trgt.rotSpeed);
					if(steps > timeLeft){
						Vector3.Slerp(forw, trgt.transform.forward, ((float)steps) / ((float)timeLeft));
					}
					else{
						timeLeft = timeLeft - steps;
						forw = trgt.transform.forward;
					}
		}
		public void goToFrame(int t){
			Debug.Log ("GO TO FRAME");
			transform.position = getPosition(t);
			Debug.Log ("POSITION SET");
			transform.forward = getForward(t);
			Debug.Log ("FORWARD SET");

		}

		public void Start(){
			playingTime = 0;
			tim= 0;
		}

		private int tim = 0;
		public void Update(){
			tim++;
			if(tim == 5){
				//tim = 0;
				Debug.Log (playingTime++);
				//goToFrame(playingTime);
				//getPosition(playingTime);
				Debug.Log ("...?");
			}
		}


/*
		// This moves the enemy in the game running environment
		void Update () {
			Vector3 outPos;
			Quaternion outRot;
			Waypoint outWay;
			
			EnemyMover.Solve (gameObject.GetHashCode (), transform.position, transform.rotation, moveSpeed, rotationSpeed, Time.deltaTime, target, 0.25f, out outPos, out outRot, out outWay);
			
			transform.position = outPos;
			transform.rotation = outRot;
			target = outWay;
			
			currentPosition = transform.position;
			currentRotation = transform.rotation;
		}
		
		// Reset back the dummy and actual gameobject back to the initial position
		public void ResetSimulation () {
			transform.position = initialPosition;
			transform.rotation = initialRotation;
			target = initialTarget;
			
			dummyPosition = transform.position;
			dummyRotation = transform.rotation;
			dummyTarget = target;
		}
		
		// Sets the initial position with the current transform coordinates
		public void SetInitialPosition () {
			initialTarget = target;
			initialPosition = transform.position;
			initialRotation = transform.rotation;
			
			ResetSimulation ();
		}
		
		// This siumulates the enemy's movement based on the actual enemy movement
		public void Simulate (float time) {
			Vector3 outPos;
			Quaternion outRot;
			Waypoint outWay;
			
			EnemyMover.Solve (gameObject.GetHashCode (), dummyPosition, dummyRotation, moveSpeed, rotationSpeed, time, dummyTarget, 0.25f, out outPos, out outRot, out outWay);
			
			dummyPosition = outPos;
			dummyRotation = outRot;
			dummyTarget = outWay;
			
			currentPosition = dummyPosition;
			currentRotation = dummyRotation;
		}
		
		public void OnDrawGizmos () {
			return; 
			
			
		}
		
		public Vector3 GetSimulationPosition () {
			return currentPosition;
		}
		
		public Vector3 GetSimulatedForward () {
			return currentRotation * Vector3.forward;
		}
		
		public Quaternion GetSimulatedRotation () {
			return currentRotation;
		}

*/

	}
}