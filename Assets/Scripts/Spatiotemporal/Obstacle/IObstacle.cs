using UnityEngine;
using GeometryLib;

namespace Spatiotemporal {
	public interface IObstacle {
		Vector3 position { get; }
		float sizeX { get; }
		float sizeZ { get; }
		float rotation { get; }
		Shape3 GetShape();
		Shape3 ShadowPolygon(Vector3 viewpoint, float viewDistance);
	}
}