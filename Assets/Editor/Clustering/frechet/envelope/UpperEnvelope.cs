
/**
 * Interface of the upper envelope data structure as required by the framework
 * of the frechetdistance.algo.FrechetDistance class.
 *
 * @author Wouter Meulemans (w.meulemans@tue.nl)
 */
public interface UpperEnvelope {
    
    void add(int i, double[] P1, double[] P2, double[] Q);
     void removeUpto(int i);
     void clear();
    
     double findMinimum(params double[] constants);
    
     void truncateLast();
}
