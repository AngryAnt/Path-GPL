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
using System.Collections;
using UnityEditor;

namespace PathLibrary
{
	public interface IEditorWindow
	{
		void SaveCollection( CollectionAsset collectionAsset, IPathAsset pathAsset );
		string GetCollectionName( IPathAsset asset );
		
		/*IWaypointPositioner CreateWaypointPositioner( string networkName, string nodeName, Vector3 position );
		IWaypointPositioner GetWaypointPositioner( string networkName, string nodeName );
		void SelectWaypointPositioner( IWaypointPositioner waypointPositioner );*/
        
		void UpdateScene();
	}
	
	
	/*
	public interface IWaypointPositioner
	{
		Vector3 Position
		{
			get;
			set;
		}
		
		string Name
		{
			get;
		}
		
		string Network
		{
			get;
		}

		string Node
		{
			get;
		}

		void Init( string networkName, string nodeName );
	}
	*/
	
	
	public class Editor
	{
        private static Editor instance;
        
        
        
        public static void Init( IEditorWindow editorWindow )
        {
			new Editor( editorWindow );
        }
        

        
        public static Editor Instance
        {
            get
            {
                return instance;
            }
        }



		private IEditorWindow editorWindow;
		
		private IPathAsset asset;
		private CollectionAsset collection;
		
		private NetworkAsset selectedNetwork;
		private ArrayList selectedNode;
		private ConnectionAsset selectedConnection;
		private TriangleAsset selectedTriangle;
		private string selectedTag;
		
		
		
		
		public Editor( IEditorWindow editorWindow )
		{
			this.editorWindow = editorWindow;
			selectedNode = new ArrayList();
			
			instance = this;
		}
		
		
		
		public void OnDestroy()
		{
			instance = null;
		}
		
		
		
        public void Repaint()
		{
			if( Browser.Instance != null )
			{
				Browser.Instance.Repaint();
			}
		}
		
		
		
		public void UpdateScene()
		{
			editorWindow.UpdateScene();
		}
		
		
		
		public NetworkAsset SelectedNetwork
		{
			get
			{
				return selectedNetwork;
			}
			set
			{
				selectedNetwork = value;
				
				Repaint();
			}
		}
		
		
		
		public ArrayList SelectedNode
		{
			get
			{
				return selectedNode;
			}
			set
			{
				bool newValue = ( selectedNode != value );
				selectedNode = value;
				
				if( newValue )
				{
					Repaint();
					HandleUtility.Repaint();
				}
			}
		}
		
		
		
		public ConnectionAsset SelectedConnection
		{
			get
			{
				return selectedConnection;
			}
			set
			{
				selectedConnection = value;
	
				Repaint();
			}
		}
		
		
		
		public TriangleAsset SelectedTriangle
		{
			get
			{
				return selectedTriangle;
			}
			set
			{
				selectedTriangle = value;
			}
		}
		
		
		
		public string SelectedTag
		{
			get
			{
				return selectedTag;
			}
			set
			{
				selectedTag = value;
			}
		}
		
		
		
		public IPathAsset Asset
		{
			get
			{
				return asset;
			}
			set
			{
				this.asset = value;
				Collection = CollectionAsset.LoadFromData( asset.Data, asset );
				
				Repaint();
			}
		}



		public CollectionAsset Collection
		{
			get
			{
				return collection;
			}
			set
			{
				collection = value;
			}
		}
		
		
		
		public void SaveCollection( CollectionAsset collectionAsset, IPathAsset pathAsset )
		{
			editorWindow.SaveCollection( collectionAsset, pathAsset );
		}
		
		
		
		public void SaveCollection()
		{
			SaveCollection( Collection, Asset );
		}
		
		
		
		public void LoadCollection( IPathAsset asset )
		{
			Asset = asset;
			Collection = CollectionAsset.LoadFromData( Asset.Data, Asset );
		}
		
		
		
		public string GetCollectionName( IPathAsset asset )
		{
			return editorWindow.GetCollectionName( asset );
		}
		
		
		
		public string CollectionName
		{
			get
			{
				return GetCollectionName( Asset );
			}
		}



