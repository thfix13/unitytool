using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Extra;
using System;
using Common;

namespace Objects {
	public class EnemyGeo : MonoBehaviour {
		
		public Waypoint target;
		public float moveSpeed;
		public float rotationSpeed;
		public float fovAngle = 33;
		public float fovDistance = 5;
		public float radius = 0.5f;
		public float dps = 2;
		public float maxHealth = 100;
		// The first index is always the time span you want to peek
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
		private Vector3 initialPosition;
		private Quaternion initialRotation;
		private Waypoint initialTarget;
		//
		private Vector3 currentPosition;
		private Quaternion currentRotation;
		public Color LineForFOV = new Color (1.0f, 0.3f, 0.0f, 1f);




		public Vector2 getPosition(int t){
			return Vector2.one;
		}
		public Vector2 getForward(int t){
			return Vector2.one;
		}

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
		

	}
}