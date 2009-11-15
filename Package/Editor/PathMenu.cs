using UnityEngine;
using UnityEditor;
using PathLibrary;
using System.Collections;

public class PathMenu : ScriptableObject
{	
	[MenuItem( "Assets/Create/Path collection" )]
	public static void CreateCollection()
	{
		PathAsset asset;
		string name = "NewPathCollection";
		int nameIdx = 0;
		
		while( System.IO.File.Exists( Application.dataPath + "/" + name + nameIdx + ".asset" ) )
		{
			nameIdx++;
		}

		asset = new PathAsset();
		asset.Data = ( new CollectionAsset() ).GetData();
		AssetDatabase.CreateAsset( asset, "Assets/" + name + nameIdx + ".asset" );
		
		EditorUtility.FocusProjectView();
		Selection.activeObject = asset;
		
		EditCollection();
	}


	[MenuItem( "Assets/Path/Edit collection %e" )]	
	public static void EditCollection()
	{
		EditCollectionAsset( ( PathAsset )Selection.activeObject );
	}
	
	
	[MenuItem( "Assets/Path/Edit collection %e", true )]	
	public static bool ValidateEditCollection()
	{
		return Selection.activeObject is PathAsset && ( Selection.activeObject as PathAsset ).data != null;
	}


	[ MenuItem( "Assets/Path/Create/Waypoint" ) ]
	public static void CreateWaypoint()
	{
		if( !ValidateCreateWaypoint() )
		{
			Debug.LogError( "Menu should not be able to execute at this time. Ignored." );
			return;
		}
		
		PathLibrary.Editor.Instance.CreateWaypoint();
	}
	
	
	[ MenuItem( "Assets/Path/Create/Waypoint", true ) ]
	public static bool ValidateCreateWaypoint()
	{
		return PathLibrary.Editor.Instance != null && PathLibrary.Editor.Instance.ValidateCreateWaypoint();
	}


	[ MenuItem( "Assets/Path/Create/Waypoint network" ) ]
	public static void CreateWaypointNetwork()
	{
		if( !ValidateCreateWaypointNetwork() )
		{
			Debug.LogError( "Menu should not be able to execute at this time. Ignored." );
			return;
		}
		
		PathLibrary.Editor.Instance.CreateWaypointNetwork();
	}
	
	
	[ MenuItem( "Assets/Path/Create/Waypoint network", true ) ]
	public static bool ValidateCreateWaypointNetwork()
	{
		return PathLibrary.Editor.Instance != null && PathLibrary.Editor.Instance.ValidateCreateWaypointNetwork();
	}
	
	
	[ MenuItem( "Assets/Path/Create/Navmesh" ) ]
	public static void CreateNavmesh()
	{
		if( !ValidateCreateNavmesh() )
		{
			Debug.LogError( "Menu should not be able to execute at this time. Ignored." );
			return;
		}
		
		PathLibrary.Editor.Instance.CreateNavmesh();
	}
	
	
	[ MenuItem( "Assets/Path/Create/Navmesh", true ) ]
	public static bool ValidateCreateNavmesh()
	{
		return PathLibrary.Editor.Instance != null && PathLibrary.Editor.Instance.ValidateCreateNavmesh();
	}
	
	
	[ MenuItem( "Assets/Path/Create/Grid node" ) ]
	public static void CreateGridNode()
	{
		if( !ValidateCreateGridNode() )
		{
			Debug.LogError( "Menu should not be able to execute at this time. Ignored." );
			return;
		}
		
		PathLibrary.Editor.Instance.CreateGridNode();
	}
	
	
	[ MenuItem( "Assets/Path/Create/Grid node", true ) ]
	public static bool ValidateCreateGridNode()
	{
		return PathLibrary.Editor.Instance != null && PathLibrary.Editor.Instance.ValidateCreateGridNode();
	}
	
	
	[ MenuItem( "Assets/Path/Create/Grid network" ) ]
	public static void CreateGridNetwork()
	{
		if( !ValidateCreateGridNetwork() )
		{
			Debug.LogError( "Menu should not be able to execute at this time. Ignored." );
			return;
		}
		
		PathLibrary.Editor.Instance.CreateGridNetwork();
	}
	
	
	[ MenuItem( "Assets/Path/Create/Grid network", true ) ]
	public static bool ValidateCreateGridNetwork()
	{
		return PathLibrary.Editor.Instance != null && PathLibrary.Editor.Instance.ValidateCreateGridNetwork();
	}
	
	
	[ MenuItem( "Assets/Path/Create/Tag" ) ]
	public static void CreateTag()
	{
		if( !ValidateCreateTag() )
		{
			Debug.LogError( "Menu should not be able to execute at this time. Ignored." );
			return;
		}
		
		PathLibrary.Editor.Instance.CreateTag();
	}
	
	
	[ MenuItem( "Assets/Path/Create/Tag", true ) ]
	public static bool ValidateCreateTag()
	{
		return PathLibrary.Editor.Instance != null && PathLibrary.Editor.Instance.ValidateCreateTag();
	}


	[ MenuItem( "Window/Path browser" ) ]
	public static void ShowBrowser()
	{
		if( PathBrowser.Instance == null )
		{
			Debug.LogError( "Failed to set up Path Browser" );
		}
		
		PathLibrary.Browser.Instance.Show();
		PathLibrary.Browser.Instance.Focus();
		PathLibrary.Browser.Instance.Repaint();
	}
	
	
	[ MenuItem( "Help/About Path..." ) ]
	public static void About()
	{
		PathAbout.Instance.ShowUtility();
	}
	
	
	public static void EditCollectionAsset( PathAsset pathAsset )
	{
		PathEditor pathEditor;
		
		if( PathLibrary.Editor.Instance == null )
		{
			pathEditor = new PathEditor();
			PathLibrary.Editor.Init( pathEditor );
			pathEditor.Init();
		}
		
		PathLibrary.Editor.Instance.Asset = pathAsset;
		
		ShowBrowser();
	}
}