		public bool ValidateCreateTag()
		{
			return Asset != null;
		}



		public bool CreateTag()
		{
			int id = 0;
			
			while( System.Array.IndexOf( Collection.Tags, "NewTag" + ( ++id ) ) != -1 );
			
			SelectedTag = Collection.AddTag( "NewTag" + id );
			
			SaveCollection();
			
			if( Browser.Instance != null )
			{
				Browser.Instance.Tags = true;
				Browser.Instance.Editing = false;
				Browser.Instance.CurrentHighlight = Browser.ItemType.Tag;
				Browser.Instance.Editing = true;
			}
			
			return true;
		}



		public bool ValidateCreateWaypoint()
		{
			return Asset != null && SelectedNetwork != null && SelectedNetwork is WaypointNetworkAsset;
		}



		public bool CreateWaypoint()
		{
			int id = 0;
			
			while( ( ( WaypointNetworkAsset )SelectedNetwork ).Get( "NewWaypoint" + ( ++id ) ) != null );
			
			SelectedNode.Clear();
			SelectedNode.Add( ( ( WaypointNetworkAsset )SelectedNetwork ).Add( new WaypointAsset( "NewWaypoint" + id, Vector3.zero, 1.0f, ( WaypointNetworkAsset )SelectedNetwork, Collection ) ) );
			
			SaveCollection();
			
			if( Browser.Instance != null )
			{
				Browser.Instance.Editing = false;
				Browser.Instance.CurrentHighlight = Browser.ItemType.Node;
				Browser.Instance.Editing = true;
			}
			
			return true;
		}
		
		
		
		public bool DuplicateWaypoint()
		{
			int id, length;

			length = SelectedNode.Count;
			for( int i = 0; i < length; i++ )
			{
				SelectedNode.Add( new WaypointAsset( ( WaypointAsset )SelectedNode[ i ], SelectedNetwork ) );
			
				id = 0;
				while( ( ( WaypointNetworkAsset )SelectedNetwork ).Get( ( ( WaypointAsset )SelectedNode[ length + i ] ).Name + "-" + ( ++id ) ) != null );
				( ( WaypointAsset )SelectedNode[ length + i ] ).Name += "-" + id;
			
				( ( WaypointNetworkAsset )SelectedNetwork ).Add( ( ( WaypointAsset )SelectedNode[ length + i ] ) );
			}
			
			SelectedNode.RemoveRange( 0, length );
			
			SaveCollection();
			
			return true;
		}
		
		
		
		public bool ValidateCreateWaypointNetwork()
		{
			return Asset != null;
		}
		


		public bool CreateWaypointNetwork()
		{
			int id = 0;
			
			while( Collection.Get( "NewWaypointNetwork" + ( ++id ) ) != null );
			
			SelectedNetwork = Collection.Add( new WaypointNetworkAsset( "NewWaypointNetwork" + id, Collection ) );
			
			SaveCollection();
			
			if( Browser.Instance != null )
			{
				Browser.Instance.Editing = false;
				Browser.Instance.CurrentHighlight = Browser.ItemType.Network;
				Browser.Instance.Editing = true;
			}
			
			return true;
		}
		
		
		
		public bool DuplicateWaypointNetwork()
		{
			SelectedNetwork = Collection.Add( new WaypointNetworkAsset( ( WaypointNetworkAsset )SelectedNetwork ) );
			SaveCollection();
			return true;
		}
		
		
		
		public bool ValidateCreateNavmesh()
		{
			return Asset != null;
		}
		


		public bool CreateNavmesh()
		{
			int id = 0;
			
			while( Collection.Get( "NewNavmesh" + ( ++id ) ) != null );
			
			SelectedNetwork = Collection.Add( new NavmeshAsset( "NewNavmesh" + id, Collection ) );
			
			SaveCollection();
			
			if( Browser.Instance != null )
			{
				Browser.Instance.Editing = false;
				Browser.Instance.CurrentHighlight = Browser.ItemType.Network;
				Browser.Instance.Editing = true;
			}
			
			return true;
		}
		
		
		
