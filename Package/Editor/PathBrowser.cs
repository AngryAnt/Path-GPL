using UnityEngine;
using UnityEditor;
using System.Collections;
using PathLibrary;

public class PathBrowser : EditorWindow, IBrowserWindow
{
	private static PathBrowser instance;
	
	
	public PathBrowser()
	{
		if( instance != null )
		{
			Debug.LogError( "Trying to create two instances of singleton. Self destruction in 3..." );
			Destroy( this );
			return;
		}
		
		PathLibrary.Browser.Init( this );
		
		instance = this;
		
		title = "Path browser";
	}
	
	
	
	public void OnDestroy()
	{
		instance = null;
		PathLibrary.Browser.Instance.OnDestroy();
	}
	
	
	
	public static PathBrowser Instance
	{
		get
		{
			if( instance == null )
			{
				new PathBrowser();
			}
			
			return instance;
		}
	}
	
	
	
    public Rect Position
    {
        get
		{
			return position;
		}
        set
		{
			this.position = value;
		}
    }
    


    public PathLibrary.Browser Browser
    {
        get
		{
			return PathLibrary.Browser.Instance;
		}
    }
    


    new public void Repaint()
	{
		base.Repaint();
	}
	
	
	
    new public void Close()
	{
		base.Close();
	}
	
	
	
    new public void Show()
	{
		base.Show();
	}
	
	
	
	new public void Focus()
	{
		base.Focus();
	}
	
	
	
	public bool HasFocus
	{
		get
		{
			return EditorWindow.focusedWindow == this;
		}
	}
	
	
	
	public void OnGUI()
	{
		Browser.OnGUI();
	}
}
