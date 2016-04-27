using UnityEngine;
//using System.Collections;
using System.Collections.Generic;
using System;
//using System.IO;
//using UnityEditor;
//using KDTreeDLL;
using Common;
//using Objects;
//using Extra;

public class RRTResult : MonoBehaviour {
    public List<NodeGeo> Nodes;
    public string type;
    public bool success;
    public List<int> rrtsUsedMM;
    public List<int> nodesUsedMM;
    public List<int> nodesRejectedMM;
    public List<int> rrtsUsed2MM;
    public List<int> nodesUsed2MM;
    public List<int> nodesRejected2MM;
    public int nodesWasted;
    public DateTime startTime;
    public DateTime endTime;

    public RRTResult() {
        Nodes = new List<NodeGeo>();
        type = "";
        success = false;
        rrtsUsedMM = new List<int>();
        rrtsUsed2MM = new List<int>();
        nodesUsedMM = new List<int>();
        nodesUsed2MM = new List<int>();
        nodesRejectedMM = new List<int>();
        nodesRejected2MM = new List<int>();
        nodesWasted = 0;
    }
}