		public bool DuplicateNavmesh()
		{
			SelectedNetwork = Collection.Add( new NavmeshAsset( ( NavmeshAsset )SelectedNetwork ) );
			SaveCollection();
			return true;
		}
		
		
		
		public bool ValidateCreateGridNode()
		{
			return Asset != null && SelectedNetwork != null && SelectedNetwork is GridNetworkAsset;
		}



		public bool CreateGridNode()
		{
			int id = 0;
			
			while( ( ( GridNetworkAsset )SelectedNetwork ).Get( "NewGridNode" + ( ++id ) ) != null );
			
			SelectedNode.Clear();
			SelectedNode.Add( ( ( GridNetworkAsset )SelectedNetwork ).Add( new GridNodeAsset( "NewGridNode" + id, ( GridNetworkAsset )SelectedNetwork, Collection ) ) );
			
			SaveCollection();
			
			if( Browser.Instance != null )
			{
				Browser.Instance.Editing = false;
				Browser.Instance.CurrentHighlight = Browser.ItemType.Node;
				Browser.Instance.Editing = true;
			}
			
			return true;
		}
		
		
		
		public bool DuplicateGridNode()
		{
			int id, length;

			length = SelectedNode.Count;
			for( int i = 0; i < length; i++ )
			{
				SelectedNode.Add( new GridNodeAsset( ( GridNodeAsset )SelectedNode[ i ], SelectedNetwork ) );
			
				id = 0;
				while( ( ( GridNetworkAsset )SelectedNetwork ).Get( ( ( GridNodeAsset )SelectedNode[ length + i ] ).Name + "-" + ( ++id ) ) != null );
				( ( GridNodeAsset )SelectedNode[ length + i ] ).Name += "-" + id;
			
				( ( GridNetworkAsset )SelectedNetwork ).Add( ( ( GridNodeAsset )SelectedNode[ length + i ] ) );
			}
			
			SelectedNode.RemoveRange( 0, length );
			
			SaveCollection();
			
			return true;
		}
		
		
		
		public bool ValidateCreateGridNetwork()
		{
			return Asset != null;
		}
		


		public bool CreateGridNetwork()
		{
			int id = 0;
			
			while( Collection.Get( "NewGridNetwork" + ( ++id ) ) != null );
			
			SelectedNetwork = Collection.Add( new GridNetworkAsset( "NewGridNetwork" + id, Collection ) );
			
			SaveCollection();
			
			if( Browser.Instance != null )
			{
				Browser.Instance.Editing = false;
				Browser.Instance.CurrentHighlight = Browser.ItemType.Network;
				Browser.Instance.Editing = true;
			}
			
			return true;
		}
		
		
		
		public bool DuplicateGridNetwork()
		{
			SelectedNetwork = Collection.Add( new GridNetworkAsset( ( GridNetworkAsset )SelectedNetwork ) );
			SaveCollection();
			return true;
		}
		
		
		
		public bool ValidateAddWaypoint()
		{
			return true;
		}



		public void Update()
		{

		}
		
		
		
		public Bounds OnGetFrameBounds( Transform transform )
		// Hack the F for focus key in the scene view
		{
			Bounds bounds;

			if( SelectedNode.Count > 1 )
			{
				bounds = GetMultiSelectBounds();
				bounds.center += transform.position + PathLibrary.Editor.Instance.SelectedNetwork.Position;
				return bounds;
			}
			else if( PathLibrary.Editor.Instance.SelectedNode.Count == 1 )
			{
				return new Bounds( ( ( NetworkNodeAsset )SelectedNode[ 0 ] ).Position + SelectedNetwork.Position + transform.position, SelectedNetwork.Size / 2.0f );
			}

			if( PathLibrary.Editor.Instance.SelectedNetwork != null )
			{
				return new Bounds( SelectedNetwork.Position + transform.position, SelectedNetwork.Size /2.0f );
			}

			return new Bounds( SceneView.current.pivot, Vector3.zero );
		}



