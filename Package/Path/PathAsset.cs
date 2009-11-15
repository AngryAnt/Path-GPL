using UnityEngine;
using System.Collections;
using PathLibrary;

[System.Serializable]
public class PathAsset : ScriptableObject, IPathAsset
{
	[HideInInspector]
	public byte[] data;
	
	
	public byte[] Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}
}
