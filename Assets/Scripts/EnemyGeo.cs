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

		public Vector3 getPositionDist(int t, int t2){
			if(t2 > t){
				computeAtTime(t);
			}
			else{
				computeAtTimeDist(t, t2);
			}

			return posT;
		}
		public Vector3 getForwardDist(int t, int t2){
			if(t2 > t){
				computeAtTime(t);
			}
			else{
				computeAtTimeDist(t, t2);
			}
			
			return forT;
		}


		public Vector3 getPositionDists(int t, List<int> ts){
			int t2 = ts[0];
			if(t2 > t){
				computeAtTime(t);
			}
			else{
				computeAtTimesDists(t, ts);
			}
			
			return posT;
		}
		public Vector3 getForwardDists(int t, List<int> ts){
			int t2 = ts[0];
			if(t2 > t){
				computeAtTime(t);
			}
			else{
				computeAtTimesDists(t, ts);
			}
			
			return forT;
		}

		public Vector3 getPositionDistsN(int t, List<int> ts, List<int> ns){
			int t2 = ts[0];
			if(t2 > t){
				computeAtTime(t);
			}
			else{
				computeAtTimesDistsN(t, ts, ns);
			}
			
			return posT;
		}
		public Vector3 getForwardDistsN(int t, List<int> ts, List<int> ns){
			int t2 = ts[0];
			if(t2 > t){
				computeAtTime(t);
			}
			else{
				computeAtTimesDistsN(t, ts, ns);
			}
			
			return forT;
		}

		private void computeAtTimesDists(int t, List<int> ts){
			if(t == curT){
				return;
			}
			int ti = ts[0];
			computeAtTime(ti);
			int tj = ts[ts.Count-1];
			for(int j = 1; j < ts.Count; j++){
				tj = ts[j];
				if(tj != curT){
					curT = tj;
					timeLeft = tj-ti;
					WaypointGeo distWay = nextWay.distractPoint;
					nextWay = distWay.next;
					tryRotDist(distWay);
					if(timeLeft <= 0 ){
						continue;
					}
					tryMove(distWay);
					if(timeLeft <= 0){
						continue;
					}
					while(timeLeft > 0){
						switch (nextWay.type)
						{
						case "wait" :
							//Debug.Log ("wait");
							timeLeft = timeLeft - nextWay.waitTime;
							break;
						case "move" :
							//Debug.Log ("move");
							tryMove(nextWay);
							break;
						case "rot" :
							//Debug.Log ("rot");
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

			if(t != curT){
				curT = t;
				timeLeft = t-tj;
				WaypointGeo distWay = nextWay.distractPoint;
				tryRotDist(distWay);
				if( timeLeft <= 0){
					
					return;
				}
				tryMove(distWay);
				if(timeLeft <= 0){
					
					
					return;
				}
				
				
				nextWay = distWay.next;

				while(timeLeft > 0){
					switch (nextWay.type)
					{
					case "wait" :
						//Debug.Log ("wait");
						timeLeft = timeLeft - nextWay.waitTime;
						break;
					case "move" :
						//Debug.Log ("move");
						tryMove(nextWay);
						break;
					case "rot" :
						//Debug.Log ("rot");
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

		private void computeAtTimesDistsN(int t, List<int> ts, List<int> ns){
			if(t == curT){
				return;
			}
			int ti = ts[0];
			int ni = ns[0];
			computeAtTime(ti);
			int tj = ts[ts.Count-1];
			int nj = ns[ns.Count-1];
			for(int j = 1; j < ts.Count; j++){
				tj = ts[j];
				nj = ns[j];
				if(tj != curT){
					curT = tj;
					timeLeft = tj-ti;
					WaypointGeo distWay = nextWay.distractPoints[ni];
					ni = nj;
					nextWay = distWay.next;
					tryRotDist(distWay);
					if(timeLeft <= 0 ){
						continue;
					}
					tryMove(distWay);
					if(timeLeft <= 0){
						continue;
					}
					while(timeLeft > 0){
						switch (nextWay.type)
						{
						case "wait" :
							//Debug.Log ("wait");
							timeLeft = timeLeft - nextWay.waitTime;
							break;
						case "move" :
							//Debug.Log ("move");
							tryMove(nextWay);
							break;
						case "rot" :
							//Debug.Log ("rot");
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
			
			if(t != curT){
				curT = t;
				timeLeft = t-tj;
				WaypointGeo distWay = nextWay.distractPoints[nj];
				tryRotDist(distWay);
				if( timeLeft <= 0){
					
					return;
				}
				tryMove(distWay);
				if(timeLeft <= 0){
					
					
					return;
				}
				
				
				nextWay = distWay.next;
				
				while(timeLeft > 0){
					switch (nextWay.type)
					{
					case "wait" :
						//Debug.Log ("wait");
						timeLeft = timeLeft - nextWay.waitTime;
						break;
					case "move" :
						//Debug.Log ("move");
						tryMove(nextWay);
						break;
					case "rot" :
						//Debug.Log ("rot");
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


		private void computeAtTimeDist(int t, int t2){
			computeAtTime(t2);
			if(t != curT){
				curT = t;
				timeLeft = t-t2;
				WaypointGeo distWay = nextWay.distractPoint;
				tryRotDist(distWay);
				if( timeLeft <= 0){

					return;
				}
				tryMove(distWay);
				if(timeLeft <= 0){

		
					return;
				}
		

				nextWay = distWay.next;

				//int bob = 100;
				//Debug.Log (timeLeft);
				while(timeLeft > 0){// && bob > 0){
					//bob--;
					//Debug.Log ("bob");
					//Debug.Log (bob);
					//Debug.Log ("end bob");
					switch (nextWay.type)
					{
					case "wait" :
						//Debug.Log ("wait");
						timeLeft = timeLeft - nextWay.waitTime;
						break;
					case "move" :
						//Debug.Log ("move");
						tryMove(nextWay);
						break;
					case "rot" :
						//Debug.Log ("rot");
						tryRot(nextWay);
						break;
					default:
						Debug.LogError("UNIDENTIFIED WAYPOINT FOUND, EVERTHING WILL NOW BREAK");
						break;
					}
					
					nextWay = nextWay.next;
				}
				
				
				//if(bob == 0){
				//	Debug.Log (timeLeft);
				//}
				
			}

		}

		private void computeAtTimeDistN(int t, int t2, int n){
			
			computeAtTime(t2);
			if(t != curT){
				curT = t;
				timeLeft = t-t2;
				WaypointGeo distWay = nextWay.distractPoints[n];
				tryRotDist(distWay);
				if( timeLeft <= 0){
					
					return;
				}
				tryMove(distWay);
				if(timeLeft <= 0){
					
					
					return;
				}
				
				
				nextWay = distWay.next;
				
				//int bob = 100;
				//Debug.Log (timeLeft);
				while(timeLeft > 0){// && bob > 0){
					//bob--;
					//Debug.Log ("bob");
					//Debug.Log (bob);
					//Debug.Log ("end bob");
					switch (nextWay.type)
					{
					case "wait" :
						//Debug.Log ("wait");
						timeLeft = timeLeft - nextWay.waitTime;
						break;
					case "move" :
						//Debug.Log ("move");
						tryMove(nextWay);
						break;
					case "rot" :
						//Debug.Log ("rot");
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


		private void tryRotDist(WaypointGeo dist){
			Vector3 trgtAngle = dist.gameObject.transform.position - posT;
			trgtAngle.Normalize();


			float angleDif = Vector3.Angle(forT, trgtAngle);
			int steps = Mathf.CeilToInt(angleDif / dist.rotSpeed);
			if(steps > timeLeft){
				forT = Vector3.Slerp(forT, trgtAngle, ((float)timeLeft) / ((float)steps));
				timeLeft = 0;
			}
			else{
				timeLeft = timeLeft - steps;
				forT = trgtAngle;
			}
		}
				
		public Vector3 getPosition(int t){
			computeAtTime(t);

			return posT;
		}
		public Vector3 getForward(int t){
			computeAtTime(t);

			return forT;
		}

		private int curT = 0;
		private Vector3 posT = Vector3.zero;
		private Vector3 forT = Vector3.zero;

		int timeLeft = 0;
		WaypointGeo nextWay;

		private void computeAtTime(int t){
			if(t != curT){
				curT = t;
				timeLeft = t;
				nextWay = initialTarget;
				posT = initialPosition;
				forT = initialForward;

				//int bob = 100;
				//Debug.Log (timeLeft);
				while(timeLeft > 0){// && bob > 0){
					//bob--;
					//Debug.Log ("bob");
					//Debug.Log (bob);
					//Debug.Log ("end bob");
					switch (nextWay.type)
					{
					case "wait" :
						//Debug.Log ("wait");
						timeLeft = timeLeft - nextWay.waitTime;
						break;
					case "move" :
						//Debug.Log ("move");
						tryMove(nextWay);
						break;
					case "rot" :
						//Debug.Log ("rot");
						tryRot(nextWay);
						break;
					default:
						Debug.LogError("UNIDENTIFIED WAYPOINT FOUND, EVERTHING WILL NOW BREAK");
						break;
					}

					nextWay = nextWay.next;
				}


				//if(bob == 0){
				//	Debug.Log (timeLeft);
				//}

			}
		}

		private void tryMove(WaypointGeo trgt){
			//Debug.Log (posT);
			//Debug.Log (trgt.transform.position);

			float distance = Vector3.Distance(posT, trgt.transform.position);
			//Debug.Log(distance);
			int steps = Mathf.CeilToInt(distance / trgt.movSpeed);
			//Debug.Log (steps);
			//Debug.Log (((float)steps) / ((float)timeLeft));
			if(steps > timeLeft){
				posT = Vector3.Lerp(posT, trgt.transform.position, ((float)timeLeft) / ((float)steps));
				//Debug.Log (posT);
				timeLeft = 0;
			}
			else{
				timeLeft = timeLeft - steps;
				posT = trgt.transform.position;
			}
		}

		private void tryRot(WaypointGeo trgt){
					float angleDif = Vector3.Angle(forT, trgt.transform.forward);
					int steps = Mathf.CeilToInt(angleDif / trgt.rotSpeed);
					if(steps > timeLeft){
						forT = Vector3.Slerp(forT, trgt.transform.forward, ((float)timeLeft) / ((float)steps));
						timeLeft = 0;
					}
					else{
						timeLeft = timeLeft - steps;
						forT = trgt.transform.forward;
					}
		}
		public void goToFrame(int t){
			Vector3 tmp = getPosition(t);
			//Debug.Log ("At time " + t + " the position was " + tmp);
			transform.position = tmp;

			Vector3 tmp2 = getForward(t);
			transform.forward = tmp2;

		}

		public void goToFrameDist(int t, int t2){
			Vector3 tmp = getPositionDist(t, t2);
			transform.position = tmp;
			
			Vector3 tmp2 = getForwardDist(t, t2);
			transform.forward = tmp2;
		}

		public void goToFrameDists(int t, List<int> ts){
			Vector3 tmp = getPositionDists(t, ts);
			transform.position = tmp;

			Vector3 tmp2 = getForwardDists(t, ts);
			transform.forward = tmp2;
		}

		public void goToFrameDistsN(int t, List<int> ts, List<int> ns){
			Vector3 tmp = getPositionDistsN(t, ts, ns);
			transform.position = tmp;
			
			Vector3 tmp2 = getForwardDistsN(t, ts, ns);
			transform.forward = tmp2;
		}


		public void Start(){
			playingTime = 0;
			//tim= 0;
			setInitialOrientationToCurrent();
		}

		//private int tim = 5;
		public void Update(){
			//tim++;
			//if(tim == 5){
				//tim = 0;

				//Debug.Log (playingTime++);
				playingTime++;
				goToFrame(playingTime);
				//getPosition(playingTime);
				//computeAtTime(1);
				//posT = initialPosition;
				//timeLeft = 15;
				//tryMove(initialTarget);
				//Debug.Log ("...?");
			//}
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