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
	public interface IInspector
	{
		Transform Transform
		{
			get;
		}
	}
	
	
	
	public class Inspector
	{
        private static Inspector instance;


        
        public static Inspector Instance
        {
            get
            {
				if( instance == null )
				{
					new Inspector();
				}
                return instance;
            }
        }



		private bool showNetwork, showNode, showConnection, networkTags, nodeTags, connectionTags, bidirectionalConnect;
		private Mesh meshReference;
		private int selectedNetworkTag, selectedNodeTag, selectedConnectionTag;
		private float connectionWidthNetwork, connectionWidthNode;
		private LayerMask blockingLayers;
		
		
		
		public Inspector()
		{
			if( instance != null )
			{
				Debug.LogError( "Creating extra instance of singleton Inspector" );
				return;
			}

			instance = this;
			blockingLayers = LayerMask.NameToLayer( "Default" );
			
			meshReference = null;
			showNetwork = true;
			showNode = true;
			showConnection = true;
			networkTags = false;
			nodeTags = false;
			connectionTags = false;
			bidirectionalConnect = false;
			blockingLayers = -1;

			connectionWidthNetwork = 1.0f;			
			connectionWidthNode = 1.0f;
			
			instance = this;
		}
		


		public void OnGUI( IInspector inspector )
		{
			if( Editor.Instance != null && Editor.Instance.Asset != null )
			{
				// Selected network properties sheet and functions
				if( Editor.Instance.SelectedNetwork != null )
				{
					OnNetworkGUI( inspector );
					
					// Selected node properties sheet and functions
					if( Editor.Instance.SelectedNode.Count > 0 )
					{
						OnNodeGUI( inspector );
						
						// Selected connection properties sheet and functions
						if( Editor.Instance.SelectedConnection != null )
						{
							OnConnectionGUI( inspector );
						}
					}
				}
				else
				{
					GUILayout.Space( 30.0f );
					GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						GUILayout.Label( "No PathBrowser selection." );
						GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				
				if( GUI.changed )
				{
					Editor.Instance.UpdateScene();
				}
			}

			GUILayout.Space( 30.0f );

			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label( Resources.Logo, GUILayout.Width( 20.0f ), GUILayout.Height( 20.0f ) );
				GUILayout.BeginVertical();
					GUILayout.Space( 2.0f );
					GUILayout.Label( Resources.Version );
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		
		
		
		public void OnNetworkGUI( IInspector inspector )
		{
			Vector3 newPosition, newSize;
			Texture2D[] tagsToggle = new Texture2D[ 2 ];
			
			tagsToggle[ 1 ] = Resources.Tag;
			
			GUILayout.Space( 30.0f );
			
			GUILayout.BeginHorizontal();
				GUILayout.Space( 15.0f );
				
				if( GUILayout.Button( ( showNetwork ) ? Resources.Expanded : Resources.Collapsed, Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) )
				{
					showNetwork = !showNetwork;
				}
				
				if( GUILayout.Toggle( Editor.Instance.SelectedNetwork.Enabled, "", GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) != Editor.Instance.SelectedNetwork.Enabled )
				{
					Editor.Instance.SelectedNetwork.Enabled = !Editor.Instance.SelectedNetwork.Enabled;
					Editor.Instance.SaveCollection();
				}
				
				tagsToggle[ 0 ] = Resources.Network;
				networkTags = ( GUILayout.Toolbar( networkTags ? 1 : 0, tagsToggle, GUILayout.Height( 20.0f ) ) == 1 );
								
//				GUILayout.Label( Resources.Network, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) );
				
				GUILayout.Label( Editor.Instance.SelectedNetwork.Name + ( ( Editor.Instance.SelectedNetwork is WaypointNetworkAsset ) ? " (waypoint network)" : ( ( Editor.Instance.SelectedNetwork is GridNetworkAsset ) ? " (grid network)" : " (navmesh)" ) ), GUILayout.MinWidth( 20.0f ) );
				GUILayout.FlexibleSpace();
				
				if( GUILayout.Button( Resources.Help, GUI.skin.GetStyle( "Label" ) ) )
				{
					Resources.Documentation( "Network" );
				}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
				GUILayout.Space( 15.0f );
				GUILayout.BeginVertical();
					if( showNetwork && !networkTags )
					{			
						newPosition = ( Editor.Instance.SelectedNetwork is GridNetworkAsset ) ? Editor.Instance.SelectedNetwork.Position : EditorGUILayout.Vector3Field( "Position", Editor.Instance.SelectedNetwork.Position );
						newSize = ( Editor.Instance.SelectedNetwork is GridNetworkAsset ) ? Editor.Instance.SelectedNetwork.Position : EditorGUILayout.Vector3Field( "Size", Editor.Instance.SelectedNetwork.Size );
						
						if( newPosition != Editor.Instance.SelectedNetwork.Position || newSize != Editor.Instance.SelectedNetwork.Size )
						{
							Editor.Instance.SelectedNetwork.Position = newPosition;
							Editor.Instance.SelectedNetwork.Size = newSize;
							Editor.Instance.SaveCollection();
						}
			
						if( Editor.Instance.SelectedNetwork is WaypointNetworkAsset )
						{
							if( Editor.Instance.SelectedNetwork.Nodes.Length > 1 )
							{
								GUILayout.BeginHorizontal();									
									if( GUILayout.Button( "Auto-connect", GUILayout.Width( Resources.DefaultLeftColumnWidth ) ) )
									{
										( ( WaypointNetworkAsset )Editor.Instance.SelectedNetwork ).AutoConnect( inspector.Transform, ( blockingLayers == -1 ) ? -1 : 1 << blockingLayers.value, connectionWidthNetwork );
										Editor.Instance.UpdateScene();
										Editor.Instance.SaveCollection();
									}
									
									GUILayout.BeginVertical();
										blockingLayers = Resources.LayerMaskField( blockingLayers );
										connectionWidthNetwork = EditorGUILayout.FloatField( "Connection width", connectionWidthNetwork );
									GUILayout.EndVertical();
								GUILayout.EndHorizontal();
								
								GUILayout.BeginHorizontal();
									GUILayout.BeginHorizontal( GUILayout.Width( Resources.DefaultLeftColumnWidth ) );
										GUILayout.FlexibleSpace();
										GUILayout.Label( "Connections" );
									GUILayout.EndHorizontal();
									if( GUILayout.Button( "Disconnect" ) &&
										EditorUtility.DisplayDialog( "Disconnect network?", "Are you sure that you wish to disconnect all waypoints in this network? All information on the existing connections, including tags will be lost.\n\nWARNING: This cannot be undone.", "Disconnect", "Cancel" ) )
									{
										( ( WaypointNetworkAsset )Editor.Instance.SelectedNetwork ).Disconnect();
										Editor.Instance.SaveCollection();
									}
								GUILayout.EndHorizontal();
							}
						}
						else if( Editor.Instance.SelectedNetwork is NavmeshAsset )
						{
							GUILayout.BeginHorizontal();
								meshReference = ( Mesh )EditorGUILayout.ObjectField( "Base mesh", meshReference, typeof( Mesh ) );
				
								if( meshReference != null )
								{
									if( GUILayout.Button( "Build" ) && ( Editor.Instance.SelectedNetwork.Nodes.Length == 0 || EditorUtility.DisplayDialog( "Rebuild?", "Are you certain that you wish to rebuild the navmesh '" + Editor.Instance.SelectedNetwork.ToString() + "'?\n\nWARNING: All current data in the navmesh will be lost. This cannot be undone.", "Rebuild", "Cancel" ) ) )
									{
										( ( NavmeshAsset )Editor.Instance.SelectedNetwork ).Generate( meshReference );
										Editor.Instance.SaveCollection();
									}
								}
							GUILayout.EndHorizontal();
						}
						else if( Editor.Instance.SelectedNetwork is GridNetworkAsset )
						{
							GUILayout.BeginHorizontal();
								GUILayout.BeginHorizontal( GUILayout.Width( Resources.DefaultLeftColumnWidth ) );
									GUILayout.FlexibleSpace();
									GUILayout.Label( "Connections" );
								GUILayout.EndHorizontal();
								if( GUILayout.Button( "Disconnect" ) &&
									EditorUtility.DisplayDialog( "Disconnect network?", "Are you sure that you wish to disconnect all grid nodes in this network? All information on the existing connections, including tags will be lost.\n\nWARNING: This cannot be undone.", "Disconnect", "Cancel" ) )
								{
									( ( GridNetworkAsset )Editor.Instance.SelectedNetwork ).Disconnect();
									Editor.Instance.UpdateScene();
									Editor.Instance.SaveCollection();
								}
							GUILayout.EndHorizontal();
						}
					}
					else if( showNetwork )
					{
						selectedNetworkTag = OnTagListGUI( Editor.Instance.SelectedNetwork, selectedNetworkTag );
					}
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		
		
		// TODO: In multiple node selections: Show a grid-node on the list in red if its target is null. Also don't connect it if its target is null.
		public void OnNodeGUI( IInspector inspector )
		{
			bool /*newBool, */askedForConnections, addToConnections, selectTarget = false;
			CellAsset mergedCell;
			string addedTag;
			int[] sharedTags;
			Vector3 newVector3;
			float newFloat;
			NetworkAsset targetNetwork;
			NetworkNodeAsset targetNode;
			Texture2D[] tagsToggle = new Texture2D[ 2 ];
			
			tagsToggle[ 1 ] = Resources.Tag;
			
			//GUILayout.Label( "", Resources.TextLineStyle, GUILayout.Height( 1 ) );
			GUILayout.Space( 30.0f );
			
			// Node properties //
			
			if( Editor.Instance.SelectedNode.Count == 1 )
			// Singular selection
			{
				GUILayout.BeginHorizontal();
					GUILayout.Space( 15.0f );
					if( GUILayout.Button( ( showNode ) ? Resources.Expanded : Resources.Collapsed, Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) )
					{
						showNode = !showNode;
					}
					
					if( GUILayout.Toggle( ( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Enabled, "", GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) != ( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Enabled )
					{
						( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Enabled = !( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Enabled;
						Editor.Instance.SaveCollection();
					}
					
					tagsToggle[ 0 ] = Resources.Waypoint;
					nodeTags = ( GUILayout.Toolbar( nodeTags ? 1 : 0, tagsToggle, GUILayout.Height( 20.0f ) ) == 1 );
					
					GUILayout.Label( ( ( NetworkNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Name + ( ( Editor.Instance.SelectedNode[ 0 ] is WaypointAsset ) ? " (waypoint)" : ( Editor.Instance.SelectedNode[ 0 ] is CellAsset ) ? " (navmesh cell)" : " (grid node)" ), GUILayout.MinWidth( 20.0f ) );
					GUILayout.FlexibleSpace();
					if( GUILayout.Button( Resources.Help, GUI.skin.GetStyle( "Label" ) ) )
					{
						Resources.Documentation( "Node" );
					}
				GUILayout.EndHorizontal();
			
				if( showNode && !nodeTags )
				{
					GUILayout.BeginHorizontal();
						GUILayout.Space( 15.0f );
						GUILayout.BeginVertical();
							if( Editor.Instance.SelectedNode[ 0 ] is WaypointAsset )
							// Waypoint specific properties
							{
								// Position
								newVector3 = EditorGUILayout.Vector3Field( "Position", ( Editor.Instance.SelectedNode[ 0 ] as WaypointAsset ).Position );
								if( ( Editor.Instance.SelectedNode[ 0 ] as WaypointAsset ).Position != newVector3 )
								{
									 Editor.Instance.SaveCollection();
								}
					
								/*
								// Positioner
								GUILayout.BeginHorizontal();
									GUILayout.BeginHorizontal( GUILayout.Width( Resources.DefaultLeftColumnWidth ) );
										GUILayout.FlexibleSpace();
										GUILayout.Label( "Positioner" );
									GUILayout.EndHorizontal();
					
									if( Editor.Instance.GetWaypointPositioner( Editor.Instance.SelectedNetwork.Name, ( Editor.Instance.SelectedNode[ 0 ] as WaypointAsset ).Name ) == null )
									{	
										if( GUILayout.Button( "Create" ) )
										{
											Editor.Instance.CreateWaypointPositioner( Editor.Instance.SelectedNetwork.Name, ( Editor.Instance.SelectedNode[ 0 ] as WaypointAsset ).Name, ( Editor.Instance.SelectedNode[ 0 ] as WaypointAsset ).Position );
										}
									}
									else
									{
										GUILayout.BeginVertical();
											if( GUILayout.Button( "Select" ) )
											{
												Editor.Instance.SelectWaypointPositioner( Editor.Instance.GetWaypointPositioner( Editor.Instance.SelectedNetwork.Name, ( Editor.Instance.SelectedNode[ 0 ] as WaypointAsset ).Name ) );
											}
					
											newBool = GUILayout.Toggle( ( Editor.Instance.SelectedNode[ 0 ] as WaypointAsset ).DisableRuntime, "Disable at runtime" );
											if( newBool != ( Editor.Instance.SelectedNode[ 0 ] as WaypointAsset ).DisableRuntime )
											{
												( Editor.Instance.SelectedNode[ 0 ] as WaypointAsset ).DisableRuntime = newBool;
												Editor.Instance.SaveCollection();
											}
										GUILayout.EndVertical();
									}
								GUILayout.EndHorizontal();*/
								
								
								// Radius
								newFloat = EditorGUILayout.FloatField( "Radius", ( Editor.Instance.SelectedNode[ 0 ] as WaypointAsset ).Radius );
								if( newFloat != ( ( WaypointAsset )Editor.Instance.SelectedNode[ 0 ] ).Radius )
								{
									( ( WaypointAsset )Editor.Instance.SelectedNode[ 0 ] ).Radius = newFloat;
									Editor.Instance.SaveCollection();
								}
								
								
								// Disconnect
								GUILayout.BeginHorizontal();
									GUILayout.BeginHorizontal( GUILayout.Width( Resources.DefaultLeftColumnWidth ) );
										GUILayout.FlexibleSpace();
										GUILayout.Label( "Connections" );
									GUILayout.EndHorizontal();
									if( GUILayout.Button( "Disconnect" ) )
									{
										( ( WaypointAsset )Editor.Instance.SelectedNode[ 0 ] ).Disconnect();
										Editor.Instance.UpdateScene();
										Editor.Instance.SaveCollection();
									}
								GUILayout.EndHorizontal();
							}
							else if( Editor.Instance.SelectedNode[ 0 ] is GridNodeAsset )
							// Grid node specific properties
							{
								GUILayout.BeginHorizontal();
									GUILayout.BeginHorizontal( GUILayout.Width( Resources.DefaultLeftColumnWidth ) );
										GUILayout.FlexibleSpace();
										GUILayout.Label( "Target" );
									GUILayout.EndHorizontal();
									GUILayout.BeginVertical();
										if( ( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Target != null )
										{
											if( GUILayout.Button( ( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Target.Network.ToString() + "." +
											 				( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Target.ToString() ) )
											// Select target
											{
												selectTarget = true;
											}
										}
										else
										{
											GUILayout.Label( "No target" );
										}
										
										targetNetwork = ( NetworkAsset )Resources.PulldownPopup( "Network", new ArrayList( Editor.Instance.Collection.Networks ), "No networks in collection" );
										targetNode = ( NetworkNodeAsset )Resources.PulldownPopup( "Node",
											( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Target == null ? new ArrayList() :
											new ArrayList( ( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Target.Network.Nodes ), "No nodes in network" );
											
										if( targetNetwork != null && targetNetwork.Nodes.Length > 0 )
										{
											( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Target = targetNetwork.Nodes[ 0 ];
									 		Editor.Instance.SaveCollection();
										}
										else if( targetNode != null )
										{
											( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Target = targetNode;
									 		Editor.Instance.SaveCollection();
										}
									GUILayout.EndVertical();
								GUILayout.EndHorizontal();
								
								// Disconnect
								GUILayout.BeginHorizontal();
									GUILayout.BeginHorizontal( GUILayout.Width( Resources.DefaultLeftColumnWidth ) );
										GUILayout.FlexibleSpace();
										GUILayout.Label( "Connections" );
									GUILayout.EndHorizontal();
									if( GUILayout.Button( "Disconnect" ) )
									{
										( ( WaypointAsset )Editor.Instance.SelectedNode[ 0 ] ).Disconnect();
										Editor.Instance.UpdateScene();
										Editor.Instance.SaveCollection();
									}
								GUILayout.EndHorizontal();
							}
							
						GUILayout.EndVertical();
					GUILayout.EndHorizontal();
					
					if( selectTarget )
					{
						Editor.Instance.SelectedNetwork = ( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Target.Network;
						Editor.Instance.SelectedNode.Add( ( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Target );
						Editor.Instance.SelectedNode.RemoveAt( 0 );
						Browser.Instance.CurrentHighlight = Browser.ItemType.Node;
						Editor.Instance.UpdateScene();
					}
				}
			}
			else if( Editor.Instance.SelectedNode.Count > 1 )
			// Multiple selection
			{
				GUILayout.BeginHorizontal();
					GUILayout.Space( 15.0f );
					if( GUILayout.Button( ( showNode ) ? Resources.Expanded : Resources.Collapsed, Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) )
					{
						showNode = !showNode;
					}
					tagsToggle[ 0 ] = Resources.Waypoint;
					nodeTags = ( GUILayout.Toolbar( nodeTags ? 1 : 0, tagsToggle, GUILayout.Height( 20.0f ) ) == 1 );
//					GUILayout.Label( Resources.Waypoint, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) );
					GUILayout.Label( "Nodes", GUILayout.MinWidth( 20.0f ) );
					GUILayout.FlexibleSpace();
//					nodeTags = GUILayout.Toggle( nodeTags, Resources.Tag, GUI.skin.GetStyle( "ToolbarButton" ) );
					if( GUILayout.Button( Resources.Help, GUI.skin.GetStyle( "Label" ) ) )
					{
						Resources.Documentation( "Node" );
					}
				GUILayout.EndHorizontal();
				
				if( showNode )
				{
					GUILayout.BeginHorizontal();
						GUILayout.Space( 15.0f );
						GUILayout.BeginVertical();
							foreach( NetworkNodeAsset node in Editor.Instance.SelectedNode )
							// List all nodes in selection
							{
								if( Editor.Instance.SelectedNode[ 0 ] is GridNodeAsset && ( ( GridNodeAsset )Editor.Instance.SelectedNode[ 0 ] ).Target ==  null )
								{
									GUI.color = Color.red;
								}
								
								GUILayout.Label( node.Name + ( ( Editor.Instance.SelectedNode[ 0 ] is WaypointAsset ) ? " (waypoint)" : (  ( Editor.Instance.SelectedNode[ 0 ] is GridNodeAsset ) ? " (grid node)" : " (navmesh cell)" ) ) );
								
								GUI.color = Color.white;
							}
							
							if( !nodeTags && Editor.Instance.SelectedNode[ 0 ] is WaypointAsset )
							// Make connect and disconnect available if multiple waypoints are selected
							{
								GUILayout.BeginHorizontal();
									if( GUILayout.Button( "Connect", GUILayout.Width( Resources.DefaultLeftColumnWidth ) ) )
									// Connect
									{										
										for( int i = 0; i < Editor.Instance.SelectedNode.Count - 1; i++ )
										{
											( Editor.Instance.SelectedNode[ i ] as WaypointAsset ).ConnectTo( Editor.Instance.SelectedNode[ i + 1 ] as NetworkNodeAsset, connectionWidthNode );
											if( bidirectionalConnect )
											{
												( Editor.Instance.SelectedNode[ i + 1 ] as WaypointAsset ).ConnectTo( Editor.Instance.SelectedNode[ i ] as NetworkNodeAsset, connectionWidthNode );
											}
										}
										
										Editor.Instance.SaveCollection();
										Editor.Instance.SelectedNode.Clear();
									}
									
									GUILayout.BeginVertical();
										connectionWidthNode = EditorGUILayout.FloatField( "Connection width", connectionWidthNode );
										bidirectionalConnect = GUILayout.Toggle( bidirectionalConnect, "Bi-directional" );
									GUILayout.EndVertical();
								GUILayout.EndHorizontal();
								
								GUILayout.BeginHorizontal();
									GUILayout.BeginHorizontal( GUILayout.Width( Resources.DefaultLeftColumnWidth ) );
										GUILayout.FlexibleSpace();
										GUILayout.Label( "Connections" );
									GUILayout.EndHorizontal();
									if( GUILayout.Button( "Disconnect" ) &&
										EditorUtility.DisplayDialog( "Remove all connections?", "Are you sure that you wish to remove all connections from the selected waypoints?\n\nWARNING: Disconnect on multi-selections removes all connections going from the selected waypoints to any other waypoint - whether it is part of the selection or not. This cannot be undone.", "Disconnect", "Cancel" ) )
									// Disconnect
									{
										foreach( WaypointAsset waypoint in Editor.Instance.SelectedNode )
										{
											waypoint.Disconnect();
										}
										Editor.Instance.SaveCollection();
									}
								GUILayout.EndHorizontal();
							}
							else if( !nodeTags && Editor.Instance.SelectedNode[ 0 ] is GridNodeAsset )
							// Make connect and disconnect available if multiple grid nodes are selected
							{
								GUILayout.BeginHorizontal();
									if( GUILayout.Button( "Connect", GUILayout.Width( Resources.DefaultLeftColumnWidth ) ) )
									// Connect
									{										
										for( int i = 0; i < Editor.Instance.SelectedNode.Count - 1; i++ )
										{
											( Editor.Instance.SelectedNode[ i ] as GridNodeAsset ).ConnectTo( Editor.Instance.SelectedNode[ i + 1 ] as GridNodeAsset, connectionWidthNode );
											if( bidirectionalConnect )
											{
												( Editor.Instance.SelectedNode[ i + 1 ] as GridNodeAsset ).ConnectTo( Editor.Instance.SelectedNode[ i ] as GridNodeAsset, connectionWidthNode );
											}
										}
										
										Editor.Instance.SaveCollection();
										Editor.Instance.SelectedNode.Clear();
									}
									
									GUILayout.BeginVertical();
										connectionWidthNode = EditorGUILayout.FloatField( "Connection width", connectionWidthNode );
										bidirectionalConnect = GUILayout.Toggle( bidirectionalConnect, "Bi-directional" );
									GUILayout.EndVertical();
								GUILayout.EndHorizontal();
								
								GUILayout.BeginHorizontal();
									GUILayout.BeginHorizontal( GUILayout.Width( Resources.DefaultLeftColumnWidth ) );
										GUILayout.FlexibleSpace();
										GUILayout.Label( "Connections" );
									GUILayout.EndHorizontal();
									if( GUILayout.Button( "Disconnect" ) &&
										EditorUtility.DisplayDialog( "Remove all connections?", "Are you sure that you wish to remove all connections from the selected grid nodes?\n\nWARNING: Disconnect on multi-selections removes all connections going from the selected grid nodes to any other grid node - whether it is part of the selection or not. This cannot be undone.", "Disconnect", "Cancel" ) )
									// Disconnect
									{
										foreach( GridNodeAsset node in Editor.Instance.SelectedNode )
										{
											node.Disconnect();
										}
										Editor.Instance.SaveCollection();
									}
								GUILayout.EndHorizontal();
							}
							else if( !nodeTags && Editor.Instance.SelectedNode[ 0 ] is CellAsset )
							// Make merge available if cell assets are selected
							{
								GUILayout.BeginHorizontal();
									GUILayout.FlexibleSpace();
									if( GUILayout.Button( "Merge" ) )
									{
										mergedCell = CellAsset.Merge( Editor.Instance.SelectedNode );
						
										( ( NavmeshAsset )Editor.Instance.SelectedNetwork ).ReplaceConnections( Editor.Instance.SelectedNode, mergedCell );
						
										foreach( CellAsset oldCell in Editor.Instance.SelectedNode )
										{
											( ( NavmeshAsset )Editor.Instance.SelectedNetwork ).Remove( oldCell );
										}
						
										( ( NavmeshAsset )Editor.Instance.SelectedNetwork ).Add( mergedCell );

										Editor.Instance.SelectedNode.Clear();
										Debug.Log( "TODO: Control should scan all cell networks for connections leading to any of the cells and these should be replaced by new connections to the new cell" );
										Editor.Instance.SaveCollection();
									}
								GUILayout.EndHorizontal();
							}
						GUILayout.EndVertical();
					GUILayout.EndHorizontal();
				}
			}
			
			if( !showNode || !nodeTags )
			{
				return;
			}
			
			// Node tags toolbar //
			
			sharedTags = Resources.SharedTags( Editor.Instance.SelectedNode );
			
			GUILayout.BeginHorizontal( /*"Toolbar"*/ );
				GUILayout.Space( 30.0f );
				//GUILayout.Label( "Tags" );
		
				//GUILayout.FlexibleSpace();
		
				addedTag = Resources.PulldownPopup( "Add tag", new ArrayList( Editor.Instance.Collection.Tags ), "No tags in collection" ) as string;
				if( addedTag != null )
				// Add the new tag
				{
					askedForConnections = false;
					addToConnections = false;
					
					foreach( NetworkNodeAsset node in Editor.Instance.SelectedNode )
					// Apply the tag to all nodes in selection
					{
						node.AddTag( addedTag );
						
						if( !askedForConnections && node.Connections.Length > 0 )
						// Ask if the tag should be applied to all connections from the nodes in the selection
						{
							askedForConnections = true;
							if( EditorUtility.DisplayDialog( "Apply to connections?", "Should the tag \"" + addedTag + "\" be applied to the connections from the selection as well?", "Apply", "Do not apply" ) )
							{
								addToConnections = true;
							}
						}
						
						if( addToConnections )
						// Apply the tag to all connections of the node
						{
							foreach( ConnectionAsset connection in node.Connections )
							{
								connection.AddTag( addedTag );
							}
						}
					}
					
					Editor.Instance.SaveCollection();
				}
				
				
				
				if( ( /*Editor.Instance.SelectedNode.Count > 1 && */System.Array.IndexOf( sharedTags, selectedNodeTag ) != -1 )  &&
					GUILayout.Button( "Delete", GUILayout.Height( 15.0f ) ) )
				{
					foreach( TaggedAsset asset in Editor.Instance.SelectedNode )
					{
						asset.RemoveTag( selectedNodeTag );
					}
					
					Editor.Instance.SaveCollection();
				}
				
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
	
			// Node tags list //

			GUILayout.BeginHorizontal();
				GUILayout.Space( 30.0f );
				GUILayout.BeginVertical();
	
					if( Editor.Instance.SelectedNode.Count == 1 )
					{
						if( ( Editor.Instance.SelectedNode[ 0 ] as NetworkNodeAsset ).Tags.Length == 0 )
						{
							GUILayout.Label( "No tags added" );
						}
						else
						{
							selectedNodeTag = ( int )Resources.SelectList( ( Editor.Instance.SelectedNode[ 0 ] as NetworkNodeAsset ).Tags, selectedNodeTag, OnTagListItemGUI );
						}
					}
					else
					{
						if( sharedTags.Length > 0 )
						{
							selectedNodeTag = ( int )Resources.SelectList( sharedTags, selectedNodeTag, OnTagListItemGUI );
						}
						else
						{
							GUILayout.Label( "No shared tags in selection" );
						}
					}
			
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		
		
		
		public void OnConnectionGUI( IInspector inspector )
		{
//			string addedTag;
			float newFloat;
			Texture2D[] tagsToggle = new Texture2D[ 2 ];
			bool selectDestination = false;
			
			tagsToggle[ 1 ] = Resources.Tag;
			
			GUILayout.Space( 30.0f );
			
			GUILayout.BeginHorizontal();
				GUILayout.Space( 15.0f );
				if( GUILayout.Button( ( showConnection ) ? Resources.Expanded : Resources.Collapsed, Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) )
				{
					showConnection = !showConnection;
				}
				
				if( GUILayout.Toggle( Editor.Instance.SelectedConnection.Enabled, "", GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) != Editor.Instance.SelectedConnection.Enabled )
				{
					Editor.Instance.SelectedConnection.Enabled = !Editor.Instance.SelectedConnection.Enabled;
					Editor.Instance.SaveCollection();
				}
				
//				GUILayout.Label( Resources.Connection, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) );
				
				tagsToggle[ 0 ] = Resources.Connection;
				connectionTags = ( GUILayout.Toolbar( connectionTags ? 1 : 0, tagsToggle, GUILayout.Height( 20.0f ) ) == 1 );
				
				GUILayout.Label( "Connection", GUILayout.MinWidth( 20.0f ) );
				GUILayout.FlexibleSpace();
				
//				connectionTags = GUILayout.Toggle( connectionTags, Resources.Tag, GUI.skin.GetStyle( "ToolbarButton" ) );
				
				if( GUILayout.Button( Resources.Help, GUI.skin.GetStyle( "Label" ) ) )
				{
					Resources.Documentation( "Connection" );
				}
			GUILayout.EndHorizontal();
			
			if( showConnection )
			{
				if( !connectionTags )
				// Connection properties
				{
					GUILayout.BeginHorizontal();
						GUILayout.Space( 15.0f );
						GUILayout.BeginVertical();
					
					
							// Connection origin
							EditorGUILayout.LabelField( "From", Editor.Instance.SelectedConnection.From.ToString() );
			
			
							// Connection destination (selectable)
							GUILayout.BeginHorizontal();
								GUILayout.BeginHorizontal( GUILayout.Width( Resources.DefaultLeftColumnWidth ) );
									GUILayout.FlexibleSpace();
									GUILayout.Label( "To" );
								GUILayout.EndHorizontal();
							
								GUILayout.BeginHorizontal();
									GUILayout.FlexibleSpace();
									if( GUILayout.Button( Editor.Instance.SelectedConnection.To.ToString() ) )
									{
										selectDestination = true;
									}
								GUILayout.EndHorizontal();
							GUILayout.EndHorizontal();
				

							// Connection width:
							newFloat = EditorGUILayout.FloatField( "Width", Editor.Instance.SelectedConnection.Width );
							if( newFloat != Editor.Instance.SelectedConnection.Width )
							{
								Editor.Instance.SelectedConnection.Width = newFloat;
								Editor.Instance.SaveCollection();
							}
							
							
							// Connection weight factor:
							newFloat = EditorGUILayout.FloatField( "Weight factor", Editor.Instance.SelectedConnection.WeightFactor );
							if( newFloat != Editor.Instance.SelectedConnection.WeightFactor )
							{
								Editor.Instance.SelectedConnection.WeightFactor = newFloat;
								Editor.Instance.SaveCollection();
							}
							
							
							// Return connections:
							GUILayout.BeginHorizontal();
								GUILayout.BeginHorizontal( GUILayout.Width( Resources.DefaultLeftColumnWidth ) );
									GUILayout.FlexibleSpace();
									GUILayout.Label( "Return connections" );
								GUILayout.EndHorizontal();
							
								GUILayout.BeginVertical();
									if( Editor.Instance.SelectedConnection.To.Connections.Length == 0 )
									{
										GUILayout.Label( "None" );
									}
									else
									{
										foreach( ConnectionAsset returnConnection in Editor.Instance.SelectedConnection.To.Connections )
										{
											if( returnConnection.To == Editor.Instance.SelectedConnection.From &&
												GUILayout.Button( returnConnection.ToString() ) )
											{
												Editor.Instance.SelectedNode.Clear();
												Editor.Instance.SelectedNode.Add( returnConnection.From );
												Editor.Instance.SelectedConnection = returnConnection;
												if( Browser.Instance != null )
												{
													Browser.Instance.SelectionUpdate = true;
												}
												Editor.Instance.UpdateScene();
											}
										}
									}
								GUILayout.EndVertical();
							GUILayout.EndHorizontal();
							
							
							if( selectDestination )
							{
								Editor.Instance.SelectedNode.Add( Editor.Instance.SelectedConnection.To );
								Editor.Instance.SelectedNode.RemoveAt( 0 );
								Editor.Instance.SelectedConnection = null;
								Browser.Instance.CurrentHighlight = Browser.ItemType.Node;
								Editor.Instance.UpdateScene();
							}
							

						GUILayout.EndVertical();
					GUILayout.EndHorizontal();
				}
				else
				// Connection tags
				{
					selectedConnectionTag = OnTagListGUI( Editor.Instance.SelectedConnection, selectedConnectionTag );
				}
			}
		}
		
		
		
		private int OnTagListGUI( TaggedAsset taggedAsset, int selectedTag )
		{
			string addedTag;
			
			GUILayout.BeginHorizontal();
				GUILayout.Space( 30.0f );
				
				addedTag = Resources.PulldownPopup( "Add tag", new ArrayList( Editor.Instance.Collection.Tags ), "No tags in collection" ) as string;
				if( addedTag != null )
				{
					taggedAsset.AddTag( addedTag );
					Editor.Instance.SaveCollection();
				}
				
				if( taggedAsset.HasTag( selectedTag ) && GUILayout.Button( "Delete", GUILayout.Height( 15.0f ) ) )
				{
					taggedAsset.RemoveTag( selectedTag );
					Editor.Instance.SaveCollection();
				}
				
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
	
			GUILayout.BeginHorizontal();
				GUILayout.Space( 30.0f );
				GUILayout.BeginVertical();
					if( taggedAsset.Tags.Length == 0 )
					{
						GUILayout.Label( "No tags added" );
					}
					else
					{
						selectedTag = ( int )Resources.SelectList( taggedAsset.Tags, selectedTag, OnTagListItemGUI );
					}
				GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			
			return selectedTag;
		}



		private bool OnTagListItemGUI( object item, bool selected, ICollection list )
		{
			string tag;

			tag = Editor.Instance.Collection.Tags[ ( int )item ];
			
			if( tag == null )
			{
				return false;
			}

			GUILayout.BeginHorizontal( selected ? Resources.SelectedListItemStyle : Resources.ListItemStyle );
				if( GUILayout.Button( Resources.Tag, selected ? Resources.SelectedListStyle : Resources.ListStyle, GUILayout.Width( Resources.DefaultListItemHeight ), GUILayout.Height( Resources.DefaultListItemHeight ) ) ||
					GUILayout.Button( tag, selected ? Resources.SelectedListStyle : Resources.ListStyle, GUILayout.Height( Resources.DefaultListItemHeight ) ) )
				{
					selected = true;
				}
			GUILayout.EndHorizontal();

			return selected;
		}
	}
}
