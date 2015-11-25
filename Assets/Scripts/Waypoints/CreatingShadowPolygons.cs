using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
public partial class Visibility1 : MonoBehaviour 
{
	int m_modBoundaryCount=0;
	int m_modObstcleCount=0;
	int m_VisibleTriangleCount=0;
	int m_ShadowPolygonCount=0;
	int m_CommonLinesCount=0;
	private List<Geometry> FindShadowPolygons(Vector3 pPoint, List<VisibleTriangles> listTriangles)
	{
		m_modBoundaryCount=0;
		m_modObstcleCount=0;
		m_VisibleTriangleCount=0;
		m_ShadowPolygonCount=0;
		m_CommonLinesCount=0;

		Geometry visiblePoly = new Geometry ();
		//Creating visible Polygon from triangles
		for(int currIndx=0;currIndx<listTriangles.Count;currIndx++)
		{
			visiblePoly.edges.Add(new Line(listTriangles[currIndx].pt2,listTriangles[currIndx].pt3));

			int nextIndx = (currIndx+1)%listTriangles.Count;

			//If any pt is common, skip adding a new line

			if(VectorApprox(listTriangles[currIndx].pt2,listTriangles[nextIndx].pt2)
			   || VectorApprox(listTriangles[currIndx].pt2,listTriangles[nextIndx].pt3)
			   || VectorApprox(listTriangles[currIndx].pt3,listTriangles[nextIndx].pt2)
			   || VectorApprox(listTriangles[currIndx].pt3,listTriangles[nextIndx].pt3))
			{
				continue;
			}
			//

			float? m1 = slopeFromPts(pPoint,listTriangles[currIndx].pt2);
			float? m2 = slopeFromPts(pPoint,listTriangles[currIndx].pt3);

			float? m3 = slopeFromPts(pPoint,listTriangles[nextIndx].pt2);
			float? m4 = slopeFromPts(pPoint,listTriangles[nextIndx].pt3);

			if((m1==null && m3==null) || (m1.HasValue && m3.HasValue && slopeCompare(m1.Value,m3.Value)))
			{
				visiblePoly.edges.Add(new Line(listTriangles[currIndx].pt2,listTriangles[nextIndx].pt2));
				m_CommonLinesCount++;
			}
			else if((m1==null && m4==null) || (m1.HasValue && m4.HasValue && slopeCompare(m1.Value,m4.Value)))
			{
				visiblePoly.edges.Add(new Line(listTriangles[currIndx].pt2,listTriangles[nextIndx].pt3));
				m_CommonLinesCount++;
			}
			else if((m2==null && m3==null) || (m2.HasValue && m3.HasValue && slopeCompare(m2.Value,m3.Value)))
			{
				visiblePoly.edges.Add(new Line(listTriangles[currIndx].pt3,listTriangles[nextIndx].pt2));
				m_CommonLinesCount++;
			}
			else if((m2==null && m4==null) || (m2.HasValue && m4.HasValue && slopeCompare(m2.Value,m4.Value)))
			{
				visiblePoly.edges.Add(new Line(listTriangles[currIndx].pt3,listTriangles[nextIndx].pt3));
				m_CommonLinesCount++;
			}
		}
		//
		//List<Geometry> tempListGeo = new List<Geometry> ();
		//tempListGeo.Add (visiblePoly);
		//return tempListGeo;
		List<Geometry> shadowListFinal = FindShadowPolygons(visiblePoly,pathPoints.IndexOf(pPoint));

		//Debug
		m_VisibleTriangleCount=listTriangles.Count-1;//-1 because do not know why yet?
		if((m_modBoundaryCount+m_modObstcleCount) != (m_VisibleTriangleCount+m_ShadowPolygonCount-m_CommonLinesCount))
		{
			Debug.Log("ERROR!!!!!!!!!!!!!!!!!!!!! at "+pathPoints.IndexOf(pPoint)+". Shadow Polygon not correct.");
			/*Debug.Log("(m_modBoundaryCount+m_modObstcleCount) = "+(m_modBoundaryCount+m_modObstcleCount));
			Debug.Log("(m_VisibleTriangleCount+m_ShadowPolygonCount-m_CommonLinesCount) = "+(m_VisibleTriangleCount+m_ShadowPolygonCount-m_CommonLinesCount));
			Debug.Log("(m_modBoundaryCount) = "+(m_modBoundaryCount));
			Debug.Log("(m_modObstcleCount) = "+(m_modObstcleCount));
			Debug.Log("(m_VisibleTriangleCount) = "+(m_VisibleTriangleCount));
			Debug.Log("(m_ShadowPolygonCount) = "+(m_ShadowPolygonCount));
			Debug.Log("(m_CommonLinesCount) = "+(m_CommonLinesCount));*/
		}
		//

		return shadowListFinal;
	}
	float? slopeFromPts(Vector3 pt1,Vector3 pt2)
	{
		if((pt2.x != pt1.x))
			return ((pt2.z - pt1.z) / (pt2.x - pt1.x));
		return null;
	}
	private bool slopeCompare ( float a, float b )
	{
		//if(currSceneName=="myCrash.unity" || currSceneName=="myCrash_Shorter.unity" || currSceneName=="myCrash_Shorter2.unity")
		//{
			return Mathf.Abs (a - b) < eps4;
		//}
		//return Mathf.Abs (a - b) < eps;
	}
}