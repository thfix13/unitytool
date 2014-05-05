using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PathSelector
{
	private string filePath = null, exportedPath = "LongestPaths.txt";
	private List<List<List<int>>> trialList = new List<List<List<int>>> ();
	public List<List<int>> longestPaths = new List<List<int>> ();
	
	public PathSelector (string filePath)
	{
		this.filePath = filePath;
	}
	
	public void selectLongestPath ()
	{		
		using (StreamReader sr = new StreamReader (filePath)) {
			string line = "";
			int trialIndex = -1;
			while ((line = sr.ReadLine())!=null) {
				if (!line.Equals ("")) {
					string[] tokens = line.Split (' ');
					int[] convertedIndice = Array.ConvertAll<string, int> (tokens, int.Parse);
					trialList.ElementAt (trialIndex).Add (new List<int> ());
					foreach (int index in convertedIndice) {
						trialList.ElementAt (trialIndex).Last ().Add (index);
					}
				} else {
					trialIndex += 1;
					trialList.Add (new List<List<int>> ());
				}
			}
		}
		
		int numOfGuards = trialList.First ().Count;
		
		// Choose the longest path for each guard
		for (int i = 0; i < numOfGuards; i++) {
			int cnt = 0, whichTrial = 0;
			for (int trialIndex = 0; trialIndex < trialList.Count; trialIndex++) {
				int tempCnt = trialList.ElementAt (trialIndex).ElementAt (i).Count;
				if (tempCnt > cnt) {
					cnt = tempCnt;
					whichTrial = trialIndex;
				}
			}
			List<int> longestPath = new List<int> ();
			foreach (int index in trialList.ElementAt (whichTrial).ElementAt (i)) {
				longestPath.Add (index);
			}
			longestPaths.Add (longestPath);
		}
	}
	
	public void saveLongestPathsToFile ()
	{
		using (StreamWriter sw = new StreamWriter(exportedPath, false)) {
			for (int size = 0; size < longestPaths.Count; size++) {
				for (int i = 0; i < longestPaths.ElementAt (size).Count; i++) {
					sw.Write (longestPaths.ElementAt (size).ElementAt (i));
					if (i != longestPaths.ElementAt (size).Count - 1) {
						sw.Write (" ");
					}
				}
				sw.WriteLine ();
			}
		}
	}
}
