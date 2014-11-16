using System;
using System.Collections.Generic;

[Serializable]
public class BResultsRoot2
{
	public List<BResultBatch2> everything = new List<BResultBatch2> ();
}

[Serializable]
public class BResultBatch2
{
	public int numOfCameras = 0;
	public int numOfGuards = 0;
	public int numOfIterations = 0;
//	public int numOfPaths = 0;
//	public int rrtAttempts = 0;
	public List<BResult2> results = new List<BResult2> ();
	public float averageRatio = 0f;
}

[Serializable]
public class BResult2
{
	public double ratio = 0f;
	// public List<float> listOfDanger3 = new List<float> ();
}
