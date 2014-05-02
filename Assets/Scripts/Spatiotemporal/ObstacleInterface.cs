using UnityEngine;

public interface Obstacle {
	Shape3 ShadowPolygon(Vector3 viewpoint, float viewDistance);
}