		public bool OnSceneGUI( Transform transform )
		{
			Quaternion quat;
			WaypointAsset waypoint;
			Vector3 origin, newPosition, diff;
			bool repaint;

			repaint = false;

			// Handle normal rendering //

			quat = transform.rotation;

			// Calculate center of selection on multi-select
			origin = Vector3.zero;
			if( SelectedNode.Count > 1 )
			{
				origin = GetMultiSelectBounds().center;
			}

			if( SelectedNetwork != null && Browser.Instance != null &&
				Browser.Instance.CurrentHighlight == Browser.ItemType.Network &&
				!( SelectedNetwork is GridNetworkAsset ) )
			// Network position handle if needed
			{
				newPosition = Handles.PositionHandle( SelectedNetwork.Position + transform.position, quat ) - transform.position;
				if( newPosition != SelectedNetwork.Position )
				{
					SelectedNetwork.Position = newPosition;
					SaveCollection();
					repaint = true;
				}
			}

			if( SelectedNode.Count == 1 && SelectedNode[ 0 ] is WaypointAsset )
			// Single selection node handle
			{
				waypoint = SelectedNode[ 0 ] as WaypointAsset;
				newPosition = Handles.PositionHandle( waypoint.Position + transform.position + SelectedNetwork.Position, quat ) - transform.position - SelectedNetwork.Position;
				if( waypoint.Position != newPosition )
				{
					waypoint.Position = newPosition;
					SaveCollection();
					repaint = true;
				}
			}
			else if( SelectedNode.Count > 1 && SelectedNode[ 0 ] is WaypointAsset )
			// Multi-select node handle
			{
				newPosition = Handles.PositionHandle( origin + transform.position + PathLibrary.Editor.Instance.SelectedNetwork.Position, quat ) - transform.position - SelectedNetwork.Position;
				if( origin != newPosition )
				{
					diff = newPosition - origin;

					foreach( WaypointAsset point in SelectedNode )
					{
						point.Position += diff;
					}
					SaveCollection();
					repaint = true;
				}
			}

			// Update if needed
			if( GUI.changed || repaint )
			{
				return true;
			}
			
			return false;
		}



		public Bounds GetMultiSelectBounds()
		{
			Vector3 min, max;

			min = new Vector3( Mathf.Infinity, Mathf.Infinity, Mathf.Infinity );
			max = new Vector3( -Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity );

			foreach( NetworkNodeAsset point in PathLibrary.Editor.Instance.SelectedNode )
			{
				min = new Vector3(
					( min.x < point.Position.x ) ? min.x : point.Position.x,
					( min.y < point.Position.y ) ? min.y : point.Position.y,
					( min.z < point.Position.z ) ? min.z : point.Position.z );

				max = new Vector3(
					( max.x > point.Position.x ) ? max.x : point.Position.x,
					( max.y > point.Position.y ) ? max.y : point.Position.y,
					( max.z > point.Position.z ) ? max.z : point.Position.z );
			}

			return new Bounds( new Vector3( ( max.x - min.x ) / 2.0f + min.x, ( max.y - min.y ) / 2.0f + min.y, ( max.z - min.z ) / 2.0f + min.z ),
			 					new Vector3( max.x - min.x, max.y - min.y, max.z - min.z ) );
		}

		/*
		public IWaypointPositioner CreateWaypointPositioner( string networkName, string nodeName, Vector3 position )
		{
			return editorWindow.CreateWaypointPositioner( networkName, nodeName, position );
		}
		
		
		
		public IWaypointPositioner GetWaypointPositioner( string networkName, string nodeName )
		{
			return editorWindow.GetWaypointPositioner( networkName, nodeName );
		}
		
		
		
		public void SelectWaypointPositioner( IWaypointPositioner waypointPositioner )
		{
			editorWindow.SelectWaypointPositioner( waypointPositioner );
		}
		
		
		
		public void OnUpdateWaypointPosition( IWaypointPositioner waypointPositioner )
		{
			foreach( NetworkAsset network in Collection.Networks )
			{
				if( network.Name == waypointPositioner.Network )
				{
					foreach( NetworkNodeAsset node in network.Nodes )
					{
						if( node.Name == waypointPositioner.Node )
						{
							node.Position = waypointPositioner.Position;
							SaveCollection();
							return;
						}
					}
				}
			}
		}*/
	}
}
