
/**
 * Generic class for the computation of the Frechet distance.
 * Abstracts from the upper envelope, which is distance-measure specific.
 *
 * @author Wouter Meulemans (w.meulemans@tue.nl)
 */

		// source : http://www.win.tue.nl/~wmeulema/implementations.html
		// Original Java Implementation by Wouter Meulemans
		// based on paper by Buchin et al
		// http://arxiv.org/abs/1306.5527

using UnityEngine;
using System.Collections;

public abstract class FrechetDistance {

    protected int N, M;
    protected double[][] P, Q;

    public double computeDistance(double[][] P, double[][] Q) {

        this.P = P;
        this.Q = Q;

        this.N = P.Length - 1;
        this.M = Q.Length - 1;

        double dist = compute();

        this.P = null;
        this.Q = null;

        return dist;
    }

    private double compute() {

        Deque<DInt>[] column_queues = new Deque<DInt>[N];
        UpperEnvelope[] column_envelopes = new UpperEnvelope[N];
        for (int i = 0; i < N; i++) {
            column_queues[i] = new Deque<DInt>();
            column_envelopes[i] = initializeColumnUpperEnvelope(i);
        }

        Deque<DInt>[] row_queues = new Deque<DInt>[M];
        UpperEnvelope[] row_envelopes = new UpperEnvelope[M];
        for (int j = 0; j < M; j++) {
            row_queues[j] = new Deque<DInt>();
            row_envelopes[j] = initializeRowUpperEnvelope(j);
        }

        double[][] Lopt = new double[N][];
		for(int i = 0; i < N; i ++) Lopt[i] = new double[M];
        Lopt[0][0] = distance(P[0], Q[0]);
        for (int j = 1; j < M; j++) {
            Lopt[0][j] = double.PositiveInfinity;
        }

        double[][] B_opt = new double[N][];
		for(int i = 0; i < N; i ++) B_opt[i] = new double[M];
        B_opt[0][0] = Lopt[0][0];
        for (int i = 1; i < N; i++) {
            B_opt[i][0] = double.PositiveInfinity;
        }

        for (int i = 0; i < N; i++) {
            for (int j = 0; j < M; j++) {

                //System.out.println("Computing cell [" + i + "," + j + "]");
                if (i < N - 1) {
                    // compute Lopt[i+1][j]
                    //System.out.println("  Lopt[i+1][j]");

                    Deque<DInt> queue = row_queues[j];
                    UpperEnvelope upperenv = row_envelopes[j];

                    while (queue.getSize() > 0 && B_opt[queue.getLast().value][j] > B_opt[i][j]) {
                        queue.removeLast();
                    }
                    queue.addLast(new DInt(i));

                    if (queue.getSize() == 1) {
                        upperenv.clear();
                    }

                    upperenv.add(i + 1, Q[j], Q[j + 1], P[i + 1]);

                    int h = queue.getFirst().value;
                    double min = h < i ? upperenv.findMinimum(Lopt[i][j], B_opt[h][j]) : upperenv.findMinimum(B_opt[h][j]);

                    while (queue.getSize() > 1 && B_opt[queue.getFirst().getNext().value][j] <= min) {
                        queue.removeFirst();

                        h = queue.getFirst().value;
//                        assert h <= i;
                        upperenv.removeUpto(h);

                        min = h < i ? upperenv.findMinimum(Lopt[i][j], B_opt[h][j]) : upperenv.findMinimum(B_opt[h][j]);
                    }

                    Lopt[i + 1][j] = min;
                    upperenv.truncateLast();
                }

                if (j < M - 1) {
                    // compute B_opt[i][j+1]
                    //System.out.println("  B_opt[i][j+1]");

                    Deque<DInt> queue = column_queues[i];
                    UpperEnvelope upperenv = column_envelopes[i];

                    while (queue.getSize() > 0 && Lopt[i][queue.getLast().value] >= Lopt[i][j]) {
                        queue.removeLast();
                    }
                    queue.addLast(new DInt(j));

                    if (queue.getSize() == 1) {
                        upperenv.clear();
                    }

                    upperenv.add(j + 1, P[i], P[i + 1], Q[j + 1]);

                    int h = queue.getFirst().value;
                    double min = h < j ? upperenv.findMinimum(B_opt[i][j], Lopt[i][h]) : upperenv.findMinimum(Lopt[i][h]);

                    while (queue.getSize() > 1 && Lopt[i][queue.getFirst().getNext().value] <= min) {
                        queue.removeFirst();

                        h = queue.getFirst().value;
//                        assert h <= j;
                        upperenv.removeUpto(h);

                        min = h < j ? upperenv.findMinimum(B_opt[i][j], Lopt[i][h]) : upperenv.findMinimum(Lopt[i][h]);
                    }

                    B_opt[i][j + 1] = min;
                    upperenv.truncateLast();
                }
            }
        }

        return Mathf.Max((float)distance(P[N], Q[M]), Mathf.Min((float)Lopt[N - 1][M - 1], (float)B_opt[N - 1][M - 1]));
    }

    public abstract double distance(double[] p, double[] q);

    protected abstract UpperEnvelope initializeRowUpperEnvelope(int row);

    protected abstract UpperEnvelope initializeColumnUpperEnvelope(int column);

    private class DInt : DequeItem<DInt> {

        public int value;

        public DInt(int value) {
            this.value = value;
        }
    }
}
