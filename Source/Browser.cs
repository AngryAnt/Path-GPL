/*
    Path - pathfinding system for the Unity engine

    Copyright (C) 2008 Emil E. Johansen

    This file is part of Path.

    Path is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 2 of the License, or
    (at your option) any later version.

    Path is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Path.  If not, see <http://www.gnu.org/licenses/>.

    For alternative license options, contact the copyright holder.

    Emil E. Johansen emil@eej.dk
*/


using UnityEngine;
using UnityEditor;
using System.Collections;

namespace PathLibrary
{
	public interface IBrowserWindow
	{
        Rect Position
        {
            get;
            set;
        }
        
        Browser Browser
        {
            get;
        }

		bool HasFocus
		{
			get;
		}
        
        void Repaint();
        void Close();
        void Show();
		void Focus();
	}

	
	public class Browser
	{
        private static Browser instance;
        
        
        
        public static void Init( IBrowserWindow browserWindow )
        {
			if( instance != null )
			{
				Debug.LogError( "Browser::Init: Instance already exists. Updating interface connection in stead." );
				instance.browserWindow = browserWindow;
				return;
			}
			
			new Browser( browserWindow );
        }
        

        
        public static Browser Instance
        {
            get
            {
                return instance;
            }
        }



		private IBrowserWindow browserWindow;
		private bool visible;
		private Vector2 scrollPosition;
		public enum ItemType{ Network, Node, Connection, Triangle, Tag };
		private ItemType currentHighlight;
		private bool editing, tags, selectionUpdate;
		private string editString;
		private Rect scrollTo;
		
		
		
		public Browser( IBrowserWindow browserWindow )
		{
			this.browserWindow = browserWindow;
			visible = false;			
			instance = this;
		}
		
		
		
		public void OnDestroy()
		{
			instance = null;
		}
		
		
		
		public bool Visible
		{
			get
			{
				return visible;
			}
		}
		
		
		
