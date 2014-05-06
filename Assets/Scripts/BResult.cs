using System;
using System.Collections.Generic;

[Serializable]
public class BResultsRoot
{
	public List<BResultBatch> everything = new List<BResultBatch> ();
}

[Serializable]
public class BResultBatch
{
	public int numOfGuards = 0;
	public int numOfIterations = 0;
//	public int numOfPaths = 0;
//	public int rrtAttempts = 0;
	public List<BResult> results = new List<BResult> ();
	public float averageRatio = 0f;
}

[Serializable]
public class BResult
{
	public double ratio = 0f;
	public List<float> listOfDanger3 = new List<float> ();
}
