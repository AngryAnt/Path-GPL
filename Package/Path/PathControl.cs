using UnityEngine;
using System.Collections;
using PathLibrary;

public class PathControl : MonoBehaviour
{
	public Object pathAsset;
	public bool showNetworks = true;
	public float standardMarkerSize = 1.0f;
	
	
	
	public void Awake()
	{
		if( pathAsset == null )
		{
			Debug.LogError( "No PathAsset given" );
			Destroy( this );
			return;
		}
		
		Control.Instance.Init( this );
		
		Control.Instance.LoadCollection( pathAsset as IPathAsset );
	}
	
	
	
	public void OnDrawGizmos()
	{
		if( pathAsset != null && showNetworks )
		{
			DrawGizmos();
		}
	}
	
	
	
	public void OnDrawGizmosSelected()
	{
		if( pathAsset != null && !showNetworks )
		{
			DrawGizmos();
		}
	}
	
	
	
	public void DrawGizmos()
	{
		CollectionAsset collectionAsset;
		
		collectionAsset = ( Editor.Instance != null && Editor.Instance.Asset == ( IPathAsset )pathAsset ) ? Editor.Instance.Collection :
							CollectionAsset.LoadFromData( ( ( IPathAsset )pathAsset ).Data, ( IPathAsset )pathAsset );
		collectionAsset.OnRenderGizmos( transform, standardMarkerSize );
	}
}
