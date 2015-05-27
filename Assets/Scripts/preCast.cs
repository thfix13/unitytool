using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using KDTreeDLL;
using Common;
using Objects;
using Extra;//------------------------------------------------------------------------------

public class preCast{

	public float minX;
	public float maxX;
	public float minY;
	public float maxY;
	public float startX;
	public float startY;
	public int width;
	public int height;
	public float stepX;
	public float stepY;
	public bool[,,,] casts;

	public preCast (float iminX, float imaxX, float iminY, float imaxY, int iwidth, int iheight, float istepX, float istepY, bool[,,,] icasts){
		minX = iminX;
		maxX = imaxX;
		minY = iminY;
		maxY = imaxY;
		width = iwidth;
		height = iheight;
		stepX = istepX;
		stepY = istepY;
		casts = icasts;
		startX = Mathf.Floor(minX);
		startY = Mathf.Floor (minY);

	}

	public bool getCast(float x1, float y1, float x2, float y2){
		float distx1 = x1 - startX;
		float distx2 = x2 - startX;
		float disty1 = y1 - startY;
		float disty2 = y2 - startY;

		int i1 = Mathf.FloorToInt(distx1 / stepX);
		int i2 = Mathf.FloorToInt(disty1 / stepY);
		int i3 = Mathf.FloorToInt(distx2 / stepX);
		int i4 = Mathf.FloorToInt(disty2 / stepY);

		return casts[i1, i2, i3, i4];

	}

	private void squid(int q){
		float startX = Mathf.Floor(minX);
		float startY = Mathf.Floor(minY);
		float stepX = Mathf.Round((maxX - minX) / width);
		float stepY = Mathf.Round ((maxY - minY) / height);
		float curX1 = startX;
		float curY1 = startY;
		float curX2 = startX;
		float curY2 = startY;
		bool[,,,] castsA = new bool[width,height,width,height];
		
		
		for (int i = 0; i < width; i++){
			curY1 = startY;
			for (int j = 0; j < height; j++){
				curX2 = startX;
				for (int k = 0; k < width; k++){
					curY2 = startY;
					for(int l = 0; l < height; l++){
						
						Vector3 start = new Vector3(curX1, 0, curY1);
						Vector3 end = new Vector3(curX2, 0, curY2);
						int layerMask = 1 << 8;
						castsA[i,j,k,l] = Physics.Linecast (start, end, layerMask);
						curY2 = curY2 + stepY;
					}
					curX2 = curX2 + stepX;
				}
				curY1 = curY1 + stepY;
			}
			curX1 = curX1 + stepX;
		}
	}
}

