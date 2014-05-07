
/**
 * A polyhedral distance function.
 * Supports a number of constructors via static functions.
 * L-1, L-infinity, and 2D-approximation functions are supported.
 * For irregular polyhedrons, use the custom() function. *
 *
 * @author Wouter Meulemans (w.meulemans@tue.nl)
 */

using UnityEngine;

public class PolyhedralDistanceFunction {

    public static PolyhedralDistanceFunction L1(int dimensions) {
   //     assert dimensions >= 2;

        int k = (int) Mathf.Round(Mathf.Pow(2, dimensions));
        double[][] facetdescripts = new double[k][];
		for (int i = 0; i < k; i++) facetdescripts[i] = new double[dimensions];

        double val = 1.0 / (double) dimensions;

        int totalblock = k;
        for (int d = 0; d < dimensions; d++) {

            int halfblock = totalblock / 2;

            for (int f = 0; f < k; f++) {

                if (f % totalblock < halfblock) {
                    facetdescripts[f][d] = val;
                } else {
                    facetdescripts[f][d] = -val;
                }
            }

            totalblock = halfblock;
        }

        return new PolyhedralDistanceFunction(facetdescripts);
    }

    public static PolyhedralDistanceFunction LInfinity(int dimensions) {
   //     assert dimensions >= 2;

        double[][] facetdescripts = new double[2 * dimensions][];
		for (int i = 0; i < 2 * dimensions; i++) facetdescripts[i] = new double[dimensions];
        for (int i = 0; i < dimensions; i++) {
            for (int j = 0; j < dimensions; j++) {
                facetdescripts[2 * i][j] = i == j ? 1 : 0;
                facetdescripts[2 * i + 1][j] = i == j ? -1 : 0;
            }
        }

        return new PolyhedralDistanceFunction(facetdescripts);
    }

    public static PolyhedralDistanceFunction epsApproximation2D(double eps) {
    //    assert eps > 1;

        int k;
        if (eps >= Mathf.Sqrt(2)) {
            k = 4;
        } else {
            k = (int) Mathf.Ceil((float)(Mathf.PI * 2.0 / Mathf.Acos((float)(1.0 / eps))));
            if (k % 2 == 1) {
                k++;
            }
        }

        return kRegular2D(k);
    }

    public static PolyhedralDistanceFunction kRegular2D(int k) {
//        assert k >= 4 && k % 2 == 0;

        double[][] facetdescripts = new double[k][];
		for (int i = 0; i < k; i++) facetdescripts[i] = new double[2];
		
        double alpha = 2 * Mathf.PI / (double) k;

        for (int i = 0; i < k; i++) {
            facetdescripts[i][0] = 0.5 * (Mathf.Cos((float)(i * alpha)) + Mathf.Cos((float)((i + 1) * alpha)));
            facetdescripts[i][1] = 0.5 * (Mathf.Sin((float)(i * alpha)) + Mathf.Sin((float)((i + 1) * alpha)));
        }

        return new PolyhedralDistanceFunction(facetdescripts);
    }

    public static PolyhedralDistanceFunction custom(double[][] facetNormals, double[][] facetPoints, bool normalize) {

        // NB: should be symmetric!!

  //      assert facetNormals.length == facetPoints.length;
    //    assert facetNormals[0].length == facetPoints[0].length;

        double[][] facetdescripts = new double[facetNormals.Length][];

        for (int i = 0; i < facetdescripts.Length; i++) {
            double[] N = facetNormals[i];
            if (normalize) {
                N = VectorUtil.normalize(N);
            }
            facetdescripts[i] = VectorUtil.scale(VectorUtil.dotProduct(facetNormals[i], facetPoints[i]), facetNormals[i]);
        }

        return new PolyhedralDistanceFunction(facetdescripts);
    }

    // facet is described by the closest point to the origin
    // it is directly the normal and its length encodes normalization
    //
    private double[][] facets;
    private double[] facetSqrLength;

    private PolyhedralDistanceFunction(double[][] facets) {
        this.facets = facets;
        this.facetSqrLength = new double[facets.Length];
        for (int i = 0; i < this.facetSqrLength.Length; i++) {
            this.facetSqrLength[i] = VectorUtil.squaredLength(this.facets[i]);
        }
    }

    public int getComplexity() {
        return facets.Length;
    }

    public double[] getFacet(int facet) {
        return facets[facet];
    }

    public double getDistance(double[] p, double[] q) {
        return getDistance(VectorUtil.subtract(q, p));
    }

    public double getDistance(double[] d) {
        double max = double.NegativeInfinity;
        for (int i = 0; i < facets.Length; i++) {
            double fd = getFacetDistance(d, i);
            max = Mathf.Max((float)max, (float)fd);
        }
        return max;
    }

    public double getFacetDistance(double[] p, double[] q, int facet) {
        return getFacetDistance(VectorUtil.subtract(q, p), facet);
    }

    public double getFacetDistance(double[] d, int facet) {
        return VectorUtil.dotProduct(facets[facet], d) / facetSqrLength[facet];
    }

    public double getFacetSlope(double[] p1, double[] p2, int facet) {
        return getFacetDistance(VectorUtil.subtract(p2, p1), facet);
    }
}
