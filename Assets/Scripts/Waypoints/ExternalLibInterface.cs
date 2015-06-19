using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using System.Runtime.InteropServices;
public partial class Visibility1 : MonoBehaviour 
{
	private const string DllFilePath = @"c:\pathto\mydllfile.dll";
	
	[DllImport(DllFilePath , CallingConvention = CallingConvention.Cdecl)]
	private extern static int calculateVisibilityForPath();
	
	public void CalculateVisibilityForPath2()
	{
		calculateVisibilityForPath();
	}
}