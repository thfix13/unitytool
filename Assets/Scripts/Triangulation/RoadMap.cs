using System.Collections.Generic;
using System.Collections; 
using UnityEngine; 
using Vectrosity;
using System.Linq;
using System; 

[Serializable]
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
	public void CleanRoadMap(List<Geometry> geos)
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
		List<Line> linesPlayedWith = new List<Line>(); 
		List<Line> newSegments = new List<Line>(); 

		foreach(Line l in candidates)
		{
			// l.DrawVector(Color.red); 
			int i = segments.IndexOf(l);

			List<Line>lineToClean = new List<Line>(); 


			lineToClean.Add(segments[i]);
			visited[i] = 1; 
			//Find which direction to go, it should not be in the 
			//candidates root. 


			Line toBeNext = null ; 

			foreach(Line tobe in voisins[i])
			{
				if ( 
					( !candidates.Contains(tobe) 
					  || voisins[segments.IndexOf(tobe)].Count==1
					) 
					&& visited[segments.IndexOf(tobe)] != 1)
					
					toBeNext = tobe;
			}

			if(toBeNext==null)
				continue; 


			List<Line>next = voisins[segments.IndexOf(toBeNext)];
			List<Line>oldNext = null; 
			int count = 0; 

			// l.DrawVector(Color.red);
			// continue; 

			do
			{

				//Make sure moving forward. 
				oldNext = next; 
				//find where to go. 
				//if we have one voisin then we add and are done 
				
				//Add to my list
				if(!lineToClean.Contains(segments[voisins.IndexOf(next)] ))
				{
					lineToClean.Add( segments[voisins.IndexOf(next)] );
					visited[voisins.IndexOf(next)] = 1; 
				}				
				if(next.Count==3)
					break; 
				else if(next.Count == 1)
				{
					
					if(!lineToClean.Contains(next[0] ))
					{
						lineToClean.Add( next[0]);
						visited[segments.IndexOf(next[0])] = 1; 	
					}
				}

				// find where to go from the two places
				else if (lineToClean.Contains(next[0]))
				{

					next = voisins[segments.IndexOf(next[1])];						
				}  
				else
				{

					next = voisins[segments.IndexOf(next[0])];

				}
				
			}
			while(oldNext != next);	

			Color c = new Color(UnityEngine.Random.Range(0.0f,1.0f),
	                    UnityEngine.Random.Range(0.0f,1.0f),
        	            UnityEngine.Random.Range(0.0f,1.0f)) ;

			
			foreach(Line l1 in lineToClean)
			{
				linesPlayedWith.Add(l1);
				//l1.DrawVector(c);
			}

			//Clean the road now. 
			//Need to keep first point and last point
			List<Vector3> points = new List<Vector3>(); 
				
			Vector3 toAdd = lineToClean[0].getNotSharedVertex(lineToClean[1]); 
			points.Add(toAdd);
			
			int k = 0; 

			for(int j = 0; j<lineToClean.Count; j+=1)
			{	
				if(!containListVertex( points,lineToClean[j].getOtherVertex(points[k])))
				{
					points.Add(lineToClean[j].getOtherVertex(points[k]));
					k++;
				}
			}
			//points.Add(lineToClean[lineToClean.Count-1].getOtherVertex(points[points.Count-1]));

			//Now with the list of point time to clear it. 

			//The first one is the exterior one of the geos. 
				


			List<Vector3> toKeep = new List<Vector3>(); 
			toKeep.Add(points[0]);
			for(int j = 0; j<points.Count;j++)
			{
				Vector3 v1 = points[j];

				int p2 = j; 
				do
				{
					
					p2++; 
				}
				while(p2 < points.Count && CollisionFree(geos, v1,points[p2]));

				//if(points.Count==p2)
				p2--;

				toKeep.Add(points[p2]);
				j=p2; 
			}


			for(int j = 0; j<toKeep.Count-1; j++)
			{
				newSegments.Add(new Line(toKeep[j],toKeep[j+1]));
			}
			
		}	
		

		List<Line> differenceQuery = new List<Line>(); 

		foreach(Line l1 in segments)
		{
			bool found = false; 
			foreach(Line l2 in linesPlayedWith)
			{
				if (l1.Equals(l2))
				{
					found = true; 
					break; 
				}
			}
			if(!found)
				newSegments.Add(l1);
		}

		// Debug.Log(differenceQuery);

		foreach(Line l1 in newSegments)
		{
			l1.DrawVector(); 
		}
		segments = newSegments; 	
	}


	public void FindCusps(List<Geometry> geos,List<Vector3> obsListPoint)
	{
		Debug.Log(segments.Count);

		GameObject[] spheres = GameObject.FindGameObjectsWithTag("sphere");

		foreach(GameObject g in spheres)
		{
			GameObject.DestroyImmediate(g);

		}

		foreach(Line l in segments)
		{
			//Get the line to be broken appart.
			List<Vector3> points = l.BreakConstantRate(1f);
			
			//Find the points that be seen

			foreach(Vector3 v1 in points)
			{
				List<Vector3> canSee = new List<Vector3>(); 

				foreach(Vector3 v2 in obsListPoint)
				{
					if(CollisionFree(geos,v1,v2))
					{
						canSee.Add(v2);
					}
				}
				//Create sphere at v1

				GameObject tBalle = DrawSphere(v1);
				CubeVision cVision = tBalle.AddComponent("CubeVision") as CubeVision;

				cVision.pointsCanSee = canSee; 
//				return;
			}

		}

	}

	bool CollisionFree(List<Geometry> geos,Vector3 v1, Vector3 v2)
	{
		Line l = new Line(v1,v2);

		for(int i = 0; i<geos.Count; i++)
		{
			if(geos[i].LineCollision(l))
				return false; 
		}
		return true; 
	}

	bool CollisionFree(List<Geometry> geos,Line l)
	{
		for(int i = 0; i<geos.Count; i++)
		{
			if(geos[i].LineCollision(l))
				return false; 
		}
		return true; 
	}

	bool containListVertex(List<Vector3> vs, Vector3 v)
	{
		foreach(Vector3 v1 in vs)
		{
			if(Line.VectorApprox(v1,v))
				return true; 
		}
		return false;
	}

	void DrawSphere( Vector3 v, Color x )
	{
		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.name = "balle";
		inter.transform.renderer.material.color = x;
		inter.transform.position = v;
		inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f);
		inter.transform.parent = temp.transform;
		//inter.gameObject.name = vlcnt.ToString();
	}
	GameObject DrawSphere( Vector3 v )
	{

		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.name = "Balle";
		inter.transform.position = v;
		inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f); 
		inter.transform.parent = temp.transform;
		//inter.gameObject.name = vlcnt.ToString();
		return inter;
	}

	void DrawSphere( Vector3 v,string s )
	{

		GameObject temp = GameObject.Find ("temp");
		GameObject inter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		inter.name = "Balle " + s;
		inter.transform.position = v;
		inter.transform.localScale = new Vector3(0.3f,0.3f,0.3f); 
		inter.transform.parent = temp.transform;
		//inter.gameObject.name = vlcnt.ToString();
	}

}