		public bool Editing
		{
			get
			{
				return editing;
			}
			set
			{
				if( !editing && value )
				// Start of editing
				{
					if( CurrentHighlight == ItemType.Network && Editor.Instance.SelectedNetwork != null )
					{
						editString = Editor.Instance.SelectedNetwork.Name;
					}
					else if( CurrentHighlight == ItemType.Node && Editor.Instance.SelectedNode.Count == 1 )
					{
						editString = ( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Name;
					}
					else if( CurrentHighlight == ItemType.Tag && Editor.Instance.SelectedTag != null )
					{
						editString = Editor.Instance.SelectedTag;
					}
				}
				else if( editing && !value && editString != null && editString != "" )
				// End of editing
				{
					if( CurrentHighlight == ItemType.Network && Editor.Instance.SelectedNetwork != null )
					{
						Editor.Instance.SelectedNetwork.Name = editString;
					}
					else if( CurrentHighlight == ItemType.Node && Editor.Instance.SelectedNode.Count == 1 )
					{
						( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Name = editString;
					}
					else if( CurrentHighlight == ItemType.Tag && Editor.Instance.SelectedTag != null && System.Array.IndexOf( Editor.Instance.Collection.Tags, editString ) == -1 )
					{
						Editor.Instance.Collection.Tags[ System.Array.IndexOf( Editor.Instance.Collection.Tags, Editor.Instance.SelectedTag ) ] = editString;
						Editor.Instance.SelectedTag = editString;
					}
					
					Editor.Instance.SaveCollection();
				}
				
				editing = value;
				Repaint();
			}
		}
		
		
		
		public bool Tags
		{
			get
			{
				return tags;
			}
			set
			{
				tags = value;
				Repaint();
			}
		}
		
		
		
		public ItemType CurrentHighlight
		{
			get
			{
				return currentHighlight;
			}
			set
			{
				currentHighlight = value;
				Repaint();
			}
		}
		
		
		
		public bool SelectionUpdate
		{
			get
			{
				return selectionUpdate;
			}
			set
			{
				selectionUpdate = value;
			}
		}
		
		
		
		public Rect ScrollTo
		{
			get
			{
				return scrollTo;
			}
			set
			{
				scrollTo = value;
			}
		}
		
		
		
		public void Repaint()
		{
			browserWindow.Repaint();
		}
		
		
		
        public void Show()
		{
			scrollPosition = new Vector2( 0, 0 );
			editing = false;
			tags = false;
			currentHighlight = ItemType.Network;
			
			visible = true;
			browserWindow.Show();
		}
		
		
		
		public void Focus()
		{
			browserWindow.Focus();
		}
		
		
		
		public void HandleShortcuts()
		{
			bool moveUp, moveDown, moveHigher, moveLower, toggleEditing;
			ArrayList sortedNodes = null;
			
			moveUp = Event.current.keyCode == KeyCode.UpArrow;
			moveDown = Event.current.keyCode == KeyCode.DownArrow;
			moveHigher = Event.current.keyCode == KeyCode.LeftArrow;
			moveLower = Event.current.keyCode == KeyCode.RightArrow;
			toggleEditing = Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter;
			
			if( Event.current.type != EventType.KeyDown || ( !moveUp && !moveDown && !moveHigher && !moveLower && !toggleEditing ) )
			{
				return;
			}
			
			if( toggleEditing && CurrentHighlight != ItemType.Connection )
			{
				Editing = !Editing;
			}
			
			if( Editor.Instance.SelectedNetwork != null )
			{
				sortedNodes = new ArrayList( Editor.Instance.SelectedNetwork.Nodes );
				sortedNodes.Sort();
			}
			
			switch( CurrentHighlight )
			{
				case ItemType.Tag:
					if( moveHigher || moveLower )
					{
						return;
					}
				
					if( ( moveUp && System.Array.IndexOf( Editor.Instance.Collection.Tags, Editor.Instance.SelectedTag ) > 0 ) ||
						( moveDown && Editor.Instance.Collection.Tags.Length > ( System.Array.IndexOf( Editor.Instance.Collection.Tags, Editor.Instance.SelectedTag ) + 1 ) ) )
					{
						Editor.Instance.SelectedTag = Editor.Instance.Collection.Tags[ System.Array.IndexOf( Editor.Instance.Collection.Tags, Editor.Instance.SelectedTag ) + ( ( moveUp ) ? -1 : 1 ) ];
						SelectionUpdate = true;
					}
				break;
				case ItemType.Network:
					if( moveHigher || Editor.Instance.SelectedNetwork == null || Editor.Instance.SelectedNetwork.Nodes.Length == 0 )
					{
						return;
					}
				
					if( ( moveUp && System.Array.IndexOf( Editor.Instance.Collection.Networks, Editor.Instance.SelectedNetwork ) > 0 ) ||
						( moveDown && Editor.Instance.Collection.Networks.Length > ( System.Array.IndexOf( Editor.Instance.Collection.Networks, Editor.Instance.SelectedNetwork ) + 1 ) ) )
					{
						Editor.Instance.SelectedNetwork = Editor.Instance.Collection.Networks[ System.Array.IndexOf( Editor.Instance.Collection.Networks, Editor.Instance.SelectedNetwork ) + ( ( moveUp ) ? -1 : 1 ) ];
						SelectionUpdate = true;
					}
					else if( moveLower )
					{
						CurrentHighlight = ItemType.Node;
						Editor.Instance.SelectedNode.Clear();
						Editor.Instance.SelectedNode.Add( ( NetworkNodeAsset )sortedNodes[ 0 ] );
						SelectionUpdate = true;
					}
				break;
				case ItemType.Node:
					if( Editor.Instance.SelectedNode.Count != 1 || ( moveLower && Editor.Instance.SelectedNode[ 0 ] is CellAsset ) )
					{
						return;
					}
				
					if( ( moveUp && sortedNodes.IndexOf( Editor.Instance.SelectedNode[ 0 ] ) > 0 ) ||
						( moveDown && sortedNodes.Count > ( sortedNodes.IndexOf( Editor.Instance.SelectedNode[ 0 ] ) + 1 ) ) )
					{
						Editor.Instance.SelectedNode.Add( ( NetworkNodeAsset )sortedNodes[ sortedNodes.IndexOf( Editor.Instance.SelectedNode[ 0 ] ) + ( ( moveUp ) ? -1 : 1 ) ] );
						if( !Event.current.shift )
						{
							Editor.Instance.SelectedNode.RemoveAt( 0 );
						}
						SelectionUpdate = true;
					}
					else if( moveHigher )
					{
						CurrentHighlight = ItemType.Network;
						Editor.Instance.SelectedNode.Clear();
						SelectionUpdate = true;
					}
					else if( moveLower && ( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Connections.Length != 0 )
					{
						CurrentHighlight = ItemType.Connection;
						Editor.Instance.SelectedConnection = ( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Connections[ 0 ];
						SelectionUpdate = true;
					}
				break;
				case ItemType.Connection:
					if( moveLower )
					{
						return;
					}
				
					if( ( moveUp && System.Array.IndexOf( ( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Connections, Editor.Instance.SelectedConnection ) > 0 ) ||
						( moveDown && ( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Connections.Length > ( System.Array.IndexOf( ( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Connections, Editor.Instance.SelectedConnection ) + 1 ) ) )
					{
						Editor.Instance.SelectedConnection = ( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Connections[ System.Array.IndexOf( ( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Connections, Editor.Instance.SelectedConnection ) + ( ( moveUp ) ? -1 : 1 ) ];
						SelectionUpdate = true;
					}
					else if( moveHigher )
					{
						CurrentHighlight = ItemType.Node;
						Editor.Instance.SelectedConnection = null;
						SelectionUpdate = true;
					}
				break;
			}
			
			Event.current.Use();
		}
		
		
		
		public void OnGUI()
		{
//			WaypointAsset waypoint;
			bool newTags;
			
			SelectionUpdate = false;
			ScrollTo = new Rect( 0.0f, 0.0f, 0.0f, 0.0f );
			
			newTags = tags;
			
			GUI.DrawTexture( new Rect( browserWindow.Position.width - 270, browserWindow.Position.height - 275, 250, 250 ), Resources.Logo );
//			GUI.DrawTexture( new Rect( browserWindow.Position.width - 20, browserWindow.Position.height - 20, 15, 15 ), Resources.Scale );
			
			if( Editor.Instance != null && Editor.Instance.Asset != null )
			{
				if( browserWindow.HasFocus )
				{
					HandleShortcuts();
				}
				
				GUILayout.BeginHorizontal( "Toolbar" );
					if( !tags )
					// The network browser toolbar
					{
						// Create menu //
						if( GUILayout.Button( "Create", Resources.ToolbarPullDownUpStyle ) )
						{
							EditorUtility.DisplayPopupMenu( new Rect( GUILayoutUtility.GetLastRect().x + 5, GUILayoutUtility.GetLastRect().y + 8, 0, 0 ), "Assets/Path/Create/", null );
						}
						
						// Duplicate button //
						if( (
							( currentHighlight == ItemType.Network && Editor.Instance.SelectedNetwork != null && ( Editor.Instance.SelectedNetwork is NavmeshAsset || Editor.Instance.SelectedNetwork is WaypointNetworkAsset || Editor.Instance.SelectedNetwork is GridNetworkAsset ) ) ||
							( currentHighlight == ItemType.Node && Editor.Instance.SelectedNode.Count > 0 && ( Editor.Instance.SelectedNode[ 0 ] is WaypointAsset || Editor.Instance.SelectedNode[ 0 ] is GridNodeAsset ) )
							) &&
							GUILayout.Button( "Duplicate", "ToolbarButton" ) )
						{
							switch( currentHighlight )
							{
								case ItemType.Network:
									if( Editor.Instance.SelectedNetwork is NavmeshAsset )
									{
										Editor.Instance.DuplicateNavmesh();
									}
									else if( Editor.Instance.SelectedNetwork is WaypointNetworkAsset )
									{
										Editor.Instance.DuplicateWaypointNetwork();
									}
									else if( Editor.Instance.SelectedNetwork is GridNetworkAsset )
									{
										Editor.Instance.DuplicateGridNetwork();
									}
								break;
								case ItemType.Node:
									if( Editor.Instance.SelectedNode.Count > 0 && Editor.Instance.SelectedNode[ 0 ] is WaypointAsset )
									{
										Editor.Instance.DuplicateWaypoint();
									}
									else if( Editor.Instance.SelectedNode.Count > 0 && Editor.Instance.SelectedNode[ 0 ] is GridNodeAsset )
									{
										Editor.Instance.DuplicateGridNode();
									}
								break;
							}
						}
						
						// Rename toggle //
						if( ( Editor.Instance.SelectedNetwork != null || Editor.Instance.SelectedNode.Count != 0 || Editor.Instance.SelectedConnection != null ) &&
							CurrentHighlight != ItemType.Tag )
						{
							Editing = GUILayout.Toggle( Editing, "Rename", "ToolbarButton" );
						}
						
						// Delete button //
						if( Editor.Instance.SelectedNetwork == null && ( Editor.Instance.SelectedNode.Count == 0 || !( Editor.Instance.SelectedNode[ 0 ] is WaypointAsset ) ) && Editor.Instance.SelectedConnection == null )
						{
							//GUILayout.Label( "No selection", GUI.skin.GetStyle( "Button" ) );
						}
						else if(
							(
								( currentHighlight == ItemType.Network && Editor.Instance.SelectedNetwork != null ) ||
								( currentHighlight == ItemType.Node && Editor.Instance.SelectedNode.Count == 1 && ( Editor.Instance.SelectedNode[ 0 ] is WaypointAsset || Editor.Instance.SelectedNode[ 0 ] is GridNodeAsset ) )
							) && GUILayout.Button( "Delete", "ToolbarButton" ) )
						{
							switch( currentHighlight )
							{
								case ItemType.Network:
									if( EditorUtility.DisplayDialog( "Delete network?", "Are you certain that you wish to delete the network '" + Editor.Instance.SelectedNetwork.ToString() + "' and all its nodes and connections?\n\nWARNING: This cannot be undone.", "Delete", "Cancel" ) )
									{
										Editor.Instance.Collection.Remove( Editor.Instance.SelectedNetwork );
										Editor.Instance.SelectedNetwork = null;
										Editor.Instance.SaveCollection();
									}
								break;
								case ItemType.Node:
									if( EditorUtility.DisplayDialog( "Delete node?", "Are you certain that you wish to delete the node '" + /*( NetworkNodeAsset )*/Editor.Instance.SelectedNode[ 0 ].ToString() + "' and all its connections?\n\nWARNING: This cannot be undone.", "Delete", "Cancel" ) )
									{
										if( Editor.Instance.SelectedNode[ 0 ] is WaypointAsset )
										{
											( Editor.Instance.SelectedNetwork as WaypointNetworkAsset ).Remove( ( WaypointAsset )Editor.Instance.SelectedNode[ 0 ] );
											Editor.Instance.SelectedNode.Remove( ( WaypointAsset )Editor.Instance.SelectedNode[ 0 ] );
										}
										else if( Editor.Instance.SelectedNode[ 0 ] is GridNodeAsset )
										{
											( Editor.Instance.SelectedNetwork as GridNetworkAsset ).Remove( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] );
											Editor.Instance.SelectedNode.Remove( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] );
										}
										Editor.Instance.SaveCollection();
									}
								break;
								case ItemType.Connection:
									if( EditorUtility.DisplayDialog( "Delete connection?", "Are you certain that you wish to delete the connection '" + Editor.Instance.SelectedConnection.ToString() + "'?\n\nWARNING: This cannot be undone.", "Delete", "Cancel" ) )
									{
										Editor.Instance.SelectedConnection.From.RemoveConnection( Editor.Instance.SelectedConnection );
										Editor.Instance.SelectedConnection = null;
										Editor.Instance.SaveCollection();
									}
								break;
							}
						}
					}
					else
					// The tag browser toolbar
					{
						if( GUILayout.Button( "Add", "ToolbarButton" ) )
						{
							Editor.Instance.CreateTag();
						}
						
						if( Editor.Instance.SelectedTag != null && CurrentHighlight == ItemType.Tag )
						{
							Editing = GUILayout.Toggle( Editing, "Rename", "ToolbarButton" );
						}
						
						if( Editor.Instance.SelectedTag != null && CurrentHighlight == ItemType.Tag && GUILayout.Button( "Delete", "ToolbarButton" ) )
						{
							Editor.Instance.Collection.RemoveTag( Editor.Instance.SelectedTag );
						}
					}
					
					GUILayout.FlexibleSpace();
					
					newTags = GUILayout.Toggle( tags, Resources.Tag, "ToolbarButton" );
				GUILayout.EndHorizontal();
				
				GUILayout.BeginVertical( Resources.TitleStyle );
				
					GUILayout.Label( Editor.Instance.CollectionName );
				
				GUILayout.EndVertical();
				
				scrollPosition = GUILayout.BeginScrollView( scrollPosition );
				
				if( !tags )
				{
					OnStandardGUI();
				}
				else
				{
					if( Editor.Instance.Collection.Tags.Length == 0 )
					{
						GUILayout.Label( "No tags in this collection" );
					}
					else
					{
						Editor.Instance.SelectedTag = Resources.SelectList( Editor.Instance.Collection.Tags, Editor.Instance.SelectedTag, OnTagListItemGUI ) as string;
					}
				}
				
				GUILayout.EndScrollView();
				
				if( SelectionUpdate )//!( ScrollTo.x == 0.0f && ScrollTo.y == 0.0f && ScrollTo.width == 0.0f && ScrollTo.height == 0.0f ) )
				{
					// TODO: FixScrollTo.
					//GUI.ScrollTo( ScrollTo );
					/*
					Rect rect = new Rect( 0.0f, 0.0f, 0.0f, 0.0f );
					
					GUI.FocusControl( "Highlight" );
					GUIUtility.GetControlID( FocusType.Passive, rect );
					if( rect != null )
					{
						GUI.ScrollTo( rect );
					}*/
				}
				
				if( SelectionUpdate || GUI.changed )
				{
					Editor.Instance.UpdateScene();
					SelectionUpdate = false;
				}
			}
			else
			{
				GUILayout.BeginVertical( Resources.TitleStyle );
					GUILayout.Label( "No library loaded" );
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}
			
			// Version notice //
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label( Resources.Version );
			GUILayout.EndHorizontal();
			
			tags = newTags;
		}
		
		
		
		public void OnStandardGUI()
		{
			if( Editor.Instance.Collection.Networks.Length > 0 )
			{
				Editor.Instance.SelectedNetwork = Resources.SelectList( Editor.Instance.Collection.Networks, Editor.Instance.SelectedNetwork, OnNeworkListItemGUI ) as NetworkAsset;
			}
			else
			{
				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
						GUILayout.Label( "Library is empty" );
					GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
		}
		
		// TODO: Find out why the SelectLists are so CPU hungry
		
		private bool OnNeworkListItemGUI( object item, bool selected, ICollection list )
		{
			NetworkAsset networkAsset;
			ArrayList sortedNodeList;
			
			networkAsset = ( NetworkAsset )item;
			sortedNodeList = new ArrayList( networkAsset.Nodes );
			sortedNodeList.Sort();
			
			if( selected && CurrentHighlight == ItemType.Network )
			{
				GUI.SetNextControlName( "Highlight" );
			}
			
			GUILayout.BeginHorizontal( ( selected && CurrentHighlight == ItemType.Network ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListItemStyle : Resources.UnfocusedSelectedListItemStyle ) : Resources.ListItemStyle );
				if( Editing && selected && CurrentHighlight == ItemType.Network )
				{
					GUILayout.Label( Resources.Expanded, ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ), GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) );
					GUILayout.Label( Resources.Network, ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ), GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) );
					editString = GUILayout.TextField( editString, /*( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ), */GUILayout.Height( Resources.DefaultListItemHeight ) );
				}
				else if( GUILayout.Button( selected ? Resources.Expanded : Resources.Collapsed, ( selected && CurrentHighlight == ItemType.Network ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) ||
					GUILayout.Button( Resources.Network, ( selected && CurrentHighlight == ItemType.Network ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) ||
					GUILayout.Button( item.ToString() + " (" + networkAsset.Nodes.Length + " nodes)", ( selected && CurrentHighlight == ItemType.Network ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Height( Resources.DefaultListItemHeight ) ) )
				{
					Editing = false;
					selected = !selected;
					
					CurrentHighlight = ItemType.Network;
					
					Editor.Instance.SelectedNode.Clear();
					Editor.Instance.SelectedConnection = null;
					
					SelectionUpdate = true;
				}
				/*else if( SelectionUpdate && selected && CurrentHighlight == ItemType.Network )
				{
					ScrollTo = GUILayoutUtility.GetLastRect();
				}*/
			GUILayout.EndHorizontal();
			
			
			if( selected )
			{					
				GUILayout.Space( 1.0f );

				if( ( item as NetworkAsset ).Nodes.Length > 0 )
				{
					if( item is WaypointNetworkAsset )
					{
						Editor.Instance.SelectedNode = new ArrayList( Resources.SelectList( sortedNodeList, Editor.Instance.SelectedNode, networkAsset.Nodes.Length, OnWaypointListItemGUI ) );//INetworkNodeAsset;
					}
					else if( item is NavmeshAsset )
					{
						Editor.Instance.SelectedNode = new ArrayList( Resources.SelectList( sortedNodeList, Editor.Instance.SelectedNode, networkAsset.Nodes.Length, OnCellListItemGUI ) );//INetworkNodeAsset;
					}
					else if( item is GridNetworkAsset )
					{
						Editor.Instance.SelectedNode = new ArrayList( Resources.SelectList( sortedNodeList, Editor.Instance.SelectedNode, networkAsset.Nodes.Length, OnGridNodeListItemGUI ) );//INetworkNodeAsset;
					}
				}
				else
				{
					GUILayout.Label( "No nodes" );
				}

				GUILayout.Space( 1.0f );
			}
			
			return selected;
		}
		
		
		
		private bool OnWaypointListItemGUI( object item, bool selected, ICollection list )
		{
			if( selected && CurrentHighlight == ItemType.Node )
			{
				GUI.SetNextControlName( "Highlight" );
			}
			
			GUILayout.BeginHorizontal( ( selected && CurrentHighlight == ItemType.Node ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListItemStyle : Resources.UnfocusedSelectedListItemStyle ) : Resources.ListItemStyle );
				GUILayout.Space( 20 );
				if( Editing && selected && CurrentHighlight == ItemType.Node )
				{
					GUILayout.Label( ( selected && Editor.Instance.SelectedNode.Count == 1 ) ? Resources.Expanded : Resources.Collapsed, ( selected && CurrentHighlight == ItemType.Node ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) );
					GUILayout.Label( Resources.Waypoint, ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ), GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) );
					editString = GUILayout.TextField( editString, GUILayout.Height( Resources.DefaultListItemHeight ) );
				}
				else if( GUILayout.Button( ( selected && Editor.Instance.SelectedNode.Count == 1 ) ? Resources.Expanded : Resources.Collapsed, ( selected && CurrentHighlight == ItemType.Node ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) ||
					GUILayout.Button( Resources.Waypoint, ( selected && CurrentHighlight == ItemType.Node ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) ||
					GUILayout.Button( item.ToString(), ( selected && CurrentHighlight == ItemType.Node ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Height( Resources.DefaultListItemHeight ) ) )
				{
					Editing = false;
					selected = !selected;
					
					CurrentHighlight = ( !selected && Editor.Instance.SelectedNode.Count == 1 ) ? ItemType.Network : ItemType.Node;
					
					Editor.Instance.SelectedConnection = null;
					
					SelectionUpdate = true;
				}
				/*else if( SelectionUpdate && selected && CurrentHighlight == ItemType.Node )
				{
					ScrollTo = GUILayoutUtility.GetLastRect();
				}*/
			GUILayout.EndHorizontal();
				
			if( selected && Editor.Instance.SelectedNode.Count == 1 && ( item as NetworkNodeAsset ).Connections.Length > 0 )
			{
				GUILayout.Space( 1.0f );
				Editor.Instance.SelectedConnection = Resources.SelectList( ( item as NetworkNodeAsset ).Connections, Editor.Instance.SelectedConnection, OnConnectionListItemGUI ) as ConnectionAsset;
			}
			
			return selected;
		}
		
		
		
		private bool OnCellListItemGUI( object item, bool selected, ICollection list )
		{
			if( selected && CurrentHighlight == ItemType.Node )
			{
				GUI.SetNextControlName( "Highlight" );
			}
			
			GUILayout.BeginHorizontal( ( selected && CurrentHighlight == ItemType.Node ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListItemStyle : Resources.UnfocusedSelectedListItemStyle ) : Resources.ListItemStyle );
				GUILayout.Space( 20 );
				if( editing && selected && currentHighlight == ItemType.Node )
				{
					GUILayout.Label( Resources.Waypoint, ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ), GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) );
					editString = GUILayout.TextField( editString, GUILayout.Height( Resources.DefaultListItemHeight ) );
				}
				else if( GUILayout.Button( Resources.Waypoint, ( selected && CurrentHighlight == ItemType.Node ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) ||
					GUILayout.Button( item.ToString() + " (" + ( item as CellAsset ).Triangles.Length + " triangles)", ( selected && CurrentHighlight == ItemType.Node ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Height( Resources.DefaultListItemHeight ) ) )
				{
					Editing = false;
					selected = !selected;
					
					CurrentHighlight = ( !selected && Editor.Instance.SelectedNode.Count == 1 ) ? ItemType.Network : ItemType.Node;
					
					SelectionUpdate = true;
				}
				/*else if( SelectionUpdate && selected && CurrentHighlight == ItemType.Node )
				{
					ScrollTo = GUILayoutUtility.GetLastRect();
				}*/
			GUILayout.EndHorizontal();
			
			return selected;
		}
		
		
		
		private bool OnGridNodeListItemGUI( object item, bool selected, ICollection list )
		{
			if( selected && CurrentHighlight == ItemType.Node )
			{
				GUI.SetNextControlName( "Highlight" );
			}
			
			GUILayout.BeginHorizontal( ( selected && CurrentHighlight == ItemType.Node ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListItemStyle : Resources.UnfocusedSelectedListItemStyle ) : Resources.ListItemStyle );
				GUILayout.Space( 20 );
				if( Editing && selected && CurrentHighlight == ItemType.Node )
				{
					GUILayout.Label( ( selected && Editor.Instance.SelectedNode.Count == 1 ) ? Resources.Expanded : Resources.Collapsed, ( selected && CurrentHighlight == ItemType.Node ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) );
					GUILayout.Label( Resources.Waypoint, ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ), GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) );
					editString = GUILayout.TextField( editString, GUILayout.Height( Resources.DefaultListItemHeight ) );
				}
				else if( GUILayout.Button( ( selected && Editor.Instance.SelectedNode.Count == 1 ) ? Resources.Expanded : Resources.Collapsed, ( selected && CurrentHighlight == ItemType.Node ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) ||
					GUILayout.Button( Resources.Waypoint, ( selected && CurrentHighlight == ItemType.Node ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) ||
					GUILayout.Button( item.ToString(), ( selected && CurrentHighlight == ItemType.Node ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Height( Resources.DefaultListItemHeight ) ) )
				{
					Editing = false;
					selected = !selected;
					
					CurrentHighlight = ( !selected && Editor.Instance.SelectedNode.Count == 1 ) ? ItemType.Network : ItemType.Node;
					
					Editor.Instance.SelectedConnection = null;
					
					SelectionUpdate = true;
				}
				/*else if( SelectionUpdate && selected && CurrentHighlight == ItemType.Node )
				{
					ScrollTo = GUILayoutUtility.GetLastRect();
				}*/
			GUILayout.EndHorizontal();
			
			if( selected && Editor.Instance.SelectedNode.Count == 1 && ( item as NetworkNodeAsset ).Connections.Length > 0 )
			{
				GUILayout.Space( 1.0f );
				Editor.Instance.SelectedConnection = Resources.SelectList( ( item as NetworkNodeAsset ).Connections, Editor.Instance.SelectedConnection, OnConnectionListItemGUI ) as ConnectionAsset;
			}
			
			return selected;
		}
		
		
		
		private bool OnConnectionListItemGUI( object item, bool selected, ICollection list )
		{
			ConnectionAsset connection;
			bool result;
			
			connection = item as ConnectionAsset;
			
			result = selected;
			
			if( selected && CurrentHighlight == ItemType.Connection )
			{
				GUI.SetNextControlName( "Highlight" );
			}
			
			GUILayout.BeginHorizontal( ( selected && CurrentHighlight == ItemType.Connection ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListItemStyle : Resources.UnfocusedSelectedListItemStyle ) : Resources.ListItemStyle );
				GUILayout.Space( 55 );
				if( GUILayout.Button( Resources.Connection, ( selected && CurrentHighlight == ItemType.Connection ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) ||
					GUILayout.Button( connection.ToString(), ( selected && CurrentHighlight == ItemType.Connection ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Height( Resources.DefaultListItemHeight ) ) )
				{
					Editing = false;
					result = !result;
					CurrentHighlight = ( result ) ? ItemType.Connection : ItemType.Node;
					
					SelectionUpdate = true;
				}
				else if( SelectionUpdate && selected && CurrentHighlight == ItemType.Connection )
				{
					ScrollTo = GUILayoutUtility.GetLastRect();
				}
			GUILayout.EndHorizontal();
			
			return result;
		}



		private bool OnTagListItemGUI( object item, bool selected, ICollection list )
		{
			string tag;
			bool result;
			
			if( item == null )
			{
				return false;
			}

			tag = item as string;

			result = selected;

			GUILayout.BeginHorizontal( ( selected && CurrentHighlight == ItemType.Tag ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListItemStyle : Resources.UnfocusedSelectedListItemStyle ) : Resources.ListItemStyle );
				if( Editing && selected && CurrentHighlight == ItemType.Tag )
				{
					GUILayout.Label( Resources.Tag, ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ), GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) );
					editString = GUILayout.TextField( editString, GUILayout.Height( Resources.DefaultListItemHeight ) );
				}
				else if( GUILayout.Button( Resources.Tag, ( selected && CurrentHighlight == ItemType.Tag ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) ||
					GUILayout.Button( tag, ( selected && CurrentHighlight == ItemType.Tag ) ? ( ( browserWindow.HasFocus ) ? Resources.SelectedListStyle : Resources.UnfocusedSelectedListStyle ) : Resources.ListStyle, GUILayout.Height( Resources.DefaultListItemHeight ) ) )
				{
					Editing = false;
					result = !result;
					CurrentHighlight = ItemType.Tag;
				}
				else if( SelectionUpdate && selected && CurrentHighlight == ItemType.Tag )
				{
					ScrollTo = GUILayoutUtility.GetLastRect();
				}
			GUILayout.EndHorizontal();

			return result;
		}
	}
}
