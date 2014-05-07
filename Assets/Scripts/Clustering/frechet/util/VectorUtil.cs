
/**
 * Some simple vector operations...
 *
 * @author Wouter Meulemans (w.meulemans@tue.nl)
 */
using UnityEngine;
public class VectorUtil {

    public static double[] add(double[] A, double[] B) {
  //      assert A.length == B.length;

        double[] R = new double[A.Length];
        for (int i = 0; i < R.Length; i++) {
            R[i] = A[i] + B[i];
        }
        return R;
    }

    public static double[] subtract(double[] A, double[] B) {
  //      assert A.length == B.length;

        double[] R = new double[A.Length];
        for (int i = 0; i < R.Length; i++) {
            R[i] = A[i] - B[i];
        }
        return R;
    }

    public static double dotProduct(double[] A, double[] B) {
 //       assert A.length == B.length;

        double dot = 0;
        for (int i = 0; i < A.Length; i++) {
            dot += A[i] * B[i];
        }
        return dot;
    }

    public static double distance(double[] A, double[] B) {
        return length(subtract(B,A));
    }

    public static double length(double[] A) {
        return Mathf.Sqrt((float)squaredLength(A));
    }

    public static double squaredLength(double[] A) {
        double sum = 0;
        for (int i = 0; i < A.Length; i++) {
            sum += A[i] * A[i];
        }
        return sum;
    }

    public static double[] scale(double S, double[] A) {
        double[] R = new double[A.Length];
        for (int i = 0; i < R.Length; i++) {
            R[i] = S * A[i];
        }
        return R;
    }

    public static double[] normalize(double[] A) {
        return scale(1.0 / length(A), A);
    }
}
