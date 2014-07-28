using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Enemy2 : MonoBehaviour
{
	public class MoveSpeed
	{
		public float moveSpeed = 0.0f;
		public GameObject referredWp;
	};
	
	public class RotationSpeed
	{
		public float rotationSpeed = 0.0f;
		public GameObject referredWp;
	};
	
	public Waypoint target;
	public float fovAngle = 33;
	public float fovDistance = 5;
	public float radius = 0.5f;

	// The changeable movement speed and rotation speed
	public MoveSpeed currentMoveSpeed;
	public RotationSpeed currentRotationSpeed;
	public MoveSpeed[] moveSpeeds;
	public RotationSpeed[] rotationSpeeds;
	public Dictionary<int, MoveSpeed> moveSpeedDictionary = new Dictionary<int, MoveSpeed> ();
	public Dictionary<GameObject, int> reverseMoveSpeedDictionary = new Dictionary<GameObject, int> ();
	public Dictionary<int, RotationSpeed> rotationSpeedDictionary = new Dictionary<int, RotationSpeed> ();
	public Dictionary<GameObject, int> reverseRotationSpeedDictionary = new Dictionary<GameObject, int> ();

	// The first index is always the time span you want to peek
	public Vector3[] positions;
	public Vector3[] forwards;
	public Quaternion[] rotations;
	public Vector2[][] cells; // The second index goes from 0 to the amount of seen cells in that time span
	//
	private Waypoint dummyTarget;
	private Vector3 dummyPosition;
	private Quaternion dummyRotation;
	private Waypoint dummyTargets;
	private Vector3[] dummyPositions;
	private Quaternion dummyRotations;
	//
	private MoveSpeed initialMoveSpeed;
	private RotationSpeed initialRotationSpeed;
	private Vector3 initialPosition;
	private Quaternion initialRotation;
	private Waypoint initialTarget;
	public Color LineForFOV = new Color (1.0f, 0.3f, 0.0f, 1f);
	
	// This moves the enemy in the game running environment
	// Obsolete
	void Update ()
	{
		//return; 
		Vector3 outPos;
		Quaternion outRot;
		Waypoint outWay;
		
		EnemyMover.Solve (gameObject.GetHashCode (), dummyPosition, dummyRotation, currentMoveSpeed.moveSpeed, currentRotationSpeed.rotationSpeed, Time.deltaTime, dummyTarget, 0.05f, out outPos, out outRot, out outWay);
		
		transform.position = outPos;
		transform.rotation = outRot;
		target = outWay;

		currentMoveSpeed = GetNextMoveSpeed (0);
		currentRotationSpeed = GetNextRotationSpeed (0);
	}
	
	// Reset back the dummy and actual gameobject back to the initial position
	public void ResetSimulation ()
	{
		transform.position = initialPosition;
		transform.rotation = initialRotation;
		target = initialTarget;
//		initialMoveSpeed = new MoveSpeed ();
//		initialRotationSpeed = new RotationSpeed ();

		dummyPosition = transform.position;
		dummyRotation = transform.rotation;
		dummyTarget = target;
		currentMoveSpeed = initialMoveSpeed;
		currentRotationSpeed = initialRotationSpeed;
	}
	
	// Sets the initial position with the current transform coordinates
	public void SetInitialPosition ()
	{
		initialTarget = target;
		initialPosition = transform.position;
		initialRotation = transform.rotation;
		initialMoveSpeed = new MoveSpeed ();
		initialRotationSpeed = new RotationSpeed ();

		ResetSimulation ();
	}
	
	// This siumulates the enemy's movement based on the actual enemy movement
	public void SimulateOverTimePreserving (float time, int currentTimeStamp)
	{
		Vector3 outPos;
		Quaternion outRot;
		Waypoint outWay;
		
		EnemyMover.Solve (gameObject.GetHashCode (), dummyPosition, dummyRotation, currentMoveSpeed.moveSpeed, currentRotationSpeed.rotationSpeed, time, dummyTarget, 0.13f, out outPos, out outRot, out outWay);
		
		dummyPosition = outPos;
		dummyRotation = outRot;
		dummyTarget = outWay;

		currentMoveSpeed = GetNextMoveSpeed (currentTimeStamp);
		currentRotationSpeed = GetNextRotationSpeed (currentTimeStamp);
	}
	

	public void OnDrawGizmos ()
	{
		if (transform.FindChild ("FOV") != null) {
			GameObject FOV = transform.FindChild ("FOV").gameObject;
			Mesh mesh = FOV.GetComponent<MeshFilter> ().sharedMesh; 
			if (mesh == null) {
				//Create Mesh
				mesh = new Mesh (); 
				
				List<Vector3> vertices = new List<Vector3> ();
				
				Quaternion q = Quaternion.Euler (new Vector3 (0f, fovAngle, 0f));
				Vector3 dir = q * transform.forward;
				vertices.Add (dir * fovDistance);
				
				dir = (Quaternion.Inverse (q)) * transform.forward;
				vertices.Add (dir * fovDistance);
				
				vertices.Add (Vector3.zero);
				
				mesh.vertices = vertices.ToArray ();
				mesh.uv = new Vector2[] {
					new Vector2 (0, 0),
					new Vector2 (0, 1),
					new Vector2 (1, 1)
				};
				mesh.triangles = new int[] {2, 1, 0};
				
				//Normal
				mesh.RecalculateNormals ();
				mesh.RecalculateBounds ();
				mesh.Optimize (); 
			}
			FOV.GetComponent<MeshFilter> ().mesh = mesh;	
			
		} else {	
			Quaternion q = Quaternion.Euler (new Vector3 (0f, fovAngle, 0f));
			Vector3 dir = q * transform.forward;
			//Gizmos.color = LineForFOV; 
			Gizmos.DrawLine (transform.position, transform.position + dir * fovDistance);
			dir = (Quaternion.Inverse (q)) * transform.forward;
			Gizmos.DrawLine (transform.position, transform.position + dir * fovDistance);
		}
	}
	
	public Vector3 GetSimulationPosition ()
	{
		return dummyPosition;
	}
	
	public Vector3 GetSimulatedForward ()
	{
		return dummyRotation * Vector3.forward;
	}
	
	public Quaternion GetSimulatedRotation ()
	{
		return dummyRotation;
	}

	public MoveSpeed GetNextMoveSpeed (int timeStamp)
	{
		if (!moveSpeedDictionary.ContainsKey (timeStamp + 1)) {
			return null;
		} else {
			return moveSpeedDictionary [timeStamp + 1];
		}
	}

	public RotationSpeed GetNextRotationSpeed (int timeStamp)
	{
		if (!rotationSpeedDictionary.ContainsKey (timeStamp + 1)) {
			return null;
		} else {
			return rotationSpeedDictionary [timeStamp + 1];
		}
	}
}
