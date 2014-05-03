using UnityEngine;
using System.Collections;

public class HSV {

	public static Color Color(float h, float s, float v, float a=1.0f) {
		h = (h % 360 + 360) % 360;
		if (s < 0)
			s = 0;
		if (s > 1)
			s = 1;
		if (v < 0)
			v = 0;
		if (v > 1)
			v = 1;

		float c = v * s;
		float x = c * (1 - Mathf.Abs ((h / 60) % 2 - 1));
		float m = v - c;

		Color prime;

		if (h < 60) {
			prime = new Color(c, x, 0);
		} else if (h < 120) {
			prime = new Color(x, c, 0);
		} else if (h < 180) {
			prime = new Color(0, c, x);
		} else if (h < 240) {
			prime = new Color(0, x, c);
		} else if (h < 300) {
			prime = new Color(x, 0, c);
		} else {
			prime = new Color(c, 0, x);
		}

		return new Color(prime.r+m, prime.g+m, prime.b+m, a);
	}
}
