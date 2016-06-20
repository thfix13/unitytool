using UnityEngine;
using System.Collections;
using Common;
using System.Collections.Generic;

public class SoundTrigger : MonoBehaviour {
    private List<TriggerZone> triggers;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // gets the trigger zones in the map
    public void getZones() {
        // TODO
    }

    // checks to see if a given line intersects a trigger zone
    // could make this return a bool instead
    public void checkLine(Vector2 point1, Vector2 point2) {
        // TODO
        List<TriggerZone> activeTriggerZones = new List<TriggerZone>();
        foreach (TriggerZone t in triggers) {
            Vector2 triggerPoint;
            triggerPoint.x = t.x;
            triggerPoint.y = t.y;
            if (distanceFromLineSegment(point1, point2, triggerPoint) < t.radius) activeTriggerZones.Add(t);
        }
        onTriggerEnter();
    }

    // an overloaded version with values instead of vectors
    public void checkLine(float x1, float y1, float x2, float y2) {
        checkLine(new Vector2(x1, y1), new Vector2(x2, y2));
    }

    // with help from Wolfram MathWorld
    private float distanceFromLineSegment(Vector2 a, Vector2 b, Vector3 p) {
        Vector2 ap;
        ap.x = a.x - p.x;
        ap.y = a.y - p.y;
        Vector2 ba;
        ba.x = b.x - a.x;
        ba.y = b.y - a.y;

        float t = -(Vector2.Dot(ap, ba)) / (Vector2.SqrMagnitude(ba));
        Vector2 newPoint;
        if (t < 0.0f) newPoint = a;
        else if (t > 1.0f) newPoint = b;
        else newPoint = new Vector2(a.x + t * ba.x, a.y + t * ba.y);

        float result = 0.0f; // TODO: get distance from newPoint to p

        return result;
    }

    // TODO: expand
    // handles the activation of a trigger zone
    protected void onTriggerEnter(/* parameters should be information about the trigger areas */) {
        Debug.Log("Sound zone triggered");
    }
}
