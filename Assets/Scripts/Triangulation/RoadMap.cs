using System.Collections.Generic;
using UnityEngine; 
using Vectrosity;

public class RoadMap
{
	public List<Line> segments = new List<Line>();
	public List<List<Line>> voisins; 

	public void Clear()
	{
		segments.Clear(); 
	}

	public void Add(Line l)
	{
		segments.Add(l);
	}
	public bool Contains(Line l)
	{

		return segments.Contains(l);
	}
	public void Draw()
	{
		foreach(Line l in segments)
		{
			l.DrawVector(Color.blue); 
		}
	}

	public int Count()
	{
		return segments.Count; 
	}
	
	public List<Line> getList()
	{
		return segments;
	}

	public void SetVoisins()
	{
		voisins = new List<List<Line>>();
		int same = 0; 
		//Find data structure of friends
		foreach(Line l1 in segments)
		{
			List<Line> friends = new List<Line>(); 
			foreach(Line l2 in segments)
			{
				if( l1.Equals(l2))
				{
					//Debug.Log("SAME");
					same++; 
					continue; 
				}
				//find the friends

				if (!friends.Contains(l2) && l1.ShareVertex(l2))
					friends.Add(l2);

			}
			// Debug.Log(friends.Count);
			voisins.Add(friends);
		}


	}
	public void CleanRoadMap()
	{
		//Find long segments with only 2 neigbhoors.
		//Start place with 1 voisins. 
		//move towards 

		int[] visited = new int[segments.Count];
		List<Line> candidates = new List<Line>(); 

		for(int i =0; i<segments.Count; i++)
		{			
			if (voisins[i].Count == 1 || voisins[i].Count == 3 )
			{
				candidates.Add(segments[i]);
			}			
		}	



		

		foreach(Line l in candidates)
		{
			l.DrawVector(Color.red); 

			int i = segments.IndexOf(l);

			List<Line>lineToClean = new List<Line>(); 


			lineToClean.Add(segments[i]);
			visited[i] = 1; 
			//Find which direction to go, it should not be in the 
			//candidates root. 


			Line toBeNext = null ; 

			foreach(Line tobe in voisins[i])
			{
				if (!candidates.Contains(tobe) && visited[segments.IndexOf(tobe)] != 1)
					toBeNext = tobe;
			}

			if(toBeNext==null)
				continue; 


			List<Line>next = voisins[segments.IndexOf(toBeNext)];
			List<Line>oldNext = null; 
			int count = 0; 

			do
			{

				//Make sure moving forward. 
				oldNext = next; 
				//find where to go. 
				//if we have one voisin then we add and are done 
				
				//Add to my list
				lineToClean.Add( segments[voisins.IndexOf(next)] );
				visited[voisins.IndexOf(next)] = 1; 
				
				if(next.Count == 0 )
					continue; 


				// find where to go from the two places
				if (lineToClean.Contains(next[0]))
				{
					lineToClean.Add(next[1]);
					visited[segments.IndexOf(next[1])] = 1; 

					next = voisins[segments.IndexOf(next[1])];						
				}  
				else
				{
					lineToClean.Add(next[0]);
					visited[segments.IndexOf(next[0])] = 1; 

					next = voisins[segments.IndexOf(next[0])];

				}
						
				count++; 
				
			}
			while(oldNext != next && next.Count == 2 );	

			Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
	                    UnityEngine.Random.Range(0.0f,1.0f),
        	            UnityEngine.Random.Range(0.0f,1.0f)) ;

			foreach(Line l1 in lineToClean)
			{
				l1.DrawVector(c);
			}

		}
		
	}

}