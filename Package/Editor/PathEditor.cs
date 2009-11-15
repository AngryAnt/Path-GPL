using UnityEngine;
using UnityEditor;
using System.Collections;
using PathLibrary;

public class PathEditor : ScriptableObject, IEditorWindow
{
	private static PathEditor instance;
	private PathLibrary.Editor editor;
	private Quaternion quat = new Quaternion( 0, 0, 0, 0 );
	private Vector3 vect = new Vector3( 0, 0, 0 );
	
	
	
	public PathEditor()
	{
		if( instance != null )
		{
			Debug.LogError( "Trying to create two instances of singleton. Self destruction in 3..." );
			Destroy( this );
			return;
		}
		
		if( this.Editor == null )
		{
			Debug.LogError( "Failed to link with library implementation" );
			Destroy( this );
			return;
		}
		
		instance = this;
	}
	
	
	
	public void OnDestroy()
	{
		Editor.OnDestroy();
		instance = null;
	}
	
	
	
	public static PathEditor Instance
	{
		get
		{
			if( instance == null )
			{
				new PathEditor();
			}
			
			return instance;
		}
	}
	
	
	
	public void Init()
	{
		this.editor = PathLibrary.Editor.Instance;
	}
	
	
	
    public PathLibrary.Editor Editor
    {
        get
		{
			if( editor == null )
			{
				if( PathLibrary.Editor.Instance == null )
				{
					PathLibrary.Editor.Init( this );
				}
				
				Init();
			}
			
			return editor;
		}
    }
    


    public IPathAsset SelectedAsset
    {
        get
		{
			return Selection.activeObject as PathAsset;
		}
    }



	public void SaveCollection( CollectionAsset collectionAsset, IPathAsset pathAsset )
	{
		pathAsset.Data = collectionAsset.GetData();
		EditorUtility.SetDirty( ( PathAsset )pathAsset );
	}
	
	
	
	public string GetCollectionName( IPathAsset asset )
	{
		string name;
		
		name = AssetDatabase.GetAssetPath( ( PathAsset )asset );
		name = name.Substring( name.LastIndexOf( "/" ) + 1 );
		name = name.Substring( 0, name.LastIndexOf( "." ) );
		
		return name;
	}


	
	public void UpdateScene()
	{
		Object[] controls;
		
		HandleUtility.Repaint();
		EditorUtility.SetDirty( this );

		controls = FindObjectsOfType( typeof( PathControl ) );
		foreach( PathControl control in controls )
		{
			EditorUtility.SetDirty( control );
		}
		
		if( SceneView.current != null )
		{
			SceneView.RepaintAll();
		}
	}
	
	
	
	public void Update()
	{
		Editor.Update();
	}
}
