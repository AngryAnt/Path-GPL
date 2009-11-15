using UnityEngine;
using UnityEditor;
using System.Collections;
using PathLibrary;

[ CustomEditor( typeof( PathControl ) ) ]
public class PathComponentEditor : UnityEditor.Editor, IInspector
{
	private bool doEdit = false;
	
	
	public PathComponentEditor()
	{
		//Debug.Log( "Constructor!" );
		EditorApplication.update += Update;
	}
	
	
	public void Update()
	{
		PathControl control;
		
		// Start editing if needed
		if( doEdit )
		{
			doEdit = false;
			control = target as PathControl;
			PathMenu.EditCollectionAsset( control.pathAsset as PathAsset );
		}
	}
	
	
	public Transform Transform
	{
		get
		{
			PathControl control;
			
			control = target as PathControl;
			
			if( control == null )
			{
				return null;
			}
			
			return control.transform;
		}
	}
	
	
	public void OnInspectorGUI()
	{
		PathControl control;
		string[] selector;
		
		selector = new string[ 2 ];
		selector[ 0 ] = "Always";
		selector[ 1 ] = "When selected";
		
		control = target as PathControl;
		
		// Visualisation
		GUILayout.BeginHorizontal();
			GUILayout.BeginHorizontal( GUILayout.Width( PathLibrary.Resources.DefaultLeftColumnWidth ) );
				GUILayout.FlexibleSpace();
				GUILayout.Label( "Visualisation" );
			GUILayout.EndHorizontal();
			control.showNetworks = ( GUILayout.Toolbar( control.showNetworks ? 0 : 1, selector ) == 0 );
		GUILayout.EndHorizontal();
		
		// Standard marker size
		control.standardMarkerSize = EditorGUILayout.FloatField( "Marker size", control.standardMarkerSize );
		
		// Collection
		control.pathAsset = EditorGUILayout.ObjectField( "Collection", control.pathAsset, typeof( PathAsset ) );
		if( control != null && control.pathAsset != null && (
			PathLibrary.Editor.Instance == null ||
			PathLibrary.Editor.Instance.Asset != ( IPathAsset )control.pathAsset )
		)
		{
			GUILayout.BeginHorizontal();
				GUILayout.Space( PathLibrary.Resources.DefaultLeftColumnWidth );
				if( GUILayout.Button( "Edit" ) )
				{
					doEdit = true;
				}
			GUILayout.EndHorizontal();
		}
		
		// Inspector
		if( control != null && control.pathAsset != null && Inspector.Instance != null && PathLibrary.Editor.Instance != null &&
			PathLibrary.Editor.Instance.Asset == ( IPathAsset )control.pathAsset )
		{
			Inspector.Instance.OnGUI( this );
		}
		
		// Update if needed
		if( GUI.changed )
		{
			EditorUtility.SetDirty( control );
		}
	}
	
	
	public Bounds OnGetFrameBounds()
	{
		PathControl control;
	
		control = target as PathControl;
	
		if( control.pathAsset != null && PathLibrary.Editor.Instance != null &&
			PathLibrary.Editor.Instance.Asset == ( IPathAsset )control.pathAsset )
		{
			return PathLibrary.Editor.Instance.OnGetFrameBounds( control.transform );
		}
		
		return new Bounds( SceneView.current.pivot, Vector3.zero );
	}
	
	
	public void OnSceneGUI()
	{
		PathControl control;

		control = target as PathControl;

		// Handle if we're now rendering for default references //
		try
		{
			if( control == null || control.transform == null )
			{
				return;
			}
		}
		catch( System.Exception )
		{
			return;
		}
		
		if( control.pathAsset != null && PathLibrary.Editor.Instance != null && PathLibrary.Editor.Instance.OnSceneGUI( control.transform ) )
		{
			EditorUtility.SetDirty( control );
			PathLibrary.Editor.Instance.UpdateScene();
		}
	}
}
