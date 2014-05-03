using System;
using System.Collections.Generic;

[Serializable]
public class TResultRoot
{
	public List<TResultBatch> everything = new List<TResultBatch> ();
}
	
[Serializable]
public class TResultBatch
{
	public float fovAngle = 0f;
	public float fovDistance = 0f;
	public List<TResult> results = new List<TResult> ();
	public double averageRatio = 0f;
}
	
[Serializable]
public class TResult
{
	public double ratio = 0f;
}
